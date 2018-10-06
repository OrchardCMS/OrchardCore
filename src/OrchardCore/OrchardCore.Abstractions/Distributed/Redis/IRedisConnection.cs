using System.Threading.Tasks;
using StackExchange.Redis;

namespace OrchardCore.Distributed.Redis
{
    public interface IRedisConnection
    {
        Task<IDatabase> GetDatabaseAsync();
    }
}
