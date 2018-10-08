using System.Threading.Tasks;
using StackExchange.Redis;

namespace OrchardCore.Distributed.Redis
{
    public interface IRedis
    {
        Task ConnectAsync();
        bool IsConnected { get; }
        IConnectionMultiplexer Connection { get; }
        IDatabase Database { get; }
    }
}
