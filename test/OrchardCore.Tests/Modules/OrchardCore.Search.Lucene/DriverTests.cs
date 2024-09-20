using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentManagement.Metadata.Builders;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Implementation;
using OrchardCore.DisplayManagement.Theming;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.DisplayManagement.Zones;
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
using OrchardCore.Tests.Stubs;

namespace OrchardCore.Tests.Modules.OrchardCore.Search.Lucene;

public partial class DriverTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ShapeTable _shapeTable;

    public DriverTests()
    {
        IServiceCollection serviceCollection = new ServiceCollection();

        serviceCollection.AddScripting();
        serviceCollection.AddLogging();

        serviceCollection.AddScoped<ILoggerFactory, NullLoggerFactory>();
        serviceCollection.AddScoped<IThemeManager, ThemeManager>();
        serviceCollection.AddScoped<IShapeFactory, DefaultShapeFactory>();
        serviceCollection.AddScoped<IExtensionManager, StubExtensionManager>();
        serviceCollection.AddScoped<IShapeTableManager, TestShapeTableManager>();
        serviceCollection.AddScoped<IDocumentManager<LuceneIndexSettingsDocument>, MockLuceneIndexSettingsDocumentManager>();
        _shapeTable = new ShapeTable
        (
            new Dictionary<string, ShapeDescriptor>(StringComparer.OrdinalIgnoreCase),
            new Dictionary<string, ShapeBinding>(StringComparer.OrdinalIgnoreCase)
        );

        serviceCollection.AddSingleton(_shapeTable);
        serviceCollection.AddSingleton<IDistributedLock, LocalLock>();
        _serviceProvider = serviceCollection.BuildServiceProvider();
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

            // Act
            var contentDefinition =
                BuildContentPartWithField(field => field
                            .OfType(nameof(ContentPickerField))
                            .WithSettings(new ContentPickerFieldLuceneEditorSettings
                            {
                                Index = "testIndex",
                                Indices = ["idx1", "idx2", "testIndex"]
                            }));
            var luceneService = new LuceneIndexSettingsService(new MockLuceneIndexSettingsDocumentManager());
            var shapeResult = await GetShapeResult(contentDefinition, new ContentPickerFieldLuceneEditorSettingsDriver(luceneService));
            var settings = (ContentPickerFieldLuceneEditorSettings)shapeResult.Shape;

            // Assert
            Assert.Equal("testIndex", settings.Index);
            Assert.Equal<string[]>(["idx1", "idx2", "testIndex"], settings.Indices);
        });
    }

    #region Private methods
    private ShellContext CreateShellContext() => new()
    {
        Settings = new ShellSettings().AsDefaultShell().AsRunning(),
        ServiceProvider = _serviceProvider,
    };

    private static Task<ShellScope> GetScopeAsync() => ShellScope.Context.CreateScopeAsync();

    private static ContentPartDefinition BuildContentPartWithField(Action<ContentPartFieldDefinitionBuilder> configuration)
    {        
        return new ContentPartDefinitionBuilder()
                .Named("SomeContentPart")
                .WithField("SomeField",
                    configuration)
                .Build();
    }
    private async Task<ShapeResult> GetShapeResult(ContentPartDefinition contentDefinition, IContentPartFieldDefinitionDisplayDriver driver)
    {
        var factory = _serviceProvider.GetService<IShapeFactory>();
        var partFieldDefinition = contentDefinition.Fields.First();

        var partFieldDefinitionShape = await factory.CreateAsync("ContentPartFieldDefinition_Edit", () =>
            ValueTask.FromResult<IShape>(new ZoneHolding(() => factory.CreateAsync("ContentZone"))));
        partFieldDefinitionShape.Properties["ContentField"] = partFieldDefinition;

        var editorContext = new BuildEditorContext(partFieldDefinitionShape, "", false, "", factory, null, null);

        var result = await driver.BuildEditorAsync(partFieldDefinition, editorContext);
        await result.ApplyAsync(editorContext);

        return (ShapeResult)result;
    }
    #endregion
}
