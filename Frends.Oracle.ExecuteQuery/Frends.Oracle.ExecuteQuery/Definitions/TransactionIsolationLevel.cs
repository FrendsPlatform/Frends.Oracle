namespace Frends.Oracle.ExecuteQuery.Definitions;

/// <summary>
/// Enumeration for Oracle transaction isolation levels.
/// </summary>
public enum TransactionIsolationLevel
{
    /// <summary>Use the default isolation level.</summary>
    Default,

    /// <summary>Use no transaction isolation.</summary>
    None,

    /// <summary>Allow reading uncommitted changes.</summary>
    ReadUncommitted,

    /// <summary>Allow reading only committed changes.</summary>
    ReadCommitted,

    /// <summary>Repeatable reads within the transaction.</summary>
    RepeatableRead,

    /// <summary>Serializable transaction isolation.</summary>
    Serializable,
}
