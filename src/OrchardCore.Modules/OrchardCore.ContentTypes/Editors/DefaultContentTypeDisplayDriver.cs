using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.ViewModels;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.ContentTypes.Editors
{
    public class DefaultContentTypeDisplayDriver : ContentTypeDefinitionDisplayDriver
    {
        protected readonly IStringLocalizer S;

        public DefaultContentTypeDisplayDriver(IStringLocalizer<DefaultContentTypeDisplayDriver> localizer)
        {
            S = localizer;
        }

        public override IDisplayResult Edit(ContentTypeDefinition contentTypeDefinition)
        {
            return Initialize<ContentTypeViewModel>("ContentType_Edit", model =>
            {
                model.DisplayName = contentTypeDefinition.DisplayName;
                model.Name = contentTypeDefinition.Name;
            }).Location("Content");
        }

        public override async Task<IDisplayResult> UpdateAsync(ContentTypeDefinition contentTypeDefinition, UpdateTypeEditorContext context)
        {
            var model = new ContentTypeViewModel();

            await context.Updater.TryUpdateModelAsync(model, Prefix);

            context.Builder.DisplayedAs(model.DisplayName);

            if (String.IsNullOrWhiteSpace(model.DisplayName))
            {
                context.Updater.ModelState.AddModelError("DisplayName", S["The Content Type name can't be empty."]);
            }

            return Edit(contentTypeDefinition);
        }
    }
}
