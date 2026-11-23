namespace Frends.Oracle.ExecuteProcedure.Definitions;

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

/// <summary>
/// Properties for the query to be executed.
/// </summary>
public class Input
{
    /// <summary>
    /// Query to be executed in string format.
    /// </summary>
    /// <example>"SpGetResultsByAge".</example>
    [DisplayFormat(DataFormatString = "Sql")]
    [DefaultValue("")]
    public string Command { get; set; }

    /// <summary>
    /// Type of the command: Command or Stored Procedure.
    /// </summary>
    /// <example>OracleCommandType.StoredProcedure.</example>
    [DefaultValue(OracleCommandType.StoredProcedure)]
    public OracleCommandType CommandType { get; set; }

    /// <summary>
    /// Parameters for the database query.
    /// </summary>
    /// <example>{ Name = "ParamName", Value = "Example value", DataType = ProcedureParameterType.NVarchar2 }.</example>
    public InputParameter[] Parameters { get; set; }

    /// <summary>
    /// Data type for the result of the procedure.
    /// </summary>
    /// <example>OracleCommandReturnType.XDocument.</example>
    [DisplayFormat(DataFormatString = "Text")]
    [DefaultValue(OracleCommandReturnType.XDocument)]
    public OracleCommandReturnType DataReturnType { get; set; }

    /// <summary>
    /// Parameters for the output of the procedure or command.
    /// </summary>
    /// <example>{ Name = "address", DataType = ProcedureParameterType.Varchar2, Size = 255 }.</example>
    public OutputParameter[] OutputParameters { get; set; }
}
