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
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Razor;
using Microsoft.AspNetCore.Razor.Runtime.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Orchard.DisplayManagement.Fluid.Ast;
using Orchard.Mvc;

namespace Orchard.DisplayManagement.Fluid.Statements
{
    public class TagHelperStatement : TagStatement
    {
        private static ConcurrentDictionary<string, Type> _types = new ConcurrentDictionary<string, Type>();
        private static ConcurrentBag<string> _assemblies = new ConcurrentBag<string>();

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

            EnsureTagHelpers(page);

            if (!_types.TryGetValue(Name, out var tagHelperType))
            {
                return Completion.Normal;
            }

            var tagHelper = typeof(FluidPage).GetMethod("CreateTagHelper")
                .MakeGenericMethod(tagHelperType).Invoke(page, null) as ITagHelper;

            if (tagHelper == null)
            {
                return Completion.Normal;
            }

            var descriptors = page.GetService<ITagHelperDescriptorFactory>().CreateDescriptors(
                tagHelperType.GetTypeInfo().Assembly.GetName().Name, tagHelperType, new ErrorSink());

            var tagHelperDescriptor = descriptors.FirstOrDefault(x => x.TagName == Name);

            if (tagHelperDescriptor != null)
            {
                var attributes = new TagHelperAttributeList();

                foreach (var name in arguments.Names)
                {
                    var attributeName = name.Replace("_", "-");
                    var found = false;

                    foreach (var attribute in tagHelperDescriptor.Attributes)
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

        internal static void EnsureTagHelpers(FluidPage page)
        {
            if (!_types.IsEmpty)
            {
                return;
            }

            var types = page.GetService<ApplicationPartManager>().ApplicationParts
                .Where(part => part is TagHelperApplicationPart)
                .SelectMany(part => ((TagHelperApplicationPart)part).Types);

            if (!types.Any())
            {
                _types["notempty"] = typeof(object);
                return;
            }

            var assemblies = types.Select(type => type.Assembly.GetName().Name).Distinct();

            foreach (var type in types)
            {
                _types[GetTagName(type.AsType())] = type.AsType();
            }

            foreach (var assembly in assemblies)
            {
                _assemblies.Add(assembly);
            }
        }

        internal static void RegisterTagHelper(FluidPage page, string name, string assembly)
        {
            EnsureTagHelpers(page);

            if (name != "*" && _types.TryGetValue(name, out var tagHelperType) || _assemblies.Contains(assembly))
            {
                return;
            }

            var resolver = page.GetService<ITagHelperTypeResolver>();

            var types = resolver.Resolve(assembly, SourceLocation.Zero, new ErrorSink()).ToList();

            if (name == "*")
            {
                _assemblies.Add(assembly);

                foreach (var type in types)
                {
                    _types[GetTagName(type)] = type;
                }
            }
            else
            {
                var type = types.FirstOrDefault(t => GetTagName(t) == name);

                if (type != null)
                {
                    _types[GetTagName(type)] = type;

                    if (!types.Except(_types.Values).Any())
                    {
                        _assemblies.Add(assembly);
                    }
                }
            }
        }

        internal static string GetTagName(Type type)
        {
            var attributes = type.GetTypeInfo().CustomAttributes.Where(
                a => a.AttributeType == typeof(HtmlTargetElementAttribute));

            return attributes.Any() ? attributes.FirstOrDefault().ConstructorArguments
                .FirstOrDefault().Value.ToString() : type.GetTypeInfo().Name;
        }
    }
}