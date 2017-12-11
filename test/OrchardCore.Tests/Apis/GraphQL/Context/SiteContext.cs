using System.Threading.Tasks;

namespace OrchardCore.Tests.Apis.GraphQL.Context
{
    public class SiteContext : TestContext
    {
        public override Task InitializeAsync()
        {
            return Client
                .Tenants
                .CreateTenant(
                    "Test Site",
                    "Sqlite",
                    "admin",
                    "Password01_",
                    "Fu@bar.com",
                    "Blog"
                );
        }
    }
}
