using System.IO;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Fluid;
using Fluid.Ast;
using Orchard.DisplayManagement.Fluid.Ast;

namespace Orchard.DisplayManagement.Fluid.Statements
{
    public class DisplayStatement : Statement
    {
        private readonly ArgumentsExpression _arguments;

        public DisplayStatement(ArgumentsExpression arguments)
        {
            _arguments = arguments;
        }

        public override async Task<Completion> WriteToAsync(TextWriter writer, TextEncoder encoder, TemplateContext context)
        {
            var page = FluidViewTemplate.EnsureFluidPage(context, "Display");
            var arguments = (FilterArguments)(await _arguments.EvaluateAsync(context)).ToObjectValue();

            var shape = arguments.HasNamed("shape") ? arguments["shape"].ToObjectValue() : null;
            await writer.WriteAsync((await page.DisplayAsync(shape)).ToString());
            return Completion.Normal;
        }
    }
}