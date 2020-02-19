using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Environment.Shell.Builders;
using OrchardCore.Environment.Shell.Scope;

namespace OrchardCore.Environment.Shell
{
    public interface IShellHost
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
        /// Reloads a shell so that its settings will be reloaded before creating a new one
        /// for subsequent requests, while existing requests get flushed.
        /// </summary>
        /// <param name="settings"></param>
        Task ReloadShellContextAsync(ShellSettings settings);

        /// <summary>
        /// Releases a shell so that a new one will be created for subsequent requests.
        /// Note: Can be used to free up resources after a given period of inactivity.
        /// </summary>
        Task ReleaseShellContextAsync(ShellSettings settings);

        /// <summary>
        /// Lists all available <see cref="ShellContext"/> instances.
        /// A shell might have been released or not yet built, if so 'shell.Released' is true and
        /// 'shell.CreateScope()' return null, but you can still use 'GetScopeAsync(shell.Settings)'.
        /// </summary>
        IEnumerable<ShellContext> ListShellContexts();

        /// <summary>
        /// Tries to retrieve the shell settings associated with the specified tenant.
        /// </summary>
        bool TryGetSettings(string name, out ShellSettings settings);

        /// <summary>
        /// Retrieves all shell settings.
        /// </summary>
        IEnumerable<ShellSettings> GetAllSettings();
    }
}
