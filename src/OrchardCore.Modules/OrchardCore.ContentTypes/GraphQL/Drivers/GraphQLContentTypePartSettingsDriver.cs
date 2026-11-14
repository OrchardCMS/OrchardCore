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

public sealed class GraphQLContentTypePartSettingsDriver : ContentTypePartDefinitionDisplayDriver
{
    private readonly GraphQLContentOptions _contentOptions;

    internal readonly IStringLocalizer S;

    public GraphQLContentTypePartSettingsDriver(
        IOptions<GraphQLContentOptions> optionsAccessor,
        IStringLocalizer<GraphQLContentTypePartSettingsDriver> stringLocalizer)
    {
        _contentOptions = optionsAccessor.Value;
        S = stringLocalizer;
    }

    public override IDisplayResult Edit(ContentTypePartDefinition contentTypePartDefinition, BuildEditorContext context)
    {
        if (contentTypePartDefinition.ContentTypeDefinition.Name == contentTypePartDefinition.PartDefinition.Name)
        {
            return null;
        }

        return Initialize<GraphQLContentTypePartSettingsViewModel>("GraphQLContentTypePartSettings_Edit", async model =>
        {
            model.Definition = contentTypePartDefinition;
            model.Options = _contentOptions;
            model.Settings = contentTypePartDefinition.GetSettings<GraphQLContentTypePartSettings>();

            if (!context.Updater.ModelState.IsValid)
            {
                await context.Updater.TryUpdateModelAsync(model, Prefix,
                    m => m.Settings);
            }
        }).Location(l => l.Zone("Content").Card(S["GraphQL"], "50"));
    }

    public override async Task<IDisplayResult> UpdateAsync(ContentTypePartDefinition contentTypePartDefinition, UpdateTypePartEditorContext context)
    {
        if (contentTypePartDefinition.ContentTypeDefinition.Name == contentTypePartDefinition.PartDefinition.Name)
        {
            return null;
        }

        var model = new GraphQLContentTypePartSettingsViewModel();

        await context.Updater.TryUpdateModelAsync(model, Prefix, m => m.Settings);

        context.Builder.WithSettings(model.Settings);

        return Edit(contentTypePartDefinition, context);
    }
}
