using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace OrchardCore.Tests.Apis.GraphQL
{
    public class GraphQLBlogTests : IClassFixture<BlogSiteContext>
    {
        private BlogSiteContext _siteContext;

        public GraphQLBlogTests(BlogSiteContext siteContext)
        {
            _siteContext = siteContext;
        }

        [Fact]
        public async Task ShouldCreateABlog() {
            var titlePart = @"titlePart: { ""title"": ""Hi There"" }";

            var variables =
@"{ 
    ""contentItem"": {
        ""contentType"": ""Blog"", 
        ""contentParts"": " + JsonConvert.SerializeObject("{" + titlePart + "}") + @"
    }
}";

            var json = @"{
  ""query"": ""mutation ($contentItem: ContentItemInput!){ createContentItem(contentItem: $contentItem) { contentItemId } }"",
  ""variables"": " + JsonConvert.SerializeObject(variables) + @"}";

            var response = await _siteContext.Site.Client.PostJsonAsync("graphql", json);

            Assert.True(response.IsSuccessStatusCode);
            var result = JObject.Parse(await response.Content.ReadAsStringAsync());

            //"{\"data\":{\"createContentItem\":{\"id\":81}}}"
            Assert.NotEmpty(result["data"]["createContentItem"]["contentItemId"].ToString());
        }
    }
}
