using System.Text.Json;
using System.Text.Json.Nodes;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.ContentManagement.Models;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Contents;
using OrchardCore.Html.Models;
using OrchardCore.Html.Settings;
using OrchardCore.Liquid;
using OrchardCore.Liquid.Models;
using OrchardCore.Localization;
using OrchardCore.Markdown.Models;
using OrchardCore.Markdown.Settings;
using OrchardCore.Security;
using OrchardCore.Security.Permissions;
using OrchardCore.Security.Services;
using OrchardCore.Tests.Apis.Context;
using GraphQLPermissions = OrchardCore.Apis.GraphQL.CommonPermissions;
using LiquidPermissions = OrchardCore.Liquid.Permissions;

namespace OrchardCore.Tests.Apis.ContentManagement;

public class LiquidContentAuthorizationTests
{
    [Fact]
    public async Task ContentApiShouldRequireLiquidPermissionAndPreserveAuthoredSource()
    {
        using var cultureScope = CultureScope.Create("en");

        var permissionsContext = new PermissionsContext
        {
            UsePermissionsContext = true,
            AuthorizedPermissions =
            [
                CommonPermissions.AccessContentApi,
                CommonPermissions.EditContent,
                CommonPermissions.PublishContent,
            ],
        };
        using var context = new SiteContext().WithPermissionsContext(permissionsContext);
        await context.InitializeAsync();
        context.Client.DefaultRequestHeaders.AcceptLanguage.ParseAdd("en");

        await context.UsingTenantScopeAsync(async scope =>
        {
            var roleService = scope.ServiceProvider.GetRequiredService<IRoleService>();
            var editor = (await roleService.GetRolesAsync())
                .OfType<Role>()
                .Single(role => role.RoleName == OrchardCoreConstants.Roles.Editor);

            Assert.Equal(
                "Grants users the ability to edit content and Liquid templates.",
                editor.RoleDescription);
            Assert.Contains(editor.RoleClaims, claim =>
                claim.ClaimType == Permission.ClaimType &&
                claim.ClaimValue == LiquidPermissions.ManageLiquidTemplates.Name);
        });

        await SetEmbeddedLiquidContentDefinitionsAsync(context);

        var embeddedLiquidContentItem = new ContentItem
        {
            ContentType = "LiquidWidget",
        };
        embeddedLiquidContentItem.Weld(new LiquidPart { Liquid = "{{ ContentItem.DisplayText }}" });
        var containerContentItem = new ContentItem
        {
            ContentType = "LiquidContainer",
            DisplayText = "Container",
        };
        ((JsonObject)containerContentItem.Content)["Embedded"] =
            JsonSerializer.SerializeToNode(embeddedLiquidContentItem);

        var embeddedResponse = await context.Client.PostAsJsonAsync("api/content", containerContentItem);

        Assert.Equal(HttpStatusCode.Unauthorized, embeddedResponse.StatusCode);

        await SetHtmlBodyPartSettingsAsync(context, sanitizeHtml: true);

        const string source =
            "[locale 'en']<script>alert(1)</script><p>{{ ContentItem.DisplayText }}</p>[/locale]";
        var contentItem = new ContentItem
        {
            ContentType = "LiquidArticle",
            DisplayText = "Liquid Article",
        };
        contentItem.Weld(new HtmlBodyPart { Html = source });

        var forbiddenResponse = await context.Client.PostAsJsonAsync("api/content", contentItem);

        Assert.Equal(HttpStatusCode.Unauthorized, forbiddenResponse.StatusCode);

        permissionsContext.AuthorizedPermissions =
        [
            CommonPermissions.AccessContentApi,
            CommonPermissions.EditContent,
            CommonPermissions.PublishContent,
            LiquidPermissions.ManageLiquidTemplates,
        ];

        var response = await context.Client.PostAsJsonAsync("api/content", contentItem);
        response.EnsureSuccessStatusCode();
        var savedContentItem = await response.Content.ReadAsAsync<ContentItem>();

        Assert.Equal(source, savedContentItem.Get<HtmlBodyPart>(nameof(HtmlBodyPart)).Html);

        await context.UsingTenantScopeAsync(async scope =>
        {
            var contentManager = scope.ServiceProvider.GetRequiredService<IContentManager>();
            var storedContentItem = await contentManager.GetAsync(
                savedContentItem.ContentItemId,
                VersionOptions.Latest);

            Assert.Equal(source, storedContentItem.Get<HtmlBodyPart>(nameof(HtmlBodyPart)).Html);

            var bodyAspect = await contentManager.PopulateAspectAsync<BodyAspect>(storedContentItem);
            var renderedHtml = bodyAspect.Body.ToString();

            Assert.DoesNotContain("<script", renderedHtml, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("[locale", renderedHtml, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("<p>Liquid Article</p>", renderedHtml, StringComparison.Ordinal);
        });

        await SetHtmlBodyPartSettingsAsync(context, sanitizeHtml: false);

        await context.UsingTenantScopeAsync(async scope =>
        {
            var contentManager = scope.ServiceProvider.GetRequiredService<IContentManager>();
            var storedContentItem = await contentManager.GetAsync(
                savedContentItem.ContentItemId,
                VersionOptions.Latest);
            var bodyAspect = await contentManager.PopulateAspectAsync<BodyAspect>(storedContentItem);
            var renderedHtml = bodyAspect.Body.ToString();

            Assert.Contains("<script>alert(1)</script>", renderedHtml, StringComparison.Ordinal);
            Assert.Contains("<p>Liquid Article</p>", renderedHtml, StringComparison.Ordinal);
        });

        await SetMarkdownBodyPartSettingsAsync(context);

        const string markdownSource =
            "# {{ ContentItem.DisplayText }}\n\n<script>alert(2)</script>";
        var markdownContentItem = new ContentItem
        {
            ContentType = "LiquidMarkdownArticle",
            DisplayText = "Markdown Article",
        };
        markdownContentItem.Weld(new MarkdownBodyPart { Markdown = markdownSource });

        var markdownResponse = await context.Client.PostAsJsonAsync("api/content", markdownContentItem);
        markdownResponse.EnsureSuccessStatusCode();
        var savedMarkdownContentItem = await markdownResponse.Content.ReadAsAsync<ContentItem>();

        Assert.Equal(markdownSource, savedMarkdownContentItem.Get<MarkdownBodyPart>(nameof(MarkdownBodyPart)).Markdown);

        await context.UsingTenantScopeAsync(async scope =>
        {
            var contentManager = scope.ServiceProvider.GetRequiredService<IContentManager>();
            var storedContentItem = await contentManager.GetAsync(
                savedMarkdownContentItem.ContentItemId,
                VersionOptions.Latest);

            Assert.Equal(markdownSource, storedContentItem.Get<MarkdownBodyPart>(nameof(MarkdownBodyPart)).Markdown);

            var bodyAspect = await contentManager.PopulateAspectAsync<BodyAspect>(storedContentItem);
            var renderedHtml = bodyAspect.Body.ToString();

            Assert.Contains("Markdown Article</h1>", renderedHtml, StringComparison.Ordinal);
            Assert.DoesNotContain("{{ ContentItem.DisplayText }}", renderedHtml, StringComparison.Ordinal);
            Assert.DoesNotContain("<script", renderedHtml, StringComparison.OrdinalIgnoreCase);
        });

        const string graphQlSource =
            "[locale 'en']<script>alert(3)</script><p>{{ ContentItem.DisplayText }}</p>[/locale]";

        await context.UsingTenantScopeAsync(async scope =>
        {
            var contentDefinitionManager = scope.ServiceProvider.GetRequiredService<IContentDefinitionManager>();
            await contentDefinitionManager.AlterTypeDefinitionAsync("BlogPost", type => type
                .WithPart("HtmlBodyPart", part => part.MergeSettings<HtmlBodyPartSettings>(settings =>
                {
                    settings.RenderLiquid = true;
                    settings.SanitizeHtml = true;
                })));

            var session = scope.ServiceProvider.GetRequiredService<YesSql.ISession>();
            var blogPost = await session.Query<ContentItem, ContentItemIndex>(index =>
                index.ContentType == "BlogPost" && index.Published).FirstOrDefaultAsync();
            var contentManager = scope.ServiceProvider.GetRequiredService<IContentManager>();
            var draft = await contentManager.GetAsync(blogPost.ContentItemId, VersionOptions.DraftRequired);
            draft.Alter<HtmlBodyPart>(part => part.Html = graphQlSource);
            await contentManager.UpdateAsync(draft);
            await contentManager.PublishAsync(draft);
        });

        permissionsContext.AuthorizedPermissions =
        [
            CommonPermissions.AccessContentApi,
            CommonPermissions.EditContent,
            CommonPermissions.PublishContent,
            CommonPermissions.ViewContent,
            GraphQLPermissions.ExecuteGraphQL,
            LiquidPermissions.ManageLiquidTemplates,
        ];

        var graphQlResult = await context.GraphQLClient.Content.Query(
            "blogPost { displayText htmlBody { html } }");
        var graphQlHtml = graphQlResult["data"]["blogPost"][0]["htmlBody"]["html"].ToString();

        Assert.DoesNotContain("<script", graphQlHtml, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("<p>", graphQlHtml, StringComparison.Ordinal);
        Assert.DoesNotContain("[locale", graphQlHtml, StringComparison.OrdinalIgnoreCase);
    }

    private static Task SetHtmlBodyPartSettingsAsync(SiteContext context, bool sanitizeHtml)
    {
        return context.UsingTenantScopeAsync(async scope =>
        {
            var contentDefinitionManager = scope.ServiceProvider.GetRequiredService<IContentDefinitionManager>();

            await contentDefinitionManager.AlterTypeDefinitionAsync("LiquidArticle", type => type
                .Creatable()
                .Draftable()
                .Versionable()
                .WithPart("HtmlBodyPart", part => part.MergeSettings<HtmlBodyPartSettings>(settings =>
                {
                    settings.RenderLiquid = true;
                    settings.SanitizeHtml = sanitizeHtml;
                })));
        });
    }

    private static Task SetMarkdownBodyPartSettingsAsync(SiteContext context)
    {
        return context.UsingTenantScopeAsync(async scope =>
        {
            var contentDefinitionManager = scope.ServiceProvider.GetRequiredService<IContentDefinitionManager>();

            await contentDefinitionManager.AlterTypeDefinitionAsync("LiquidMarkdownArticle", type => type
                .Creatable()
                .Draftable()
                .Versionable()
                .WithPart("MarkdownBodyPart", part => part.MergeSettings<MarkdownBodyPartSettings>(settings =>
                {
                    settings.RenderLiquid = true;
                    settings.SanitizeHtml = true;
                })));
        });
    }

    private static Task SetEmbeddedLiquidContentDefinitionsAsync(SiteContext context)
    {
        return context.UsingTenantScopeAsync(async scope =>
        {
            var contentDefinitionManager = scope.ServiceProvider.GetRequiredService<IContentDefinitionManager>();

            await contentDefinitionManager.AlterTypeDefinitionAsync("LiquidWidget", type => type
                .WithPart("LiquidPart"));
            await contentDefinitionManager.AlterTypeDefinitionAsync("LiquidContainer", type => type
                .Creatable()
                .Draftable()
                .Versionable());
        });
    }
}
