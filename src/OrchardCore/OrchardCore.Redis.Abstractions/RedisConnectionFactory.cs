using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace OrchardCore.Redis.Services
{
    public sealed class RedisConnectionFactory : IRedisConnectionFactory, IDisposable, IAsyncDisposable
    {
        private readonly ILogger _logger;
        private readonly SemaphoreSlim _semaphore = new(1);
        private IConnectionMultiplexer _connection;

        public RedisConnectionFactory(ILogger<RedisConnectionFactory> logger) => _logger = logger;

        public async Task<IConnectionMultiplexer> CreateAsync(ConfigurationOptions options)
        {
            if (_connection != null)
            {
                return _connection;
            }

            await _semaphore.WaitAsync();
            try
            {
                _connection ??= await ConnectionMultiplexer.ConnectAsync(options);
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

            return _connection;
        }

        public void Dispose() => _connection?.Dispose();

        public ValueTask DisposeAsync() => _connection?.DisposeAsync() ?? default;
    }
}
