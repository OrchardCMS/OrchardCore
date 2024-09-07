namespace OrchardCore.Navigation;

public abstract class NamedNavigationProvider : INavigationProvider
{
    /// <summary>
    /// The name of the navigation menu.
    /// </summary>
    protected readonly string Name;

    protected NamedNavigationProvider(string name)
    {
        Name = name;
    }

    public ValueTask BuildNavigationAsync(string name, NavigationBuilder builder)
    {
        if (name != Name)
        {
            return ValueTask.CompletedTask;
        }

        return BuildAsync(builder);
    }

    /// <summary>
    /// Asynchronously builds the navigations menu.
    /// </summary>
    /// <param name="builder"></param>
    protected abstract ValueTask BuildAsync(NavigationBuilder builder);
}
