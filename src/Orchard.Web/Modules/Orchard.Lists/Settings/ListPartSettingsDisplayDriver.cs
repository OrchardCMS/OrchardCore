using System;
using System.Threading.Tasks;
using Orchard.Lists.Models;
using Orchard.ContentTypes.Editors;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.DisplayManagement.Views;
using Orchard.DisplayManagement.ModelBinding;

namespace Orchard.Lists.Settings
{
    public class ListPartSettingsDisplayDriver : ContentTypePartDisplayDriver
    {
        public override IDisplayResult Edit(ContentTypePartDefinition contentTypePartDefinition, IUpdateModel updater)
        {
            if (!String.Equals("ListPart", contentTypePartDefinition.PartDefinition.Name, StringComparison.Ordinal))
            {
                return null;
            }

            return Shape<ListPartSettings>("ListPartSettings_Edit", model =>
            {
                var settings = contentTypePartDefinition.Settings.ToObject<ListPartSettings>();

                model.ContainedContentType = settings.ContainedContentType;

                return Task.CompletedTask;
            }).Location("Content");
        }

        public override async Task<IDisplayResult> UpdateAsync(ContentTypePartDefinition contentTypePartDefinition, UpdatePartEditorContext context)
        {
            if (!String.Equals("ListPart", contentTypePartDefinition.PartDefinition.Name, StringComparison.Ordinal))
            {
                return null;
            }

            var model = new ListPartSettings();

            await context.Updater.TryUpdateModelAsync(model, Prefix);

            context.Builder.ContainedContentType(model.ContainedContentType);

            return Edit(contentTypePartDefinition, context.Updater);
        }
    }
}