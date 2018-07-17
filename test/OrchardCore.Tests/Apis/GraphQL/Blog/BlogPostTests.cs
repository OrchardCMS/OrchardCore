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
        private static bool _initialize;

        static BlogPostTests()
        {
        }

        [Fact]
        public async Task ShouldListAllBlogs()
        {
            var initialize = false;

            if (!_initialize)
            {
                lock (_sync)
                {
                    if (!_initialize)
                    {

                        initialize = true;
                        _initialize = true;
                    }
                }
            }

            if (initialize)
            {
                var context = new BlogContext();
                context.Initialize();
                await context.InitializeAsync();
                _context = context;
            }

            while (_context == null)
            {
                await Task.Delay(5000);
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
            var initialize = false;

            if (!_initialize)
            {
                lock (_sync)
                {
                    if (!_initialize)
                    {

                        initialize = true;
                        _initialize = true;
                    }
                }
            }

            if (initialize)
            {
                var context = new BlogContext();
                context.Initialize();
                await context.InitializeAsync();
                _context = context;
            }

            while (_context == null)
            {
                await Task.Delay(5000);
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
            var initialize = false;

            if (!_initialize)
            {
                lock (_sync)
                {
                    if (!_initialize)
                    {

                        initialize = true;
                        _initialize = true;
                    }
                }
            }

            if (initialize)
            {
                var context = new BlogContext();
                context.Initialize();
                await context.InitializeAsync();
                _context = context;
            }

            while (_context == null)
            {
                await Task.Delay(5000);
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
            var initialize = false;

            if (!_initialize)
            {
                lock (_sync)
                {
                    if (!_initialize)
                    {

                        initialize = true;
                        _initialize = true;
                    }
                }
            }

            if (initialize)
            {
                var context = new BlogContext();
                context.Initialize();
                await context.InitializeAsync();
                _context = context;
            }

            while (_context == null)
            {
                await Task.Delay(5000);
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
