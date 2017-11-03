using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using OrchardCore.Users.Models;

namespace OrchardCore.Users.Services
{
    public class MembershipService : IMembershipService
    {
        private readonly UserManager<IUser> _userManager;
        private readonly IUserClaimsPrincipalFactory<IUser> _claimsPrincipalFactory;

        public MembershipService(
            IUserClaimsPrincipalFactory<IUser> claimsPrincipalFactory,
            UserManager<IUser> userManager)
        {
            _claimsPrincipalFactory = claimsPrincipalFactory;
            _userManager = userManager;
        }

        public async Task<bool> CheckPasswordAsync(string userName, string password)
        {
            var user = await _userManager.FindByNameAsync(userName);

            if (user == null)
            {
                return false;
            }

            return await _userManager.CheckPasswordAsync(user, password);
        }

        public async Task<IUser> GetUserAsync(string userName)
        {
            var user = await _userManager.FindByNameAsync(userName);

            return user;
        }

        public Task<ClaimsPrincipal> CreateClaimsPrincipal(IUser user)
        {
            return _claimsPrincipalFactory.CreateAsync(user as User);
        }
    }
}
