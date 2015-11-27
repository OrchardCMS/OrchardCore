//using Orchard.Events;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using Xunit;

//namespace Orchard.Tests.Events {
//    public class EventTests
//    {
//        [Fact]
//        public void EventsAreCorrectlyDispatchedToEventHandlers() {
//            var stub1 = new StubEventHandler();
//            var stub2 = new StubEventHandler2();
//            var eventBus = new DefaultOrchardEventBus(new IEventHandler[] { stub1, stub2 });

//            Assert.Equal(0, stub1.Count);
//            eventBus.Notify("ITestEventHandler.Increment", new Dictionary<string, object>());
//            Assert.Equal(1, stub1.Count);
//        }

//        [Fact]
//        public void EventParametersAreCorrectlyPassedToEventHandlers() {
//            var stub1 = new StubEventHandler();
//            var stub2 = new StubEventHandler2();
//            var eventBus = new DefaultOrchardEventBus(new IEventHandler[] { stub1, stub2 });

//            Assert.Equal(0, stub1.Result);
//            Dictionary<string, object> arguments = new Dictionary<string, object>();
//            arguments["a"] = 5200;
//            arguments["b"] = 2600;
//            eventBus.Notify("ITestEventHandler.Substract", arguments);
//            Assert.Equal(2600, stub1.Result);
//        }


//        [Fact]
//        public void EventParametersArePassedInCorrectOrderToEventHandlers() {
//            var stub1 = new StubEventHandler();
//            var stub2 = new StubEventHandler2();
//            var eventBus = new DefaultOrchardEventBus(new IEventHandler[] { stub1, stub2 });

//            Assert.Equal(0, stub1.Result);
//            Dictionary<string, object> arguments = new Dictionary<string, object>();
//            arguments["a"] = 2600;
//            arguments["b"] = 5200;
//            eventBus.Notify("ITestEventHandler.Substract", arguments);
//            Assert.Equal(-2600, stub1.Result);
//        }

//        [Fact]
//        public void EventParametersAreCorrectlyPassedToMatchingMethod() {
//            var stub1 = new StubEventHandler();
//            var stub2 = new StubEventHandler2();
//            var eventBus = new DefaultOrchardEventBus(new IEventHandler[] { stub1, stub2 });

//            Assert.Null(stub1.Summary);
//            Dictionary<string, object> arguments = new Dictionary<string, object>();
//            arguments["a"] = "a";
//            arguments["b"] = "b";
//            arguments["c"] = "c";
//            eventBus.Notify("ITestEventHandler.Concat", arguments);
//            Assert.Equal("abc", stub1.Summary);
//        }

//        [Fact]
//        public void EventParametersAreCorrectlyPassedToExactlyMatchingMethod() {
//            var stub1 = new StubEventHandler();
//            var stub2 = new StubEventHandler2();
//            var eventBus = new DefaultOrchardEventBus(new IEventHandler[] { stub1, stub2 });

//            Assert.Equal(0, stub1.Result);
//            Dictionary<string, object> arguments = new Dictionary<string, object>();
//            arguments["a"] = 1000;
//            arguments["b"] = 100;
//            arguments["c"] = 10;
//            eventBus.Notify("ITestEventHandler.Sum", arguments);
//            Assert.Equal(1110, stub1.Result);
//        }

//        [Fact]
//        public void EventParametersAreCorrectlyPassedToBestMatchingMethodAndExtraParametersAreIgnored() {
//            var stub1 = new StubEventHandler();
//            var stub2 = new StubEventHandler2();
//            var eventBus = new DefaultOrchardEventBus(new IEventHandler[] { stub1, stub2 });

//            Assert.Equal(0, stub1.Result);
//            Dictionary<string, object> arguments = new Dictionary<string, object>();
//            arguments["a"] = 1000;
//            arguments["b"] = 100;
//            arguments["c"] = 10;
//            arguments["e"] = 1;
//            eventBus.Notify("ITestEventHandler.Sum", arguments);
//            Assert.Equal(1110, stub1.Result);
//        }

