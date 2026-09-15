using Microsoft.Extensions.Localization;
using OrchardCore.Admin.Models;

namespace OrchardCore.ContentTypes;

/// <summary>
/// The admin list of content types rendered by the <c>AdminList</c> shape.
/// </summary>
public static class ContentTypesAdminList
{
    /// <summary>
    /// The name of the list, used by <see cref="Admin.IAdminListColumnProvider"/> and the
    /// <c>AdminList__ContentTypes</c> and <c>AdminListCell__ContentTypes__{Column}</c> alternates.
    /// </summary>
    public const string Name = "ContentTypes";

    /// <summary>
    /// The default columns used by layouts with columns, e.g. <c>Table</c>. There is no selection column:
    /// the page has no bulk actions to apply to the selected rows.
    /// </summary>
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
            Name = "TechnicalName",
            Position = "20",
            Title = S["Technical name"],
            Zones = ["Tags"],
            Width = AdminListColumn.AutoWidth,
        },
        new()
        {
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
/// The admin list of content parts rendered by the <c>AdminList</c> shape.
/// </summary>
public static class ContentPartsAdminList
{
    /// <summary>
    /// The name of the list, used by <see cref="Admin.IAdminListColumnProvider"/> and the
    /// <c>AdminList__ContentParts</c> and <c>AdminListCell__ContentParts__{Column}</c> alternates.
    /// </summary>
    public const string Name = "ContentParts";

    /// <summary>
    /// The default columns used by layouts with columns, e.g. <c>Table</c>. There is no selection column:
    /// the page has no bulk actions to apply to the selected rows.
    /// </summary>
    public static List<AdminListColumn> GetDefaultColumns(IStringLocalizer S) =>
    [
        new()
        {
            // The display name and the description take the space left by the actions.
            Name = "DisplayName",
            Position = "10",
            Title = S["Display name"],
            Zones = ["Content", "Description"],
        },
        new()
        {
            Name = "Actions",
            Position = "end",
            Title = S["Actions"],
            Zones = ["Actions", "ActionsMenu"],
            Width = AdminListColumn.AutoWidth,
            Alignment = AdminListColumnAlignment.End,
        },
    ];
}
