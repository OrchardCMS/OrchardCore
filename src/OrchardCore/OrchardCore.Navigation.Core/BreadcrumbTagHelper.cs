using System.Net;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Admin;
using OrchardCore.Admin.Models;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Html;
using OrchardCore.DisplayManagement.Title;
using OrchardCore.DisplayManagement.Zones;
using OrchardCore.Security.Permissions;
using OrchardCore.Settings;

namespace OrchardCore.Navigation.TagHelpers;

/// <summary>
/// Renders the breadcrumb trail of the given name. Ancestors are collected from <c>breadcrumb-item</c> children,
/// then updated by registered providers. The explicit title is appended last as the current node.
/// </summary>
/// <remarks>
/// When the admin breadcrumb setting is disabled, renders only the heading and page title without creating shapes.
/// Providers, link, permission and shape services are not resolved. Children are not evaluated and no breadcrumb
/// nodes are allocated; only the explicit title is used.
/// </remarks>
/// <example>
/// <code>
/// &lt;breadcrumb name="ContentsEdit" title="@T["Edit Article"]"&gt;
///     &lt;breadcrumb-item action="List" controller="Admin" area="OrchardCore.Contents"&gt;Manage Content&lt;/breadcrumb-item&gt;
/// &lt;/breadcrumb&gt;
/// </code>
/// </example>
[HtmlTargetElement("breadcrumb", Attributes = NameAttribute)]
public sealed class BreadcrumbTagHelper : TagHelper
{
    private const string NameAttribute = "name";

    private readonly IPageTitleBuilder _pageTitleBuilder;
    private readonly ISiteService _siteService;
    private IAuthorizationService _authorizationService;
    private IPermissionService _permissionService;
    private IUrlHelper _urlHelper;

    public BreadcrumbTagHelper(
        IPageTitleBuilder pageTitleBuilder,
        ISiteService siteService)
    {
        _pageTitleBuilder = pageTitleBuilder;
        _siteService = siteService;
    }

    /// <summary>
    /// The stable trail name used by providers to target a breadcrumb and by rendering to generate CSS classes
    /// and shape alternates. e.g., <c>ContentsEdit</c>.
    /// </summary>
    [HtmlAttributeName(NameAttribute)]
    public string Name { get; set; }

    /// <summary>
    /// The required HTML-aware page title, including a
    /// <see cref="Microsoft.AspNetCore.Mvc.Localization.LocalizedHtmlString"/> from the view localizer.
    /// Wrap plain text in <see cref="HtmlContentString"/> so it is safely encoded.
    /// It is normalized to text for the heading and browser title. In a visible trail, it is also appended as the last,
    /// unlinked node after providers have run. An empty value suppresses the title text but still adds a node to a visible trail.
    /// </summary>
    [HtmlAttributeName("title")]
    public IHtmlContent Title { get; set; }

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
        ArgumentException.ThrowIfNullOrEmpty(Name);
        ArgumentNullException.ThrowIfNull(Title);

        var isAdmin = AdminAttribute.IsApplied(ViewContext.HttpContext);
        var heading = GetHeading(isAdmin);
        var showTrail = !isAdmin || (await _siteService.GetSettingsAsync<AdminSettings>()).ShowBreadcrumb;

        if (!showTrail && !PageTitle && string.IsNullOrEmpty(heading))
        {
            output.SuppressOutput();

            return;
        }

        var title = GetTitleText();
        List<BreadcrumbItem> items = null;

        if (showTrail)
        {
            items = [];
            var breadcrumbContext = new BreadcrumbContext(Name, title, items, ViewContext, showTrail);
            context.Items[typeof(BreadcrumbItemTagHelper)] = breadcrumbContext;

            await output.GetChildContentAsync();

            foreach (var provider in ViewContext.HttpContext.RequestServices.GetServices<IBreadcrumbProvider>())
            {
                await provider.BuildBreadcrumbAsync(breadcrumbContext);
            }

            items = items.OrderBy(item => item, FlatPositionComparer.Instance).ToList();

            foreach (var item in items)
            {
                item.IsCurrent = false;
                item.Href = await GetHrefAsync(item);
            }

            items.Add(new BreadcrumbItem { Text = title, Id = "Title", IsCurrent = true });
        }

        if (PageTitle && !string.IsNullOrEmpty(title))
        {
            _pageTitleBuilder.AddSegment(new HtmlContentString(title));
        }

        if (!showTrail)
        {
            if (string.IsNullOrEmpty(heading) || string.IsNullOrEmpty(title))
            {
                output.SuppressOutput();

                return;
            }

            var titleTag = new TagBuilder(heading);
            titleTag.AddCssClass("oc-breadcrumb-title");
            titleTag.InnerHtml.Append(title);

            output.TagName = null;
            output.Content.SetHtmlContent(titleTag);

            return;
        }

        var shapeFactory = ViewContext.HttpContext.RequestServices.GetRequiredService<IShapeFactory>();
        var displayHelper = ViewContext.HttpContext.RequestServices.GetRequiredService<IDisplayHelper>();
        var shape = await shapeFactory.BreadcrumbAsync(
            Name,
            items,
            heading,
            string.IsNullOrWhiteSpace(DisplayType) ? (isAdmin ? "DetailAdmin" : "Detail") : DisplayType);

        output.TagName = null;
        output.Content.SetHtmlContent(await displayHelper.ShapeExecuteAsync(shape));
    }

    private string GetTitleText()
    {
        using var writer = new StringWriter();
        Title.WriteTo(writer, HtmlEncoder.Default);

        // HTML-localized arguments are already encoded. Normalize once before the text-only renderers encode them.
        return WebUtility.HtmlDecode(writer.ToString());
    }

    private async Task<string> GetHrefAsync(BreadcrumbItem item)
    {
        if (!item.LinkEnabled || (item.RouteValues is not { Count: > 0 } && string.IsNullOrEmpty(item.Url)))
        {
            return null;
        }

        var httpContext = ViewContext.HttpContext;

        if (!string.IsNullOrEmpty(item.PermissionName))
        {
            _permissionService ??= httpContext.RequestServices.GetRequiredService<IPermissionService>();
            var permission = await _permissionService.FindByNameAsync(item.PermissionName);

            if (permission is not null && !await IsAuthorizedAsync(permission, item.Resource))
            {
                return null;
            }
        }

        foreach (var permission in item.Permissions)
        {
            if (!await IsAuthorizedAsync(permission, item.Resource))
            {
                return null;
            }
        }

        if (item.RouteValues is { Count: > 0 })
        {
            _urlHelper ??= httpContext.RequestServices.GetRequiredService<IUrlHelperFactory>().GetUrlHelper(ViewContext);

            return _urlHelper.RouteUrl(new UrlRouteContext { Values = item.RouteValues });
        }

        var url = item.Url;

        if (url[0] == '/' || url.Contains("://"))
        {
            return url;
        }

        if (url.StartsWith("~/", StringComparison.Ordinal))
        {
            url = url[2..];
        }

        return httpContext.Request.PathBase.Add($"/{url}").Value;
    }

    private Task<bool> IsAuthorizedAsync(Permission permission, object resource)
    {
        var httpContext = ViewContext.HttpContext;
        _authorizationService ??= httpContext.RequestServices.GetRequiredService<IAuthorizationService>();

        return _authorizationService.AuthorizeAsync(httpContext.User, permission, resource);
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
