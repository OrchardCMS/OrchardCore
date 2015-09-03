using Orchard.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orchard.Demo.TestEvents
{
    public interface ITestEvent : IEventHandler {
        void Talk(string value);
    }

    public class TestEvent1 : ITestEvent {
        public void Talk(string value) {
            Console.WriteLine("Talk Event ONE! " + value);
        }
    }

    public class TestEvent2 : ITestEvent {
        public void Talk(string value) {
            Console.WriteLine("Talk Event TWO! " + value);
        }
    }
}
