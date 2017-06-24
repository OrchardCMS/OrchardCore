using System.Threading.Tasks;
using Newtonsoft.Json;
using Orchard.ContentManagement.Display.ContentDisplay;
using Orchard.ContentManagement.Display.Models;
using Orchard.DisplayManagement.ModelBinding;
using Orchard.DisplayManagement.Views;
using Orchard.Media.Fields;
using Orchard.Media.ViewModels;

namespace Orchard.Media.Drivers
{
    public class MediaFieldDisplayDriver : ContentFieldDisplayDriver<MediaField>
    {
        public override IDisplayResult Display(MediaField field, BuildFieldDisplayContext context)
        {
            return Shape<DisplayMediaFieldViewModel>("MediaField", model =>
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
            return Shape<EditMediaFieldViewModel>("MediaField_Edit", model =>
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
            }

            return Edit(field, context);
        }
    }
}
