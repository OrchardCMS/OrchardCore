using System.Linq;
using System.Threading.Tasks;
using Assent;
using OrchardCore.Autoroute.Model;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentManagement;
using OrchardCore.Lists.Models;
using OrchardCore.Tests.Apis.Context;
using Xunit;

namespace OrchardCore.Tests.Apis.GraphQL.Blog
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
        public async Task ShouldQueryByBlogPostAutoroutePart()
        {
            await BlogContext.InitializeBlogAsync();

            var blogPostContentItemId1 = await BlogContext
                .CreateContentItem("BlogPost", builder =>
                {
                    builder
                        .DisplayText = "Some sorta blogpost!";

                    builder
                        .Weld(new AutoroutePart
                        {
                            Path = "Path1"
                        });

                    builder
                        .Weld(new ContainedPart
                        {
                            ListContentItemId = BlogContext.BlogContentItemId
                        });
                });

            var blogPostContentItemId2 = await BlogContext
                .CreateContentItem("BlogPost", builder =>
                {
                    builder
                        .DisplayText = "Some sorta other blogpost!";

                    builder
                        .Weld(new AutoroutePart
                        {
                            Path = "Path2"
                        });

                    builder
                        .Weld(new ContainedPart
                        {
                            ListContentItemId = BlogContext.BlogContentItemId
                        });
                });

            var result = await BlogContext
                .GraphQLClient
                .Content
                .Query("BlogPost", builder =>
                {
                    builder
                        .WithNestedQueryField("AutoroutePart", "path: \"Path1\"");

                    builder
                        .AddField("DisplayText");
                });
            
            this.Assent(result.ToString());
        }

        [Fact]
        public async Task WhenThePartHasTheSameNameAsTheContentTypeShouldCollapseFieldsToContentType()
        {
            await BlogContext.InitializeBlogAsync();

            var result = await BlogContext
                .GraphQLClient
                .Content
                .Query("BlogPost", builder =>
                {
                    builder.AddField("Subtitle");
                });

            Assert.Equal(
                "Problems look mighty small from 150 miles up",
                result["data"]["blogPost"][0]["subtitle"].ToString());
        }

        [Fact]
        public async Task WhenCreatingABlogPostShouldBeAbleToPopulateField()
        {
            await BlogContext.InitializeBlogAsync();

            var blogPostContentItemId = await BlogContext
                .CreateContentItem("BlogPost", builder =>
                {
                    builder
                        .DisplayText = "Some sorta blogpost!";

                    builder
                        .Weld("BlogPost", new ContentPart());

                    builder
                        .Alter<ContentPart>("BlogPost", (cp) => {
                            cp.Weld("Subtitle", new TextField());

                            cp.Alter<TextField>("Subtitle", tf =>
                            {
                                tf.Text = "Hey - Is this working!?!?!?!?";
                            });
                        });

                    builder
                        .Weld(new ContainedPart
                        {
                            ListContentItemId = BlogContext.BlogContentItemId
                        });
                });

            var result = await BlogContext
                .GraphQLClient
                .Content
                .Query("BlogPost", builder =>
                {
                    builder
                        .WithQueryField("ContentItemId", blogPostContentItemId);

                    builder
                        .AddField("Subtitle");
                });

            Assert.Equal(
                "Hey - Is this working!?!?!?!?",
                result["data"]["blogPost"][0]["subtitle"].ToString());
        }
    }
}
