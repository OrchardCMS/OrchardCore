using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OrchardCore.Environment.Shell;

namespace OrchardCore.Distributed.Redis.Services
{
    public class RedisBus : IMessageBus
    {
        private readonly string _hostName;
        private readonly string _channelPrefix;
        private readonly string _messagePrefix;
        private readonly IRedisClient _redis;

        public RedisBus(ShellSettings shellSettings, IRedisClient redis, ILogger<RedisBus> logger)
        {
            _hostName = Dns.GetHostName() + ":" + Process.GetCurrentProcess().Id;
            _channelPrefix = shellSettings.Name + ':';
            _messagePrefix = _hostName + '/';
            _redis = redis;
            Logger = logger;
        }

        public ILogger Logger { get; set; }

        public async Task SubscribeAsync(string channel, Action<string, string> handler)
        {
            await _redis.ConnectAsync();

            if (_redis.IsConnected)
            {
                try
                {
                    var subscriber = _redis.Connection.GetSubscriber();

                    await subscriber.SubscribeAsync(_channelPrefix + channel, (redisChannel, redisValue) =>
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

                catch (Exception e)
                {
                    Logger.LogError(e, "'Unable to subscribe to the channel {ChannelName}'.", _channelPrefix + channel);
                }
            }
        }

        public async Task PublishAsync(string channel, string message)
        {
            await _redis.ConnectAsync();

            if (_redis.IsConnected)
            {
                try
                {
                    await _redis.Connection.GetSubscriber().PublishAsync(
                        _channelPrefix + channel, _messagePrefix + message);
                }

                catch (Exception e)
                {
                    Logger.LogError(e, "'Unable to publish to the channel {ChannelName}'.", _channelPrefix + channel);
                }
            }
        }
    }
}