using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Frends.Oracle.ExecuteProcedure.Definitions;

/// <summary>
/// Properties for the query to be executed.
/// </summary>
public class Input
{
    /// <summary>
    /// Query to be executed in string format.
    /// </summary>
    /// <example>"SpGetResultsByAge"</example>
    [DisplayFormat(DataFormatString = "Sql")]
    [DefaultValue("")]
    public string Command { get; set; }

    /// <summary>
    /// Type of the command: Command or Stored Procedure.
    /// </summary>
    /// <example>Type.StoredProcedure</example>
    [DefaultValue(OracleCommandType.StoredProcedure)]
    public OracleCommandType CommandType { get; set; }

    /// <summary>
    /// Parameters for the database query.
    /// </summary>
    /// <example>{ Name = "ParamName", Value = "1", DataType = QueryParameterType.NVarchar2 }</example>
    public InputParameter[] Parameters { get; set; }

    /// <summary>
    /// Oracle connection string.
    /// </summary>
    /// <example>"Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=MyHost)(PORT=MyPort))(CONNECT_DATA=(SERVICE_NAME=MyOracleSID)));User Id=myUsername;Password=myPassword;"</example>
    [PasswordPropertyText]
    [DisplayFormat(DataFormatString = "Text")]
    [DefaultValue("Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=MyHost)(PORT=MyPort))(CONNECT_DATA=(SERVICE_NAME=MyOracleSID)));User Id=myUsername;Password=myPassword;")]
    public string ConnectionString { get; set; }
}