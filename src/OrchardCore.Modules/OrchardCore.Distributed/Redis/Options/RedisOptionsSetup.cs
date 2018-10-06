using System;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Distributed.Redis.Settings;
using OrchardCore.Entities;
using OrchardCore.Environment.Shell;
using OrchardCore.Settings;
using StackExchange.Redis;

namespace OrchardCore.Distributed.Redis.Options
{
    /// <summary>
    /// Configure 'RedisOptions' which is a wrapper of the redis 'ConfigurationOptions' object. This allows
    /// to override all configuration options whose some are not available through the configuration string.
    /// </summary>
    public class RedisOptionsSetup : IConfigureOptions<RedisOptions>
    {
        private readonly string _tenantName;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public RedisOptionsSetup(ShellSettings shellSettings, IHttpContextAccessor httpContextAccessor, ILogger<RedisOptionsSetup> logger)
        {
            _tenantName = shellSettings.Name;
            _httpContextAccessor = httpContextAccessor;
            Logger = logger;
        }

        public ILogger Logger { get; set; }

        public void Configure(RedisOptions options)
        {
            // Site service is resolved here to break a circular dependency when using 'RedisMessageBus'.
            // 'ISiteService' => 'ISignal' => 'IMessageBus' => 'IOptions<RedisOptions>' => 'ISiteService'.
            var siteService = _httpContextAccessor.HttpContext.RequestServices.GetService<ISiteService>();
            var siteSettings = siteService.GetSiteSettingsAsync().GetAwaiter().GetResult();

            if (siteSettings.Has<RedisSettings>())
            {
                try
                {
                    // Convert the redis configuration string to a redis 'ConfigurationOptions' object.
                    options.ConfigurationOptions = ConfigurationOptions.Parse(siteSettings.As<RedisSettings>().Configuration);
                }
                catch (Exception e)
                {
                    Logger.LogError(e, "'{TenantName}' has an invalid Redis configuration.", _tenantName);
                }
            }
        }
    }
}
