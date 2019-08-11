using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Contents.Models;
using OrchardCore.Contents.ViewModels;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Lists.Settings
{
    public class CommonPartSettingsDisplayDriver : ContentTypePartDefinitionDisplayDriver
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;

        public CommonPartSettingsDisplayDriver(
            IContentDefinitionManager contentDefinitionManager,
            IStringLocalizer<CommonPartSettingsDisplayDriver> localizer)
        {
            _contentDefinitionManager = contentDefinitionManager;
            TS = localizer;
        }

        public IStringLocalizer TS { get; set; }

        public override IDisplayResult Edit(ContentTypePartDefinition contentTypePartDefinition, IUpdateModel updater)
        {
            if (!String.Equals(nameof(CommonPart), contentTypePartDefinition.PartDefinition.Name, StringComparison.Ordinal))
            {
                return null;
            }

            return Initialize<CommonPartSettingsViewModel>("CommonPartSettings_Edit", model =>
            {
                var settings = contentTypePartDefinition.GetSettings<CommonPartSettings>();
                model.DisplayDateEditor = settings.DisplayDateEditor;
                model.DisplayOwnerEditor = settings.DisplayOwnerEditor;

            }).Location("Content");
        }

        public override async Task<IDisplayResult> UpdateAsync(ContentTypePartDefinition contentTypePartDefinition, UpdateTypePartEditorContext context)
        {
            if (!String.Equals(nameof(CommonPart), contentTypePartDefinition.PartDefinition.Name, StringComparison.Ordinal))
            {
                return null;
            }

            var model = new CommonPartSettingsViewModel();

            if (await context.Updater.TryUpdateModelAsync(model, Prefix))
            {
                context.Builder.WithSettings(new CommonPartSettings { DisplayDateEditor = model.DisplayDateEditor, DisplayOwnerEditor = model.DisplayOwnerEditor});
            }

            return Edit(contentTypePartDefinition, context.Updater);
        }
    }
}