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
        private readonly FilterArgumentsExpression _arguments;

        public ScriptStatement(FilterArgumentsExpression arguments)
        {
            _arguments = arguments;
        }

        public override async Task<Completion> WriteToAsync(TextWriter writer, TextEncoder encoder, TemplateContext context)
        {
            if (context.AmbientValues.TryGetValue("FluidView", out var view) && view is FluidView)
            {
                var arguments = (await _arguments.EvaluateAsync(context)).ToObjectValue() as FilterArguments;

                var aspName = arguments.HasNamed("asp_name") ? arguments["asp_name"].ToStringValue() : String.Empty;
                var useCdn = arguments.HasNamed("use_cdn") ? arguments["use_cdn"].ToStringValue() : String.Empty;
                var at = arguments.HasNamed("at") ? arguments["at"].ToStringValue() : String.Empty;

                // To be continued...
            }
            else
            {
                throw new ParseException("FluidView missing while invoking 'script'.");
            }

            return Completion.Normal;
        }
    }
}