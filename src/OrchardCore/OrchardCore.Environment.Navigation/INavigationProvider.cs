namespace OrchardCore.Environment.Navigation
{
    public interface INavigationProvider
    {
        void BuildNavigation(string name, NavigationBuilder builder);
    }
}
