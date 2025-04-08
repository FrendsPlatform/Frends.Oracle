namespace Frends.Oracle.ExecuteProcedure.Definitions;

/// <summary>
/// Return object with private setters.
/// </summary>
public class Result
{
    /// <summary>
    /// Boolean value of success of the query.
    /// </summary>
    /// <example>true</example>
    public bool Success { get; private set; }

    /// <summary>
    /// Content and data type depends on task output properties.
    /// </summary>
    /// <example>[{"ID": "1","FIRST_NAME": "Saija","LAST_NAME": "Saijalainen","START_DATE": ""}]</example>
    public dynamic Output { get; private set; }

    /// <summary>
    /// String value of the Error message that comes from Oracle.
    /// </summary>
    /// <example>"ORA-01722: invalid number"</example>
    public string Error { get; private set; }

    internal Result(bool success, dynamic output)
    {
        Success = success;
        Output = output;
    }

    internal Result(string error)
    {
        Success = false;
        Error = error;
    }
}

