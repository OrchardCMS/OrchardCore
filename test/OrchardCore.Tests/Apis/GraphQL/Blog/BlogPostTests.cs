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

            this.Assent(result.ToString());
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
    }
}
