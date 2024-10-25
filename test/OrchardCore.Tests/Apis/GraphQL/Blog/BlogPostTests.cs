using System.Text.Json.Nodes;
using OrchardCore.Apis.GraphQL;
using OrchardCore.Autoroute.Models;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentManagement;
using OrchardCore.Lists.Models;
using OrchardCore.Tests.Apis.Context;
using GraphQLApi = OrchardCore.Apis.GraphQL;

namespace OrchardCore.Tests.Apis.GraphQL;

public class BlogPostTests
{
    [Fact]

    public async Task ShouldListAllBlogs()
    {
        using var context = new BlogContext();
        await context.InitializeAsync();

        var result = await context
            .GraphQLClient
            .Content
            .Query("Blog", builder =>
            {
                builder
                    .WithField("contentItemId");
            });

        Assert.Single(
            result["data"]["blog"].AsArray(), node => node["contentItemId"].ToString() == context.BlogContentItemId);
    }

    [Fact]
    public async Task ShouldQueryByBlogPostAutoroutePart()
    {
        using var context = new BlogContext();
        await context.InitializeAsync();

        var blogPostContentItemId1 = await context
            .CreateContentItem("BlogPost", builder =>
            {
                builder
                    .DisplayText = "Some sorta blogpost!";

                builder
                    .Weld(new AutoroutePart
                    {
                        Path = "Path1",
                    });

                builder
                    .Weld(new ContainedPart
                    {
                        ListContentItemId = context.BlogContentItemId,
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
                        Path = "Path2",
                    });

                builder
                    .Weld(new ContainedPart
                    {
                        ListContentItemId = context.BlogContentItemId,
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

    [Fact]
    public async Task WhenThePartHasTheSameNameAsTheContentTypeShouldCollapseFieldsToContentType()
    {
        using var context = new BlogContext();
        await context.InitializeAsync();

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

    [Fact]
    public async Task WhenCreatingABlogPostShouldBeAbleToPopulateField()
    {
        using var context = new BlogContext();
        await context.InitializeAsync();

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
                        ListContentItemId = context.BlogContentItemId,
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

    [Fact]
    public async Task ShouldQueryByStatus()
    {
        using var context = new BlogContext();
        await context.InitializeAsync();

        var draft = await context
            .CreateContentItem("BlogPost", builder =>
            {
                builder.DisplayText = "Draft blog post";
                builder.Published = false;
                builder.Latest = true;

                builder
                    .Weld(new ContainedPart
                    {
                        ListContentItemId = context.BlogContentItemId,
                    });
            }, draft: true);

        var result = await context.GraphQLClient.Content
            .Query("blogPost(status: PUBLISHED) { displayText, published }");

        Assert.Single(result["data"]["blogPost"].AsArray());
        Assert.True(result["data"]["blogPost"][0]["published"].Value<bool>());

        result = await context.GraphQLClient.Content
            .Query("blogPost(status: DRAFT) { displayText, published }");

        Assert.Single(result["data"]["blogPost"].AsArray());
        Assert.False(result["data"]["blogPost"][0]["published"].Value<bool>());

        result = await context.GraphQLClient.Content
            .Query("blogPost(status: LATEST) { displayText, published }");

        Assert.Equal(2, result["data"]["blogPost"].AsArray().Count);
    }

    [Fact]
    public async Task ShouldNotBeAbleToExecuteAnyQueriesWithoutPermission()
    {
        using var context = new SiteContext()
            .WithPermissionsContext(new PermissionsContext { UsePermissionsContext = true });

        await context.InitializeAsync();

        var response = await context.GraphQLClient.Client.GetAsync("api/graphql");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ShouldReturnBlogsWithViewBlogContentPermission()
    {
        using var context = new SiteContext()
            .WithPermissionsContext(new PermissionsContext
            {
                UsePermissionsContext = true,
                AuthorizedPermissions =
                [
                    CommonPermissions.ExecuteGraphQL,
                    Contents.CommonPermissions.ViewContent,
                ],
            });

        await context.InitializeAsync();

        var result = await context.GraphQLClient.Content
            .Query("blog", builder =>
            {
                builder.WithField("contentItemId");
            });

        Assert.NotEmpty(result["data"]["blog"].AsArray());
    }

    [Fact]
    public async Task ShouldNotReturnBlogsWithViewOwnBlogContentPermission()
    {
        using var context = new SiteContext()
            .WithPermissionsContext(new PermissionsContext
            {
                UsePermissionsContext = true,
                AuthorizedPermissions =
                [
                    CommonPermissions.ExecuteGraphQL,
                    Contents.CommonPermissions.ViewOwnContent,
                ],
            });

        await context.InitializeAsync();

        var result = await context.GraphQLClient.Content
            .Query("blog", builder =>
            {
                builder.WithField("contentItemId");
            });

        Assert.Empty(result["data"]["blog"].AsArray());
    }

    [Fact]
    public async Task ShouldNotReturnBlogsWithoutViewBlogContentPermission()
    {
        using var context = new SiteContext()
            .WithPermissionsContext(new PermissionsContext
            {
                UsePermissionsContext = true,
                AuthorizedPermissions =
                [
                    CommonPermissions.ExecuteGraphQL,
                ],
            });

        await context.InitializeAsync();

        var result = await context.GraphQLClient.Content
            .Query("blog", builder =>
            {
                builder.WithField("contentItemId");
            });

        Assert.Equal(GraphQLApi.ValidationRules.RequiresPermissionValidationRule.ErrorCode, result["errors"][0]["extensions"]["number"].ToString());
    }
}
