using System;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;
using Microsoft.Extensions.Primitives;

namespace OrchardCore.Environment.Shell
{
    /// <summary>
    /// Holds the tenant <see cref="IConfiguration"/> which is lazyly built
    /// from the application configuration, the 'App_Data/appsettings.json'
    /// file and then the 'App_Data/Sites/{tenant}/appsettings.json' file.
    /// </summary>
    internal class ShellConfiguration : IEquatable<ShellConfiguration>
    {
        private IConfiguration _configuration;
        private IConfiguration _updatableData;

        private IConfigurationBuilder _configurationBuilder;

        public ShellConfiguration(IConfigurationBuilder configurationBuilder)
        {
            _configurationBuilder = configurationBuilder;
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
                            _updatableData = _updatableData ??
                                new ConfigurationBuilder()
                                .Add(new MemoryConfigurationSource())
                                .Build();

                            _configurationBuilder = _configurationBuilder ??
                                new ConfigurationBuilder();

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

        internal StringValues StringValues => new StringValues(
            Configuration.GetChildren()
            .Where(s => s.Value != null)
            .OrderBy(s => s.Key)
            .Select(s => s.Value)
            .ToArray());

        public bool Equals(ShellConfiguration other)
        {
            return StringValues.Equals(other?.StringValues ?? String.Empty);
        }

        public override int GetHashCode()
        {
            return StringValues.GetHashCode();
        }
    }
}
