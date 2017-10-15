namespace OrchardCore.Tests.Apis.GraphQL.Context
{
    public class SiteContext : TestContext
    {
        public SiteContext() : base()
        {
            Client
                .Tenants
                .CreateTenant(
                    "Test Site",
                    "Sqlite",
                    "admin",
                    "Password01_",
                    "Fu@bar.com",
                    "Blog"
                ).Wait();
        }
    }
}
