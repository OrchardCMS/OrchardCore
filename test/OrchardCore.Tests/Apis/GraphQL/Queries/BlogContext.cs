using System.Threading.Tasks;
using OrchardCore.Tests.Apis.GraphQL.Context;

namespace OrchardCore.Tests.Apis.GraphQL.Queries
{
    public class BlogContext : SiteContext
    {
        public string BlogContentItemId { get; private set; }

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();

            var result = await Client
                .Content
                .Query("Blog", builder => {
                    builder
                        .AddField("contentItemId");
                });

            BlogContentItemId = result["data"]["blog"].First["contentItemId"].ToString();
        }
    }
}
