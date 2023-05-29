using OrchardCore.Environment.Shell.Models;

namespace OrchardCore.Environment.Shell
{
    public static class ShellSettingsExtensions
    {
        /// <summary>
        /// Wether it is the 'Default' tenant or not.
        /// </summary>
        public static bool IsDefaultShell(this ShellSettings settings) =>
            settings is not null && settings.Name == ShellHelper.DefaultShellName;

        /// <summary>
        /// Wether the tenant is uninitialized or not.
        /// </summary>
        public static bool IsUninitialized(this ShellSettings settings) =>
            settings is not null && settings.State == TenantState.Uninitialized;

        /// <summary>
        /// Wether the tenant is initializing or not.
        /// </summary>
        public static bool IsInitializing(this ShellSettings settings) =>
            settings is not null && settings.State == TenantState.Initializing;

        /// <summary>
        /// Wether the tenant is running or not.
        /// </summary>
        public static bool IsRunning(this ShellSettings settings) =>
            settings is not null && settings.State == TenantState.Running;

        /// <summary>
        /// Wether the tenant is disabled or not.
        /// </summary>
        public static bool IsDisabled(this ShellSettings settings) =>
            settings is not null && settings.State == TenantState.Disabled;

        /// <summary>
        /// Wether the tenant is initialized or not.
        /// </summary>
        public static bool IsInitialized(this ShellSettings settings) =>
            settings is not null && (settings.IsRunning() || settings.IsDisabled());
    }
}
