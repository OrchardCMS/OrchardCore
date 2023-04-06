using System.Threading.Tasks;
using StackExchange.Redis;

namespace OrchardCore.Redis.Services
{
    public interface IRedisConnectionFactory
    {
        Task<IConnectionMultiplexer> CreateAsync(ConfigurationOptions options);
    }
}
