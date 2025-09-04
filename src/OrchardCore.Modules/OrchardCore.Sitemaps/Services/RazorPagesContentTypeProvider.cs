using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.Sitemaps.Builders;

namespace OrchardCore.Sitemaps.Services;

public class RazorPagesContentTypeProvider : IRouteableContentTypeProvider
{
    private readonly SitemapsRazorPagesOptions _options;
    private readonly IContentDefinitionManager _contentDefinitionManager;

    public RazorPagesContentTypeProvider(
        IOptions<SitemapsRazorPagesOptions> options,
        IContentDefinitionManager contentDefinitionManager
        )
    {
        _options = options.Value;
        _contentDefinitionManager = contentDefinitionManager;
    }

    public Task<string> GetRouteAsync(SitemapBuilderContext context, ContentItem contentItem)
    {
        var option = _options.ContentTypeOptions.FirstOrDefault(o => o.ContentType == contentItem.ContentType);
        if (option != null && option.RouteValues != null)
        {
            var pageName = string.IsNullOrEmpty(option.PageName) ? option.ContentType : option.PageName;

            // When used from outside a razor page name must start with a /
            if (!pageName.StartsWith('/'))
            {
                pageName = '/' + pageName;
            }

            var url = context.HostPrefix + context.UrlHelper.Page(pageName, option.RouteValues.Invoke(contentItem));
            return Task.FromResult(url);
        }

        return Task.FromResult<string>(null);
    }

    public async Task<IEnumerable<ContentTypeDefinition>> ListRoutableTypeDefinitionsAsync()
    {
        var definitions = await _contentDefinitionManager.ListTypeDefinitionsAsync();

        return definitions.Where(definition => _options.ContentTypeOptions.Any(o => o.ContentType == definition.Name));
    }
}
