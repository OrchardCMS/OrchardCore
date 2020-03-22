using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.Environment.Shell;

namespace OrchardCore.Tests.Apis.Context
{
    public class BlogPostApiControllerContext : SiteContext
    {
        public static IShellHost ShellHost { get; private set; }
        public string BlogContentItemId { get; private set; }
        public ContentItem BlogPost { get; private set; }

        static BlogPostApiControllerContext()
        {
            ShellHost = Site.Services.GetRequiredService<IShellHost>();
        }

        public override async Task InitializeAsync(PermissionsContext permissionsContext = null)
        {
            await base.InitializeAsync();

            var blogPostResult = await GraphQLClient
                .Content
                .Query("blogPost", builder =>
                {
                    builder
                        .WithField("contentItemId");
                });

            var blogPostContentItemId = blogPostResult["data"]["blogPost"].First["contentItemId"].ToString();

            var content = await Client.GetAsync($"api/content/{blogPostContentItemId}");
            BlogPost = await content.Content.ReadAsAsync<ContentItem>();

            var blogResult = await GraphQLClient
                .Content
                .Query("blog", builder =>
                {
                    builder
                        .WithField("contentItemId");
                });

            BlogContentItemId = blogResult["data"]["blog"].First["contentItemId"].ToString();
        }
    }
}
