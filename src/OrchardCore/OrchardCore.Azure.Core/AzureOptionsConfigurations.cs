using System.Text.Json.Nodes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell.Configuration;

namespace OrchardCore.Azure.Core;

public sealed class AzureOptionsConfigurations : IConfigureNamedOptions<AzureOptions>
{
    private readonly IShellConfiguration _configuration;
    private HashSet<string> _configuredAliases;

    public AzureOptionsConfigurations(IShellConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void Configure(AzureOptions options)
    {
        options.AuthenticationType = AzureAuthenticationType.Default;
    }

    public void Configure(string name, AzureOptions options)
    {
        var credentialsSection = _configuration.GetSection("Azure:Credentials");

        if (!credentialsSection.Exists())
        {
            return;
        }

        var section = credentialsSection.GetSection(name);

        if (!section.Exists())
        {
            return;
        }

        var alias = section["Alias"];

        if (alias is not null)
        {
            _configuredAliases ??= new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            if (_configuredAliases.Add(alias))
            {
                Configure(alias, options);
            }

            return;
        }

        var type = section[nameof(AzureOptions.AuthenticationType)];

        if (type is not null && Enum.TryParse<AzureAuthenticationType>(type, true, out var authType))
        {
            options.AuthenticationType = authType;
        }

        options.TenantId = section[nameof(AzureOptions.TenantId)];
        options.ClientId = section[nameof(AzureOptions.ClientId)];

        options.Properties = new JsonObject();

        foreach (var child in section.GetChildren())
        {
            var key = child.Key;
            if (child.Value is null ||
                key == nameof(AzureOptions.TenantId) ||
                key == nameof(AzureOptions.ClientId) ||
                key == nameof(AzureOptions.AuthenticationType))
            {
                continue;
            }

            try
            {
                var node = JsonNode.Parse(child.Value);
                options.Properties[key] = node;
            }
            catch
            {
                // fallback to string if not JSON
                options.Properties[key] = child.Value;
            }
        }
    }
}
