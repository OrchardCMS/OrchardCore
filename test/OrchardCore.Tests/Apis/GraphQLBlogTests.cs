using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.FunctionalTests;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OrchardCore.Tests.Apis.Sources;
using Xunit;

namespace OrchardCore.Tests.Apis
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
            var siteName = Guid.NewGuid().ToString().Replace("-", "");

            var titlePart = @"titlePart: { ""title"": ""Hi There"" }";


            var variables =
@"{ 
    ""contentItem"": {
        ""contentType"": ""Blog"", 
        ""contentParts"": " + JsonConvert.SerializeObject("{" + titlePart + "}") + @"
    }
}";

            var json = @"{
  ""query"": ""mutation ($contentItem: ContentItemInput!){ createContentItem(contentItem: $contentItem) { id } }"",
  ""variables"": " + JsonConvert.SerializeObject(variables) + @"}";

            var response = await _siteContext.Site.Client.PostJsonAsync("graphql", json);

            response.EnsureSuccessStatusCode();
        }

    }
}
