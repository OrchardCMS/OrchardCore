using Orchard.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Orchard.Events
{
    public interface IEventBus : IDependency
    {
        Task NotifyAsync(string message, IDictionary<string, object> arguments);
        void Subscribe(string message, Func<IServiceProvider, IDictionary<string, object>, Task> action);
        Task NotifyAsync<TEventHandler>(Expression<Action<TEventHandler>> eventNotifier) where TEventHandler : IEventHandler;
    }
}