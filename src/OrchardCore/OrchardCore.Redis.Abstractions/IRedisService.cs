using System.Threading.Tasks;
using OrchardCore.Modules;
using StackExchange.Redis;

namespace OrchardCore.Redis
{
    public interface IRedisService : IModularTenantEvents
    {
        Task ConnectAsync();

        IConnectionMultiplexer Connection { get; }

        string InstancePrefix { get; }

        IDatabase Database { get; }
    }
}
