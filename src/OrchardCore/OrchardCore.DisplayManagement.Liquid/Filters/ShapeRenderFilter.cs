using System;
using System.Threading.Tasks;
using Fluid;
using Fluid.Values;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using OrchardCore.Liquid;

namespace OrchardCore.DisplayManagement.Liquid.Filters
{
    public class ShapeRenderFilter : ILiquidFilter
    {
        private readonly IDisplayHelper _displayHelper;

        public ShapeRenderFilter(IDisplayHelper displayHelper)
        {
            _displayHelper = displayHelper;
        }

        public ValueTask<FluidValue> ProcessAsync(FluidValue input, FilterArguments arguments, TemplateContext ctx)
        {
            static async ValueTask<FluidValue> Awaited(Task<IHtmlContent> task)
            {
                return new HtmlContentValue(await task);
            }

            var inputObject = input.ToObjectValue();
            if (inputObject is IShape shape)
            {
                var task = _displayHelper.ShapeExecuteAsync(shape);
                if (!task.IsCompletedSuccessfully)
                {
                    return Awaited(task);
                }

                return new ValueTask<FluidValue>(new HtmlContentValue(task.Result));
            }
            else if (inputObject is IHtmlContent htmlContent)
            {
                return new ValueTask<FluidValue>(new HtmlContentValue(htmlContent));
            }
            else if (inputObject is string stringHtml)
            {
                return new ValueTask<FluidValue>(new HtmlContentValue(new StringHtmlContent(stringHtml)));
            }

            return new ValueTask<FluidValue>(NilValue.Instance);
        }
    }
}
