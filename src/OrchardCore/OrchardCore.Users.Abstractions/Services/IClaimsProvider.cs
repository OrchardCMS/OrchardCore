using System.Security.Claims;
using System.Threading.Tasks;

namespace OrchardCore.Users.Services
{
    public interface IClaimsProvider
    {
        Task<ClaimsIdentity> GenerateAsync(IUser user, ClaimsIdentity claims);
    }
}
