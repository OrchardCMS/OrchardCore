using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentLocalization.Services;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Localization;
using OrchardCore.Taxonomies.Fields;
using OrchardCore.Taxonomies.Models;
using OrchardCore.Taxonomies.Settings;
using OrchardCore.Taxonomies.ViewModels;

namespace OrchardCore.Taxonomies.Drivers
{
    public class TaxonomyFieldDisplayDriver : ContentFieldDisplayDriver<TaxonomyField>
    {
        private readonly IContentManager _contentManager;
        private readonly ILocalizationEntries _localizationEntries;
        private readonly ILocalizationService _localizationService;
        private readonly IStringLocalizer S;

        public TaxonomyFieldDisplayDriver(
            IContentManager contentManager,
            ILocalizationEntries localizationEntries,
            ILocalizationService localizationService,
            IStringLocalizer<TaxonomyFieldDisplayDriver> localizer)
        {
            _contentManager = contentManager;
            _localizationEntries = localizationEntries;
            _localizationService = localizationService;
            S = localizer;
        }

        public override IDisplayResult Display(TaxonomyField field, BuildFieldDisplayContext context)
        {
            return Initialize<DisplayTaxonomyFieldViewModel>(GetDisplayShapeType(context), model =>
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
                string taxonomyContentItemId = settings.TaxonomyContentItemId;
                if (settings.Localized)
                {
                    if (_localizationEntries.TryGetLocalization(settings.TaxonomyContentItemId, out var initialLocalization))
                    {
                        string localizedCulture = context.IsNew ? (await _localizationService.GetDefaultCultureAsync()).ToLowerInvariant() : context.ContentPart.ContentItem.Content.LocalizationPart.Culture;
                        if(initialLocalization.Culture.ToLowerInvariant()!= localizedCulture.ToLowerInvariant())
                        {
                            var localizations = _localizationEntries.GetLocalizations(initialLocalization.LocalizationSet);
                            foreach(var localization in localizations)
                            {
                                if(localization.Culture.ToLowerInvariant() == localizedCulture.ToLowerInvariant())
                                {
                                    taxonomyContentItemId = localization.ContentItemId;
                                }
                            }
                        }
                    }
                }
                model.Taxonomy = await _contentManager.GetAsync(taxonomyContentItemId, VersionOptions.Latest);

                if (model.Taxonomy != null)
                {
                    var termEntries = new List<TermEntry>();
                    TaxonomyFieldDriverHelper.PopulateTermEntries(termEntries, field, model.Taxonomy.As<TaxonomyPart>().Terms, 0);

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
                var settings = context.PartFieldDefinition.GetSettings<TaxonomyFieldSettings>();

                field.TaxonomyContentItemId = settings.TaxonomyContentItemId;
                field.TermContentItemIds = model.TermEntries.Where(x => x.Selected).Select(x => x.ContentItemId).ToArray();

                if (settings.Unique && !String.IsNullOrEmpty(model.UniqueValue))
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
    }
}
