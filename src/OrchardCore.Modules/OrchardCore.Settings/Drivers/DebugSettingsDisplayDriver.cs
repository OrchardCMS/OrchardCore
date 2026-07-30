using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Environment.Shell;
using OrchardCore.Settings.ViewModels;

namespace OrchardCore.Settings.Drivers;

public sealed class DebugSettingsDisplayDriver : SiteDisplayDriver<DebugSettings>
{
    public const string GroupId = "debugging";

    private readonly IShellReleaseManager _shellReleaseManager;

    protected override string SettingsGroupId
        => GroupId;

    public DebugSettingsDisplayDriver(IShellReleaseManager shellReleaseManager)
    {
        _shellReleaseManager = shellReleaseManager;
    }

    public override IDisplayResult Edit(ISite site, DebugSettings settings, BuildEditorContext context)
    {
        context.AddTenantReloadWarningWrapper();

        return Initialize<DebugSettingsViewModel>("DebugSettings_Edit", model =>
        {
            model.WriteShapeDebugInformation = settings.WriteShapeDebugInformation;
        }).Location("Content:5")
        .OnGroup(SettingsGroupId);
    }

    public override async Task<IDisplayResult> UpdateAsync(ISite site, DebugSettings settings, UpdateEditorContext context)
    {
        var model = new DebugSettingsViewModel();

        await context.Updater.TryUpdateModelAsync(model, Prefix);

        if (settings.WriteShapeDebugInformation != model.WriteShapeDebugInformation)
        {
            _shellReleaseManager.RequestRelease();
        }

        settings.WriteShapeDebugInformation = model.WriteShapeDebugInformation;

        return Edit(site, settings, context);
    }
}
