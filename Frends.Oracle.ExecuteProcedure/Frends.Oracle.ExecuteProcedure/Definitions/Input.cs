// <copyright file="Input.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Frends.Oracle.ExecuteProcedure.Definitions;

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

/// <summary>
/// Properties for the query to be executed.
/// </summary>
public class Input
{
    /// <summary>
    /// Gets or sets query to be executed in string format.
    /// </summary>
    /// <example>"SpGetResultsByAge".</example>
    [DisplayFormat(DataFormatString = "Sql")]
    [DefaultValue("")]
    public string Command { get; set; }

    /// <summary>
    /// Gets or sets type of the command: Command or Stored Procedure.
    /// </summary>
    /// <example>OracleCommandType.StoredProcedure.</example>
    [DefaultValue(OracleCommandType.StoredProcedure)]
    public OracleCommandType CommandType { get; set; }

    /// <summary>
    /// Gets or sets parameters for the database query.
    /// </summary>
    /// <example>{ Name = "ParamName", Value = "Example value", DataType = ProcedureParameterType.NVarchar2 }.</example>
    public InputParameter[] Parameters { get; set; }

    /// <summary>
    /// Gets or sets data type for the result of the procedure.
    /// </summary>
    /// <example>OracleCommandReturnType.XDocument.</example>
    [DisplayFormat(DataFormatString = "Text")]
    [DefaultValue(OracleCommandReturnType.XDocument)]
    public OracleCommandReturnType DataReturnType { get; set; }

    /// <summary>
    /// Gets or sets parameters for the output of the procedure or command.
    /// </summary>
    /// <example>{ Name = "address", DataType = ProcedureParameterType.Varchar2, Size = 255 }.</example>
    public OutputParameter[] OutputParameters { get; set; }

    /// <summary>
    /// Gets or sets oracle connection string.
    /// </summary>
    /// <example>"Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=MyHost)(PORT=MyPort))(CONNECT_DATA=(SERVICE_NAME=MyOracleSID)));User Id=myUsername;Password=myPassword;".</example>
    [PasswordPropertyText]
    [DisplayFormat(DataFormatString = "Text")]
    [DefaultValue("Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=MyHost)(PORT=MyPort))(CONNECT_DATA=(SERVICE_NAME=MyOracleSID)));User Id=myUsername;Password=myPassword;")]
    public string ConnectionString { get; set; }
}
