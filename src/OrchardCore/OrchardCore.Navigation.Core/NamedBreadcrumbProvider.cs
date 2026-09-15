namespace OrchardCore.Navigation;

/// <summary>
/// A base <see cref="IBreadcrumbProvider"/> that only contributes to the breadcrumb of a given name.
/// </summary>
public abstract class NamedBreadcrumbProvider : IBreadcrumbProvider
{
    /// <summary>
    /// The name of the breadcrumb this provider contributes to.
    /// </summary>
    protected readonly string Name;

    protected NamedBreadcrumbProvider(string name)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);

        Name = name;
    }

    public ValueTask BuildBreadcrumbAsync(BreadcrumbBuilder builder)
    {
        if (!string.Equals(builder.Name, Name, StringComparison.OrdinalIgnoreCase))
        {
            return ValueTask.CompletedTask;
        }

        return BuildAsync(builder);
    }

    /// <summary>
    /// Adds the nodes of the breadcrumb to the builder.
    /// </summary>
    /// <param name="builder">The builder collecting the nodes of the breadcrumb being rendered.</param>
    protected abstract ValueTask BuildAsync(BreadcrumbBuilder builder);
}
