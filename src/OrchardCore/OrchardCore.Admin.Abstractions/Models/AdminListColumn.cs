using Microsoft.Extensions.Localization;

namespace OrchardCore.Admin.Models;

/// <summary>
/// Describes a column of an admin list when it is rendered with a layout that has columns, e.g. the <c>Table</c> or <c>Grid</c> layouts.
/// A column renders one or more zones of the row shape (e.g. <c>Content_SummaryAdmin</c>) in a single cell.
/// </summary>
public class AdminListColumn
{
    /// <summary>
    /// The technical name of the column, e.g. <c>Title</c>. It is used to build the alternates of the
    /// <c>AdminListCell</c> shape (<c>AdminListCell__{Name}</c> and <c>AdminListCell__{ListName}__{Name}</c>) and as a CSS class.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The localized header text of the column. Can be <see langword="null"/> for columns without a header, e.g. a selection checkbox.
    /// </summary>
    public LocalizedString Title { get; set; }

    /// <summary>
    /// The zones of the row shape that are rendered in this column, in order.
    /// </summary>
    public string[] Zones { get; set; } = [];

    /// <summary>
    /// The position of the column using the placement position syntax, e.g. <c>10</c>, <c>25</c> or <c>35.5</c>.
    /// Columns are sorted by position once every <see cref="IAdminListColumnProvider"/> ran, so a feature can insert
    /// a column between two others without knowing which other features add columns. The list owner assigns
    /// increasing positions to its default columns (e.g. <c>10</c>, <c>20</c>, ...); a column added without a
    /// position is placed after all positioned columns.
    /// </summary>
    public string Position { get; set; }

    /// <summary>
    /// Optional CSS classes added to the header and the cells of this column.
    /// </summary>
    public string CssClass { get; set; }

    /// <summary>
    /// The width of the column: any CSS width such as <c>20%</c> or <c>12rem</c>, or <see cref="AutoWidth"/> to make
    /// the column as narrow as its content (e.g. a checkbox or the action buttons). When <see langword="null"/> the
    /// column shares the remaining space with the other columns without a width.
    /// </summary>
    public string Width { get; set; }

    /// <summary>
    /// The horizontal alignment of the header and the cells of this column.
    /// </summary>
    public AdminListColumnAlignment Alignment { get; set; } = AdminListColumnAlignment.Start;

    /// <summary>
    /// Whether the content of the cells must not wrap, e.g. dates and badges.
    /// </summary>
    public bool NoWrap { get; set; }

    /// <summary>
    /// The <see cref="Width"/> value making the column as narrow as its content.
    /// </summary>
    public const string AutoWidth = "auto";

    /// <summary>
    /// Whether <see cref="Width"/> is <see cref="AutoWidth"/>.
    /// </summary>
    public bool HasAutoWidth => string.Equals(Width, AutoWidth, StringComparison.OrdinalIgnoreCase);
}

/// <summary>
/// The horizontal alignment of an <see cref="AdminListColumn"/>.
/// </summary>
public enum AdminListColumnAlignment
{
    Start,
    Center,
    End,
}
