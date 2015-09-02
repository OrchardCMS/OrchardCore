using Orchard.Events;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;

namespace Orchard.Tests.Events
{
    public class InternalNotifierTests
    {
        [Fact]
        public void CallingNotifyWillInvokeMethod() {

            var target = new TestEvent1();
            var notifier = new Notifier(new []{ target });

            notifier.Subscribe(new EventObserver(target));

            Assert.Equal(0, target.Counter);

            notifier.Notify("ITestEvent_Add", new Dictionary<string, object> {  { "value", 1 } });
            
            Assert.Equal(1, target.Counter);
        }
    }

    public class Notifier : IObservable<KeyValuePair<string, IDictionary<string, object>>> {
        private readonly IList<IObserver<KeyValuePair<string, IDictionary<string, object>>>> _observers
            = new List<IObserver<KeyValuePair<string, IDictionary<string, object>>>>();

        private readonly IEnumerable<IEventHandler> _eventHandlers;

        public Notifier(IEnumerable<IEventHandler> eventHandlers) {
            _eventHandlers = eventHandlers;
        }

        public IDisposable Subscribe(IObserver<KeyValuePair<string, IDictionary<string, object>>> observer) {
            _observers.Add(observer);

            return null;
        }

        public void Notify(string notificationName, IDictionary<string, object> parameters) {
            foreach(var observer in _observers) {
                try {

                    observer.OnNext(
                        new KeyValuePair<string, IDictionary<string, object>>(notificationName, parameters));
                }
                catch (Exception ex) {
                    observer.OnError(ex);
                }
                finally {
                    observer.OnCompleted();
                }
            }
        }
    }


    public class EventObserver : IObserver<KeyValuePair<string, IDictionary<string, object>>> {
        private readonly NotificationListenerCache _listeners = new NotificationListenerCache();

        public EventObserver(IEventHandler target) {
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

        public void OnCompleted() {
        }

        public void OnError(Exception error) {
        }

        public void OnNext(KeyValuePair<string, IDictionary<string, object>> value) {
            var notificationName = value.Key;
            ConcurrentBag<ListenerEntry> entries;
            if (_listeners.TryGetValue(notificationName, out entries)) {
                foreach (var entry in entries) {
                    // Do Something!
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
            }

            public MethodInfo MethodInfo { get; }

            public object Target { get; }
        }
    }

    public interface ITestEvent : IEventHandler {
        int Counter { get; set; }
        void Add(int value);
    }

    public class TestEvent1 : ITestEvent {
        public int Counter { get; set; }

        public void Add(int value) {
            Counter = Counter + 1;
        }
    }

    public class TestEvent2 : ITestEvent {
        public int Counter { get; set; }

        public void Add(int value) {
            Counter = Counter + 1;
        }
    }
}
