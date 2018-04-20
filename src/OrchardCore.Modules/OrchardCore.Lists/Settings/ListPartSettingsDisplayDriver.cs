using System;
using System.Collections.Specialized;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Lists.Models;
using OrchardCore.Lists.ViewModels;

namespace OrchardCore.Lists.Settings
{
    public class ListPartSettingsDisplayDriver : ContentTypePartDefinitionDisplayDriver
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;

        public ListPartSettingsDisplayDriver(
            IContentDefinitionManager contentDefinitionManager,
            IStringLocalizer<ListPartSettingsDisplayDriver> localizer)
        {
            _contentDefinitionManager = contentDefinitionManager;
            TS = localizer;
        }

        public IStringLocalizer TS { get; set; }

        public override IDisplayResult Edit(ContentTypePartDefinition contentTypePartDefinition, IUpdateModel updater)
        {
            if (!String.Equals(nameof(ListPart), contentTypePartDefinition.PartDefinition.Name, StringComparison.Ordinal))
            {
                return null;
            }

            return Initialize<ListPartSettingsViewModel>("ListPartSettings_Edit", model =>
            {
                model.ListPartSettings = contentTypePartDefinition.Settings.ToObject<ListPartSettings>();
                model.PageSize = model.ListPartSettings.PageSize;
                model.ContainedContentTypes = model.ListPartSettings.ContainedContentTypes;
                model.ContentTypes = new NameValueCollection();

                foreach(var contentTypeDefinition in _contentDefinitionManager.ListTypeDefinitions())
                {
                    model.ContentTypes.Add(contentTypeDefinition.Name, contentTypeDefinition.DisplayName);
                }
            }).Location("Content");
        }

        public override async Task<IDisplayResult> UpdateAsync(ContentTypePartDefinition contentTypePartDefinition, UpdateTypePartEditorContext context)
        {
            if (!String.Equals(nameof(ListPart), contentTypePartDefinition.PartDefinition.Name, StringComparison.Ordinal))
            {
                return null;
            }

            var model = new ListPartSettingsViewModel();

            await context.Updater.TryUpdateModelAsync(model, Prefix, m => m.ContainedContentTypes, m => m.PageSize);

            if (model.ContainedContentTypes == null || model.ContainedContentTypes.Length == 0)
            {
                context.Updater.ModelState.AddModelError(nameof(model.ContainedContentTypes), TS["At least one content type must be selected."]);
            }
            else
            {
                context.Builder.WithSetting("PageSize", model.PageSize.ToString());
                context.Builder.ContainedContentTypes(model.ContainedContentTypes);
            }

            return Edit(contentTypePartDefinition, context.Updater);
        }
    }
}