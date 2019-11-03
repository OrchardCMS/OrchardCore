using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Fluid;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Html.Models;
using OrchardCore.Html.ViewModels;
using OrchardCore.Liquid;

namespace OrchardCore.Html.Drivers
{
    public class HtmlBodyPartDisplay : ContentPartDisplayDriver<HtmlBodyPart>
    {
        private readonly ILiquidTemplateManager _liquidTemplatemanager;

        public HtmlBodyPartDisplay(ILiquidTemplateManager liquidTemplatemanager, IStringLocalizer<HtmlBodyPartDisplay> localizer)
        {
            _liquidTemplatemanager = liquidTemplatemanager;
            T = localizer;
        }

        public IStringLocalizer T { get; private set; }

        public override IDisplayResult Display(HtmlBodyPart HtmlBodyPart, BuildPartDisplayContext context)
        {
            return Initialize<HtmlBodyPartViewModel>("HtmlBodyPart", m => BuildViewModelAsync(m, HtmlBodyPart))
                .Location("Detail", "Content:5")
                .Location("Summary", "Content:10");
        }

        public override IDisplayResult Edit(HtmlBodyPart HtmlBodyPart, BuildPartEditorContext context)
        {
            return Initialize<HtmlBodyPartViewModel>(GetEditorShapeType(context), model =>
            {
                model.Html = HtmlBodyPart.Html;
                model.ContentItem = HtmlBodyPart.ContentItem;
                model.HtmlBodyPart = HtmlBodyPart;
                model.TypePartDefinition = context.TypePartDefinition;
            });
        }

        public override async Task<IDisplayResult> UpdateAsync(HtmlBodyPart model, IUpdateModel updater, UpdatePartEditorContext context)
        {
            var viewModel = new HtmlBodyPartViewModel();

            if (await updater.TryUpdateModelAsync(viewModel, Prefix, t => t.Html))
            {
                if (!string.IsNullOrEmpty(viewModel.Html) && !_liquidTemplatemanager.Validate(viewModel.Html, out var errors))
                {
                    context.Updater.ModelState.AddModelError(nameof(model.Html), T["The 'HtmlBody' contains an invalid Liquid expression. Details: {0}", string.Join(" ", errors)]);
                }
                else
                {
                    model.Html = viewModel.Html;
                }
            }

            return Edit(model, context);
        }

        private async ValueTask BuildViewModelAsync(HtmlBodyPartViewModel model, HtmlBodyPart HtmlBodyPart)
        {
            var templateContext = new TemplateContext();
            templateContext.SetValue("ContentItem", HtmlBodyPart.ContentItem);
            templateContext.MemberAccessStrategy.Register<HtmlBodyPartViewModel>();

            model.HtmlBodyPart = HtmlBodyPart;
            model.ContentItem = HtmlBodyPart.ContentItem;
            templateContext.LocalScope.SetValue("Model", model);

            model.Html = await _liquidTemplatemanager.RenderAsync(HtmlBodyPart.Html, HtmlEncoder.Default, templateContext);

            model.Source = HtmlBodyPart.Html;
        }
    }
}
