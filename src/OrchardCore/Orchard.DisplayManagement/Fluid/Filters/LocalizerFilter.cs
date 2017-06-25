using System.IO;
using System.Text.Encodings.Web;
using Fluid;
using Fluid.Values;
using Orchard.Liquid;

namespace Orchard.DisplayManagement.Fluid.Filters
{
    public class LocalizerFilter : ITemplateContextHandler
    {
        public void OnTemplateProcessing(TemplateContext context)
        {   
            context.Filters.AddFilter("t", (input, arguments, ctx) =>
            {
                if (!ctx.AmbientValues.TryGetValue("FluidView", out var view) && view is FluidView)
                {
                    throw new ParseException("FluidView missing while invoking 'T'.");
                }

                var test = new StringValue((view as FluidView).T[input.ToStringValue()].Value);
                return new StringValue((view as FluidView).T[input.ToStringValue()].Value);
            });
        }
    }
}
