using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Primitives;
using OrchardCore.Environment.Cache;
using OrchardCore.Modules;

namespace OrchardCore.Distributed
{
    /// <summary>
    /// This component is a singleton and holds all the existing signal token for a tenant.
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

        async Task ISignal.SignalTokenAsync(string key)
        {
            await SignalTokenAsync(key);

            if (_messageBus != null)
            {
                await _messageBus.PublishAsync("Signal", key);
            }
        }

        public Task ActivatingAsync() { return Task.CompletedTask; }

        public async Task ActivatedAsync()
        {
            if (_messageBus != null)
            {
                await _messageBus.SubscribeAsync("Signal", (channel, message) =>
                {
                    SignalToken(message);
                });
            }
        }

        public Task TerminatingAsync() { return Task.CompletedTask; }
        public Task TerminatedAsync() { return Task.CompletedTask; }
    }
}
