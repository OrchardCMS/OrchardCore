using System;
using System.Threading.Tasks;
using System.Security.Claims;

namespace OrchardCore.Users.Services
{
    public interface IUserService
    {
        Task<IUser> CreateUserAsync(string userName, string email, string[] roleNames, string password, string timeZone, Action<string, string> reportError);
        Task<bool> ChangePasswordAsync(IUser user, string currentPassword, string newPassword, Action<string, string> reportError);
        Task<IUser> GetAuthenticatedUserAsync(ClaimsPrincipal principal);
        Task<IUser> GetForgotPasswordUserAsync(string userIdentifier);
        Task<bool> ResetPasswordAsync(string userIdentifier, string resetToken, string newPassword, Action<string, string> reportError);
    }
}