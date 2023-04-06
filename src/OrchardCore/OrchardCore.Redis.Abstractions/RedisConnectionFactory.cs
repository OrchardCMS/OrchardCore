using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace OrchardCore.Redis.Services
{
    public sealed class RedisConnectionFactory : IRedisConnectionFactory, IDisposable
    {
        private readonly ILogger _logger;

        private readonly SemaphoreSlim _semaphore = new(1);

        public RedisConnectionFactory(ILogger<RedisConnectionFactory> logger)
        {
            _logger = logger;
        }

        public IConnectionMultiplexer Connection { get; private set; }

        public async Task<IConnectionMultiplexer> CreateAsync(ConfigurationOptions options)
        {
            if (Connection != null)
            {
                return Connection;
            }

            await _semaphore.WaitAsync();
            try
            {
                Connection ??= await ConnectionMultiplexer.ConnectAsync(options);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Unable to connect to Redis.");
                throw;
            }
            finally
            {
                _semaphore.Release();
            }

            return Connection;
        }

        public void Dispose()
        {
            Connection?.Close();
        }
    }
}
