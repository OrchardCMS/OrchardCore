using System;
using System.Collections.Generic;
using System.IO;
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
    public class StyleStatement : Statement
    {
        private readonly ArgumentsExpression _arguments;

        public StyleStatement(ArgumentsExpression arguments)
        {
            _arguments = arguments;
        }

        public override async Task<Completion> WriteToAsync(TextWriter writer, TextEncoder encoder, TemplateContext context)
        {
            var page = FluidViewTemplate.EnsureFluidPage(context, "style");
            var arguments = (FilterArguments)(await _arguments.EvaluateAsync(context)).ToObjectValue();

            if (arguments.Count == 0)
            {
                return Completion.Continue;
            }

            var attributes = new TagHelperAttributeList();
            foreach (var name in arguments.Names)
            {
                var attributeName = name.Replace("_", "-");
                attributes.Add(new TagHelperAttribute(attributeName, arguments[name].ToObjectValue()));
            }

            var styleTagHelper = new StyletTagHelper(page.GetService<IResourceManager>());

            TagHelperAttribute attribute;
            if (attributes.TryGetAttribute("asp-name", out attribute))
            {
                styleTagHelper.Name = attribute.Value.ToString();
            }

            if (attributes.TryGetAttribute("asp-src", out attribute))
            {
                styleTagHelper.Src = attribute.Value.ToString();
                attributes.Remove(attribute);
            }

            if (attributes.TryGetAttribute("cdn-src", out attribute))
            {
                styleTagHelper.CdnSrc = attribute.Value.ToString();
                attributes.Remove(attribute);
            }

            if (attributes.TryGetAttribute("debug-src", out attribute))
            {
                styleTagHelper.DebugSrc = attribute.Value.ToString();
                attributes.Remove(attribute);
            }

            if (attributes.TryGetAttribute("debug-cdn-src", out attribute))
            {
                styleTagHelper.DebugCdnSrc = attribute.Value.ToString();
                attributes.Remove(attribute);
            }

            if (attributes.TryGetAttribute("use-cdn", out attribute))
            {
                styleTagHelper.UseCdn = Convert.ToBoolean(attribute.Value);
            }

            if (attributes.TryGetAttribute("condition", out attribute))
            {
                styleTagHelper.Condition = attribute.Value.ToString();
                attributes.Remove(attribute);
            }

            if (attributes.TryGetAttribute("culture", out attribute))
            {
                styleTagHelper.Culture = attribute.Value.ToString();
                attributes.Remove(attribute);
            }

            if (attributes.TryGetAttribute("debug", out attribute))
            {
                styleTagHelper.Debug = Convert.ToBoolean(attribute.Value);
                attributes.Remove(attribute);
            }

            if (attributes.TryGetAttribute("dependencies", out attribute))
            {
                styleTagHelper.Dependencies = attribute.Value.ToString();
                attributes.Remove(attribute);
            }

            if (attributes.TryGetAttribute("version", out attribute))
            {
                styleTagHelper.Version = attribute.Value.ToString();
            }

            if (attributes.TryGetAttribute("at", out attribute))
            {
                styleTagHelper.At = (ResourceLocation)Enum.Parse(typeof(ResourceLocation), attribute.Value.ToString());
            }

            var tagHelperContext = new TagHelperContext(attributes,
                new Dictionary<object, object>(), Guid.NewGuid().ToString("N"));

            var tagHelperOutput = new TagHelperOutput("script", attributes,
                getChildContentAsync: (useCachedResult, htmlEncoder) =>
                    Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));

            await styleTagHelper.ProcessAsync(tagHelperContext, tagHelperOutput);

            return Completion.Normal;
        }
    }
}