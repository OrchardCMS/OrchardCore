using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Navigation;
using OrchardCore.Taxonomies.Indexing;
using OrchardCore.Taxonomies.Fields;
using OrchardCore.Taxonomies.Models;
using YesSql;
using OrchardCore.Mvc.Utilities;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement.Metadata.Models;

namespace OrchardCore.Taxonomies.Services
{
    public class TaxonomyService : ITaxonomyService
    {
        private readonly ISession _session;
        private readonly IContentManager _contentManager;
        private readonly IServiceProvider _serviceProvider;
        private IContentDefinitionManager _contentDefinitionManager;

        public TaxonomyService(
            ISession session,
            IContentManager contentManager,
            IServiceProvider serviceProvider
        )
        {
            _session = session;
            _contentManager = contentManager;
            _serviceProvider = serviceProvider;
        }

        public async Task<IEnumerable<ContentItem>> QueryCategorizedItemsAsync(TermPart termPart, PagerSlim pager, bool enableOrdering, bool published)
        {
            IEnumerable<ContentItem> categorizedItems;

            IQueryIndex<TaxonomyIndex> query = _session.QueryIndex<TaxonomyIndex>(x => x.TermContentItemId == termPart.ContentItem.ContentItemId);

            if (pager.Before != null)
            {
                if (enableOrdering)
                {
                    var beforeValue = int.Parse(pager.Before);
                    query = query.Where(x => x.Order > beforeValue);
                }
                else
                {
                    var beforeValue = new DateTime(long.Parse(pager.Before));
                    query = query = query.Where(x => x.CreatedUtc > beforeValue);
                }
            }
            else if (pager.After != null)
            {
                if (enableOrdering)
                {
                    var afterValue = int.Parse(pager.After);
                    query = query = query.Where(x => x.Order < afterValue);
                }
                else
                {
                    var afterValue = new DateTime(long.Parse(pager.After));
                    query = query = query.Where(x => x.CreatedUtc < afterValue);
                }
            }

            if (pager.Before != null)
            {
                if (enableOrdering)
                {
                    query = query.OrderBy(x => x.Order);
                }
                else
                {
                    query = query.OrderBy(x => x.CreatedUtc);
                }
            }
            else
            {
                if (enableOrdering)
                {
                    query = query.OrderByDescending(x => x.Order);
                }
                else
                {
                    query.OrderByDescending(x => x.CreatedUtc);
                }
            }

            if (published)
            {
                query = query.Where(x => x.Published);
            }
            else
            {
                query = query.Where(x => x.Latest);
            }

            query = query.Take(pager.PageSize + 1);

            categorizedItems = await GetCategorizedItemsAsync(query);

            if (categorizedItems.Count() == 0)
            {
                return categorizedItems;
            }

            if (pager.Before != null)
            {
                categorizedItems = categorizedItems.Reverse();

                // There is always an After as we clicked on Before
                pager.Before = null;

                pager.After = enableOrdering ?
                    GetTaxonomyTermOrder(categorizedItems.Last(), termPart.ContentItem.ContentItemId).ToString() :
                    categorizedItems.Last().CreatedUtc.Value.Ticks.ToString();

                if (categorizedItems.Count() == pager.PageSize + 1)
                {
                    categorizedItems = categorizedItems.Skip(1);
                    if (enableOrdering)
                    {
                        pager.Before = GetTaxonomyTermOrder(categorizedItems.First(), termPart.ContentItem.ContentItemId).ToString();
                    }
                    else
                    {
                        pager.Before = categorizedItems.First().CreatedUtc.Value.Ticks.ToString();
                    }
                }
            }
            else if (pager.After != null)
            {
                // There is always a Before page as we clicked on After
                if (enableOrdering)
                {
                    pager.Before = GetTaxonomyTermOrder(categorizedItems.First(), termPart.ContentItem.ContentItemId).ToString();
                }
                else
                {
                    pager.Before = categorizedItems.First().CreatedUtc.Value.Ticks.ToString();
                }
                pager.After = null;

                if (categorizedItems.Count() == pager.PageSize + 1)
                {
                    categorizedItems = categorizedItems.Take(pager.PageSize);
                    if (enableOrdering)
                    {
                        pager.After = GetTaxonomyTermOrder(categorizedItems.Last(), termPart.ContentItem.ContentItemId).ToString();
                    }
                    else
                    {
                        pager.After = categorizedItems.Last().CreatedUtc.Value.Ticks.ToString();
                    }
                }
            }
            else
            {
                pager.Before = null;
                pager.After = null;

                if (categorizedItems.Count() == pager.PageSize + 1)
                {
                    if (pager.PageSize > 0)
                    {
                        categorizedItems = categorizedItems.Take(pager.PageSize);
                    }
                    if (enableOrdering)
                    {
                        pager.After = GetTaxonomyTermOrder(categorizedItems.Last(), termPart.ContentItem.ContentItemId).ToString();
                    }
                    else
                    {
                        pager.After = categorizedItems.Last().CreatedUtc.Value.Ticks.ToString();
                    }
                }
            }

            return categorizedItems;
        }

