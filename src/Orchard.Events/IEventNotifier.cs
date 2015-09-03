using System;
using System.Linq.Expressions;

namespace Orchard.Events {
    public interface IEventNotifier {
        void Notify<TEventHandler>(Expression<Action<TEventHandler>> eventNotifier) where TEventHandler : IEventHandler;
    }
}
