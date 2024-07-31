using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using OrchardCore.ContentManagement.GraphQL.Options;
using OrchardCore.ContentManagement.GraphQL.Settings;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.ContentTypes.GraphQL.ViewModels;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.ContentTypes.GraphQL.Drivers
{
    public class GraphQLContentTypePartSettingsDriver : ContentTypePartDefinitionDisplayDriver
    {
        private readonly GraphQLContentOptions _contentOptions;

        public GraphQLContentTypePartSettingsDriver(IOptions<GraphQLContentOptions> optionsAccessor)
        {
            _contentOptions = optionsAccessor.Value;
        }

        public override Task<IDisplayResult> EditAsync(ContentTypePartDefinition contentTypePartDefinition, BuildEditorContext context)
        {
            if (contentTypePartDefinition.ContentTypeDefinition.Name == contentTypePartDefinition.PartDefinition.Name)
            {
                return Task.FromResult<IDisplayResult>(null);
            }

            return Task.FromResult<IDisplayResult>(
                Initialize<GraphQLContentTypePartSettingsViewModel>("GraphQLContentTypePartSettings_Edit", async model =>
                {
                    model.Definition = contentTypePartDefinition;
                    model.Options = _contentOptions;
                    model.Settings = contentTypePartDefinition.GetSettings<GraphQLContentTypePartSettings>();

                    if (!context.Updater.ModelState.IsValid)
                    {
                        await context.Updater.TryUpdateModelAsync(model, Prefix, x => x.Settings);
                    }
                }).Location("Content")
            );
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

            return await EditAsync(contentTypePartDefinition, context);
        }
    }
}
