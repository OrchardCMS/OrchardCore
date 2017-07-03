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
using Orchard.ResourceManagement;
using Orchard.ResourceManagement.TagHelpers;

namespace Orchard.DisplayManagement.Fluid.Statements
{
    public class ScriptStatement : TagStatement
    {
        private readonly ArgumentsExpression _arguments;

        public ScriptStatement(ArgumentsExpression arguments, IList<Statement> statements) : base(statements)
        {
            _arguments = arguments;
        }

        public override async Task<Completion> WriteToAsync(TextWriter writer, TextEncoder encoder, TemplateContext context)
        {
            var page = FluidViewTemplate.EnsureFluidPage(context, "script");
            var arguments = (FilterArguments)(await _arguments.EvaluateAsync(context)).ToObjectValue();

            var attributes = new TagHelperAttributeList();
            foreach (var name in arguments.Names)
            {
                var attributeName = name.Replace("_", "-");
                attributes.Add(new TagHelperAttribute(attributeName, arguments[name].ToObjectValue()));
            }

            var scriptTagHelper = new ScriptTagHelper(page.GetService<IResourceManager>());

            TagHelperAttribute attribute;
            if (attributes.TryGetAttribute("asp-name", out attribute))
            {
                scriptTagHelper.Name = attribute.Value.ToString();
                attributes.Remove(attribute);
            }

            if (attributes.TryGetAttribute("asp-src", out attribute))
            {
                scriptTagHelper.Src = attribute.Value.ToString();
                attributes.Remove(attribute);
            }

            if (attributes.TryGetAttribute("cdn-src", out attribute))
            {
                scriptTagHelper.CdnSrc = attribute.Value.ToString();
                attributes.Remove(attribute);
            }

            if (attributes.TryGetAttribute("debug-src", out attribute))
            {
                scriptTagHelper.DebugSrc = attribute.Value.ToString();
                attributes.Remove(attribute);
            }

            if (attributes.TryGetAttribute("debug-cdn-src", out attribute))
            {
                scriptTagHelper.DebugCdnSrc = attribute.Value.ToString();
                attributes.Remove(attribute);
            }

            if (attributes.TryGetAttribute("use-cdn", out attribute))
            {
                scriptTagHelper.UseCdn = Convert.ToBoolean(attribute.Value);
                attributes.Remove(attribute);
            }

            if (attributes.TryGetAttribute("condition", out attribute))
            {
                scriptTagHelper.Condition = attribute.Value.ToString();
                attributes.Remove(attribute);
            }

            if (attributes.TryGetAttribute("culture", out attribute))
            {
                scriptTagHelper.Culture = attribute.Value.ToString();
                attributes.Remove(attribute);
            }

            if (attributes.TryGetAttribute("debug", out attribute))
            {
                scriptTagHelper.Debug = Convert.ToBoolean(attribute.Value);
                attributes.Remove(attribute);
            }

            if (attributes.TryGetAttribute("depend-on", out attribute))
            {
                scriptTagHelper.DependsOn = attribute.Value.ToString();
                attributes.Remove(attribute);
            }

            if (attributes.TryGetAttribute("version", out attribute))
            {
                scriptTagHelper.Version = attribute.Value.ToString();
                attributes.Remove(attribute);
            }

            if (attributes.TryGetAttribute("at", out attribute))
            {
                scriptTagHelper.At = (ResourceLocation)Enum.Parse(typeof(ResourceLocation), attribute.Value.ToString());
                attributes.Remove(attribute);
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

            var tagHelperOutput = new TagHelperOutput("script", attributes,
                getChildContentAsync: (useCachedResult, htmlEncoder) =>
                    Task.FromResult(new DefaultTagHelperContent().AppendHtml(content.ToString())));

            await scriptTagHelper.ProcessAsync(tagHelperContext, tagHelperOutput);

            return Completion.Normal;
        }
    }
}