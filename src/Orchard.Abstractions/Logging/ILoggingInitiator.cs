using Microsoft.Framework.Logging;

namespace Orchard.Abstractions.Logging {
    public interface ILoggingInitiator {
        void Initialize(ILoggerFactory loggerFactory);
    }
}
