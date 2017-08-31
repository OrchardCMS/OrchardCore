using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OpenIddict.Core;

namespace Orchard.OpenId.Services
{
    /// <summary>
    /// Provides an abstraction for a store with maps apps to roles.
    /// </summary>
    /// <typeparam name="TOpenIdApplication">The type encapsulating a app.</typeparam>
    public interface IOpenIdApplicationRoleStore<TOpenIdApplication> : IOpenIddictApplicationStore<TOpenIdApplication> where TOpenIdApplication : class
    {
        /// <summary>
        /// Add a the specified <paramref name="application"/> to the named app.
        /// </summary>
        /// <param name="application">The app to add the named role.</param>
        /// <param name="roleName">The name of the role to add the app to.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation.</</returns>
        Task AddToRoleAsync(TOpenIdApplication application, string roleName, CancellationToken cancellationToken);

        /// <summary>
        /// Add a the specified <paramref name="application"/> from the named role.
        /// </summary>
        /// <param name="application">The app to remove the named role from.</param>
        /// <param name="roleName">The name of the role to remove.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation.</returns>
        Task RemoveFromRoleAsync(TOpenIdApplication application, string roleName, CancellationToken cancellationToken);

        /// <summary>
        /// Gets a list of role names the specified <paramref name="application"/> belongs to.
        /// </summary>
        /// <param name="application">The app whose role names to retrieve.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing a list of role names.</returns>
        Task<IList<string>> GetRolesAsync(TOpenIdApplication application, CancellationToken cancellationToken);

        /// <summary>
        /// Returns a flag indicating whether the specified <paramref name="application"/> is a member of the give named role.
        /// </summary>
        /// <param name="application">The app whose role membership should be checked.</param>
        /// <param name="roleName">The name of the role to be checked.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
        /// <returns>
        /// The <see cref="Task"/> that represents the asynchronous operation, containing a flag indicating whether the specified <paramref name="user"/> is
        /// a member of the named role.
        /// </returns>
        Task<bool> IsInRoleAsync(TOpenIdApplication application, string roleName, CancellationToken cancellationToken);

        /// <summary>
        /// Returns a list of Apps who are members of the named role.
        /// </summary>
        /// <param name="roleName">The name of the role whose membership should be returned.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
        /// <returns>
        /// The <see cref="Task"/> that represents the asynchronous operation, containing a list of users who are in the named role.
        /// </returns>
        Task<IList<TOpenIdApplication>> GetAppsInRoleAsync(string roleName, CancellationToken cancellationToken);
    }
}