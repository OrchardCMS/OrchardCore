using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using OrchardCore.Environment.Shell.Configuration.Internal;

namespace OrchardCore.Environment.Shell.Configuration
{
    /// <summary>
    /// Holds the tenant <see cref="IConfiguration"/> which is lazily built
    /// from the application configuration 'appsettings.json', the 'App_Data/appsettings.json'
    /// file and then the 'App_Data/Sites/{tenant}/appsettings.json' file.
    /// </summary>
    public class ShellConfiguration : IShellConfiguration
    {
        internal IConfigurationRoot _configuration;
        private UpdatableDataProvider _updatableData;
        private readonly IEnumerable<KeyValuePair<string, string>> _initialData;

        private readonly string _name;
        private readonly Func<string, Task<IConfigurationBuilder>> _configBuilderFactory;
        private readonly IConfiguration _initialConfiguration;
        private readonly SemaphoreSlim _semaphore = new(1);
        private bool _released;

        public ShellConfiguration()
        {
        }

        public ShellConfiguration(IConfiguration configuration)
        {
            _initialConfiguration = configuration;
        }

        public ShellConfiguration(string name, Func<string, Task<IConfigurationBuilder>> factory)
        {
            _name = name;
            _configBuilderFactory = factory;
        }

        public ShellConfiguration(ShellConfiguration configuration) : this(null, configuration)
        {
        }

        public ShellConfiguration(string name, ShellConfiguration configuration)
        {
            _name = name;

            if (configuration._configuration is not null)
            {
                _initialConfiguration = configuration._configuration;
                _initialData = configuration._updatableData.ToArray();

                return;
            }

            if (name is null)
            {
                _initialConfiguration = configuration._initialConfiguration;
                _initialData = configuration._initialData;
                return;
            }

            _configBuilderFactory = configuration._configBuilderFactory;
        }

        private void EnsureConfiguration()
        {
            if (_configuration is not null)
            {
                return;
            }

            EnsureConfigurationAsync().GetAwaiter().GetResult();
        }

        internal async Task EnsureConfigurationAsync()
        {
            if (_configuration is not null)
            {
                return;
            }

            await _semaphore.WaitAsync();
            try
            {
                if (_configuration is not null)
                {
                    return;
                }

                var builder = _configBuilderFactory is not null ?
                    await _configBuilderFactory.Invoke(_name)
                    : new ConfigurationBuilder();

                if (_initialConfiguration is not null)
                {
                    builder.AddConfiguration(_initialConfiguration, shouldDisposeConfiguration: true);
                }

                _updatableData = new UpdatableDataProvider(_initialData ?? Enumerable.Empty<KeyValuePair<string, string>>());

                _configuration = builder
                    .AddConfiguration(new ConfigurationRoot(new[] { _updatableData }), shouldDisposeConfiguration: true)
                    .Build();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        /// <summary>
        /// The tenant configuration lazily built <see cref="IConfiguration"/>.
        /// </summary>
        private IConfiguration Configuration
        {
            get
            {
                EnsureConfiguration();
                return _configuration;
            }
        }

        public string this[string key]
        {
            get
            {
                var value = Configuration[key];

                return value ?? (key.Contains('_')
                    ? Configuration[key.Replace('_', '.')]
                    : null);
            }
            set
            {
                EnsureConfiguration();
                _updatableData.Set(key, value);
            }
        }

        public IConfigurationSection GetSection(string key) => Configuration.GetSectionCompat(key);

        public IEnumerable<IConfigurationSection> GetChildren() => Configuration.GetChildren();

        public IChangeToken GetReloadToken() => Configuration.GetReloadToken();

        public void Release()
        {
            if (_released)
            {
                return;
            }

            _released = true;

            (_configuration as IDisposable)?.Dispose();
        }
    }
}
