using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Fluid;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentFields.ViewModels;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Liquid;

namespace OrchardCore.ContentFields.Fields
{
    public class HtmlFieldDisplayDriver : ContentFieldDisplayDriver<HtmlField>
    {
        private readonly ILiquidTemplateManager _liquidTemplatemanager;
        private readonly HtmlEncoder _htmlEncoder;

        public HtmlFieldDisplayDriver(ILiquidTemplateManager liquidTemplatemanager, IStringLocalizer<HtmlFieldDisplayDriver> localizer, HtmlEncoder htmlEncoder)
        {
            _liquidTemplatemanager = liquidTemplatemanager;
            T = localizer;
            _htmlEncoder = htmlEncoder;
        }

        public IStringLocalizer T { get; }

        public override IDisplayResult Display(HtmlField field, BuildFieldDisplayContext context)
        {
            return Initialize<DisplayHtmlFieldViewModel>(GetDisplayShapeType(context), async model =>
            {
                var templateContext = new TemplateContext();
                templateContext.SetValue("ContentItem", field.ContentItem);
                templateContext.MemberAccessStrategy.Register<DisplayHtmlFieldViewModel>();

                model.Html = field.Html;
                model.Field = field;
                model.Part = context.ContentPart;
                model.PartFieldDefinition = context.PartFieldDefinition;
                templateContext.SetValue("Model", model);

                model.Html = await _liquidTemplatemanager.RenderAsync(field.Html, _htmlEncoder, templateContext);
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
            var viewModel = new EditHtmlFieldViewModel();

            if (await updater.TryUpdateModelAsync(viewModel, Prefix, f => f.Html))
            {
                if (!string.IsNullOrEmpty(viewModel.Html) && !_liquidTemplatemanager.Validate(viewModel.Html, out var errors))
                {
                    var fieldName = context.PartFieldDefinition.DisplayName();
                    context.Updater.ModelState.AddModelError(nameof(field.Html), T["{0} doesn't contain a valid Liquid expression. Details: {1}", fieldName, string.Join(" ", errors)]);
                }
                else
                {
                    field.Html = viewModel.Html;
                }
            }

            return Edit(field, context);
        }
    }
}
