using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Fluid;
using Fluid.Ast;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor;
using Microsoft.AspNetCore.Razor.Compilation.TagHelpers;
using Microsoft.AspNetCore.Razor.Runtime.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.DependencyInjection;
using Orchard.DisplayManagement.Fluid.Ast;

namespace Orchard.DisplayManagement.Fluid.Statements
{
    public class TagHelperStatement : TagStatement
    {
        private static ConcurrentDictionary<string, TagHelperDescriptor> _descriptors = new ConcurrentDictionary<string, TagHelperDescriptor>();

        private readonly ArgumentsExpression _arguments;

        public TagHelperStatement(string name, ArgumentsExpression arguments, IList<Statement> statements) : base(statements)
        {
            Name = name.Replace("javascript", "script");
            _arguments = arguments;
        }

        public string Name { get; }

        public override async Task<Completion> WriteToAsync(TextWriter writer, TextEncoder encoder, TemplateContext context)
        {
            if (!context.AmbientValues.TryGetValue("Services", out var services))
            {
                throw new ArgumentException("Services missing while invoking '" + Name + "'");
            }

            if (!context.AmbientValues.TryGetValue("ViewContext", out var viewContext))
            {
                throw new ArgumentException("ViewContext missing while invoking '" + Name + "'");
            }

            var arguments = (FilterArguments)(await _arguments.EvaluateAsync(context)).ToObjectValue();

            // temporary code before implementing tag helpers differently
            Register((IServiceProvider)services, "*", "Orchard.Contents");
            Register((IServiceProvider)services, "*", "Orchard.ResourceManagement");

            var descriptor = _descriptors.FirstOrDefault(kv => kv.Key.StartsWith(Name)).Value;

            if (descriptor == null)
            {
                return Completion.Normal;
            }

            var tagHelperType = Type.GetType(descriptor.TypeName + ", " + descriptor.AssemblyName);
            var razorPage = (((ViewContext)viewContext).View as RazorView).RazorPage as RazorPage;

            var tagHelper = typeof(FluidPage).GetMethod("CreateTagHelper")
                .MakeGenericMethod(tagHelperType).Invoke(razorPage, null) as ITagHelper;

            var attributes = new TagHelperAttributeList();

            foreach (var name in arguments.Names)
            {
                var attributeName = name.Replace("_", "-");
                var found = false;

                foreach (var attribute in descriptor.Attributes)
                {
                    if (attribute.IsNameMatch(attributeName))
                    {
                        found = true;
                        var property = tagHelperType.GetProperty(attribute.PropertyName);

                        if (attribute.IsEnum)
                        {
                            var value = Enum.Parse(property.PropertyType, arguments[name].ToStringValue());
                            property.SetValue(tagHelper, value, null);
                        }
                        else if (attribute.IsStringProperty)
                        {
                            property.SetValue(tagHelper, arguments[name].ToStringValue(), null);
                        }
                        else if (property.PropertyType == typeof(Boolean))
                        {
                            var value = Convert.ToBoolean(arguments[name].ToStringValue());
                            property.SetValue(tagHelper, value, null);
                        }

                        // Todo: implement attribute.IsIndexer

                        else
                        {
                            property.SetValue(tagHelper, arguments[name].ToObjectValue(), null);
                        }

                        break;
                    }
                }

                if (!found)
                {
                    attributes.Add(new TagHelperAttribute(attributeName, arguments[name].ToObjectValue()));
                }
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

            var tagHelperOutput = new TagHelperOutput(Name, attributes, (_, e)
                => Task.FromResult(new DefaultTagHelperContent().AppendHtml(content.ToString())));

            tagHelperOutput.Content.AppendHtml(content.ToString());
            await tagHelper.ProcessAsync(tagHelperContext, tagHelperOutput);

            tagHelperOutput.WriteTo(writer, HtmlEncoder.Default);

            return Completion.Normal;
        }

        internal static void Register(IServiceProvider services, string name, string assembly)
        {
            if (_descriptors.ContainsKey(name + assembly) || _descriptors.ContainsKey("*" + assembly))
            {
                return;
            }

            var resolver = services.GetRequiredService<ITagHelperTypeResolver>();
            var types = resolver.Resolve(assembly, SourceLocation.Zero, new ErrorSink()).ToList();

            foreach (var type in types)
            {
                var descriptors = services.GetRequiredService<ITagHelperDescriptorFactory>().CreateDescriptors(
                    assembly, type, new ErrorSink()).GroupBy(d => d.TagName).Select(g => g.First());

                if (name != "*")
                {
                    descriptors = descriptors.Where(d => d.TagName == name);
                }

                foreach (var descriptor in descriptors)
                {
                    _descriptors[descriptor.TagName + assembly] = descriptor;
                }
            }
        }
    }
}