using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Fluid;
using Fluid.Ast;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Orchard.DisplayManagement.Fluid.Ast;
using Orchard.DisplayManagement.TagHelpers;
using Orchard.ResourceManagement;
using Orchard.ResourceManagement.TagHelpers;

namespace Orchard.DisplayManagement.Fluid.Statements
{
    public class ShapeStatement : Statement
    {
        private readonly string _name;
        private readonly ArgumentsExpression _arguments;

        public ShapeStatement(string name, ArgumentsExpression arguments)
        {
            _name = name;
            _arguments = arguments;
        }

        public override async Task<Completion> WriteToAsync(TextWriter writer, TextEncoder encoder, TemplateContext context)
        {
            var page = FluidViewTemplate.EnsureFluidPage(context, "resources");
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

            var resourcesTagHelper = new ResourcesTagHelper(page.GetService<IResourceManager>(),
                page.GetService<IRequireSettingsProvider>());

            TagHelperAttribute attribute;
            if (attributes.TryGetAttribute("type", out attribute))
            {
                resourcesTagHelper.Type = (ResourceType)Enum.Parse(typeof(ResourceType), attribute.Value.ToString());
            }

            var tagHelperContext = new TagHelperContext(attributes,
                new Dictionary<object, object>(), Guid.NewGuid().ToString("N"));

            var tagHelperOutput = new TagHelperOutput(_name, attributes,
                getChildContentAsync: (useCachedResult, htmlEncoder) =>
                    Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));

            var shapeTagHelper = new ShapeTagHelper(page.GetService<IShapeFactory>(),
                page.GetService<IDisplayHelperFactory>()) { ViewContext = page.ViewContext };

            await shapeTagHelper.ProcessAsync(tagHelperContext, tagHelperOutput);

            tagHelperOutput.WriteTo(writer, HtmlEncoder.Default);

            return Completion.Normal;
        }
    }
}