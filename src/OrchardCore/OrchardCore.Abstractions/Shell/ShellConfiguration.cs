using System;
using Microsoft.Extensions.Configuration;

namespace OrchardCore.Environment.Shell
{
    /// <summary>
    /// Holds the tenant <see cref="IConfiguration"/> which is lazyly built
    /// from the application configuration, the 'App_Data/appsettings.json'
    /// file and then the 'App_Data/Sites/{tenant}/appsettings.json' file.
    /// </summary>
    internal class ShellConfiguration
    {
        private IConfiguration _configuration;
        private IConfiguration _updatableData;

        private readonly ShellSettings _settings;
        private Func<string, IConfigurationBuilder> _configBuilderFactory;
        private IConfigurationBuilder _configurationBuilder;

        public ShellConfiguration(ShellSettings settings, Func<string, IConfigurationBuilder> factory)
        {
            _settings = settings;
            _configBuilderFactory = factory;
        }

        public ShellConfiguration(ShellSettings settings, ShellConfiguration configuration)
        {
            _settings = settings;

            if (configuration._configuration == null)
            {
                _configBuilderFactory = configuration._configBuilderFactory;
                return;
            }

            _configurationBuilder = new ConfigurationBuilder()
                .AddConfiguration(configuration._configuration);
        }

        /// <summary>
        /// The tenant lazyly built <see cref="IConfiguration"/>.
        /// </summary>
        public IConfiguration Configuration
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
                                _configBuilderFactory?.Invoke(_settings.Name) ??
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
                if (Configuration[key] != value)
                {
                    lock (this)
                    {
                        _updatableData[key] = value;
                    }
                }
            }
        }
    }
}
