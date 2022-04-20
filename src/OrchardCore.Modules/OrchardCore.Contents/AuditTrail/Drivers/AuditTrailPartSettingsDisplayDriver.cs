using System;
using System.Threading.Tasks;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.Contents.AuditTrail.Models;
using OrchardCore.Contents.AuditTrail.Settings;
using OrchardCore.Contents.AuditTrail.ViewModels;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Contents.AuditTrail.Drivers
{
    public class AuditTrailPartSettingsDisplayDriver : ContentTypePartDefinitionDisplayDriver
    {
        public override IDisplayResult Edit(ContentTypePartDefinition model, IUpdateModel updater)
        {
            if (!String.Equals(nameof(AuditTrailPart), model.PartDefinition.Name)) return null;

            return Initialize<AuditTrailPartSettingsViewModel>("AuditTrailPartSettings_Edit", viewModel =>
            {
                var settings = model.GetSettings<AuditTrailPartSettings>();
                viewModel.ShowCommentInput = settings.ShowCommentInput;
            }).Location("Content");
        }

        public override async Task<IDisplayResult> UpdateAsync(ContentTypePartDefinition model, UpdateTypePartEditorContext context)
        {
            if (!String.Equals(nameof(AuditTrailPart), model.PartDefinition.Name)) return null;

            var viewModel = new AuditTrailPartSettingsViewModel();

            if (await context.Updater.TryUpdateModelAsync(viewModel, Prefix, m => m.ShowCommentInput))
            {
                context.Builder.WithSettings(new AuditTrailPartSettings
                {
                    ShowCommentInput = viewModel.ShowCommentInput
                });
            }

            return Edit(model, context.Updater);
        }
    }
}
