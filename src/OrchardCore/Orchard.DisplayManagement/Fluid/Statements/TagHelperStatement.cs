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
using Microsoft.AspNetCore.Razor;
using Microsoft.AspNetCore.Razor.Compilation.TagHelpers;
using Microsoft.AspNetCore.Razor.Runtime.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Orchard.DisplayManagement.Fluid.Ast;

namespace Orchard.DisplayManagement.Fluid.Statements
{
    public class TagHelperStatement : TagStatement
    {
        private static ConcurrentBag<TagHelperDescriptor> _descriptors = new ConcurrentBag<TagHelperDescriptor>();
        private static ConcurrentBag<string> _registered = new ConcurrentBag<string>();

        private readonly ArgumentsExpression _arguments;

        public TagHelperStatement(string name, ArgumentsExpression arguments, IList<Statement> statements) : base(statements)
        {
            Name = name;
            _arguments = arguments;
        }

        public string Name { get; }

        public override async Task<Completion> WriteToAsync(TextWriter writer, TextEncoder encoder, TemplateContext context)
        {
            var page = FluidViewTemplate.EnsureFluidPage(context, Name);
            var arguments = (FilterArguments)(await _arguments.EvaluateAsync(context)).ToObjectValue();

            var descriptor = _descriptors.FirstOrDefault(d => d.TagName == Name);

            if (descriptor != null)
            {
                var tagHelperType = Type.GetType(descriptor.TypeName + ", " + descriptor.AssemblyName);

                var tagHelper = typeof(FluidPage).GetMethod("CreateTagHelper")
                    .MakeGenericMethod(tagHelperType).Invoke(page, null) as ITagHelper;

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

                tagHelperOutput.WriteTo(writer, page.HtmlEncoder);
            }

            return Completion.Normal;
        }

        internal static void Register(FluidPage page, string name, string assembly)
        {
            if (_registered.Contains(name + assembly) || _registered.Contains("*" + assembly))
            {
                return;
            }

            _registered.Add(name + assembly);

            var resolver = page.GetService<ITagHelperTypeResolver>();
            var types = resolver.Resolve(assembly, SourceLocation.Zero, new ErrorSink()).ToList();

            foreach (var type in types)
            {
                var descriptors = page.GetService<ITagHelperDescriptorFactory>().CreateDescriptors(
                    assembly, type, new ErrorSink()).GroupBy(d => d.TagName).Select(g => g.First());

                if (name != "*")
                {
                    descriptors = descriptors.Where(d => d.TagName == name);
                }

                foreach (var descriptor in descriptors)
                {
                    if (!_descriptors.Contains(descriptor, TagHelperDescriptorComparer.Default))
                    {
                        _descriptors.Add(descriptor);
                    }
                }
            }
        }
    }
}