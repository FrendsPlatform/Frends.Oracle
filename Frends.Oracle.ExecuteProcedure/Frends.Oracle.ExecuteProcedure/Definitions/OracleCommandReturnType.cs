// <copyright file="OracleCommandReturnType.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Frends.Oracle.ExecuteProcedure.Definitions;

/// <summary>
/// Defines the supported formats for procedure results.
/// </summary>
public enum OracleCommandReturnType
{
    /// <summary>
    /// Returns XML as a string.
    /// </summary>
    XmlString,

    /// <summary>
    /// Returns an XML document.
    /// </summary>
    XDocument,

    /// <summary>
    /// Returns the number of affected rows.
    /// </summary>
    AffectedRows,

    /// <summary>
    /// Returns JSON as a string.
    /// </summary>
    JSONString,

    /// <summary>
    /// Returns the procedure's output parameters.
    /// </summary>
    Parameters,
}