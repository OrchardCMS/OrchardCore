using Microsoft.Extensions.Localization;
using OrchardCore.Admin.Models;

namespace OrchardCore.Tenants;

/// <summary>
/// The admin list of feature profiles rendered by the <c>AdminList</c> shape.
/// </summary>
public static class FeatureProfilesAdminList
{
    /// <summary>
    /// The name of the list, used by <see cref="Admin.IAdminListColumnProvider"/> and the
    /// <c>AdminList__FeatureProfiles</c> and <c>AdminListCell__FeatureProfiles__{Column}</c> alternates.
    /// </summary>
    public const string Name = "FeatureProfiles";

    /// <summary>
    /// The default columns used by layouts with columns, e.g. <c>Table</c>. Each column renders one or more
    /// zones of the <c>FeatureProfileEntry_SummaryAdmin</c> shape.
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
            // The name takes the space left by the actions.
            Name = "Name",
            Position = "20",
            Title = S["Name"],
            Zones = ["Content", "Description"],
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
