using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.ContainerRoute.Models;
using OrchardCore.ContainerRoute.ViewModels;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Liquid;

namespace OrchardCore.ContainerRoute.Settings
{
    public class ContainerRoutePartSettingsDisplayDriver : ContentTypePartDefinitionDisplayDriver
    {
        private readonly ILiquidTemplateManager _templateManager;
        private readonly IStringLocalizer<ContainerRoutePartSettingsDisplayDriver> S;

        public ContainerRoutePartSettingsDisplayDriver(ILiquidTemplateManager templateManager, IStringLocalizer<ContainerRoutePartSettingsDisplayDriver> localizer)
        {
            _templateManager = templateManager;
            S = localizer;
        }

        public override IDisplayResult Edit(ContentTypePartDefinition contentTypePartDefinition, IUpdateModel updater)
        {
            if (!String.Equals(nameof(ContainerRoutePart), contentTypePartDefinition.PartDefinition.Name))
            {
                return null;
            }

            return Initialize<ContainerRoutePartSettingsViewModel>("ContainerRoutePartSettings_Edit", model =>
            {
                var settings = contentTypePartDefinition.GetSettings<ContainerRoutePartSettings>();

                model.AllowCustomPath = settings.AllowCustomPath;
                model.AllowUpdatePath = settings.AllowUpdatePath;
                model.Pattern = settings.Pattern;
                model.ShowHomepageOption = settings.ShowHomepageOption;
                model.ContainerRoutePartSettings = settings;
            }).Location("Content");
        }

        public override async Task<IDisplayResult> UpdateAsync(ContentTypePartDefinition contentTypePartDefinition, UpdateTypePartEditorContext context)
        {
            if (!String.Equals(nameof(ContainerRoutePart), contentTypePartDefinition.PartDefinition.Name))
            {
                return null;
            }

            var model = new ContainerRoutePartSettingsViewModel();

            await context.Updater.TryUpdateModelAsync(model, Prefix,
                m => m.Pattern,
                m => m.AllowCustomPath,
                m => m.AllowUpdatePath,
                m => m.ShowHomepageOption);

            if (!string.IsNullOrEmpty(model.Pattern) && !_templateManager.Validate(model.Pattern, out var errors))
            {
                context.Updater.ModelState.AddModelError(nameof(model.Pattern), S["Pattern doesn't contain a valid Liquid expression. Details: {0}", string.Join(" ", errors)]);
            }
            else
            {
                context.Builder.WithSettings(new ContainerRoutePartSettings
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
