using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.Contents.AuditTrail.Models;
using OrchardCore.Contents.AuditTrail.Settings;
using OrchardCore.Contents.AuditTrail.ViewModels;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Contents.AuditTrail.Drivers;

public sealed class AuditTrailPartSettingsDisplayDriver : ContentTypePartDefinitionDisplayDriver
{
    internal readonly IStringLocalizer S;

    public AuditTrailPartSettingsDisplayDriver(IStringLocalizer<AuditTrailPartSettingsDisplayDriver> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public override IDisplayResult Edit(ContentTypePartDefinition model, BuildEditorContext context)
    {
        if (!string.Equals(nameof(AuditTrailPart), model.PartDefinition.Name, StringComparison.Ordinal))
        {
            return null;
        }

        return Initialize<AuditTrailPartSettingsViewModel>("AuditTrailPartSettings_Edit", viewModel =>
        {
            var settings = model.GetSettings<AuditTrailPartSettings>();
            viewModel.ShowCommentInput = settings.ShowCommentInput;
        }).Location(l => l.Zone("Content").Card(S["Settings"], "20"));
    }

    public override async Task<IDisplayResult> UpdateAsync(ContentTypePartDefinition model, UpdateTypePartEditorContext context)
    {
        if (!string.Equals(nameof(AuditTrailPart), model.PartDefinition.Name, StringComparison.Ordinal))
        {
            return null;
        }

        var viewModel = new AuditTrailPartSettingsViewModel();

        await context.Updater.TryUpdateModelAsync(viewModel, Prefix, m => m.ShowCommentInput);

        context.Builder.WithSettings(new AuditTrailPartSettings
        {
            ShowCommentInput = viewModel.ShowCommentInput,
        });

        return Edit(model, context);
    }
}
