using System;
using System.Threading.Tasks;
using Orchard.Lists.Models;
using Orchard.ContentTypes.Editors;
using Orchard.ContentManagement.Metadata.Models;
using Orchard.DisplayManagement.Views;
using Orchard.DisplayManagement.ModelBinding;
using Orchard.ContentManagement.MetaData;
using Microsoft.Extensions.Localization;
using Orchard.Lists.ViewModels;
using System.Collections.Specialized;

namespace Orchard.Lists.Settings
{
    public class ListPartSettingsDisplayDriver : ContentTypePartDisplayDriver
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
            if (!String.Equals("ListPart", contentTypePartDefinition.PartDefinition.Name, StringComparison.Ordinal))
            {
                return null;
            }

            return Shape<ListPartSettingsViewModel>("ListPartSettings_Edit", model =>
            {
                model.ListPartSettings = contentTypePartDefinition.Settings.ToObject<ListPartSettings>();
                model.ContainedContentType = model.ListPartSettings.ContainedContentType;
                model.ContentTypes = new NameValueCollection();

                foreach(var contentTypeDefinition in _contentDefinitionManager.ListTypeDefinitions())
                {
                    model.ContentTypes.Add(contentTypeDefinition.Name, contentTypeDefinition.DisplayName);
                }

                return Task.CompletedTask;
            }).Location("Content");
        }

        public override async Task<IDisplayResult> UpdateAsync(ContentTypePartDefinition contentTypePartDefinition, UpdateTypePartEditorContext context)
        {
            if (!String.Equals("ListPart", contentTypePartDefinition.PartDefinition.Name, StringComparison.Ordinal))
            {
                return null;
            }

            var model = new ListPartSettingsViewModel();

            await context.Updater.TryUpdateModelAsync(model, Prefix, m => m.ContainedContentType);

            if (_contentDefinitionManager.GetTypeDefinition(model.ContainedContentType) == null)
            {
                context.Updater.ModelState.AddModelError(nameof(model.ContainedContentType), TS["The content type could not be found"]);
            }
            else
            {
                context.Builder.ContainedContentType(model.ContainedContentType);
            }

            return Edit(contentTypePartDefinition, context.Updater);
        }
    }
}