using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Mvc.ModelBinding;

namespace OrchardCore.ContentFields.Settings
{
    public class ContentPickerFieldSettingsDriver : ContentPartFieldDefinitionDisplayDriver<ContentPickerField>
    {
        public const string ContentTypes = "ContentTypes";
        public const string Stereotypes = "Stereotypes";
        public const string AllTypes = "AllTypes";

        private readonly IStringLocalizer S;

        public ContentPickerFieldSettingsDriver(IStringLocalizer<ContentPickerFieldSettingsDriver> stringLocalizer)
        {
            S = stringLocalizer;
        }

        public override IDisplayResult Edit(ContentPartFieldDefinition partFieldDefinition)
        {
            return Initialize<ContentPickerFieldSettingsViewModel>("ContentPickerFieldSettings_Edit", model =>
            {
                var settings = partFieldDefinition.GetSettings<ContentPickerFieldSettings>();
                model.Hint = settings.Hint;
                model.Required = settings.Required;
                model.Multiple = settings.Multiple;
                model.DisplayedContentTypes = settings.DisplayedContentTypes;
                model.DisplayedStereotypes = settings.DisplayedStereotypes;
                model.Source = GetSource(settings);
                model.Stereotypes = String.Join(',', settings.DisplayedStereotypes ?? Array.Empty<string>());
            }).Location("Content");
        }

        public override async Task<IDisplayResult> UpdateAsync(ContentPartFieldDefinition partFieldDefinition, UpdatePartFieldEditorContext context)
        {
            var model = new ContentPickerFieldSettingsViewModel();

            if (await context.Updater.TryUpdateModelAsync(model, Prefix))
            {
                var settings = new ContentPickerFieldSettings()
                {
                    Hint = model.Hint,
                    Required = model.Required,
                    Multiple = model.Multiple
                };

                if (ContentTypes.Equals(model.Source, StringComparison.OrdinalIgnoreCase))
                {
                    SetContentTypes(context.Updater, model.DisplayedContentTypes, settings);
                }
                else if (Stereotypes.Equals(model.Source, StringComparison.OrdinalIgnoreCase))
                {
                    SetStereoTypes(context.Updater, model.Stereotypes, settings);
                }
                else
                {
                    settings.DisplayAllContentTypes = true;
                }

                context.Builder.WithSettings(settings);
            }

            return Edit(partFieldDefinition);
        }

        private void SetStereoTypes(IUpdateModel updater, string stereotypes, ContentPickerFieldSettings settings)
        {
            if (String.IsNullOrEmpty(stereotypes))
            {
                updater.ModelState.AddModelError(Prefix, nameof(ContentPickerFieldSettingsViewModel.Stereotypes), S["Please provide a Stereotype."]);

                return;
            }

            settings.DisplayedStereotypes = stereotypes.Split(',', ';', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        }

        private void SetContentTypes(IUpdateModel updater, string[] displayedContentTypes, ContentPickerFieldSettings settings)
        {
            if (displayedContentTypes == null || displayedContentTypes.Length == 0)
            {
                updater.ModelState.AddModelError(Prefix, nameof(ContentPickerFieldSettingsViewModel.DisplayedContentTypes), S["At least one content type must be selected."]);

                return;
            }

            settings.DisplayedContentTypes = displayedContentTypes;
        }

        private static string GetSource(ContentPickerFieldSettings settings)
        {
            if (settings.DisplayAllContentTypes)
            {
                return AllTypes;
            }

            return settings.DisplayedStereotypes != null && settings.DisplayedStereotypes.Length > 0 ? Stereotypes : ContentTypes;
        }
    }
}
