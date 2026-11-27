namespace Frends.Oracle.ExecuteProcedure.Definitions;

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

/// <summary>
/// Options for executing an Oracle command or stored procedure.
/// </summary>
public class Options
{
    /// <summary>
    /// Command timeout in seconds.
    /// </summary>
    /// <example>30</example>
    [DefaultValue(30)]
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Whether Oracle binds parameters by name. When false, parameter order is significant.
    /// </summary>
    /// <example>true</example>
    [DefaultValue(true)]
    public bool BindParameterByName { get; set; } = true;

    /// <summary>
    /// Whether an exception is thrown when the task fails. When false, the failure is reported in the result.
    /// </summary>
    /// <example>true</example>
    [DefaultValue(true)]
    public bool ThrowErrorOnFailure { get; set; } = true;

    /// <summary>
    /// Custom context to prepend to the error message when the task fails.
    /// </summary>
    /// <example>Oracle procedure execution failed</example>
    [DisplayFormat(DataFormatString = "Text")]
    [DefaultValue("")]
    public string ErrorMessageOnFailure { get; set; } = string.Empty;
}
