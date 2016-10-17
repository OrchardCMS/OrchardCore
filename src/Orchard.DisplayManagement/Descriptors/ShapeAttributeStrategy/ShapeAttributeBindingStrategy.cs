using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.CSharp.RuntimeBinder;
using Microsoft.Extensions.DependencyInjection;
using Orchard.DisplayManagement.Implementation;
using Orchard.Environment.Extensions;

namespace Orchard.DisplayManagement.Descriptors.ShapeAttributeStrategy
{
    public class ShapeAttributeBindingStrategy : IShapeTableProvider
    {
        private readonly ITypeFeatureProvider _typeFeatureProvider;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IServiceProvider _serviceProvider;
        private readonly IEnumerable<IShapeAttributeProvider> _shapeProviders;

        public ShapeAttributeBindingStrategy(
            IServiceProvider serviceProvider,
            IHttpContextAccessor httpContextAccessor,
            ITypeFeatureProvider typeFeatureProvider,
            IEnumerable<IShapeAttributeProvider> shapeProviders)
        {
            _serviceProvider = serviceProvider;
            _httpContextAccessor = httpContextAccessor;
            _typeFeatureProvider = typeFeatureProvider;
            _shapeProviders = shapeProviders;
        }

        public void Discover(ShapeTableBuilder builder)
        {
            var shapeAttributeOccurrences = new List<ShapeAttributeOccurrence>();

            foreach (var shapeProvider in _shapeProviders)
            {
                var serviceType = shapeProvider.GetType();

                foreach (var method in serviceType.GetMethods())
                {
                    var customAttributes = method.GetCustomAttributes(typeof(ShapeAttribute), false).OfType<ShapeAttribute>();
                    foreach (var customAttribute in customAttributes)
                    {
                        shapeAttributeOccurrences.Add(new ShapeAttributeOccurrence(customAttribute, method, serviceType));
                    }
                }
            }

            foreach (var iter in shapeAttributeOccurrences)
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
        private Func<DisplayContext, Task<IHtmlContent>> CreateDelegate(
            ShapeAttributeOccurrence attributeOccurrence,
            ShapeDescriptor descriptor)
        {
            return context =>
            {
                var serviceInstance = _serviceProvider.GetService(attributeOccurrence.ServiceType);
                // oversimplification for the sake of evolving
                return PerformInvokeAsync(context, attributeOccurrence.MethodInfo, serviceInstance);
            };
        }

        private Task<IHtmlContent> PerformInvokeAsync(DisplayContext displayContext, MethodInfo methodInfo, object serviceInstance)
        {
            var parameters = _parameters.GetOrAdd(methodInfo, m => m.GetParameters());
            var arguments = parameters.Select(parameter => BindParameter(displayContext, parameter));

            // Resolve the service the method is declared on
            var returnValue = methodInfo.Invoke(serviceInstance, arguments.ToArray());

            // If the shape returns a value, write it to the stream
            if (methodInfo.ReturnType != typeof(void))
            {
                return Task.FromResult(CoerceHtmlContent(returnValue));
            }

            return Task.FromResult<IHtmlContent>(null);
        }

        private static IHtmlContent CoerceHtmlContent(object invoke)
        {
            var htmlContent = invoke as IHtmlContent;

            if(htmlContent != null)
            {
                return htmlContent;
            }

            return invoke != null ? new HtmlString(invoke.ToString()) : null;
        }

        private object BindParameter(DisplayContext displayContext, ParameterInfo parameter)
        {
            if (String.Equals(parameter.Name, "Shape", StringComparison.OrdinalIgnoreCase))
            {
                return displayContext.Value;
            }

            if (String.Equals(parameter.Name, "Display", StringComparison.OrdinalIgnoreCase))
            {
                return displayContext.Display;
            }

            if (String.Equals(parameter.Name, "New", StringComparison.OrdinalIgnoreCase))
            {
                var httpContext = _httpContextAccessor.HttpContext;
                return httpContext.RequestServices.GetService<IShapeFactory>();
            }

            if (String.Equals(parameter.Name, "Html", StringComparison.OrdinalIgnoreCase))
            {
                return MakeHtmlHelper(displayContext.ViewContext, displayContext.ViewContext.ViewData);
            }

            if (String.Equals(parameter.Name, "Url", StringComparison.OrdinalIgnoreCase) &&
                parameter.ParameterType.IsAssignableFrom(typeof(UrlHelper)))
            {
                var httpContext = _httpContextAccessor.HttpContext;
                var urlHelperFactory = httpContext.RequestServices.GetService<IUrlHelperFactory>();
                return urlHelperFactory.GetUrlHelper(displayContext.ViewContext);
            }

            if (String.Equals(parameter.Name, "Output", StringComparison.OrdinalIgnoreCase) &&
                parameter.ParameterType == typeof(TextWriter))
            {
                throw new InvalidOperationException("Output is no more a valid Shape method parameter. Return an IHtmlContent instead.");
            }

            if (String.Equals(parameter.Name, "Output", StringComparison.OrdinalIgnoreCase) &&
                parameter.ParameterType == typeof(Action<object>))
            {
                throw new InvalidOperationException("Output is no more a valid Shape method parameter. Return an IHtmlContent instead.");
            }

            var getter = _getters.GetOrAdd(parameter.Name, n =>
                CallSite<Func<CallSite, object, dynamic>>.Create(
                Binder.GetMember(
                CSharpBinderFlags.None, n, null, new[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) })));

            object result = getter.Target(getter, displayContext.Value);

            if (result == null)
                return null;

            if(parameter.ParameterType.IsAssignableFrom(result.GetType()))
            {
                return result;
            }

            return Convert.ChangeType(result, parameter.ParameterType);
        }


        static readonly ConcurrentDictionary<string, CallSite<Func<CallSite, object, dynamic>>> _getters =
            new ConcurrentDictionary<string, CallSite<Func<CallSite, object, dynamic>>>();

        static readonly ConcurrentDictionary<MethodInfo, ParameterInfo[]> _parameters =
            new ConcurrentDictionary<MethodInfo, ParameterInfo[]>();

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

            var contextable = newHelper as IViewContextAware;
            if (contextable != null)
            {
                var newViewContext = new ViewContext(viewContext, viewContext.View, viewData, viewContext.Writer);
                contextable.Contextualize(newViewContext);
            }

            return newHelper;
        }
    }
}