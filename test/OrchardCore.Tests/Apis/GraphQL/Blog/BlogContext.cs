using System.Threading.Tasks;
using OrchardCore.Tests.Apis.Context;

namespace OrchardCore.Tests.Apis.GraphQL.Blog
{
    public class BlogContext : SiteContext
    {
        public string BlogContentItemId { get; private set; }

        static BlogContext()
        {
        }

        public async Task InitializeAsync()
        {
            var result = await GraphQLClient
                .Content
                .Query("Blog", builder => {
                    builder
                        .AddField("contentItemId");
                });

            BlogContentItemId = result["data"]["blog"].First["contentItemId"].ToString();
        }
    }
}
