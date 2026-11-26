using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.ContentManagement.GraphQL.Options;
using OrchardCore.ContentManagement.GraphQL.Settings;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.ContentTypes.GraphQL.ViewModels;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.ContentTypes.GraphQL.Drivers;

public sealed class GraphQLContentTypeSettingsDisplayDriver : ContentTypeDefinitionDisplayDriver
{
    private readonly GraphQLContentOptions _contentOptions;

    internal readonly IStringLocalizer S;

    public GraphQLContentTypeSettingsDisplayDriver(
        IOptions<GraphQLContentOptions> optionsAccessor,
        IStringLocalizer<GraphQLContentTypeSettingsDisplayDriver> stringLocalizer)
    {
        _contentOptions = optionsAccessor.Value;
        S = stringLocalizer;
    }

    public override IDisplayResult Edit(ContentTypeDefinition contentTypeDefinition, BuildEditorContext context)
    {
        return Initialize<GraphQLContentTypeSettingsViewModel>("GraphQLContentTypeSettings_Edit", model =>
        {
            model.Definition = contentTypeDefinition;
            model.Settings = contentTypeDefinition.GetSettings<GraphQLContentTypeSettings>();
            model.Options = _contentOptions;
        }).Location(l => l.Zone("Content", "5").Card(S["GraphQL"], "50"));
    }

    public override async Task<IDisplayResult> UpdateAsync(ContentTypeDefinition contentTypeDefinition, UpdateTypeEditorContext context)
    {
        var model = new GraphQLContentTypeSettingsViewModel();

        await context.Updater.TryUpdateModelAsync(model, Prefix);

        context.Builder.WithSettings(model.Settings);

        return Edit(contentTypeDefinition, context);
    }
}
