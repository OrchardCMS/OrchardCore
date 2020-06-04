using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Shapes;
using OrchardCore.DisplayManagement.Zones;
using OrchardCore.Entities;
using OrchardCore.Modules;
using OrchardCore.Settings;
using OrchardCore.Taxonomies.Models;
using OrchardCore.Taxonomies.Settings;
using OrchardCore.Taxonomies.ViewModels;

namespace OrchardCore.Taxonomies
{
    [Feature("OrchardCore.Taxonomies.ContentsAdminList")]
    public class TaxonomyContentsAdminListShapes : IShapeTableProvider
    {
        private const string LevelPadding = "\xA0\xA0";

        public void Discover(ShapeTableBuilder builder)
        {
            builder.Describe("ContentsAdminListHeader")
                .OnCreated(async context =>
                {
                    var shape = (dynamic)context.Shape;

                    var S = context.ServiceProvider.GetRequiredService<IStringLocalizer<TaxonomyContentsAdminListShapes>>();
                    var siteService = context.ServiceProvider.GetRequiredService<ISiteService>();
                    var settings = (await siteService.GetSiteSettingsAsync()).As<TaxonomyContentsAdminListSettings>();

                    var contentManager = context.ServiceProvider.GetRequiredService<IContentManager>();
                    var taxonomies = await contentManager.GetAsync(settings.TaxonomyContentItemIds);

                    var position = 5;
                    foreach (var taxonomy in taxonomies)
                    {
                        var termEntries = new List<FilterTermEntry>();
                        PopulateTermEntries(termEntries, taxonomy.As<TaxonomyPart>().Terms, 0);
                        var terms = new List<SelectListItem>
                            {
                                new SelectListItem() { Text = S["Clear filter"], Value = ""  },
                                new SelectListItem() { Text = S["Show all"], Value = "Taxonomy:" + taxonomy.ContentItemId }
                            };

                        foreach (var term in termEntries)
                        {
                            using var sb = StringBuilderPool.GetInstance();
                            for (var l = 0; l < term.Level; l++)
                            {
                                sb.Builder.Insert(0, LevelPadding);
                            }
                            sb.Builder.Append(term.DisplayText);
                            var item = new SelectListItem() { Text = sb.Builder.ToString(), Value = "Term:" + term.ContentItemId };
                            terms.Add(item);
                        }
                        var taxonomyShape = await context.ShapeFactory.CreateAsync<TaxonomyContentAdminFilterViewModel>("ContentsAdminList__TaxonomyFilter", m =>
                        {
                            m.DisplayText = taxonomy.DisplayText;
                            m.Taxonomies = terms;
                        });

                        taxonomyShape.Metadata.Prefix = "Taxonomy" + taxonomy.ContentItemId;

                        var zone = shape.Zones["Actions"];
                        if (zone is ZoneOnDemand zoneOnDemand)
                        {
                            await zoneOnDemand.AddAsync(taxonomyShape, ":40." + position.ToString());
                        }
                        else if (zone is Shape zoneShape)
                        {
                            zoneShape.Add(taxonomyShape, ":40." + position.ToString());
                        }

                        position += 5;
                    }
                });
        }

        private static void PopulateTermEntries(List<FilterTermEntry> termEntries, IEnumerable<ContentItem> contentItems, int level)
        {
            foreach (var contentItem in contentItems)
            {
                var children = Array.Empty<ContentItem>();

                if (contentItem.Content.Terms is JArray termsArray)
                {
                    children = termsArray.ToObject<ContentItem[]>();
                }

                var termEntry = new FilterTermEntry
                {
                    DisplayText = contentItem.DisplayText,
                    ContentItemId = contentItem.ContentItemId,
                    Level = level
                };

                termEntries.Add(termEntry);

                if (children.Length > 0)
                {
                    PopulateTermEntries(termEntries, children, level + 1);
                }
            }
        }
    }

    public class FilterTermEntry
    {
        public string DisplayText { get; set; }
        public string ContentItemId { get; set; }

        public int Level { get; set; }
    }
}
