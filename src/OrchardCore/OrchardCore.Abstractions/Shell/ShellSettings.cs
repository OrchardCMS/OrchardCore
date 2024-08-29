using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Environment.Shell.Models;

namespace OrchardCore.Environment.Shell;

/// <summary>
/// The minimalistic set of settings fields and the configuration of a given tenant, all data can be first provided
/// by regular configuration sources, then by default settings of all tenants are stored in 'App_Data/tenants.json',
/// while each tenant configuration is stored in the related site folder 'App_Data/Sites/{tenant}/appsettings.json'.
/// </summary>
public class ShellSettings : IDisposable
{
    /// <summary>
    /// The name of the 'Default' tenant.
    /// </summary>
    public const string DefaultShellName = "Default";

    /// <summary>
    /// The 'RequestUrlHost' string separators allowing to provide multiple hosts.
    /// </summary>
    public static readonly char[] HostSeparators = [',', ' '];

    private readonly ShellConfiguration _settings;
    private readonly ShellConfiguration _configuration;
    internal volatile int _shellCreating;

    private string _requestUrlPrefix;
    private string _versionId;
    private string _tenantId;
    private string _requestUrlHost;
    private string[] _requestUrlHosts;
    private TenantState? _state;

    /// <summary>
    /// Initializes a new <see cref="ShellSettings"/>.
    /// </summary>
    public ShellSettings()
    {
        _settings = new ShellConfiguration();
        _configuration = new ShellConfiguration();
    }

    /// <summary>
    /// Initializes a new <see cref="ShellSettings"/> from an existing one
    /// and from an existing <see cref="Configuration.ShellConfiguration"/>.
    /// </summary>
    public ShellSettings(ShellConfiguration settings, ShellConfiguration configuration)
    {
        _settings = settings;
        _configuration = configuration;
    }

    /// <summary>
    /// Initializes a new <see cref="ShellSettings"/> from an existing one.
    /// </summary>
    public ShellSettings(ShellSettings settings)
    {
        _settings = new ShellConfiguration(settings._settings);
        _configuration = new ShellConfiguration(settings.Name, settings._configuration);
        Name = settings.Name;
    }

    /// <summary>
    /// The tenant name.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Whether this instance has been disposed or not.
    /// </summary>
    [JsonIgnore]
    public bool Disposed { get; private set; }

    /// <summary>
    /// Whether this instance is disposable or not.
    /// </summary>
    [JsonIgnore]
    public bool Disposable { get; internal set; }

    /// <summary>
    /// The tenant version identifier.
    /// </summary>
    public string VersionId
    {
        get => _versionId ??= _settings["VersionId"];
        set
        {
            _settings["TenantId"] ??= _settings["VersionId"] ?? value;
            _versionId = _settings["VersionId"] = value;
        }
    }

    /// <summary>
    /// The tenant identifier.
    /// </summary>
    public string TenantId => _tenantId ??= _settings["TenantId"] ?? _settings["VersionId"];

    /// <summary>
    /// The tenant request url host, multiple separated hosts may be provided.
    /// </summary>
    public string RequestUrlHost
    {
        get => _requestUrlHost ??= _settings["RequestUrlHost"] ?? string.Empty;
        set => _requestUrlHost = _settings["RequestUrlHost"] = value;
    }

    /// <summary>
    /// The tenant request url host(s).
    /// </summary>
    [JsonIgnore]
    public string[] RequestUrlHosts => _requestUrlHosts ??= _settings["RequestUrlHost"]
        ?.Split(HostSeparators, StringSplitOptions.RemoveEmptyEntries)
        ?? [];

    /// <summary>
    /// The tenant request url prefix.
    /// </summary>
    public string RequestUrlPrefix
    {
        get => _requestUrlPrefix ??= _settings["RequestUrlPrefix"]?.Trim(' ', '/') ?? string.Empty;
        set => _requestUrlPrefix = _settings["RequestUrlPrefix"] = value;
    }

    /// <summary>
    /// The tenant state.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public TenantState State
    {
        get => _state ??= _settings.GetValue<TenantState>("State");
        set => _state = Enum.Parse<TenantState>(_settings["State"] = value.ToString());
    }

    /// <summary>
    /// The tenant configuration.
    /// </summary>
    [JsonIgnore]
    public IShellConfiguration ShellConfiguration => _configuration;

    [JsonIgnore]
    public string this[string key]
    {
        get => _configuration[key];
        set => _configuration[key] = value;
    }

    /// <summary>
    /// Ensures that the tenant configuration is initialized.
    /// </summary>
    public Task EnsureConfigurationAsync() => _configuration.EnsureConfigurationAsync();

    public void Dispose()
    {
        // Disposable on reloading or if never registered.
        if (!Disposable || _shellCreating > 0)
        {
            return;
        }

        Close();
        GC.SuppressFinalize(this);
    }

    private void Close()
    {
        if (Disposed)
        {
            return;
        }

        Disposed = true;

        _settings?.Release();
        _configuration?.Release();
    }

    ~ShellSettings() => Close();
}
