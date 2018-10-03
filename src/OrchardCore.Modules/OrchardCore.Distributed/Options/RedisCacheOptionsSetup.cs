using Microsoft.Extensions.Caching.Redis;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;
using StackExchange.Redis;

namespace OrchardCore.Distributed.Options
{
    public class RedisCacheOptionsSetup : IConfigureOptions<RedisCacheOptions>
    {
        private readonly IOptions<ConfigurationOptions> _optionsAccessor;
        private readonly ShellSettings _shellSettings;

        public RedisCacheOptionsSetup(IOptions<ConfigurationOptions> optionsAccessor, ShellSettings shellSettings)
        {
            _optionsAccessor = optionsAccessor;
            _shellSettings = shellSettings;
        }

        public void Configure(RedisCacheOptions options)
        {
            // Right now we can only pass a configuration string.
            options.Configuration = _optionsAccessor.Value.ToString();
            options.InstanceName = _shellSettings.Name;
        }
    }
}
