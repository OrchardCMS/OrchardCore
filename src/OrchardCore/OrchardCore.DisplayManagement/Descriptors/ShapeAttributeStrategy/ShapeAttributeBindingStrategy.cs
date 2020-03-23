using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.CSharp.RuntimeBinder;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.DisplayManagement.Implementation;
using OrchardCore.Environment.Extensions;

namespace OrchardCore.DisplayManagement.Descriptors.ShapeAttributeStrategy
{
    public class ShapeAttributeBindingStrategy : IShapeTableHarvester
    {
        private static readonly ConcurrentDictionary<string, CallSite<Func<CallSite, object, dynamic>>> _getters =
            new ConcurrentDictionary<string, CallSite<Func<CallSite, object, dynamic>>>();

        private static readonly ConcurrentDictionary<MethodInfo, ParameterInfo[]> _parameters =
            new ConcurrentDictionary<MethodInfo, ParameterInfo[]>();

        private readonly ITypeFeatureProvider _typeFeatureProvider;
        private readonly IEnumerable<IShapeAttributeProvider> _shapeProviders;

        public ShapeAttributeBindingStrategy(
            ITypeFeatureProvider typeFeatureProvider,
            IEnumerable<IShapeAttributeProvider> shapeProviders)
        {
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
                        CreateDelegate(occurrence));
            }
        }

        [DebuggerStepThrough]
        private Func<DisplayContext, Task<IHtmlContent>> CreateDelegate(
            ShapeAttributeOccurrence attributeOccurrence)
        {
            return context =>
            {
                var serviceInstance = context.ServiceProvider.GetService(attributeOccurrence.ServiceType);
                // oversimplification for the sake of evolving
                return PerformInvokeAsync(context, attributeOccurrence.MethodInfo, serviceInstance);
            };
        }

        private static Task<IHtmlContent> PerformInvokeAsync(DisplayContext displayContext, MethodInfo methodInfo, object serviceInstance)
        {
            var parameters = _parameters.GetOrAdd(methodInfo, m => m.GetParameters());
            var arguments = parameters.Select(parameter => BindParameter(displayContext, parameter));

            // Resolve the service the method is declared on
            var returnValue = methodInfo.Invoke(serviceInstance, arguments.ToArray());

            if (methodInfo.ReturnType == typeof(Task<IHtmlContent>))
            {
                return (Task<IHtmlContent>)returnValue;
            }
            else if (methodInfo.ReturnType == typeof(IHtmlContent))
            {
                return Task.FromResult((IHtmlContent)returnValue);
            }
            else if (methodInfo.ReturnType != typeof(void))
            {
                return Task.FromResult(CoerceHtmlContent(returnValue));
            }

            return Task.FromResult<IHtmlContent>(null);
        }

        private static IHtmlContent CoerceHtmlContent(object invoke)
        {
            if (invoke == null)
            {
                return HtmlString.Empty;
            }

            if (invoke is IHtmlContent htmlContent)
            {
                return htmlContent;
            }

            return new HtmlString(invoke.ToString());
        }

        private static object BindParameter(DisplayContext displayContext, ParameterInfo parameter)
        {
            if (String.Equals(parameter.Name, "Shape", StringComparison.OrdinalIgnoreCase))
            {
                return displayContext.Value;
            }

            if (String.Equals(parameter.Name, "DisplayAsync", StringComparison.OrdinalIgnoreCase))
            {
                return displayContext.DisplayAsync;
            }

            if (String.Equals(parameter.Name, "New", StringComparison.OrdinalIgnoreCase))
            {
                return displayContext.ServiceProvider.GetService<IShapeFactory>();
            }

            if (String.Equals(parameter.Name, "Html", StringComparison.OrdinalIgnoreCase))
            {
                var viewContextAccessor = displayContext.ServiceProvider.GetRequiredService<ViewContextAccessor>();
                var viewContext = viewContextAccessor.ViewContext;

                return MakeHtmlHelper(viewContext, viewContext.ViewData);
            }

            if (String.Equals(parameter.Name, "DisplayContext", StringComparison.OrdinalIgnoreCase))
            {
                return displayContext;
            }

            if (String.Equals(parameter.Name, "Url", StringComparison.OrdinalIgnoreCase) && typeof(IUrlHelper).IsAssignableFrom(parameter.ParameterType))
            {
                var viewContextAccessor = displayContext.ServiceProvider.GetRequiredService<ViewContextAccessor>();
                var viewContext = viewContextAccessor.ViewContext;

                var urlHelperFactory = displayContext.ServiceProvider.GetService<IUrlHelperFactory>();
                return urlHelperFactory.GetUrlHelper(viewContext);
            }

            if (String.Equals(parameter.Name, "Output", StringComparison.OrdinalIgnoreCase) && parameter.ParameterType == typeof(TextWriter))
            {
                throw new InvalidOperationException("Output is no more a valid Shape method parameter. Return an IHtmlContent instead.");
            }

            if (String.Equals(parameter.Name, "Output", StringComparison.OrdinalIgnoreCase) && parameter.ParameterType == typeof(Action<object>))
            {
                throw new InvalidOperationException("Output is no more a valid Shape method parameter. Return an IHtmlContent instead.");
            }

            var getter = _getters.GetOrAdd(parameter.Name, n =>
                CallSite<Func<CallSite, object, dynamic>>.Create(
                Microsoft.CSharp.RuntimeBinder.Binder.GetMember(
                CSharpBinderFlags.None, n, null, new[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) })));

            object result = getter.Target(getter, displayContext.Value);

            if (result == null)
            {
                return null;
            }

            if (parameter.ParameterType.IsAssignableFrom(result.GetType()))
            {
                return result;
            }

            // Specific implementation for DateTimes
            if (result.GetType() == typeof(string) && (parameter.ParameterType == typeof(DateTime) || parameter.ParameterType == typeof(DateTime?)))
            {
                return DateTime.Parse((string)result);
            }

            return Convert.ChangeType(result, parameter.ParameterType);
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
