using System.Threading.Tasks;
using StackExchange.Redis;

namespace OrchardCore.Redis
{
    public interface IRedisService
    {
        Task ConnectAsync();
        IConnectionMultiplexer Connection { get; }
        IDatabase Database { get; }
    }
}
