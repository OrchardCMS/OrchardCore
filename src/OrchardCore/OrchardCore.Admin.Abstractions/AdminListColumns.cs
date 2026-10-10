using Microsoft.Extensions.Localization;
using OrchardCore.Admin.Models;

namespace OrchardCore.Admin;

/// <summary>
/// The columns most admin lists share, so every list declares them the same way.
/// </summary>
public static class AdminListColumns
{
    /// <summary>
    /// The name of the <see cref="Select"/> column.
    /// </summary>
    public const string SelectName = "Select";

    /// <summary>
    /// The name of the <see cref="Actions"/> column.
    /// </summary>
    public const string ActionsName = "Actions";

    /// <summary>
    /// The first column of a list whose rows can be selected for a bulk action: the <c>Checkbox</c> zone of the
    /// row, at position <c>10</c>, as narrow as the checkbox.
    /// </summary>
    public static AdminListColumn Select() => new()
    {
        Name = SelectName,
        Position = "10",
        Zones = ["Checkbox"],
        CssClass = "admin-list-select",
        Width = AdminListColumn.AutoWidth,
    };

    /// <summary>
    /// The last column of a list: the <c>Actions</c> and <c>ActionsMenu</c> zones of the row, aligned to the end
    /// and as narrow as the buttons. Its position is <c>end</c>, so it stays last even when a provider adds a
    /// column without a position.
    /// </summary>
    /// <param name="title">The localized header of the column, e.g. <c>S["Actions"]</c>.</param>
    public static AdminListColumn Actions(LocalizedString title) => new()
    {
        Name = ActionsName,
        Position = "end",
        Title = title,
        Zones = ["Actions", "ActionsMenu"],
        Width = AdminListColumn.AutoWidth,
        Alignment = AdminListColumnAlignment.End,
    };
}
