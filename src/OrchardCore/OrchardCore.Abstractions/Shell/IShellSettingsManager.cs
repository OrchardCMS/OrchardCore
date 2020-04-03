using System.Collections.Generic;

namespace OrchardCore.Environment.Shell
{
    public interface IShellSettingsManager
    {
        /// <summary>
        /// Creates a default shell settings based on the configuration.
        /// </summary>
        ShellSettings CreateDefaultSettings();

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
    }
}