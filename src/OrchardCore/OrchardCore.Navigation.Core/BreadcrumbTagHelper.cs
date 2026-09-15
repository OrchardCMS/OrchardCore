using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.AspNetCore.Routing;
using OrchardCore.Admin;
using OrchardCore.Admin.Models;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Html;
using OrchardCore.DisplayManagement.Title;
using OrchardCore.Settings;

namespace OrchardCore.Navigation.TagHelpers;

/// <summary>
/// Renders the breadcrumb trail of the given name, and registers the text of its current node as a segment of the
/// page title. The nodes come from the <see cref="IBreadcrumbProvider"/> implementations of the trail, from the
/// <c>breadcrumb-item</c> children declared inline in the view, or from both.
/// </summary>
/// <example>
/// <code>
/// &lt;breadcrumb name="ContentsEdit" data="@(new { ContentItem = contentItem })" /&gt;
/// </code>
/// </example>
[HtmlTargetElement("breadcrumb", Attributes = NameAttribute)]
public sealed class BreadcrumbTagHelper : TagHelper
{
    private const string NameAttribute = "name";

    private readonly IBreadcrumbManager _breadcrumbManager;
    private readonly IShapeFactory _shapeFactory;
    private readonly IDisplayHelper _displayHelper;
    private readonly IPageTitleBuilder _pageTitleBuilder;
    private readonly ISiteService _siteService;

    public BreadcrumbTagHelper(
        IBreadcrumbManager breadcrumbManager,
        IShapeFactory shapeFactory,
        IDisplayHelper displayHelper,
        IPageTitleBuilder pageTitleBuilder,
        ISiteService siteService)
    {
        _breadcrumbManager = breadcrumbManager;
        _shapeFactory = shapeFactory;
        _displayHelper = displayHelper;
        _pageTitleBuilder = pageTitleBuilder;
        _siteService = siteService;
    }

    /// <summary>
    /// The name of the breadcrumb to render. e.g., <c>ContentsEdit</c>.
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
    /// The html tag of the page title, rendered below the trail from the text of the current node, so that the
    /// breadcrumb carries the title of the page. Defaults to <c>h1</c> on a request to the admin, and to no title
    /// everywhere else, where the page has a heading of its own. Set it to an empty value to render none.
    /// </summary>
    [HtmlAttributeName("heading")]
    public string Heading { get; set; }

    /// <summary>
    /// Whether the text of the current node is registered as a segment of the page title. Defaults to <c>true</c>.
    /// </summary>
    [HtmlAttributeName("page-title")]
    public bool PageTitle { get; set; } = true;

    /// <summary>
    /// The display type of the trail, which becomes an alternate of every shape it renders, so that a theme can give
    /// the admin and the front end a presentation of their own. Defaults to <c>DetailAdmin</c> on a request to the
    /// admin, and to <c>Detail</c> everywhere else.
    /// </summary>
    [HtmlAttributeName("display-type")]
    public string DisplayType { get; set; }

    [HtmlAttributeNotBound]
    [ViewContext]
    public ViewContext ViewContext { get; set; }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        // Let any 'breadcrumb-item' children declared in the view seed the trail. They add themselves to this list
        // while the child content renders.
        var inlineItems = new List<BreadcrumbItem>();
        context.Items[typeof(BreadcrumbItemTagHelper)] = inlineItems;

        await output.GetChildContentAsync();

        var items = await _breadcrumbManager.BuildBreadcrumbAsync(
            Name,
            ViewContext,
            Data is null ? null : new RouteValueDictionary(Data),
            inlineItems.Count > 0 ? inlineItems : null);

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

        // A trail rendered on the admin and a trail rendered by a front end theme are the same shape, so they are told
        // apart by their display type, the way the rest of the display system tells those contexts apart.
        var isAdmin = AdminAttribute.IsApplied(ViewContext.HttpContext);

        // The trail can be turned off for the whole admin from the admin settings, in which case only the page title
        // is rendered. The setting is about the admin, so a front end trail is never hidden by it.
        var showTrail = true;

        if (isAdmin)
        {
            var adminSettings = await _siteService.GetSettingsAsync<AdminSettings>();

            showTrail = adminSettings.ShowBreadcrumb;
        }

        var shape = await _shapeFactory.BreadcrumbAsync(
            Name,
            items,
            GetHeading(isAdmin),
            string.IsNullOrWhiteSpace(DisplayType) ? (isAdmin ? "DetailAdmin" : "Detail") : DisplayType,
            showTrail);

        output.TagName = null;
        output.Content.SetHtmlContent(await _displayHelper.ShapeExecuteAsync(shape));
    }

    /// <summary>
    /// On the admin the trail carries the title of the screen, rendered below it from the current node, so it defaults
    /// to an <c>h1</c>. Everywhere else the page has a heading of its own, and a second one would be wrong.
    /// </summary>
    private string GetHeading(bool isAdmin)
    {
        if (Heading is null)
        {
            return isAdmin ? "h1" : null;
        }

        return string.IsNullOrWhiteSpace(Heading) ? null : Heading;
    }
}
