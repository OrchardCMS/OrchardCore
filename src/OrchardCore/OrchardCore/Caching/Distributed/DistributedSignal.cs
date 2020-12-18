using System.Threading.Tasks;
using Microsoft.Extensions.Primitives;
using OrchardCore.Environment.Cache;

namespace OrchardCore.Caching.Distributed
{
    /// <summary>
    /// Distributed implementation of <see cref="Signal"/> using an <see cref="IMessageBus"/>.
    /// </summary>
    public class DistributedSignal : Signal, ISignal
    {
        private readonly IMessageBus _messageBus;

        public DistributedSignal(IMessageBus messageBus) => _messageBus = messageBus;

        IChangeToken ISignal.GetToken(string key) => GetToken(key);

        Task ISignal.SignalTokenAsync(string key)
        {
            SignalToken(key);
            return _messageBus.PublishAsync("Signal", key);
        }

        public override Task ActivatedAsync() => _messageBus.SubscribeAsync("Signal", (channel, message) => SignalToken(message));
    }
}
