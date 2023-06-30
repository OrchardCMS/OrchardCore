using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Localization;
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
        private readonly IStringLocalizer S;

        public GraphQLContentTypePartSettingsDriver(IOptions<GraphQLContentOptions> optionsAccessor, IStringLocalizer<GraphQLContentTypePartSettingsDriver> stringLocalizer)
        {
            _contentOptions = optionsAccessor.Value;
            S = stringLocalizer;
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

                model.Settings.AvailablePreventFieldNameCollisionMethods = new List<SelectListItem>() {
                    new SelectListItem { Value = nameof(PreventFieldNameCollisionMethods.None), Text = S["Do nothing"] },
                    new SelectListItem { Value = nameof(PreventFieldNameCollisionMethods.AddPartNamePrefix), Text = S["Add Part Name Prefix"] },
                    new SelectListItem { Value = nameof(PreventFieldNameCollisionMethods.AddCustomPrefix), Text = S["Add Custom Prefix"] },
                    new SelectListItem { Value = nameof(PreventFieldNameCollisionMethods.AddCustomSuffix), Text = S["Add Custom Suffix"] }
                };
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
