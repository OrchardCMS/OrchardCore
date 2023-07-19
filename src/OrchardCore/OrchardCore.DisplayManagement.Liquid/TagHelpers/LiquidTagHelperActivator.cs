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
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.Mvc.Utilities;

namespace OrchardCore.DisplayManagement.Liquid.TagHelpers
{
    public class LiquidTagHelperActivator
    {
        private const string AspPrefix = "asp-";

        public static readonly LiquidTagHelperActivator None = new();

        private readonly Func<ITagHelperFactory, ViewContext, ITagHelper> _activatorByFactory;
        private readonly Dictionary<string, Action<ITagHelper, ModelExpressionProvider, ViewDataDictionary<object>, string, FluidValue>> _setters =
            new(StringComparer.OrdinalIgnoreCase);

        private readonly Func<ViewContext, ITagHelper> _activatorByService;
        private readonly Action<object, object> _viewContextSetter;

        public LiquidTagHelperActivator()
        {
        }

        public LiquidTagHelperActivator(Type type)
        {
            var accessibleProperties = type.GetProperties().Where(p =>
                (p.GetCustomAttribute<HtmlAttributeNotBoundAttribute>() == null ||
                p.GetCustomAttribute<ViewContextAttribute>() != null) &&
                p.GetSetMethod() != null);

            foreach (var property in accessibleProperties)
            {
                var setter = MakeFastPropertySetter(type, property);
                var viewContextAttribute = property.GetCustomAttribute<ViewContextAttribute>();

                if (viewContextAttribute != null && property.PropertyType == typeof(ViewContext))
                {
                    _viewContextSetter = (helper, context) => setter(helper, context);
                    continue;
                }

                var allNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { property.Name };
                var htmlAttribute = property.GetCustomAttribute<HtmlAttributeNameAttribute>();

                if (htmlAttribute != null && htmlAttribute.Name != null)
                {
                    allNames.Add(htmlAttribute.Name.ToPascalCaseDash());

                    if (htmlAttribute.Name.StartsWith(AspPrefix, StringComparison.Ordinal))
                    {
                        allNames.Add(htmlAttribute.Name[AspPrefix.Length..].ToPascalCaseDash());
                    }

                    var dictionaryPrefix = htmlAttribute.DictionaryAttributePrefix;
                    if (dictionaryPrefix != null)
                    {
                        allNames.Add(dictionaryPrefix.Replace('-', '_'));

                        if (dictionaryPrefix.StartsWith(AspPrefix, StringComparison.Ordinal))
                        {
                            allNames.Add(dictionaryPrefix[AspPrefix.Length..].Replace('-', '_'));
                        }
                    }
                }

                var getter = MakeFastPropertyGetter(type, property);

                foreach (var propertyName in allNames)
                {
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
                        else if (property.PropertyType == typeof(IDictionary<string, string>))
                        {
                            var dictionary = (IDictionary<string, string>)getter(h);
                            dictionary[k] = v.ToStringValue();
                            value = dictionary;
                        }
                        else if (property.PropertyType == typeof(IDictionary<string, object>))
                        {
                            var dictionary = (IDictionary<string, object>)getter(h);
                            dictionary[k] = v.ToObjectValue();
                            value = dictionary;
                        }
                        else if (property.PropertyType == typeof(ModelExpression))
                        {
                            value = mp.CreateModelExpression(vd, v.ToStringValue());
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

            var expresionProvider = context.HttpContext.RequestServices.GetRequiredService<ModelExpressionProvider>();

            var viewData = context.ViewData as ViewDataDictionary<object>;
            viewData ??= new ViewDataDictionary<object>(context.ViewData);

            foreach (var name in arguments.Names)
            {
                var propertyName = name.ToPascalCaseUnderscore();
                var dictionaryName = String.Empty;
                var dictionaryKey = String.Empty;

                if (!_setters.TryGetValue(propertyName, out var setter))
                {
                    var index = name.LastIndexOf('_');

                    if (index > -1)
                    {
                        dictionaryName = name[..(index + 1)];
                        dictionaryKey = name[(index + 1)..];

                        if (dictionaryName.Length > 0 && dictionaryKey.Length > 0)
                        {
                            _setters.TryGetValue(dictionaryName, out setter);
                        }
                    }
                }

                var found = false;

                if (setter != null)
                {
                    try
                    {
                        setter(tagHelper, expresionProvider, viewData, dictionaryKey, arguments[name]);
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
            var setterClosedGenericMethod = _callPropertySetterOpenGenericMethod.MakeGenericMethod(type, prop.PropertyType);
            var setterDelegate = setterClosedGenericMethod.CreateDelegate(typeof(Action<object, object>), setterAsAction);

            return (Action<object, object>)setterDelegate;
        }

        private static readonly MethodInfo _callPropertySetterOpenGenericMethod =
            typeof(LiquidTagHelperActivator).GetTypeInfo().GetDeclaredMethod(nameof(CallPropertySetter));

        private static void CallPropertySetter<TDeclaringType, TValue>(Action<TDeclaringType, TValue> setter, object target, object value)
            => setter((TDeclaringType)target, (TValue)value);

        private static Func<object, object> MakeFastPropertyGetter(Type type, PropertyInfo prop)
        {
            // Create a delegate TDeclaringType -> { TDeclaringType.Property = TValue; }
            var getterAsFunc = prop.GetMethod.CreateDelegate(typeof(Func<,>).MakeGenericType(type, prop.PropertyType));
            var getterClosedGenericMethod = _callPropertyGetterOpenGenericMethod.MakeGenericMethod(type, prop.PropertyType);
            var getterDelegate = getterClosedGenericMethod.CreateDelegate(typeof(Func<object, object>), getterAsFunc);

            return (Func<object, object>)getterDelegate;
        }

        private static readonly MethodInfo _callPropertyGetterOpenGenericMethod =
            typeof(LiquidTagHelperActivator).GetTypeInfo().GetDeclaredMethod(nameof(CallPropertyGetter));

        private static object CallPropertyGetter<TDeclaringType, TValue>(Func<TDeclaringType, TValue> getter, object target)
            => getter((TDeclaringType)target);
    }
}
