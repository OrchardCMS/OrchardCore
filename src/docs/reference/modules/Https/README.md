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
| `RequireHttpsPermanent`         | Boolean | Whether to use a permanent (301) redirect for HTTPS redirection.                             |
| `SslPort`                       | Integer | The port number for SSL connections.                                                         |

### `StrictTransportSecurityMode` values

| Value | Behavior |
|-------|----------|
| `Disabled` | Always disables HSTS headers, regardless of environment. |
| `Enabled` | Always enables HSTS headers, regardless of environment. |
| `FromConfiguration` | Enables HSTS automatically in the `Production` environment and disables it in other environments. |
