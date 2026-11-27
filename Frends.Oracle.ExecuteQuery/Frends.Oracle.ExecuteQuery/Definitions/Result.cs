namespace Frends.Oracle.ExecuteQuery.Definitions;

/// <summary>
/// Return object with private setters.
/// </summary>
public class Result
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Result"/> class.
    /// </summary>
    /// <param name="success">Whether the query succeeded.</param>
    /// <param name="message">The query result message.</param>
    /// <param name="output">The query output.</param>
    /// <param name="error">The error details, if the query failed.</param>
    internal Result(bool success, string message, object output, Error error = null)
    {
        Success = success;
        Message = message;
        Output = output;
        Error = error;
    }

    /// <summary>
    /// Boolean value of success of the query.
    /// </summary>
    /// <example>true</example>
    public bool Success { get; private set; }

    /// <summary>
    /// String value of the Error message that comes from Oracle.
    /// </summary>
    /// <example>"ORA-01722: invalid number"</example>
    public string Message { get; private set; }

    /// <summary>
    /// List of JObjects showing the output of the query.
    /// </summary>
    /// <example>[{"ID": "1","FIRST_NAME": "Saija","LAST_NAME": "Saijalainen","START_DATE": ""}]</example>
    public dynamic Output { get; private set; }

    /// <summary>
    /// Error that occurred during the task.
    /// </summary>
    /// <example>object { string Message, Exception AdditionalInfo }</example>
    public Error Error { get; private set; }
}
