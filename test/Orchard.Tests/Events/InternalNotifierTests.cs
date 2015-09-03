using Orchard.Events;
using System.Collections.Generic;
using Xunit;

namespace Orchard.Tests.Events {
    public class InternalNotifierTests
    {
        [Fact]
        public void CallingNotifyWillInvokeMethod() {

            var target = new TestEvent1();
            var notifier = new InternalTelemetrySourceAdapter(new DefaultMethodAdaptor());

            notifier.EnlistTarget(target);

            Assert.Equal(0, target.Counter);

            notifier.WriteTelemetry("ITestEvent_Add", new Dictionary<string, object> {  { "value", 1 } });
            
            Assert.Equal(1, target.Counter);
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
