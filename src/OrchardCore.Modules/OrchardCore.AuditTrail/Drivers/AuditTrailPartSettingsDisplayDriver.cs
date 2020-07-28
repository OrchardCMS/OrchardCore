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
        public override IDisplayResult Edit(ContentTypePartDefinition contentTypePartDefinition, IUpdateModel updater)
        {
            if (!string.Equals(nameof(AuditTrailPart), contentTypePartDefinition.PartDefinition.Name)) return null;

            return Initialize<AuditTrailPartSettingsViewModel>("AuditTrailPartSettings_Edit", model =>
            {
                var settings = contentTypePartDefinition.GetSettings<AuditTrailPartSettings>();
                model.ShowAuditTrailCommentInput = settings.ShowAuditTrailCommentInput;
                model.AuditTrailPartSettings = settings;
            }).Location("Content");
        }

        public override async Task<IDisplayResult> UpdateAsync(ContentTypePartDefinition contentTypePartDefinition, UpdateTypePartEditorContext context)
        {
            if (!string.Equals(nameof(AuditTrailPart), contentTypePartDefinition.PartDefinition.Name)) return null;

            var model = new AuditTrailPartSettingsViewModel();

            if (await context.Updater.TryUpdateModelAsync(model, Prefix, m => m.ShowAuditTrailCommentInput))
            {
                context.Builder.WithSettings(new AuditTrailPartSettings
                {
                    ShowAuditTrailCommentInput = model.ShowAuditTrailCommentInput
                });
            }

            return Edit(contentTypePartDefinition, context.Updater);
        }
    }
}
