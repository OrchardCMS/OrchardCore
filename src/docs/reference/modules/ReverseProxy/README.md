# Reverse Proxy (`OrchardCore.ReverseProxy`)

Enables configuration of hosting scenarios with a reverse proxy, like which HTTP headers to forward.

## Reverse Proxy Settings Configuration

The `OrchardCore.ReverseProxy` module allows the user to use configuration values to override the settings configured from the admin area by calling the `ConfigureReverseProxySettings()` extension method on `OrchardCoreBuilder` when initializing the app.

The following configuration values can be customized:

```json
{
  "OrchardCore_ReverseProxy": {
    "ForwardedHeaders": "None",
    "KnownNetworks": ["192.168.1.100", "192.168.1.101"],
    "KnownProxies": ["192.168.1.200", "192.168.1.201"],
  }
}
```

For more information please refer to [Configuration](../Configuration/README.md).
