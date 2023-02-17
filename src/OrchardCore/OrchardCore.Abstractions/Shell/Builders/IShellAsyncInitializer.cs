using System;
using System.Threading.Tasks;

namespace OrchardCore.Environment.Shell.Builders;

/// <summary>
/// Provides a mechanism for initializing tenant singleton services asynchronously.
/// </summary>
public interface IShellAsyncInitializer
{
    Task InitializeAsync(IServiceProvider serviceProvider);
}
