using System.Collections.Generic;

namespace OrchardCore.Environment.Shell
{
    public interface IShellSettingsManager
    {
        /// <summary>
        /// Retrieves the shell settings associated with the specified tenant.
        /// </summary>
        /// <returns>The shell settings associated with the tenant.</returns>
        ShellSettings GetSettings(string name);

        /// <summary>
        /// Retrieves all shell settings stored.
        /// </summary>
        /// <returns>All shell settings.</returns>
        IEnumerable<ShellSettings> LoadSettings();

        /// <summary>
        /// Persists shell settings to the storage.
        /// </summary>
        /// <param name="settings">The shell settings to store.</param>
        void SaveSettings(ShellSettings settings);

        /// <summary>
        /// Tries to retrieve the shell settings associated with the specified tenant.
        /// </summary>
        /// <returns><c>true</c> if the settings could be found, <c>false</c> otherwise.</returns>
        bool TryGetSettings(string name, out ShellSettings settings);
    }
}