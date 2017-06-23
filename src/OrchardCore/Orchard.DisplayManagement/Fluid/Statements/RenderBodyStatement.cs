using System.IO;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Fluid;
using Fluid.Ast;

namespace Orchard.DisplayManagement.Fluid.Statements
{
    public class RenderBodyStatement : Statement
    {
        public override async Task<Completion> WriteToAsync(TextWriter writer, TextEncoder encoder, TemplateContext context)
        {
            if (context.AmbientValues.TryGetValue("FluidView", out var view) && view is FluidView)
            {
                await writer.WriteAsync((await (view as FluidView).RenderBodyAsync()).ToString());
            }
            else
            {
                throw new ParseException("FluidView missing while invoking 'renderbody'.");
            }

            return Completion.Normal;
        }
    }
}