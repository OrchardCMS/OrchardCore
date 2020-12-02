using System.Threading.Tasks;

namespace OrchardCore.Tests.Apis.Context
{
    public class BlogContext : SiteContext
    {
        public string BlogContentItemId { get; private set; }

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();

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
