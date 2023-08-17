using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Fluid;
using Fluid.Ast;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Liquid;
using OrchardCore.Mvc.Utilities;

namespace OrchardCore.DisplayManagement.Liquid.Tags
{
    public class ShapeTag
    {
        private static readonly char[] _separators = { ',', ' ' };

        public static async ValueTask<Completion> WriteToAsync(List<FilterArgument> argumentsList, TextWriter writer, TextEncoder encoder, TemplateContext context)
        {
            var services = ((LiquidTemplateContext)context).Services;
            var shapeFactory = services.GetRequiredService<IShapeFactory>();
            var displayHelper = services.GetRequiredService<IDisplayHelper>();

            string cacheId = null;
            TimeSpan? cacheFixedDuration = null;
            TimeSpan? cacheSlidingDuration = null;
            // string cacheDependency = null;
            string cacheContext = null;
            string cacheTag = null;

            string id = null;
            string alternate = null;
            string wrapper = null;
            string type = null;

            Dictionary<string, object> customAttributes = null;

            foreach (var argument in argumentsList)
            {
                switch (argument.Name)
                {
                    case "cache_id": cacheId = (await argument.Expression.EvaluateAsync(context)).ToStringValue(); break;

#pragma warning disable CA1806 // Do not ignore method results
                    case "cache_fixed_duration": TimeSpan.TryParse((await argument.Expression.EvaluateAsync(context)).ToStringValue(), out var fd); cacheFixedDuration = fd; break;
                    case "cache_sliding_duration": TimeSpan.TryParse((await argument.Expression.EvaluateAsync(context)).ToStringValue(), out var sd); cacheSlidingDuration = sd; break;
#pragma warning restore CA1806 // Do not ignore method results

                    // case "cache_dependency": cacheDependency = (await argument.Expression.EvaluateAsync(context)).ToStringValue(); break;
                    case "cache_context": cacheContext = (await argument.Expression.EvaluateAsync(context)).ToStringValue(); break;
                    case "cache_tag": cacheTag = (await argument.Expression.EvaluateAsync(context)).ToStringValue(); break;

                    case "id": id = (await argument.Expression.EvaluateAsync(context)).ToStringValue(); break;
                    case "alternate": alternate = (await argument.Expression.EvaluateAsync(context)).ToStringValue(); break;
                    case "wrapper": wrapper = (await argument.Expression.EvaluateAsync(context)).ToStringValue(); break;

                    case null:
                    case "type":
                    case "": type = (await argument.Expression.EvaluateAsync(context)).ToStringValue(); break;

                    default: (customAttributes ??= new Dictionary<string, object>())[argument.Name.ToPascalCaseUnderscore()] = (await argument.Expression.EvaluateAsync(context)).ToObjectValue(); break;
                }
            }

            var shape = await shapeFactory.CreateAsync<object>(type, customAttributes == null ? Arguments.Empty : Arguments.From(customAttributes));

            if (!String.IsNullOrEmpty(id))
            {
                shape.Id = id;
            }

            if (!String.IsNullOrEmpty(alternate))
            {
                shape.Metadata.Alternates.Add(alternate);
            }

            if (!String.IsNullOrEmpty(wrapper))
            {
                shape.Metadata.Wrappers.Add(wrapper);
            }

            if (!String.IsNullOrWhiteSpace(cacheId))
            {
                var metadata = shape.Metadata;

                metadata.Cache(cacheId);

                if (cacheFixedDuration.HasValue)
                {
                    metadata.Cache().WithExpiryAfter(cacheFixedDuration.Value);
                }

                if (cacheSlidingDuration.HasValue)
                {
                    metadata.Cache().WithExpirySliding(cacheSlidingDuration.Value);
                }

                if (!String.IsNullOrWhiteSpace(cacheContext))
                {
                    var contexts = cacheContext.Split(_separators, StringSplitOptions.RemoveEmptyEntries);
                    metadata.Cache().AddContext(contexts);
                }

                if (!String.IsNullOrWhiteSpace(cacheTag))
                {
                    var tags = cacheTag.Split(_separators, StringSplitOptions.RemoveEmptyEntries);
                    metadata.Cache().AddTag(tags);
                }
            }

            var shapeContent = await displayHelper.ShapeExecuteAsync(shape);

            shapeContent.WriteTo(writer, (HtmlEncoder)encoder);

            return Completion.Normal;
        }
    }
}
