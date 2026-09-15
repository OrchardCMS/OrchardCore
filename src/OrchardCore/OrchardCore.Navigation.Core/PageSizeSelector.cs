using System.Globalization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace OrchardCore.Navigation;

/// <summary>
/// Builds the options of the <c>Pager_PageSizeSelector</c> shape. The pager renders the selector itself, and
/// a page that wants it somewhere else, e.g. an admin list placing it through its layout, builds the shape
/// with these options and turns the built-in one off with <c>ShowPageSizeSelector = false</c> on the pager.
/// </summary>
public static class PageSizeSelector
{
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
        var basePath = (request.PathBase + request.Path).Value;

        // Preserve the current query string, but reset the page number and cursor and override the page size.
        var preserved = new List<KeyValuePair<string, string>>();

        foreach (var pair in QueryHelpers.ParseQuery(request.QueryString.Value))
        {
            if (string.Equals(pair.Key, "pagenum", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(pair.Key, "pageSize", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(pair.Key, "before", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(pair.Key, "after", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            foreach (var value in pair.Value)
            {
                preserved.Add(new KeyValuePair<string, string>(pair.Key, value));
            }
        }

        var items = new List<SelectListItem>(pagerOptions.PageSizeOptions.Length);

        foreach (var size in pagerOptions.PageSizeOptions)
        {
            var optionParams = new List<KeyValuePair<string, string>>(preserved)
            {
                new("pageSize", size.ToString(CultureInfo.InvariantCulture)),
            };

            items.Add(new SelectListItem
            {
                Text = size.ToString(CultureInfo.InvariantCulture),
                Value = QueryHelpers.AddQueryString(basePath, optionParams),
                Selected = size == currentPageSize,
            });
        }

        return items;
    }
}
