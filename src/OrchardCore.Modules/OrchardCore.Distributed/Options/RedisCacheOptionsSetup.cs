using Microsoft.Extensions.Caching.Redis;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;
using StackExchange.Redis;

namespace OrchardCore.Distributed.Options
{
    public class RedisCacheOptionsSetup : IConfigureOptions<RedisCacheOptions>
    {
        private readonly IOptions<ConfigurationOptions> _configurationOptions;
        private readonly ShellSettings _shellSettings;

        public RedisCacheOptionsSetup(
            IOptions<ConfigurationOptions> configurationOptions,
            ShellSettings shellSettings)
        {
            _configurationOptions = configurationOptions;
            _shellSettings = shellSettings;
        }

        public void Configure(RedisCacheOptions options)
        {
            // We can only pass a string representing the configuration.
            // Passing a redis 'ConfigurationOptions' is not yet available.
            options.Configuration = _configurationOptions.Value.ToString();
            options.InstanceName = _shellSettings.Name;
        }
    }
}
