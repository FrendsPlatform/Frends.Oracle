// <copyright file="OracleCommandType.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Frends.Oracle.ExecuteProcedure.Definitions;

/// <summary>
/// Enumeration for the comnmand type.
/// </summary>
public enum OracleCommandType
{
    /// <summary>
    /// Used to make commands in the CommandType.Text.
    /// </summary>
    Command,

    /// <summary>
    /// Used to call stored procedures as CommandType.StoredProcedure.
    /// </summary>
    StoredProcedure,
}
