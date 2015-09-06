using System;
using System.Linq.Expressions;

namespace Orchard.Events {
    public interface IEventNotifier {
        object Notify<TEventHandler>(Expression<Action<TEventHandler>> eventNotifier) where TEventHandler : IEventHandler;
    }
}
