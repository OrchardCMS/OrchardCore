using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;
using StackExchange.Redis;

namespace OrchardCore.Redis.Services
{
    public class RedisService : IRedisService, IDisposable
    {
        private readonly string _tenant;
        private readonly IOptions<RedisOptions> _options;
        private readonly ILogger _logger;

        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1);

        public RedisService(ShellSettings shellSettings, IOptions<RedisOptions> options, ILogger<RedisService> logger)
        {
            _tenant = shellSettings.Name;
            _options = options;
            _logger = logger;
        }

        public IConnectionMultiplexer Connection { get; private set; }
        public IDatabase Database { get; private set; }

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
                _logger.LogError(e, "Tenant '{TenantName}' is unable to connect to Redis.", _tenant);
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
