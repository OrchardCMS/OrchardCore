using System.Collections.Generic;
using System.Threading.Tasks;

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
        Task<IEnumerable<ShellSettings>> LoadSettingsAsync();

        /// <summary>
        /// Retrieves the settings of a given tenant
        /// </summary>
        /// <returns>The shell settings.</returns>
        Task<ShellSettings> LoadSettingsAsync(string tenant);

        /// <summary>
        /// Persists shell settings to the storage.
        /// </summary>
        /// <param name="settings">The shell settings to store.</param>
        Task SaveSettingsAsync(ShellSettings settings);
    }
}
