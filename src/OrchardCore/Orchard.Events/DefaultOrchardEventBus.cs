using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace Orchard.Events
{
    /// <summary>
    /// Registrations are shared across all EventBus instances for a single tenant
    /// </summary>
    public interface IEventBusState
    {
        ConcurrentDictionary<string, ConcurrentBag<Func<IServiceProvider, IDictionary<string, object>, Task>>> Subscribers { get; }

        void Add(string message, Func<IServiceProvider, IDictionary<string, object>, Task> action);
    }

    public class EventBusState : IEventBusState
    {
        public ConcurrentDictionary<string, ConcurrentBag<Func<IServiceProvider, IDictionary<string, object>, Task>>> Subscribers { get; private set; }

        public EventBusState()
        {
            Subscribers = new ConcurrentDictionary<string, ConcurrentBag<Func<IServiceProvider, IDictionary<string, object>, Task>>>();
        }

        public void Add(string message, Func<IServiceProvider, IDictionary<string, object>, Task> action)
        {
            var messageSubscribers = Subscribers.GetOrAdd(message, m => new ConcurrentBag<Func<IServiceProvider, IDictionary<string, object>, Task>>());
            messageSubscribers.Add(action);
        }
    }

    public class DefaultOrchardEventBus : IEventBus
    {
        private readonly IEventBusState _state;
        private readonly IServiceProvider _serviceProvider;

        public DefaultOrchardEventBus(
            IEventBusState state,
            IServiceProvider serviceProvider)
        {
            _state = state;
            _serviceProvider = serviceProvider;
        }

        public async Task NotifyAsync(string message, IDictionary<string, object> arguments)
        {
            var messageSubscribers = _state.Subscribers.GetOrAdd(message, m => new ConcurrentBag<Func<IServiceProvider, IDictionary<string, object>, Task>>());
            foreach (var subscriber in messageSubscribers)
            {
                await subscriber(_serviceProvider, arguments);
            }
        }

        public void Subscribe(string message, Func<IServiceProvider, IDictionary<string, object>, Task> action)
        {
            _state.Add(message, action);
        }

        public static Task Invoke(IServiceProvider serviceProvider, IDictionary<string, object> arguments, MethodInfo methodInfo, Type handlerClass)
        {
            var service = serviceProvider.GetService(handlerClass);
            var methodParameters = methodInfo.GetParameters();
            var parameters = new object[methodParameters.Length];
            for (var i = 0; i < methodParameters.Length; i++)
            {
                var parameterName = methodParameters[i].Name;

                // If the requested parameter is not present in the original call, just set null
                object value = null;
                if (!arguments.TryGetValue(parameterName, out value))
                {
                    var parameterType = methodParameters[i].ParameterType;
                    if (parameterType.GetTypeInfo().IsValueType)
                    {
                        value = Activator.CreateInstance(parameterType);
                    }

                    value = null;
                }

                parameters[i] = value;
            }

            var result = methodInfo.Invoke(service, parameters) as Task;
            if (result != null)
            {
                return result;
            }

            return Task.CompletedTask;
        }

        public static INotifyProxy CreateProxy(Type interfaceType)
        {
            var genericNotifyProxyType = typeof(NotifyProxy<>).MakeGenericType(interfaceType);
            var genericDispatchProxyCreateMethod = typeof(DispatchProxy).GetMethod("Create").MakeGenericMethod(interfaceType, genericNotifyProxyType);
            var proxyType = genericDispatchProxyCreateMethod.Invoke(null, null) as INotifyProxy;

            return proxyType;
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

        public IEventBus EventBus { get; set; }

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
