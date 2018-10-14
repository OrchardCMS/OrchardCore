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
                        .WithContentPart("TitlePart")
                        .AddField("Title", "Some sorta blogpost!");

                    builder
                        .WithContentPart("ContainedPart")
                        .AddField("ListContentItemId", BlogContext.BlogContentItemId);
                });
            
            var result = await BlogContext
                .GraphQLClient
                .Content
                .Query("BlogPost", builder =>
                {
                    builder
                        .WithQueryField("ContentItemId", blogPostContentItemId);

                    builder
                        .WithNestedField("TitlePart")
                        .AddField("Title");
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
                        .WithContentPart("TitlePart")
                        .AddField("Title", "Some sorta blogpost!");

                    builder
                        .WithContentPart("AutoroutePart")
                        .AddField("Path", "Path1");

                    builder
                        .WithContentPart("ContainedPart")
                        .AddField("ListContentItemId", BlogContext.BlogContentItemId);
                });

            var blogPostContentItemId2 = await BlogContext
                .GraphQLClient
                .Content
                .Create("BlogPost", builder =>
                {
                    builder
                        .WithContentPart("TitlePart")
                        .AddField("Title", "Some sorta other blogpost!");

                    builder
                        .WithContentPart("AutoroutePart")
                        .AddField("Path", "Path2");

                    builder
                        .WithContentPart("ContainedPart")
                        .AddField("ListContentItemId", BlogContext.BlogContentItemId);
                });

            var result = await BlogContext
                .GraphQLClient
                .Content
                .Query("BlogPost", builder =>
                {
                    builder
                        .WithNestedQueryField("AutoroutePart", "path: \"Path1\"");

                    builder
                        .WithNestedField("TitlePart")
                        .AddField("Title");
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
                        .WithContentPart("TitlePart")
                        .AddField("Title", "Some sorta blogpost!");

                    builder
                        .WithContentPart("ContainedPart")
                        .AddField("ListContentItemId", BlogContext.BlogContentItemId);
                });

            var result = await BlogContext
                .GraphQLClient
                .Content
                .Query("BlogPost", builder =>
                {
                    builder
                        .WithQueryField("ContentItemId", blogPostContentItemId);

                    builder
                        .WithNestedField("TitlePart")
                        .AddField("Title");
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
                        .WithQueryField("ContentItemId", blogPostContentItemId);

                    builder
                        .WithNestedField("TitlePart")
                        .AddField("Title");
                });


            Assert.False(result2["data"]["blogPost"].HasValues);
        }

        [Fact]
        public async Task ShouldGetFieldValueOffPart()
        {
            await BlogContext.InitializeBlogAsync();

            var result = await BlogContext
                .GraphQLClient
                .Content
                .Query("BlogPost", builder => {
                    builder
                        .WithNestedField("BlogPost")
                        .WithNestedField("Subtitle")
                        .AddField("Text");
                });

            Assert.Equal(
                "Problems look mighty small from 150 miles up",
                result["data"]["blogPost"][0]["blogPost"]["subtitle"]["text"].ToString());
        }
    }
}
