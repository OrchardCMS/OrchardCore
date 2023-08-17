using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using Fluid;
using Fluid.Ast;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.DisplayManagement.Liquid.TagHelpers;
using OrchardCore.Liquid;

namespace OrchardCore.DisplayManagement.Liquid.Tags
{
    public class FluidTagHelper
    {
        public static readonly Dictionary<string, string> DefaultArgumentsMapping = new();
        private static long _uniqueId;

        public static ValueTask<Completion> WriteArgumentsTagHelperAsync(List<FilterArgument> arguments, TextWriter writer, TextEncoder encoder, TemplateContext context)
        {
            return WriteToAsync(null, arguments, Array.Empty<Statement>(), writer, encoder, context);
        }

        public static ValueTask<Completion> WriteArgumentsBlockHelperAsync(List<FilterArgument> arguments, IReadOnlyList<Statement> statements, TextWriter writer, TextEncoder encoder, TemplateContext context)
        {
            return WriteToAsync(null, arguments, statements, writer, encoder, context);
        }

        public static async ValueTask<Completion> WriteToAsync(string identifier, List<FilterArgument> arguments, IReadOnlyList<Statement> statements, TextWriter writer, TextEncoder encoder, TemplateContext context)
        {
            var services = ((LiquidTemplateContext)context).Services;

            var viewContextAccessor = services.GetRequiredService<ViewContextAccessor>();
            var viewContext = viewContextAccessor.ViewContext;

            // If no identifier is set, use the first argument as the name of the tag helper
            // e.g., {% helper "input", for: "Text", class: "form-control" %}

            identifier ??= (await arguments[0].Expression.EvaluateAsync(context)).ToStringValue();

            // These mapping will assign an argument name to the first element in the filter arguments,
            // such that the tag helper can be matched based on the expected attribute names.
            if (DefaultArgumentsMapping.TryGetValue(identifier, out var mapping))
            {
                arguments = new List<FilterArgument>(arguments);
                arguments[0] = new FilterArgument(mapping, arguments[0].Expression);
            }

            var filterArguments = new FilterArguments();
            foreach (var argument in arguments)
            {
                filterArguments.Add(argument.Name, await argument.Expression.EvaluateAsync(context));
            }

            var factory = services.GetRequiredService<LiquidTagHelperFactory>();
            var activator = factory.GetActivator(identifier, filterArguments.Names);

            if (activator == LiquidTagHelperActivator.None)
            {
                return Completion.Normal;
            }

            var tagHelper = factory.CreateTagHelper(activator, viewContext,
                filterArguments, out var contextAttributes, out var outputAttributes);

            ViewBufferTextWriterContent content = null;

            if (statements != null && statements.Count > 0)
            {
                content = new ViewBufferTextWriterContent();

                var completion = await statements.RenderStatementsAsync(content, encoder, context);

                if (completion != Completion.Normal)
                {
                    return completion;
                }
            }

            Interlocked.CompareExchange(ref _uniqueId, Int64.MaxValue, 0);
            var id = Interlocked.Increment(ref _uniqueId);

            var tagHelperContext = new TagHelperContext(contextAttributes, new Dictionary<object, object>(), id.ToString());

            TagHelperOutput tagHelperOutput = null;

            if (content != null)
            {
                tagHelperOutput = new TagHelperOutput(
                    identifier,
                    outputAttributes, (_, e) => Task.FromResult(new DefaultTagHelperContent().AppendHtml(content))
                );
                tagHelperOutput.Content.AppendHtml(content);
            }
            else
            {
                tagHelperOutput = new TagHelperOutput(
                    identifier,
                    outputAttributes, (_, e) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent())
                );
            }

            await tagHelper.ProcessAsync(tagHelperContext, tagHelperOutput);
            tagHelperOutput.WriteTo(writer, (HtmlEncoder)encoder);

            return Completion.Normal;
        }
    }
}
