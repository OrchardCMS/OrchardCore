using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.FunctionalTests;
using OrchardCore.Apis.GraphQL.Client;
using OrchardCore.Apis.JsonApi.Client;

namespace OrchardCore.Tests.Apis.Context
{
    public class SiteContext
    {
        private static bool _initialized;
        private static bool _initializing;
        private static object _sync = new object();

        public static OrchardTestFixture<SiteStartup> Site { get; }
        public static OrchardGraphQLClient GraphQLClient { get; }
        public static OrchardJsonApiClient JsonApiClient { get; }

        static SiteContext()
        {
            Site = new OrchardTestFixture<SiteStartup>();
            Site.ShellsContainerName = "Sites_ApiTests";
            GraphQLClient = new OrchardGraphQLClient(Site.CreateClient());
            JsonApiClient = new OrchardJsonApiClient(Site.CreateClient());
        }

        public async Task InitializeSiteAsync()
        {
            var initialize = false;

            if (!_initialized && !_initializing)
            {
                lock (_sync)
                {
                    if (!_initialized && !_initializing)
                    {
                        initialize = true;
                        _initializing = true;
                    }
                }
            }

            if (initialize)
            {
                await GraphQLClient
                    .Tenants
                    .CreateTenant(
                        "Test Site",
                        "Sqlite",
                        "admin",
                        "Password01_",
                        "Fu@bar.com",
                        "Blog"
                    );

                _initialized = true;
            }

            while (!_initialized)
            {
                await Task.Delay(5000);
            }
        }
    }
}
