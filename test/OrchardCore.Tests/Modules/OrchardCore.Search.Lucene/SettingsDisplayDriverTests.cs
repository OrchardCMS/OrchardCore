using OrchardCore.ContentFields.Fields;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Implementation;
using OrchardCore.DisplayManagement.Theming;
using OrchardCore.Documents;
using OrchardCore.Environment.Extensions;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Builders;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.Locking;
using OrchardCore.Locking.Distributed;
using OrchardCore.Recipes.Services;
using OrchardCore.Scripting;
using OrchardCore.Search.Lucene;
using OrchardCore.Search.Lucene.Model;
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
            .AddScoped<IDocumentManager<LuceneIndexSettingsDocument>, MockLuceneIndexSettingsDocumentManager>()
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

            // Act
            var contentDefinition = DisplayDriverTestHelper.GetContentPartDefinition<ContentPickerField>(field => field.WithSettings(settings));
            var luceneService = new LuceneIndexSettingsService(new MockLuceneIndexSettingsDocumentManager());
            var shapeResult = await DisplayDriverTestHelper.GetShapeResultAsync(_shapeFactory, contentDefinition, new ContentPickerFieldLuceneEditorSettingsDriver(luceneService));
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
