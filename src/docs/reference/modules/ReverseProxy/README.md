# Reverse Proxy (`OrchardCore.ReverseProxy`)

Enables configuration of hosting scenarios with a reverse proxy, like which HTTP headers to forward.

## Reverse Proxy Settings Configuration

The `OrchardCore.ReverseProxy` module allows the user to use configuration values to override the settings configured from the admin area by calling the `ConfigureReverseProxySettings()` extension method on `OrchardCoreBuilder` when initializing the app.

The following configuration values can be customized:

```json
    "OrchardCore_ReverseProxy": {
      "ForwardedHeaders": "None"
    }
```

For more information please refer to [Configuration](../../core/Configuration/README.md).