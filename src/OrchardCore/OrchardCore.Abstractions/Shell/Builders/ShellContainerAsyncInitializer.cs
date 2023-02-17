using System;
using System.Threading.Tasks;

namespace OrchardCore.Environment.Shell.Builders;

/// <summary>
/// Provides a mechanism for initializing tenant container services asynchronously.
/// </summary>
internal class ShellContainerAsyncInitializer : IShellContainerAsyncInitializer
{
    private readonly Func<IServiceProvider, Task> _initializeAsync;

    public ShellContainerAsyncInitializer(Func<IServiceProvider, Task> initializeAsync) =>
        _initializeAsync = initializeAsync;

    public Task InitializeAsync(IServiceProvider serviceProvider) =>
        _initializeAsync.Invoke(serviceProvider);
}
