using Microsoft.Framework.Notification;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;

namespace Orchard.Events
{
    public class InternalNotifier : INotifier {
        private readonly NotificationListenerCache _listeners = new NotificationListenerCache();

        private readonly INotifierMethodAdapter _methodAdapter;

        public InternalNotifier(INotifierMethodAdapter methodAdapter) {
            _methodAdapter = methodAdapter;
        }

        public void EnlistTarget(object target) {
            var typeInfo = target.GetType().GetTypeInfo();

            var methodInfos = typeInfo.DeclaredMethods;

            foreach (var methodInfo in methodInfos) {
                var parameterNames = methodInfo.GetParameters().Select(x => x.Name);
                var key = typeInfo.FullName + "_" + methodInfo.Name + "_" + string.Join("_", parameterNames);

                Enlist(key, target, methodInfo);
            }
        }

        private void Enlist(string notificationName, object target, MethodInfo methodInfo) {
            var entries = _listeners.GetOrAdd(
                notificationName,
                _ => new ConcurrentBag<ListenerEntry>());

            entries.Add(new ListenerEntry(target, methodInfo));
        }

        public bool ShouldNotify(string notificationName) {
            return _listeners.ContainsKey(notificationName);
        }

        public void Notify(string notificationName, object parameters) {
            if (parameters == null) {
                return;
            }

            ConcurrentBag<ListenerEntry> entries;
            if (_listeners.TryGetValue(notificationName, out entries)) {
                foreach (var entry in entries) {
                    var succeeded = false;
                    foreach (var adapter in entry.Adapters) {
                        if (adapter(entry.Target, parameters)) {
                            succeeded = true;
                            break;
                        }
                    }

                    if (!succeeded) {
                        // creates object
                        var newAdapter = _methodAdapter.Adapt(entry.MethodInfo, parameters.GetType());
                        // sends values
                        succeeded = newAdapter(entry.Target, parameters);
                        
                        Debug.Assert(succeeded);

                        entry.Adapters.Add(newAdapter);
                    }
                }
            }
        }

        private class NotificationListenerCache : ConcurrentDictionary<string, ConcurrentBag<ListenerEntry>> {
            public NotificationListenerCache()
                : base(StringComparer.Ordinal) {
            }
        }

        private class ListenerEntry {
            public ListenerEntry(object target, MethodInfo methodInfo) {
                Target = target;
                MethodInfo = methodInfo;

                Adapters = new ConcurrentBag<Func<object, object, bool>>();
            }

            public ConcurrentBag<Func<object, object, bool>> Adapters { get; }

            public MethodInfo MethodInfo { get; }

            public object Target { get; }
        }
    }
}
