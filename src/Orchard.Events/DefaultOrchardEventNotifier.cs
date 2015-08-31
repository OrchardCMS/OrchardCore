using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Orchard.Events {
    public class DefaultOrchardEventNotifier : IEventNotifier {
        private readonly IEventBus _eventBus;

        public DefaultOrchardEventNotifier(IEventBus eventBus) {
            _eventBus = eventBus;
        }

        public void Notify<TEventHandler>(Expression<Action<TEventHandler>> eventNotifier) where TEventHandler : IEventHandler {
            var expression = GetExpression(eventNotifier);
            
            var interfaceName = expression.Method.DeclaringType.Name;
            var methodName = expression.Method.Name;
            
            var data = expression.Method.GetParameters()
                .Select((parameter, index) => new { parameter.Name, Value = GetValue(parameter, expression.Arguments[index]) })
                .ToDictionary(kv => kv.Name, kv => kv.Value);

            var key = interfaceName + "." + methodName;

            _eventBus.Notify(key, data);
        }

        private MethodCallExpression GetExpression<TEventHandler>(Expression<Action<TEventHandler>> eventHandler) where TEventHandler : IEventHandler {
            return (MethodCallExpression)eventHandler.Body;
        }

        private object GetValue(ParameterInfo parameterInfo, Expression member) {
            var objectMember = Expression.Convert(member, parameterInfo.ParameterType);

            var getterLambda = Expression.Lambda<Func<object>>(objectMember);

            var getter = getterLambda.Compile();

            return getter();
        }
    }
}
