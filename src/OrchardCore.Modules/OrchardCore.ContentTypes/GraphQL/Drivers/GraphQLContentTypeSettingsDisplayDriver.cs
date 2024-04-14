using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using OrchardCore.ContentManagement.GraphQL.Options;
using OrchardCore.ContentManagement.GraphQL.Settings;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.ContentTypes.GraphQL.ViewModels;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.ContentTypes.GraphQL.Drivers;

public class GraphQLContentTypeSettingsDisplayDriver : ContentTypeDefinitionDisplayDriver
{
    private readonly GraphQLContentOptions _contentOptions;

    public GraphQLContentTypeSettingsDisplayDriver(IOptions<GraphQLContentOptions> optionsAccessor)
    {
        _contentOptions = optionsAccessor.Value;
    }

    public override IDisplayResult Edit(ContentTypeDefinition contentTypeDefinition)
    {
        return Initialize<GraphQLContentTypeSettingsViewModel>("GraphQLContentTypeSettings_Edit", model =>
        {
            model.Definition = contentTypeDefinition;
            model.Settings = contentTypeDefinition.GetSettings<GraphQLContentTypeSettings>();
            model.Options = _contentOptions;
        }).Location("Content:5");
    }

    public override async Task<IDisplayResult> UpdateAsync(ContentTypeDefinition contentTypeDefinition, UpdateTypeEditorContext context)
    {
        var model = new GraphQLContentTypeSettingsViewModel();

        await context.Updater.TryUpdateModelAsync(model, Prefix);

        context.Builder.WithSettings(model.Settings);

        return Edit(contentTypeDefinition);
    }
}
