using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Orchard.Events {
    public class DefaultOrchardEventNotifier : IEventNotifier {
        private readonly IEventBus _eventBus;
        private static readonly ConcurrentDictionary<Type, MethodInfo> _enumerableOfTypeTDictionary = new ConcurrentDictionary<Type, MethodInfo>();

        public DefaultOrchardEventNotifier(IEventBus eventBus) {
            _eventBus = eventBus;
        }

        public object Notify<TEventHandler>(Expression<Action<TEventHandler>> eventNotifier) where TEventHandler : IEventHandler {
            var expression = GetExpression(eventNotifier);
            
            var interfaceName = expression.Method.DeclaringType.Name;
            var methodName = expression.Method.Name;
            
            var data = expression.Method.GetParameters()
                .Select((parameter, index) => new { parameter.Name, Value = GetValue(parameter, expression.Arguments[index]) })
                .ToDictionary(kv => kv.Name, kv => kv.Value);

            var results = _eventBus.Notify(interfaceName + "." + methodName, data);

            return Adjust(results, expression.Method.ReturnType);
        }

        private MethodCallExpression GetExpression<TEventHandler>(Expression<Action<TEventHandler>> eventHandler) where TEventHandler : IEventHandler {
            return (MethodCallExpression)eventHandler.Body;
        }

        public static object Adjust(IEnumerable results, Type returnType) {
            if (returnType == typeof(void) ||
                results == null ||
                results.GetType() == returnType) {
                return results;
            }

            // acquire method:
            // static IEnumerable<T> IEnumerable.OfType<T>(this IEnumerable source)
            // where T is from returnType's IEnumerable<T>
            var enumerableOfTypeT = _enumerableOfTypeTDictionary
                .GetOrAdd(
                    returnType, 
                    type => typeof(Enumerable).GetGenericMethod(
                        "OfType", 
                        type.GetGenericArguments(), 
                        new[] { typeof(IEnumerable) }, typeof(IEnumerable<>)));
            return enumerableOfTypeT.Invoke(null, new[] { results });
        }

        private object GetValue(ParameterInfo parameterInfo, Expression member) {
            var objectMember = Expression.Convert(member, parameterInfo.ParameterType);

            var getterLambda = Expression.Lambda<Func<object>>(objectMember);

            var getter = getterLambda.Compile();

            return getter();
        }
    }

    public static class Extensions {
        public static MethodInfo GetGenericMethod(this Type t, string name, Type[] genericArgTypes, Type[] argTypes, Type returnType) {
            return (from m in t.GetMethods(BindingFlags.Public | BindingFlags.Static)
                    where m.Name == name &&
                    m.GetGenericArguments().Length == genericArgTypes.Length &&
                    m.GetParameters().Select(pi => pi.ParameterType).SequenceEqual(argTypes) &&
                    (m.ReturnType.GetTypeInfo().IsGenericType && !m.ReturnType.GetTypeInfo().IsGenericTypeDefinition ? returnType.GetGenericTypeDefinition() : m.ReturnType) == returnType
                    select m).Single().MakeGenericMethod(genericArgTypes);

        }
    }
}
