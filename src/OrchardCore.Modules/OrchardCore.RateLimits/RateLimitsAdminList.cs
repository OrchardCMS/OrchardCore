using Microsoft.Extensions.Localization;
using OrchardCore.Admin.Models;

namespace OrchardCore.RateLimits;

/// <summary>
/// The admin list of rate limit policies rendered by the <c>AdminList</c> shape.
/// </summary>
public static class RateLimitsAdminList
{
    /// <summary>
    /// The name of the list, used by <see cref="OrchardCore.Admin.IAdminListColumnProvider"/> and the
    /// <c>AdminList__RateLimits</c> and <c>AdminListCell__RateLimits__{Column}</c> alternates.
    /// </summary>
    public const string Name = "RateLimits";

    /// <summary>
    /// The default columns used by layouts with columns, e.g. <c>Table</c>. Each column renders one or more
    /// zones of the <c>RateLimitPolicy_SummaryAdmin</c> shape.
    /// </summary>
    public static List<AdminListColumn> GetDefaultColumns(IStringLocalizer S) =>
    [
        new()
        {
            Name = "Select",
            Position = "10",
            Zones = ["Checkbox"],
            CssClass = "admin-list-select",
            Width = AdminListColumn.AutoWidth,
        },
        new()
        {
            // The name and what the policy targets take the space left by the other columns.
            Name = "Name",
            Position = "20",
            Title = S["Name"],
            Zones = ["Content"],
        },
        new()
        {
            Name = "Status",
            Position = "30",
            Title = S["Status"],
            Zones = ["Tags"],
            Width = AdminListColumn.AutoWidth,
        },
        new()
        {
            Name = "Enabled",
            Position = "40",
            Title = S["Enabled"],
            Zones = ["Meta"],
            Width = AdminListColumn.AutoWidth,
        },
        new()
        {
            // "end" keeps the actions last even when a feature adds a column without a position.
            Name = "Actions",
            Position = "end",
            Title = S["Actions"],
            Zones = ["Actions", "ActionsMenu"],
            Width = AdminListColumn.AutoWidth,
            Alignment = AdminListColumnAlignment.End,
        },
    ];
}
