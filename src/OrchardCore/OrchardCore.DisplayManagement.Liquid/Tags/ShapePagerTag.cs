using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Fluid;
using Fluid.Ast;
using Fluid.Values;
using OrchardCore.DisplayManagement.Shapes;
using OrchardCore.Liquid.Ast;
using OrchardCore.Mvc.Utilities;

namespace OrchardCore.DisplayManagement.Liquid.Tags
{
    public class ShapePagerTag : ExpressionArgumentsTag
    {
        private static readonly HashSet<string> _properties = new HashSet<string>
        {
            "PreviousText", "NextText", "PreviousClass", "NextClass"
        };

        public override async ValueTask<Completion> WriteToAsync(TextWriter writer, TextEncoder encoder, TemplateContext context, Expression expression, FilterArgument[] args)
        {
            var objectValue = (await expression.EvaluateAsync(context)).ToObjectValue() as dynamic;

            if (objectValue is Shape shape)
            {
                var arguments = (FilterArguments)(await new ArgumentsExpression(args).EvaluateAsync(context)).ToObjectValue();

                if (shape.Metadata.Type == "PagerSlim")
                {
                    foreach (var name in arguments.Names)
                    {
                        var argument = arguments[name];
                        var propertyName = name.ToPascalCaseUnderscore();

                        if (_properties.Contains(propertyName))
                        {
                            objectValue[propertyName] = argument.ToStringValue();
                        }
                    }
                }

                if (shape.Metadata.Type == "PagerSlim" || shape.Metadata.Type == "Pager")
                {
                    if (arguments.Names.Contains("item_classes"))
                    {
                        var classes = arguments["item_classes"];

                        if (classes.Type == FluidValues.String)
                        {
                            var values = classes.ToStringValue().Split(' ', StringSplitOptions.RemoveEmptyEntries);

                            foreach (var value in values)
                            {
                                objectValue.ItemClasses.Add(value);
                            }
                        }

                        else if (classes.Type == FluidValues.Array)
                        {
                            foreach (var value in classes.Enumerate())
                            {
                                objectValue.ItemClasses.Add(value.ToStringValue());
                            }
                        }
                    }
                }
            }

            return Completion.Normal;
        }
    }
}
