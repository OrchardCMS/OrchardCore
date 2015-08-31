using Microsoft.Framework.Notification;
using System;
using System.Collections.Generic;
using Orchard.Abstractions.Localization;
using System.Linq;
using System.Dynamic;

namespace Orchard.Events {
    public class DefaultOrchardEventBus : IEventBus {
        private readonly IEnumerable<IEventHandler> _eventHandlers;
        private readonly INotifier _notifier;

        public DefaultOrchardEventBus(IEnumerable<IEventHandler> eventHandlers,
            INotifier notifier) {
            _eventHandlers = eventHandlers;
            _notifier = notifier;
            foreach (var eventHandler in eventHandlers) {
                _notifier.EnlistTarget(eventHandler);
            }
            
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public void Notify(string messageName, IDictionary<string, object> eventData) {
            string[] parameters = messageName.Split('.');
            if (parameters.Length != 2) {
                throw new ArgumentException(T("{0} is not formatted correctly", messageName));
            }
            string interfaceName = parameters[0];
            string methodName = parameters[1];
            
            foreach (var eventHandler in _eventHandlers) {
                var key = eventHandler.GetType().FullName + "_" + methodName + "_" + string.Join("_", eventData.Keys);
                
                _notifier.Notify(key, eventData);
            }
        }
    }
}
