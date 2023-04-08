using System.Threading.Tasks;
using StackExchange.Redis;

namespace OrchardCore.Redis
{
    /// <summary>
    /// Host level factory allowing to share a <see cref="IDatabase"/> across tenants.
    /// </summary>
    public interface IRedisDatabaseFactory
    {
        Task<IDatabase> CreateAsync(ConfigurationOptions options);
    }
}
