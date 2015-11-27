using Microsoft.AspNet.Html.Abstractions;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.Rendering;
using Microsoft.AspNet.Mvc.Routing;
using Microsoft.AspNet.Mvc.ViewFeatures;
using Microsoft.AspNet.Mvc.ViewFeatures.Internal;
using Microsoft.CSharp.RuntimeBinder;
using Microsoft.Extensions.DependencyInjection;
using Orchard.DisplayManagement.Implementation;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Orchard.DisplayManagement.Descriptors.ShapeAttributeStrategy
{
    public class ShapeAttributeBindingStrategy : IShapeTableProvider
    {
        private readonly IEnumerable<ShapeAttributeOccurrence> _shapeAttributeOccurrences;
        private readonly IServiceProvider _componentContext;
        private readonly ITypeFeatureProvider _typeFeatureProvider;

        public ShapeAttributeBindingStrategy(
            IEnumerable<ShapeAttributeOccurrence> shapeAttributeOccurrences,
            IServiceProvider componentContext,
            ITypeFeatureProvider typeFeatureProvider)
        {
            _shapeAttributeOccurrences = shapeAttributeOccurrences;
            _componentContext = componentContext;
            _typeFeatureProvider = typeFeatureProvider;
        }

        public void Discover(ShapeTableBuilder builder)
        {
            foreach (var iter in _shapeAttributeOccurrences)
            {
                var occurrence = iter;
                var shapeType = occurrence.ShapeAttribute.ShapeType ?? occurrence.MethodInfo.Name;
                builder.Describe(shapeType)
                    .From(_typeFeatureProvider.GetFeatureForDependency(occurrence.ServiceType))
                    .BoundAs(
                        occurrence.MethodInfo.DeclaringType.FullName + "::" + occurrence.MethodInfo.Name,
                        descriptor => CreateDelegate(occurrence, descriptor));
            }
        }

        [DebuggerStepThrough]
        private Func<DisplayContext, IHtmlContent> CreateDelegate(
            ShapeAttributeOccurrence attributeOccurrence,
            ShapeDescriptor descriptor)
        {
            return context =>
            {
                var serviceInstance = _componentContext.GetService(attributeOccurrence.ServiceType);
                // oversimplification for the sake of evolving
                return PerformInvoke(context, attributeOccurrence.MethodInfo, serviceInstance);
            };
        }

        private IHtmlContent PerformInvoke(DisplayContext displayContext, MethodInfo methodInfo, object serviceInstance)
        {
            using (var output = new StringCollectionTextWriter(System.Text.Encoding.UTF8))
            {
                var arguments = methodInfo
                    .GetParameters()
                    .Select(parameter => BindParameter(displayContext, parameter, output));

                // Resolve the service the method is declared on
                var returnValue = methodInfo.Invoke(serviceInstance, arguments.ToArray());

                // If the shape returns a value, write it to the stream
                if (methodInfo.ReturnType != typeof(void))
                {
                    output.Write(CoerceHtmlString(returnValue));
                }

                return output.Content;
            }
        }

        private static IHtmlContent CoerceHtmlString(object invoke)
        {
            return invoke as IHtmlContent ?? (invoke != null ? new HtmlString(invoke.ToString()) : null);
        }

        private object BindParameter(DisplayContext displayContext, ParameterInfo parameter, TextWriter output)
        {
            if (String.Equals(parameter.Name, "Shape", StringComparison.OrdinalIgnoreCase))
            {
                return displayContext.Value;
            }

            if (String.Equals(parameter.Name, "Display", StringComparison.OrdinalIgnoreCase))
            {
                return displayContext.Display;
            }

            if (String.Equals(parameter.Name, "Output", StringComparison.OrdinalIgnoreCase) &&
                parameter.ParameterType == typeof(TextWriter))
            {
                return output;
            }

            if (String.Equals(parameter.Name, "Output", StringComparison.OrdinalIgnoreCase) &&
                parameter.ParameterType == typeof(Action<object>))
            {
                return new Action<object>(output.Write);
            }

            if (String.Equals(parameter.Name, "Html", StringComparison.OrdinalIgnoreCase))
            {
                return MakeHtmlHelper(displayContext.ViewContext, displayContext.ViewContext.ViewData);
            }

            if (String.Equals(parameter.Name, "Url", StringComparison.OrdinalIgnoreCase) &&
                parameter.ParameterType.IsAssignableFrom(typeof(UrlHelper)))
            {
                return _componentContext.GetService<IUrlHelper>();
            }

            var getter = _getters.GetOrAdd(parameter.Name.ToLowerInvariant(), n =>
                CallSite<Func<CallSite, object, dynamic>>.Create(
                Binder.GetMember(
                CSharpBinderFlags.None, n, null, new[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) })));

            var result = getter.Target(getter, displayContext.Value);

            if (result == null)
                return null;

            //var converter = _converters.GetOrAdd(parameter.ParameterType, CompileConverter);
            //var argument = converter.Invoke(result);
            //return argument;

            return Convert.ChangeType(result, parameter.ParameterType);
        }


        static readonly ConcurrentDictionary<string, CallSite<Func<CallSite, object, dynamic>>> _getters =
            new ConcurrentDictionary<string, CallSite<Func<CallSite, object, dynamic>>>();

        static readonly ConcurrentDictionary<Type, Func<dynamic, object>> _converters =
            new ConcurrentDictionary<Type, Func<dynamic, object>>();

        static Func<dynamic, object> CompileConverter(Type targetType)
        {
            var valueParameter = Expression.Parameter(typeof(object), "value");

            return Expression.Lambda<Func<object, object>>(
                Expression.Convert(
                    DynamicExpression.Dynamic(
                        Binder.Convert(CSharpBinderFlags.ConvertExplicit, targetType, null),
                                targetType,
                                valueParameter
                        ),
                    typeof(object)),
                valueParameter).Compile();
        }

        private static IHtmlHelper MakeHtmlHelper(ViewContext viewContext, ViewDataDictionary viewData)
        {
            var newHelper = viewContext.HttpContext.RequestServices.GetRequiredService<IHtmlHelper>();

            var contextable = newHelper as ICanHasViewContext;
            if (contextable != null)
            {
                var newViewContext = new ViewContext(viewContext, viewContext.View, viewData, viewContext.Writer);
                contextable.Contextualize(newViewContext);
            }

            return newHelper;
        }
    }
}