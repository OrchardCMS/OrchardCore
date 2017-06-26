using System;
using System.IO;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Fluid;
using Fluid.Ast;
using Orchard.DisplayManagement.Fluid.Ast;

namespace Orchard.DisplayManagement.Fluid.Statements
{
    public class RenderSectionStatement : Statement
    {
        private readonly ArgumentsExpression _arguments;

        public RenderSectionStatement(ArgumentsExpression arguments)
        {
            _arguments = arguments;
        }

        public override async Task<Completion> WriteToAsync(TextWriter writer, TextEncoder encoder, TemplateContext context)
        {
            if (context.AmbientValues.TryGetValue("FluidView", out var view) && view is FluidView)
            {
                var arguments = (await _arguments.EvaluateAsync(context)).ToObjectValue() as FilterArguments;

                var name = arguments.At(0).ToStringValue();

                var required = arguments.HasNamed("required") ?
                    Convert.ToBoolean(arguments["required"].ToStringValue()) : false;

                await writer.WriteAsync((await (view as FluidView).RenderSectionAsync(name, required)).ToString());
            }
            else
            {
                throw new ParseException("FluidView missing while invoking 'RenderSection'.");
            }

            return Completion.Normal;
        }
    }
}