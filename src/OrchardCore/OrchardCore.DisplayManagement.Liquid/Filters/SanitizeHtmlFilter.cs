using System.Threading.Tasks;
using Fluid;
using Fluid.Values;
using OrchardCore.Infrastructure.Html;
using OrchardCore.Liquid;

namespace OrchardCore.DisplayManagement.Liquid.Filters
{
    public class SanitizeHtmlFilter : ILiquidFilter
    {
        private readonly IHtmlSanitizerService _htmlSanitizerService;

        public SanitizeHtmlFilter(IHtmlSanitizerService htmlSanitizerService)
        {
            _htmlSanitizerService = htmlSanitizerService;
        }
        public ValueTask<FluidValue> ProcessAsync(FluidValue input, FilterArguments arguments, LiquidTemplateContext ctx)
        {
            var html = input.ToStringValue();

            html = _htmlSanitizerService.Sanitize(html);

            return new ValueTask<FluidValue>(new StringValue(html));
        }
    }
}
