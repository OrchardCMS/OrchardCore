using OrchardCore.Environment.Shell.Models;

namespace OrchardCore.Environment.Shell
{
    public static class ShellSettingsExtensions
    {
        /// <summary>
        /// Wether it is the 'Default' tenant or not.
        /// </summary>
        public static bool IsDefaultShell(this ShellSettings settings) => settings is { Name: ShellHelper.DefaultShellName };

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
    }
}
