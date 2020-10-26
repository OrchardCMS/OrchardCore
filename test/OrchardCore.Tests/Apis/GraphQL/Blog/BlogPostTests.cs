using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Autoroute.Models;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentManagement;
using OrchardCore.Lists.Models;
using OrchardCore.Tests.Apis.Context;
using OrchardCore.Tests.Apis.Context.Attributes;
using Xunit;
using GraphQLApi = OrchardCore.Apis.GraphQL;

namespace OrchardCore.Tests.Apis.GraphQL
{
    public class BlogPostTests
    {
        [Theory]
        [SqliteData]
        [SqlServerData]
        [MySqlData]
        [PostgreSqlData]
        public async Task ShouldListAllBlogs(string databaseProvider, string connectionString)
        {
            using (var context = new BlogContext())
            {
                await context.InitializeAsync(databaseProvider, connectionString);

                var result = await context
                    .GraphQLClient
                    .Content
                    .Query("Blog", builder =>
                    {
                        builder
                            .WithField("contentItemId");
                    });

                Assert.Single(result["data"]["blog"].Children()["contentItemId"]
                    .Where(b => b.ToString() == context.BlogContentItemId));
            }
        }

        [Theory]
        [SqliteData]
        [SqlServerData]
        [MySqlData]
        [PostgreSqlData]
        public async Task ShouldQueryByBlogPostAutoroutePart(string databaseProvider, string connectionString)
        {
            using (var context = new BlogContext())
            {
                await context.InitializeAsync(databaseProvider, connectionString);

                var blogPostContentItemId1 = await context
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
                                ListContentItemId = context.BlogContentItemId
                            });
                    });

                var blogPostContentItemId2 = await context
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
                                ListContentItemId = context.BlogContentItemId
                            });
                    });

                var result = await context
                    .GraphQLClient
                    .Content
                    .Query("BlogPost", builder =>
                    {
                        builder
                            .WithQueryStringArgument("where", "path", "Path1");

                        builder
                            .WithField("DisplayText");
                    });

                Assert.Equal(
                    "Some sorta blogpost!",
                    result["data"]["blogPost"][0]["displayText"].ToString());
            }
        }

        [Theory]
        [SqliteData]
        [SqlServerData]
        [MySqlData]
        [PostgreSqlData]
        public async Task WhenThePartHasTheSameNameAsTheContentTypeShouldCollapseFieldsToContentType(string databaseProvider, string connectionString)
        {
            using (var context = new BlogContext())
            {
                await context.InitializeAsync(databaseProvider, connectionString);

                var result = await context
                    .GraphQLClient
                    .Content
                    .Query("BlogPost", builder =>
                    {
                        builder.WithField("Subtitle");
                    });

                Assert.Equal(
                    "Problems look mighty small from 150 miles up",
                    result["data"]["blogPost"][0]["subtitle"].ToString());
            }
        }

        [Theory]
        [SqliteData]
        [SqlServerData]
        [MySqlData]
        [PostgreSqlData]
        public async Task WhenCreatingABlogPostShouldBeAbleToPopulateField(string databaseProvider, string connectionString)
        {
            using (var context = new BlogContext())
            {
                await context.InitializeAsync(databaseProvider, connectionString);

                var blogPostContentItemId = await context
                    .CreateContentItem("BlogPost", builder =>
                    {
                        builder
                            .DisplayText = "Some sorta blogpost!";

                        builder
                            .Weld("BlogPost", new ContentPart());

                        builder
                            .Alter<ContentPart>("BlogPost", (cp) =>
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
                    .Query("BlogPost", builder =>
                    {
                        builder
                            .WithQueryStringArgument("where", "ContentItemId", blogPostContentItemId);

                        builder
                            .WithField("Subtitle");
                    });

                Assert.Equal(
                    "Hey - Is this working!?!?!?!?",
                    result["data"]["blogPost"][0]["subtitle"].ToString());
            }
        }

        [Theory]
        [SqliteData]
        [SqlServerData]
        [MySqlData]
        [PostgreSqlData]
        public async Task ShouldQueryByStatus(string databaseProvider, string connectionString)
        {
            using (var context = new BlogContext())
            {
                await context.InitializeAsync(databaseProvider, connectionString);

                var draft = await context
                    .CreateContentItem("BlogPost", builder =>
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
                Assert.Equal(true, result["data"]["blogPost"][0]["published"]);

                result = await context.GraphQLClient.Content
                    .Query("blogPost(status: DRAFT) { displayText, published }");

                Assert.Single(result["data"]["blogPost"]);
                Assert.Equal(false, result["data"]["blogPost"][0]["published"]);

                result = await context.GraphQLClient.Content
                    .Query("blogPost(status: LATEST) { displayText, published }");

                Assert.Equal(2, result["data"]["blogPost"].Count());
            }
        }

        [Theory]
        [SqliteData]
        [SqlServerData]
        [MySqlData]
        [PostgreSqlData]
        public async Task ShouldNotBeAbleToExecuteAnyQueriesWithoutPermission(string databaseProvider, string connectionString)
        {
            using (var context = new SiteContext())
            {
                await context.InitializeAsync(databaseProvider, connectionString, new PermissionsContext { UsePermissionsContext = true });

                var response = await context.GraphQLClient.Client.GetAsync("api/graphql");
                Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
            }
        }

        [Theory]
        [SqliteData]
        [SqlServerData]
        [MySqlData]
        [PostgreSqlData]
        public async Task ShouldReturnBlogsWithViewBlogContentPermission(string databaseProvider, string connectionString)
        {
            var permissionContext = new PermissionsContext
            {
                UsePermissionsContext = true,
                AuthorizedPermissions = new[] {
                    GraphQLApi.Permissions.ExecuteGraphQL,
                    Contents.Permissions.ViewContent
                }
            };

            using (var context = new SiteContext())
            {
                await context.InitializeAsync(databaseProvider, connectionString, permissionContext);

                var result = await context.GraphQLClient.Content
                    .Query("blog", builder =>
                    {
                        builder.WithField("contentItemId");
                    });

                Assert.NotEmpty(result["data"]["blog"]);
            }
        }

        [Theory]
        [SqliteData]
        [SqlServerData]
        [MySqlData]
        [PostgreSqlData]
        public async Task ShouldNotReturnBlogsWithoutViewBlogContentPermission(string databaseProvider, string connectionString)
        {
            var permissionContext = new PermissionsContext
            {
                UsePermissionsContext = true,
                AuthorizedPermissions = new[] {
                    GraphQLApi.Permissions.ExecuteGraphQL
                }
            };

            using (var context = new SiteContext())
            {
                await context.InitializeAsync(databaseProvider, connectionString, permissionContext);

                var result = await context.GraphQLClient.Content
                    .Query("blog", builder =>
                    {
                        builder.WithField("contentItemId");
                    });

                Assert.Equal(GraphQLApi.ValidationRules.RequiresPermissionValidationRule.ErrorCode, result["errors"][0]["extensions"]["code"]);
            }
        }
    }
}
