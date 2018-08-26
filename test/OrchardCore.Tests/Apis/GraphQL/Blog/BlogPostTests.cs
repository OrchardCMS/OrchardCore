using System.Linq;
using System.Threading.Tasks;
using Assent;
using OrchardCore.Tests.Apis.Context;
using Xunit;

namespace OrchardCore.Tests.Apis.GraphQL
{
    public class BlogPostTests
    {
        [Fact]
        public async Task ShouldListAllBlogs()
        {
            await BlogContext.InitializeBlogAsync();

            var result = await BlogContext
                .GraphQLClient
                .Content
                .Query("Blog", builder => {
                    builder
                        .AddField("contentItemId");
                });

            Assert.Single(result["data"]["blog"].Children()["contentItemId"]
                .Where(b => b.ToString() == BlogContext.BlogContentItemId));
        }

        [Fact]
        public async Task ShouldCreateBlogPost()
        {
            await BlogContext.InitializeBlogAsync();

            var blogPostContentItemId = await BlogContext
                .GraphQLClient
                .Content
                .Create("BlogPost", builder =>
                {
                    builder
                        .WithContentPart("titlePart")
                        .AddField("title", "Some sorta blogpost!");

                    builder
                        .WithContentPart("containedPart")
                        .AddField("listContentItemId", BlogContext.BlogContentItemId);
                });
            
            var result = await BlogContext
                .GraphQLClient
                .Content
                .Query("BlogPost", builder =>
                {
                    builder
                        .WithQueryField("contentItemId", blogPostContentItemId);

                    builder
                        .WithNestedField("titlePart")
                        .AddField("title");
                });

            this.Assent(result.ToString());
        }

        [Fact]
        public async Task ShouldQueryByBlogPostAutoroutePart()
        {
            await BlogContext.InitializeBlogAsync();

            var blogPostContentItemId1 = await BlogContext
                .GraphQLClient
                .Content
                .Create("BlogPost", builder =>
                {
                    builder
                        .WithContentPart("titlePart")
                        .AddField("Title", "Some sorta blogpost!");

                    builder
                        .WithContentPart("autoroutePart")
                        .AddField("Path", "Path1");

                    builder
                        .WithContentPart("containedPart")
                        .AddField("listContentItemId", BlogContext.BlogContentItemId);
                });

            var blogPostContentItemId2 = await BlogContext
                .GraphQLClient
                .Content
                .Create("BlogPost", builder =>
                {
                    builder
                        .WithContentPart("titlePart")
                        .AddField("title", "Some sorta other blogpost!");

                    builder
                        .WithContentPart("autoroutePart")
                        .AddField("path", "Path2");

                    builder
                        .WithContentPart("containedPart")
                        .AddField("listContentItemId", BlogContext.BlogContentItemId);
                });

            var result = await BlogContext
                .GraphQLClient
                .Content
                .Query("BlogPost", builder =>
                {
                    builder
                        .WithNestedQueryField("autoroutePart", "path: \"Path1\"");

                    builder
                        .WithNestedField("titlePart")
                        .AddField("title");
                });
            
            this.Assent(result.ToString());
        }

        [Fact]
        public async Task ShouldDeleteBlogPost()
        {
            await BlogContext.InitializeBlogAsync();

            var blogPostContentItemId = await BlogContext
                .GraphQLClient
                .Content
                .Create("BlogPost", builder =>
                {
                    builder
                        .WithContentPart("titlePart")
                        .AddField("title", "Some sorta blogpost!");

                    builder
                        .WithContentPart("containedPart")
                        .AddField("listContentItemId", BlogContext.BlogContentItemId);
                });

            var result = await BlogContext
                .GraphQLClient
                .Content
                .Query("BlogPost", builder =>
                {
                    builder
                        .WithQueryField("contentItemId", blogPostContentItemId);

                    builder
                        .WithNestedField("titlePart")
                        .AddField("title");
                });

            Assert.True(result["data"]["blogPost"].HasValues);

            await BlogContext
                .GraphQLClient
                .Content
                .Delete(blogPostContentItemId);

            var result2 = await BlogContext
                .GraphQLClient
                .Content
                .Query("BlogPost", builder =>
                {
                    builder
                        .WithQueryField("contentItemId", blogPostContentItemId);

                    builder
                        .WithNestedField("titlePart")
                        .AddField("title");
                });


            Assert.False(result2["data"]["blogPost"].HasValues);
        }
    }
}
