namespace Frends.Oracle.ExecuteQuery.Definitions;

/// <summary>
/// Error that occurred during the task.
/// </summary>
public class Error
{
    /// <summary>
    /// Summary of the error.
    /// </summary>
    /// <example>Oracle query execution failed</example>
    public string Message { get; set; }

    /// <summary>
    /// Additional information about the error.
    /// </summary>
    /// <example>InvalidOperationException: ORA-01722: invalid number</example>
    public Exception AdditionalInfo { get; set; }
}
