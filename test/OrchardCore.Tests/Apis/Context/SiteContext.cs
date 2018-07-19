using System;
using Microsoft.AspNetCore.Mvc.FunctionalTests;
using OrchardCore.Apis.GraphQL.Client;
using OrchardCore.Apis.JsonApi.Client;

namespace OrchardCore.Tests.Apis.Context
{
    public class SiteContext
    {
        public static OrchardTestFixture<SiteStartup> Site { get; }
        public static OrchardGraphQLClient GraphQLClient { get; }
        public static OrchardJsonApiClient JsonApiClient { get; }

        static SiteContext()
        {
            Site = new OrchardTestFixture<SiteStartup>();
            Site.ShellsContainerName = "Sites_ApiTests";

            var client = Site.CreateClient();
            client.Timeout = TimeSpan.FromMinutes(10);
            GraphQLClient = new OrchardGraphQLClient(client);

            client = Site.CreateClient();
            client.Timeout = TimeSpan.FromMinutes(10);
            JsonApiClient = new OrchardJsonApiClient(client);

            CreateTenant();
        }

        public static void CreateTenant()
        {
            GraphQLClient
                .Tenants
                .CreateTenant(
                    "Test Site",
                    "Sqlite",
                    "admin",
                    "Password01_",
                    "Fu@bar.com",
                    "Blog"
                )
                .GetAwaiter()
                .GetResult();
        }
    }
}
