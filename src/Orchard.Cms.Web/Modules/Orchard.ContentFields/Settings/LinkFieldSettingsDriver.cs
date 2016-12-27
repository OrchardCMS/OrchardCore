using System.Threading.Tasks;
using Orchard.ContentFields.Fields;
using Orchard.ContentManagement.Metadata.Models;
using Orchard.ContentTypes.Editors;
using Orchard.DisplayManagement.Views;

namespace Orchard.ContentFields.Settings
{
    public class LinkFieldSettingsDriver : ContentPartFieldDisplayDriver<LinkField>
    {
        public override IDisplayResult Edit(ContentPartFieldDefinition partFieldDefinition)
        {
            return Shape<LinkFieldSettings>("LinkFieldSettings_Edit", model => partFieldDefinition.Settings.Populate(model))
                .Location("Content");
        }

        public override async Task<IDisplayResult> UpdateAsync(ContentPartFieldDefinition partFieldDefinition, UpdatePartFieldEditorContext context)
        {
            var model = new LinkFieldSettings();

            await context.Updater.TryUpdateModelAsync(model, Prefix);

            context.Builder.Hint(model.Hint);
            context.Builder.WithSetting(nameof(model.HintLinkText), model.HintLinkText);
            context.Builder.WithSetting(nameof(model.Required), model.Required.ToString());
            context.Builder.WithSetting(nameof(model.DefaultValue), model.DefaultValue);
            context.Builder.WithSetting(nameof(model.TextDefaultValue), model.TextDefaultValue);
            context.Builder.WithSetting(nameof(model.TargetMode), model.TargetMode.ToString());
            context.Builder.WithSetting(nameof(model.LinkTextMode), model.LinkTextMode.ToString());
            context.Builder.WithSetting(nameof(model.UrlPlaceholder), model.UrlPlaceholder);
            context.Builder.WithSetting(nameof(model.TextPlaceholder), model.TextPlaceholder);

            return Edit(partFieldDefinition);
        }
    }
}
