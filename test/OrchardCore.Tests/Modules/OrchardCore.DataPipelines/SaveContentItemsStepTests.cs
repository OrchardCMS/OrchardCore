using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Records;
using OrchardCore.DataPipelines.Steps;
using OrchardCore.DataSources;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.Tests.Apis.Context;
using OrchardCore.Users;
using ISession = YesSql.ISession;

namespace OrchardCore.Tests.Modules.OrchardCore.DataPipelines;

public sealed class SaveContentItemsStepTests
{
    private static readonly DataField[] _posts =
    [
        new("Key", "Key", DataFieldType.Text),
        new("Title", "Title", DataFieldType.Text),
        new("Subtitle", "Subtitle", DataFieldType.Text),
    ];

    [Fact]
    public async Task Execute_NewRows_CreatesPublishedItemsWithMappedValues()
    {
        // Arrange
        using var context = await CreateContextAsync();

        await context.UsingTenantScopeAsync(async scope =>
        {
            var settings = new SaveContentItemsStepSettings
            {
                ContentType = "BlogPost",
                MatchBy = ContentItemMatch.DisplayText,
                KeyField = "Title",
                Mappings =
                [
                    new ContentFieldMapping { Target = "TitlePart.Title", Source = "Title" },
                    new ContentFieldMapping { Target = "BlogPost.Subtitle", Source = "Subtitle" },
                ],
            };

            using var host = await CreateHostAsync(scope, settings);
            host.WithRows(_posts, ["k1", "Imported one", "First"], ["k2", "Imported two", "Second"]);

            // Act
            await host.ExecuteAsync();
        });

        // Assert
        await context.UsingTenantScopeAsync(async scope =>
        {
            var items = await FindAsync(scope, "Imported one", "Imported two");
            Assert.Equal(2, items.Count);
            Assert.All(items, item => Assert.True(item.Published));

            var first = items.Single(item => item.DisplayText == "Imported one");
            Assert.Equal("First", (string)first.Content.BlogPost.Subtitle.Text);
            Assert.Equal("Imported one", (string)first.Content.TitlePart.Title);
        });
    }

    [Fact]
    public async Task Execute_ExistingItem_UpdatesItInsteadOfCreatingAnother()
    {
        // Arrange
        using var context = await CreateContextAsync();
        await context.CreateContentItem("BlogPost", builder =>
        {
            builder.DisplayText = "Existing post";
            builder.Content.TitlePart = new System.Text.Json.Nodes.JsonObject { ["Title"] = "Existing post" };
        });

        await context.UsingTenantScopeAsync(async scope =>
        {
            var settings = new SaveContentItemsStepSettings
            {
                ContentType = "BlogPost",
                MatchBy = ContentItemMatch.DisplayText,
                KeyField = "Title",
                Mappings = [new ContentFieldMapping { Target = "BlogPost.Subtitle", Source = "Subtitle" }],
            };

            using var host = await CreateHostAsync(scope, settings);
            host.WithRows(_posts, ["k", "Existing post", "Updated subtitle"]);

            // Act
            await host.ExecuteAsync();
        });

        // Assert
        await context.UsingTenantScopeAsync(async scope =>
        {
            var item = Assert.Single(await FindAsync(scope, "Existing post"));
            Assert.Equal("Updated subtitle", (string)item.Content.BlogPost.Subtitle.Text);
        });
    }

    [Fact]
    public async Task Execute_UpdateOnlyWithUnknownKey_RejectsTheRow()
    {
        // Arrange
        using var context = await CreateContextAsync();

        await context.UsingTenantScopeAsync(async scope =>
        {
            var settings = new SaveContentItemsStepSettings
            {
                ContentType = "BlogPost",
                Mode = ContentItemSaveMode.UpdateOnly,
                MatchBy = ContentItemMatch.ContentItemId,
                KeyField = "Key",
                Mappings = [new ContentFieldMapping { Target = "BlogPost.Subtitle", Source = "Subtitle" }],
            };

            using var host = await CreateHostAsync(scope, settings);
            host.WithRows(_posts, ["missing-id", "x", "y"]);

            // Act
            await host.ExecuteAsync();

            // Assert
            var rejected = Assert.Single(host.Output(SaveContentItemsStep.Rejected).Rows);
            Assert.Equal("missing-id", rejected[0]);
            Assert.Contains("missing-id", (string)rejected[^1], StringComparison.Ordinal);
        });
    }

    [Fact]
    public async Task Describe_UnknownTargetOrSource_ReportsErrors()
    {
        // Arrange
        using var context = await CreateContextAsync();

        await context.UsingTenantScopeAsync(async scope =>
        {
            var settings = new SaveContentItemsStepSettings
            {
                ContentType = "BlogPost",
                KeyField = "Nope",
                Mappings = [new ContentFieldMapping { Target = "BlogPost.Missing", Source = "Title" }],
            };

            using var host = await CreateHostAsync(scope, settings);
            host.WithRows(_posts);

            // Act
            var description = await host.DescribeAsync();

            // Assert
            Assert.Contains(description.Issues, issue => issue.Message.Contains("Nope", StringComparison.Ordinal));
            Assert.Contains(description.Issues, issue => issue.Message.Contains("BlogPost.Missing", StringComparison.Ordinal));
        });
    }

    private static async Task<StepTestHost> CreateHostAsync(ShellScope scope, SaveContentItemsStepSettings settings)
    {
        var stepType = scope.ServiceProvider.GetServices<IDataPipelineStepType>().Single(step => step.Name == SaveContentItemsStep.StepName);
        var host = new StepTestHost(stepType, settings, scope.ServiceProvider);
        host.Run.User = await GetAdminAsync(scope);

        return host;
    }

    private static async Task<List<ContentItem>> FindAsync(ShellScope scope, params string[] titles)
        => (await scope.ServiceProvider.GetRequiredService<ISession>()
            .Query<ContentItem, ContentItemIndex>(index => index.ContentType == "BlogPost" && index.Latest)
            .ListAsync())
            .Where(item => titles.Contains(item.DisplayText))
            .ToList();

    private static async Task<ClaimsPrincipal> GetAdminAsync(ShellScope scope)
    {
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IUser>>();
        var factory = scope.ServiceProvider.GetRequiredService<IUserClaimsPrincipalFactory<IUser>>();

        return await factory.CreateAsync(await userManager.FindByNameAsync("admin"));
    }

    private static async Task<BlogContext> CreateContextAsync()
    {
        var context = new BlogContext();
        await context.InitializeAsync();

        await context.UsingTenantScopeAsync(async scope =>
        {
            var featuresManager = scope.ServiceProvider.GetRequiredService<IShellFeaturesManager>();
            var features = (await featuresManager.GetAvailableFeaturesAsync())
                .Where(feature => feature.Id is "OrchardCore.DataPipelines.Contents")
                .ToList();

            await featuresManager.UpdateFeaturesAsync([], features, force: true);
        });

        return context;
    }
}
