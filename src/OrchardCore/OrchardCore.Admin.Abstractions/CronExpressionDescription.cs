namespace OrchardCore.Admin;

/// <summary>
/// Where the <c>CronExpression</c> shape shows the description of a cron expression, such as "At 03:00 AM".
/// </summary>
public enum CronExpressionDescription
{
    /// <summary>
    /// The description is shown as text next to the expression.
    /// </summary>
    Text,

    /// <summary>
    /// The description is the tooltip of the expression, where there is no room for its text.
    /// </summary>
    Tooltip,

    /// <summary>
    /// The expression is shown without its description.
    /// </summary>
    None,
}
