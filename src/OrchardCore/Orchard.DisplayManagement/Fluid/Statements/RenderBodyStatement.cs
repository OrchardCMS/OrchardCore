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
            if (context.AmbientValues.TryGetValue("FluidPage", out var view) && view is FluidPage)
            {
                await writer.WriteAsync((await (view as FluidPage).RenderBodyAsync()).ToString());
            }
            else
            {
                throw new ParseException("FluidPage missing while invoking 'RenderBody'.");
            }

            return Completion.Normal;
        }
    }
}