using OrchardCore.ContentManagement.Metadata.Builders;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.DisplayManagement.Zones;

namespace OrchardCore.Tests.Modules.OrchardCore.ContentFields.Settings;

public class DisplayDriverTestHelper
{
    public static ContentPartDefinition GetContentPartDefinition<TField>(Action<ContentPartFieldDefinitionBuilder> configuration)
    {
        return new ContentPartDefinitionBuilder()
            .Named("SomeContentPart")
            .WithField<TField>("SomeField", configuration)
            .Build();
    }

    public static Task<ShapeResult> GetShapeResultAsync<TDriver>(IShapeFactory factory, ContentPartDefinition contentDefinition)
        where TDriver : IContentPartFieldDefinitionDisplayDriver, new()
        => GetShapeResultAsync(factory, contentDefinition, new TDriver());

    public static async Task<ShapeResult> GetShapeResultAsync(IShapeFactory factory, ContentPartDefinition contentDefinition, IContentPartFieldDefinitionDisplayDriver driver)
    {
        var partFieldDefinition = contentDefinition.Fields.First();

        var partFieldDefinitionShape = await factory.CreateAsync("ContentPartFieldDefinition_Edit", () =>
            ValueTask.FromResult<IShape>(new ZoneHolding(() => factory.CreateAsync("ContentZone")))).ConfigureAwait(false);
        partFieldDefinitionShape.Properties["ContentField"] = partFieldDefinition;

        var editorContext = new BuildEditorContext(partFieldDefinitionShape, "", false, "", factory, null, null);

        var result = await driver.BuildEditorAsync(partFieldDefinition, editorContext).ConfigureAwait(false);
        await result.ApplyAsync(editorContext).ConfigureAwait(false);

        return (ShapeResult)result;
    }
}
