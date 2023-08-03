using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OrchardCore.Caching.Distributed;
using OrchardCore.Environment.Shell;
using StackExchange.Redis;

namespace OrchardCore.Redis.Services
{
    public class RedisBus : IMessageBus
    {
        private readonly IRedisService _redis;
        private readonly string _hostName;
        private readonly string _channelPrefix;
        private readonly string _messagePrefix;
        private readonly ILogger _logger;

        public RedisBus(IRedisService redis, ShellSettings shellSettings, ILogger<RedisBus> logger)
        {
            _redis = redis;
            _hostName = Dns.GetHostName() + ':' + System.Environment.ProcessId;
            _channelPrefix = redis.InstancePrefix + shellSettings.Name + ':';
            _messagePrefix = _hostName + '/';
            _logger = logger;
        }

        public async Task SubscribeAsync(string channel, Action<string, string> handler)
        {
            if (_redis.Connection == null)
            {
                await _redis.ConnectAsync();
                if (_redis.Connection == null)
                {
                    _logger.LogError("Unable to subscribe to the channel '{ChannelName}'.", _channelPrefix + channel);
                    return;
                }
            }

            try
            {
                var subscriber = _redis.Connection.GetSubscriber();

                await subscriber.SubscribeAsync(RedisChannel.Literal(_channelPrefix + channel), (redisChannel, redisValue) =>
                {
                    var tokens = redisValue.ToString().Split('/').ToArray();

                    if (tokens.Length != 2 || tokens[0].Length == 0 || tokens[0].Equals(_hostName, StringComparison.OrdinalIgnoreCase))
                    {
                        return;
                    }

                    handler(channel, tokens[1]);
                });
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Unable to subscribe to the channel '{ChannelName}'.", _channelPrefix + channel);
            }
        }

        public async Task PublishAsync(string channel, string message)
        {
            if (_redis.Connection == null)
            {
                await _redis.ConnectAsync();
                if (_redis.Connection == null)
                {
                    _logger.LogError("Unable to publish to the channel '{ChannelName}'.", _channelPrefix + channel);
                    return;
                }
            }

            try
            {
                await _redis.Connection.GetSubscriber().PublishAsync(RedisChannel.Literal(_channelPrefix + channel), _messagePrefix + message);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Unable to publish to the channel '{ChannelName}'.", _channelPrefix + channel);
            }
        }
    }
}
