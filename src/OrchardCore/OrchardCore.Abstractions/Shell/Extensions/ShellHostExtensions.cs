using System;

namespace OrchardCore.Environment.Shell
{
    public static class ShellHostExtensions
    {
        /// <summary>
        /// Retrieves the shell settings associated with the specified tenant.
        /// </summary>
        /// <returns>The shell settings associated with the tenant.</returns>
        public static ShellSettings GetSettings(this IShellHost shellHost, string name)
        {
            if (!shellHost.TryGetSettings(name, out ShellSettings settings))
            {
                throw new ArgumentException("The specified tenant name is not valid.", nameof(name));
            }

            return settings;
        }
    }
}
