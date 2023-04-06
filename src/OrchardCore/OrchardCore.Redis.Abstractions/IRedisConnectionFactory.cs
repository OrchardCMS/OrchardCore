using System.Threading.Tasks;
using StackExchange.Redis;

namespace OrchardCore.Redis.Services
{
    public interface IRedisConnectionFactory
    {
        Task<(IConnectionMultiplexer Connection, IDatabase Database)> ConnectAsync(ConfigurationOptions options);
    }
}