//        [Fact]
//        public void EventParametersAreCorrectlyPassedToBestMatchingMethodAndExtraParametersAreIgnored2() {
//            var stub1 = new StubEventHandler();
//            var stub2 = new StubEventHandler2();
//            var eventBus = new DefaultOrchardEventBus(new IEventHandler[] { stub1, stub2 });

//            Assert.Equal(0, stub1.Result);
//            Dictionary<string, object> arguments = new Dictionary<string, object>();
//            arguments["a"] = 1000;
//            arguments["e"] = 1;
//            eventBus.Notify("ITestEventHandler.Sum", arguments);
//            Assert.Equal(3000, stub1.Result);
//        }

//        [Fact]
//        public void EventParametersAreCorrectlyPassedToExactlyMatchingMethodWhenThereIsOne() {
//            var stub1 = new StubEventHandler();
//            var stub2 = new StubEventHandler2();
//            var eventBus = new DefaultOrchardEventBus(new IEventHandler[] { stub1, stub2 });

//            Assert.Equal(0, stub1.Result);
//            Dictionary<string, object> arguments = new Dictionary<string, object>();
//            arguments["a"] = 1000;
//            arguments["b"] = 100;
//            eventBus.Notify("ITestEventHandler.Sum", arguments);
//            Assert.Equal(2200, stub1.Result);
//        }

//        [Fact]
//        public void EventParametersAreCorrectlyPassedToExactlyMatchingMethodWhenThereIsOne2() {
//            var stub1 = new StubEventHandler();
//            var stub2 = new StubEventHandler2();
//            var eventBus = new DefaultOrchardEventBus(new IEventHandler[] { stub1, stub2 });

//            Assert.Equal(0, stub1.Result);
//            Dictionary<string, object> arguments = new Dictionary<string, object>();
//            arguments["a"] = 1000;
//            eventBus.Notify("ITestEventHandler.Sum", arguments);
//            Assert.Equal(3000, stub1.Result);
//        }

//        [Fact]
//        public void EventHandlerWontBeCalledWhenNoParameterMatchExists() {
//            var stub1 = new StubEventHandler();
//            var stub2 = new StubEventHandler2();
//            var eventBus = new DefaultOrchardEventBus(new IEventHandler[] { stub1, stub2 });

//            Assert.Equal(0, stub1.Result);
//            Dictionary<string, object> arguments = new Dictionary<string, object>();
//            arguments["e"] = 1;
//            eventBus.Notify("ITestEventHandler.Sum", arguments);
//            Assert.Equal(0, stub1.Result);
//        }

//        [Fact]
//        public void EventHandlerWontBeCalledWhenNoParameterMatchExists2() {
//            var stub1 = new StubEventHandler();
//            var stub2 = new StubEventHandler2();
//            var eventBus = new DefaultOrchardEventBus(new IEventHandler[] { stub1, stub2 });

//            Assert.Equal(0, stub1.Result);
//            Dictionary<string, object> arguments = new Dictionary<string, object>();
//            eventBus.Notify("ITestEventHandler.Sum", arguments);
//            Assert.Equal(0, stub1.Result);
//        }

//        [Fact]
//        public void EventBusThrowsIfMessageNameIsNotCorrectlyFormatted() {
//            var stub1 = new StubEventHandler();
//            var stub2 = new StubEventHandler2();
//            var eventBus = new DefaultOrchardEventBus(new IEventHandler[] { stub1, stub2 });

//            Assert.Throws<ArgumentException>(() => eventBus.Notify("StubEventHandlerIncrement", new Dictionary<string, object>()));
//        }

