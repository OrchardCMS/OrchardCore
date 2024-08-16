using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.Contents.AuditTrail.Models;
using OrchardCore.Contents.AuditTrail.Settings;
using OrchardCore.Contents.AuditTrail.ViewModels;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Contents.AuditTrail.Drivers;

public sealed class AuditTrailPartDisplayDriver : ContentPartDisplayDriver<AuditTrailPart>
{
    public override IDisplayResult Edit(AuditTrailPart part, BuildPartEditorContext context)
    {
        var settings = context.TypePartDefinition.GetSettings<AuditTrailPartSettings>();
        if (settings.ShowCommentInput)
        {
            return Initialize<AuditTrailPartViewModel>(GetEditorShapeType(context), model =>
            {
                if (part.ShowComment)
                {
                    model.Comment = part.Comment;
                }
            });
        }

        return null;
    }

    public override async Task<IDisplayResult> UpdateAsync(AuditTrailPart part, UpdatePartEditorContext context)
    {
        await context.Updater.TryUpdateModelAsync(part, Prefix);

        return Edit(part, context);
    }
}
