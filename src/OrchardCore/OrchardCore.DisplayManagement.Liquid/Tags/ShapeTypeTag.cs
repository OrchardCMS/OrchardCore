using System;
using System.IO;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Fluid;
using Fluid.Ast;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Liquid.Ast;

namespace OrchardCore.DisplayManagement.Liquid.Tags
{
    public class ShapeTypeTag : ExpressionArgumentsTag
    {
        public override async ValueTask<Completion> WriteToAsync(TextWriter writer, TextEncoder encoder, TemplateContext context, Expression expression, FilterArgument[] args)
        {
            var objectValue = (await expression.EvaluateAsync(context)).ToObjectValue();

            if (objectValue == null)
            {
                if (!context.AmbientValues.TryGetValue("Services", out var services))
                {
                    throw new ArgumentException("Services missing while invoking 'shape_type'");
                }

                var shapeScopeManager = ((IServiceProvider)services).GetRequiredService<IShapeScopeManager>();
                objectValue = shapeScopeManager.GetCurrentShape();
            }

            if (objectValue is IShape shape)
            {
                var arguments = (FilterArguments)(await new ArgumentsExpression(args).EvaluateAsync(context)).ToObjectValue();
                shape.Metadata.Type = arguments["type"].Or(arguments.At(0)).ToStringValue();
            }

            return Completion.Normal;
        }
    }
}
