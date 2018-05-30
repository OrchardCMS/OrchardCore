using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace OrchardCore.Users.Services
{
    public interface IUserService
    {
        Task<IUser> CreateUserAsync(string userName, string email, IList<string> roles, string password, Action<string, string> reportError);
        Task<bool> ChangePasswordAsync(IUser user, string currentPassword, string newPassword, Action<string, string> reportError);
        Task<IUser> GetAuthenticatedUserAsync(ClaimsPrincipal principal);
        Task<IUser> GetForgotPasswordUserAsync(string userIdentifier);
        Task<bool> ResetPasswordAsync(string userIdentifier, string resetToken, string newPassword, Action<string, string> reportError);
    }
}