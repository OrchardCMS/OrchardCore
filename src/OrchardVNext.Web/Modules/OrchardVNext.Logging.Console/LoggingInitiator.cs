using Microsoft.Framework.Logging;
using OrchardVNext.Abstractions.Logging;

namespace OrchardVNext.Logging {
    public class LoggingInitiator : ILoggingInitiator {
        public void Initialize(ILoggerFactory loggerFactory) {
            loggerFactory.AddConsole(LogLevel.Debug);
        }
    }
}