//        [Fact]
//        public void InterceptorCanCoerceResultingCollection() {
//            var data = new object[] { "5", "18", "2" };
//            var adjusted = DefaultOrchardEventNotifier.Adjust(data, typeof(IEnumerable<string>));
//            Assert.IsAssignableFrom<IEnumerable<object>>(data);
//            Assert.IsNotType<IEnumerable<string>>(data);
//            Assert.IsAssignableFrom<IEnumerable<string>>(adjusted);
//        }

//        [Fact]
//        public void EnumerableResultsAreTreatedLikeSelectMany() {
//            var stub1 = new StubEventHandler();
//            var stub2 = new StubEventHandler2();
//            var eventBus = new DefaultOrchardEventBus(new IEventHandler[] { stub1, stub2 });

//            var results = eventBus.Notify("ITestEventHandler.Gather", new Dictionary<string, object> { { "a", 42 }, { "b", "alpha" } }).Cast<string>();
//            Assert.Equal(3, results.Count());
//            Assert.Contains("42", results);
//            Assert.Contains("alpha", results);
//            Assert.Contains("[42,alpha]", results);
//        }

//        [Fact]
//        public void StringResultsAreTreatedLikeSelect() {
//            var stub1 = new StubEventHandler();
//            var stub2 = new StubEventHandler2();
//            var eventBus = new DefaultOrchardEventBus(new IEventHandler[] { stub1, stub2 });

//            var results = eventBus.Notify("ITestEventHandler.GetString", new Dictionary<string, object>()).Cast<string>();
//            Assert.Equal(2, results.Count());
//            Assert.Contains("Foo", results);
//            Assert.Contains("Bar", results);
//        }

//        [Fact]
//        public void NonStringNonEnumerableResultsAreTreatedLikeSelect() {
//            var stub1 = new StubEventHandler();
//            var stub2 = new StubEventHandler2();
//            var eventBus = new DefaultOrchardEventBus(new IEventHandler[] { stub1, stub2 });

//            var results = eventBus.Notify("ITestEventHandler.GetInt", new Dictionary<string, object>()).Cast<int>();
//            Assert.Equal(2, results.Count());
//            Assert.Contains(1, results);
//            Assert.Contains(2, results);
//        }


//        public interface ITestEventHandler : IEventHandler {
//            void Increment();
//            void Sum(int a);
//            void Sum(int a, int b);
//            void Sum(int a, int b, int c);
//            void Substract(int a, int b);
//            void Concat(string a, string b, string c);
//            IEnumerable<string> Gather(int a, string b);
//            string GetString();
//            int GetInt();
//        }

//        public class StubEventHandler : ITestEventHandler {
//            public int Count { get; set; }
//            public int Result { get; set; }
//            public string Summary { get; set; }

//            public void Increment() {
//                Count++;
//            }

//            public void Sum(int a) {
//                Result = 3 * a;
//            }

//            public void Sum(int a, int b) {
//                Result = 2 * (a + b);
//            }

//            public void Sum(int a, int b, int c) {
//                Result = a + b + c;
//            }

//            public void Substract(int a, int b) {
//                Result = a - b;
//            }

//            public void Concat(string a, string b, string c) {
//                Summary = a + b + c;
//            }

//            public IEnumerable<string> Gather(int a, string b) {
//                yield return string.Format("[{0},{1}]", a, b);
//            }

//            public string GetString() {
//                return "Foo";
//            }

//            public int GetInt() {
//                return 1;
//            }
//        }
//        public class StubEventHandler2 : ITestEventHandler {
//            public void Increment() {
//            }

//            public void Sum(int a) {
//            }

//            public void Sum(int a, int b) {
//            }

//            public void Sum(int a, int b, int c) {
//            }

//            public void Substract(int a, int b) {
//            }

//            public void Concat(string a, string b, string c) {
//            }

//            public IEnumerable<string> Gather(int a, string b) {
//                return new[] { a.ToString(), b };
//            }

//            public string GetString() {
//                return "Bar";
//            }

//            public int GetInt() {
//                return 2;
//            }
//        }
//    }
//}