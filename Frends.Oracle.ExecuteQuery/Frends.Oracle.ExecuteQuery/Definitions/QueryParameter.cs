using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Frends.Oracle.ExecuteQuery.Definitions;

/// <summary>
/// Query parameter.
/// </summary>
public class QueryParameter
{
    /// <summary>
    /// The name of the parameter.
    /// </summary>
    /// <example>"first_name"</example>
    [DefaultValue("ParameterName")]
    [DisplayFormat(DataFormatString = "Text")]
    public string Name { get; set; }

    /// <summary>
    /// The value of the parameter.
    /// </summary>
    /// <example>"Saija"</example>
    [DefaultValue("Parameter value")]
    [DisplayFormat(DataFormatString = "Text")]
    public dynamic Value { get; set; }

    /// <summary>
    /// The data type of the parameter.
    /// </summary>
    /// <example>QueryParameterType.NVarchar2</example>
    [DefaultValue(QueryParameterType.NVarchar2)]
    public QueryParameterType DataType { get; set; }
}
