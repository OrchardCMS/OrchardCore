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
    private readonly IShellContextFactory _shellContextFactory;
    private readonly IEnumerable<IShellRemovingHostHandler> _shellRemovingHostHandlers;
    private readonly ILogger _logger;

    public ShellRemovingManager(
        IShellHost shellHost,
        IShellContextFactory shellContextFactory,
        IEnumerable<IShellRemovingHostHandler> shellRemovingHostHandlers,
        ILogger<ShellRemovingManager> logger)
    {
        _shellHost = shellHost;
        _shellContextFactory = shellContextFactory;
        _shellRemovingHostHandlers = shellRemovingHostHandlers;
        _logger = logger;
    }

    public async Task<ShellRemovingContext> RemoveAsync(ShellSettings settings, bool localResourcesOnly = false)
    {
        var context = new ShellRemovingContext
        {
            ShellSettings = settings,
            LocalResourcesOnly = localResourcesOnly,
        };

        if (settings.Name == ShellHelper.DefaultShellName)
        {
            context.ErrorMessage = $"The tenant should not be the '{ShellHelper.DefaultShellName}' tenant.";
            return context;
        }

        if (settings.State != TenantState.Disabled && settings.State != TenantState.Uninitialized)
        {
            context.ErrorMessage = $"The tenant '{settings.Name}' should be 'Disabled' or 'Uninitialized'.";
            return context;
        }

        if (settings.State == TenantState.Disabled && !context.LocalResourcesOnly)
        {
            // Create an isolated shell context composed of all features that have been installed.
            using var shellContext = await _shellContextFactory.CreateMaximumContextAsync(settings);
            (var locker, var locked) = await shellContext.TryAcquireShellRemovingLockAsync();
            if (!locked)
            {
                _logger.LogError(
                    "Failed to acquire a lock before executing the tenant level removing handlers while removing the tenant '{TenantName}'.",
                    settings.Name);

                context.ErrorMessage = $"Failed to acquire a lock before executing the tenant level removing handlers.";
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
                            "Failed to execute the tenant level removing handler '{TenantHandler}' while removing the tenant '{TenantName}'.",
                            type,
                            settings.Name);

                        context.ErrorMessage = $"Failed to execute the tenant level removing handler '{type}'.";
                        context.Error = ex;

                        break;
                    }
                }
            });
        }

        if (_shellHost.TryGetSettings(ShellHelper.DefaultShellName, out var shellSettings))
        {
            // Use the default shell context to execute the host level removing handlers.
            var shellContext = await _shellHost.GetOrCreateShellContextAsync(shellSettings);
            (var locker, var locked) = await shellContext.TryAcquireShellRemovingLockAsync();
            if (!locked)
            {
                _logger.LogError(
                    "Failed to acquire a lock before executing the host level removing handlers while removing the tenant '{TenantName}'.",
                    settings.Name);

                context.ErrorMessage = $"Failed to acquire a lock before executing the host level removing handlers.";
                return context;
            }

            await using var acquiredLock = locker;

            // Execute removing host level handlers in a reverse order.
            foreach (var handler in _shellRemovingHostHandlers.Reverse())
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
                        "Failed to execute the host level removing handler '{HostHandler}' while removing the tenant '{TenantName}'.",
                        type,
                        settings.Name);

                    context.ErrorMessage = $"Failed to execute the host level removing handler '{type}'.";
                    context.Error = ex;

                    break;
                }
            }
        }

        return context;
    }
}
