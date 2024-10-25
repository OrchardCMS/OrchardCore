using System.Text.Json.Nodes;
using Cysharp.Text;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Contents.ViewModels;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Entities;
using OrchardCore.Settings;
using OrchardCore.Taxonomies.Fields;
using OrchardCore.Taxonomies.Models;
using OrchardCore.Taxonomies.Settings;
using OrchardCore.Taxonomies.ViewModels;

namespace OrchardCore.Taxonomies.Drivers;

public sealed class TaxonomyContentsAdminListDisplayDriver : DisplayDriver<ContentOptionsViewModel>
{
    private const string LevelPadding = "\xA0\xA0";

    private readonly ISiteService _siteService;
    private readonly IContentManager _contentManager;
    private readonly IContentDefinitionManager _contentDefinitionManager;

    internal readonly IStringLocalizer S;

    public TaxonomyContentsAdminListDisplayDriver(
        ISiteService siteService,
        IContentManager contentManager,
        IContentDefinitionManager contentDefinitionManager,
        IStringLocalizer<TaxonomyContentsAdminListDisplayDriver> stringLocalizer)
    {
        _siteService = siteService;
        _contentManager = contentManager;
        _contentDefinitionManager = contentDefinitionManager;
        S = stringLocalizer;
    }

    public override async Task<IDisplayResult> EditAsync(ContentOptionsViewModel model, BuildEditorContext context)
    {
        var settings = await _siteService.GetSettingsAsync<TaxonomyContentsAdminListSettings>();

        if (settings.TaxonomyContentItemIds.Length == 0)
        {
            return null;
        }

        var taxonomyContentItemIds = settings.TaxonomyContentItemIds;
        if (!string.IsNullOrEmpty(model.SelectedContentType))
        {
            var contentTypeDefinition = await _contentDefinitionManager.GetTypeDefinitionAsync(model.SelectedContentType);
            var fieldDefinitions = contentTypeDefinition
                .Parts.SelectMany(x => x.PartDefinition.Fields.Where(f => f.FieldDefinition.Name == nameof(TaxonomyField)));
            var fieldTaxonomyContentItemIds = fieldDefinitions.Select(x => x.GetSettings<TaxonomyFieldSettings>().TaxonomyContentItemId);
            taxonomyContentItemIds = taxonomyContentItemIds.Intersect(fieldTaxonomyContentItemIds).ToArray();

            if (taxonomyContentItemIds.Length == 0)
            {
                return null;
            }
        }

        var results = new List<IDisplayResult>();
        var taxonomies = await _contentManager.GetAsync(taxonomyContentItemIds);

        var position = 5;
        foreach (var taxonomy in taxonomies)
        {
            results.Add(
                Initialize<TaxonomyContentsAdminFilterViewModel>("ContentsAdminListTaxonomyFilter", m =>
                {
                    using var sb = ZString.CreateStringBuilder();
                    var termEntries = new List<FilterTermEntry>();
                    PopulateTermEntries(termEntries, taxonomy.As<TaxonomyPart>().Terms, 0);
                    var terms = new List<SelectListItem>
                        {
                            new()
                            {
                                Text = S["Clear filter"],
                                Value = string.Empty,
                            },
                            new()
                            {
                                Text = S["Show all"],
                                Value = "Taxonomy:" + taxonomy.ContentItemId,
                            }
                        };

                    foreach (var term in termEntries)
                    {
                        sb.Clear();
                        for (var l = 0; l < term.Level; l++)
                        {
                            sb.Append(LevelPadding);
                        }
                        sb.Append(term.DisplayText);
                        var item = new SelectListItem
                        {
                            Text = sb.ToString(),
                            Value = "Term:" + term.ContentItemId,
                        };
                        terms.Add(item);
                    }

                    m.DisplayText = taxonomy.DisplayText;
                    m.Taxonomies = terms;
                })
                .Location("Actions:40." + position)
                .Prefix("Taxonomy" + taxonomy.ContentItemId)
            );

            position += 5;
        }

        if (results.Count > 0)
        {
            return Combine(results);
        }

        return null;
    }

    public override async Task<IDisplayResult> UpdateAsync(ContentOptionsViewModel model, UpdateEditorContext context)
    {
        var settings = await _siteService.GetSettingsAsync<TaxonomyContentsAdminListSettings>();
        foreach (var contentItemId in settings.TaxonomyContentItemIds)
        {
            var viewModel = new TaxonomyContentsAdminFilterViewModel();
            await context.Updater.TryUpdateModelAsync(viewModel, "Taxonomy" + contentItemId);

            if (!string.IsNullOrEmpty(viewModel.SelectedContentItemId))
            {
                model.RouteValues.TryAdd("Taxonomy" + contentItemId + ".SelectedContentItemId", viewModel.SelectedContentItemId);
            }
        }

        return await EditAsync(model, context);
    }

    private static void PopulateTermEntries(List<FilterTermEntry> termEntries, IEnumerable<ContentItem> contentItems, int level)
    {
        foreach (var contentItem in contentItems)
        {
            var children = Array.Empty<ContentItem>();

            if (((JsonObject)contentItem.Content)["Terms"] is JsonArray termsArray)
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
