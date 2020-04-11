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
    public class TaxonomyFieldTagsDisplayDriver : ContentFieldDisplayDriver<TaxonomyField>
    {
        private readonly IContentManager _contentManager;
        private readonly IStringLocalizer S;

        public TaxonomyFieldTagsDisplayDriver(
            IContentManager contentManager,
            IStringLocalizer<TaxonomyFieldDisplayDriver> s)
        {
            _contentManager = contentManager;
            S = s;
        }

        public override IDisplayResult Display(TaxonomyField field, BuildFieldDisplayContext context)
        {
            return Initialize<DisplayTaxonomyFieldTagsViewModel>(GetDisplayShapeType(context), model =>
            {
                model.Field = field;
                model.Part = context.ContentPart;
                model.PartFieldDefinition = context.PartFieldDefinition;
            })
            .Location("Detail", "Content")
            .Location("Summary", "Content");
        }

        public override IDisplayResult Edit(TaxonomyField field, BuildFieldEditorContext context)
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

        public override async Task<IDisplayResult> UpdateAsync(TaxonomyField field, IUpdateModel updater, UpdateFieldEditorContext context)
        {
            var model = new EditTaxonomyFieldViewModel();

            if (await updater.TryUpdateModelAsync(model, Prefix))
            {
                var settings = context.PartFieldDefinition.GetSettings<TaxonomyFieldSettings>();

                field.TaxonomyContentItemId = settings.TaxonomyContentItemId;
                field.TermContentItemIds = model.TermEntries.Where(x => x.Selected).Select(x => x.ContentItemId).ToArray();

                if (settings.Required && field.TermContentItemIds.Length == 0)
                {
                    updater.ModelState.AddModelError(
                        nameof(EditTaxonomyFieldViewModel.TermEntries),
                        S["A value is required for '{0}'", context.PartFieldDefinition.Name]);
                }

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

                field.SetTagNames(terms.Select(t => t.DisplayText).ToArray());
            }

            return Edit(field, context);
        }
    }
}
