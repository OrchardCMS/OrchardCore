namespace OrchardCore.Navigation;

public abstract class NavigationProviderBase : INavigationProvider
{
    /// <summary>
    /// The name of the navigation menu.
    /// </summary>
    protected abstract string Name { get; }

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
    protected virtual ValueTask BuildAsync(NavigationBuilder builder)
    {
        Build(builder);

        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Synchronously builds the navigations menu.
    /// </summary>
    /// <param name="builder"></param>
    protected virtual void Build(NavigationBuilder builder)
    {
    }
}
