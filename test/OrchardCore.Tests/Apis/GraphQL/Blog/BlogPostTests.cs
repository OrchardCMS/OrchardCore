using System.Threading.Tasks;
using Assent;
using OrchardCore.Tests.Apis.GraphQL.Blog;
using Xunit;

namespace OrchardCore.Tests.Apis.GraphQL
{
    public class BlogPostTests : IClassFixture<BlogContext>
    {
        private BlogContext _context;

        public BlogPostTests(BlogContext context)
        {
            _context = context;
        }

        [Fact]
        public async Task ShouldListAllBlogs()
        {
            var result = await _context
                .Client
                .Content
                .QueryAsync("Blog", builder => {
                    builder
                        .AddField("contentItemId");
                });

            Assert.Single(result["data"]["blog"].Children());
            Assert.NotEmpty(result["data"]["blog"].Children()["contentItemId"]);



        }

        [Fact]
        public async Task ShouldCreateBlogPost()
        {
            var blogPostContentItemId = await _context
                .Client
                .Content
                .CreateAsync("BlogPost", builder =>
                {
                    builder
                        .WithContentPart("TitlePart")
                        .AddField("Title", "Some sorta blogpost!");

                    builder
                        .WithContentPart("ContainedPart")
                        .AddField("ListContentItemId", _context.BlogContentItemId);
                });
            
            await _context
                .Client
                .Content
                .QueryAsync("BlogPost", builder =>
                {
                    builder
                        .WithQueryField("contentItemId", blogPostContentItemId);

                    builder
                        .WithNestedField("TitlePart")
                        .AddField("Title");
                });
            
            var result = await _context
                .Client
                .Content
                .QueryAsync("BlogPost", builder =>
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
            var blogPostContentItemId1 = await _context
                .Client
                .Content
                .CreateAsync("BlogPost", builder =>
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
                .CreateAsync("BlogPost", builder =>
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
                .QueryAsync("BlogPost", builder =>
                {
                    builder
                        .WithQueryField("AutoroutePart", "{ path: \"Path1\" }");

                    builder
                        .WithNestedField("TitlePart")
                        .AddField("Title");
                });
            //body = @"query { blogPost( autoroutePart: ""{path: \""Path1\""}"" ) { titlePart { title } } }"
            this.Assent(result.ToString());
        }
    }
}
