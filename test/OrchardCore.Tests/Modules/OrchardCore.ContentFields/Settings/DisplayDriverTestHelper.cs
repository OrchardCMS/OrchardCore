using OrchardCore.ContentManagement.Metadata.Builders;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.DisplayManagement.Zones;
using OrchardCore.Localization;

namespace OrchardCore.Tests.Modules.OrchardCore.ContentFields.Settings;

public class DisplayDriverTestHelper
{
    // The services the drivers depend on, e.g. the localizer of the names of their cards.
    private static readonly IServiceProvider _services = new ServiceCollection()
        .AddSingleton<IStringLocalizerFactory, NullStringLocalizerFactory>()
        .AddTransient(typeof(IStringLocalizer<>), typeof(StringLocalizer<>))
        .BuildServiceProvider();

    public static ContentPartDefinition GetContentPartDefinition<TField>(Action<ContentPartFieldDefinitionBuilder> configuration)
    {
        return new ContentPartDefinitionBuilder()
            .Named("SomeContentPart")
            .WithField<TField>("SomeField", configuration)
            .Build();
    }

    public static Task<ShapeResult> GetShapeResultAsync(IContentPartFieldDefinitionDisplayDriver driver, IShapeFactory factory, ContentPartDefinition contentDefinition)
        => GetShapeResultAsync(factory, contentDefinition, driver);

    public static Task<ShapeResult> GetShapeResultAsync<TDriver>(IShapeFactory factory, ContentPartDefinition contentDefinition)
        where TDriver : IContentPartFieldDefinitionDisplayDriver
        => GetShapeResultAsync(factory, contentDefinition, ActivatorUtilities.CreateInstance<TDriver>(_services));

    public static async Task<ShapeResult> GetShapeResultAsync(IShapeFactory factory, ContentPartDefinition contentDefinition, IContentPartFieldDefinitionDisplayDriver driver)
    {
        var partFieldDefinition = contentDefinition.Fields.First();

        var partFieldDefinitionShape = await factory.CreateAsync("ContentPartFieldDefinition_Edit", () =>
            ValueTask.FromResult<IShape>(new ZoneHolding(() => factory.CreateAsync("ContentZone"))));
        partFieldDefinitionShape.Properties["ContentField"] = partFieldDefinition;

        var editorContext = new BuildEditorContext(partFieldDefinitionShape, "", false, "", factory, null, null);

        var result = await driver.BuildEditorAsync(partFieldDefinition, editorContext);
        await result.ApplyAsync(editorContext);

        return (ShapeResult)result;
    }
}
