using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OrchardCore.Environment.Commands;

namespace OrchardCore.Demo.Commands
{
    public class DemoCommands : DefaultCommandHandler
    {
        private readonly ILogger _logger;

        public DemoCommands(ILogger<DemoCommands> logger,
            IStringLocalizer<DemoCommands> localizer) : base(localizer)
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
