using System.Collections.Generic;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.ViewModels;
using Orchard.ContentTypes.ViewModels;
using Orchard.ContentTypes.Editors;
using Orchard.DisplayManagement.ModelBinding;
using Orchard.ContentManagement.Metadata.Settings;
using Orchard.ContentManagement.MetaData.Settings;

namespace Orchard.ContentTypes.Settings
{
    public class EditorEvents : ContentDefinitionEditorEventsBase
    {

        public override IEnumerable<TemplateViewModel> TypeEditor(ContentTypeDefinition definition)
        {
            var settings = definition.Settings.ToObject<ContentTypeSettings>();
            var model = new ContentTypeSettingsViewModel
            {
                Creatable = settings.Creatable,
                Listable = settings.Listable,
                Draftable = settings.Draftable,
                Securable = settings.Securable,
            };

            model.Stereotype = definition.Settings.Value<string>("Stereotype") ?? "";

            yield return DefinitionTemplate(model);
        }

        public override IEnumerable<TemplateViewModel> TypeEditorUpdate(ContentTypeDefinitionBuilder builder, IUpdateModel updateModel)
        {
            var model = new ContentTypeSettingsViewModel();
            updateModel.TryUpdateModelAsync(model, "ContentTypeSettingsViewModel").Wait();

            builder.Creatable(model.Creatable);
            builder.Listable(model.Listable);
            builder.Draftable(model.Draftable);
            builder.Securable(model.Securable);
            builder.WithSetting("Stereotype", model.Stereotype);

            yield return DefinitionTemplate(model);
        }

        public override IEnumerable<TemplateViewModel> PartEditor(ContentPartDefinition definition)
        {
            var model = definition.Settings.ToObject<ContentPartSettings>();
            yield return DefinitionTemplate(model);
        }

        public override IEnumerable<TemplateViewModel> PartEditorUpdate(ContentPartDefinitionBuilder builder, IUpdateModel updateModel)
        {
            var model = new ContentPartSettings();
            updateModel.TryUpdateModelAsync(model, "ContentPartSettings").Wait();
            builder.Attachable(model.Attachable);
            builder.WithDescription(model.Description);
            yield return DefinitionTemplate(model);
        }
    }
}