using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using Orchard.ContentManagement.Metadata.Models;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentTypes.ViewModels;
using Orchard.DisplayManagement.Views;

namespace Orchard.ContentTypes.Editors
{
    public class DefaultContentTypeDisplayDriver : ContentTypeDisplayDriver
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
            return Shape<ContentTypeViewModel>("ContentType_Edit", model =>
            {
                model.DisplayName = contentTypeDefinition.DisplayName;
                return Task.CompletedTask;
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