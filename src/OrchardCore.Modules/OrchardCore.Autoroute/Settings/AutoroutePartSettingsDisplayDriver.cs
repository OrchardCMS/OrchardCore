using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.Autoroute.Models;
using OrchardCore.Autoroute.ViewModels;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Liquid;

namespace OrchardCore.Autoroute.Settings
{
    public class AutoroutePartSettingsDisplayDriver : ContentTypePartDefinitionDisplayDriver
    {
        private readonly ILiquidTemplateManager _templateManager;

        public AutoroutePartSettingsDisplayDriver(ILiquidTemplateManager templateManager, IStringLocalizer<AutoroutePartSettingsDisplayDriver> localizer)
        {
            _templateManager = templateManager;
            T = localizer;
        }

        public IStringLocalizer T { get; }

        public override IDisplayResult Edit(ContentTypePartDefinition contentTypePartDefinition, IUpdateModel updater)
        {
            if (!String.Equals(nameof(AutoroutePart), contentTypePartDefinition.PartDefinition.Name))
            {
                return null;
            }

            return Initialize<AutoroutePartSettingsViewModel>("AutoroutePartSettings_Edit", model =>
            {
                var settings = contentTypePartDefinition.GetSettings<AutoroutePartSettings>();

                model.AllowCustomPath = settings.AllowCustomPath;
                model.AllowUpdatePath = settings.AllowUpdatePath;
                model.Pattern = settings.Pattern;
                model.ShowHomepageOption = settings.ShowHomepageOption;
                model.AutoroutePartSettings = settings;
            }).Location("Content");
        }

        public override async Task<IDisplayResult> UpdateAsync(ContentTypePartDefinition contentTypePartDefinition, UpdateTypePartEditorContext context)
        {
            if (!String.Equals(nameof(AutoroutePart), contentTypePartDefinition.PartDefinition.Name))
            {
                return null;
            }

            var model = new AutoroutePartSettingsViewModel();

            await context.Updater.TryUpdateModelAsync(model, Prefix,
                m => m.Pattern,
                m => m.AllowCustomPath,
                m => m.AllowUpdatePath,
                m => m.ShowHomepageOption);

            if (!string.IsNullOrEmpty(model.Pattern) && !_templateManager.Validate(model.Pattern, out var errors))
            {
                context.Updater.ModelState.AddModelError(nameof(model.Pattern), T["Pattern doesn't contain a valid Liquid expression. Details: {0}", string.Join(" ", errors)]);
            }
            else
            {
                context.Builder.WithSettings(new AutoroutePartSettings
                {
                    Pattern = model.Pattern,
                    AllowCustomPath = model.AllowCustomPath,
                    AllowUpdatePath = model.AllowUpdatePath,
                    ShowHomepageOption = model.ShowHomepageOption
                });
            }

            return Edit(contentTypePartDefinition, context.Updater);
        }
    }
}
