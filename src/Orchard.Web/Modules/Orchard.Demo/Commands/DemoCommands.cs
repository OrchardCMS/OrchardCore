using Microsoft.Extensions.Logging;
using Orchard.Environment.Commands;

namespace Orchard.Demo.Commands
{
    public class DemoCommands : DefaultOrchardCommandHandler
    {
        private readonly ILogger _logger;

        public DemoCommands(ILogger<DemoCommands> logger)
        {
            _logger = logger;
        }

        [CommandName("demo helloworld")]
        [CommandHelp("demo helloworld says hi!")]
        public void HelloWorld()
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Hi there from Hello World!");
            }
        }
    }
}