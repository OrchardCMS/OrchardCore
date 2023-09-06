using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OrchardCore.Environment.Shell.Builders;
using OrchardCore.Modules;

namespace OrchardCore.Environment.Shell.Removing;

public class ShellRemovalManager : IShellRemovalManager
{
    private readonly IShellHost _shellHost;
    private readonly IShellContextFactory _shellContextFactory;
    private readonly IEnumerable<IShellRemovingHandler> _shellRemovingHandlers;
    protected readonly IStringLocalizer S;
    private readonly ILogger _logger;

    public ShellRemovalManager(
        IShellHost shellHost,
        IShellContextFactory shellContextFactory,
        IEnumerable<IShellRemovingHandler> shellRemovingHandlers,
        IStringLocalizer<ShellRemovalManager> localizer,
        ILogger<ShellRemovalManager> logger)
    {
        _shellHost = shellHost;
        _shellContextFactory = shellContextFactory;
        _shellRemovingHandlers = shellRemovingHandlers;
        S = localizer;
        _logger = logger;
    }

    public async Task<ShellRemovingContext> RemoveAsync(ShellSettings shellSettings, bool localResourcesOnly = false)
    {
        var context = new ShellRemovingContext
        {
            ShellSettings = shellSettings,
            LocalResourcesOnly = localResourcesOnly,
        };

        if (shellSettings.IsDefaultShell())
        {
            context.ErrorMessage = S["The tenant should not be the '{0}' tenant.", ShellSettings.DefaultShellName];
            return context;
        }

        // A disabled tenant may be still in use in at least one active scope.
        if (!shellSettings.IsRemovable() || _shellHost.IsShellActive(shellSettings))
        {
            context.ErrorMessage = S["The tenant '{0}' should be 'Disabled' or 'Uninitialized'.", shellSettings.Name];
            return context;
        }

        // Check if the tenant is not 'Uninitialized' and that all resources should be removed.
        if (shellSettings.IsDisabled() && !context.LocalResourcesOnly)
        {
            // Create an isolated shell context composed of all features that have been installed.
            ShellContext maximumContext = null;
            try
            {
                maximumContext = await _shellContextFactory.CreateMaximumContextAsync(shellSettings);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to create a 'ShellContext' before removing the tenant '{TenantName}'.",
                    shellSettings.Name);

                context.ErrorMessage = S["Failed to create a 'ShellContext' before removing the tenant."];
                context.Error = ex;

                return context;
            }

            await using var shellContext = maximumContext;
            (var locker, var locked) = await shellContext.TryAcquireShellRemovingLockAsync();
            if (!locked)
            {
                _logger.LogError(
                    "Failed to acquire a lock before executing the tenant handlers while removing the tenant '{TenantName}'.",
                    shellSettings.Name);

                context.ErrorMessage = S["Failed to acquire a lock before executing the tenant handlers."];
                return context;
            }

            await using var acquiredLock = locker;

            await (await shellContext.CreateScopeAsync()).UsingServiceScopeAsync(async scope =>
            {
                // Execute tenant level removing handlers (singletons or scoped) in a reverse order.
                // If feature A depends on feature B, the activating handler of feature B should run
                // before the handler of the dependent feature A, but on removing the resources of
                // feature B should be removed after the resources of the dependent feature A.
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
                            shellSettings.Name);

                        context.ErrorMessage = S["Failed to execute the tenant handler '{0}'.", type];
                        context.Error = ex;

                        break;
                    }
                }
            });
        }

        if (_shellHost.TryGetSettings(ShellSettings.DefaultShellName, out var defaultSettings))
        {
            // Use the default shell context to execute the host level removing handlers.
            var shellContext = await _shellHost.GetOrCreateShellContextAsync(defaultSettings);
            (var locker, var locked) = await shellContext.TryAcquireShellRemovingLockAsync();
            if (!locked)
            {
                _logger.LogError(
                    "Failed to acquire a lock before executing the host handlers while removing the tenant '{TenantName}'.",
                    shellSettings.Name);

                context.ErrorMessage = S["Failed to acquire a lock before executing the host handlers."];

                // If only local resources should be removed while syncing tenants.
                if (context.LocalResourcesOnly)
                {
                    // Indicates that we can retry in a next loop.
                    context.FailedOnLockTimeout = true;
                }

                return context;
            }

            await using var acquiredLock = locker;

            // Execute host level removing handlers in a reverse order.
            foreach (var handler in _shellRemovingHandlers.Reverse())
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
                        shellSettings.Name);

                    context.ErrorMessage = S["Failed to execute the host handler '{0}'.", type];
                    context.Error = ex;

                    break;
                }
            }
        }

        return context;
    }
}
