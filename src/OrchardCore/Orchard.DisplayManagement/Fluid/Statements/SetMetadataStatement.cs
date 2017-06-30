using System.IO;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Fluid;
using Fluid.Ast;
using Orchard.DisplayManagement.Fluid.Ast;
using Orchard.DisplayManagement.Shapes;

namespace Orchard.DisplayManagement.Fluid.Statements
{
    public class SetMetadataStatement : Statement
    {
        private readonly ArgumentsExpression _arguments;

        public SetMetadataStatement(ArgumentsExpression arguments)
        {
            _arguments = arguments;
        }

        public override async Task<Completion> WriteToAsync(TextWriter writer, TextEncoder encoder, TemplateContext context)
        {
            var model = (dynamic)context.LocalScope.GetValue("Model").ToObjectValue();

            if (model is IShape && model.Metadata != null)
            {
                var arguments = (FilterArguments)(await _arguments.EvaluateAsync(context)).ToObjectValue();

                if (arguments.HasNamed("type"))
                {
                    ((ShapeMetadata)model.Metadata).Type = arguments["type"].ToStringValue();
                }

                if (arguments.HasNamed("display_type"))
                {
                    ((ShapeMetadata)model.Metadata).DisplayType = arguments["display_type"].ToStringValue();
                }

                if (arguments.HasNamed("position"))
                {
                    ((ShapeMetadata)model.Metadata).Position = arguments["position"].ToStringValue();
                }

                if (arguments.HasNamed("tab"))
                {
                    ((ShapeMetadata)model.Metadata).Tab = arguments["tab"].ToStringValue();
                }
            }

            return Completion.Normal;
        }
    }
}