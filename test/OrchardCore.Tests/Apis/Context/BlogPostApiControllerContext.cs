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

            var blogPostContentItemId = result.GetNode("data", "blogPost", 0).GetContentItemId();

            var content = await Client.GetAsync($"api/content/{blogPostContentItemId}");
            BlogPost = await content.Content.ReadAsAsync<ContentItem>();

            BlogContentItemId = result.GetNode("data", "blog", 0).GetContentItemId();

            TagsTaxonomyContentItemId = result.GetNode("data", "taxonomy", 1).GetContentItemId();
        }
    }
}
