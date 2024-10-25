using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Environment.Shell.Builders;
using OrchardCore.Environment.Shell.Descriptor;
using OrchardCore.Environment.Shell.Descriptor.Models;

namespace OrchardCore.Environment.Shell;

public static class ShellContextFactoryExtensions
{
    /// <summary>
    /// Creates a maximum shell context composed of all installed features, and
    /// marked by default as using shared settings that should not be disposed. 
    /// </summary>
    public static async Task<ShellContext> CreateMaximumContextAsync(
        this IShellContextFactory shellContextFactory, ShellSettings shellSettings, bool sharedSettings = true)
    {
        var shellDescriptor = await shellContextFactory.GetShellDescriptorAsync(shellSettings);
        if (shellDescriptor is null)
        {
            return await shellContextFactory.CreateMinimumContextAsync(shellSettings, sharedSettings);
        }

        shellDescriptor = new ShellDescriptor { Features = shellDescriptor.Installed.Cast<ShellFeature>().ToList() };

        var context = await shellContextFactory.CreateDescribedContextAsync(shellSettings, shellDescriptor);
        if (sharedSettings)
        {
            context.WithSharedSettings();
        }

        return context;
    }

    /// <summary>
    /// Creates a minimum shell context without any feature, and marked
    /// by default as using shared settings that should not be disposed. 
    /// </summary>
    public static async Task<ShellContext> CreateMinimumContextAsync(
        this IShellContextFactory shellContextFactory, ShellSettings shellSettings, bool sharedSettings = true)
    {
        var context = await shellContextFactory.CreateDescribedContextAsync(shellSettings, new ShellDescriptor());
        if (sharedSettings)
        {
            context.WithSharedSettings();
        }

        return context;
    }

    /// <summary>
    /// Gets the shell descriptor from the store.
    /// </summary>
    public static async Task<ShellDescriptor> GetShellDescriptorAsync(this IShellContextFactory shellContextFactory, ShellSettings shellSettings)
    {
        ShellDescriptor shellDescriptor = null;

        await using var shellContext = await shellContextFactory.CreateMinimumContextAsync(shellSettings);
        await (await shellContext.CreateScopeAsync()).UsingServiceScopeAsync(async scope =>
        {
            var shellDescriptorManager = scope.ServiceProvider.GetRequiredService<IShellDescriptorManager>();
            shellDescriptor = await shellDescriptorManager.GetShellDescriptorAsync();
        });

        return shellDescriptor;
    }
}
