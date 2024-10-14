namespace OrchardCore.Navigation;

public abstract class AdminNavigationProvider : NamedNavigationProvider
{
    public AdminNavigationProvider()
        : base(NavigationConstants.AdminId)
    {
    }
}
