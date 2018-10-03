using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.Distributed.Settings;
using OrchardCore.Entities;
using OrchardCore.Settings;
using StackExchange.Redis;

namespace OrchardCore.Distributed.Options
{
    /// <summary>
    /// Allow to override individual redis 'ConfigurationOptions' properties
    /// whose some are not available through the redis configuration string.
    /// </summary>
    public class RedisConfigurationOptionsSetup : IConfigureOptions<ConfigurationOptions>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public RedisConfigurationOptionsSetup(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public void Configure(ConfigurationOptions o)
        {
            // Resolved here to break a circular dependency when using the 'RedisMessageBus'.
            var siteService = _httpContextAccessor.HttpContext.RequestServices.GetService<ISiteService>();
            var siteSettings = siteService.GetSiteSettingsAsync().GetAwaiter().GetResult();

            if (siteSettings.Has<RedisSettings>())
            {
                // Convert the redis configuration string to a redis 'ConfigurationOptions' object.
                var c = ConfigurationOptions.Parse(siteSettings.As<RedisSettings>().Configuration);

                o.SyncTimeout = c.SyncTimeout;
                // Property not yet available.
                //o.AsyncTimeout = c.AsyncTimeout;
                o.AllowAdmin = c.AllowAdmin;
                o.AbortOnConnectFail = c.AbortOnConnectFail;
                o.ResolveDns = c.ResolveDns;
                o.ServiceName = c.ServiceName;
                o.ClientName = c.ClientName;
                o.ChannelPrefix = c.ChannelPrefix;
                o.ConfigurationChannel = c.ConfigurationChannel;
                o.KeepAlive = c.KeepAlive;
                o.ConnectTimeout = c.ConnectTimeout;
                o.ConnectRetry = c.ConnectRetry;
                o.ConfigCheckSeconds = c.ConfigCheckSeconds;
                o.DefaultVersion = c.DefaultVersion;
                o.Password = c.Password;
                o.TieBreaker = c.TieBreaker;
                o.Ssl = c.Ssl;
                o.SslHost = c.SslHost;
                o.HighPrioritySocketThreads = c.HighPrioritySocketThreads;
                o.WriteBuffer = c.WriteBuffer;
                o.Proxy = c.Proxy;
                o.ResponseTimeout = c.ResponseTimeout;
                o.DefaultDatabase = c.DefaultDatabase;
                o.SslProtocols = c.SslProtocols;
                o.CommandMap = c.CommandMap;

                foreach (var item in c.EndPoints)
                {
                    o.EndPoints.Add(item);
                }
            }
        }
    }
}
