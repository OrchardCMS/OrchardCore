using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentTypes.ViewModels;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.ContentTypes.Editors
{
    public class DefaultContentPartDisplayDriver : ContentPartDisplayDriver
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;

        public DefaultContentPartDisplayDriver(
            IContentDefinitionManager contentDefinitionManager,
            IStringLocalizer<DefaultContentDefinitionDisplayManager> localizer)
        {
            _contentDefinitionManager = contentDefinitionManager;
            T = localizer;
        }

        public IStringLocalizer T { get; }

        public override IDisplayResult Edit(ContentPartDefinition contentPartDefinition)
        {
            return Shape<ContentPartViewModel>("ContentPart_Edit", model =>
            {
                model.DisplayName = contentPartDefinition.DisplayName;
                return Task.CompletedTask;
            }).Location("Content:before");
        }

        public override async Task<IDisplayResult> UpdateAsync(ContentPartDefinition contentPartDefinition, UpdatePartEditorContext context)
        {
            var model = new ContentPartViewModel();

            await context.Updater.TryUpdateModelAsync(model, Prefix);

            context.Builder.DisplayedAs(model.DisplayName);

            if (String.IsNullOrWhiteSpace(model.DisplayName))
            {
                context.Updater.ModelState.AddModelError("DisplayName", T["The Content Part display name can't be empty."]);
            }

            return Edit(contentPartDefinition, context.Updater);
        }
    }
}