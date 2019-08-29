using System;
using System.Threading.Tasks;
using OrchardCore.Autoroute.Model;
using OrchardCore.ContentManagement;
using OrchardCore.Tests.Apis.Context;
using Xunit;

namespace OrchardCore.Tests.Apis.GraphQL
{
    public class ContentTypeQueryResourceBuilderTests
    {
        
        [Fact]
        public async Task ShouldBeAbleToCreateMultiArgumentQuery()
        {
            using (var context = new BlogContext())
            {
                await context.InitializeAsync();

                await context
                    .CreateContentItem("BlogPost", builder =>
                    {
                        builder.Published = true;
                        builder.Latest = true;
                        builder.DisplayText = "It worked!";

                        builder
                            .Weld(new AutoroutePart
                            {
                                Path = "Path1"
                            });
                    }, draft: true);

                await context
                    .CreateContentItem("BlogPost", builder =>
                    {
                        builder.Published = true;
                        builder.Latest = true;
                        builder.DisplayText = "don't get this one";

                        builder
                            .Weld(new AutoroutePart
                            {
                                Path = "Path1"
                            });
                    });

                await context
                    .CreateContentItem("BlogPost", builder =>
                    {
                        builder.Published = true;
                        builder.Latest = true;
                        builder.DisplayText = "or this one";

                        builder
                            .Weld(new AutoroutePart
                            {
                                Path = "Path2"
                            });
                    }, draft: true);

                var result = await context
                    .GraphQLClient
                    .Content
                    .Query("blogPost", builder =>
                    {
                        builder.WithQueryStringArgument("where", "path", "Path1");
                        builder.WithQueryArgument("status", "DRAFT");
                        builder
                            .WithField("displayText");
                    });

                var nodes = result["data"]["blogPost"];

                Assert.Single(nodes);
                Assert.Equal("It worked!", nodes[0]["displayText"].ToString());
            }
        }

        [Fact]
        public async Task ShouldBeAbleToCreateMultiFieldArgument()
        {
            using (var context = new BlogContext())
            {
                await context.InitializeAsync();

                await context
                    .CreateContentItem("BlogPost", builder =>
                    {
                        builder.Published = true;
                        builder.Latest = true;
                        builder.DisplayText = "Great Blog";

                        builder
                            .Weld(new AutoroutePart
                            {
                                Path = "Path1"
                            });
                    });

                await context
                    .CreateContentItem("BlogPost", builder =>
                    {
                        builder.Published = true;
                        builder.Latest = true;
                        builder.DisplayText = "Another Great Blog";

                        builder
                            .Weld(new AutoroutePart
                            {
                                Path = "Path1"
                            });
                    });

                await context
                    .CreateContentItem("BlogPost", builder =>
                    {
                        builder.Published = true;
                        builder.Latest = true;
                        builder.DisplayText = "Great Blog";

                        builder
                            .Weld(new AutoroutePart
                            {
                                Path = "Path2"
                            });
                    });

                var result = await context
                    .GraphQLClient
                    .Content
                    .Query("blogPost", builder =>
                    {
                        builder.WithQueryStringArgument("where", "displayText", "Great Blog");
                        builder.WithQueryStringArgument("where", "path", "Path1");
                        builder
                            .WithField("displayText")
                            .WithField("path");
                    });

                var nodes = result["data"]["blogPost"];

                Assert.Single(nodes);
                Assert.Equal("Great Blog", nodes[0]["displayText"].ToString());
                Assert.Equal("Path1", nodes[0]["path"].ToString());
            }
        }

        [Fact]
        public async Task ShouldNotBeAbleToIncludeDuplicateArguments()
        {
            using (var context = new BlogContext())
            {
                await context.InitializeAsync();

                await Assert.ThrowsAsync<Exception>(async () => await context
                    .GraphQLClient
                    .Content
                    .Query("blogPost", builder =>
                    {
                        builder.WithQueryArgument("status", "PUBLISHED");
                        builder.WithQueryArgument("status", "DRAFT");
                    }));

                await Assert.ThrowsAsync<Exception>(async () => await context
                    .GraphQLClient
                    .Content
                    .Query("blogPost", builder =>
                    {
                        builder.WithQueryArgument("status", "PUBLISHED");
                        builder.WithQueryArgument("status", "field", "DRAFT");
                    }));

                await Assert.ThrowsAsync<Exception>(async () => await context
                    .GraphQLClient
                    .Content
                    .Query("blogPost", builder =>
                    {
                        builder.WithQueryArgument("status", "PUBLISHED");
                        builder.WithNestedQueryArgument("status", "field", "DRAFT");
                    }));

                await Assert.ThrowsAsync<Exception>(async () => await context
                    .GraphQLClient
                    .Content
                    .Query("blogPost", builder =>
                    {
                        builder.WithQueryArgument("status", "field", "DRAFT");
                        builder.WithQueryArgument("status", "PUBLISHED");
                    }));

                await Assert.ThrowsAsync<Exception>(async () => await context
                    .GraphQLClient
                    .Content
                    .Query("blogPost", builder =>
                    {
                        builder.WithNestedQueryArgument("status", "field", "DRAFT");
                        builder.WithQueryArgument("status", "PUBLISHED");
                    }));
            }
        }
    }
}
