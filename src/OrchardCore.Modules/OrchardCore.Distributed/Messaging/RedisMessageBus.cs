using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Distributed.Options;
using OrchardCore.Environment.Shell;
using StackExchange.Redis;

namespace OrchardCore.Distributed.Messaging
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

        public void Subscribe(string channel, Action<string, string> handler)
        {
            Connect();

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
                });
            }
        }

        public void Publish(string channel, string message)
        {
            Connect();

            if (_database?.Multiplexer.IsConnected ?? false)
            {
                _database.Publish(_channelPrefix + channel, _messagePrefix + message);
            }
        }

        private void Connect()
        {
            if (_initialized)
            {
                return;
            }

            _connectionLock.Wait();
            try
            {
                if (!_initialized)
                {
                    _connection = ConnectionMultiplexer.Connect(_redisOptionsAccessor.Value.ConfigurationOptions);
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