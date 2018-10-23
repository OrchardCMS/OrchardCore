using System.Collections.Generic;

namespace OrchardCore.Environment.Shell
{
    public interface IShellSettingsManager
    {
        /// <summary>
        /// Retrieves all shell settings stored.
        /// </summary>
        /// <returns>All shell settings.</returns>
        IEnumerable<ShellSettings> LoadSettings();

        /// <summary>
        /// Tries to load the shell settings associated with the specified name.
        /// </summary>
        /// <returns><c>true</c> if the settings could be found, <c>false</c> otherwise.</returns>
        bool TryLoadSettings(string name, out ShellSettings settings);

        /// <summary>
        /// Persists shell settings to the storage.
        /// </summary>
        /// <param name="settings">The shell settings to store.</param>
        void SaveSettings(ShellSettings settings);
    }
}