using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;
using StackExchange.Redis;

namespace OrchardCore.Distributed.Messaging
{
    public class RedisMessageBus : IMessageBus, IDisposable
    {
        private readonly SemaphoreSlim _connectionLock = new SemaphoreSlim(initialCount: 1, maxCount: 1);
        private volatile ConnectionMultiplexer _connection;

        private IDatabase _database;
        private readonly string _hostName;
        private readonly string _channelPrefix;
        private readonly string _messagePrefix;

        private readonly IOptions<ConfigurationOptions> _optionsAccessor;

        public RedisMessageBus(IOptions<ConfigurationOptions> optionsAccessor, ShellSettings shellSettings)
        {
            _optionsAccessor = optionsAccessor;
            _channelPrefix = shellSettings.Name + ":";
            _hostName = Dns.GetHostName() + ":" + Process.GetCurrentProcess().Id;
            _messagePrefix = _hostName + "/";
        }

        public void Subscribe(string channel, Action<string, string> handler)
        {
            Connect();

            var subscriber = _connection?.GetSubscriber();

            subscriber?.Subscribe(_channelPrefix + channel, (redisChannel, redisValue) =>
            {
                var tokens = redisValue.ToString().Split('/').ToArray();

                if (tokens.Length != 2 || tokens[0].Length == 0)
                {
                    return;
                }

                handler(channel, tokens[1]);
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
                    _connection = ConnectionMultiplexer.Connect(_optionsAccessor.Value);
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