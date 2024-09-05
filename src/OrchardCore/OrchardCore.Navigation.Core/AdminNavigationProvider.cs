namespace OrchardCore.Navigation;

public abstract class AdminNavigationProvider : NavigationProviderBase
{
    protected sealed override string Name
        => NavigationConstants.AdminId;
}
