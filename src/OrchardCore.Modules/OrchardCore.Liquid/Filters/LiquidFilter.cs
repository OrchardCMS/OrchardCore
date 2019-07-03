using System.Threading.Tasks;
using Fluid;
using Fluid.Values;

namespace OrchardCore.Liquid.Filters
{
    public class LiquidFilter : ILiquidFilter
    {
        private readonly ILiquidTemplateManager _liquidTemplateManager;

        public LiquidFilter(ILiquidTemplateManager liquidTemplateManager)
        {
            _liquidTemplateManager = liquidTemplateManager;
        }

        public async ValueTask<FluidValue> ProcessAsync(FluidValue input, FilterArguments arguments, TemplateContext ctx)
        {
            var context = new TemplateContext();

            var model = arguments.At(0);

            if (!model.IsNil())
            {
                context.MemberAccessStrategy.Register(model.GetType());
                context.LocalScope.SetValue("Model", model);
            }

            var content = await _liquidTemplateManager.RenderAsync(input.ToStringValue(), context);

            return new StringValue(content, false);
        }
    }
}
