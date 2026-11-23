// <copyright file="ProcedureParameterType.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Frends.Oracle.ExecuteProcedure.Definitions
{
    /// <summary>
    /// Enumerator representing oracle parameter data types.
    /// </summary>
    public enum ProcedureParameterType
    {
        /// <summary>
        /// An external binary file.
        /// </summary>
        BFile = 101,

        /// <summary>
        /// A binary large object.
        /// </summary>
        Blob = 102,

        /// <summary>
        /// A byte value.
        /// </summary>
        Byte = 103,

        /// <summary>
        /// A fixed-length character value.
        /// </summary>
        Char = 104,

        /// <summary>
        /// A character large object.
        /// </summary>
        Clob = 105,

        /// <summary>
        /// A date and time value.
        /// </summary>
        Date = 106,

        /// <summary>
        /// A decimal number.
        /// </summary>
        Decimal = 107,

        /// <summary>
        /// A double-precision floating-point number.
        /// </summary>
        Double = 108,

        /// <summary>
        /// A long character value.
        /// </summary>
        Long = 109,

        /// <summary>
        /// A long raw binary value.
        /// </summary>
        LongRaw = 110,

        /// <summary>
        /// A 16-bit integer.
        /// </summary>
        Int16 = 111,

        /// <summary>
        /// A 32-bit integer.
        /// </summary>
        Int32 = 112,

        /// <summary>
        /// A 64-bit integer.
        /// </summary>
        Int64 = 113,

        /// <summary>
        /// An interval expressed in days and seconds.
        /// </summary>
        IntervalDS = 114,

        /// <summary>
        /// An interval expressed in years and months.
        /// </summary>
        IntervalYM = 115,

        /// <summary>
        /// A national character large object.
        /// </summary>
        NClob = 116,

        /// <summary>
        /// A fixed-length national character value.
        /// </summary>
        NChar = 117,

        /// <summary>
        /// A variable-length national character value.
        /// </summary>
        NVarchar2 = 119,

        /// <summary>
        /// A raw binary value.
        /// </summary>
        Raw = 120,

        /// <summary>
        /// A reference cursor.
        /// </summary>
        RefCursor = 121,

        /// <summary>
        /// A single-precision floating-point number.
        /// </summary>
        Single = 122,

        /// <summary>
        /// A timestamp value.
        /// </summary>
        TimeStamp = 123,

        /// <summary>
        /// A timestamp with local time zone.
        /// </summary>
        TimeStampLTZ = 124,

        /// <summary>
        /// A timestamp with time zone.
        /// </summary>
        TimeStampTZ = 125,

        /// <summary>
        /// A variable-length character value.
        /// </summary>
        Varchar2 = 126,

        /// <summary>
        /// An XML value.
        /// </summary>
        XmlType = 127,

        /// <summary>
        /// A binary double-precision floating-point number.
        /// </summary>
        BinaryDouble = 132,

        /// <summary>
        /// A binary single-precision floating-point number.
        /// </summary>
        BinaryFloat = 133,

        /// <summary>
        /// A Boolean value.
        /// </summary>
        Boolean = 134,
    }
}
