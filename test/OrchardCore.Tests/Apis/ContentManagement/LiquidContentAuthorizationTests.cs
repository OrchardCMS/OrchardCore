using System.Text.Json;
using System.Text.Json.Nodes;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Contents;
using OrchardCore.Liquid.Fields;
using OrchardCore.Liquid.Models;
using OrchardCore.Security.Permissions;
using OrchardCore.Tests.Apis.Context;
using LiquidPermissions = OrchardCore.Liquid.Permissions;

namespace OrchardCore.Tests.Apis.ContentManagement;

public class LiquidContentAuthorizationTests
{
    [Fact]
    public async Task ContentApi_LiquidContent_RequiresPermissionBeforeAndAfterMerge()
    {
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
        await SetContentDefinitionsAsync(context);

        var directPartResponse = await context.Client.PostAsJsonAsync(
            "api/content",
            CreateLiquidPartItem());
        Assert.Equal(HttpStatusCode.Unauthorized, directPartResponse.StatusCode);

        var directFieldResponse = await context.Client.PostAsJsonAsync(
            "api/content",
            CreateLiquidFieldItem());
        Assert.Equal(HttpStatusCode.Unauthorized, directFieldResponse.StatusCode);

        var embeddedPartResponse = await context.Client.PostAsJsonAsync(
            "api/content",
            CreateContainer(CreateLiquidPartItem()));
        Assert.Equal(HttpStatusCode.Unauthorized, embeddedPartResponse.StatusCode);

        var embeddedFieldResponse = await context.Client.PostAsJsonAsync(
            "api/content",
            CreateContainer(CreateLiquidFieldItem(), recursivelyEmbedded: true));
        Assert.Equal(HttpStatusCode.Unauthorized, embeddedFieldResponse.StatusCode);

        var unknownEmbeddedType = CreateContainer();
        ((JsonObject)unknownEmbeddedType.Content)["Embedded"] = new JsonObject
        {
            [nameof(ContentItem.ContentType)] = "UnknownLiquidType",
            [nameof(LiquidPart)] = new JsonObject
            {
                [nameof(LiquidPart.Liquid)] = "{{ unsafe }}",
            },
        };
        var unknownEmbeddedTypeResponse = await context.Client.PostAsJsonAsync(
            "api/content",
            unknownEmbeddedType);
        unknownEmbeddedTypeResponse.EnsureSuccessStatusCode();

        var safeContainerResponse = await context.Client.PostAsJsonAsync(
            "api/content",
            CreateContainer());
        safeContainerResponse.EnsureSuccessStatusCode();
        var safeContainer = await safeContainerResponse.Content.ReadAsAsync<ContentItem>();

        var secondSafeContainerResponse = await context.Client.PostAsJsonAsync(
            "api/content",
            CreateContainer());
        secondSafeContainerResponse.EnsureSuccessStatusCode();
        var secondSafeContainer =
            await secondSafeContainerResponse.Content.ReadAsAsync<ContentItem>();

        ((JsonObject)safeContainer.Content)["Embedded"] =
            JsonSerializer.SerializeToNode(CreateLiquidPartItem());

        var forgedUpdateResponse = await context.Client.PostAsJsonAsync(
            "api/content",
            safeContainer);
        Assert.Equal(HttpStatusCode.Unauthorized, forgedUpdateResponse.StatusCode);

        await context.UsingTenantScopeAsync(async scope =>
        {
            var contentManager = scope.ServiceProvider.GetRequiredService<IContentManager>();
            var storedContainer = await contentManager.GetAsync(
                safeContainer.ContentItemId,
                VersionOptions.Latest);

            Assert.NotNull(storedContainer);
            Assert.False(((JsonObject)storedContainer.Content).ContainsKey("Embedded"));
        });

        ((JsonObject)secondSafeContainer.Content)["Embedded"] =
            ((JsonObject)CreateContainer(
                CreateLiquidFieldItem(),
                recursivelyEmbedded: true).Content)["Embedded"]?.DeepClone();

        var recursiveFieldUpdateResponse = await context.Client.PostAsJsonAsync(
            "api/content",
            secondSafeContainer);
        Assert.Equal(HttpStatusCode.Unauthorized, recursiveFieldUpdateResponse.StatusCode);

        await context.UsingTenantScopeAsync(async scope =>
        {
            var contentManager = scope.ServiceProvider.GetRequiredService<IContentManager>();
            var storedContainer = await contentManager.GetAsync(
                secondSafeContainer.ContentItemId,
                VersionOptions.Latest);

            Assert.NotNull(storedContainer);
            Assert.False(((JsonObject)storedContainer.Content).ContainsKey("Embedded"));
        });

        permissionsContext.AuthorizedPermissions =
        [
            CommonPermissions.AccessContentApi,
            CommonPermissions.EditContent,
            CommonPermissions.PublishContent,
            LiquidPermissions.ManageLiquidTemplates,
        ];

        var authorizedPartResponse = await context.Client.PostAsJsonAsync(
            "api/content",
            CreateLiquidPartItem());
        authorizedPartResponse.EnsureSuccessStatusCode();
        var authorizedPart = await authorizedPartResponse.Content.ReadAsAsync<ContentItem>();

        var authorizedFieldResponse = await context.Client.PostAsJsonAsync(
            "api/content",
            CreateLiquidFieldItem());
        authorizedFieldResponse.EnsureSuccessStatusCode();
        var authorizedField = await authorizedFieldResponse.Content.ReadAsAsync<ContentItem>();

        var authorizedEmbeddedFieldResponse = await context.Client.PostAsJsonAsync(
            "api/content",
            CreateContainer(CreateLiquidFieldItem(), recursivelyEmbedded: true));
        authorizedEmbeddedFieldResponse.EnsureSuccessStatusCode();

        permissionsContext.AuthorizedPermissions =
        [
            CommonPermissions.AccessContentApi,
            CommonPermissions.EditContent,
            CommonPermissions.PublishContent,
        ];

        authorizedPart.DisplayText = "Forged part update";
        Assert.True(authorizedPart.TryGet<LiquidPart>(out var authorizedLiquidPart));
        authorizedLiquidPart.Liquid = "{{ forged }}";
        var directPartUpdateResponse = await context.Client.PostAsJsonAsync(
            "api/content",
            authorizedPart);
        Assert.Equal(HttpStatusCode.Unauthorized, directPartUpdateResponse.StatusCode);

        authorizedField.DisplayText = "Forged field update";
        authorizedField
            .Get<ContentPart>("LiquidFields")
            .Get<LiquidField>("Template")
            .Liquid = "{{ forged }}";
        var directFieldUpdateResponse = await context.Client.PostAsJsonAsync(
            "api/content",
            authorizedField);
        Assert.Equal(HttpStatusCode.Unauthorized, directFieldUpdateResponse.StatusCode);

        await context.UsingTenantScopeAsync(async scope =>
        {
            var contentManager = scope.ServiceProvider.GetRequiredService<IContentManager>();
            var storedPart = await contentManager.GetAsync(
                authorizedPart.ContentItemId,
                VersionOptions.Latest);
            var storedField = await contentManager.GetAsync(
                authorizedField.ContentItemId,
                VersionOptions.Latest);

            Assert.Equal("Liquid part article", storedPart.DisplayText);
            Assert.True(storedPart.TryGet<LiquidPart>(out var storedLiquidPart));
            Assert.Equal(
                "{{ ContentItem.DisplayText }}",
                storedLiquidPart.Liquid);
            Assert.Equal("Liquid field article", storedField.DisplayText);
            Assert.Equal(
                "{{ ContentItem.DisplayText }}",
                storedField
                    .Get<ContentPart>("LiquidFields")
                    .Get<LiquidField>("Template")
                    .Liquid);
        });
    }

