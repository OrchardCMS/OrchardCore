using System.Security.Claims;
using System.Threading.Tasks;
using OrchardCore.Users;

namespace OrchardCore.Lucene.Services
{
    public interface ISearchPermissionService
    {
        Task<SearchPermissionResult> CheckPermission(string indexName, IUser user);

        Task<SearchPermissionResult> CheckPermission(string indexName, ClaimsPrincipal user);
    }
}
