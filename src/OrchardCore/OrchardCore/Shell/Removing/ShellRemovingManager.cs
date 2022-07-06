using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.Environment.Shell.Builders;
using OrchardCore.Environment.Shell.Models;
using OrchardCore.Modules;

namespace OrchardCore.Environment.Shell.Removing;

public class ShellRemovingManager : IShellRemovingManager
{
    private readonly IShellHost _shellHost;
    private readonly IEnumerable<IShellRemovingHandler> _hostHandlers;
    private readonly IShellContextFactory _shellContextFactory;
    private readonly ILogger _logger;

    public ShellRemovingManager(
        IShellHost shellHost,
        IEnumerable<IShellRemovingHandler> hostHandlers,
        IShellContextFactory shellContextFactory,
        ILogger<ShellRemovingManager> logger)
    {
        _shellHost = shellHost;
        _hostHandlers = hostHandlers;
        _shellContextFactory = shellContextFactory;
        _logger = logger;
    }

    public async Task<ShellRemovingContext> RemoveAsync(string tenant)
    {
        var context = new ShellRemovingContext(tenant);
        if (!_shellHost.TryGetSettings(tenant, out var shellSettings))
        {
            context.ErrorMessage = $"The tenant '{tenant}' doesn't exist.";
            return context;
        }

        if (shellSettings.Name == ShellHelper.DefaultShellName)
        {
            context.ErrorMessage = "The tenant should not be the 'Default' tenant.";
            return context;
        }

        if (shellSettings.State != TenantState.Disabled)
        {
            context.ErrorMessage = $"The tenant '{tenant}' should be 'Disabled'.";
            return context;
        }

        // Create a shell context composed of all features that have been installed.
        using var shellContext = await _shellContextFactory.CreateMaximumContextAsync(shellSettings);
        await shellContext.CreateScope().UsingServiceScopeAsync(async scope =>
        {
            // Executes tenant level removing handlers (singletons or scoped) in a reverse order.
            var tenantHandlers = scope.ServiceProvider.GetServices<IModularTenantEvents>().Reverse();
            foreach (var handler in tenantHandlers)
            {
                try
                {
                    await handler.RemovingAsync(context);
                    if (!context.Success)
                    {
                        break;
                    }
                }
                catch (Exception ex)
                {
                    var type = handler.GetType().FullName;

                    _logger.LogError(
                        ex,
                        "Failed to execute the tenant handler '{TenantHandler}' while removing the tenant '{TenantName}'.",
                        type,
                        tenant);

                    context.ErrorMessage = $"Failed to execute the tenant handler '{type}'.";

                    break;
                }
            }
        });

        // Executes host level removing handlers in a reverse order.
        foreach (var handler in _hostHandlers.Reverse())
        {
            try
            {
                await handler.RemovingAsync(context);
                if (!context.Success)
                {
                    break;
                }
            }
            catch (Exception ex)
            {
                var type = handler.GetType().FullName;

                _logger.LogError(
                    ex,
                    "Failed to execute the host handler '{HostHandler}' while removing the tenant '{TenantName}'.",
                    type,
                    tenant);

                context.ErrorMessage = $"Failed to execute the host handler '{type}'.";

                break;
            }
        }

        return context;
    }
}
