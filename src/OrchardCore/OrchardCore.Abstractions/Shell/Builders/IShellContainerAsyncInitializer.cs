using System;
using System.Threading.Tasks;

namespace OrchardCore.Environment.Shell.Builders;

/// <summary>Provides a mechanism for initializing tenant singleton services asynchronously.</summary>
public interface IShellContainerAsyncInitializer
{
    Task InitializeAsync(IServiceProvider serviceProvider);
}