    private static ContentItem CreateLiquidPartItem()
    {
        var contentItem = new ContentItem
        {
            ContentType = "LiquidPartArticle",
            DisplayText = "Liquid part article",
        };

        contentItem.Weld(new LiquidPart { Liquid = "{{ ContentItem.DisplayText }}" });

        return contentItem;
    }

    private static ContentItem CreateLiquidFieldItem()
    {
        var contentItem = new ContentItem
        {
            ContentType = "LiquidFieldArticle",
            DisplayText = "Liquid field article",
        };
        var fields = new ContentPart();
        fields.Weld("Template", new LiquidField { Liquid = "{{ ContentItem.DisplayText }}" });
        contentItem.Weld("LiquidFields", fields);

        return contentItem;
    }

    private static ContentItem CreateContainer(
        ContentItem embeddedContentItem = null,
        bool recursivelyEmbedded = false)
    {
        var contentItem = new ContentItem
        {
            ContentType = "LiquidContainer",
            DisplayText = "Liquid container",
        };

        if (embeddedContentItem is null)
        {
            return contentItem;
        }

        var embeddedNode = JsonSerializer.SerializeToNode(embeddedContentItem);

        ((JsonObject)contentItem.Content)["Embedded"] = recursivelyEmbedded
            ? new JsonObject
            {
                ["Items"] = new JsonArray
                {
                    new JsonObject
                    {
                        ["Nested"] = embeddedNode,
                    },
                },
            }
            : embeddedNode;

        return contentItem;
    }

    private static Task SetContentDefinitionsAsync(SiteContext context)
    {
        return context.UsingTenantScopeAsync(async scope =>
        {
            var contentDefinitionManager =
                scope.ServiceProvider.GetRequiredService<IContentDefinitionManager>();

            await contentDefinitionManager.AlterPartDefinitionAsync("LiquidFields", part => part
                .WithField("Template", field => field.OfType(nameof(LiquidField))));

            await contentDefinitionManager.AlterTypeDefinitionAsync("LiquidPartArticle", type => type
                .Creatable()
                .Draftable()
                .Versionable()
                .WithPart(nameof(LiquidPart)));

            await contentDefinitionManager.AlterTypeDefinitionAsync("LiquidFieldArticle", type => type
                .Creatable()
                .Draftable()
                .Versionable()
                .WithPart("LiquidFields"));

            await contentDefinitionManager.AlterTypeDefinitionAsync("LiquidContainer", type => type
                .Creatable()
                .Draftable()
                .Versionable());
        });
    }
}
