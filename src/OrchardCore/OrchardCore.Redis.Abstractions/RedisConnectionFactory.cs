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

        public IDatabase Database { get; private set; }

        public async Task<(IConnectionMultiplexer Connection, IDatabase Database)> ConnectAsync(ConfigurationOptions options)
        {
            if (Database != null)
            {
                return (Connection, Database);
            }

            await _semaphore.WaitAsync();
            try
            {
                Connection ??= await ConnectionMultiplexer.ConnectAsync(options);
                Database ??= Connection.GetDatabase();
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

            return (Connection, Database);
        }

        public void Dispose()
        {
            Connection?.Close();
        }
    }
}
