using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace OrchardCore.Redis
{
    public sealed class RedisDatabaseFactory : IRedisDatabaseFactory, IDisposable, IAsyncDisposable
    {
        private readonly ILogger _logger;
        private readonly SemaphoreSlim _semaphore = new(1);
        private IDatabase _database;

        public RedisDatabaseFactory(ILogger<RedisDatabaseFactory> logger) => _logger = logger;

        public async Task<IDatabase> CreateAsync(ConfigurationOptions options)
        {
            if (_database != null)
            {
                return _database;
            }

            await _semaphore.WaitAsync();
            try
            {
                _database ??= (await ConnectionMultiplexer.ConnectAsync(options)).GetDatabase();
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

            return _database;
        }

        public void Dispose() => _database?.Multiplexer.Dispose();

        public ValueTask DisposeAsync() => _database?.Multiplexer.DisposeAsync() ?? default;
    }
}
