using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace OrchardCore.Users.Services
{
    /// <summary>
    /// This interface provides an abstraction for common user operations.
    /// </summary>
    public interface IUserService
    {
        Task<IUser> CreateUserAsync(IUser user, string password, Action<string, string> reportError);
        Task<bool> ChangePasswordAsync(IUser user, string currentPassword, string newPassword, Action<string, string> reportError);
        Task<IUser> GetAuthenticatedUserAsync(ClaimsPrincipal principal);
        Task<IUser> GetUserAsync(string userName);
        Task<IUser> GetForgotPasswordUserAsync(string userIdentifier);
        Task<bool> ResetPasswordAsync(string userIdentifier, string resetToken, string newPassword, Action<string, string> reportError);
    }
}