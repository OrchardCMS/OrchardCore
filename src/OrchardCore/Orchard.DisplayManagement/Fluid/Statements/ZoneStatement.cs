using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Fluid;
using Fluid.Ast;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Orchard.DisplayManagement.Fluid.Ast;
using Orchard.DisplayManagement.Layout;
using Orchard.DisplayManagement.TagHelpers;

namespace Orchard.DisplayManagement.Fluid.Statements
{
    public class ZoneStatement : TagStatement
    {
        private readonly ArgumentsExpression _arguments;

        public ZoneStatement(ArgumentsExpression arguments, IList<Statement> statements) : base(statements)
        {
            _arguments = arguments;
        }

        public override async Task<Completion> WriteToAsync(TextWriter writer, TextEncoder encoder, TemplateContext context)
        {
            if (!context.AmbientValues.TryGetValue("LayoutAccessor", out var layoutAccessor))
            {
                throw new ArgumentException("LayoutAccessor missing while invoking 'zone'");
            }

            var arguments = (FilterArguments)(await _arguments.EvaluateAsync(context)).ToObjectValue();

            var attributes = new TagHelperAttributeList();

            var zoneTagHelper = new ZoneTagHelper((ILayoutAccessor)layoutAccessor);

            if (arguments.HasNamed("position"))
            {
                zoneTagHelper.Position = arguments["position"].ToStringValue();
            }

            if (arguments.HasNamed("name"))
            {
                zoneTagHelper.Name = arguments["name"].ToStringValue();
            }

            var content = new StringWriter();
            if (Statements?.Any() ?? false)
            {
                Completion completion = Completion.Break;
                for (var index = 0; index < Statements.Count; index++)
                {
                    completion = await Statements[index].WriteToAsync(
                        content, encoder, context);

                    if (completion != Completion.Normal)
                    {
                        return completion;
                    }
                }
            }

            var tagHelperContext = new TagHelperContext(attributes,
                new Dictionary<object, object>(), Guid.NewGuid().ToString("N"));

            var tagHelperOutput = new TagHelperOutput("zone", attributes,
                getChildContentAsync: (useCachedResult, htmlEncoder) =>
                    Task.FromResult(new DefaultTagHelperContent().AppendHtml(content.ToString())));

            await zoneTagHelper.ProcessAsync(tagHelperContext, tagHelperOutput);

            return Completion.Normal;
        }
    }
}