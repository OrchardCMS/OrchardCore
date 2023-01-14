using OrchardCore.Apis.GraphQL.Client;
using OrchardCore.ContentManagement;

namespace OrchardCore.Tests.Apis.Context
{
    public class BlogPostApiControllerContext : SiteContext
    {
        public string BlogContentItemId { get; private set; }
        public ContentItem BlogPost { get; private set; }
        public string CategoriesTaxonomyContentItemId { get; private set; }
        public string TagsTaxonomyContentItemId { get; private set; }

        static BlogPostApiControllerContext()
        {
        }

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();

            var body = new ContentTypeQueryResourceBuilder("blogPost")
                    .WithField("contentItemId").Build() +
                 new ContentTypeQueryResourceBuilder("blog")
                    .WithField("contentItemId").Build() +
                 new ContentTypeQueryResourceBuilder("taxonomy")
                    .WithField("contentItemId").Build();

            var result = await GraphQLClient.Content.Query(body);

            var blogPostContentItemId = result["data"]["blogPost"].First["contentItemId"].ToString();

            var content = await Client.GetAsync($"api/content/{blogPostContentItemId}");
            BlogPost = await content.Content.ReadAsAsync<ContentItem>();

            BlogContentItemId = result["data"]["blog"].First["contentItemId"].ToString();

            TagsTaxonomyContentItemId = result["data"]["taxonomy"][1]["contentItemId"].ToString();
        }
    }
}
