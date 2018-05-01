using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Fluid;
using Fluid.Values;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace OrchardCore.DisplayManagement.Liquid.TagHelpers
{
    public class LiquidTagHelperActivator
    {
        public readonly static LiquidTagHelperActivator None = new LiquidTagHelperActivator();
        private readonly Func<ITagHelperFactory, ViewContext, ITagHelper> _activator;
        private readonly Dictionary<string, Action<ITagHelper, FluidValue>> _setters = new Dictionary<string, Action<ITagHelper, FluidValue>>();

        public LiquidTagHelperActivator() { }

        public LiquidTagHelperActivator(Type type)
        {
            var accessibleProperties = type.GetProperties().Where(p =>
                p.GetCustomAttribute<HtmlAttributeNotBoundAttribute>() == null &&
                p.GetSetMethod() != null);

            foreach (var property in accessibleProperties)
            {
                var invokeType = typeof(Action<,>).MakeGenericType(type, property.PropertyType);
                var setterDelegate = Delegate.CreateDelegate(invokeType, property.GetSetMethod());

                _setters.Add(property.Name, (h, v) =>
                {
                    object value = null;

                    if (property.PropertyType.IsEnum)
                    {
                        value = Enum.Parse(property.PropertyType, v.ToStringValue());
                    }
                    else if (property.PropertyType == typeof(String))
                    {
                        value = v.ToStringValue();
                    }
                    else if (property.PropertyType == typeof(Boolean))
                    {
                        value = Convert.ToBoolean(v.ToStringValue());
                    }
                    else
                    {
                        value = v.ToObjectValue();
                    }

                    setterDelegate.DynamicInvoke(new[] { h, value });
                });
            }

            var genericFactory = typeof(ReusableTagHelperFactory<>).MakeGenericType(type);
            var factoryMethod = genericFactory.GetMethod("CreateTagHelper");

            _activator = Delegate.CreateDelegate(typeof(Func<ITagHelperFactory, ViewContext, ITagHelper>),
                factoryMethod) as Func<ITagHelperFactory, ViewContext, ITagHelper>;
        }

        public ITagHelper Create(ITagHelperFactory factory, ViewContext context, FilterArguments arguments,
            out TagHelperAttributeList contextAttributes, out TagHelperAttributeList outputAttributes)
        {
            contextAttributes = new TagHelperAttributeList();
            outputAttributes = new TagHelperAttributeList();

            var tagHelper = _activator(factory, context);

            foreach (var name in arguments.Names)
            {
                var propertyName = Filters.LiquidViewFilters.LowerKebabToPascalCase(name);

                var found = false;

                if (_setters.TryGetValue(propertyName, out var setter))
                {
                    try
                    {
                        setter(tagHelper, arguments[name]);
                        found = true;
                    }
                    catch (ArgumentException e)
                    {
                        throw new ArgumentException("Incorrect value type assigned to a tag.", name, e);
                    }
                }

                var attr = new TagHelperAttribute(name.Replace("_", "-"), arguments[name].ToObjectValue());

                contextAttributes.Add(attr);

                if (!found)
                {
                    outputAttributes.Add(attr);
                }
            }

            return tagHelper;
        }

        private class ReusableTagHelperFactory<T> where T : ITagHelper
        {
            public static ITagHelper CreateTagHelper(ITagHelperFactory tagHelperFactory, ViewContext viewContext)
            {
                return tagHelperFactory.CreateTagHelper<T>(viewContext);
            }
        }
    }
}