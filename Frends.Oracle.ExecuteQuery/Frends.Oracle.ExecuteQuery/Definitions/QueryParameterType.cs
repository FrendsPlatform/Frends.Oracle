namespace Frends.Oracle.ExecuteQuery.Definitions;

/// <summary>
/// Oracle parameter data types.
/// </summary>
public enum QueryParameterType
{
    /// <summary>National character set string with variable length.</summary>
    NVarchar2,

    /// <summary>Character string with variable length.</summary>
    Varchar2,

    /// <summary>National character set string with fixed length.</summary>
    NChar,

    /// <summary>Character string with fixed length.</summary>
    Char,

    /// <summary>16-bit integer.</summary>
    Int16,

    /// <summary>32-bit integer.</summary>
    Int32,

    /// <summary>64-bit integer.</summary>
    Int64,

    /// <summary>Double-precision floating-point number.</summary>
    Double,

    /// <summary>Decimal number.</summary>
    Decimal,

    /// <summary>Long character data.</summary>
    Long,

    /// <summary>Long binary data.</summary>
    LongRaw,

    /// <summary>Boolean value.</summary>
    Boolean,

    /// <summary>Date and time value.</summary>
    Date,

    /// <summary>Timestamp value.</summary>
    TimeStamp,

    /// <summary>Timestamp with local time zone.</summary>
    TimeStampLTZ,

    /// <summary>Timestamp with time zone.</summary>
    TimeStampTZ,

    /// <summary>XML data.</summary>
    XmlType,

    /// <summary>Binary data.</summary>
    Raw,

    /// <summary>External binary file locator.</summary>
    BFile,

    /// <summary>Double-precision binary floating-point number.</summary>
    BinaryDouble,

    /// <summary>Single-precision binary floating-point number.</summary>
    BinaryFloat,

    /// <summary>Binary large object.</summary>
    Blob,

    /// <summary>8-bit integer.</summary>
    Byte,

    /// <summary>Character large object.</summary>
    Clob,

    /// <summary>National character large object.</summary>
    NClob,

    /// <summary>Day-to-second interval.</summary>
    IntervalDS,

    /// <summary>Year-to-month interval.</summary>
    IntervalYM,

    /// <summary>Reference cursor.</summary>
    RefCursor,

    /// <summary>Single-precision floating-point number.</summary>
    Single,
}
