using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using OrchardCore.Modules;
using StackExchange.Redis;

namespace OrchardCore.Redis.Services
{
    public class RedisService : ModularTenantEvents, IRedisService
    {
        private readonly RedisOptions _options;
        private readonly IRedisConnectionFactory _factory;

        public RedisService(IOptions<RedisOptions> options, IRedisConnectionFactory factory)
        {
            _options = options.Value;
            InstancePrefix = _options.InstancePrefix;
            _factory = factory;
        }

        public IConnectionMultiplexer Connection { get; private set; }

        public string InstancePrefix { get; }

        public IDatabase Database { get; private set; }

        public override Task ActivatingAsync() => ConnectAsync();

        public async Task ConnectAsync()
        {
            if (Database != null)
            {
                return;
            }

            (Connection, Database) = await _factory.ConnectAsync(_options.ConfigurationOptions);
        }
    }
}
