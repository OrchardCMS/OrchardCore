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

        public async Task<IEnumerable<ContentItem>> QueryCategorizedItemsAsync(TermPart termPart, bool enableOrdering, PagerSlim pager)
        {
            IEnumerable<ContentItem> containedItems;
            
            IQuery<ContentItem> query = null;
            if (pager.Before != null)
            {
                if (enableOrdering)
                {
                    var beforeValue = int.Parse(pager.Before);
                    query = _session.Query<ContentItem>()
                        .With<TaxonomyIndex>(x => x.TermContentItemId == termPart.ContentItem.ContentItemId && x.Order > beforeValue)
                        .OrderBy(x => x.Order)
                        .Take(pager.PageSize + 1);
                }
                else
                {
                    var beforeValue = new DateTime(long.Parse(pager.Before));
                    query = _session.Query<ContentItem>()
                        .With<TaxonomyIndex>(x => x.TermContentItemId == termPart.ContentItem.ContentItemId)
                        .With<ContentItemIndex>(x => x.Published && x.CreatedUtc > beforeValue)
                        .OrderBy(x => x.CreatedUtc)
                        .Take(pager.PageSize + 1);
                }

                containedItems = await query.ListAsync();

                if (containedItems.Count() == 0)
                {
                    return containedItems;
                }

                containedItems = containedItems.Reverse();

                // There is always an After as we clicked on Before
                pager.Before = null;
                if (enableOrdering)
                {
                    pager.After = GetTaxonomyTermOrder(containedItems.Last(), termPart.ContentItem.ContentItemId).ToString();
                }
                else
                {
                    pager.After = containedItems.Last().CreatedUtc.Value.Ticks.ToString();
                }

                if (containedItems.Count() == pager.PageSize + 1)
                {
                    containedItems = containedItems.Skip(1);
                    if (enableOrdering)
                    {
                        pager.Before = GetTaxonomyTermOrder(containedItems.First(), termPart.ContentItem.ContentItemId).ToString();
                    }
                    else
                    {
                        pager.Before = containedItems.First().CreatedUtc.Value.Ticks.ToString();
                    }
                }
            }
            else if (pager.After != null)
            {
                if (enableOrdering)
                {
                    var afterValue = int.Parse(pager.After);
                    query = _session.Query<ContentItem>()
                        .With<TaxonomyIndex>(x => x.TermContentItemId == termPart.ContentItem.ContentItemId && x.Order < afterValue)
                        .OrderByDescending(x => x.Order)
                        .Take(pager.PageSize + 1);
                }
                else
                {
                    var afterValue = new DateTime(long.Parse(pager.After));
                    query = _session.Query<ContentItem>()
                        .With<TaxonomyIndex>(x => x.TermContentItemId == termPart.ContentItem.ContentItemId)
                        .With<ContentItemIndex>(x => x.Published && x.CreatedUtc < afterValue)
                        .OrderByDescending(x => x.CreatedUtc)
                        .Take(pager.PageSize + 1);
                }
                containedItems = await query.ListAsync();

                if (containedItems.Count() == 0)
                {
                    return containedItems;
                }

                // There is always a Before page as we clicked on After
                if (enableOrdering)
                {
                    pager.Before = GetTaxonomyTermOrder(containedItems.First(), termPart.ContentItem.ContentItemId).ToString();
                }
                else
                {
                    pager.Before = containedItems.First().CreatedUtc.Value.Ticks.ToString();
                }
                pager.After = null;

                if (containedItems.Count() == pager.PageSize + 1)
                {
                    containedItems = containedItems.Take(pager.PageSize);
                    if (enableOrdering)
                    {
                        pager.After = GetTaxonomyTermOrder(containedItems.Last(), termPart.ContentItem.ContentItemId).ToString();
                    }
                    else
                    {
                        pager.After = containedItems.Last().CreatedUtc.Value.Ticks.ToString();
                    }
                }
            }
            else
            {
                if (enableOrdering)
                {
                    query = _session.Query<ContentItem>()
                        .With<TaxonomyIndex>(x => x.TermContentItemId == termPart.ContentItem.ContentItemId)
                        .OrderByDescending(x => x.Order)
                        .Take(pager.PageSize + 1);
                }
                else
                {
                    query = _session.Query<ContentItem>()
                        .With<TaxonomyIndex>(x => x.TermContentItemId == termPart.ContentItem.ContentItemId)
                        .With<ContentItemIndex>(x => x.Published)
                        .OrderByDescending(x => x.CreatedUtc)
                        .Take(pager.PageSize + 1);
                }

                containedItems = await query.ListAsync();

                if (containedItems.Count() == 0)
                {
                    return containedItems;
                }

                pager.Before = null;
                pager.After = null;

                if (containedItems.Count() == pager.PageSize + 1)
                {
                    containedItems = containedItems.Take(pager.PageSize);
                    if (enableOrdering)
                    {
                        pager.After = GetTaxonomyTermOrder(containedItems.Last(), termPart.ContentItem.ContentItemId).ToString();
                    }
                    else
                    {
                        pager.After = containedItems.Last().CreatedUtc.Value.Ticks.ToString();
                    }
                }
            }

            return (await _contentManager.LoadAsync(containedItems));
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
                var categorizedItems = await _session.Query<ContentItem>()
                    .With<TaxonomyIndex>(t => t.TermContentItemId == term.ContentItemId)
                    .OrderByDescending(t => t.Order)
                    .With<ContentItemIndex>(c => c.Published || c.Latest)
                    .ThenByDescending(c => c.CreatedUtc)
                    .ListAsync();

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
                var itemWithSameTermAndOrder = await _session.Query<ContentItem>()
                    // categorized with the same term and having the same order value
                    .With<TaxonomyIndex>(t => t.TermContentItemId == term.Key && t.Order == term.Value && t.ContentItemId != field.ContentItem.ContentItemId)
                    // that is published but is not the same version of the same content item id
                    .With<ContentItemIndex>(c => c.Published)
                    .ListAsync();

                if (itemWithSameTermAndOrder.Count() > 0)
                {
                    var categorizedItems = await _session.Query<ContentItem>()
                        .With<TaxonomyIndex>(t => t.TermContentItemId == term.Key && t.Order >= term.Value)
                        .OrderByDescending(t => t.Order)
                        .With<ContentItemIndex>(c => c.Published)
                        .ThenByDescending(c => c.CreatedUtc)
                        .ListAsync();

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

            foreach (var categorizedItem in categorizedItems.Reverse())
            {
                RegisterCategorizedItemOrder(categorizedItem, termContentItemId, orderValue);

                // If this published item also has a draft version, make sure that both versions appear at the same position for this term (when the draft is also categorized with it).
                if (categorizedItem.HasDraft())
                {
                    var draftCategorizedItem = await _contentManager.GetAsync(categorizedItem.ContentItemId, VersionOptions.Draft);
                    if (draftCategorizedItem != null)
                    {
                        RegisterCategorizedItemOrder(draftCategorizedItem, termContentItemId, orderValue);
                    }
                }

                ++orderValue;
            }

            return;
        }

        public int GetTaxonomyTermOrder(ContentItem categorizedItem, string termContentItemId)
        {
            (var field, var fieldDefinition) = GetTaxonomyField(categorizedItem: categorizedItem, termContentItemId: termContentItemId);
            return field.TermContentItemOrder[termContentItemId];
        }

        private void RegisterCategorizedItemOrder(ContentItem categorizedItem, string termContentItemId, int orderValue)
        {
            (var field, var fieldDefinition) = GetTaxonomyField(categorizedItem: categorizedItem, termContentItemId: termContentItemId);

            if (field != null)
            {
                var currentOrder = field.TermContentItemOrder.GetValueOrDefault(termContentItemId, 0);

                if (orderValue != currentOrder)
                {
                    field.TermContentItemOrder[termContentItemId] = orderValue;

                    var jPart = (JObject)categorizedItem.Content[fieldDefinition.PartDefinition.Name];
                    jPart[fieldDefinition.Name] = JObject.FromObject(field);
                    categorizedItem.Content[fieldDefinition.PartDefinition.Name] = jPart;

                    _session.Save(categorizedItem);
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
