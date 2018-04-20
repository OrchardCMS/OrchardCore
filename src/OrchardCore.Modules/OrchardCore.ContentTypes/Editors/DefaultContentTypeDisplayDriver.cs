using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentTypes.ViewModels;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.ContentTypes.Editors
{
    public class DefaultContentTypeDisplayDriver : ContentTypeDefinitionDisplayDriver
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;

        public DefaultContentTypeDisplayDriver(
            IContentDefinitionManager contentDefinitionManager,
            IStringLocalizer<DefaultContentDefinitionDisplayManager> localizer)
        {
            _contentDefinitionManager = contentDefinitionManager;
            T = localizer;
        }

        public IStringLocalizer T { get; }

        public override IDisplayResult Edit(ContentTypeDefinition contentTypeDefinition)
        {
            return Initialize<ContentTypeViewModel>("ContentType_Edit", model =>
            {
                model.DisplayName = contentTypeDefinition.DisplayName;
            }).Location("Content");
        }

        public override async Task<IDisplayResult> UpdateAsync(ContentTypeDefinition contentTypeDefinition, UpdateTypeEditorContext context)
        {
            var model = new ContentTypeViewModel();

            await context.Updater.TryUpdateModelAsync(model, Prefix);

            context.Builder.DisplayedAs(model.DisplayName);

            if (String.IsNullOrWhiteSpace(model.DisplayName))
            {
                context.Updater.ModelState.AddModelError("DisplayName", T["The Content Type name can't be empty."]);
            }

            return Edit(contentTypeDefinition, context.Updater);
        }
    }
}