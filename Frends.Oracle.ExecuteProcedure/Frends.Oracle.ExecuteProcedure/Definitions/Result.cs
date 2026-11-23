// <copyright file="Result.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Frends.Oracle.ExecuteProcedure.Definitions;

/// <summary>
/// Return object with private setters.
/// </summary>
public class Result
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Result"/> class.
    /// </summary>
    /// <param name="success">Whether the procedure succeeded.</param>
    /// <param name="output">The procedure output.</param>
    /// <param name="error">The error message, if the procedure failed.</param>
    internal Result(bool success, dynamic output, string error = null)
    {
        this.Success = success;
        this.Output = output;
        this.Error = error;
    }

    /// <summary>
    /// Gets a value indicating whether boolean value of success of the query.
    /// </summary>
    /// <example>true.</example>
    public bool Success { get; private set; }

    /// <summary>
    /// Gets content and data type depends on task output properties.
    /// </summary>
    /// <example>[{"ID": "1","FIRST_NAME": "Saija","LAST_NAME": "Saijalainen","START_DATE": ""}].</example>
    public dynamic Output { get; private set; }

    /// <summary>
    /// Gets string value of the Error message that comes from Oracle.
    /// </summary>
    /// <example>"ORA-01722: invalid number".</example>
    public string Error { get; private set; }
}
