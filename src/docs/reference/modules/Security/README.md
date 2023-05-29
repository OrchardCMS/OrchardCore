# Security (`OrchardCore.Security`)

This module adds `HTTP` headers to follow security best practices.

## Security Settings

Enabling the `OrchardCore.Security` module will allow the user to set the following settings:

| Setting | Description |
| --- | --- |
| `ContentSecurityPolicy` | Gets or sets the `Content-Security-Policy` HTTP header. |
| `ContentTypeOptions` | Gets or sets the `X-Content-Type-Options` HTTP header. |
| `PermissionsPolicy` | Gets or sets the `Permissions-Policy` HTTP header. |
| `ReferrerPolicy` | Gets or sets the `Referrer-Policy` HTTP header. |

## Security Settings Configuration

The `OrchardCore.Security` module allows the user to use configuration values to override the `AdminSettings` by calling `ConfigureSecuritySettings()` extension method.

The following configuration values can be customized:

```json
    "OrchardCore_Security": {
      "ContentSecurityPolicy": {},
      "PermissionsPolicy": { "fullscreen": "self" },
      "ReferrerPolicy": "no-referrer"
    }
```

For more information please refer to [Configuration](../../core/Configuration/README.md).

## Video

<iframe width="560" height="315" src="https://www.youtube-nocookie.com/embed/nYfNq8sTIAg" title="YouTube video player" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture" allowfullscreen></iframe>