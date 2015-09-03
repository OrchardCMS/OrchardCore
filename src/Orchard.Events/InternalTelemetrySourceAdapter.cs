using Microsoft.Framework.TelemetryAdapter;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;
using System.Linq;

namespace Orchard.Events
{
    public class InternalTelemetrySourceAdapter : TelemetrySourceAdapter {
        private readonly ListenerCache _listeners = new ListenerCache();

        private readonly IMethodAdaptor _methodAdapter;
 		 
        public InternalTelemetrySourceAdapter(IMethodAdaptor methodAdapter) { 
            _methodAdapter = methodAdapter;
        }

        public override void EnlistTarget(object target) {
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
        
        public override void WriteTelemetry(string telemetryName, object parameters) {
            if (parameters == null) {
                return;
            }

            ConcurrentBag<ListenerEntry> entries;
            if (_listeners.TryGetValue(telemetryName, out entries)) {
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
                        var newAdapter = _methodAdapter.Adapt(entry.MethodInfo, parameters);
                        // sends values
                        succeeded = newAdapter(entry.Target, parameters);

                        Debug.Assert(succeeded);

                        entry.Adapters.Add(newAdapter);
                    }
                }
            }
        }

        public override bool IsEnabled(string telemetryName) {
            return _listeners.ContainsKey(telemetryName);
        }

        private class ListenerCache : ConcurrentDictionary<string, ConcurrentBag<ListenerEntry>> {
            public ListenerCache()
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
