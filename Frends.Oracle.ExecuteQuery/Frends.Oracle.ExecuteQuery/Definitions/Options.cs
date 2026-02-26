using System.ComponentModel;

namespace Frends.Oracle.ExecuteQuery.Definitions;

/// <summary>
/// Source transfer options
/// </summary>
public class Options
{
    /// <summary>
    /// Choose if an error should be thrown if the Task fails.
    /// Otherwise, returns Object { Success = false }
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
    /// Transaction isolation level for the query.
    /// Options:
    ///     - Default
    ///     - ReadCommited
    ///     - None
    ///     - Serializable
    ///     - ReadUncommited
    ///     - RepeatableRead
    /// Additional information can be found from [here](https://docs.oracle.com/cd/E25054_01/server.1111/e25789/consist.htm)
    /// </summary>
    /// <example>Default</example>
    [DefaultValue(TransactionIsolationLevel.Default)]
    public TransactionIsolationLevel OracleIsolationLevel { get; set; }

    /// <summary>
    /// Choose to bind the parameter by name.
    /// If set to false parameter order is crucial.
    /// </summary>
    /// <example>true</example>
    [DefaultValue(true)]
    public bool BindParameterByName { get; set; }

    /// <summary>
    /// When true, attempts to safely map high-precision Oracle numbers to .NET types
    /// by rounding the scale.  (Oracle can store up to 38 digits, while .NET only 29)
    /// Note: Whole numbers exceeding 29 digits may still fail
    /// </summary>
    /// <example>false</example>
    [DefaultValue(false)]
    public bool EnableSafeNumericMapping { get; set; }
}
