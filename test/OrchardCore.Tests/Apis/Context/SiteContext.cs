using System.Threading.Tasks;
using OrchardCore.Apis.GraphQL.Client;
using OrchardCore.Apis.JsonApi.Client;

namespace OrchardCore.Tests.Apis.Context
{
    public class SiteContext
    {
        private static Task _initialize;
        public static OrchardTestFixture<SiteStartup> Site { get; }
        public static OrchardGraphQLClient GraphQLClient { get; }
        public static OrchardJsonApiClient JsonApiClient { get; }
        public static Task InitializeSiteAsync() => _initialize;

        static SiteContext()
        {
            Site = new OrchardTestFixture<SiteStartup>();
            GraphQLClient = new OrchardGraphQLClient(Site.CreateClient());
            JsonApiClient = new OrchardJsonApiClient(Site.CreateClient());
            _initialize = InitializeAsync();
        }

        private static Task InitializeAsync()
        {
            return GraphQLClient
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
