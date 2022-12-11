using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;
using OrchardCore.Modules;
using StackExchange.Redis;

namespace OrchardCore.Redis.Services
{
    public class RedisService : ModularTenantEvents, IRedisService, IDisposable
    {
        private readonly IOptions<RedisOptions> _options;
        private readonly ILogger _logger;

        private readonly SemaphoreSlim _semaphore = new(1);

        public RedisService(ShellSettings shellSettings, IOptions<RedisOptions> options, ILogger<RedisService> logger)
        {
            _options = options;
            _logger = logger;

            if (!String.IsNullOrWhiteSpace(options.Value.InstancePrefix))
            {
                InstancePrefix = $"{options.Value.InstancePrefix}_";
            }

            TenantPrefix = $"{shellSettings.TenantId}_{shellSettings.Name}_";
        }

        public IConnectionMultiplexer Connection { get; private set; }
        public string InstancePrefix { get; private set; } = String.Empty;
        public string TenantPrefix { get; private set; }
        public IDatabase Database { get; private set; }

        public override Task ActivatingAsync() => ConnectAsync();

        public async Task ConnectAsync()
        {
            if (Database != null)
            {
                return;
            }

            await _semaphore.WaitAsync();

            try
            {
                if (Database == null)
                {
                    Connection = await ConnectionMultiplexer.ConnectAsync(_options.Value.ConfigurationOptions);
                    Database = Connection.GetDatabase();
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Unable to connect to Redis.");
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public void Dispose()
        {
            if (Connection != null)
            {
                Connection.Close();
            }
        }
    }
}
