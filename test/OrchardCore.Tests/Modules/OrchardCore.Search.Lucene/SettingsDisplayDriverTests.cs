using OrchardCore.ContentFields.Fields;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Implementation;
using OrchardCore.DisplayManagement.Theming;
using OrchardCore.Environment.Extensions;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Builders;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.Indexing;
using OrchardCore.Indexing.Models;
using OrchardCore.Locking;
using OrchardCore.Locking.Distributed;
using OrchardCore.Recipes.Services;
using OrchardCore.Scripting;
using OrchardCore.Search.Lucene.Settings;
using OrchardCore.Tests.Modules.OrchardCore.ContentFields.Settings;
using OrchardCore.Tests.Stubs;

namespace OrchardCore.Tests.Modules.OrchardCore.Search.Lucene;

public partial class SettingsDisplayDriverTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IShapeFactory _shapeFactory;
    private readonly ShapeTable _shapeTable;

    public SettingsDisplayDriverTests()
    {
        var serviceCollection = new ServiceCollection();

        serviceCollection.AddScripting()
            .AddLogging()
            .AddScoped<ILoggerFactory, NullLoggerFactory>()
            .AddScoped<IThemeManager, ThemeManager>()
            .AddScoped<IShapeFactory, DefaultShapeFactory>()
            .AddScoped<IExtensionManager, StubExtensionManager>()
            .AddScoped<IShapeTableManager, TestShapeTableManager>()
            .AddSingleton<IDistributedLock, LocalLock>();

        _shapeTable = new ShapeTable
        (
            new Dictionary<string, ShapeDescriptor>(StringComparer.OrdinalIgnoreCase),
            new Dictionary<string, ShapeBinding>(StringComparer.OrdinalIgnoreCase)
        );

        serviceCollection.AddSingleton(_shapeTable);

        _serviceProvider = serviceCollection.BuildServiceProvider();

        _shapeFactory = _serviceProvider.GetRequiredService<IShapeFactory>();
    }

    [Fact]
    public async Task ContentPickerFieldLuceneEditorSettingsShouldDeserialize()
    {
        await (await CreateShellContext().CreateScopeAsync()).UsingAsync(async scope =>
        {
            // Arrange
            var shellHostMock = new Mock<IShellHost>();

            shellHostMock.Setup(h => h.GetScopeAsync(It.IsAny<ShellSettings>()))
                .Returns(GetScopeAsync);

            var loggerMock = new Mock<ILogger<RecipeExecutor>>();
            var localizerMock = new Mock<IStringLocalizer<RecipeExecutor>>();

            localizerMock.Setup(localizer => localizer[It.IsAny<string>()])
            .Returns((string name) => new LocalizedString(name, name));

            var settings = new ContentPickerFieldLuceneEditorSettings
            {
                Index = "testIndex",
                Indices = ["idx1", "idx2", "testIndex"],
            };


            var indexes = new List<IndexEntity>
            {
                new IndexEntity
                {
                    Id = "idx1",
                    IndexName = "idx1",
                    IndexFullName = "idx1",
                    ProviderName = "Lucene",
                    Type = "Content",
                    Name = "Test Index",
                },
                new IndexEntity
                {
                    Id = "idx2",
                    IndexName = "idx2",
                    IndexFullName = "idx2",
                    ProviderName = "Lucene",
                    Type = "Content",
                    Name = "Test Index",
                },
                new IndexEntity
                {
                    Id = "testIndex",
                    IndexName = "testIndex",
                    IndexFullName = "testIndex",
                    ProviderName = "Lucene",
                    Type = "Content",
                    Name = "Test Index",
                },
            };

            var storeMock = new Mock<IIndexEntityStore>();
            storeMock.Setup(x => x.GetByProviderAsync(It.IsAny<string>()))
                .ReturnsAsync(indexes);

            var contentDefinition = DisplayDriverTestHelper.GetContentPartDefinition<ContentPickerField>(field => field.WithSettings(settings));

            // Act
            var shapeResult = await DisplayDriverTestHelper.GetShapeResultAsync(_shapeFactory, contentDefinition, new ContentPickerFieldLuceneEditorSettingsDriver(storeMock.Object));
            var shape = (ContentPickerFieldLuceneEditorSettings)shapeResult.Shape;

            // Assert
            Assert.Equal(settings.Index, shape.Index);
            Assert.Equal(settings.Indices, shape.Indices);
        });
    }

    private ShellContext CreateShellContext() => new()
    {
        Settings = new ShellSettings().AsDefaultShell().AsRunning(),
        ServiceProvider = _serviceProvider,
    };

    private static Task<ShellScope> GetScopeAsync()
        => ShellScope.Context.CreateScopeAsync();
}
