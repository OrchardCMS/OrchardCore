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
    public class RouteHandlerPartSettingsDisplayDriver : ContentTypePartDefinitionDisplayDriver
    {
        private readonly ILiquidTemplateManager _templateManager;
        private readonly IStringLocalizer<ContainerRoutePartSettingsDisplayDriver> S;

        public RouteHandlerPartSettingsDisplayDriver(ILiquidTemplateManager templateManager, IStringLocalizer<ContainerRoutePartSettingsDisplayDriver> localizer)
        {
            _templateManager = templateManager;
            S = localizer;
        }

        public override IDisplayResult Edit(ContentTypePartDefinition contentTypePartDefinition, IUpdateModel updater)
        {
            if (!String.Equals(nameof(RouteHandlerPart), contentTypePartDefinition.PartDefinition.Name))
            {
                return null;
            }

            return Initialize<RouteHandlerPartSettingsViewModel>("RouteHandlerPartSettings_Edit", model =>
            {
                var settings = contentTypePartDefinition.GetSettings<RouteHandlerPartSettings>();

                model.AllowCustomPath = settings.AllowCustomPath;
                model.AllowUpdatePath = settings.AllowUpdatePath;
                model.Pattern = settings.Pattern;
                model.RouteHandlerPartSettings = settings;
            }).Location("Content");
        }

        public override async Task<IDisplayResult> UpdateAsync(ContentTypePartDefinition contentTypePartDefinition, UpdateTypePartEditorContext context)
        {
            if (!String.Equals(nameof(RouteHandlerPart), contentTypePartDefinition.PartDefinition.Name))
            {
                return null;
            }

            var model = new RouteHandlerPartSettingsViewModel();

            await context.Updater.TryUpdateModelAsync(model, Prefix,
                m => m.Pattern,
                m => m.AllowCustomPath,
                m => m.AllowUpdatePath);

            if (!string.IsNullOrEmpty(model.Pattern) && !_templateManager.Validate(model.Pattern, out var errors))
            {
                context.Updater.ModelState.AddModelError(nameof(model.Pattern), S["Pattern doesn't contain a valid Liquid expression. Details: {0}", string.Join(" ", errors)]);
            }
            else
            {
                context.Builder.WithSettings(new RouteHandlerPartSettings
                {
                    Pattern = model.Pattern,
                    AllowCustomPath = model.AllowCustomPath,
                    AllowUpdatePath = model.AllowUpdatePath
                });
            }

            return Edit(contentTypePartDefinition, context.Updater);
        }
    }
}
