using System.Threading.Tasks;
using StackExchange.Redis;

namespace OrchardCore.Redis
{
    public interface IRedisDatabaseFactory
    {
        Task<IDatabase> CreateAsync(ConfigurationOptions options);
    }
}
