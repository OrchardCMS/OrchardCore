
using System;
using System.Threading.Tasks;
using Assent;
using OrchardCore.Tests.Apis.GraphQL.Blog;
using Xunit;

namespace OrchardCore.Tests.Apis.GraphQL
{
    public class BlogPostTests
    {
        private static Lazy<BlogContext> _context;

        static BlogPostTests()
        {
            _context = new Lazy<BlogContext>(() =>
            {
                var siteContext = new BlogContext();
                siteContext.InitializeAsync().GetAwaiter().GetResult();
                return siteContext;
            });
        }

        [Fact]
        public async Task ShouldListAllBlogs()
        {
            var result = await _context.Value
                .Client
                .Content
                .Query("Blog", builder => {
                    builder
                        .AddField("contentItemId");
                });

            Assert.Single(result["data"]["blog"].Children());
            Assert.NotEmpty(result["data"]["blog"].Children()["contentItemId"]);
        }

        [Fact]
        public async Task ShouldCreateBlogPost()
        {
            var blogPostContentItemId = await _context.Value
                .Client
                .Content
                .Create("BlogPost", builder =>
                {
                    builder
                        .WithContentPart("TitlePart")
                        .AddField("Title", "Some sorta blogpost!");

                    builder
                        .WithContentPart("ContainedPart")
                        .AddField("ListContentItemId", _context.Value.BlogContentItemId);
                });
            
            var result = await _context.Value
                .Client
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
            var blogPostContentItemId1 = await _context.Value
                .Client
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
                        .AddField("ListContentItemId", _context.Value.BlogContentItemId);
                });

            var blogPostContentItemId2 = await _context.Value
                .Client
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
                        .AddField("ListContentItemId", _context.Value.BlogContentItemId);
                });

            var result = await _context.Value
                .Client
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
            var blogPostContentItemId = await _context.Value
                .Client
                .Content
                .Create("BlogPost", builder =>
                {
                    builder
                        .WithContentPart("TitlePart")
                        .AddField("Title", "Some sorta blogpost!");

                    builder
                        .WithContentPart("ContainedPart")
                        .AddField("ListContentItemId", _context.Value.BlogContentItemId);
                });

            var result = await _context.Value
                .Client
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

            await _context.Value
                .Client
                .Content
                .Delete(blogPostContentItemId);

            var result2 = await _context.Value
                .Client
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
