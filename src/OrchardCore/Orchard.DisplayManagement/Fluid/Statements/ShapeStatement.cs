using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Fluid;
using Fluid.Ast;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Orchard.DisplayManagement.Fluid.Ast;
using Orchard.DisplayManagement.TagHelpers;

namespace Orchard.DisplayManagement.Fluid.Statements
{
    public class ShapeStatement : Statement
    {
        private readonly ArgumentsExpression _arguments;

        public ShapeStatement(string type, ArgumentsExpression arguments)
        {
            Type = type;
            _arguments = arguments;
        }

        public string Type { get; }

        public override async Task<Completion> WriteToAsync(TextWriter writer, TextEncoder encoder, TemplateContext context)
        {
            if (!context.AmbientValues.TryGetValue("ShapeFactory", out var shapeFactory))
            {
                throw new ArgumentException("ShapeFactory missing while invoking 'shape " + Type + "'");
            }

            if (!context.AmbientValues.TryGetValue("DisplayHelperFactory", out var displayHelperFactory))
            {
                throw new ArgumentException("DisplayHelperFactory missing while invoking 'shape " + Type + "'");
            }

            if (!context.AmbientValues.TryGetValue("ViewContext", out var viewContext))
            {
                throw new ArgumentException("ViewContext missing while invoking 'shape " + Type + "'");
            }

            var arguments = (FilterArguments)(await _arguments.EvaluateAsync(context)).ToObjectValue();

            var attributes = new TagHelperAttributeList();
            foreach (var name in arguments.Names)
            {
                var attributeName = name.Replace("_", "-");
                attributes.Add(new TagHelperAttribute(attributeName, arguments[name].ToObjectValue()));
            }

            var tagHelperContext = new TagHelperContext(attributes,
                new Dictionary<object, object>(), Guid.NewGuid().ToString("N"));

            var tagHelperOutput = new TagHelperOutput(Type, attributes, (_, e) =>
                Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));

            var shapeTagHelper = new ShapeTagHelper((IShapeFactory) shapeFactory,
                (IDisplayHelperFactory)displayHelperFactory) { ViewContext = (ViewContext)viewContext };

            await shapeTagHelper.ProcessAsync(tagHelperContext, tagHelperOutput);

            tagHelperOutput.WriteTo(writer, HtmlEncoder.Default);

            return Completion.Normal;
        }
    }
}