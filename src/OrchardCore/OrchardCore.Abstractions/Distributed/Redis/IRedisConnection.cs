using System.Threading.Tasks;
using StackExchange.Redis;

namespace OrchardCore.Distributed
{
    public interface IRedisConnection
    {
        Task<IDatabase> GetDatabaseAsync();
    }
}
