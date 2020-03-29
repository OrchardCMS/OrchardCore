using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Configuration;
using StackExchange.Redis;

namespace OrchardCore.Redis.Options
{
    public class RedisOptionsSetup : IConfigureOptions<RedisOptions>
    {
        private readonly string _tenant;
        private readonly IShellConfiguration _shellConfiguration;
        private readonly ILogger _logger;

        public RedisOptionsSetup(ShellSettings shellSettings, IShellConfiguration shellConfiguration, ILogger<RedisOptionsSetup> logger)
        {
            _tenant = shellSettings.Name;
            _shellConfiguration = shellConfiguration;
            _logger = logger;
        }

        public void Configure(RedisOptions options)
        {
            var configuration = _shellConfiguration.GetSection("OrchardCore.Redis").GetValue<string>("Configuration") ?? String.Empty;

            if (String.IsNullOrWhiteSpace(configuration))
            {
                _logger.LogError("Tenant '{TenantName}' does not have a valid Redis configuration.", _tenant);
                return;
            }

            options.ConfigurationOptions = ConfigurationOptions.Parse(configuration);
        }
    }
}
