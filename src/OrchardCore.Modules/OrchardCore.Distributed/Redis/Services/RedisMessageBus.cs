using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Distributed.Redis.Options;
using OrchardCore.Environment.Shell;
using StackExchange.Redis;

namespace OrchardCore.Distributed.Redis.Services
{
    public class RedisMessageBus : IMessageBus, IDisposable
    {
        private readonly SemaphoreSlim _connectionLock = new SemaphoreSlim(initialCount: 1, maxCount: 1);
        private volatile ConnectionMultiplexer _connection;

        private bool _initialized;
        private IDatabase _database;
        private readonly string _hostName;
        private readonly string _tenantName;
        private readonly string _channelPrefix;
        private readonly string _messagePrefix;

        private readonly IOptions<RedisOptions> _redisOptionsAccessor;

        public RedisMessageBus(ShellSettings shellSettings, IOptions<RedisOptions> redisOptionsAccessor, ILogger<RedisMessageBus> logger)
        {
            _hostName = Dns.GetHostName() + ":" + Process.GetCurrentProcess().Id;

            _tenantName = shellSettings.Name;
            _channelPrefix = _tenantName + ":";
            _messagePrefix = _hostName + "/";

            _redisOptionsAccessor = redisOptionsAccessor;
            Logger = logger;
        }

        public ILogger Logger { get; set; }

        public async Task SubscribeAsync(string channel, Action<string, string> handler)
        {
            await ConnectAsync();

            if (_database?.Multiplexer.IsConnected ?? false)
            {
                var subscriber = _database.Multiplexer.GetSubscriber();

                subscriber.Subscribe(_channelPrefix + channel, (redisChannel, redisValue) =>
                {
                    var tokens = redisValue.ToString().Split('/').ToArray();

                    if (tokens.Length != 2 || tokens[0].Length == 0 || tokens[0]
                        .Equals(_hostName, StringComparison.OrdinalIgnoreCase))
                    {
                        return;
                    }

                    handler(channel, tokens[1]);
                }, CommandFlags.FireAndForget);
            }
        }

        public async Task PublishAsync(string channel, string message)
        {
            await ConnectAsync();

            if (_database?.Multiplexer.IsConnected ?? false)
            {
                var subscriber = _database.Multiplexer.GetSubscriber();
                subscriber.Publish(_channelPrefix + channel, _messagePrefix + message, CommandFlags.FireAndForget);
            }
        }

        private async Task ConnectAsync()
        {
            if (_initialized)
            {
                return;
            }

            await _connectionLock.WaitAsync();
            try
            {
                if (!_initialized)
                {
                    _connection = await ConnectionMultiplexer.ConnectAsync(_redisOptionsAccessor.Value.ConfigurationOptions);
                    _database = _connection.GetDatabase();
                    _initialized = true;
                }
            }
            catch (Exception e)
            {
                Logger.LogError(e, "'{TenantName}' is unable to connect to Redis.", _tenantName);
            }
            finally
            {
                _initialized = true;
                _connectionLock.Release();
            }
        }

        public void Dispose()
        {
            if (_connection != null)
            {
                _connection.Close();
            }
        }
    }
}