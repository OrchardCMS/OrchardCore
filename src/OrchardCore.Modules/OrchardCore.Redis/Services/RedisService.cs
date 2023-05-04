using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using OrchardCore.Modules;
using StackExchange.Redis;

namespace OrchardCore.Redis.Services
{
    public class RedisService : ModularTenantEvents, IRedisService
    {
        private readonly IRedisDatabaseFactory _factory;
        private readonly RedisOptions _options;

        public RedisService(IRedisDatabaseFactory factory, IOptions<RedisOptions> options)
        {
            _factory = factory;
            _options = options.Value;
        }

        public IConnectionMultiplexer Connection => Database?.Multiplexer;

        public string InstancePrefix => _options.InstancePrefix;

        public IDatabase Database { get; private set; }

        public override Task ActivatingAsync() => ConnectAsync();

        public async Task ConnectAsync() => Database ??= await _factory.CreateAsync(_options);
    }
}
