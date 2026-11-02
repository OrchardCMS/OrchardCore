using System.Net;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Localization;
using OrchardCore.Admin.Models;

namespace OrchardCore.Admin.TagHelpers;

/// <summary>
/// Declares a column in an admin-list tag or its admin-list-grid child.
/// </summary>
[HtmlTargetElement("admin-list-column", ParentTag = "admin-list")]
[HtmlTargetElement("admin-list-column", ParentTag = "admin-list-grid")]
public sealed class AdminListColumnTagHelper : TagHelper
{
    private AdminListColumnAlignment? _alignment;

    /// <summary>
    /// Optional built-in column preset: Select or Actions. Omit for a regular column.
    /// </summary>
    public string Type { get; set; }

    /// <summary>
    /// The column name and, by default, its row zone. Required for regular columns.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The column header and narrow-layout cell label. Accepts plain text, a LocalizedString,
    /// or HTML-aware localized content, which is normalized to text before rendering.
    /// </summary>
    public object Title { get; set; }

    /// <summary>
    /// The placement position, e.g. 20 or end. Preset positions are retained when omitted.
    /// </summary>
    public string Position { get; set; }

    /// <summary>
    /// Optional comma- or space-separated row zones instead of the column's named zone.
    /// </summary>
    public string Zones { get; set; }

    /// <summary>
    /// CSS width, or auto to hug the column's content.
    /// </summary>
    public string Width { get; set; }

    /// <summary>
    /// Additional header and cell CSS classes.
    /// </summary>
    public string CssClass { get; set; }

    /// <summary>
    /// Horizontal alignment. Preset alignment is retained when omitted.
    /// </summary>
    public AdminListColumnAlignment Alignment
    {
        get => _alignment ?? AdminListColumnAlignment.Start;
        set => _alignment = value;
    }

    /// <summary>
    /// Whether cell content must not wrap.
    /// </summary>
    public bool NoWrap { get; set; }

    /// <inheritdoc />
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        if (!context.Items.TryGetValue(typeof(AdminListColumnTagHelper), out var value) || value is not AdminListColumnCollection columns)
        {
            throw new InvalidOperationException("Admin list columns must be inside an admin-list tag.");
        }

        var titleText = GetTitleText();
        var title = titleText == null ? null : new LocalizedString(titleText, titleText);
        var column = Type?.ToUpperInvariant() switch
        {
            null => new AdminListColumn(),
            "SELECT" => AdminListColumns.Select(),
            "ACTIONS" => AdminListColumns.Actions(title),
            _ => throw new InvalidOperationException($"Unknown admin list column type '{Type}'. Use Select or Actions, or omit type for a regular column."),
        };

        column.Name = Name ?? column.Name;
        column.Title = title;
        column.Position = Position ?? column.Position;
        column.Width = Width ?? column.Width;
        column.CssClass = CssClass ?? column.CssClass;
        column.Alignment = _alignment ?? column.Alignment;
        column.NoWrap = NoWrap;
        if (Zones != null)
        {
            column.Zones = Zones.Split([',', ' '], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        }

        columns.Add(column);
        output.SuppressOutput();
    }

    private string GetTitleText()
    {
        switch (Title)
        {
            case null:
                return null;
            case string text:
                return text;
            case LocalizedString localized:
                return localized.Value;
            case IHtmlContent content:
                using (var writer = new StringWriter())
                {
                    content.WriteTo(writer, HtmlEncoder.Default);
                    return WebUtility.HtmlDecode(writer.ToString());
                }

            default:
                throw new InvalidOperationException("An admin list column title must be text, a LocalizedString, or IHtmlContent.");
        }
    }
}
