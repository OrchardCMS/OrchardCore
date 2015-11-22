using Microsoft.Extensions.Logging;
using Orchard.Environment.Commands;

namespace Orchard.Demo.Commands
{
    public class DemoCommands : DefaultOrchardCommandHandler
    {
        private readonly ILogger _logger;

        public DemoCommands(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<DemoCommands>();
        }

        [CommandName("demo helloworld")]
        [CommandHelp("demo helloworld says hi!")]
        public void HelloWorld()
        {
            _logger.LogInformation("Hi there from Hello World!");
        }
    }
}