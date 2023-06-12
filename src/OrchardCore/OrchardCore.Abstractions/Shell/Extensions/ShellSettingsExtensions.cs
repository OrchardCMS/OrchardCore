using System;
using OrchardCore.Environment.Shell.Models;

namespace OrchardCore.Environment.Shell;

public static class ShellSettingsExtensions
{
    /// <summary>
    /// Wether the tenant is the 'Default' tenant or not.
    /// </summary>
    public static bool IsDefaultShell(this ShellSettings settings) => settings is { Name: ShellSettings.DefaultShellName };

    /// <summary>
    /// Wether the tenant is uninitialized or not.
    /// </summary>
    public static bool IsUninitialized(this ShellSettings settings) => settings is { State: TenantState.Uninitialized };

    /// <summary>
    /// Wether the tenant is initializing or not.
    /// </summary>
    public static bool IsInitializing(this ShellSettings settings) => settings is { State: TenantState.Initializing };

    /// <summary>
    /// Wether the tenant is running or not.
    /// </summary>
    public static bool IsRunning(this ShellSettings settings) => settings is { State: TenantState.Running };

    /// <summary>
    /// Wether the tenant is disabled or not.
    /// </summary>
    public static bool IsDisabled(this ShellSettings settings) => settings is { State: TenantState.Disabled };

    /// <summary>
    /// Wether the tenant is initialized or not.
    /// </summary>
    public static bool IsInitialized(this ShellSettings settings) => settings.IsRunning() || settings.IsDisabled();

    /// <summary>
    /// Wether the tenant is removable or not.
    /// </summary>
    public static bool IsRemovable(this ShellSettings settings) => settings.IsUninitialized() || settings.IsDisabled();

    /// <summary>
    /// Wether or not the tenant has the provided url prefix.
    /// </summary>
    public static bool HasUrlPrefix(this ShellSettings settings, string urlPrefix) =>
        settings is not null &&
        String.Equals(
            settings.RequestUrlPrefix ?? String.Empty,
            urlPrefix?.Trim(' ', '/') ?? String.Empty,
            StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Wether or not the tenant has one of the url host(s) defined by the provided string.
    /// </summary>
    public static bool HasUrlHost(this ShellSettings settings, string urlHost) =>
        settings.HasUrlHost(urlHost
            ?.Split(ShellSettings.HostSeparators, StringSplitOptions.RemoveEmptyEntries)
            ?? Array.Empty<string>());

    /// <summary>
    /// Wether or not the tenant has one of the provided url hosts.
    /// </summary>
    public static bool HasUrlHost(this ShellSettings settings, string[] urlHosts)
    {
        if (settings is null)
        {
            return false;
        }

        if (settings.RequestUrlHosts.Length == 0 && urlHosts.Length == 0)
        {
            return true;
        }

        if (settings.RequestUrlHosts.Length == 0 || urlHosts.Length == 0)
        {
            return false;
        }

        for (var i = 0; i < settings.RequestUrlHosts.Length; i++)
        {
            for (var j = 0; j < urlHosts.Length; j++)
            {
                if (settings.RequestUrlHosts[i].Equals(urlHosts[j], StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// As the 'Default' tenant.
    /// </summary>
    public static ShellSettings AsDefaultShell(this ShellSettings settings)
    {
        settings.Name = ShellSettings.DefaultShellName;
        return settings;
    }

    /// <summary>
    /// As an uninitialized tenant.
    /// </summary>
    public static ShellSettings AsUninitialized(this ShellSettings settings)
    {
        settings.State = TenantState.Uninitialized;
        return settings;
    }

    /// <summary>
    /// As an initializing tenant.
    /// </summary>
    public static ShellSettings AsInitializing(this ShellSettings settings)
    {
        settings.State = TenantState.Initializing;
        return settings;
    }

    /// <summary>
    /// As a running tenant.
    /// </summary>
    public static ShellSettings AsRunning(this ShellSettings settings)
    {
        settings.State = TenantState.Running;
        return settings;
    }

    /// <summary>
    /// As a disabled tenant.
    /// </summary>
    public static ShellSettings AsDisabled(this ShellSettings settings)
    {
        settings.State = TenantState.Disabled;
        return settings;
    }
}
