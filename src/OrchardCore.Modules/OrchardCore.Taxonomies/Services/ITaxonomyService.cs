using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.Navigation;
using OrchardCore.Taxonomies.Fields;
using OrchardCore.Taxonomies.Models;

namespace OrchardCore.Taxonomies.Services
{
    public interface ITaxonomyService
    {
        Task<IEnumerable<ContentItem>> QueryCategorizedItemsAsync(TermPart termPart, PagerSlim pager, bool enableOrdering, bool published);

        ContentItem FindTerm(ContentItem taxonomy, string termContentItemId);

        JObject FindTermObject(JObject contentItem, string taxonomyItemId);

        bool FindTermHierarchy(ContentItem taxonomy, string termContentItemId, List<ContentItem> terms);

        List<ContentItem> FindTermSiblings(ContentItem taxonomy, string termContentItemId);

        Task InitializeCategorizedItemsOrderAsync(string TaxonomyContentItemId);

        Task EnsureUniqueOrderValues(TaxonomyField field);

        Task SyncTaxonomyFieldProperties(TaxonomyField field);

        Task SaveCategorizedItemsOrder(IEnumerable<ContentItem> categorizedItems, string taxonomyContentItemId, int topOrderValue);

        int GetTaxonomyTermOrder(ContentItem categorizedItem, string termContentItemId);

        (TaxonomyField field, ContentPartFieldDefinition fieldDefinition) GetTaxonomyField(ContentItem categorizedItem, string taxonomyContentItemId = null, string termContentItemId = null);

    }
}
