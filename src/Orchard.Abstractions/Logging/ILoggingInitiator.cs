using Microsoft.Framework.Logging;

namespace Orchard.Logging {
    public interface ILoggingInitiator {
        void Initialize(ILoggerFactory loggerFactory);
    }
}
