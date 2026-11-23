namespace Frends.Oracle.ExecuteProcedure.Definitions;

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

/// <summary>
/// Options for executing an Oracle command or stored procedure.
/// </summary>
public class Options
{
    /// <summary>
    /// Whether an exception is thrown when the task fails. When false, the failure is reported in the result.
    /// </summary>
    /// <example>true.</example>
    [DefaultValue(true)]
    public bool ThrowErrorOnFailure { get; set; } = true;

    /// <summary>
    /// Command timeout in seconds.
    /// </summary>
    /// <example>30.</example>
    [DefaultValue(30)]
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Whether Oracle binds parameters by name. When false, parameter order is significant.
    /// </summary>
    /// <example>true.</example>
    [DefaultValue(true)]
    public bool BindParameterByName { get; set; } = true;

    /// <summary>
    /// Whether to clear Oracle connection pools after execution.
    /// </summary>
    /// <example>true.</example>
    [DefaultValue(true)]
    public bool ClearConnectionPools { get; set; } = true;

    /// <summary>
    /// Whether to close the connection after execution.
    /// </summary>
    /// <example>true.</example>
    [DefaultValue(true)]
    public bool CloseConnection { get; set; } = true;

    /// <summary>
    /// Number of retry attempts after an initial failure caused by a stale Oracle connection.
    /// Value of 0 means no retry (fail immediately on first error).
    /// Value of 1 means one retry after initial failure (2 total attempts).
    /// Used to automatically recover from connection errors after Oracle server restart.
    /// Valid range: 0-4.
    /// </summary>
    /// <example>1.</example>
    [DefaultValue(1)]
    public int ConnectionRetryAttempts { get; set; } = 1;

    /// <summary>
    /// Delay in milliseconds between retries after a stale connection is detected.
    /// </summary>
    /// <example>100.</example>
    [DefaultValue(100)]
    public int ConnectionRetryDelayMs { get; set; } = 100;

    /// <summary>
    /// Custom context to prepend to the error message when the task fails.
    /// </summary>
    /// <example>Oracle procedure execution failed.</example>
    [DisplayFormat(DataFormatString = "Text")]
    [DefaultValue("")]
    public string ErrorMessageOnFailure { get; set; } = string.Empty;
}
