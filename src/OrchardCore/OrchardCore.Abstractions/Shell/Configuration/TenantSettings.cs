using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;

namespace OrchardCore.Environment.Shell.Configuration
{
    /// <summary>
    /// Holds the tenant settings <see cref="IConfiguration"/> which is built once
    /// from the application configuration 'appsettings.json', the 'App_Data/appsettings.json'
    /// file and then the 'App_Data/tenants.json' file.
    /// </summary>
    public class TenantSettings : IConfiguration
    {
        private IConfiguration _configuration;
        private IConfiguration _updatableData;

        private IConfigurationBuilder _configurationBuilder;

        public TenantSettings()
        {
        }

        public TenantSettings(IConfiguration configuration)
        {
            _configurationBuilder = new ConfigurationBuilder()
                .AddConfiguration(configuration);
        }

        public TenantSettings(TenantSettings settings)
        {
            if (settings._configuration == null)
            {
                _configurationBuilder = settings._configurationBuilder;
                return;
            }

            _configurationBuilder = new ConfigurationBuilder()
                .AddConfiguration(settings._configuration);
        }

        /// <summary>
        /// The tenant settings <see cref="IConfiguration"/>.
        /// </summary>
        private IConfiguration Configuration
        {
            get
            {
                if (_configuration == null)
                {
                    lock (this)
                    {
                        if (_configuration == null)
                        {
                            var configurationBuilder = _configurationBuilder ??
                                new ConfigurationBuilder();

                            _updatableData = new ConfigurationBuilder()
                                .AddInMemoryCollection()
                                .Build();

                            _configuration = configurationBuilder
                                .AddConfiguration(_updatableData)
                                .Build();
                        }
                    }
                }

                return _configuration;
            }
        }

        public string this[string key]
        {
            get => Configuration[key];

            set
            {
                if (value != null && Configuration[key] != value)
                {
                    lock (this)
                    {
                        _updatableData[key] = value;
                    }
                }
            }
        }

        public IConfigurationSection GetSection(string key)
        {
            return Configuration.GetSection(key);
        }

        public IEnumerable<IConfigurationSection> GetChildren()
        {
            return Configuration.GetChildren();
        }

        public IChangeToken GetReloadToken()
        {
            return Configuration.GetReloadToken();
        }
    }
}
