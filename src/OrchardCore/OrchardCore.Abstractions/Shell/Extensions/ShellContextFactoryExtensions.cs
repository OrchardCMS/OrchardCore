using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Environment.Shell.Builders;
using OrchardCore.Environment.Shell.Descriptor;
using OrchardCore.Environment.Shell.Descriptor.Models;

namespace OrchardCore.Environment.Shell;

public static class ShellContextFactoryExtensions
{
    /// <summary>
    /// Creates an isolated maximum shell context composed of all installed features. 
    /// </summary>
    public static async Task<ShellContext> CreateMaximumContextAsync(
        this IShellContextFactory shellContextFactory, ShellSettings shellSettings, bool ownSettings = false)
    {
        var shellDescriptor = await shellContextFactory.GetShellDescriptorAsync(shellSettings);
        if (shellDescriptor is null)
        {
            return await shellContextFactory.CreateMinimumContextAsync(shellSettings, ownSettings);
        }

        shellDescriptor = new ShellDescriptor { Features = shellDescriptor.Installed };

        return (await shellContextFactory
            .CreateDescribedContextAsync(shellSettings, shellDescriptor))
            .WithOwnSettings(ownSettings);
    }

    /// <summary>
    /// Creates an isolated minimum shell context without any feature. 
    /// </summary>
    public static async Task<ShellContext> CreateMinimumContextAsync(
        this IShellContextFactory shellContextFactory, ShellSettings shellSettings, bool ownSettings = false) =>
            (await shellContextFactory
                .CreateDescribedContextAsync(shellSettings, new ShellDescriptor()))
                .WithOwnSettings(ownSettings);

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
