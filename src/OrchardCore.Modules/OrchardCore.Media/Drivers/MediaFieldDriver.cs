using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Media.Fields;
using OrchardCore.Media.Settings;
using OrchardCore.Media.ViewModels;

namespace OrchardCore.Media.Drivers
{
    public class MediaFieldDisplayDriver : ContentFieldDisplayDriver<MediaField>
    {
        public MediaFieldDisplayDriver(IStringLocalizer<MediaFieldDisplayDriver> localizer)
        {
            S = localizer;
        }

        public IStringLocalizer S { get; set; }

        public override IDisplayResult Display(MediaField field, BuildFieldDisplayContext context)
        {
            return Initialize<DisplayMediaFieldViewModel>("MediaField", model =>
            {
                model.Field = field;
                model.Part = context.ContentPart;
                model.PartFieldDefinition = context.PartFieldDefinition;
            })
            .Location("Content")
            .Location("SummaryAdmin", "");
        }

        public override IDisplayResult Edit(MediaField field, BuildFieldEditorContext context)
        {
            return Initialize<EditMediaFieldViewModel>(GetEditorShapeType(context), model =>
            {
                model.Paths = JsonConvert.SerializeObject(field.Paths);

                model.Field = field;
                model.Part = context.ContentPart;
                model.PartFieldDefinition = context.PartFieldDefinition;
            });
        }

        public override async Task<IDisplayResult> UpdateAsync(MediaField field, IUpdateModel updater, UpdateFieldEditorContext context)
        {
            var model = new EditMediaFieldViewModel();

            if (await updater.TryUpdateModelAsync(model, Prefix, f => f.Paths))
            {
                field.Paths = JsonConvert.DeserializeObject<string[]>(model.Paths);

                var settings = context.PartFieldDefinition.Settings.ToObject<MediaFieldSettings>();
                
                if (settings.Required && field.Paths.Length < 1)
                {
                    updater.ModelState.AddModelError(Prefix, S["{0}: A media is required.", context.PartFieldDefinition.DisplayName()]);
                }

                if (field.Paths.Length > 1 && !settings.Multiple)
                {
                    updater.ModelState.AddModelError(Prefix, S["{0}: Selecting multiple media is forbidden.", context.PartFieldDefinition.DisplayName()]);
                }                
            }

            return Edit(field, context);
        }
    }
}
