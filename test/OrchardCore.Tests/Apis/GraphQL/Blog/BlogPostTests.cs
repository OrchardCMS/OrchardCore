using System;
using System.Linq;
using System.Threading.Tasks;
using GraphQL;
using GraphQL.Types;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Apis;
using OrchardCore.Autoroute.Model;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.GraphQL.Settings;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Lists.Models;
using OrchardCore.Recipes.Events;
using OrchardCore.Recipes.Models;
using OrchardCore.Tests.Apis.Context;
using Xunit;
using YesSql;

namespace OrchardCore.Tests.Apis.GraphQL.Blog
{
    public class BlogPostTests
    {
        [Fact]
        public async Task ShouldListAllBlogs()
        {
            using (var context = new BlogContext())
            {
                await context.InitializeAsync();

                var result = await context
                    .GraphQLClient
                    .Content
                    .Query(context.BlogContentType, builder =>
                    {
                        builder
                            .AddField("contentItemId");
                    });

                Assert.Single(result["data"][context.BlogContentType.ToCamelCase()].Children()["contentItemId"]
                    .Where(b => b.ToString() == context.BlogContentItemId));
            }
        }

        [Fact]
        public async Task ShouldQueryByBlogPostAutoroutePart()
        {
            using (var context = new BlogContext())
            {
                await context.InitializeAsync();

                var blogPostContentItemId1 = await context
                    .CreateContentItem(context.BlogPostContentType, builder =>
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
                                ListContentItemId = context.BlogContentItemId
                            });
                    });

