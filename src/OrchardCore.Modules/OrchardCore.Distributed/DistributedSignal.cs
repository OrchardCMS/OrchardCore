using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Primitives;
using OrchardCore.Distributed.Messaging;
using OrchardCore.Environment.Cache;
using OrchardCore.Modules;

namespace OrchardCore.Distributed
{
    /// <summary>
    /// This component is a singleton and holds all the existing signal token for a tenant.
    /// </summary>
    public class DistributedSignal : ISignal, IModularTenantEvents
    {
        private ConcurrentDictionary<string, Signal.ChangeTokenInfo> _changeTokens = new ConcurrentDictionary<string, Signal.ChangeTokenInfo>();

        private readonly IMessageBus _messageBus;

        public DistributedSignal(IEnumerable<IMessageBus> _messageBuses)
        {
            _messageBus = _messageBuses.LastOrDefault();
        }

        public IChangeToken GetToken(string key)
        {
            return _changeTokens.GetOrAdd(
                key,
                _ =>
                {
                    var cancellationTokenSource = new CancellationTokenSource();
                    var changeToken = new CancellationChangeToken(cancellationTokenSource.Token);
                    return new Signal.ChangeTokenInfo(changeToken, cancellationTokenSource);
                }).ChangeToken;
        }

        public void SignalToken(string key)
        {
            if (_changeTokens.TryRemove(key, out Signal.ChangeTokenInfo changeTokenInfo))
            {
                changeTokenInfo.TokenSource.Cancel();
                _messageBus?.Publish("Signal", key);
            }
        }

        private void InternalSignalToken(string key)
        {
            if (_changeTokens.TryRemove(key, out Signal.ChangeTokenInfo changeTokenInfo))
            {
                changeTokenInfo.TokenSource.Cancel();
            }
        }

        public Task ActivatingAsync() { return Task.CompletedTask; }

        public Task ActivatedAsync()
        {
            if (_messageBus != null)
            {
                _messageBus.Subscribe("Signal", (channel, msg) =>
                {
                    InternalSignalToken(msg);
                });

            }

            return Task.CompletedTask;
        }

        public Task TerminatingAsync() { return Task.CompletedTask; }
        public Task TerminatedAsync() { return Task.CompletedTask; }
    }
}
