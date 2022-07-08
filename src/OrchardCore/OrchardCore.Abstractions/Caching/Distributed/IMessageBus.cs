using System;
using System.Threading.Tasks;

namespace OrchardCore.Caching.Distributed
{
    public interface IMessageBus
    {
        Task SubscribeAsync(string channel, Action<string, string> handler);
        Task PublishAsync(string channel, string message);
    }
}
