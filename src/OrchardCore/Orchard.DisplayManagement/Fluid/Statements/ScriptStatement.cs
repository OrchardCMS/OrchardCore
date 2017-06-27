using System;
using System.IO;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Fluid;
using Fluid.Ast;
using Orchard.DisplayManagement.Fluid.Ast;

namespace Orchard.DisplayManagement.Fluid.Statements
{
    public class ScriptStatement : Statement
    {
        private readonly ArgumentsExpression _arguments;

        public ScriptStatement(ArgumentsExpression arguments)
        {
            _arguments = arguments;
        }

        public override async Task<Completion> WriteToAsync(TextWriter writer, TextEncoder encoder, TemplateContext context)
        {
            if (context.AmbientValues.TryGetValue("FluidPage", out var view) && view is FluidPage)
            {
                var arguments = (await _arguments.EvaluateAsync(context)).ToObjectValue() as FilterArguments;

                var aspName = arguments.At(0).ToStringValue();

                var useCdn = arguments.HasNamed("use_cdn") ?
                    arguments["use_cdn"].ToStringValue() : String.Empty;

                var at = arguments.HasNamed("at") ?
                    arguments["at"].ToStringValue() : String.Empty;

                // To be continued...
            }
            else
            {
                throw new ParseException("FluidPage missing while invoking 'script'.");
            }

            return Completion.Normal;
        }
    }
}