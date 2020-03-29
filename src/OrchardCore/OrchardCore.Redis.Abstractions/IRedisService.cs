using System.Threading.Tasks;
using StackExchange.Redis;

namespace OrchardCore.Redis
{
    public interface IRedisService
    {
        Task ConnectAsync();
        bool IsConnected { get; }
        IConnectionMultiplexer Connection { get; }
        IDatabase Database { get; }
    }
}
