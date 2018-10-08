using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Primitives;
using OrchardCore.Environment.Cache;
using OrchardCore.Modules;

namespace OrchardCore.Distributed
{
    /// <summary>
    /// This component is a singleton and holds all the existing signal token for a given tenant.
    /// It becomes a distributed signal if at least one message bus implementation is registered.
    /// </summary>
    public class DistributedSignal : Signal, ISignal, IModularTenantEvents
    {
        private readonly IMessageBus _messageBus;

        public DistributedSignal(IEnumerable<IMessageBus> _messageBuses)
        {
            _messageBus = _messageBuses.LastOrDefault();
        }

        IChangeToken ISignal.GetToken(string key)
        {
            return GetToken(key);
        }

        Task ISignal.SignalTokenAsync(string key)
        {
            SignalToken(key);

            if (_messageBus != null)
            {
                return _messageBus.PublishAsync("Signal", key);
            }

            return Task.CompletedTask;
        }

        public Task ActivatingAsync() { return Task.CompletedTask; }

        public Task ActivatedAsync()
        {
            if (_messageBus != null)
            {
                return _messageBus.SubscribeAsync("Signal", (channel, message) =>
                {
                    SignalToken(message);
                });
            }

            return Task.CompletedTask;
        }

        public Task TerminatingAsync() { return Task.CompletedTask; }
        public Task TerminatedAsync() { return Task.CompletedTask; }
    }
}
