
using System.Linq;
using System.Threading.Tasks;
using Assent;
using OrchardCore.Tests.Apis.GraphQL.Blog;
using Xunit;

namespace OrchardCore.Tests.Apis.GraphQL
{
    public class BlogPostTests
    {
        private BlogContext _context;

        public BlogPostTests()
        {
            _context = new BlogContext();
        }

        [Fact]
        public async Task ShouldListAllBlogs()
        {
            await _context.InitializeBlogAsync();

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
            await _context.InitializeBlogAsync();

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
                        .WithQueryField("contentItemId", blogPostContentItemId);

                    builder
                        .WithNestedField("TitlePart")
                        .AddField("Title");
                });

            this.Assent(result.ToString());
        }

        [Fact]
        public async Task ShouldQueryByBlogPostAutoroutePart()
        {
            await _context.InitializeBlogAsync();

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
            await _context.InitializeBlogAsync();

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
                        .WithQueryField("contentItemId", blogPostContentItemId);

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
                        .WithQueryField("contentItemId", blogPostContentItemId);

                    builder
                        .WithNestedField("TitlePart")
                        .AddField("Title");
                });


            Assert.False(result2["data"]["blogPost"].HasValues);
        }
    }
}
