using System.IO;
using System.Threading.Tasks;
using Fluid;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Liquid;
using OrchardCore.Markdown.Fields;
using OrchardCore.Markdown.ViewModels;

namespace OrchardCore.Markdown.Drivers
{
    public class MarkdownFieldDisplayDriver : ContentFieldDisplayDriver<MarkdownField>
    {
        private readonly ILiquidTemplateManager _liquidTemplatemanager;

        public MarkdownFieldDisplayDriver(ILiquidTemplateManager liquidTemplatemanager)
        {
            _liquidTemplatemanager = liquidTemplatemanager;
        }

        public override IDisplayResult Display(MarkdownField field, BuildFieldDisplayContext context)
        {
            return Initialize<MarkdownFieldViewModel>("MarkdownField", async model =>
            {
                var templateContext = new TemplateContext();
                templateContext.SetValue("ContentItem", field.ContentItem);
                templateContext.MemberAccessStrategy.Register<MarkdownFieldViewModel>();

                using (var writer = new StringWriter())
                {
                    await _liquidTemplatemanager.RenderAsync(field.Markdown, writer, NullEncoder.Default, templateContext);
                    model.Markdown = writer.ToString();
                    model.Html = Markdig.Markdown.ToHtml(model.Markdown ?? "");
                }

                model.Field = field;
                model.Part = context.ContentPart;
                model.PartFieldDefinition = context.PartFieldDefinition;
            })
            .Location("Content")
            .Location("SummaryAdmin", "");
        }

        public override IDisplayResult Edit(MarkdownField field, BuildFieldEditorContext context)
        {
            return Initialize<EditMarkdownFieldViewModel>(GetEditorShapeType(context), model =>
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
