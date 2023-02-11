using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell.Descriptor.Models;
using OrchardCore.Modules;
using StackExchange.Redis;

namespace OrchardCore.Redis.Services
{
    public class RedisService : ModularTenantEvents, IRedisService, IDisposable
    {
        private readonly IOptions<RedisOptions> _options;
        private readonly ILogger _logger;
        private readonly ShellDescriptor _shellDescriptor;

        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1);

        public RedisService(
            IOptions<RedisOptions> options,
            ILogger<RedisService> logger,
            ShellDescriptor shellDescriptor)
        {
            _options = options;
            _logger = logger;
            _shellDescriptor = shellDescriptor;
            InstancePrefix = options.Value.InstancePrefix;
        }

        public IConnectionMultiplexer Connection { get; private set; }
        public string InstancePrefix { get; private set; }
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
                    Database = Connection.GetDatabase(_shellDescriptor.SerialNumber);
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
