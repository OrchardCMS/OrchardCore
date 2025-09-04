namespace OrchardCore.Environment.Shell.Builders;

internal sealed class ShellContainerOptions
{
    /// <summary>
    /// Delegates to be invoked asynchronously after a tenant container is created.
    /// </summary>
    public List<Func<IServiceProvider, ValueTask>> Initializers { get; } = [];
}
