using Microsoft.Framework.Logging;
using Orchard.Abstractions.Logging;

namespace Orchard.Logging {
    public class LoggingInitiator : ILoggingInitiator {
        public void Initialize(ILoggerFactory loggerFactory) {
            loggerFactory.AddConsole(LogLevel.Debug);
        }
    }
}