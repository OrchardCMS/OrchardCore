using System.Threading.Tasks;
using Assent;
using OrchardCore.Tests.Apis.GraphQL.Blog;
using Xunit;

namespace OrchardCore.Tests.Apis.GraphQL
{
    public class BlogPostTests
    {
        private static BlogContext _context;
        private static object _sync = new object();

        static BlogPostTests()
        {
        }

        [Fact]
        public async Task ShouldListAllBlogs()
        {
            if (_context == null)
            {
                lock (_sync)
                {
                    if (_context == null)
                    {
                        var context = new BlogContext();
                        context.InitializeAsync().GetAwaiter().GetResult();
                        _context = context;
                    }
                }
            }

            var result = await _context
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
            if (_context == null)
            {
                lock (_sync)
                {
                    if (_context == null)
                    {
                        var context = new BlogContext();
                        context.InitializeAsync().GetAwaiter().GetResult();
                        _context = context;
                    }
                }
            }

            var blogPostContentItemId = await _context
                .Client
                .Content
                .Create("BlogPost", builder =>
                {
                    builder
                        .WithContentPart("TitlePart")
                        .AddField("Title", "Some sorta blogpost!");

                    builder
                        .WithContentPart("ContainedPart")
                        .AddField("ListContentItemId", _context.BlogContentItemId);
                });
            
            var result = await _context
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
            if (_context == null)
            {
                lock (_sync)
                {
                    if (_context == null)
                    {
                        var context = new BlogContext();
                        context.InitializeAsync().GetAwaiter().GetResult();
                        _context = context;
                    }
                }
            }

            var blogPostContentItemId1 = await _context
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
                        .AddField("ListContentItemId", _context.BlogContentItemId);
                });

            var blogPostContentItemId2 = await _context
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
                        .AddField("ListContentItemId", _context.BlogContentItemId);
                });

            var result = await _context
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
            if (_context == null)
            {
                lock (_sync)
                {
                    if (_context == null)
                    {
                        var context = new BlogContext();
                        context.InitializeAsync().GetAwaiter().GetResult();
                        _context = context;
                    }
                }
            }

            var blogPostContentItemId = await _context
                .Client
                .Content
                .Create("BlogPost", builder =>
                {
                    builder
                        .WithContentPart("TitlePart")
                        .AddField("Title", "Some sorta blogpost!");

                    builder
                        .WithContentPart("ContainedPart")
                        .AddField("ListContentItemId", _context.BlogContentItemId);
                });

            var result = await _context
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

            await _context
                .Client
                .Content
                .Delete(blogPostContentItemId);

            var result2 = await _context
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
