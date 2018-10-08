using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using OrchardCore.Environment.Shell;
using StackExchange.Redis;

namespace OrchardCore.Distributed.Redis.Services
{
    public class RedisMessageBus : IMessageBus
    {
        private readonly string _hostName;
        private readonly string _channelPrefix;
        private readonly string _messagePrefix;
        private readonly IRedis _redis;

        public RedisMessageBus(ShellSettings shellSettings, IRedis redis)
        {
            _hostName = Dns.GetHostName() + ":" + Process.GetCurrentProcess().Id;
            _channelPrefix = shellSettings.Name + ":";
            _messagePrefix = _hostName + "/";
            _redis = redis;
        }

        public async Task SubscribeAsync(string channel, Action<string, string> handler)
        {
            await _redis.ConnectAsync();

            if (_redis.IsConnected)
            {
                var subscriber = _redis.Connection.GetSubscriber();

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
            await _redis.ConnectAsync();

            if (_redis.IsConnected)
            {
                var subscriber = _redis.Connection.GetSubscriber();
                subscriber.Publish(_channelPrefix + channel, _messagePrefix + message, CommandFlags.FireAndForget);
            }
        }
    }
}