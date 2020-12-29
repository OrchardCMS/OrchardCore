using System.Security.Claims;
using System.Threading.Tasks;

namespace OrchardCore.Users.Services
{
    public interface IUserClaimsProvider
    {
        Task<ClaimsIdentity> GenerateAsync(IUser user, ClaimsIdentity claims);
    }
}
