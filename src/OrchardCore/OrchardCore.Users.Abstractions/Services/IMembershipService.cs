using System.Security.Claims;
using System.Threading.Tasks;

namespace OrchardCore.Users.Services
{
    /// <summary>
    /// Contract for membership services.
    /// </summary>
    public interface IMembershipService
    {
        /// <summary>
        /// Get the user with the specified username.
        /// </summary>
        /// <param name="userName">The username for the retrieved user.</param>
        /// <returns>The <see cref="IUser"/>.</returns>
        Task<IUser> GetUserAsync(string userName);

        /// <summary>
        /// Checks the user password.
        /// </summary>
        /// <param name="userName">The username.</param>
        /// <param name="password">The user password</param>
        /// <returns>Returns<c>true</c> if the password is correct, otherwise <c>false</c>.</returns>
        Task<bool> CheckPasswordAsync(string userName, string password);

        /// <summary>
        /// Creates a <see cref="ClaimsPrincipal"/> for the specified user.
        /// </summary>
        /// <param name="user">The <see cref="IUser"/>.</param>
        /// <returns>A user <see cref="ClaimsPrincipal"/>s.</returns>
        Task<ClaimsPrincipal> CreateClaimsPrincipal(IUser user);
    }
}
