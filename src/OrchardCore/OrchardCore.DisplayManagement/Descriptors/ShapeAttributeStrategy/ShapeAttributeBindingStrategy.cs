using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.DisplayManagement.Implementation;
using OrchardCore.Environment.Extensions;

namespace OrchardCore.DisplayManagement.Descriptors.ShapeAttributeStrategy
{
    public class ShapeAttributeBindingStrategy : IShapeTableHarvester
    {
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
        private static Func<DisplayContext, Task<IHtmlContent>> CreateDelegate(ShapeAttributeOccurrence attributeOccurrence)
        {
            var type = attributeOccurrence.ServiceType;
            var methodInfo = attributeOccurrence.MethodInfo;
            var parameters = methodInfo.GetParameters();

            // Create an array of lambdas that will generate the value of each parameter. This prevents from having to test each name and type on every invocation.
            var argumentBuilders = parameters.Select(parameter => BindParameter(parameter)).ToArray();

            var methodWrapper = CreateMethodWrapper(type, methodInfo);

            Func<object, DisplayContext, Task<IHtmlContent>> action;

            if (methodInfo.ReturnType == typeof(Task<IHtmlContent>))
            {
                action = (s, d) =>
                {
                    var arguments = new object[argumentBuilders.Length];
                    for (var i = 0; i < arguments.Length; i++)
                    {
                        arguments[i] = argumentBuilders[i](d);
                    }

                    return (Task<IHtmlContent>)methodWrapper(s, arguments);
                };
            }
            else if (attributeOccurrence.MethodInfo.ReturnType == typeof(IHtmlContent))
            {
                action = (s, d) =>
                {
                    var arguments = new object[argumentBuilders.Length];
                    for (var i = 0; i < arguments.Length; i++)
                    {
                        arguments[i] = argumentBuilders[i](d);
                    }

                    return Task.FromResult((IHtmlContent)methodWrapper(s, arguments));
                };
            }
            else if (attributeOccurrence.MethodInfo.ReturnType != typeof(void))
            {
                action = (s, d) =>
                {
                    var arguments = new object[argumentBuilders.Length];
                    for (var i = 0; i < arguments.Length; i++)
                    {
                        arguments[i] = argumentBuilders[i](d);
                    }

                    return Task.FromResult(CoerceHtmlContent(methodWrapper(s, arguments)));
                };
            }
            else
            {
                action = (s, d) => null;
            }

            return context =>
            {
                var serviceInstance = context.ServiceProvider.GetService(attributeOccurrence.ServiceType);
                return action(serviceInstance, context);
            };
        }

        private static Func<object, object[], object> CreateMethodWrapper(Type type, MethodInfo method)
        {
            CreateParamsExpressions(method, out var argsExp, out var paramsExps);

            var targetExp = Expression.Parameter(typeof(object), "target");
            var castTargetExp = Expression.Convert(targetExp, type);


            LambdaExpression lambdaExp;

            if (method.ReturnType != typeof(void))
            {
                var resultExp = Expression.Convert(Expression.Call(castTargetExp, method, paramsExps), typeof(object));
                lambdaExp = Expression.Lambda(resultExp, targetExp, argsExp);
            }
            else
            {
                var constExp = Expression.Constant(null, typeof(object));
                var blockExp = Expression.Block(Expression.Call(castTargetExp, method, paramsExps), constExp);
                lambdaExp = Expression.Lambda(blockExp, targetExp, argsExp);
            }

            var lambda = lambdaExp.Compile();
            return (Func<object, object[], object>)lambda;
        }

        private static void CreateParamsExpressions(MethodBase method, out ParameterExpression argsExp, out Expression[] paramsExps)
        {
            var parameters = method.GetParameters().Select(x => x.ParameterType).ToArray();

            argsExp = Expression.Parameter(typeof(object[]), "args");
            paramsExps = new Expression[parameters.Length];

            for (var i = 0; i < parameters.Length; i++)
            {
                var constExp = Expression.Constant(i, typeof(int));
                var argExp = Expression.ArrayIndex(argsExp, constExp);
                paramsExps[i] = Expression.Convert(argExp, parameters[i]);
            }
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

        private static Func<DisplayContext, object> BindParameter(ParameterInfo parameter)
        {
            if (String.Equals(parameter.Name, "Shape", StringComparison.OrdinalIgnoreCase))
            {
                return d => d.Value;
            }

            if (String.Equals(parameter.Name, "DisplayAsync", StringComparison.OrdinalIgnoreCase))
            {
                return d => d.DisplayHelper;
            }

            if (String.Equals(parameter.Name, "New", StringComparison.OrdinalIgnoreCase))
            {
                return d => d.ServiceProvider.GetService<IShapeFactory>();
            }

            if (String.Equals(parameter.Name, "ShapeFactory", StringComparison.OrdinalIgnoreCase))
            {
                return d => d.ServiceProvider.GetService<IShapeFactory>();
            }

            if (String.Equals(parameter.Name, "Html", StringComparison.OrdinalIgnoreCase))
            {
                return d =>
                {
                    var viewContextAccessor = d.ServiceProvider.GetRequiredService<ViewContextAccessor>();
                    var viewContext = viewContextAccessor.ViewContext;

                    return MakeHtmlHelper(viewContext, viewContext.ViewData);
                };
            }

            if (String.Equals(parameter.Name, "DisplayContext", StringComparison.OrdinalIgnoreCase))
            {
                return d => d;
            }

            if (String.Equals(parameter.Name, "Url", StringComparison.OrdinalIgnoreCase) && typeof(IUrlHelper).IsAssignableFrom(parameter.ParameterType))
            {
                return d =>
                {
                    var viewContextAccessor = d.ServiceProvider.GetRequiredService<ViewContextAccessor>();
                    var viewContext = viewContextAccessor.ViewContext;

                    var urlHelperFactory = d.ServiceProvider.GetService<IUrlHelperFactory>();
                    return urlHelperFactory.GetUrlHelper(viewContext);
                };
            }

            // pre-calculate the default value 
            var defaultValue = parameter.ParameterType.IsValueType ? Activator.CreateInstance(parameter.ParameterType) : null;

            var isDateTimeType =
                parameter.ParameterType == typeof(DateTime) ||
                parameter.ParameterType == typeof(DateTime?) ||
                parameter.ParameterType == typeof(DateTimeOffset) ||
                parameter.ParameterType == typeof(DateTimeOffset?);

            return d =>
            {
                if (!d.Value.Properties.TryGetValue(parameter.Name, out var result) || result == null)
                {
                    return defaultValue;
                }

                if (parameter.ParameterType.IsAssignableFrom(result.GetType()))
                {
                    return result;
                }

                // Specific implementation for DateTimes
                if (result.GetType() == typeof(string) && isDateTimeType)
                {
                    return DateTime.Parse((string)result);
                }

                return Convert.ChangeType(result, parameter.ParameterType);
            };
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
