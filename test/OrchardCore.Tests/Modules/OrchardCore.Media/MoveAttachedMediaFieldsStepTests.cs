using System.Text.Json.Nodes;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.Media;
using OrchardCore.Media.Fields;
using OrchardCore.Tests.Apis.Context;

namespace OrchardCore.Tests.Modules.OrchardCore.Media;

public class MoveAttachedMediaFieldsStepTests
{
    [Fact]
    public async Task MoveAttachedMediaFieldsStep_MovesOnlySelectedContentTypes()
    {
        using var context = new SiteContext();
        await context.InitializeAsync();

        string selectedContentItemId = null;
        string skippedContentItemId = null;

        await context.UsingTenantScopeAsync(async scope =>
        {
            selectedContentItemId = await CreateContentItemWithAttachedEditorAsync(scope, "SelectedMigrationArticle", "selected-shared/report.png");
            skippedContentItemId = await CreateContentItemWithAttachedEditorAsync(scope, "SkippedMigrationArticle", "skipped-shared/report.png");
        });

        await RecipeHelpers.RunRecipeAsync(context, CreateRecipe(["SelectedMigrationArticle"]));
        await context.WaitForHttpBackgroundJobsAsync(TestContext.Current.CancellationToken);

        await context.UsingTenantScopeAsync(async scope =>
        {
            await AssertFieldPathAsync(scope, selectedContentItemId, "SelectedMigrationArticle", "selected-shared/report.png", expectedToMove: true);
            await AssertFieldPathAsync(scope, skippedContentItemId, "SkippedMigrationArticle", "skipped-shared/report.png", expectedToMove: false);
        });
    }

    [Fact]
    public async Task MoveAttachedMediaFieldsStep_EvaluatesAllContentTypes_WhenFilterIsMissing()
    {
        using var context = new SiteContext();
        await context.InitializeAsync();

        string contentItemId = null;

        await context.UsingTenantScopeAsync(async scope =>
        {
            contentItemId = await CreateContentItemWithAttachedEditorAsync(scope, "AllTypesMigrationArticle", "all-shared/report.png");
        });

        await RecipeHelpers.RunRecipeAsync(context, CreateRecipe(null));
        await context.WaitForHttpBackgroundJobsAsync(TestContext.Current.CancellationToken);

        await context.UsingTenantScopeAsync(async scope =>
        {
            await AssertFieldPathAsync(scope, contentItemId, "AllTypesMigrationArticle", "all-shared/report.png", expectedToMove: true);
        });
    }

    private static async Task<string> CreateContentItemWithAttachedEditorAsync(ShellScope scope, string contentType, string sourcePath)
    {
        var contentDefinitionManager = scope.ServiceProvider.GetRequiredService<IContentDefinitionManager>();
        var contentManager = scope.ServiceProvider.GetRequiredService<IContentManager>();
        var mediaFileStore = scope.ServiceProvider.GetRequiredService<IMediaFileStore>();

        await contentDefinitionManager.AlterPartDefinitionAsync($"{contentType}Part", part => part
            .WithField<MediaField>("Document", field => field.WithEditor("Attached")));

        await contentDefinitionManager.AlterTypeDefinitionAsync(contentType, type => type
            .Creatable()
            .Draftable()
            .Versionable()
            .WithPart($"{contentType}Part"));

        await using (var stream = new MemoryStream([1, 2, 3, 4]))
        {
            await mediaFileStore.CreateFileFromStreamAsync(sourcePath, stream, overwrite: true);
        }

        var contentItem = await contentManager.NewAsync(contentType);
        SetMediaFieldPath(contentItem, $"{contentType}Part", "Document", sourcePath);

        await contentManager.CreateAsync(contentItem);

        var draft = await contentManager.GetAsync(contentItem.ContentItemId, VersionOptions.DraftRequired);
        SetMediaFieldPath(draft, $"{contentType}Part", "Document", sourcePath);
        await contentManager.SaveDraftAsync(draft);

        return contentItem.ContentItemId;
    }

    private static void SetMediaFieldPath(ContentItem contentItem, string partName, string fieldName, string path)
    {
        var part = contentItem.GetOrCreate<ContentPart>(partName);
        var field = part.GetOrCreate<MediaField>(fieldName);

        field.Paths = [path];

        part.Apply(fieldName, field);
        contentItem.Apply(partName, part);
    }

    private static async Task AssertFieldPathAsync(ShellScope scope, string contentItemId, string contentType, string originalPath, bool expectedToMove)
    {
        var contentManager = scope.ServiceProvider.GetRequiredService<IContentManager>();
        var mediaFileStore = scope.ServiceProvider.GetRequiredService<IMediaFileStore>();

        var versions = (await contentManager.GetAllVersionsAsync(contentItemId)).ToArray();

        Assert.NotEmpty(versions);

        foreach (var version in versions)
        {
            var field = version.Get<ContentPart>($"{contentType}Part").Get<MediaField>("Document");
            var path = Assert.Single(field.Paths);

            if (expectedToMove)
            {
                Assert.StartsWith($"mediafields/{contentType}/{contentItemId}/", path, StringComparison.OrdinalIgnoreCase);
                Assert.Equal(["report.png"], field.GetAttachedFileNames());
                Assert.NotNull(await mediaFileStore.GetFileInfoAsync(path));
                Assert.Null(await mediaFileStore.GetFileInfoAsync(originalPath));
            }
            else
            {
                Assert.Equal(originalPath, path);
                Assert.NotNull(await mediaFileStore.GetFileInfoAsync(originalPath));
            }
        }
    }

    private static JsonObject CreateRecipe(string[] contentTypes)
    {
        var step = new JsonObject
        {
            ["name"] = "move-attached-media-fields",
        };

        if (contentTypes?.Length > 0)
        {
            step["ContentTypes"] = new JsonArray(contentTypes.Select(x => (JsonNode)x).ToArray());
        }

        return new JsonObject
        {
            ["name"] = "MediaMigration",
            ["displayName"] = "Media Migration",
            ["description"] = "Moves media fields to attached media paths.",
            ["author"] = "Tests",
            ["website"] = "https://localhost",
            ["version"] = "1.0.0",
            ["issetuprecipe"] = false,
            ["categories"] = new JsonArray("Tests"),
            ["tags"] = new JsonArray(),
            ["steps"] = new JsonArray(step),
        };
    }
}
