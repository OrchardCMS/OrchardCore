using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Environment.Shell.Builders;
using OrchardCore.Environment.Shell.Scope;

namespace OrchardCore.Environment.Shell
{
    public interface IShellHost : IShellDescriptorManagerEventHandler
    {
        /// <summary>
        /// Ensure that all the <see cref="ShellContext"/> are pre-created and available to process requests.
        /// </summary>
        Task InitializeAsync();

        /// <summary>
        /// Returns an existing <see cref="ShellContext"/> or creates a new one if necessary.
        /// </summary>
        /// <param name="settings">The <see cref="ShellSettings"/> object representing the shell to get.</param>
        /// <returns></returns>
        Task<ShellContext> GetOrCreateShellContextAsync(ShellSettings settings);

        /// <summary>
        /// Creates a standalone service scope that can be used to resolve local services.
        /// </summary>
        /// <param name="settings">The <see cref="ShellSettings"/> object representing the shell to get.</param>
        Task<ShellScope> GetScopeAsync(ShellSettings settings);

        /// <summary>
        /// Updates an existing shell configuration.
        /// </summary>
        /// <param name="settings"></param>
        Task UpdateShellSettingsAsync(ShellSettings settings);

        /// <summary>
        /// Reloads a shell.
        /// </summary>
        /// <param name="settings"></param>
        Task ReloadShellContextAsync(ShellSettings settings);

        /// <summary>
        /// Creates a new <see cref="ShellContext"/>.
        /// </summary>
        /// <param name="settings">The <see cref="ShellSettings"/> object representing the shell to create.</param>
        /// <returns></returns>
        Task<ShellContext> CreateShellContextAsync(ShellSettings settings);

        /// <summary>
        /// Lists all available <see cref="ShellContext"/> instances.
        /// A shell might have been released or not yet built, if so 'shell.Released' is true and
        /// 'shell.CreateScope()' return null, but you can still use 'GetScopeAsync(shell.Settings)'.
        /// </summary>
        /// <remarks>A shell might not be listed if it hasn't been created yet, for instance if it has been removed and not yet recreated.</remarks>
        IEnumerable<ShellContext> ListShellContexts();

        /// <summary>
        /// Tries to retrieve the shell settings associated with the specified tenant.
        /// </summary>
        /// <returns><c>true</c> if the settings could be found, <c>false</c> otherwise.</returns>
        bool TryGetSettings(string name, out ShellSettings settings);

        /// <summary>
        /// Retrieves all shell settings.
        /// </summary>
        /// <returns>All shell settings.</returns>
        IEnumerable<ShellSettings> GetAllSettings();
    }
}
