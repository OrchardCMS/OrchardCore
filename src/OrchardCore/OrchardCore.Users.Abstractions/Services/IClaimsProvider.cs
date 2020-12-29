using System.Security.Claims;
using System.Threading.Tasks;

namespace OrchardCore.Users.Services
{
    public interface IClaimsProvider
    {
        Task GenerateAsync(IUser user, ClaimsIdentity claims);
    }
}
