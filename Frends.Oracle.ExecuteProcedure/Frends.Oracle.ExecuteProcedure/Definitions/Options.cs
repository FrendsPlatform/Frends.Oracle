using System.ComponentModel;

namespace Frends.Oracle.ExecuteProcedure.Definitions;

/// <summary>
/// Source transfer options
/// </summary>
public class Options
{
    /// <summary>
    /// Choose if error should be thrown if Task failes.
    /// Otherwise returns Object { Success = false }
    /// </summary>
    /// <example>true</example>
    [DefaultValue(true)]
    public bool ThrowErrorOnFailure { get; set; }

    /// <summary>
    /// Timeout value in seconds.
    /// </summary>
    /// <example>30</example>
    [DefaultValue(30)]
    public int TimeoutSeconds { get; set; }

    /// <summary>
    /// Choose to bind the parameter by name.
    /// If set to false parameter order is crucial.
    /// </summary>
    /// <example>true</example>
    [DefaultValue(true)]
    public bool BindParameterByName { get; set; }

    /// <summary>
    /// Choose to clear connection pools after execution.
    /// This is an internal Oracle setting to clear cached connections.
    /// </summary>
    /// <example>true</example>
    [DefaultValue(true)]
    public bool ClearConnectionPools { get; set; } = true;

    /// <summary>
    /// Choose to close the created connection after execution.
    /// Setting this to false will allow the connection to be reused
    /// but can cause issues with left open connections or concurrent usage if not handled properly.
    /// </summary>
    /// <example>true</example>
    [DefaultValue(true)]
    public bool CloseConnection { get; set; } = true;

    /// <summary>
    /// Number of retry attempts after initial failure when encountering stale Oracle connections.
    /// Value of 0 means no retry (fail immediately on first error).
    /// Value of 1 means one retry after initial failure (2 total attempts).
    /// Used to automatically recover from connection errors after Oracle server restart.
    /// Valid range: 0-4.
    /// </summary>
    /// <example>1</example>
    [DefaultValue(1)]
    public int ConnectionRetryAttempts { get; set; } = 1;

    /// <summary>
    /// Delay in milliseconds between connection retry attempts.
    /// Used when a stale connection is detected after Oracle server restart.
    /// </summary>
    /// <example>100</example>
    [DefaultValue(100)]
    public int ConnectionRetryDelayMs { get; set; } = 100;
}
