namespace Frends.Oracle.ExecuteQuery.Definitions;
#pragma warning disable 1591 // Self-explanatory. Information can be found from Oracle documentation.

/// <summary>
/// Enumeration for Oracle TransactionIsolationLevels 
/// </summary>
public enum TransactionIsolationLevel
{
    Default,
    None,
    ReadUncommitted,
    ReadCommitted,
    RepeatableRead,
    Serializable
}

