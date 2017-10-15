using OrchardCore.Tests.Apis.GraphQL.Context;

namespace OrchardCore.Tests.Apis.GraphQL.Blog
{
    public class BlogContext : SiteContext
    {
        public BlogContext() : base()
        {
            var result = Client
                .Content
                .QueryAsync("Blog", builder => {
                    builder
                        .AddField("contentItemId");
                })
                .GetAwaiter()
                .GetResult();

            BlogContentItemId = result["data"]["blog"].First["contentItemId"].ToString();
        }

        public string BlogContentItemId { get; }
    }
}
