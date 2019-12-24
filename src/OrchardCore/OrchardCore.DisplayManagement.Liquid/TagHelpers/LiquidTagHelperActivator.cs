using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Fluid;
using Fluid.Values;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using OrchardCore.Mvc.Utilities;

namespace OrchardCore.DisplayManagement.Liquid.TagHelpers
{
    public class LiquidTagHelperActivator
    {
        public readonly static LiquidTagHelperActivator None = new LiquidTagHelperActivator();
        private readonly Func<ITagHelperFactory, ViewContext, ITagHelper> _activator;
        private readonly Dictionary<string, Action<ITagHelper, ModelExpressionProvider, ViewDataDictionary<dynamic> ,FluidValue, string>> _setters = new Dictionary<string, Action<ITagHelper, ModelExpressionProvider, ViewDataDictionary<dynamic> ,FluidValue, string>>(StringComparer.OrdinalIgnoreCase);

        public LiquidTagHelperActivator() { }

        public LiquidTagHelperActivator(Type type)
        {
            var accessibleProperties = type.GetProperties().Where(p =>
                p.GetCustomAttribute<HtmlAttributeNotBoundAttribute>() == null &&
                p.GetSetMethod() != null);

            foreach (var property in accessibleProperties)
            {
                var invokeType = typeof(Action<,>).MakeGenericType(type, property.PropertyType);
                var invokeTypeGet = typeof(Func<,>).MakeGenericType(type, property.PropertyType);                
                var setterDelegate = Delegate.CreateDelegate(invokeType, property.GetSetMethod());
                var getterDelegate = Delegate.CreateDelegate(invokeTypeGet, property.GetGetMethod());

                var allNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { property.Name };
                var htmlAttribute = property.GetCustomAttribute<HtmlAttributeNameAttribute>();

                if (htmlAttribute != null && htmlAttribute.Name != null)
                {
                    allNames.Add(htmlAttribute.Name.ToPascalCaseDash());

                    if (htmlAttribute.Name.StartsWith("asp-", StringComparison.Ordinal))
                    {
                        allNames.Add(htmlAttribute.Name.Substring(4).ToPascalCaseDash());
                    }
                    var dictonaryPrefix =  htmlAttribute.DictionaryAttributePrefix;
                    if(dictonaryPrefix != null)
                    {
                        allNames.Add(dictonaryPrefix.ToPascalCaseDash());

                        if (dictonaryPrefix.StartsWith("asp-", StringComparison.Ordinal))
                        {
                            allNames.Add(dictonaryPrefix.Substring(4).ToPascalCaseDash());
                        }
                    }
                }

                foreach (var propertyName in allNames)
                {
                    _setters.Add(propertyName, (h, mp, vd, v, k) =>
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
                        else if (property.PropertyType == typeof(Nullable<Boolean>))
                        {
                            value = v.IsNil() ? null : (bool?)Convert.ToBoolean(v.ToStringValue());
                        }
                        else if(property.PropertyType == typeof(IDictionary<string, string>)) 
                        {
                            IDictionary<string, string> dictValue = (IDictionary<string, string>) getterDelegate.DynamicInvoke(new[] { h }); 
                            if(!string.IsNullOrWhiteSpace(k))
                                dictValue[k] = v.ToStringValue();
                            value = dictValue;
                        }
                        else if(property.PropertyType == typeof(IDictionary<string, object>)) 
                        {
                            IDictionary<string, object> dictValue = (IDictionary<string, object>) getterDelegate.DynamicInvoke(new[] { h });
                            if(!string.IsNullOrWhiteSpace(k)) 
                                dictValue[k] = v.ToObjectValue();
                            value = dictValue;
                        }
                        else if(property.PropertyType == typeof(Microsoft.AspNetCore.Mvc.ViewFeatures.ModelExpression))
                        {
                            value = mp.CreateModelExpression<dynamic>(vd,v.ToStringValue() );                            
                        }
                        else
                        {
                            value = v.ToObjectValue();
                        }

                        setterDelegate.DynamicInvoke(new[] { h, value });
                    });
                }
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
            var dictKeySeperator =new char[] {'-','_'};
            
            var expresionProvider = context.HttpContext.RequestServices.GetService(typeof(Microsoft.AspNetCore.Mvc.ViewFeatures.ModelExpressionProvider)) as 
                            Microsoft.AspNetCore.Mvc.ViewFeatures.ModelExpressionProvider;

            var viewData = context.ViewData as ViewDataDictionary<dynamic>;

            foreach (var name in arguments.Names)
            {
                var propertyName = name.ToPascalCaseUnderscore();
                var dictPropertyName = propertyName.LastIndexOfAny(dictKeySeperator) > -1 ? propertyName.Substring(0,propertyName.LastIndexOfAny(dictKeySeperator)+1) : String.Empty;
                var dictPropertyKey = propertyName.LastIndexOfAny(dictKeySeperator) > -1 ? propertyName.Substring(propertyName.LastIndexOfAny(dictKeySeperator)+1) : String.Empty;
                var found = false;

                if (_setters.TryGetValue(propertyName, out var setter) || ( !string.IsNullOrWhiteSpace(dictPropertyName) && _setters.TryGetValue(dictPropertyName, out setter) ) )
                {
                    try
                    {
                        setter(tagHelper, expresionProvider, viewData, arguments[name], dictPropertyKey);
                        found = true;
                    }
                    catch (ArgumentException e)
                    {
                        throw new ArgumentException("Incorrect value type assigned to a tag.", name, e);
                    }
                }

                var attr = new TagHelperAttribute(name.Replace('_', '-'), arguments[name].ToObjectValue());

                contextAttributes.Add(attr);

                if (!found)
                {
                    outputAttributes.Add(attr);
                }
            }

            return tagHelper;
        }

        private class ReusableTagHelperFactory<T> where T : class, ITagHelper
        {
            public static ITagHelper CreateTagHelper(ITagHelperFactory tagHelperFactory, ViewContext viewContext)
            {
                return tagHelperFactory.CreateTagHelper<T>(viewContext);
            }
        }
    }
}
