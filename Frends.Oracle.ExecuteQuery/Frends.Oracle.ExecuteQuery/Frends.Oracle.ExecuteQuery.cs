using Oracle.ManagedDataAccess.Client;
using System.Data;
using System.ComponentModel;
using System.Globalization;
using Frends.Oracle.ExecuteQuery.Definitions;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace Frends.Oracle.ExecuteQuery;

/// <summary>
/// Task class
/// </summary>
public static class Oracle
{
    /// <summary>
    /// Task for performing queries in an Oracle database.
    /// [Documentation](https://tasks.frends.com/tasks/frends-tasks/Frends.Oracle.ExecuteQuery)
    /// </summary>
    /// <param name="input">Properties for the query to be executed</param>
    /// <param name="options">Task options</param>
    /// <param name="cancellationToken">CancellationToken is given by Frends UI</param>
    /// <returns>Object { bool Success, string Message, JToken.JObject[] Output }</returns>
    public static async Task<Result> ExecuteQuery([PropertyTab] Input input, [PropertyTab] Options options,
        CancellationToken cancellationToken)
    {
        try
        {
            await using OracleConnection con = new OracleConnection(input.ConnectionString);
            await con.OpenAsync(cancellationToken);
            await using var transaction = con.BeginTransaction(GetIsolationLevel(options.OracleIsolationLevel));
            await using var command = con.CreateCommand();

            command.Transaction = transaction;
            command.CommandTimeout = options.TimeoutSeconds;
            command.CommandText = input.Query;
            command.BindByName = options.BindParameterByName;

            if (input.Parameters != null)
                command.Parameters.AddRange(input.Parameters.Select(CreateOracleParameter).ToArray());

            try
            {
                Result result;
                int rows;

                switch (input.ExecuteType)
                {
                    case ExecuteTypes.Auto:
                        // Execute query
                        if (input.Query.ToLower().StartsWith("select"))
                        {
                            var jToken = await ToJTokenAsync(command, options, cancellationToken);
                            result = new Result(true, "Success", jToken);
                        }
                        else
                        {
                            rows = await command.ExecuteNonQueryAsync(cancellationToken);
                            await transaction.CommitAsync(cancellationToken);
                            result = new Result(true, "Success", JToken.FromObject(new { AffectedRows = rows }));
                        }

                        break;
                    case ExecuteTypes.NonQuery:
                        rows = await command.ExecuteNonQueryAsync(cancellationToken);
                        await transaction.CommitAsync(cancellationToken);
                        result = new Result(true, "Success", JToken.FromObject(new
                        {
                            AffectedRows = rows
                        }));

                        break;
                    case ExecuteTypes.ExecuteReader:
                        {
                            var jToken = await ToJTokenAsync(command, options, cancellationToken);
                            result = new Result(true, "Success", jToken);

                            break;
                        }
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

    private static async Task<JToken> ToJTokenAsync(OracleCommand command, Options options,
        CancellationToken cancellationToken)
    {
        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        reader.SuppressGetDecimalInvalidCastException = options.EnableSafeNumericMapping;

        await using var writer = new JTokenWriter();
        writer.Formatting = Formatting.Indented;
        writer.Culture = CultureInfo.InvariantCulture;

        await writer.WriteStartArrayAsync(cancellationToken).ConfigureAwait(false);

        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            cancellationToken.ThrowIfCancellationRequested();
            await writer.WriteStartObjectAsync(cancellationToken).ConfigureAwait(false);

            for (var i = 0; i < reader.FieldCount; i++)
            {
                await writer.WritePropertyNameAsync(reader.GetName(i), cancellationToken).ConfigureAwait(false);

                try
                {
                    switch (reader.GetDataTypeName(i))
                    {
                        case "Date":
                        case "TimeStamp":
                        case "TimeStampLTZ":
                        case "TimeStampTZ":
                            string dateString = reader.GetValue(i).ToString();
                            await writer.WriteValueAsync(dateString, cancellationToken).ConfigureAwait(false);

                            break;
                        default:
                            await writer.WriteValueAsync(reader.GetValue(i), cancellationToken).ConfigureAwait(false);

                            break;
                    }
                }
                catch (Exception e)
                {
                    var invalidValue = reader.GetString(i);

                    throw new InvalidCastException($"Value: {invalidValue}, couldn't be properly formatted", e);
                }
            }

            await writer.WriteEndObjectAsync(cancellationToken).ConfigureAwait(false);
        }

        await writer.WriteEndArrayAsync(cancellationToken).ConfigureAwait(false);

        return writer.Token;
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

    private static IsolationLevel GetIsolationLevel(TransactionIsolationLevel level)
    {
        return level switch
        {
            TransactionIsolationLevel.None => IsolationLevel.Unspecified,
            TransactionIsolationLevel.ReadCommitted => IsolationLevel.ReadCommitted,
            TransactionIsolationLevel.RepeatableRead => IsolationLevel.RepeatableRead,
            TransactionIsolationLevel.Serializable => IsolationLevel.Serializable,
            TransactionIsolationLevel.ReadUncommitted => IsolationLevel.ReadUncommitted,
            _ => IsolationLevel.Serializable,
        };
    }
}
