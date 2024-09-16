using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Settings.Drivers;

public sealed class ButtonsSettingsDisplayDriver : DisplayDriver<ISite>
{
    public const string GroupId = "general";

    public override IDisplayResult Edit(ISite site, BuildEditorContext context)
    {
        return Dynamic("SiteSettings_SaveButton")
            .Location("Actions")
            .OnGroup(context.GroupId); // Trick to render the shape for all groups
    }
}
