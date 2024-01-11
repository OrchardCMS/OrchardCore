using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Fluid;
using Fluid.Ast;
using Fluid.Values;
using Newtonsoft.Json;
using OrchardCore.DisplayManagement.Shapes;
using OrchardCore.Mvc.Utilities;

namespace OrchardCore.DisplayManagement.Liquid.Tags
{
    public class ShapePagerTag
    {
        private static readonly HashSet<string> _properties = new()
        {
            "Id", "PreviousText", "NextText", "PreviousClass", "NextClass", "TagName", "ItemTagName"
        };

        public static async ValueTask<Completion> WriteToAsync(ValueTuple<Expression, List<FilterArgument>> arguments, TextWriter _1, TextEncoder _2, TemplateContext context)
        {
            var objectValue = (await arguments.Item1.EvaluateAsync(context)).ToObjectValue() as dynamic;

            if (objectValue is Shape shape)
            {
                if (shape.Metadata.Type == "PagerSlim")
                {
                    foreach (var argument in arguments.Item2)
                    {
                        var propertyName = argument.Name.ToPascalCaseUnderscore();

                        if (_properties.Contains(propertyName))
                        {
                            objectValue[propertyName] = (await argument.Expression.EvaluateAsync(context)).ToStringValue();
                        }
                    }
                }

                var expressions = new NamedExpressionList(arguments.Item2);

                if (shape.Metadata.Type == "PagerSlim" || shape.Metadata.Type == "Pager")
                {
                    if (expressions.HasNamed("item_classes"))
                    {
                        var itemClasses = await expressions["item_classes"].EvaluateAsync(context);

                        if (itemClasses.Type == FluidValues.String)
                        {
                            var values = itemClasses.ToStringValue().Split(' ', StringSplitOptions.RemoveEmptyEntries);

                            foreach (var value in values)
                            {
                                objectValue.ItemClasses.Add(value);
                            }
                        }
                        else if (itemClasses.Type == FluidValues.Array)
                        {
                            foreach (var value in itemClasses.Enumerate(context))
                            {
                                objectValue.ItemClasses.Add(value.ToStringValue());
                            }
                        }
                    }

                    if (expressions.HasNamed("classes"))
                    {
                        var classes = await expressions["classes"].EvaluateAsync(context);

                        if (classes.Type == FluidValues.String)
                        {
                            var values = classes.ToStringValue().Split(' ', StringSplitOptions.RemoveEmptyEntries);

                            foreach (var value in values)
                            {
                                objectValue.Classes.Add(value);
                            }
                        }
                        else if (classes.Type == FluidValues.Array)
                        {
                            foreach (var value in classes.Enumerate(context))
                            {
                                objectValue.Classes.Add(value.ToStringValue());
                            }
                        }
                    }

                    if (expressions.HasNamed("attributes"))
                    {
                        var attributes = await expressions["attributes"].EvaluateAsync(context);

                        if (attributes.Type == FluidValues.String)
                        {
                            var values = JsonConvert.DeserializeObject<Dictionary<string, string>>(attributes.ToStringValue());
                            foreach (var value in values)
                            {
                                objectValue.Attributes.TryAdd(value.Key, value.Value);
                            }
                        }
                    }

                    if (expressions.HasNamed("item_attributes"))
                    {
                        var itemAttributes = await expressions["item_attributes"].EvaluateAsync(context);

                        if (itemAttributes.Type == FluidValues.String)
                        {
                            var values = JsonConvert.DeserializeObject<Dictionary<string, string>>(itemAttributes.ToStringValue());
                            foreach (var value in values)
                            {
                                objectValue.ItemAttributes.TryAdd(value.Key, value.Value);
                            }
                        }
                    }
                }
            }

            return Completion.Normal;
        }
    }
}
