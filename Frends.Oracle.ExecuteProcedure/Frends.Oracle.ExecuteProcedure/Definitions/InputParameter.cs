// <copyright file="InputParameter.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Frends.Oracle.ExecuteProcedure.Definitions;

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

/// <summary>
/// Input parameter.
/// </summary>
public class InputParameter
{
    /// <summary>
    /// Gets or sets the name of the parameter.
    /// </summary>
    /// <example>"first_name".</example>
    [DefaultValue("ParameterName")]
    [DisplayFormat(DataFormatString = "Text")]
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the value of the parameter.
    /// </summary>
    /// <example>"Saija".</example>
    [DisplayFormat(DataFormatString = "Text")]
    public dynamic Value { get; set; }

    /// <summary>
    /// Gets or sets the data type of the parameter.
    /// </summary>
    /// <example>QueryParameterType.NVarchar2.</example>
    [DefaultValue(ProcedureParameterType.NVarchar2)]
    public ProcedureParameterType DataType { get; set; }

    /// <summary>
    /// Gets or sets size of the parameter.
    /// </summary>
    /// <example>255.</example>
    public int Size { get; set; }
}
