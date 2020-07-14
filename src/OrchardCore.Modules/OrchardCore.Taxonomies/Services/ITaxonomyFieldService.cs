using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.ContentManagement;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.Navigation;
using OrchardCore.Taxonomies.Fields;
using OrchardCore.Taxonomies.Indexing;
using OrchardCore.Taxonomies.Models;
using YesSql;

namespace OrchardCore.Taxonomies.Services
{
    public interface ITaxonomyFieldService
    {
        Task InitializeCategorizedItemsOrderAsync(string TaxonomyContentItemId);

        Task<IEnumerable<ContentItem>> QueryUnpagedOrderedCategorizedItemsAsync(string termContentItemId);

        Task UpdateTaxonomyFieldOrderAsync(TaxonomyField field);

        Task RegisterCategorizedItemsOrder(IEnumerable<ContentItem> categorizedItems, string taxonomyContentItemId);

        Task<IEnumerable<ContentItem>> QueryCategorizedItemsAsync(TermPart termPart, bool enableOrdering, PagerSlim pager);
    }
}
