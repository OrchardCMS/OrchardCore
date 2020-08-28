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
        Task<IEnumerable<ContentItem>> QueryCategorizedItemsAsync(TermPart termPart, bool enableOrdering, PagerSlim pager);

        Task InitializeCategorizedItemsOrderAsync(string TaxonomyContentItemId);

        Task SyncTaxonomyFieldProperties(TaxonomyField field);

        void SaveCategorizedItemsOrder(IEnumerable<ContentItem> categorizedItems, string taxonomyContentItemId, int topOrderValue);

        int GetTaxonomyTermOrder(ContentItem categorizedItem, string termContentItemId);
    }
}
