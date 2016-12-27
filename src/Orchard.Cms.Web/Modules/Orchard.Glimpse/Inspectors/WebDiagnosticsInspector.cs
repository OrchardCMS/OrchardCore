using Glimpse;
using Glimpse.Agent;
using Glimpse.Agent.Internal.Inspectors;
using Glimpse.Internal;
using Microsoft.Extensions.Logging;

namespace Orchard.Glimpse.Inspectors
{
    public partial class OrchardWebDiagnosticsInspector
    {
        private readonly IAgentBroker _broker;
        private readonly IContextData<MessageContext> _contextData;
        private readonly IExceptionProcessor _exceptionProcessor;
        private readonly ILogger _logger;

        public OrchardWebDiagnosticsInspector(IExceptionProcessor exceptionProcessor, IAgentBroker broker, IContextData<MessageContext> contextData, ILoggerFactory loggerFactory)
        {
            _exceptionProcessor = exceptionProcessor;
            _broker = broker;
            _contextData = contextData;
            _logger = loggerFactory.CreateLogger<OrchardWebDiagnosticsInspector>();
        }
    }
}
