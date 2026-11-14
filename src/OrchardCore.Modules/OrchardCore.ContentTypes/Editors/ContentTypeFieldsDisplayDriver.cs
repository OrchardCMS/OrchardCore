using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.ViewModels;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.ContentTypes.Editors;

/// <summary>
/// Renders the fields of a content type, which can be reordered, in the "Fields" card of the content type editor.
/// </summary>
internal sealed class ContentTypeFieldsDisplayDriver : ContentTypeDefinitionDisplayDriver
{
    private readonly ContentOptions _contentOptions;

    internal readonly IStringLocalizer S;

    public ContentTypeFieldsDisplayDriver(
        IOptions<ContentOptions> contentOptions,
        IStringLocalizer<ContentTypeFieldsDisplayDriver> stringLocalizer)
    {
        _contentOptions = contentOptions.Value;
        S = stringLocalizer;
    }

    public override IDisplayResult Edit(ContentTypeDefinition contentTypeDefinition, BuildEditorContext context)
    {
        var typePart = contentTypeDefinition.Parts.FirstOrDefault(x => string.Equals(x.Name, contentTypeDefinition.Name, StringComparison.OrdinalIgnoreCase));

        // The card is rendered when the type has fields, or when fields can be added to it.
        if (typePart?.PartDefinition?.Fields.Any() != true && _contentOptions.ContentFieldOptions.Count == 0)
        {
            return null;
        }

        return Initialize<EditTypeViewModel>("ContentTypeFields_Edit", model =>
        {
            model.Name = contentTypeDefinition.Name;
            model.DisplayName = contentTypeDefinition.DisplayName;
            model.TypeDefinition = contentTypeDefinition;
        }).Location(l => l.Zone("Content", "10").Card(S["Fields"], "70"));
    }

    // The order of the fields is posted as 'OrderedFieldNames', which the controller binds.
    protected override void BuildPrefix(ContentTypeDefinition model, string htmlFieldPrefix)
    {
        Prefix = htmlFieldPrefix ?? string.Empty;
    }
}
