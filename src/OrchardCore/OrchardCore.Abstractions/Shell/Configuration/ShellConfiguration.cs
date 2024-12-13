using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using OrchardCore.Environment.Shell.Configuration.Internal;

namespace OrchardCore.Environment.Shell.Configuration;

/// <summary>
/// Holds the tenant <see cref="IConfiguration"/> which is lazily built
/// from the application configuration 'appsettings.json', the 'App_Data/appsettings.json'
/// file and then the 'App_Data/Sites/{tenant}/appsettings.json' file.
/// </summary>
public class ShellConfiguration : IShellConfiguration
{
    private IConfigurationRoot _configuration;
    private UpdatableDataProvider _updatableData;

    private readonly string _name;
    private readonly Func<string, Action<IConfigurationBuilder>, Task<IConfigurationRoot>> _factoryAsync;
    private readonly SemaphoreSlim _semaphore = new(1);
    private bool _released;

    public ShellConfiguration()
    {
    }

    public ShellConfiguration(IConfiguration configuration)
    {
        _updatableData = new UpdatableDataProvider();

        _configuration = new ConfigurationBuilder()
            .AddConfiguration(configuration)
            .Add(_updatableData)
            .Build();
    }

    public ShellConfiguration(IConfigurationBuilder builder)
    {
        _updatableData = new UpdatableDataProvider();
        _configuration = builder.Add(_updatableData).Build();
    }

    public ShellConfiguration(string name, Func<string, Action<IConfigurationBuilder>, Task<IConfigurationRoot>> factoryAsync)
    {
        _name = name;
        _factoryAsync = factoryAsync;
    }

    public ShellConfiguration(ShellConfiguration configuration) : this(null, configuration)
    {
    }

    public ShellConfiguration(string name, ShellConfiguration configuration)
    {
        _name = name;

        if (configuration._configuration is not null)
        {
            _updatableData = new UpdatableDataProvider(configuration._updatableData.ToArray());

            _configuration = new ConfigurationBuilder()
                .AddConfiguration(configuration._configuration, shouldDisposeConfiguration: true)
                .Add(_updatableData)
                .Build();

            return;
        }

        if (name is null)
        {
            return;
        }

        _factoryAsync = configuration._factoryAsync;
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

            _updatableData = new UpdatableDataProvider();

            _configuration = _factoryAsync is not null
                ? await _factoryAsync(_name, builder => builder.Add(_updatableData))
                : new ConfigurationBuilder().Add(_updatableData).Build();
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
        _semaphore?.Dispose();
    }
}
