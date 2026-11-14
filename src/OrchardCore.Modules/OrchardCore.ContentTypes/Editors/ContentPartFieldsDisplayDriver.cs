using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.ViewModels;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.ContentTypes.Editors;

/// <summary>
/// Renders the fields of a content part, which can be reordered, in the "Fields" card of the content part editor.
/// </summary>
internal sealed class ContentPartFieldsDisplayDriver : ContentPartDefinitionDisplayDriver
{
    private readonly ContentOptions _contentOptions;

    internal readonly IStringLocalizer S;

    public ContentPartFieldsDisplayDriver(
        IOptions<ContentOptions> contentOptions,
        IStringLocalizer<ContentPartFieldsDisplayDriver> stringLocalizer)
    {
        _contentOptions = contentOptions.Value;
        S = stringLocalizer;
    }

    public override IDisplayResult Edit(ContentPartDefinition contentPartDefinition, BuildEditorContext context)
    {
        // The card is rendered when the part has fields, or when fields can be added to it.
        if (!contentPartDefinition.Fields.Any() && _contentOptions.ContentFieldOptions.Count == 0)
        {
            return null;
        }

        return Initialize<ContentPartFieldsViewModel>("ContentPartFields_Edit", model =>
        {
            model.PartDefinition = contentPartDefinition;
        }).Location(l => l.Zone("Content", "10").Card(S["Fields"], "70"));
    }

    // The order of the fields is posted as 'OrderedFieldNames', which the controller binds.
    protected override void BuildPrefix(ContentPartDefinition model, string htmlFieldPrefix)
    {
        Prefix = htmlFieldPrefix ?? string.Empty;
    }
}
