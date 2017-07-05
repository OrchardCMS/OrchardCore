using System.IO;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Fluid;
using Fluid.Ast;
using Orchard.DisplayManagement.Fluid.Ast;

namespace Orchard.DisplayManagement.Fluid.Statements
{
    public class SetMetadataStatement : Statement
    {
        private readonly ArgumentsExpression _arguments;

        public SetMetadataStatement(Expression shape, ArgumentsExpression arguments)
        {
            Shape = shape;
            _arguments = arguments;
        }

        public Expression Shape { get; }

        public override async Task<Completion> WriteToAsync(TextWriter writer, TextEncoder encoder, TemplateContext context)
        {
            var shape = (await Shape.EvaluateAsync(context)).ToObjectValue() as IShape;

            if (shape?.Metadata != null)
            {
                var arguments = (FilterArguments)(await _arguments.EvaluateAsync(context)).ToObjectValue();

                if (arguments.HasNamed("type"))
                {
                    shape.Metadata.Type = arguments["type"].ToStringValue();
                }

                if (arguments.HasNamed("display_type"))
                {
                    shape.Metadata.DisplayType = arguments["display_type"].ToStringValue();
                }

                if (arguments.HasNamed("position"))
                {
                    shape.Metadata.Position = arguments["position"].ToStringValue();
                }

                if (arguments.HasNamed("tab"))
                {
                    shape.Metadata.Tab = arguments["tab"].ToStringValue();
                }
            }

            return Completion.Normal;
        }
    }
}