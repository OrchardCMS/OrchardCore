using System.Threading.Tasks;
using OrchardCore.Tests.Apis.Context;
using Xunit;
using GraphQLApi = OrchardCore.Apis.GraphQL;

namespace OrchardCore.Tests.Apis.GraphQL.Permissions
{
    public class PermissionsTests
    {
        [Fact]
        public async Task ShouldNotBeAbleToExecuteAnyQueriesWithoutPermission()
        {
            var permissionContext = new PermissionsContext {
                UsePermissionsContext = true
            };

            using (var context = new SiteContext())
            {
                await context.InitializeAsync(permissionContext);

                var response = await context.GraphQLClient.Client.GetAsync("/api/graphql");
                Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
                Assert.Equal(string.Empty, await response.Content.ReadAsStringAsync());
            }
        }

        [Fact]
        public async Task ShouldReturnBlogsWithViewBlogContentPermission()
        {
            var permissionContext = new PermissionsContext
            {
                UsePermissionsContext = true,
                AuthorizedPermissions = new[]
                {
                    GraphQLApi.Permissions.ExecuteGraphQL,
                    Contents.Permissions.ViewContent
                }
            };

            using (var context = new SiteContext())
            {
                await context.InitializeAsync(permissionContext);

                var result = await context.GraphQLClient.Content
                    .Query("blog", builder =>
                      {
                          builder
                              .WithField("contentItemId");
                    });

                Assert.NotEmpty(result["data"]["blog"]);
            }
        }

        [Fact]
        public async Task ShouldNotReturnBlogsWithoutViewBlogContentPermission()
        {
            var permissionContext = new PermissionsContext
            {
                UsePermissionsContext = true,
                AuthorizedPermissions = new[]
                {
                    GraphQLApi.Permissions.ExecuteGraphQL
                }
            };

            using (var context = new SiteContext())
            {
                await context.InitializeAsync(permissionContext);

                var result = await context.GraphQLClient.Content
                    .Query("blog", builder =>
                    {
                        builder
                            .WithField("contentItemId");
                    });


                Assert.Equal(GraphQLApi.ValidationRules.RequiresPermissionValidationRule.ErrorCode, result["errors"][0]["extensions"]["code"]);
            }
        }
    }
}
