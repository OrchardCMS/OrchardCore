using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Reflection;
using System.Linq.Expressions;
using System.Linq;

namespace Orchard.Events
{
    public class DefaultOrchardEventBus : IEventBus
    {
        private ConcurrentDictionary<string, ConcurrentBag<Func<IDictionary<string, object>, Task>>> _subscribers = new ConcurrentDictionary<string, ConcurrentBag<Func<IDictionary<string, object>, Task>>>();

        public async Task NotifyAsync(string message, IDictionary<string, object> arguments)
        {
            var messageSubscribers = _subscribers.GetOrAdd(message, m => new ConcurrentBag<Func<IDictionary<string, object>, Task>>());
            foreach (var subscriber in messageSubscribers)
            {
                await subscriber(arguments);
            }
        }

        public void Subscribe(string message, Func<IDictionary<string, object>, Task> action)
        {
            var messageSubscribers = _subscribers.GetOrAdd(message, m => new ConcurrentBag<Func<IDictionary<string, object>, Task>>());
            messageSubscribers.Add(action);
        }

        public async Task NotifyAsync<TEventHandler>(Expression<Action<TEventHandler>> eventHandler) where TEventHandler : IEventHandler
        {
            var expression = eventHandler.Body as MethodCallExpression;

            if(expression == null)
            {
                throw new ArgumentException("Only method calls are allowed in NotifyAsync");
            }

            var interfaceName = expression.Method.DeclaringType.Name;
            var methodName = expression.Method.Name;
            var messageName = $"{interfaceName}.{methodName}";

            var data = expression.Method
                .GetParameters()
                .Select((parameter, index) => new {
                    parameter.Name,
                    Value = GetValue(parameter, expression.Arguments[index]) })
                .ToDictionary(kv => kv.Name, kv => kv.Value);

            await NotifyAsync(messageName, data);
        }

        public static Task Invoke(IDictionary<string, object> arguments, IServiceProvider serviceProvider, MethodInfo methodInfo, Type eventHandler)
        {
            var service = serviceProvider.GetService(eventHandler);
            var parameters = new object[arguments.Count];
            var methodParameters = methodInfo.GetParameters();
            for (var i=0; i<methodParameters.Length; i++)
            {
                var parameterName = methodParameters[i].Name;
                parameters[i] = arguments[parameterName];
            }

            var result = methodInfo.Invoke(service, parameters) as Task;
            if(result != null)
            {
                return result;
            }

            return Task.FromResult(0);
        }

        public static INotifyProxy CreateProxy(Type interfaceType) 
        {
            var genericNotifyProxyType = typeof(NotifyProxy<>).MakeGenericType(interfaceType);
            var genericDispatchProxyCreateMethod = typeof(DispatchProxy).GetMethod("Create").MakeGenericMethod(interfaceType, genericNotifyProxyType);
            var proxyType = genericDispatchProxyCreateMethod.Invoke(null, null) as INotifyProxy;

            return proxyType;
        }

        private object GetValue(ParameterInfo parameterInfo, Expression member)
        {
            var objectMember = Expression.Convert(member, parameterInfo.ParameterType);

            var getterLambda = Expression.Lambda<Func<object>>(objectMember);

            var getter = getterLambda.Compile();

            return getter();
        }
    }

    public interface INotifyProxy
    {
        IEventBus EventBus { get; set; }
    }

    public class NotifyProxy<T> : DispatchProxy, INotifyProxy
    {
        public NotifyProxy()
        {

        }

        public IEventBus EventBus
        {
            get; set;
        }


        protected override object Invoke(MethodInfo targetMethod, object[] args)
        {
            var message = typeof(T).Name + "." + targetMethod.Name;

            var parameters = new Dictionary<string, object>();
            var methodParameters = targetMethod.GetParameters();
            for (var i = 0; i < methodParameters.Length; i++)
            {
                var parameterName = methodParameters[i].Name;
                parameters[parameterName] = args[i];
            }

            Task.WaitAll(EventBus.NotifyAsync(message, parameters));

            return null;
        }
    }
}
