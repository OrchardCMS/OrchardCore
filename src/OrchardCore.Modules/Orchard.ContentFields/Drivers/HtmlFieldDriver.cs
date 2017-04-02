using System.Threading.Tasks;
using Orchard.ContentFields.ViewModels;
using Orchard.ContentManagement.Display.ContentDisplay;
using Orchard.ContentManagement.Display.Models;
using Orchard.DisplayManagement.ModelBinding;
using Orchard.DisplayManagement.Views;

namespace Orchard.ContentFields.Fields
{
    public class HtmlFieldDisplayDriver : ContentFieldDisplayDriver<HtmlField>
    {
        public override IDisplayResult Display(HtmlField field, BuildFieldDisplayContext context)
        {
            return Shape<DisplayHtmlFieldViewModel>("HtmlField", model =>
            {
                model.Field = field;
                model.Part = context.ContentPart;
                model.PartFieldDefinition = context.PartFieldDefinition;
            })
            .Location("Content")
            .Location("SummaryAdmin", "");
        }

        public override IDisplayResult Edit(HtmlField field, BuildFieldEditorContext context)
        {
            return Shape<EditHtmlFieldViewModel>("HtmlField_Edit", model =>
            {
                model.Html = field.Html;
                model.Field = field;
                model.Part = context.ContentPart;
                model.PartFieldDefinition = context.PartFieldDefinition;
            });
        }

        public override async Task<IDisplayResult> UpdateAsync(HtmlField field, IUpdateModel updater, UpdateFieldEditorContext context)
        {
            await updater.TryUpdateModelAsync(field, Prefix, f => f.Html);

            return Edit(field, context);
        }
    }
}
