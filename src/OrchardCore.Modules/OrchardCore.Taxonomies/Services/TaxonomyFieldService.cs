using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Records;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.Navigation;
using OrchardCore.Taxonomies.Indexing;
using OrchardCore.Taxonomies.Fields;
using OrchardCore.Taxonomies.Models;
using YesSql;
using YesSql.Services;
using Castle.DynamicProxy.Generators.Emitters.SimpleAST;
using OrchardCore.Mvc.Utilities;
using Microsoft.Extensions.DependencyInjection;
using System.Text.RegularExpressions;
using Castle.Core.Internal;
using GraphQL.Types;
using GraphQL.Instrumentation;
using Org.BouncyCastle.Math.EC;
using GraphQL.Validation;
using OrchardCore.Modules;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using OrchardCore.ContentManagement.Metadata.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace OrchardCore.Taxonomies.Services
{
    public class TaxonomyFieldService : ITaxonomyFieldService
    {
        private readonly ISession _session;
        private readonly IContentManager _contentManager;
        private readonly IServiceProvider _serviceProvider;
        private IContentDefinitionManager _contentDefinitionManager;

        public TaxonomyFieldService(
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
            IQuery<ContentItem> query = null;
            if (pager.Before != null)
            {
                if (enableOrdering)
                {
                    var beforeValue = int.Parse(pager.Before);
                    query = _session.Query<ContentItem>()
                        .With<TaxonomyIndex>(x => x.TermContentItemId == termPart.ContentItem.ContentItemId && x.Order > beforeValue)
                        .OrderBy(x => x.Order)
                        .With<ContentItemIndex>(x => x.Published)
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

                var containedItems = await query.ListAsync();

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

                return await _contentManager.LoadAsync(containedItems);
            }
            else if (pager.After != null)
            {
                if (enableOrdering)
                {
                    var afterValue = int.Parse(pager.After);
                    query = _session.Query<ContentItem>()
                        .With<TaxonomyIndex>(x => x.TermContentItemId == termPart.ContentItem.ContentItemId && x.Order < afterValue)
                        .OrderByDescending(x => x.Order)
                        .With<ContentItemIndex>(x => x.Published)
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
                var containedItems = await query.ListAsync();

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

                return await _contentManager.LoadAsync(containedItems);
            }
            else
            {
                if (enableOrdering)
                {
                    query = _session.Query<ContentItem>()
                        .With<TaxonomyIndex>(x => x.TermContentItemId == termPart.ContentItem.ContentItemId)
                        .OrderByDescending(x => x.Order)
                        .With<ContentItemIndex>(x => x.Published)
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

                var containedItems = await query.ListAsync();

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

                return await _contentManager.LoadAsync(containedItems);
            }
        }

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
                    .With<ContentItemIndex>(c => c.Latest)
                    .ThenByDescending(c => c.CreatedUtc)
                    .ListAsync();

                await RegisterCategorizedItemsOrder(categorizedItems, term.ContentItemId);
            }
        }

        public async Task UpdateTaxonomyFieldOrderAsync(TaxonomyField field)
        {
            var removedTerms = field.TermContentItemOrder.Where(o => !field.TermContentItemIds.Contains(o.Key)).Select(o => o.Key).ToList();
            foreach (var removedTerm in removedTerms)
            {
                // Remove the order information because the content item in no longer categorized with this term
                field.TermContentItemOrder.Remove(removedTerm);
            }

            foreach (var term in field.TermContentItemIds)
            {
                if (!field.TermContentItemOrder.ContainsKey(term))
                {
                    // When categorized with a new term, when ordering is enabled, the content item goes into the first (higher order) position
                    field.TermContentItemOrder.Add(term, await GetNextOrderNumberAsync(term));
                }
            }
        }

        public async Task<IEnumerable<ContentItem>> QueryUnpagedOrderedCategorizedItemsAsync(string termContentItemId)
        {
            var contentItems = await _session.Query<ContentItem>()
                .With<TaxonomyIndex>(x => x.TermContentItemId == termContentItemId)
                .OrderByDescending(x => x.Order)
                .With<ContentItemIndex>(x => x.Latest)
                .ListAsync();

            return await _contentManager.LoadAsync(contentItems);
        }

        public async Task RegisterCategorizedItemsOrder(IEnumerable<ContentItem> categorizedItems, string termContentItemId)
        {
            var termContentItemOrder = categorizedItems.Count();

            // The list of content items is already ordered (first to last), all we do here is register that order on the appropriate field for each content item
            foreach (var categorizedItem in categorizedItems)
            {
                RegisterCategorizedItemOrder(categorizedItem, termContentItemId, termContentItemOrder);

                // Keep the published and draft orders the same to avoid confusion in the admin list.
                if (!categorizedItem.IsPublished())
                {
                    var publishedCategorizedItem = await _contentManager.GetAsync(categorizedItem.ContentItemId, VersionOptions.Published);
                    if (publishedCategorizedItem != null)
                    {
                        RegisterCategorizedItemOrder(publishedCategorizedItem, termContentItemId, termContentItemOrder);
                    }
                }

                --termContentItemOrder;
            }

            return;
        }

        private void RegisterCategorizedItemOrder(ContentItem categorizedItem, string termContentItemId, int termContentItemOrder)
        {
            (var field, var fieldDefinition) = GetTaxonomyFielForTerm(categorizedItem: categorizedItem, termContentItemId: termContentItemId);

            if (field != null)
            {
                var currentOrder = field.TermContentItemOrder.GetValueOrDefault(termContentItemId, 0);

                if (termContentItemOrder != currentOrder)
                {
                    field.TermContentItemOrder[termContentItemId] = termContentItemOrder;

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

        // Given a content item, this method returns the taxonomy field for a specific taxonomy and/or the one that includes (categorizes) the content item in a specific taxonomy term
        private (TaxonomyField field, ContentPartFieldDefinition fieldDefinition) GetTaxonomyFielForTerm(ContentItem categorizedItem, string taxonomyContentItemId = null, string termContentItemId = null)
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

        private int GetTaxonomyTermOrder(ContentItem categorizedItem, string termContentItemId)
        {
            (var field, var fieldDefinition) = GetTaxonomyFielForTerm(categorizedItem: categorizedItem, termContentItemId: termContentItemId);
            return field.TermContentItemOrder[termContentItemId];
        }
    }
}


