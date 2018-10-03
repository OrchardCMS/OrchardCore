using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using OrchardCore.Environment.Shell;
using StackExchange.Redis;

namespace OrchardCore.Distributed.Messaging
{
    public class RedisMessageBus : IMessageBus, IDisposable
    {
        private readonly SemaphoreSlim _connectionLock = new SemaphoreSlim(initialCount: 1, maxCount: 1);
        private volatile ConnectionMultiplexer _connection;

        private IDatabase _database;
        private readonly string _host;
        private readonly string _channelPrefix;
        private readonly string _messagePrefix;

        private readonly IOptions<ConfigurationOptions> _configurationOptions;

        public RedisMessageBus(ShellSettings shellSettings, IOptions<ConfigurationOptions> configurationOptions)
        {
            _channelPrefix = shellSettings.Name;
            _host = Dns.GetHostName() + ":" + Process.GetCurrentProcess().Id;
            _messagePrefix = _host + "/";

            _configurationOptions = configurationOptions;
        }

        public void Subscribe(string channel, Action<string, string> handler)
        {
            Connect();

            var subscriber = _connection?.GetSubscriber();

            subscriber?.Subscribe(_channelPrefix + channel, (redisChannel, redisValue) =>
            {
                var tokenizer = new StringTokenizer(redisValue, new char[] { '/' });

                if (tokenizer.Count() != 2)
                {
                    return;
                }

                // Ignore self sent messages.
                if (tokenizer.First().Equals(_host, StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                handler(channel, tokenizer.ElementAt(1).ToString());
            });
        }

        public void Publish(string channel, string message)
        {
            Connect();

            _database?.Publish(_channelPrefix + channel, _messagePrefix + message);
        }

        private void Connect()
        {
            if (_database != null)
            {
                return;
            }

            _connectionLock.Wait();
            try
            {
                if (_database == null)
                {
                    _connection = ConnectionMultiplexer.Connect(_configurationOptions.Value);
                    _database = _connection.GetDatabase();
                }
            }
            finally
            {
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