using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using OrchardCore.DisplayManagement.Zones;

namespace OrchardCore.Navigation;

public class BreadcrumbManager : IBreadcrumbManager
{
    private readonly IEnumerable<IBreadcrumbProvider> _breadcrumbProviders;
    private readonly IUrlHelperFactory _urlHelperFactory;
    private readonly IAuthorizationService _authorizationService;
    private readonly ILogger _logger;

    private IUrlHelper _urlHelper;

    public BreadcrumbManager(
        IEnumerable<IBreadcrumbProvider> breadcrumbProviders,
        IUrlHelperFactory urlHelperFactory,
        IAuthorizationService authorizationService,
        ILogger<BreadcrumbManager> logger)
    {
        _breadcrumbProviders = breadcrumbProviders;
        _urlHelperFactory = urlHelperFactory;
        _authorizationService = authorizationService;
        _logger = logger;
    }

    public async Task<IList<BreadcrumbItem>> BuildBreadcrumbAsync(string name, ActionContext actionContext, IReadOnlyDictionary<string, object> data = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        ArgumentNullException.ThrowIfNull(actionContext);

        var builder = new BreadcrumbBuilder(name, data);

        // Process all breadcrumb providers to create a flat list of nodes.
        // If a breadcrumb provider fails, it is ignored.
        foreach (var breadcrumbProvider in _breadcrumbProviders)
        {
            try
            {
                await breadcrumbProvider.BuildBreadcrumbAsync(builder);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An exception occurred while building the breadcrumb '{BreadcrumbName}'.", name);
            }
        }

        // Order the nodes by their position, keeping the order they were added in for equal positions.
        var items = builder.Build()
            .OrderBy(item => item, FlatPositionComparer.Instance)
            .ToList();

        var user = actionContext.HttpContext?.User;

        foreach (var item in items)
        {
            // A node the user is not authorized to reach is still rendered, but not as a link, so that the trail
            // stays complete.
            item.Href = await IsAuthorizedAsync(item, user)
                ? GetUrl(item, actionContext)
                : null;
        }

        if (items.Count > 0)
        {
            // The last node is the page being rendered, it never links to itself.
            var current = items[^1];

            current.IsCurrent = true;
            current.Href = null;
        }

        return items;
    }

    /// <summary>
    /// Gets the url of a node based on its <see cref="BreadcrumbItem.Url"/> or <see cref="BreadcrumbItem.RouteValues"/> values.
    /// </summary>
    private string GetUrl(BreadcrumbItem item, ActionContext actionContext)
    {
        if (item.RouteValues?.Count > 0)
        {
            _urlHelper ??= _urlHelperFactory.GetUrlHelper(actionContext);

            return _urlHelper.RouteUrl(new UrlRouteContext { Values = item.RouteValues });
        }

        var url = item.Url;

        if (string.IsNullOrEmpty(url))
        {
            return null;
        }

        if (url[0] == '/' || url.Contains("://"))
        {
            // Return the unescaped url and let the browser generate all uri components.
            return url;
        }

        if (url.StartsWith("~/", StringComparison.Ordinal))
        {
            url = url[2..];
        }

        // Use the unescaped 'Value' to not encode some possible reserved delimiters.
        return actionContext.HttpContext.Request.PathBase.Add($"/{url}").Value;
    }

    /// <summary>
    /// Checks whether the user has every permission a node requires.
    /// </summary>
    private async Task<bool> IsAuthorizedAsync(BreadcrumbItem item, ClaimsPrincipal user)
    {
        if (user == null || item.Permissions.Count == 0)
        {
            return true;
        }

        // When multiple permissions are supplied all permissions must be authorized.
        foreach (var permission in item.Permissions)
        {
            if (!await _authorizationService.AuthorizeAsync(user, permission, item.Resource))
            {
                return false;
            }
        }

        return true;
    }
}
