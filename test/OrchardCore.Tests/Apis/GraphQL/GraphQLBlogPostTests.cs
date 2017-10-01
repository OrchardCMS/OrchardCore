using System;
using System.Threading.Tasks;
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
            var query = @"query { contentitems(contentType: ""Blog"") { id } }";

            var response = await _siteContext.Site.Client.GetAsync("graphql?query="+ query);

            Assert.True(response.IsSuccessStatusCode);
            Assert.NotEmpty(JObject.Parse(response.Content.ReadAsStringAsync().Result)["data"]["contentitems"].First["id"].ToString());
        }



    }
}
