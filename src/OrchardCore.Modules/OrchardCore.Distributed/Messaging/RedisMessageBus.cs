using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using OrchardCore.Distributed.Settings;
using OrchardCore.Entities;
using OrchardCore.Environment.Shell;
using OrchardCore.Settings;
using StackExchange.Redis;

namespace OrchardCore.Distributed.Messaging
{
    public class RedisMessageBus : IMessageBus, IDisposable
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        private volatile ConnectionMultiplexer _connection;

        private IDatabase _database;
        private string _configuration;
        private readonly string _host;
        private readonly string _tenant;
        private readonly string _prefix;

        private readonly SemaphoreSlim _connectionLock = new SemaphoreSlim(initialCount: 1, maxCount: 1);

        private ConcurrentDictionary<string, ConcurrentBag<Action<string, string>>> _handlers = new ConcurrentDictionary<string, ConcurrentBag<Action<string, string>>>();

        public RedisMessageBus(ShellSettings shellSettings, IHttpContextAccessor httpContextAccessor)
        {
            _tenant = shellSettings.Name;
            _host = Dns.GetHostName() + ":" + Process.GetCurrentProcess().Id;
            _prefix = _host + "/" + _tenant + "/";

            _httpContextAccessor = httpContextAccessor;
        }

        public void Subscribe(string channel, Action<string, string> handler)
        {
            Connect();

            var subscriber = _connection?.GetSubscriber();

            subscriber?.Subscribe(channel, (redisChannel, redisValue) =>
            {
                var tokenizer = new StringTokenizer(redisValue, new char[] { '/' });

                if (tokenizer.Count() != 3)
                {
                    return;
                }

                // Ignore self sent messages.
                if (tokenizer.First().Equals(_host, StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                // Ignore messages from another tenant.
                if (!tokenizer.ElementAt(1).Equals(_tenant, StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                handler(redisChannel, tokenizer.ElementAt(2).ToString());
            });
        }

        public void Publish(string channel, string message)
        {
            Connect();

            _database?.Publish(channel, _prefix + message);
        }

        private void Connect()
        {
            if (_configuration != null)
            {
                return;
            }

            _connectionLock.Wait();
            try
            {
                if (_configuration == null)
                {
                    //if (_options.ConfigurationOptions != null)
                    //{
                    //    _connection = ConnectionMultiplexer.Connect(_options.ConfigurationOptions);
                    //}
                    //else
                    //{
                    //    _connection = ConnectionMultiplexer.Connect(_options.Configuration);
                    //}

                    var siteSettings = _httpContextAccessor.HttpContext.RequestServices.GetRequiredService<ISiteService>()
                        .GetSiteSettingsAsync().GetAwaiter().GetResult();

                    if (siteSettings.Has<RedisSettings>())
                    {
                        // Right now, only a string representing the configuration is available.
                        // In the next version there will be a full 'ConfigurationOptions' object.
                        _configuration = siteSettings.As<RedisSettings>().Configuration;
                        _connection = ConnectionMultiplexer.Connect(_configuration);
                        _database = _connection.GetDatabase();
                    }
                    else
                    {
                        _configuration = String.Empty;
                    }
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