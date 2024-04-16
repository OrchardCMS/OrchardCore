using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using OrchardCore.ContentManagement.GraphQL.Options;
using OrchardCore.ContentManagement.GraphQL.Settings;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.ContentTypes.GraphQL.ViewModels;
using OrchardCore.DisplayManagement.ModelBinding;
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

        public override IDisplayResult Edit(ContentTypePartDefinition contentTypePartDefinition, IUpdateModel updater)
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

                if (!updater.ModelState.IsValid)
                {
                    await updater.TryUpdateModelAsync(model, Prefix, x => x.Settings);
                }
            }).Location("Content");
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

            return Edit(contentTypePartDefinition, context.Updater);
        }
    }
}
