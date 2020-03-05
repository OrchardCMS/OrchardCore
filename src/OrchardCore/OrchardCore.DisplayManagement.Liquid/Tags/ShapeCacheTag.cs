using System;
using System.IO;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Fluid;
using Fluid.Ast;
using OrchardCore.Liquid.Ast;

namespace OrchardCore.DisplayManagement.Liquid.Tags
{
    public class ShapeCacheTag : ExpressionArgumentsTag
    {
        private static readonly char[] Separators = { ',', ' ' };

        public override async ValueTask<Completion> WriteToAsync(TextWriter writer, TextEncoder encoder, TemplateContext context, Expression expression, FilterArgument[] args)
        {
            var objectValue = (await expression.EvaluateAsync(context)).ToObjectValue();

            if (objectValue is IShape shape)
            {
                var arguments = (FilterArguments)(await new ArgumentsExpression(args).EvaluateAsync(context)).ToObjectValue();

                var metadata = shape.Metadata;

                if (arguments.HasNamed("cache_id"))
                {
                    metadata.Cache(arguments["cache_id"].ToStringValue());
                }

                if (arguments.HasNamed("cache_context"))
                {
                    var contexts = arguments["cache_context"].ToStringValue().Split(Separators, StringSplitOptions.RemoveEmptyEntries);
                    metadata.Cache().AddContext(contexts);
                }

                if (arguments.HasNamed("cache_tag"))
                {
                    var tags = arguments["cache_tag"].ToStringValue().Split(Separators, StringSplitOptions.RemoveEmptyEntries);
                    metadata.Cache().AddTag(tags);
                }

                if (arguments.HasNamed("cache_fixed_duration"))
                {
                    if (TimeSpan.TryParse(arguments["cache_fixed_duration"].ToStringValue(), out var timespan))
                    {
                        metadata.Cache().WithExpiryAfter(timespan);
                    }
                }

                if (arguments.HasNamed("cache_sliding_duration"))
                {
                    if (TimeSpan.TryParse(arguments["cache_sliding_duration"].ToStringValue(), out var timespan))
                    {
                        metadata.Cache().WithExpirySliding(timespan);
                    }
                }
            }

            return Completion.Normal;
        }
    }
}
