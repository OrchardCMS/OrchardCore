using System.Threading.Tasks;

namespace OrchardCore.Tests.Apis.Context
{
    public class BlogContext : SiteContext
    {
        public string BlogContentItemId { get; private set; }

        public override async Task InitializeAsync(string databaseProvider, string connectionString, PermissionsContext permissionsContext = null)
        {
            await base.InitializeAsync(databaseProvider, connectionString);

            var result = await GraphQLClient
                .Content
                .Query("blog", builder =>
                {
                    builder
                        .WithField("contentItemId");
                });

            BlogContentItemId = result["data"]["blog"].First["contentItemId"].ToString();
        }
    }
}
