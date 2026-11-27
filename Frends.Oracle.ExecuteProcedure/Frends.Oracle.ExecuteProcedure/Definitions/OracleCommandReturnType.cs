namespace Frends.Oracle.ExecuteProcedure.Definitions;

/// <summary>
/// Supported result formats for an Oracle command or procedure.
/// </summary>
#pragma warning disable SA1602
public enum OracleCommandReturnType
{
    XmlString,
    XDocument,
    AffectedRows,
    JSONString,
    Parameters,
}
#pragma warning restore SA1602
