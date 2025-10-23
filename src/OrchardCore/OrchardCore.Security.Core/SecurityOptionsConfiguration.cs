using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell.Configuration;

namespace OrchardCore.Security.Core;

public sealed class SecurityOptionsConfiguration : IConfigureOptions<SecurityOptions>
{
    private readonly IShellConfiguration _shellConfiguration;
    private readonly ILogger _logger;

    public SecurityOptionsConfiguration(
        IShellConfiguration shellConfiguration,
        ILogger<SecurityOptionsConfiguration> logger)
    {
        _shellConfiguration = shellConfiguration;
        _logger = logger;
    }

    public void Configure(SecurityOptions options)
    {
        var providerSettings = _shellConfiguration.GetSection("OrchardCore:Security");

        if (providerSettings is null)
        {
            _logger.LogWarning("The 'providers' in 'OrchardCore:Security' is not defined in the settings.");

            return;
        }

        try
        {
            var providerSettingsElements = JsonSerializer.Deserialize<JsonElement>(providerSettings.AsJsonNode());

            var providerSettingsObject = JsonObject.Create(providerSettingsElements);

            if (providerSettingsObject is null)
            {
                _logger.LogWarning("The 'providers' in 'OrchardCore:Security' is invalid.");

                return;
            }

            foreach (var providerPair in providerSettingsObject)
            {
                var providerName = providerPair.Key;
                var providerNode = providerPair.Value;

                var credintialsNode = providerNode[nameof(SecurityProvider.Credintials)];

                if (credintialsNode is null)
                {
                    _logger.LogWarning("The provider with the name '{Name}' has no credentials. This provider will be ignore and not used.", providerName);

                    continue;
                }

                var collectionsElement = JsonSerializer.Deserialize<JsonElement>(credintialsNode);

                var credentialsObject = JsonObject.Create(collectionsElement);

                if (credentialsObject is null || credentialsObject.Count == 0)
                {
                    _logger.LogWarning("The provider with the name '{Name}' has no credentials. This provider will be ignore and not used.", providerName);

                    continue;
                }

                var cridentials = new Dictionary<string, SecurityCredentialEntry>(StringComparer.OrdinalIgnoreCase);

                foreach (var connectionPair in credentialsObject)
                {
                    cridentials.Add(connectionPair.Key, connectionPair.Value.Deserialize<SecurityCredentialEntry>());
                }

                if (cridentials.Count == 0)
                {
                    _logger.LogWarning("The provider with the name '{Name}' has no valid credentials. This provider will be ignore and not used.", providerName);

                    continue;
                }

                var provider = new SecurityProvider()
                {
                    Credintials = cridentials,
                };

                var defaultCredentialName = providerNode["DefaultCredentialName"]?.GetValue<string>();

                if (!string.IsNullOrEmpty(defaultCredentialName))
                {
                    provider.DefaultCredentialName = defaultCredentialName;
                }
                else
                {
                    provider.DefaultCredentialName = cridentials.FirstOrDefault().Key;
                }

                options.SecurityProviders.Add(providerName, provider);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Invalid 'CrestApps_AI:Providers' configuration. Please refer to the documentation for instructions on how to set it up correctly.");
        }
    }
}

public sealed class SecurityProvider
{
    public string DefaultCredentialName { get; set; }

    public IDictionary<string, SecurityCredentialEntry> Credintials { get; set; }
}

[JsonConverter(typeof(SecurityCredentialConverter))]
public sealed class SecurityCredentialEntry : ReadOnlyDictionary<string, object>
{
    public SecurityCredentialEntry(SecurityCredentialEntry connection)
        : base(connection)
    {
    }

    public SecurityCredentialEntry(IDictionary<string, object> dictionary)
        : base(dictionary)
    {
    }
}

public sealed class SecurityCredentialConverter : JsonConverter<SecurityCredentialEntry>
{
    public override SecurityCredentialEntry Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // Deserialize into a dictionary first.
        var dictionary = JsonSerializer.Deserialize<Dictionary<string, object>>(ref reader, options);
        return dictionary != null ? new SecurityCredentialEntry(dictionary) : null;
    }

    public override void Write(Utf8JsonWriter writer, SecurityCredentialEntry value, JsonSerializerOptions options)
    {
        // Serialize as dictionary.
        JsonSerializer.Serialize(writer, (IDictionary<string, object>)value, options);
    }
}
