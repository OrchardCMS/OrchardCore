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
    public class ResourcesStatement : Statement
    {
        private readonly ArgumentsExpression _arguments;

        public ResourcesStatement(ArgumentsExpression arguments)
        {
            _arguments = arguments;
        }

        public override async Task<Completion> WriteToAsync(TextWriter writer, TextEncoder encoder, TemplateContext context)
        {
            var page = FluidViewTemplate.EnsureFluidPage(context, "resources");
            var arguments = (FilterArguments)(await _arguments.EvaluateAsync(context)).ToObjectValue();

            var resourcesTagHelper = new ResourcesTagHelper(page.GetService<IResourceManager>(),
                page.GetService<IRequireSettingsProvider>());

            var attributes = new TagHelperAttributeList();

            if (arguments.HasNamed("type"))
            {
                resourcesTagHelper.Type = (ResourceType)Enum.Parse(typeof(ResourceType), arguments["type"].ToStringValue());
            }

            var tagHelperContext = new TagHelperContext(attributes,
                new Dictionary<object, object>(), Guid.NewGuid().ToString("N"));

            var tagHelperOutput = new TagHelperOutput("resources", attributes, (_, e)
                => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));

            await resourcesTagHelper.ProcessAsync(tagHelperContext, tagHelperOutput);

            tagHelperOutput.WriteTo(writer, page.HtmlEncoder);

            return Completion.Normal;
        }
    }
}