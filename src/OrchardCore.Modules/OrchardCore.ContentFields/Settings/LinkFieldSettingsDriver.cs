using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.ContentFields.Settings
{
    public class LinkFieldSettingsDriver : ContentPartFieldDefinitionDisplayDriver<LinkField>
    {
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        public LinkFieldSettingsDriver(IOptions<JsonSerializerOptions> jsonSerializerOptions)
        {
            _jsonSerializerOptions = jsonSerializerOptions.Value;
        }

        public override IDisplayResult Edit(ContentPartFieldDefinition partFieldDefinition)
        {
            return Initialize<LinkFieldSettings>("LinkFieldSettings_Edit", model =>
            {
                var settings = partFieldDefinition.Settings.ToObject<LinkFieldSettings>(_jsonSerializerOptions);

                model.Hint = settings.Hint;
                model.HintLinkText = settings.HintLinkText;
                model.Required = settings.Required;
                model.LinkTextMode = settings.LinkTextMode;
                model.UrlPlaceholder = settings.UrlPlaceholder;
                model.TextPlaceholder = settings.TextPlaceholder;
                model.DefaultUrl = settings.DefaultUrl;
                model.DefaultText = settings.DefaultText;
            })
                .Location("Content");
        }

        public override async Task<IDisplayResult> UpdateAsync(ContentPartFieldDefinition partFieldDefinition, UpdatePartFieldEditorContext context)
        {
            var model = new LinkFieldSettings();

            await context.Updater.TryUpdateModelAsync(model, Prefix);

            context.Builder.WithSettings(model);

            return Edit(partFieldDefinition);
        }
    }
}
