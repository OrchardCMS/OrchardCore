using System;
using System.Threading.Tasks;
using Orchard.Users.Models;
using System.Security.Claims;

namespace Orchard.Users.Services
{
    public interface IUserService
    {
        Task<bool> CreateUserAsync(User user, string password, Action<string, string> reportError);
        Task<bool> ChangePasswordAsync(User user, string currentPassword, string newPassword, Action<string, string> reportError);
        Task<User> GetAuthenticatedUserAsync(ClaimsPrincipal principal);
    }
}