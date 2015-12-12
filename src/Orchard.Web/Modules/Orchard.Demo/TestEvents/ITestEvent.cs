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

        public TestEvent1(ILogger<TestEvent1> logger)
        {
            _logger = logger;
        }

        public void Talk(string value)
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Talk Event ONE! " + value);
            }
        }
    }

    public class TestEvent2 : ITestEvent
    {
        private readonly ILogger _logger;

        public TestEvent2(ILogger<TestEvent2> logger)
        {
            _logger = logger;
        }

        public void Talk(string value)
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Talk Event TWO! " + value);
            }
        }
    }
}