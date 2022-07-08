using System.Threading.Tasks;
using Fluid;
using Fluid.Values;
using Microsoft.AspNetCore.Html;
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

        public ValueTask<FluidValue> ProcessAsync(FluidValue input, FilterArguments arguments, LiquidTemplateContext ctx)
        {
            static async ValueTask<FluidValue> Awaited(Task<IHtmlContent> task)
            {
                return new HtmlContentValue(await task);
            }

            if (input.ToObjectValue() is IShape shape)
            {
                var task = _displayHelper.ShapeExecuteAsync(shape);
                if (!task.IsCompletedSuccessfully)
                {
                    return Awaited(task);
                }

                return new ValueTask<FluidValue>(new HtmlContentValue(task.Result));
            }

            return new ValueTask<FluidValue>(new HtmlContentValue(HtmlString.Empty));
        }
    }
}