        // Get all the content and contained content items
        private async Task<IEnumerable<ContentItem>> GetCategorizedItemsAsync(IQueryIndex<TaxonomyIndex> query)
        {
            var taxonomyIndexes = await query.ListAsync();

            // Get all the "containers" of the categorized content items
            var ids = taxonomyIndexes.Select(x => x.ContentItemId);
            var items = await _contentManager.GetAsync(ids, latest: true);

            List<ContentItem> containedItems = new List<ContentItem>();
            foreach (var taxonomyIndex in taxonomyIndexes)
            {
                var categorizedItem = items.FirstOrDefault(x => x.ContentItemId == taxonomyIndex.ContentItemId);

                // It represents a contained content item
                if (!string.IsNullOrEmpty(taxonomyIndex.JsonPath))
                {
                    var latest = categorizedItem.Latest;
                    var published = categorizedItem.Published;

                    // Get the Term from the Taxonomy
                    var root = categorizedItem.Content as JObject;
                    categorizedItem = root.SelectToken(taxonomyIndex.JsonPath)?.ToObject<ContentItem>();

                    // When Ordering content items, we will need to know this is a "Term" content item
                    categorizedItem.Weld<TermPart>();
                    categorizedItem.Alter<TermPart>(t => t.TaxonomyContentItemId = taxonomyIndex.ContentItemId);

                    // For when Ordering, for aesthetic reasons
                    categorizedItem.Latest = latest;
                    categorizedItem.Published = published;
                }

                // add the container or contained/term content item to the list
                containedItems.Add(categorizedItem);
            }

            return containedItems;
        }

        public ContentItem FindTerm(ContentItem taxonomy, string termContentItemId)
        {
            return FindTerm(taxonomy.Content.TaxonomyPart.Terms as JArray, termContentItemId);
        }

