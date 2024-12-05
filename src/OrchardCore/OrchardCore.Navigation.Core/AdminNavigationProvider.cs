namespace OrchardCore.Navigation;

public abstract class AdminNavigationProvider : NamedNavigationProvider
{
    protected readonly bool UseLegacyFormat
        = AppContext.TryGetSwitch(NavigationConstants.LegacyAdminMenuNavigationSwitchKey, out var enable) && enable;

    public AdminNavigationProvider()
        : base(NavigationConstants.AdminId)
    {
    }
}
