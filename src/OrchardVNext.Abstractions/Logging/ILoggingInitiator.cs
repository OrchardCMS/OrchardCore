using Microsoft.Framework.Logging;

namespace OrchardVNext.Abstractions.Logging {
    public interface ILoggingInitiator {
        void Initialize(ILoggerFactory loggerFactory);
    }
}
