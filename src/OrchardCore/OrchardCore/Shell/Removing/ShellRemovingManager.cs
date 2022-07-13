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
    private readonly IEnumerable<IShellRemovingHostHandler> _shellRemovingHostHandler;
    private readonly IShellContextFactory _shellContextFactory;
    private readonly ILogger _logger;

    public ShellRemovingManager(
        IShellHost shellHost,
        IEnumerable<IShellRemovingHostHandler> shellRemovingHostHandler,
        IShellContextFactory shellContextFactory,
        ILogger<ShellRemovingManager> logger)
    {
        _shellHost = shellHost;
        _shellRemovingHostHandler = shellRemovingHostHandler;
        _shellContextFactory = shellContextFactory;
        _logger = logger;
    }

    public async Task<ShellRemovingContext> RemoveAsync(string tenant)
    {
        if (!_shellHost.TryGetSettings(tenant, out var shellSettings))
        {
            return new ShellRemovingContext
            {
                ErrorMessage = $"The tenant '{tenant}' doesn't exist.",
            };
        }

        var context = new ShellRemovingContext
        {
            ShellSettings = shellSettings
        };

        if (shellSettings.Name == ShellHelper.DefaultShellName)
        {
            context.ErrorMessage = $"The tenant should not be the '{ShellHelper.DefaultShellName}' tenant.";
            return context;
        }

        if (shellSettings.State != TenantState.Disabled)
        {
            context.ErrorMessage = $"The tenant '{tenant}' should be 'Disabled'.";
            return context;
        }

        // Create an isolated shell context composed of all features that have been installed.
        using var shellContext = await _shellContextFactory.CreateMaximumContextAsync(shellSettings);
        (var locker, var locked) = await shellContext.TryAcquireShellRemovingLockAsync();
        if (!locked)
        {
            context.ErrorMessage = $"Failed to acquire a lock before removing the tenant: {tenant}.";
            return context;
        }

        await using var acquiredLock = locker;

        await shellContext.CreateScope().UsingServiceScopeAsync(async scope =>
        {
            // Execute removing tenant level handlers (singletons or scoped) in a reverse order.
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
                        "Failed to execute the removing tenant handler '{TenantHandler}' while removing the tenant '{TenantName}'.",
                        type,
                        tenant);

                    context.ErrorMessage = $"Failed to execute the removing tenant handler '{type}'.";
                    context.Error = ex;

                    break;
                }
            }
        });

        // Execute removing host level handlers in a reverse order.
        foreach (var handler in _shellRemovingHostHandler.Reverse())
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
                    "Failed to execute the removing host handler '{HostHandler}' while removing the tenant '{TenantName}'.",
                    type,
                    tenant);

                context.ErrorMessage = $"Failed to execute the removing host handler '{type}'.";
                context.Error = ex;

                break;
            }
        }

        return context;
    }
}
