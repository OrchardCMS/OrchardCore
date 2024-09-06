namespace OrchardCore.Navigation;

public abstract class AdminMenuNavigationProvider : NamedNavigationProvider
{
    public AdminMenuNavigationProvider()
        : base(NavigationConstants.AdminId)
    {
    }
}
