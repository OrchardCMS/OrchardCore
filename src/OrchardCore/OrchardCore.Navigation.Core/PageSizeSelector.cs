using System.Globalization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace OrchardCore.Navigation;

/// <summary>
/// Builds the options of the <c>Pager_PageSizeSelector</c> shape. The pager renders the selector itself, and
/// a page that wants it somewhere else, e.g. an admin list placing it through its layout, builds the shape
/// with these options and turns the built-in one off with <c>ShowPageSizeSelector = false</c> on the pager.
/// </summary>
public static class PageSizeSelector
{
    // The keys a new page size makes stale: the page number and the cursors, and the page size itself.
    private static readonly HashSet<string> _resetKeys = new(StringComparer.OrdinalIgnoreCase)
    {
        "pagenum",
        "pageSize",
        "before",
        "after",
    };

    /// <summary>
    /// Builds the options of the page size selector: one per configured size, each a link to the current page
    /// with that size. Returns <see langword="null"/> when the selection is turned off, when no size is
    /// configured, or when there is no request to build the links from.
    /// </summary>
    /// <param name="serviceProvider">The services of the request.</param>
    /// <param name="currentPageSize">The size the listing is using, which is the selected option.</param>
    public static List<SelectListItem> BuildOptions(IServiceProvider serviceProvider, int currentPageSize)
    {
        var pagerOptions = serviceProvider.GetService<IOptions<PagerOptions>>()?.Value;

        if (pagerOptions is null || !pagerOptions.AllowPageSizeSelection || pagerOptions.PageSizeOptions is not { Length: > 0 })
        {
            return null;
        }

        var httpContext = serviceProvider.GetService<IHttpContextAccessor>()?.HttpContext;

        if (httpContext is null)
        {
            return null;
        }

        var request = httpContext.Request;
        var path = request.PathBase + request.Path;

        // Preserve the current query string, but reset the page number and cursor and override the page size.
        var preserved = request.Query
            .Where(pair => !_resetKeys.Contains(pair.Key))
            .ToList();

        var items = new List<SelectListItem>(pagerOptions.PageSizeOptions.Length);

        foreach (var size in pagerOptions.PageSizeOptions)
        {
            var text = size.ToString(CultureInfo.InvariantCulture);

            items.Add(new SelectListItem
            {
                Text = text,
                Value = path + QueryString.Create(preserved.Append(new KeyValuePair<string, StringValues>("pageSize", text))),
                Selected = size == currentPageSize,
            });
        }

        return items;
    }
}
