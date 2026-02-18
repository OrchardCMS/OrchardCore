# Reverse Proxy (`OrchardCore.ReverseProxy`)

Enables configuration of hosting scenarios with a reverse proxy, like which HTTP headers to forward.

## Reverse Proxy Settings Configuration

### Admin Configuration

You can configure reverse proxy settings from the admin area by navigating to **Settings → Reverse Proxy**.

The following settings are available:

- **X-Forwarded-For**: Enables forwarding of the client IP address
- **X-Forwarded-Host**: Enables forwarding of the original host requested by the client
- **X-Forwarded-Proto**: Enables forwarding of the protocol (HTTP or HTTPS) used by the client

These settings are stored in the site configuration and can be managed per tenant.

### File Settings Configuration

The `OrchardCore.ReverseProxy` module allows you to use configuration values from `appsettings.json` to configure reverse proxy settings by calling the `ConfigureReverseProxySettings()` extension method on `OrchardCoreBuilder` when initializing the app.

In your `Program.cs` or startup code, call the extension method:

```csharp
builder.Services.AddOrchardCore()
    .ConfigureReverseProxySettings()
    // ... other configuration
```

Add the following section to your `appsettings.json`:

```json
{
  "OrchardCore_ReverseProxy": {
    "ForwardedHeaders": "XForwardedFor, XForwardedHost, XForwardedProto",
    "KnownNetworks": ["192.168.1.0/24"],
    "KnownProxies": ["192.168.1.200", "192.168.1.201"]
  }
}
```

## Configuration Options

| Setting | Description | Example Values |
|---------|-------------|----------------|
| `ForwardedHeaders` | Specifies which headers to forward | `None`, `XForwardedFor`, `XForwardedHost`, `XForwardedProto`, `XForwardedPrefix`, `All`, or a combination (comma-separated) |
| `KnownNetworks` | Array of known proxy networks in CIDR notation | `["192.168.1.0/24", "10.0.0.0/8"]` |
| `KnownProxies` | Array of known proxy IP addresses | `["192.168.1.200", "192.168.1.201"]` |

!!! warning
    The `KnownNetworks` values must be specified in CIDR notation (e.g., `192.168.1.0/24`).
    
    The `KnownProxies` values must be valid IP addresses.

## Configuration Precedence

When `ConfigureReverseProxySettings()` is called, settings from the configuration file are **merged** with settings configured through the admin UI:

- Configuration file values take **precedence** over admin UI settings for the same properties
- If a property is not specified in the configuration file, the admin UI value is used
- The admin UI will display a warning when configuration file settings are active

**Scenario 1: Admin UI Only**
- `ConfigureReverseProxySettings()` is NOT called
- All settings are managed through the admin UI
- No configuration file override

**Scenario 2: Configuration File Override**
- `ConfigureReverseProxySettings()` is called
- `OrchardCore_ReverseProxy` section exists in appsettings.json
- Configuration file values override admin UI values
- Admin UI shows a warning about the override

**Scenario 3: Partial Override**
- `ConfigureReverseProxySettings()` is called
- Only some settings specified in appsettings.json (e.g., only `KnownProxies`)
- Configuration file values are merged with admin UI values
- Specified values from config file take precedence

## Security Considerations

!!! danger
    Improperly configured reverse proxy settings can expose your application to security risks, including IP spoofing and header injection attacks.

When configuring reverse proxy settings:

1. **Always configure KnownProxies and KnownNetworks** when using forwarded headers in production
2. **Only trust headers from known proxies** to prevent header spoofing
3. **Use CIDR notation carefully** to avoid exposing your application to untrusted networks
4. **Test your configuration** to ensure headers are forwarded correctly

## Multi-Tenant Considerations

- Admin UI settings are **tenant-specific**
- Configuration file settings apply to **all tenants** when specified in the main appsettings.json
- For tenant-specific configuration file settings, use the tenant's appsettings.json file in `App_Data/Sites/{tenant}/appsettings.json`

## Additional Resources

For more information, please refer to:

- [Configuration](../Configuration/README.md)
- [Microsoft ASP.NET Core Forwarded Headers Documentation](https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/proxy-load-balancer)
