using System.IO;
using System.Threading.Tasks;
using Cysharp.Text;
using Fluid;
using Fluid.Values;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Razor.TagHelpers;
using OrchardCore.Liquid;

namespace OrchardCore.DisplayManagement.Liquid.Filters
{
    public class ShapeStringifyFilter : ILiquidFilter
    {
        private readonly IDisplayHelper _displayHelper;

        public ShapeStringifyFilter(IDisplayHelper displayHelper)
        {
            _displayHelper = displayHelper;
        }

        public ValueTask<FluidValue> ProcessAsync(FluidValue input, FilterArguments arguments, LiquidTemplateContext ctx)
        {
            static async ValueTask<FluidValue> Awaited(Task<IHtmlContent> task)
            {
                using var writer = new ZStringWriter();
                (await task).WriteTo(writer, NullHtmlEncoder.Default);
                return new StringValue(writer.ToString(), false);
            }

            if (input.ToObjectValue() is IShape shape)
            {
                var task = _displayHelper.ShapeExecuteAsync(shape);
                if (!task.IsCompletedSuccessfully)
                {
                    return Awaited(task);
                }

                using var writer = new ZStringWriter();
                task.Result.WriteTo(writer, NullHtmlEncoder.Default);
                return new ValueTask<FluidValue>(new StringValue(writer.ToString(), false));
            }

            return new ValueTask<FluidValue>(StringValue.Empty);
        }
    }
}
