using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Linq;
using System.Reflection;

namespace Orchard.Events
{
    public interface IEventBus
    {
        Task NotifyAsync(string message, IDictionary<string, object> arguments);
        void Subscribe(string message, Func<IServiceProvider, IDictionary<string, object>, Task> action);
    }

    public static class EventBusExtensions
    {
        public static void Notify<TEventHandler>(this IEventBus eventBus, Expression<Action<TEventHandler>> eventHandler) where TEventHandler : IEventHandler
        {
            var expression = eventHandler.Body as MethodCallExpression;

            if (expression == null)
            {
                throw new ArgumentException("Only method calls are allowed in NotifyAsync");
            }

            var interfaceName = expression.Method.DeclaringType.Name;
            var methodName = expression.Method.Name;
            var messageName = $"{interfaceName}.{methodName}";

            var data = expression.Method
                .GetParameters()
                .Select((parameter, index) => new
                {
                    parameter.Name,
                    Value = GetValue(parameter, expression.Arguments[index])
                })
                .ToDictionary(kv => kv.Name, kv => kv.Value);

            eventBus.NotifyAsync(messageName, data).Wait();
        }

        public static Task NotifyAsync<TEventHandler>(this IEventBus eventBus, Expression<Func<TEventHandler, Task>> eventHandler) where TEventHandler : IEventHandler
        {
            var expression = eventHandler.Body as MethodCallExpression;

            if (expression == null)
            {
                throw new ArgumentException("Only method calls are allowed in NotifyAsync");
            }

            var interfaceName = expression.Method.DeclaringType.Name;
            var methodName = expression.Method.Name;
            var messageName = $"{interfaceName}.{methodName}";

            var data = expression.Method
                .GetParameters()
                .Select((parameter, index) => new
                {
                    parameter.Name,
                    Value = GetValue(parameter, expression.Arguments[index])
                })
                .ToDictionary(kv => kv.Name, kv => kv.Value);

            return eventBus.NotifyAsync(messageName, data);
        }

        static private object GetValue(ParameterInfo parameterInfo, Expression member)
        {
            var objectMember = Expression.Convert(member, parameterInfo.ParameterType);

            var getterLambda = Expression.Lambda<Func<object>>(objectMember);

            var getter = getterLambda.Compile();

            return getter();
        }
    }
}