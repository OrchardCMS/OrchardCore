using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.ViewModels;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.ContentTypes.Editors;

/// <summary>
/// Renders the parts of a content type, which can be reordered, in the "Parts" card of the content type editor.
/// </summary>
internal sealed class ContentTypePartsDisplayDriver : ContentTypeDefinitionDisplayDriver
{
    internal readonly IStringLocalizer S;

    public ContentTypePartsDisplayDriver(IStringLocalizer<ContentTypePartsDisplayDriver> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public override IDisplayResult Edit(ContentTypeDefinition contentTypeDefinition, BuildEditorContext context)
    {
        return Initialize<EditTypeViewModel>("ContentTypeParts_Edit", model =>
        {
            model.Name = contentTypeDefinition.Name;
            model.DisplayName = contentTypeDefinition.DisplayName;
            model.TypeDefinition = contentTypeDefinition;
        }).Location(l => l.Zone("Content", "10").Card(S["Parts"], "80"));
    }

    // The order of the parts is posted as 'OrderedPartNames', which the controller binds.
    protected override void BuildPrefix(ContentTypeDefinition model, string htmlFieldPrefix)
    {
        Prefix = htmlFieldPrefix ?? string.Empty;
    }
}