                var blogPostContentItemId2 = await context
                    .CreateContentItem(context.BlogPostContentType, builder =>
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
                                ListContentItemId = context.BlogContentItemId
                            });
                    });

                var result = await context
                    .GraphQLClient
                    .Content
                    .Query(context.BlogPostContentType, builder =>
                    {
                        builder
                            .WithNestedQueryField("AutoroutePart", "path: \"Path1\"");

                        builder
                            .AddField("DisplayText");
                    });

                Assert.Equal(
                    "Some sorta blogpost!",
                    result["data"][context.BlogPostContentType.ToCamelCase()][0]["displayText"].ToString());
            }
        }

        [Fact]
        public async Task WhenThePartHasTheSameNameAsTheContentTypeShouldCollapseFieldsToContentType()
        {
            using (var context = new BlogContext())
            {
                await context.InitializeAsync();

                var result = await context
                    .GraphQLClient
                    .Content
                    .Query(context.BlogPostContentType, builder =>
                    {
                        builder.AddField("Subtitle");
                    });

                Assert.Equal(
                    "Problems look mighty small from 150 miles up",
                    result["data"][context.BlogPostContentType.ToCamelCase()][0]["subtitle"].ToString());
            }
        }

        [Fact]
        public async Task WhenCreatingABlogPostShouldBeAbleToPopulateField()
        {
            using (var context = new BlogContext())
            {
                await context.InitializeAsync();

                var blogPostContentItemId = await context
                    .CreateContentItem(context.BlogPostContentType, builder =>
                    {
                        builder
                            .DisplayText = "Some sorta blogpost!";

                        builder
                            .Weld(context.BlogPostContentType, new ContentPart());

                        builder
                            .Alter<ContentPart>(context.BlogPostContentType, (cp) =>
                            {
                                cp.Weld("Subtitle", new TextField());

                                cp.Alter<TextField>("Subtitle", tf =>
                                {
                                    tf.Text = "Hey - Is this working!?!?!?!?";
                                });
                            });

                        builder
                            .Weld(new ContainedPart
                            {
                                ListContentItemId = context.BlogContentItemId
                            });
                    });

                var result = await context
                    .GraphQLClient
                    .Content
                    .Query(context.BlogPostContentType, builder =>
                    {
                        builder
                            .WithQueryField("ContentItemId", blogPostContentItemId);

                        builder
                            .AddField("Subtitle");
                    });

                Assert.Equal(
                    "Hey - Is this working!?!?!?!?",
                    result["data"][context.BlogPostContentType.ToCamelCase()][0]["subtitle"].ToString());
            }
        }


        [Fact]
        public async Task ShouldQueryByStatus()
        {
            using (var context = new BlogContext())
            {
                await context.InitializeAsync();

                var draft = await context
                    .CreateContentItem(context.BlogPostContentType, builder =>
                    {
                        builder.DisplayText = "Draft blog post";
                        builder.Published = false;
                        builder.Latest = true;

                        builder
                            .Weld(new ContainedPart
                            {
                                ListContentItemId = context.BlogContentItemId
                            });
                    }, draft: true);

                var result = await context.GraphQLClient.Content
                    .Query("blogPost(status: PUBLISHED) { displayText, published }");

                Assert.Single(result["data"]["blogPost"]);
                Assert.Equal(true, result["data"][context.BlogPostContentType.ToCamelCase()][0]["published"]);

                result = await context.GraphQLClient.Content
                    .Query("blogPost(status: DRAFT) { displayText, published }");

                Assert.Single(result["data"]["blogPost"]);
                Assert.Equal(false, result["data"][context.BlogPostContentType.ToCamelCase()][0]["published"]);

                result = await context.GraphQLClient.Content
                    .Query("blogPost(status: LATEST) { displayText, published }");

                Assert.Equal(2, result["data"][context.BlogPostContentType.ToCamelCase()].Count());
            }
        }

        [Fact]
        public async Task ShouldGetNameFromBlogWhenNameIsCollapsedOnSameNamePart()
        {
            using (var context = new BlogContext<BlogSiteStartupWithRollup>())
            {
                await context.InitializeAsync();

                var contentItemId = await context
                    .CreateContentItem(context.BlogContentType, builder =>
                    {
                        builder
                            .DisplayText = "Little Carl likes to dance";

                        builder
                            .Weld(new BlogPart
                            {
                                SecondName = "Dancing with Greg"
                            });
                    });

                var result = await context
                    .GraphQLClient
                    .Content
                    .Query(context.BlogContentType, builder =>
                    {
                        builder
                            .WithQueryField("ContentItemId", contentItemId);

                        builder
                            .AddField("SecondName");
                    });

                Assert.Equal(
                    "Dancing with Greg",
                    result["data"][context.BlogContentType.ToCamelCase()][0]["secondName"].ToString());
            }
        }

        private class BlogSiteStartupWithRollup : SiteStartup
        {
            public BlogSiteStartupWithRollup()
            {
                Builder = (builder) =>
                {
                    builder.ConfigureServices((services) =>
                    {
                        services.AddSingleton<ContentPart, BlogPart>();
                        services.AddScoped<IRecipeEventHandler, AlterBlogStep>();
                        services.AddObjectGraphType<BlogPart, BlogPartQueryObjectType>();
                    }, 100);
                };
            }

            private class AlterBlogStep : IRecipeEventHandler
            {
                private readonly IServiceProvider _contentDefinitionManager;
                private readonly ISession _session;

                public AlterBlogStep(IServiceProvider contentDefinitionManager, ISession session)
                {
                    _contentDefinitionManager = contentDefinitionManager;
                    _session = session;
                }

                public Task ExecuteAsync(RecipeExecutionContext context)
                {
                    return Task.CompletedTask;
                }

                public Task ExecutionFailedAsync(string executionId, RecipeDescriptor descriptor)
                {
                    return Task.CompletedTask;
                }

                public Task RecipeExecutedAsync(string executionId, RecipeDescriptor descriptor)
                {
                    _contentDefinitionManager.GetService<IContentDefinitionManager>().AlterTypeDefinition("Blog", type => type
                        .WithPart(nameof(BlogPart), p => p
                            .WithPosition("10")
                            .WithSettings(
                              new GraphQLContentTypePartSettings
                              {
                                  Collapse = true
                              })));

                    return Task.CompletedTask;
                }

                public Task RecipeExecutingAsync(string executionId, RecipeDescriptor descriptor)
                {
                    return Task.CompletedTask;
                }

                public Task RecipeStepExecutedAsync(RecipeExecutionContext context)
                {
                    return Task.CompletedTask;
                }

                public Task RecipeStepExecutingAsync(RecipeExecutionContext context)
                {
                    return Task.CompletedTask;
                }
            }

            public class BlogPartQueryObjectType : ObjectGraphType<BlogPart>
            {
                public BlogPartQueryObjectType()
                {
                    Name = "BlogPart";
                    Description = "Test.";

                    Field(x => x.SecondName);
                }
            }
        }
    }
}
