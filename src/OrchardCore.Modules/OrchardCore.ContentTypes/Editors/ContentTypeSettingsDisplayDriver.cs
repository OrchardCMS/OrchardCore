using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.ContentTypes.ViewModels;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.ContentTypes.Editors
{
    public class ContentTypeSettingsDisplayDriver : ContentTypeDefinitionDisplayDriver
    {
        protected readonly IStringLocalizer S;

        public ContentTypeSettingsDisplayDriver(IStringLocalizer<ContentTypeSettingsDisplayDriver> stringLocalizer)
        {
            S = stringLocalizer;
        }

        public override IDisplayResult Edit(ContentTypeDefinition contentTypeDefinition)
        {
            return Initialize<ContentTypeSettingsViewModel>("ContentTypeSettings_Edit", model =>
            {
                var settings = contentTypeDefinition.GetSettings<ContentTypeSettings>();

                model.Creatable = settings.Creatable;
                model.Listable = settings.Listable;
                model.Draftable = settings.Draftable;
                model.Versionable = settings.Versionable;
                model.Securable = settings.Securable;
                model.Stereotype = settings.Stereotype;
                model.Description = settings.Description;
            }).Location("Content:5");
        }

        public override async Task<IDisplayResult> UpdateAsync(ContentTypeDefinition contentTypeDefinition, UpdateTypeEditorContext context)
        {
            var model = new ContentTypeSettingsViewModel();

            if (await context.Updater.TryUpdateModelAsync(model, Prefix))
            {
                context.Builder.Creatable(model.Creatable);
                context.Builder.Listable(model.Listable);
                context.Builder.Draftable(model.Draftable);
                context.Builder.Versionable(model.Versionable);
                context.Builder.Securable(model.Securable);
                context.Builder.WithDescription(model.Description);

                var stereotype = model.Stereotype?.Trim();
                context.Builder.Stereotype(stereotype);

                if (!IsAlphaNumericOrEmpty(stereotype))
                {
                    context.Updater.ModelState.AddModelError(nameof(ContentTypeSettingsViewModel.Stereotype), S["The stereotype should be alphanumeric."]);
                }
            }

            return Edit(contentTypeDefinition);
        }

        private static bool IsAlphaNumericOrEmpty(string value)
        {
            if (String.IsNullOrEmpty(value))
            {
                return true;
            }

            var startWithLetter = Char.IsLetter(value[0]);

            return value.Length == 1
                ? startWithLetter
                : startWithLetter && value.Skip(1).All(c => Char.IsLetterOrDigit(c));
        }
    }
}
