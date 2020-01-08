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
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.Mvc.Utilities;

namespace OrchardCore.DisplayManagement.Liquid.TagHelpers
{
    public class LiquidTagHelperActivator
    {
        public static readonly LiquidTagHelperActivator None = new LiquidTagHelperActivator();

        private readonly Func<ITagHelperFactory, ViewContext, ITagHelper> _activatorByFactory;
        private readonly Dictionary<string, Action<ITagHelper, ModelExpressionProvider, ViewDataDictionary<dynamic>, string, FluidValue>> _setters =
            new Dictionary<string, Action<ITagHelper, ModelExpressionProvider, ViewDataDictionary<dynamic>, string, FluidValue>>(StringComparer.OrdinalIgnoreCase);

        private readonly Func<ViewContext, ITagHelper> _activatorByService;
        private readonly Action<object, object> _viewContextSetter;

        public LiquidTagHelperActivator() { }

        public LiquidTagHelperActivator(Type type)
        {
            var accessibleProperties = type.GetProperties().Where(p =>
                (p.GetCustomAttribute<HtmlAttributeNotBoundAttribute>() == null ||
                p.GetCustomAttribute<ViewContextAttribute>() != null) &&
                p.GetSetMethod() != null);

            foreach (var property in accessibleProperties)
            {
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

                var setter = MakeFastPropertySetter(type, property);
                var getter = MakeFastPropertyGetter(type, property);

                foreach (var propertyName in allNames)
                {
                    if (propertyName == "ViewContext")
                    {
                        _viewContextSetter = (helper, context) => setter(helper, context);
                        continue;
                    }

                    _setters.Add(propertyName, (h, mp, vd, k, v) =>
                    {
                        object value = null;

                        if (property.PropertyType.IsEnum)
                        {
                            value = Enum.Parse(property.PropertyType, v.ToStringValue());
                        }
                        else if (property.PropertyType == typeof(string))
                        {
                            value = v.ToStringValue();
                        }
                        else if (property.PropertyType == typeof(bool))
                        {
                            value = Convert.ToBoolean(v.ToStringValue());
                        }
                        else if (property.PropertyType == typeof(bool?))
                        {
                            value = v.IsNil() ? null : (bool?)Convert.ToBoolean(v.ToStringValue());
                        }
                        else if(property.PropertyType == typeof(IDictionary<string, string>)) 
                        {
                            IDictionary<string, string> dictValue = (IDictionary<string, string>) getter(h); 
                            if(!string.IsNullOrWhiteSpace(k))
                                dictValue[k] = v.ToStringValue();
                            value = dictValue;
                        }
                        else if(property.PropertyType == typeof(IDictionary<string, object>)) 
                        {
                            IDictionary<string, object> dictValue = (IDictionary<string, object>) getter(h);
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

                        setter(h, value);
                    });
                }
            }

            if (ShellScope.Services.GetService(type) as ITagHelper != null)
            {
                _activatorByService = (context) =>
                {
                    var helper = ShellScope.Services.GetService(type) as ITagHelper;
                    _viewContextSetter?.Invoke(helper, context);
                    return helper;
                };
            }
            else
            {
                var genericFactory = typeof(ReusableTagHelperFactory<>).MakeGenericType(type);
                var factoryMethod = genericFactory.GetMethod("CreateTagHelper");

                _activatorByFactory = Delegate.CreateDelegate(typeof(Func<ITagHelperFactory, ViewContext, ITagHelper>),
                    factoryMethod) as Func<ITagHelperFactory, ViewContext, ITagHelper>;
            }
        }

        public ITagHelper Create(ITagHelperFactory factory, ViewContext context, FilterArguments arguments,
            out TagHelperAttributeList contextAttributes, out TagHelperAttributeList outputAttributes)
        {
            contextAttributes = new TagHelperAttributeList();
            outputAttributes = new TagHelperAttributeList();

            ITagHelper tagHelper;

            if (_activatorByService != null)
            {
                tagHelper = _activatorByService(context);
            }
            else
            {
                tagHelper = _activatorByFactory(factory, context);
            }
            
            var dictKeySeperator =new char[] {'-','_'};
            
            var expresionProvider = context.HttpContext.RequestServices.GetService(typeof(Microsoft.AspNetCore.Mvc.ViewFeatures.ModelExpressionProvider)) as 
                            Microsoft.AspNetCore.Mvc.ViewFeatures.ModelExpressionProvider;

            var viewData = new ViewDataDictionary<dynamic>(context.ViewData);

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
                        setter(tagHelper, expresionProvider, viewData, dictPropertyKey, arguments[name] );
                        found = true;
                    }
                    catch (ArgumentException e)
                    {
                        throw new ArgumentException("Incorrect value type assigned to a tag.", name, e);
                    }
                }

                var attribute = new TagHelperAttribute(name.Replace('_', '-'), arguments[name].ToObjectValue());

                contextAttributes.Add(attribute);

                if (!found)
                {
                    outputAttributes.Add(attribute);
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

        private static Action<object, object> MakeFastPropertySetter(Type type, PropertyInfo prop)
        {
            // Create a delegate TDeclaringType -> { TDeclaringType.Property = TValue; }
            var setterAsAction = prop.SetMethod.CreateDelegate(typeof(Action<,>).MakeGenericType(type, prop.PropertyType));
            var setterClosedGenericMethod = CallPropertySetterOpenGenericMethod.MakeGenericMethod(type, prop.PropertyType);
            var setterDelegate = setterClosedGenericMethod.CreateDelegate(typeof(Action<object, object>), setterAsAction);

            return (Action<object, object>)setterDelegate;
        }               

        private static readonly MethodInfo CallPropertySetterOpenGenericMethod =
            typeof(LiquidTagHelperActivator).GetTypeInfo().GetDeclaredMethod(nameof(CallPropertySetter));

        private static void CallPropertySetter<TDeclaringType, TValue>(Action<TDeclaringType, TValue> setter, object target, object value)
            => setter((TDeclaringType)target, (TValue)value);

        private static Func<object, object> MakeFastPropertyGetter(Type type, PropertyInfo prop)
        {
            // Create a delegate TDeclaringType -> { TDeclaringType.Property = TValue; }
            var getterAsFunc = prop.GetMethod.CreateDelegate(typeof(Func<,>).MakeGenericType(type, prop.PropertyType));
            var getterClosedGenericMethod = CallPropertyGetterOpenGenericMethod.MakeGenericMethod(type, prop.PropertyType);
            var getterDelegate = getterClosedGenericMethod.CreateDelegate(typeof(Func<object, object>), getterAsFunc);

            return (Func<object, object>)getterDelegate;
        }

        private static readonly MethodInfo CallPropertyGetterOpenGenericMethod =
            typeof(LiquidTagHelperActivator).GetTypeInfo().GetDeclaredMethod(nameof(CallPropertyGetter));

        private static object CallPropertyGetter<TDeclaringType, TValue>(Func<TDeclaringType, TValue> getter, object target)
            =>  getter((TDeclaringType)target);
    }
}
