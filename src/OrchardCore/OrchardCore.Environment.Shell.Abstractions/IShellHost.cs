using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Hosting.ShellBuilders;

namespace OrchardCore.Environment.Shell
{
    public interface IShellHost
    {
        /// <summary>
        /// Ensure that all the <see cref="ShellContext"/> are created and available to process requests. 
        /// </summary>
        void Initialize();

        /// <summary>
        /// Returns an existing <see cref="ShellContext"/> or creates a new one if necessary. 
        /// </summary>
        /// <param name="settings">The <see cref="ShellSettings"/> object representing the shell to get.</param>
        /// <returns></returns>
        ShellContext GetOrCreateShellContext(ShellSettings settings);

        /// <summary>
        /// Creates a standalone service scope that can be used to resolve local services and
        /// replaces <see cref="HttpContext.RequestServices"/> with it.
        /// </summary>
        /// <param name="settings">The <see cref="ShellSettings"/> object representing the shell to get.</param>
        /// <param name="context">The <see cref="ShellContext"/> used to create the service scope.</param>
        /// <remarks>
        /// Disposing the returned <see cref="IServiceScope"/> instance restores the previous state.
        /// </remarks>
        IServiceScope EnterServiceScope(ShellSettings settings, out ShellContext context);

        /// <summary>
        /// Updates an existing shell configuration.
        /// </summary>
        /// <param name="settings"></param>
        void UpdateShellSettings(ShellSettings settings);

        /// <summary>
        /// Reloads a shell.
        /// </summary>
        /// <param name="settings"></param>
        void ReloadShellContext(ShellSettings settings);

        /// <summary>
        /// Creates a new <see cref="ShellContext"/>.
        /// </summary>
        /// <param name="settings">The <see cref="ShellSettings"/> object representing the shell to create.</param>
        /// <returns></returns>
        Task<ShellContext> CreateShellContextAsync(ShellSettings settings);

        /// <summary>
        /// Lists all available <see cref="ShellContext"/> instances. 
        /// </summary>
        /// <remarks>A shell might not be listed if it hasn't been created yet, for instance if it has been removed and not yet recreated.</remarks>
        IEnumerable<ShellContext> ListShellContexts();
    }
}