using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Fluid;
using Fluid.Ast;
using Fluid.Values;
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
        private static Dictionary<string, Type> _tagHelperTypes;
        private static object _synLock = new object();

        private readonly ArgumentsExpression _arguments;
        private readonly string _baseType;

        public TagHelperStatement(string name, ArgumentsExpression arguments, IList<Statement> statements, string baseType = null) : base(statements)
        {
            Name = name;
            _arguments = arguments;
            _baseType = baseType;
        }

        public string Name { get; }

        public override async Task<Completion> WriteToAsync(TextWriter writer, TextEncoder encoder, TemplateContext context)
        {
            var page = FluidViewTemplate.EnsureFluidPage(context, Name);
            var arguments = (FilterArguments)(await _arguments.EvaluateAsync(context)).ToObjectValue();

            if (_tagHelperTypes == null)
            {
                lock (_synLock)
                {
                    if (_tagHelperTypes == null)
                    {
                        var partManager = page.GetService<ApplicationPartManager>();
                        var resolver = page.GetService<ITagHelperTypeResolver>();

                        var tagHelpersTypes = new List<TypeInfo>(partManager.ApplicationParts
                            .Where(x => x is TagHelperApplicationPart)
                            .SelectMany(x => ((TagHelperApplicationPart)x).Types));

                        // tag helpers which are not yet registered in applicationParts
                        tagHelpersTypes = tagHelpersTypes.Union(resolver.Resolve("Orchard.Contents", SourceLocation.Zero, new ErrorSink())
                            .Select(x => x.GetTypeInfo())).ToList();

                        _tagHelperTypes = tagHelpersTypes.ToDictionary(
                            typeInfo =>
                            {
                                var attributes = typeInfo.CustomAttributes
                                    .Where(a => a.AttributeType == typeof(HtmlTargetElementAttribute));

                                if (attributes.Any())
                                {
                                    return attributes.FirstOrDefault().ConstructorArguments
                                        .FirstOrDefault().Value.ToString();
                                }

                                return typeInfo.Name;
                            },
                            typeInfo => typeInfo.AsType());
                    }
                }
            }

            if (!_tagHelperTypes.TryGetValue(_baseType ?? Name, out var tagHelperType))
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

            var tagHelperDescriptor = descriptors.FirstOrDefault(x => x.TagName == (_baseType ?? Name));

            if (tagHelperDescriptor != null)
            {
                var attributes = new TagHelperAttributeList();

                if (_baseType != null && tagHelperDescriptor.Attributes.Any(x => x.PropertyName == "Type"))
                {
                    arguments.Add("Type", new StringValue(Name));
                }

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
    }
}