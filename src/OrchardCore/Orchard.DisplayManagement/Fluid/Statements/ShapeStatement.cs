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

        public ShapeStatement(string name, ArgumentsExpression arguments)
        {
            Name = name;
            _arguments = arguments;
        }

        public string Name { get; }

        public override async Task<Completion> WriteToAsync(TextWriter writer, TextEncoder encoder, TemplateContext context)
        {
            if (!context.AmbientValues.TryGetValue("ShapeFactory", out var shapeFactory))
            {
                throw new ArgumentException("ShapeFactory missing while invoking '" + Name + "'");
            }

            if (!context.AmbientValues.TryGetValue("DisplayHelperFactory", out var displayHelperFactory))
            {
                throw new ArgumentException("DisplayHelperFactory missing while invoking '" + Name + "'");
            }

            if (!context.AmbientValues.TryGetValue("ViewContext", out var viewContext))
            {
                throw new ArgumentException("ViewContext missing while invoking '" + Name + "'");
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

            var tagHelperOutput = new TagHelperOutput(Name, attributes, (_, e) =>
                Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));

            var shapeTagHelper = new ShapeTagHelper((IShapeFactory) shapeFactory,
                (IDisplayHelperFactory)displayHelperFactory) { ViewContext = (ViewContext)viewContext };

            await shapeTagHelper.ProcessAsync(tagHelperContext, tagHelperOutput);

            tagHelperOutput.WriteTo(writer, HtmlEncoder.Default);

            return Completion.Normal;
        }
    }
}