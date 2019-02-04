using System.Threading.Tasks;
using GraphQL;

namespace OrchardCore.Tests.Apis.Context
{
    public class BlogContext<TSiteStartup> : SiteContext<TSiteStartup> where TSiteStartup : SiteStartup
    {
        public string BlogContentItemId { get; private set; }

        public override string RecipeName => "Blog";

        public string BlogContentType => "Blog";
        public string BlogPostContentType => "BlogPost";

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();

            var result = await GraphQLClient
                .Content
                .Query(BlogContentType.ToCamelCase(), builder =>
                {
                    builder
                        .AddField("contentItemId");
                });

            BlogContentItemId = result["data"][BlogContentType.ToCamelCase()].First["contentItemId"].ToString();
        }
    }

    public class BlogContext : BlogContext<SiteStartup>
    {
    }
}
