using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Taxonomies.Fields;
using OrchardCore.Taxonomies.Models;
using OrchardCore.Taxonomies.Settings;
using OrchardCore.Taxonomies.ViewModels;

namespace OrchardCore.Taxonomies.Drivers
{
    public class TaxonomyFieldDisplayDriver : ContentFieldDisplayDriver<TaxonomyField>
    {
        private readonly IContentManager _contentManager;

        public TaxonomyFieldDisplayDriver(
            IContentManager contentManager,
            IStringLocalizer<TaxonomyFieldDisplayDriver> s)
        {
            _contentManager = contentManager;
            S = s;
        }

        public IStringLocalizer S { get; }

        public override IDisplayResult Display(TaxonomyField field, BuildFieldDisplayContext context)
        {
            return Initialize<DisplayTaxonomyFieldViewModel>("TaxonomyField", model =>
            {
                model.Field = field;
                model.Part = context.ContentPart;
                model.PartFieldDefinition = context.PartFieldDefinition;
            })
            .Location("Content")
            .Location("SummaryAdmin", "");
            ;
        }

        public override IDisplayResult Edit(TaxonomyField field, BuildFieldEditorContext context)
        {
            return Initialize<EditTaxonomyFieldViewModel>("TaxonomyField_Edit", async model =>
            {
                var settings = context.PartFieldDefinition.Settings.ToObject<TaxonomyFieldSettings>();
                model.Taxonomy = await _contentManager.GetAsync(settings.TaxonomyContentItemId, VersionOptions.Latest);

                if (model.Taxonomy != null )
                {
                    var termEntries = new List<TermEntry>();
                    PopulateTermEntries(termEntries, field, model.Taxonomy.As<TaxonomyPart>().Terms, 0);

                    model.TermEntries = termEntries;
                    model.UniqueValue = termEntries.FirstOrDefault(x => x.Selected)?.ContentItemId;
                }

                model.Field = field;
                model.Part = context.ContentPart;
                model.PartFieldDefinition = context.PartFieldDefinition;
            });
        }

        public override async Task<IDisplayResult> UpdateAsync(TaxonomyField field, IUpdateModel updater, UpdateFieldEditorContext context)
        {
            var model = new EditTaxonomyFieldViewModel();

            if (await updater.TryUpdateModelAsync(model, Prefix))
            {
                var settings = context.PartFieldDefinition.Settings.ToObject<TaxonomyFieldSettings>();

                field.TaxonomyContentItemId = settings.TaxonomyContentItemId;
                field.TermContentItemIds = model.TermEntries.Where(x => x.Selected).Select(x => x.ContentItemId).ToArray();

                if (settings.Unique)
                {
                    field.TermContentItemIds = new[] { model.UniqueValue };
                }

                if (settings.Required && field.TermContentItemIds.Length == 0)
                {
                    updater.ModelState.AddModelError(
                        nameof(EditTaxonomyFieldViewModel.TermEntries),
                        S["A value is required for '{0}'", context.PartFieldDefinition.Name]);
                }
            }

            return Edit(field, context);
        }

        /// <summary>
        /// Populates a list of <see cref="TermEntry"/> with the hierarchy of terms.
        /// The list is ordered so that roots appear right before their child terms.
        private void PopulateTermEntries(List<TermEntry> termEntries, TaxonomyField field, IEnumerable<ContentItem> contentItems, int level)
        {
            foreach(var contentItem in contentItems)
            {
                var children = Array.Empty<ContentItem>();

                if (contentItem.Content.Terms is JArray termsArray)
                {
                    children = termsArray.ToObject<ContentItem[]>();
                }

                var termEntry = new TermEntry
                {
                    Term = contentItem,
                    ContentItemId = contentItem.ContentItemId,
                    Selected = field.TermContentItemIds.Contains(contentItem.ContentItemId),
                    Level = level,
                    IsLeaf = children.Length == 0
                };

                termEntries.Add(termEntry);

                if (children.Length > 0)
                {
                    PopulateTermEntries(termEntries, field, children, level + 1);
                }
            }
        }
    }
}
