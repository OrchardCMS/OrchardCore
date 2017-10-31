using System.Threading.Tasks;

namespace OrchardCore.Apis.Client.Abstractions
{
    public interface ITenantResource
    {
        Task<string> CreateTenant(
               string siteName,
               string databaseProvider,
               string userName,
               string password,
               string email,
               string recipeName);
    }
}
