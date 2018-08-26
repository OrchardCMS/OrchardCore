using System.IO;
using System.Threading.Tasks;
using Fluid;
using OrchardCore.ContentFields.ViewModels;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Liquid;

namespace OrchardCore.ContentFields.Fields
{
    public class HtmlFieldDisplayDriver : ContentFieldDisplayDriver<HtmlField>
    {
        private readonly ILiquidTemplateManager _liquidTemplatemanager;

        public HtmlFieldDisplayDriver(ILiquidTemplateManager liquidTemplatemanager)
        {
            _liquidTemplatemanager = liquidTemplatemanager;
        }

        public override IDisplayResult Display(HtmlField field, BuildFieldDisplayContext context)
        {
            return Initialize<DisplayHtmlFieldViewModel>("HtmlField", async model =>
            {
                var templateContext = new TemplateContext();
                templateContext.SetValue("ContentItem", field.ContentItem);
                templateContext.MemberAccessStrategy.Register<DisplayHtmlFieldViewModel>();

                using (var writer = new StringWriter())
                {
                    await _liquidTemplatemanager.RenderAsync(field.Html, writer, NullEncoder.Default, templateContext);
                    model.Html = writer.ToString();
                }

                model.Field = field;
                model.Part = context.ContentPart;
                model.PartFieldDefinition = context.PartFieldDefinition;
            })
            .Location("Content")
            .Location("SummaryAdmin", "");
        }

        public override IDisplayResult Edit(HtmlField field, BuildFieldEditorContext context)
        {
            return Initialize<EditHtmlFieldViewModel>(GetEditorShapeType(context), model =>
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
