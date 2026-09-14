using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.AspNetCore.Routing;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Html;
using OrchardCore.DisplayManagement.Title;

namespace OrchardCore.Navigation.TagHelpers;

/// <summary>
/// Renders the breadcrumb trail of the given name, and registers the text of its current node as a segment of the
/// page title.
/// </summary>
/// <example>
/// <code>
/// &lt;breadcrumb name="Contents.Edit" data="@(new { ContentItem = contentItem })" /&gt;
/// </code>
/// </example>
[HtmlTargetElement("breadcrumb", Attributes = NameAttribute)]
public class BreadcrumbTagHelper : TagHelper
{
    private const string NameAttribute = "name";

    private readonly IBreadcrumbManager _breadcrumbManager;
    private readonly IShapeFactory _shapeFactory;
    private readonly IDisplayHelper _displayHelper;
    private readonly IPageTitleBuilder _pageTitleBuilder;

    public BreadcrumbTagHelper(
        IBreadcrumbManager breadcrumbManager,
        IShapeFactory shapeFactory,
        IDisplayHelper displayHelper,
        IPageTitleBuilder pageTitleBuilder)
    {
        _breadcrumbManager = breadcrumbManager;
        _shapeFactory = shapeFactory;
        _displayHelper = displayHelper;
        _pageTitleBuilder = pageTitleBuilder;
    }

    /// <summary>
    /// The name of the breadcrumb to render. e.g., <c>Contents.Edit</c>.
    /// </summary>
    [HtmlAttributeName(NameAttribute)]
    public string Name { get; set; }

    /// <summary>
    /// The optional contextual data of the page, given to every <see cref="IBreadcrumbProvider"/> building the trail.
    /// Each property of the object becomes an entry of <see cref="BreadcrumbBuilder.Data"/>.
    /// </summary>
    [HtmlAttributeName("data")]
    public object Data { get; set; }

    /// <summary>
    /// The html tag wrapping the text of the current node, so that the breadcrumb can stand in for the title of the
    /// page. Defaults to <c>h1</c>. Set it to an empty value to render no heading.
    /// </summary>
    [HtmlAttributeName("heading")]
    public string Heading { get; set; } = "h1";

    /// <summary>
    /// Whether the text of the current node is registered as a segment of the page title. Defaults to <c>true</c>.
    /// </summary>
    [HtmlAttributeName("page-title")]
    public bool PageTitle { get; set; } = true;

    [HtmlAttributeNotBound]
    [ViewContext]
    public ViewContext ViewContext { get; set; }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var items = await _breadcrumbManager.BuildBreadcrumbAsync(
            Name,
            ViewContext,
            Data is null ? null : new RouteValueDictionary(Data));

        if (items.Count == 0)
        {
            output.SuppressOutput();

            return;
        }

        if (PageTitle)
        {
            var current = items[^1];

            if (!string.IsNullOrEmpty(current.Text))
            {
                _pageTitleBuilder.AddSegment(new HtmlContentString(current.Text));
            }
        }

        var shape = await _shapeFactory.BreadcrumbAsync(Name, items, string.IsNullOrWhiteSpace(Heading) ? null : Heading);

        output.TagName = null;
        output.Content.SetHtmlContent(await _displayHelper.ShapeExecuteAsync(shape));
    }
}
