# HTTPS (`OrchardCore.Https`)

The module will ensure HTTPS is used when accessing the website. You can force HTTPS on all pages, choose how HSTS is applied, and configure the HTTPS port.

## Recipe Configuration

HTTPS settings can be configured using the `Settings` recipe step:

```json
{
  "steps": [
    {
      "name": "settings",
      "HttpsSettings": {
        "StrictTransportSecurityMode": "Disabled",
        "RequireHttps": true,
        "RequireHttpsPermanent": false,
        "SslPort": 443
      }
    }
  ]
}
```

| Property                        | Type    | Description                                                                                  |
|---------------------------------|---------|----------------------------------------------------------------------------------------------|
| `StrictTransportSecurityMode`   | String  | Whether HTTP Strict Transport Security (HSTS) is `Disabled` by default, `Enabled`, or `FromConfiguration` (enabled in Production, disabled otherwise). |
| `RequireHttps`                  | Boolean | Whether to require HTTPS for all requests.                                                   |
| `RequireHttpsPermanent`         | Boolean | Whether to use a permanent (308) redirect for HTTPS redirection.                             |
| `SslPort`                       | Integer | The port number for SSL connections.                                                         |

### `StrictTransportSecurityMode` values

| Value | Behavior |
|-------|----------|
| `Disabled` | Always disables HSTS headers, regardless of environment. |
| `Enabled` | Always enables HSTS headers, regardless of environment. |
| `FromConfiguration` | Enables HSTS automatically in the `Production` environment and disables it in other environments. |

## Remote management

The [typed `https` settings section](../../api/settings/README.md#https-section)
provides permission-checked reads, schema discovery and partial updates through
OpenAPI, Pomi and MCP. It uses `ManageHttps` and shares validation with the admin
editor. Updates require HTTPS and reload the tenant only when settings change.
An explicit port must be between 1 and 65535; null restores automatic detection.
The port selects the redirect destination and does not configure a TLS listener.
