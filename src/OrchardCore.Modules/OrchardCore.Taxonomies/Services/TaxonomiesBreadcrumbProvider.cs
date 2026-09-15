using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement;
using OrchardCore.Navigation;

namespace OrchardCore.Taxonomies.Services;

/// <summary>
/// Describes the breadcrumb trails of the taxonomy term screens. A term is edited from its taxonomy, which is a content
/// item reached from the content items list, so its trail leads back through both.
/// </summary>
public sealed class TaxonomiesBreadcrumbProvider : IBreadcrumbProvider
{
    private static readonly RouteValueDictionary s_contentsRouteValues = new()
    {
        { "area", "OrchardCore.Contents" },
    };

    private readonly IContentManager _contentManager;

    internal readonly IStringLocalizer S;

    public TaxonomiesBreadcrumbProvider(IContentManager contentManager, IStringLocalizer<TaxonomiesBreadcrumbProvider> stringLocalizer)
    {
        _contentManager = contentManager;
        S = stringLocalizer;
    }

    public async ValueTask BuildBreadcrumbAsync(BreadcrumbBuilder builder)
    {
        switch (builder.Name)
        {
            case TaxonomiesConstants.Create:
                AddManageContent(builder);
                await AddTaxonomyAsync(builder);
                builder.Add(S["New {0}", builder.GetData<string>(TaxonomiesConstants.TermTypeDisplayNameKey)],
                    item => item.Id("Term"));
                break;

            case TaxonomiesConstants.Edit:
                AddManageContent(builder);
                await AddTaxonomyAsync(builder);
                builder.Add(S["Edit {0}", builder.GetData<string>(TaxonomiesConstants.TermTypeDisplayNameKey)],
                    item => item.Id("Term"));
                break;
        }
    }

    private void AddManageContent(BreadcrumbBuilder builder)
        => builder.Add(S["Manage Content"], item => item
            .Id("Contents")
            .Action("List", "Admin", new RouteValueDictionary(s_contentsRouteValues)
            {
                { "contentTypeId", string.Empty },
            }));

    private async ValueTask AddTaxonomyAsync(BreadcrumbBuilder builder)
    {
        var taxonomyContentItemId = builder.GetData<string>(TaxonomiesConstants.TaxonomyContentItemIdKey);

        if (string.IsNullOrEmpty(taxonomyContentItemId))
        {
            return;
        }

        var taxonomy = await _contentManager.GetAsync(taxonomyContentItemId);

        builder.Add(S["Edit {0}", taxonomy?.DisplayText], item => item
            .Id("Taxonomy")
            .Action("Edit", "Admin", new RouteValueDictionary(s_contentsRouteValues)
            {
                { "contentItemId", taxonomyContentItemId },
            }));
    }
}
