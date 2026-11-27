namespace Frends.Oracle.ExecuteProcedure.Definitions;

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

/// <summary>
/// Connection parameters.
/// </summary>
public class Connection
{
    /// <summary>
    /// Connection string to the Oracle database.
    /// </summary>
    /// <example>Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=MyHost)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=MyOracleSID)));User Id=myUsername;Password=myPassword;</example>
    [PasswordPropertyText]
    [DisplayFormat(DataFormatString = "Text")]
    [DefaultValue("")]
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// Number of retry attempts after an initial failure caused by a stale Oracle connection.
    /// Value of 0 means no retry (fail immediately on first error).
    /// Value of 1 means one retry after initial failure (2 total attempts).
    /// Used to automatically recover from connection errors after Oracle server restart.
    /// Valid range: 0-4.
    /// </summary>
    /// <example>1</example>
    [DefaultValue(1)]
    public int ConnectionRetryAttempts { get; set; } = 1;

    /// <summary>
    /// Delay in milliseconds between retries after a stale connection is detected.
    /// </summary>
    /// <example>100</example>
    [DefaultValue(100)]
    public int ConnectionRetryDelayMs { get; set; } = 100;

    /// <summary>
    /// Whether to clear Oracle connection pools after execution.
    /// </summary>
    /// <example>true</example>
    [DefaultValue(true)]
    public bool ClearConnectionPools { get; set; } = true;

    /// <summary>
    /// Whether to close the connection after execution.
    /// </summary>
    /// <example>true</example>
    [DefaultValue(true)]
    public bool CloseConnection { get; set; } = true;
}
