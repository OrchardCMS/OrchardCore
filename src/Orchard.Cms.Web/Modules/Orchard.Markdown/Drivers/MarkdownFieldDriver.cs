using System.Threading.Tasks;
using Orchard.ContentManagement.Display.ContentDisplay;
using Orchard.ContentManagement.Display.Models;
using Orchard.DisplayManagement.ModelBinding;
using Orchard.DisplayManagement.Views;
using Orchard.Markdown.Fields;
using Orchard.Markdown.ViewModels;

namespace Orchard.Markdown.Drivers
{
    public class MarkdownFieldDisplayDriver : ContentFieldDisplayDriver<MarkdownField>
    {
        public override IDisplayResult Display(MarkdownField field, BuildFieldDisplayContext context)
        {
            return Shape<DisplayMarkdownFieldViewModel>("MarkdownField", model =>
            {
                model.Field = field;
                model.Part = context.ContentPart;
                model.PartFieldDefinition = context.PartFieldDefinition;
            })
            .Location("Content")
            .Location("SummaryAdmin", "");
        }

        public override IDisplayResult Edit(MarkdownField field, BuildFieldEditorContext context)
        {
            return Shape<EditMarkdownFieldViewModel>("MarkdownField_Edit", model =>
            {
                model.Markdown = field.Markdown;
                model.Field = field;
                model.Part = context.ContentPart;
                model.PartFieldDefinition = context.PartFieldDefinition;
            });
        }

        public override async Task<IDisplayResult> UpdateAsync(MarkdownField field, IUpdateModel updater, UpdateFieldEditorContext context)
        {
            await updater.TryUpdateModelAsync(field, Prefix, f => f.Markdown);

            return Edit(field, context);
        }
    }
}
