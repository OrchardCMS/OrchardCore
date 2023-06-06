using System;
using OrchardCore.Environment.Shell.Models;

namespace OrchardCore.Environment.Shell;

public static class ShellSettingsExtensions
{
    /// <summary>
    /// Wether or not the provided name is the 'Default' tenant name.
    /// </summary>
    public static bool IsDefaultShellName(this string name) => name == ShellSettings.DefaultShellName;

    /// <summary>
    /// Wether or not the provided name may be in conflict with the 'Default' tenant name.
    /// </summary>
    public static bool IsDefaultShellNameIgnoreCase(this string name) =>
        name?.Equals(ShellSettings.DefaultShellName, StringComparison.OrdinalIgnoreCase) ?? false;

    /// <summary>
    /// Wether the tenant is the 'Default' tenant or not.
    /// </summary>
    public static bool IsDefaultShell(this ShellSettings settings) => settings is { Name: ShellSettings.DefaultShellName };

    /// <summary>
    /// Wether or not settings are null or related to the 'Default' tenant.
    /// </summary>
    public static bool IsDefaultShellOrNull(this ShellSettings settings) =>
        settings is null or { Name: ShellSettings.DefaultShellName };

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
