using Microsoft.Extensions.Localization;
using OrchardCore.Admin.Models;

namespace OrchardCore.OpenId;

/// <summary>
/// The admin list of OpenID applications rendered by the <c>AdminList</c> shape.
/// </summary>
public static class OpenIdApplicationsAdminList
{
    /// <summary>
    /// The name of the list, used by <see cref="Admin.IAdminListColumnProvider"/> and the
    /// <c>AdminList__OpenIdApplications</c> and <c>AdminListCell__OpenIdApplications__{Column}</c> alternates.
    /// </summary>
    public const string Name = "OpenIdApplications";

    /// <summary>
    /// The default columns used by layouts with columns, e.g. <c>Table</c>. Each column renders one or more
    /// zones of the <c>OpenIdApplicationEntry_SummaryAdmin</c> shape.
    /// </summary>
    /// <remarks>
    /// There is no selection column: the page has no bulk actions to apply to the selected rows.
    /// </remarks>
    public static List<AdminListColumn> GetDefaultColumns(IStringLocalizer S) =>
    [
        new()
        {
            // The display name takes the space left by the actions.
            Name = "DisplayName",
            Position = "10",
            Title = S["Display name"],
            Zones = ["Content"],
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

/// <summary>
/// The admin list of OpenID scopes rendered by the <c>AdminList</c> shape.
/// </summary>
public static class OpenIdScopesAdminList
{
    /// <summary>
    /// The name of the list, used by <see cref="Admin.IAdminListColumnProvider"/> and the
    /// <c>AdminList__OpenIdScopes</c> and <c>AdminListCell__OpenIdScopes__{Column}</c> alternates.
    /// </summary>
    public const string Name = "OpenIdScopes";

    /// <summary>
    /// The default columns used by layouts with columns, e.g. <c>Table</c>. Each column renders one or more
    /// zones of the <c>OpenIdScopeEntry_SummaryAdmin</c> shape.
    /// </summary>
    /// <remarks>
    /// There is no selection column: the page has no bulk actions to apply to the selected rows.
    /// </remarks>
    public static List<AdminListColumn> GetDefaultColumns(IStringLocalizer S) =>
    [
        new()
        {
            // The display name and the description take the space left by the other columns.
            Name = "DisplayName",
            Position = "10",
            Title = S["Display name"],
            Zones = ["Content", "Description"],
        },
        new()
        {
            Name = "Name",
            Position = "20",
            Title = S["Name"],
            Zones = ["Tags"],
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
