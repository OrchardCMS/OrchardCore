using System;
using System.Threading.Tasks;
using System.Security.Claims;
using Orchard.Security;

namespace Orchard.Users.Services
{
    public interface IUserService
    {
        Task<bool> CreateUserAsync(string userName, string email, string[] roleNames, string password, Action<string, string> reportError);
        Task<bool> ChangePasswordAsync(string userName, string currentPassword, string newPassword, Action<string, string> reportError);
        Task<IUser> GetAuthenticatedUserAsync(ClaimsPrincipal principal);
    }
}