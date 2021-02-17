using System.Threading.Tasks;
using OrchardCore.AuditTrail.Models;
using OrchardCore.AuditTrail.Settings;
using OrchardCore.AuditTrail.ViewModels;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.AuditTrail.Drivers
{
    public class AuditTrailPartSettingsDisplayDriver : ContentTypePartDefinitionDisplayDriver
    {
        public override IDisplayResult Edit(ContentTypePartDefinition model, IUpdateModel updater)
        {
            if (!string.Equals(nameof(AuditTrailPart), model.PartDefinition.Name)) return null;

            return Initialize<AuditTrailPartSettingsViewModel>("AuditTrailPartSettings_Edit", viewModel =>
            {
                var settings = model.GetSettings<AuditTrailPartSettings>();
                viewModel.ShowAuditTrailCommentInput = settings.ShowAuditTrailCommentInput;
                viewModel.AuditTrailPartSettings = settings;
            }).Location("Content");
        }

        public override async Task<IDisplayResult> UpdateAsync(ContentTypePartDefinition model, UpdateTypePartEditorContext context)
        {
            if (!string.Equals(nameof(AuditTrailPart), model.PartDefinition.Name)) return null;

            var viewModel = new AuditTrailPartSettingsViewModel();

            if (await context.Updater.TryUpdateModelAsync(viewModel, Prefix, m => m.ShowAuditTrailCommentInput))
            {
                context.Builder.WithSettings(new AuditTrailPartSettings
                {
                    ShowAuditTrailCommentInput = viewModel.ShowAuditTrailCommentInput
                });
            }

            return Edit(model, context.Updater);
        }
    }
}
