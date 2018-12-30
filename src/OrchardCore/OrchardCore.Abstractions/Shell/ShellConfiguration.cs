using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;

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

        private IConfigurationBuilder _configurationBuilder;

        public ShellConfiguration(IConfigurationBuilder builder)
        {
            _configurationBuilder = builder;
        }

        public ShellConfiguration(ShellConfiguration configuration)
        {
            if (configuration._configuration == null)
            {
                _configurationBuilder = configuration._configurationBuilder;
                return;
            }

            _configurationBuilder = new ConfigurationBuilder()
                .AddConfiguration(configuration._configuration);

            _updatableData = new ConfigurationBuilder()
                .Add(new MemoryConfigurationSource())
                .Build();

            _configuration = _configurationBuilder
                .AddConfiguration(_updatableData)
                .Build();
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
                            _configurationBuilder = _configurationBuilder ??
                                new ConfigurationBuilder();

                            _updatableData = _updatableData ??
                                new ConfigurationBuilder()
                                .Add(new MemoryConfigurationSource())
                                .Build();

                            _configuration = _configurationBuilder
                                .AddConfiguration(_updatableData)
                                .Build();
                        }
                    }
                }

                return _configuration;
            }
            set
            {
                lock (this)
                {
                    _configuration = value;
                }
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
