using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Taxonomies.Fields;
using OrchardCore.Taxonomies.Models;
using OrchardCore.Taxonomies.Settings;
using OrchardCore.Taxonomies.ViewModels;

namespace OrchardCore.Taxonomies.Drivers
{
    public class TaxonomyFieldTagsDisplayDriver : ContentFieldDisplayDriver<TaxonomyField>
    {
        private readonly IContentManager _contentManager;

        public TaxonomyFieldTagsDisplayDriver(IContentManager contentManager)
        {
            _contentManager = contentManager;
        }

        public override IDisplayResult Display(TaxonomyField field, BuildFieldDisplayContext context)
        {
            if (String.Equals(context.PartFieldDefinition.DisplayMode(), "Tags", StringComparison.OrdinalIgnoreCase))
            {
                return Initialize<DisplayTaxonomyFieldTagsViewModel>(GetDisplayShapeType(context), model =>
                {
                    model.Field = field;
                    model.Part = context.ContentPart;
                    model.PartFieldDefinition = context.PartFieldDefinition;
                })
                .Location("Content")
                .Location("SummaryAdmin", "");
            }

            return null;
        }

        public override IDisplayResult Edit(TaxonomyField field, BuildFieldEditorContext context)
        {
            if (String.Equals(context.PartFieldDefinition.Editor(), "Tags", StringComparison.OrdinalIgnoreCase))
            {
                return Initialize<EditTaxonomyFieldViewModel>(GetEditorShapeType(context), async model =>
                {
                    var settings = context.PartFieldDefinition.GetSettings<TaxonomyFieldSettings>();
                    model.Taxonomy = await _contentManager.GetAsync(settings.TaxonomyContentItemId, VersionOptions.Latest);

                    if (model.Taxonomy != null)
                    {
                        var termEntries = new List<TermEntry>();
                        TaxonomyFieldDriverHelper.PopulateTermEntries(termEntries, field, model.Taxonomy.As<TaxonomyPart>().Terms, 0);

                        model.TermEntries = termEntries;
                    }

                    model.Field = field;
                    model.Part = context.ContentPart;
                    model.PartFieldDefinition = context.PartFieldDefinition;
                });
            }

            return null;
        }

        public override async Task<IDisplayResult> UpdateAsync(TaxonomyField field, IUpdateModel updater, UpdateFieldEditorContext context)
        {
            if (String.Equals(context.PartFieldDefinition.Editor(), "Tags", StringComparison.OrdinalIgnoreCase))
            {
                var model = new EditTaxonomyFieldViewModel();

                if (await updater.TryUpdateModelAsync(model, Prefix))
                {
                    var settings = context.PartFieldDefinition.GetSettings<TaxonomyFieldSettings>();

                    field.TaxonomyContentItemId = settings.TaxonomyContentItemId;
                    field.TermContentItemIds = model.TermEntries.Where(x => x.Selected).Select(x => x.ContentItemId).ToArray();

                    // Update display text for tags.
                    var taxonomy = await _contentManager.GetAsync(settings.TaxonomyContentItemId, VersionOptions.Latest);

                    if (taxonomy == null)
                    {
                        return null;
                    }

                    var terms = new List<ContentItem>();

                    foreach (var termContentItemId in field.TermContentItemIds)
                    {
                        var term = TaxonomyOrchardHelperExtensions.FindTerm(taxonomy.Content.TaxonomyPart.Terms as JArray, termContentItemId);
                        terms.Add(term);
                    }

                    field.Content.Tags = JArray.FromObject(terms.Select(t => t.DisplayText).ToArray());
                }
            }

            return Edit(field, context);
        }
    }
}
