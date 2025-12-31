using Oracle.ManagedDataAccess.Client;
using OracleParam = Oracle.ManagedDataAccess.Client.OracleParameter;
using System.ComponentModel;
using Frends.Oracle.ExecuteProcedure.Definitions;
using System.Data;
using System.Xml.Linq;
using Newtonsoft.Json;

namespace Frends.Oracle.ExecuteProcedure;

/// <summary>
/// Task class
/// </summary>
public class Oracle
{
    /// <summary>
    /// Task for performing stored procedures in Oracle database.
    /// [Documentation](https://tasks.frends.com/tasks/frends-tasks/Frends.Oracle.ExecuteProcedure)
    /// </summary>
    /// <param name="input">Properties for the procedure to be executed</param>
    /// <param name="output">Properties for the output of the procedure.</param>
    /// <param name="options">Task options</param>
    /// <param name="cancellationToken">CancellationToken is given by Frends UI</param>
    /// <returns>Object { bool Success, int RowsAffected, IEnumerable Output }</returns>
    public async static Task<Result> ExecuteProcedure([PropertyTab] Input input, [PropertyTab] Output output, [PropertyTab] Options options, CancellationToken cancellationToken)
    {
        IEnumerable<OracleParam> outputOracleParams = null;
        int rowsAffected = 0;
        using OracleConnection con = new OracleConnection(input.ConnectionString);

        try
        {
            await con.OpenAsync(cancellationToken);

            using var command = new OracleCommand();
            command.Connection = con;
            command.CommandText = input.Command;
            command.CommandTimeout = options.TimeoutSeconds;
            command.CommandType = (input.CommandType == OracleCommandType.Command) ? CommandType.Text : CommandType.StoredProcedure;

            // Add input parameters to the OracleCommand
            if (input.Parameters != null)
                command.Parameters.AddRange(input.Parameters.Select(p => CreateOracleInputParameter(p)).ToArray());

            if (output.OutputParameters != null)
                command.Parameters.AddRange(output.OutputParameters.Select(x => CreateOracleOutputParameter(x)).ToArray());

            command.BindByName = options.BindParameterByName;

            var runCommand = command.ExecuteNonQueryAsync(cancellationToken);

            if (runCommand.IsFaulted)
            {
                if (options.ThrowErrorOnFailure)
                    throw new Exception(runCommand.Exception.Message);

                return new Result(runCommand.Exception.Message);
            }

            rowsAffected = await runCommand;

            outputOracleParams = command.Parameters.Cast<OracleParam>()
                .Where(p => p.Direction == ParameterDirection.Output);

            var outputDict = outputOracleParams
                .ToDictionary(
                    p => p.ParameterName,
                    p => GetOracleParameterValue(p)
                );

            command.Dispose();

            await con.CloseAsync();
            con.Dispose();

            if (output.DataReturnType == OracleCommandReturnType.AffectedRows)
                return new Result(true, rowsAffected);
            else if (output.DataReturnType == OracleCommandReturnType.Parameters)
            {
                return new Result(true, outputDict);
            }

            var result = HandleDataset(outputOracleParams, output);
            return new Result(true, result);

        }
        catch (Exception ex)
        {
            if (options.ThrowErrorOnFailure)
                throw new ArgumentException("Error when executing command:", ex.Message);

            return new Result(false, ex.Message);
        }
        finally
        {
            await con.CloseAsync();
            OracleConnection.ClearAllPools();
        }
    }

    private static OracleParam CreateOracleInputParameter(InputParameter parameter)
    {
        return new OracleParam()
        {
            ParameterName = parameter.Name,
            Value = parameter.Value,
            OracleDbType = (OracleDbType)Enum.Parse(typeof(OracleDbType), parameter.DataType.ToString()),
            Direction = ParameterDirection.Input,
            Size = parameter.Size
        };
    }

    private static OracleParam CreateOracleOutputParameter(OutputParameter parameter)
    {
        return new OracleParam()
        {
            ParameterName = parameter.Name,
            OracleDbType = (OracleDbType)Enum.Parse(typeof(OracleDbType), parameter.DataType.ToString()),
            Direction = ParameterDirection.Output,
            Size = parameter.Size
        };
    }

    private static dynamic HandleDataset(IEnumerable<OracleParam> outputOracleParams, Output output)
    {
        //Builds xml document from Oracle output parameters
        var xDoc = new XDocument();
        var root = new XElement("Root");
        xDoc.Add(root);
        outputOracleParams.ToList().ForEach(p => root.Add(ParameterToXElement(p)));

        dynamic commandResult;
        // Affected rows are handled above!
        switch (output.DataReturnType)
        {
            case OracleCommandReturnType.JSONString:
                commandResult = JsonConvert.SerializeXNode(xDoc, Formatting.None, true);
                break;
            case OracleCommandReturnType.XDocument:
                commandResult = xDoc;
                break;
            case OracleCommandReturnType.XmlString:
                commandResult = xDoc.ToString();
                break;
            default:
                throw new Exception("Unsupported DataReturnType.");
        }
        return commandResult;
    }

    private static XElement ParameterToXElement(OracleParam parameter)
    {
        var xelem = new XElement(parameter.ParameterName);
        var value = GetOracleParameterValue(parameter);
        if (value == null)
            return xelem;
        xelem.Value = value.ToString();

        return xelem;
    }

    private static object GetOracleParameterValue(OracleParam p)
    {
        if (p.Value == null || p.Value == DBNull.Value)
            return null;

        return p.Value switch
        {
            global::Oracle.ManagedDataAccess.Types.OracleString v => v.IsNull ? null : v.Value,
            global::Oracle.ManagedDataAccess.Types.OracleDecimal v => v.IsNull ? null : v.Value,
            global::Oracle.ManagedDataAccess.Types.OracleDate v => v.IsNull ? null : v.Value,
            global::Oracle.ManagedDataAccess.Types.OracleTimeStamp v => v.IsNull ? null : v.Value,
            global::Oracle.ManagedDataAccess.Types.OracleTimeStampTZ v => v.IsNull ? null : v.Value,
            global::Oracle.ManagedDataAccess.Types.OracleTimeStampLTZ v => v.IsNull ? null : v.Value,
            global::Oracle.ManagedDataAccess.Types.OracleClob v => v.IsNull ? null : v.Value,
            global::Oracle.ManagedDataAccess.Types.OracleBlob blob => BlobToBase64(blob),
            _ => p.Value
        };
    }
    private static string BlobToBase64(global::Oracle.ManagedDataAccess.Types.OracleBlob blob)
    {
        if (blob == null || blob.IsNull || blob.Length == 0)
            return null;

        const int chunkSize = 81920;
        long remaining = blob.Length;

        using var ms = new MemoryStream((int)blob.Length);
        byte[] buffer = new byte[chunkSize];

        while (remaining > 0)
        {
            int bytesToRead = (int)Math.Min(chunkSize, remaining);
            int bytesRead = blob.Read(buffer, 0, bytesToRead);

            if (bytesRead <= 0)
                throw new EndOfStreamException("Unexpected end of Oracle BLOB stream.");

            ms.Write(buffer, 0, bytesRead);
            remaining -= bytesRead;
        }

        return Convert.ToBase64String(ms.ToArray());
    }

}
