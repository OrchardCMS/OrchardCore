using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace OrchardCore.Tests.Apis.GraphQL
{
    public class GraphQLBlogPostTests : IClassFixture<BlogSiteContext>
    {
        private BlogSiteContext _siteContext;

        public GraphQLBlogPostTests(BlogSiteContext siteContext)
        {
            _siteContext = siteContext;
        }

        [Fact]
        public async Task ShouldListAllBlogs()
        {
            var blogId = await CreateBlog();

            var query = @"query { contentitems(contentType: ""Blog"") { contentItemId } }";

            var response = await _siteContext.Site.Client.GetAsync("graphql?query="+ query);

            Assert.True(response.IsSuccessStatusCode);

            var content = await response.Content.ReadAsStringAsync();

            var contentItemIds = JObject.Parse(content)["data"]["contentitems"].Select(x => x.Value<string>("contentItemId"));
            Assert.Contains(blogId, contentItemIds);
        }

        [Fact]
        public async Task ShouldCreateBlogPost()
        {
            var blogId = await CreateBlog();

            var titlePart = @"titlePart: { ""title"": ""Hi There"" }";
            var containedPart = @"containedPart: { ""listContentItemId"": " + blogId + " }";

            var variables =
@"{ 
    ""contentItem"": {
        ""contentType"": ""BlogPost"", 
        ""contentParts"": " + JsonConvert.SerializeObject("{" + titlePart + "," + containedPart + "}") + @"
    }
}";

            var json = @"{
  ""query"": ""mutation ($contentItem: ContentItemInput!){ createContentItem(contentItem: $contentItem) { contentItemId } }"",
  ""variables"": " + JsonConvert.SerializeObject(variables) + @"}";

            var response = await _siteContext.Site.Client.PostJsonAsync("graphql", json);

            Assert.True(response.IsSuccessStatusCode);

            var result = JObject.Parse(await response.Content.ReadAsStringAsync());

            Assert.NotEmpty(result["data"]["createContentItem"]["contentItemId"].ToString());
        }

        [Fact]
        public async Task ShouldByAbleToGetTitlePartOnBlogPost()
        {
            var blogId = await CreateBlog();

            var blogPostId = await CreateBlogPost(blogId, "Hi There");

            var query = HttpUtility.UrlEncode("query { blog(id: \""+ blogId + "\") { titlePart { title } } }");
            var response = await _siteContext.Site.Client.GetAsync("graphql?query=" + query);

            Assert.True(response.IsSuccessStatusCode);

            var result = JObject.Parse(await response.Content.ReadAsStringAsync());

            Assert.Equal("Hi There", result["data"]["createContentItem"]["titlepart"]["title"].ToString());
        }

        public async Task<string> CreateBlog()
        {
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

            var result = JObject.Parse(await response.Content.ReadAsStringAsync());

            return result["data"]["createContentItem"]["contentItemId"].ToString();
        }

        public async Task<string> CreateBlogPost(string blogId, string title) {
            var titlePart = @"titlePart: { ""title"": """ + title + @""" }";
            var containedPart = @"containedPart: { ""listContentItemId"": """ + blogId + @""" }";

            var variables =
@"{ 
    ""contentItem"": {
        ""contentType"": ""BlogPost"", 
        ""contentParts"": " + JsonConvert.SerializeObject("{" + titlePart + "," + containedPart + "}") + @"
    }
}";

            var json = @"{
  ""query"": ""mutation ($contentItem: ContentItemInput!){ createContentItem(contentItem: $contentItem) { contentItemId } }"",
  ""variables"": " + JsonConvert.SerializeObject(variables) + @"}";

            var response = await _siteContext.Site.Client.PostJsonAsync("graphql", json);

            Assert.True(response.IsSuccessStatusCode);

            var result = JObject.Parse(await response.Content.ReadAsStringAsync());

            return result["data"]["createContentItem"]["contentItemId"].ToString();
        }
    }
}
