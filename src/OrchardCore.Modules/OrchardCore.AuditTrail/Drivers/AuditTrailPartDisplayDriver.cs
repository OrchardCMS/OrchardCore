using System.Threading.Tasks;
using OrchardCore.AuditTrail.Models;
using OrchardCore.AuditTrail.Settings;
using OrchardCore.AuditTrail.ViewModels;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.AuditTrail.Drivers
{
    public class AuditTrailPartDisplayDriver : ContentPartDisplayDriver<AuditTrailPart>
    {
        public override IDisplayResult Edit(AuditTrailPart part, BuildPartEditorContext context)
        {
            var settings = context.TypePartDefinition.GetSettings<AuditTrailPartSettings>();
            if (settings.ShowAuditTrailCommentInput)
            {
                return Initialize<AuditTrailCommentViewModel>(GetEditorShapeType(context), model =>
                {
                    if (part.ShowComment)
                    {
                        model.Comment = part.Comment;
                    }
                });
            }
            return null;
        }

        public override async Task<IDisplayResult> UpdateAsync(AuditTrailPart part, IUpdateModel updater, UpdatePartEditorContext context)
        {
            await updater.TryUpdateModelAsync(part, Prefix);

            return Edit(part, context);
        }
    }
}
