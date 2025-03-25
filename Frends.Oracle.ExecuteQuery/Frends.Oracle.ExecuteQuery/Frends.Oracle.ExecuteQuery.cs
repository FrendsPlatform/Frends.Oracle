using Oracle.ManagedDataAccess.Client;
using System.Data;
using System.Data.Common;
using System.ComponentModel;
using System.Globalization;
using Frends.Oracle.ExecuteQuery.Definitions;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Oracle.ManagedDataAccess.Types;

namespace Frends.Oracle.ExecuteQuery;

/// <summary>
/// Task class
/// </summary>
public static class Oracle
{
    /// <summary>
    /// Task for performing queries in Oracle database.
    /// [Documentation](https://tasks.frends.com/tasks/frends-tasks/Frends.Oracle.ExecuteQuery)
    /// </summary>
    /// <param name="input">Properties for the query to be executed</param>
    /// <param name="options">Task options</param>
    /// <param name="cancellationToken">CancellationToken is given by Frends UI</param>
    /// <returns>Object { bool Success, string Message, JToken.JObject[] Output }</returns>
    public static async Task<Result> ExecuteQuery([PropertyTab] Input input, [PropertyTab] Options options, CancellationToken cancellationToken)
    {
        try
        {
            using OracleConnection con = new OracleConnection(input.ConnectionString);
            await con.OpenAsync(cancellationToken);
            using var transaction = con.BeginTransaction(GetIsolationLevel(options.OracleIsolationLevel));
            using var command = con.CreateCommand();

            command.Transaction = transaction;
            command.CommandTimeout = options.TimeoutSeconds;
            command.CommandText = input.Query;
            command.BindByName = options.BindParameterByName;

            if (input.Parameters != null)
                command.Parameters.AddRange(input.Parameters.Select(p => CreateOracleParameter(p)).ToArray());
            try
            {
                Result result;
                JToken dataObject;
                DbDataReader dataReader;
                int rows;

                switch (input.ExecuteType)
                {
                    case ExecuteTypes.Auto:
                        // Execute query
                        if (input.Query.ToLower().StartsWith("select"))
                        {
                            dataReader = await command.ExecuteReaderAsync(cancellationToken);
                            dataObject = dataReader.ToJson(cancellationToken);
                            result = new Result(true, "Success", dataObject);
                            break;
                        }
                        else
                        {
                            rows = await command.ExecuteNonQueryAsync(cancellationToken);
                            await transaction.CommitAsync(cancellationToken);
                            result = new Result(true, "Success", JToken.FromObject(new { AffectedRows = rows }));
                            break;
                        }
                    case ExecuteTypes.NonQuery:
                        rows = await command.ExecuteNonQueryAsync(cancellationToken);
                        await transaction.CommitAsync(cancellationToken);
                        result = new Result(true, "Success", JToken.FromObject(new { AffectedRows = rows }));
                        break;
                    case ExecuteTypes.ExecuteReader:
                        var jToken = await ToJTokenAsync(command, cancellationToken);
                        result = new Result(true, "Success", jToken);
                        break;
                    case ExecuteTypes.Scalar:
                        var scalarResult = await command.ExecuteScalarAsync(cancellationToken);
                        result = new Result(true, "Success", scalarResult ?? string.Empty);
                        break;
                    default:
                        throw new NotSupportedException();
                }
                return result;
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                if (options.ThrowErrorOnFailure)
                    throw;

                return new Result(false, ex.Message, null);
            }
            finally
            {
                await con.CloseAsync();
                con.Dispose();
            }
        }
        catch (Exception ex)
        {
            if (options.ThrowErrorOnFailure)
                throw new Exception(ex.Message);

            return new Result(false, ex.Message, null);
        }
        finally
        {
            OracleConnection.ClearAllPools();
        }
    }

    private static async Task<JToken> ToJTokenAsync(OracleCommand command, CancellationToken cancellationToken)
    {
        await using var reader = await command.ExecuteReaderAsync(cancellationToken) as OracleDataReader;
        var culture = CultureInfo.InvariantCulture;

        // Create json result.
        using JsonWriter writer = new JTokenWriter();
        writer.Formatting = Formatting.Indented;
        writer.Culture = culture;

        // Start array.
        await writer.WriteStartArrayAsync(cancellationToken);

        while (reader.Read())
        {
            // Start row object.
            await writer.WriteStartObjectAsync(cancellationToken);

            for (var i = 0; i < reader.FieldCount; i++)
            {
                // Add row element name.
                await writer.WritePropertyNameAsync(reader.GetName(i), cancellationToken);

                // Add row element value.
                switch (reader.GetDataTypeName(i))
                {
                    case "Decimal":
                        // FCOM-204 fix; proper handling of decimal values and NULL values in decimal type fields.
                        var v = reader.GetOracleDecimal(i);
                        var fieldValue = OracleDecimal.SetPrecision(v, 28);

                        if (!fieldValue.IsNull) await writer.WriteValueAsync((decimal)fieldValue, cancellationToken);
                        else await writer.WriteValueAsync(string.Empty, cancellationToken);
                        break;
                    case "Date":
                    case "TimeStamp":
                    case "TimeStampLTZ":
                    case "TimeStampTZ":
                        string dateString = reader.GetValue(i).ToString();
                        await writer.WriteValueAsync(dateString, cancellationToken);
                        break;
                    default:
                        await writer.WriteValueAsync(reader.GetValue(i) ?? string.Empty, cancellationToken);
                        break;
                }
            }
            cancellationToken.ThrowIfCancellationRequested();
            // End row object.
            await writer.WriteEndObjectAsync(cancellationToken);
        }

        // End array.
        await writer.WriteEndArrayAsync(cancellationToken);

        return ((JTokenWriter)writer).Token;
    }

    private static OracleParameter CreateOracleParameter(QueryParameter parameter)
    {
        return new OracleParameter()
        {
            ParameterName = parameter.Name,
            Value = parameter.Value,
            OracleDbType = (OracleDbType)Enum.Parse(typeof(OracleDbType), parameter.DataType.ToString())
        };
    }

    private static JToken ToJson(this DbDataReader reader, CancellationToken cancellationToken)
    {
        using var writer = new JTokenWriter();
        writer.Formatting = Formatting.Indented;
        writer.Culture = CultureInfo.InvariantCulture;

        writer.WriteStartArray();

        while (reader.Read())
        {
            cancellationToken.ThrowIfCancellationRequested();
            writer.WriteStartObject();
            for (var i = 0; i < reader.FieldCount; i++)
            {
                writer.WritePropertyName(reader.GetName(i));

                writer.WriteValue(reader.GetValue(i) ?? string.Empty);
            }
            writer.WriteEndObject();
        }

        writer.WriteEndArray();
        return writer.Token;
    }

    private static IsolationLevel GetIsolationLevel(TransactionIsolationLevel level)
    {
        return level switch
        {
            TransactionIsolationLevel.None => IsolationLevel.Unspecified,
            TransactionIsolationLevel.ReadCommitted => IsolationLevel.ReadCommitted,
            TransactionIsolationLevel.RepeatableRead => IsolationLevel.RepeatableRead,
            TransactionIsolationLevel.Serializable => IsolationLevel.Serializable,
            TransactionIsolationLevel.ReadUncommitted => IsolationLevel.ReadUncommitted,
            TransactionIsolationLevel.Default => IsolationLevel.Serializable,
            _ => IsolationLevel.Serializable,
        };
    }
}