        public JObject FindTermObject(JObject contentItem, string taxonomyItemId)
        {
            if (contentItem["ContentItemId"]?.Value<string>() == taxonomyItemId)
            {
                return contentItem;
            }

            if (contentItem.GetValue("Terms") == null)
            {
                return null;
            }

            var taxonomyItems = (JArray)contentItem["Terms"];

            JObject result;

            foreach (JObject taxonomyItem in taxonomyItems)
            {
                // Search in inner taxonomy items
                result = FindTermObject(taxonomyItem, taxonomyItemId);

                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }

        public bool FindTermHierarchy(ContentItem taxonomy, string termContentItemId, List<ContentItem> terms)
        {
            return FindTermHierarchy(taxonomy.Content.TaxonomyPart.Terms as JArray, termContentItemId, terms);
        }

        public List<ContentItem> FindTermSiblings(ContentItem taxonomy, string termContentItemId)
        {
            return FindTermSiblings(taxonomy.Content.TaxonomyPart.Terms as JArray, termContentItemId);
        }


        // Given a content item, this method returs the taxonomy field for a specific taxonomy and/or the one that includes (categorizes) the content item in a specific taxonomy term.
        public (TaxonomyField field, ContentPartFieldDefinition fieldDefinition) GetTaxonomyField(ContentItem categorizedItem, string taxonomyContentItemId = null, string termContentItemId = null)
        {
            _contentDefinitionManager = _contentDefinitionManager ?? _serviceProvider.GetRequiredService<IContentDefinitionManager>();

            var fieldDefinitions = _contentDefinitionManager
                    .GetTypeDefinition(categorizedItem.ContentType)
                    .Parts.SelectMany(x => x.PartDefinition.Fields.Where(f => f.FieldDefinition.Name == nameof(TaxonomyField)))
                    .ToArray();

            foreach (var fieldDefinition in fieldDefinitions)
            {
                var jPart = (JObject)categorizedItem.Content[fieldDefinition.PartDefinition.Name];
                if (jPart == null)
                {
                    continue;
                }

                var jField = (JObject)jPart[fieldDefinition.Name];
                if (jField == null)
                {
                    continue;
                }

                var field = jField.ToObject<TaxonomyField>();

                if ((taxonomyContentItemId == null || field.TaxonomyContentItemId == taxonomyContentItemId) && (termContentItemId == null || field.TermContentItemIds.Contains(termContentItemId)))
                {
                    return (field, fieldDefinition);
                }
            }

            return (null, null);
        }

        private ContentItem FindTerm(JArray termsArray, string termContentItemId)
        {
            foreach (JObject term in termsArray)
            {
                var contentItemId = term.GetValue("ContentItemId").ToString();

                if (contentItemId == termContentItemId)
                {
                    return term.ToObject<ContentItem>();
                }

                if (term.GetValue("Terms") is JArray children)
                {
                    var found = FindTerm(children, termContentItemId);

                    if (found != null)
                    {
                        return found;
                    }
                }
            }

            return null;
        }

        private bool FindTermHierarchy(JArray termsArray, string termContentItemId, List<ContentItem> terms)
        {
            foreach (JObject term in termsArray)
            {
                var contentItemId = term.GetValue("ContentItemId").ToString();

                if (contentItemId == termContentItemId)
                {
                    terms.Add(term.ToObject<ContentItem>());

                    return true;
                }

                if (term.GetValue("Terms") is JArray children)
                {
                    var found = FindTermHierarchy(children, termContentItemId, terms);

                    if (found)
                    {
                        terms.Add(term.ToObject<ContentItem>());

                        return true;
                    }
                }
            }

            return false;
        }

        private List<ContentItem> FindTermSiblings(JArray termsArray, string termContentItemId)
        {
            foreach (JObject term in termsArray)
            {
                var contentItemId = term.GetValue("ContentItemId").ToString();

                // when we find the term, we add all it's brothers
                if (contentItemId == termContentItemId)
                {
                    var terms = new List<ContentItem>();
                    foreach (var sibling in termsArray)
                    {
                        terms.Add(sibling.ToObject<ContentItem>());
                    }

                    return terms;
                }
                // if we don't find the term, we dig deeper
                else if (term.GetValue("Terms") is JArray children)
                {
                    var siblings = FindTermSiblings(children, termContentItemId);
                    if (siblings != null)
                    {
                        return siblings;
                    }
                }
            }

            return null;
        }


        #region Categorized Content Ordering

        public async Task InitializeCategorizedItemsOrderAsync(string taxonomyContentItemId)
        {
            var taxonomy = await _contentManager.GetAsync(taxonomyContentItemId);
            if (taxonomy == null)
            {
                return;
            }

            foreach (var term in taxonomy.As<TaxonomyPart>().Terms)
            {
                var query = _session.QueryIndex<TaxonomyIndex>(
                    t => t.TermContentItemId == term.ContentItemId)
                    .OrderByDescending(t => t.Order)
                    .ThenByDescending(t => t.CreatedUtc);

                var categorizedItems = await GetCategorizedItemsAsync(query);

                await SaveCategorizedItemsOrder(categorizedItems, term.ContentItemId, 1);
            }
        }


        // Publishing a Draft, Cloning, Restoring from the Audit Trail (etc.?) might produce items with already existing Order values.
        // Here we make sure that, if that happens, the order values are adjusted and the problem is solved.
        public async Task EnsureUniqueOrderValues(TaxonomyField field)
        {
            foreach (var term in field.TermContentItemOrder)
            {
                // Look for antother content item
                // categorized with the same term and having the same order value
                var itemWithSameTermAndOrder = await _session.QueryIndex<TaxonomyIndex>(t => t.TermContentItemId == term.Key && t.Order == term.Value && t.ContentItemId != field.ContentItem.ContentItemId)
                    .ListAsync();

                if (itemWithSameTermAndOrder.Count() > 0)
                {
                    var query = _session.QueryIndex<TaxonomyIndex>(t => t.TermContentItemId == term.Key && t.Order >= term.Value)
                        .OrderByDescending(t => t.Order)
                        .ThenByDescending(c => c.CreatedUtc);

                    var categorizedItems = await GetCategorizedItemsAsync(query);

                    await SaveCategorizedItemsOrder(categorizedItems, term.Key, term.Value + 1); // the +1 creates a gap on the order sequence, for the item we are currently updating
                }
            }
        }

        // Add or remove from TermContentItemOrder elements that were added or removed from TermContentItemIds.
        public async Task SyncTaxonomyFieldProperties(TaxonomyField field)
        {
            var removedTerms = field.TermContentItemOrder.Where(o => !field.TermContentItemIds.Contains(o.Key)).Select(o => o.Key).ToList();
            foreach (var removedTerm in removedTerms)
            {
                // Remove the order information because the content item in no longer categorized with this term.
                field.TermContentItemOrder.Remove(removedTerm);
            }

            foreach (var term in field.TermContentItemIds)
            {
                if (!field.TermContentItemOrder.ContainsKey(term))
                {
                    // When categorized with a new term, the content item goes into the first (higher order) position.
                    field.TermContentItemOrder.Add(term, await GetNextOrderNumberAsync(term));
                }
            }

            // Remove any content or the elements would be merged (removed elements would not be cleared), because JsonMerge.ArrayHandling.Replace doesn't handle dictionaries.
            field.Content.TermContentItemOrder?.RemoveAll();
        }

        // The list of content items is already ordered first (higher order) to last (lower order).
        // But the order values can be wrong because we moved an item. Here we fix that, starting with the lowerOrderValue and ascending from there.
        public async Task SaveCategorizedItemsOrder(IEnumerable<ContentItem> categorizedItems, string termContentItemId, int lowerOrderValue)
        {
            var orderValue = lowerOrderValue;

            // Because _session.Save doesn't guarantee that data was saved when the method returns, the next data read might read old data.
            // We need to save all the changes to taxonomy ietms all at once, in the end.
            var taxonomies = new List<ContentItem>();

            foreach (var categorizedItem in categorizedItems.Reverse())
            {
                await RegisterCategorizedItemOrder(categorizedItem, termContentItemId, orderValue, taxonomies);

                // If this published item also has a draft version, make sure that both versions appear at the same position for this term (when the draft is also categorized with it).
                if (categorizedItem.HasDraft())
                {
                    var draftCategorizedItem = await _contentManager.GetAsync(categorizedItem.ContentItemId, VersionOptions.Draft);
                    if (draftCategorizedItem != null)
                    {
                        await RegisterCategorizedItemOrder(draftCategorizedItem, termContentItemId, orderValue, taxonomies);
                    }
                }

                ++orderValue;
            }

            foreach (var taxonomy in taxonomies)
            {
                _session.Save(taxonomy);
            }

            return;
        }

        public int GetTaxonomyTermOrder(ContentItem categorizedItem, string termContentItemId)
        {
            (var field, var fieldDefinition) = GetTaxonomyField(categorizedItem: categorizedItem, termContentItemId: termContentItemId);
            return field.TermContentItemOrder[termContentItemId];
        }

        private async Task RegisterCategorizedItemOrder(ContentItem categorizedItem, string termContentItemId, int orderValue, List<ContentItem> taxonomies)
        {
            // Find the Field that categorizes this item with the term
            (var field, var fieldDefinition) = GetTaxonomyField(categorizedItem: categorizedItem, termContentItemId: termContentItemId);

            if (field != null)
            {
                var currentOrder = field.TermContentItemOrder.GetValueOrDefault(termContentItemId, 0);

                if (orderValue != currentOrder)
                {
                    field.TermContentItemOrder[termContentItemId] = orderValue;

                    JObject jPart = categorizedItem.Content[fieldDefinition.PartDefinition.Name];
                    jPart[fieldDefinition.Name] = JObject.FromObject(field);
                    categorizedItem.Content[fieldDefinition.PartDefinition.Name] = jPart;

                    // If categorizedItem is a Term himself, we mut modify it inside the Taxonomy it belongs to
                    var termPart = categorizedItem.As<TermPart>();
                    if (termPart != null)
                    {
                        var taxonomy = taxonomies.FirstOrDefault(t => t.ContentItemId == termPart.TaxonomyContentItemId);
                        if (taxonomy == null)
                        {
                            taxonomy = await _contentManager.GetAsync(termPart.TaxonomyContentItemId);
                            taxonomies.Add(taxonomy);
                        }

                        var taxonomyItem = FindTermObject(taxonomy.As<TaxonomyPart>().Content, categorizedItem.ContentItemId);

                        taxonomyItem.Merge(categorizedItem.Content, new JsonMergeSettings
                        {
                            MergeArrayHandling = MergeArrayHandling.Replace,
                            MergeNullValueHandling = MergeNullValueHandling.Merge
                        });
                    }
                    else
                    {
                        _session.Save(categorizedItem);
                    }
                }
            }
        }

        private async Task<int> GetNextOrderNumberAsync(string termContentItemId)
        {
            var indexes = await _session.QueryIndex<TaxonomyIndex>(t => t.TermContentItemId == termContentItemId).ListAsync();
            if (indexes.Any())
            {
                return indexes.Max(t => t.Order) + 1;
            }

            return 1;
        }

        #endregion
    }
}
