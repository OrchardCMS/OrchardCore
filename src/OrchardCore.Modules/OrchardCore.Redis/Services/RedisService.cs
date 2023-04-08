using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using OrchardCore.Modules;
using StackExchange.Redis;

namespace OrchardCore.Redis.Services
{
    public class RedisService : ModularTenantEvents, IRedisService
    {
        private readonly IRedisConnectionFactory _factory;
        private readonly RedisOptions _options;

        private IDatabase _database;

        public RedisService(IRedisConnectionFactory factory, IOptions<RedisOptions> options)
        {
            _factory = factory;
            _options = options.Value;
        }

        public IConnectionMultiplexer Connection { get; private set; }

        public IDatabase Database => _database ??= Connection?.GetDatabase();

        public string InstancePrefix => _options.InstancePrefix;

        public async Task ConnectAsync() => Connection ??= await _factory.CreateAsync(_options.ConfigurationOptions);

        public override Task ActivatingAsync() => ConnectAsync();
    }
}
