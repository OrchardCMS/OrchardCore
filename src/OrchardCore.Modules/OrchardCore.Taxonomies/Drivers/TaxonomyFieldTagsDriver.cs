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
            if (String.Equals(context.PartFieldDefinition.DisplayMode(), "TagsDisplayText", StringComparison.OrdinalIgnoreCase))
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
            var editor = context.PartFieldDefinition.Editor();
            if (String.Equals(editor, "Tags", StringComparison.OrdinalIgnoreCase))
            {
                // Return a shape result, with no shape, so it applies update.
                Dynamic(String.Empty);
            }

            return null;
        }

        public override async Task<IDisplayResult> UpdateAsync(TaxonomyField field, IUpdateModel updater, UpdateFieldEditorContext context)
        {
            var editor = context.PartFieldDefinition.Editor();

            if (String.Equals(editor, "Tags", StringComparison.OrdinalIgnoreCase))
            {
                var model = new EditTaxonomyFieldViewModel();

                if (await updater.TryUpdateModelAsync(model, Prefix))
                {
                    var settings = context.PartFieldDefinition.GetSettings<TaxonomyFieldSettings>();

                    var termContentItemIds = model.TermEntries.Where(x => x.Selected).Select(x => x.ContentItemId).ToArray();

                    // TODO unused removed.
                    //if (settings.Unique && !String.IsNullOrEmpty(model.UniqueValue))
                    //{
                    //    termContentItemIds = new[] { model.UniqueValue };
                    //}

                    var taxonomy = await _contentManager.GetAsync(settings.TaxonomyContentItemId, VersionOptions.Latest);

                    if (taxonomy == null)
                    {
                        return null;
                    }

                    var terms = new List<ContentItem>();

                    foreach (var termContentItemId in termContentItemIds)
                    {
                        var term = TaxonomyOrchardHelperExtensions.FindTerm(taxonomy.Content.TaxonomyPart.Terms as JArray, termContentItemId);
                        terms.Add(term);
                    }

                    field.TagTermDisplayTexts = terms.Select(t => t.DisplayText).ToArray();
                }
            }

            return Edit(field, context);
        }
    }
}


