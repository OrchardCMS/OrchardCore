using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.Alias.Models;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Liquid;

namespace OrchardCore.Alias.Settings
{
    public class AliasPartSettingsDisplayDriver : ContentTypePartDefinitionDisplayDriver
    {
        private readonly ILiquidTemplateManager _templateManager;

        public AliasPartSettingsDisplayDriver(ILiquidTemplateManager templateManager, IStringLocalizer<AliasPartSettingsDisplayDriver> localizer)
        {
            _templateManager = templateManager;
            T = localizer;
        }

        public IStringLocalizer T { get; }

        public override IDisplayResult Edit(ContentTypePartDefinition contentTypePartDefinition, IUpdateModel updater)
        {
            if (!String.Equals(nameof(AliasPart), contentTypePartDefinition.PartDefinition.Name))
            {
                return null;
            }

            return Initialize<AliasPartSettingsViewModel>("AliasPartSettings_Edit", model =>
            {
                var settings = contentTypePartDefinition.GetSettings<AliasPartSettings>();

                model.Pattern = settings.Pattern;
                model.AliasPartSettings = settings;
            }).Location("Content");
        }

        public override async Task<IDisplayResult> UpdateAsync(ContentTypePartDefinition contentTypePartDefinition, UpdateTypePartEditorContext context)
        {
            if (!String.Equals(nameof(AliasPart), contentTypePartDefinition.PartDefinition.Name))
            {
                return null;
            }

            var model = new AliasPartSettingsViewModel();

            if (await context.Updater.TryUpdateModelAsync(model, Prefix, m => m.Pattern))
            {
                if (!string.IsNullOrEmpty(model.Pattern) && !_templateManager.Validate(model.Pattern, out var errors))
                {
                    context.Updater.ModelState.AddModelError(nameof(model.Pattern), T["Pattern doesn't contain a valid Liquid expression. Details: {0}", string.Join(" ", errors)]);
                }
                else
                {
                    context.Builder.WithSettings(new AliasPartSettings { Pattern = model.Pattern });
                }
            }

            return Edit(contentTypePartDefinition, context.Updater);
        }
    }
}
