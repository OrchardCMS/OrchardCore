using System.Security.Claims;
using System.Threading.Tasks;

namespace OrchardCore.Users.Services
{
    public interface IUserClaimsProvider
    {
        Task GenerateAsync(IUser user, ClaimsIdentity claims);
    }
}
