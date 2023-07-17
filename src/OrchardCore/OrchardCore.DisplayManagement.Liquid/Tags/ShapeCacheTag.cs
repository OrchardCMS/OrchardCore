using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Fluid;
using Fluid.Ast;

namespace OrchardCore.DisplayManagement.Liquid.Tags
{
    public class ShapeCacheTag
    {
        private static readonly char[] _separators = { ',', ' ' };

        public static async ValueTask<Completion> WriteToAsync(ValueTuple<Expression, List<FilterArgument>> arguments, TextWriter _1, TextEncoder _2, TemplateContext context)
        {
            var objectValue = (await arguments.Item1.EvaluateAsync(context)).ToObjectValue();

            if (objectValue is IShape shape)
            {
                var expressions = new NamedExpressionList(arguments.Item2);

                var metadata = shape.Metadata;

                if (expressions.HasNamed("cache_id"))
                {
                    metadata.Cache((await expressions["cache_id"].EvaluateAsync(context)).ToStringValue());
                }

                if (expressions.HasNamed("cache_context"))
                {
                    var contexts = (await expressions["cache_context"].EvaluateAsync(context)).ToStringValue().Split(_separators, StringSplitOptions.RemoveEmptyEntries);
                    metadata.Cache().AddContext(contexts);
                }

                if (expressions.HasNamed("cache_tag"))
                {
                    var tags = (await expressions["cache_tag"].EvaluateAsync(context)).ToStringValue().Split(_separators, StringSplitOptions.RemoveEmptyEntries);
                    metadata.Cache().AddTag(tags);
                }

                if (expressions.HasNamed("cache_fixed_duration"))
                {
                    if (TimeSpan.TryParse((await expressions["cache_fixed_duration"].EvaluateAsync(context)).ToStringValue(), out var timespan))
                    {
                        metadata.Cache().WithExpiryAfter(timespan);
                    }
                }

                if (expressions.HasNamed("cache_sliding_duration"))
                {
                    if (TimeSpan.TryParse((await expressions["cache_sliding_duration"].EvaluateAsync(context)).ToStringValue(), out var timespan))
                    {
                        metadata.Cache().WithExpirySliding(timespan);
                    }
                }
            }

            return Completion.Normal;
        }
    }
}
