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
    public class AutoroutePartSettingsDisplayDriver : ContentTypePartDefinitionDisplayDriver<AutoroutePart>
    {
        private readonly ILiquidTemplateManager _templateManager;
        private readonly IStringLocalizer S;

        public AutoroutePartSettingsDisplayDriver(ILiquidTemplateManager templateManager, IStringLocalizer<AutoroutePartSettingsDisplayDriver> localizer)
        {
            _templateManager = templateManager;
            S = localizer;
        }

        public override IDisplayResult Edit(ContentTypePartDefinition contentTypePartDefinition, IUpdateModel updater)
        {
            return Initialize<AutoroutePartSettingsViewModel>("AutoroutePartSettings_Edit", model =>
            {
                var settings = contentTypePartDefinition.GetSettings<AutoroutePartSettings>();

                model.AllowCustomPath = settings.AllowCustomPath;
                model.AllowUpdatePath = settings.AllowUpdatePath;
                model.Pattern = settings.Pattern;
                model.ShowHomepageOption = settings.ShowHomepageOption;
                model.AllowDisabled = settings.AllowDisabled;
                model.AllowRouteContainedItems = settings.AllowRouteContainedItems;
                model.ManageContainedItemRoutes = settings.ManageContainedItemRoutes;
                model.AllowAbsolutePath = settings.AllowAbsolutePath;
                model.AutoroutePartSettings = settings;
            }).Location("Content");
        }

        public override async Task<IDisplayResult> UpdateAsync(ContentTypePartDefinition contentTypePartDefinition, UpdateTypePartEditorContext context)
        {
            var model = new AutoroutePartSettingsViewModel();

            await context.Updater.TryUpdateModelAsync(model, Prefix,
                m => m.Pattern,
                m => m.AllowCustomPath,
                m => m.AllowUpdatePath,
                m => m.ShowHomepageOption,
                m => m.AllowDisabled,
                m => m.AllowRouteContainedItems,
                m => m.ManageContainedItemRoutes,
                m => m.AllowAbsolutePath);

            if (!string.IsNullOrEmpty(model.Pattern) && !_templateManager.Validate(model.Pattern, out var errors))
            {
                context.Updater.ModelState.AddModelError(nameof(model.Pattern), S["Pattern doesn't contain a valid Liquid expression. Details: {0}", string.Join(" ", errors)]);
            }
            else
            {
                context.Builder.WithSettings(new AutoroutePartSettings
                {
                    Pattern = model.Pattern,
                    AllowCustomPath = model.AllowCustomPath,
                    AllowUpdatePath = model.AllowUpdatePath,
                    ShowHomepageOption = model.ShowHomepageOption,
                    AllowDisabled = model.AllowDisabled,
                    AllowRouteContainedItems = model.AllowRouteContainedItems,
                    ManageContainedItemRoutes = model.ManageContainedItemRoutes,
                    AllowAbsolutePath = model.AllowAbsolutePath
                });
            }

            return Edit(contentTypePartDefinition, context.Updater);
        }
    }
}
