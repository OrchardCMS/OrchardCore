using Microsoft.Extensions.Logging;
using Orchard.Events;

namespace Orchard.Demo.TestEvents
{
    public interface ITestEvent : IEventHandler
    {
        void Talk(string value);
    }

    public class TestEvent1 : ITestEvent
    {
        private readonly ILogger _logger;

        public TestEvent1(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<TestEvent1>();
        }

        public void Talk(string value)
        {
            _logger.LogInformation("Talk Event ONE! " + value);
        }
    }

    public class TestEvent2 : ITestEvent
    {
        private readonly ILogger _logger;

        public TestEvent2(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<TestEvent2>();
        }

        public void Talk(string value)
        {
            _logger.LogInformation("Talk Event TWO! " + value);
        }
    }
}