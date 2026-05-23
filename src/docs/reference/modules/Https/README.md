# HTTPS (`OrchardCore.Https`)

The module will ensure HTTPS is used when accessing the website. You can force HTTPS on all pages, enable HSTS, and configure the HTTPS port.

## Recipe Configuration

HTTPS settings can be configured using the `Settings` recipe step:

```json
{
  "steps": [
    {
      "name": "settings",
      "HttpsSettings": {
        "EnableStrictTransportSecurity": true,
        "RequireHttps": true,
        "RequireHttpsPermanent": false,
        "SslPort": 443
      }
    }
  ]
}
```

| Property                        | Type    | Description                                                      |
|---------------------------------|---------|------------------------------------------------------------------|
| `EnableStrictTransportSecurity` | Boolean | Whether to enable HTTP Strict Transport Security (HSTS) headers. |
| `RequireHttps`                  | Boolean | Whether to require HTTPS for all requests.                       |
| `RequireHttpsPermanent`         | Boolean | Whether to use a permanent (301) redirect for HTTPS redirection. |
| `SslPort`                       | Integer | The port number for SSL connections.                             |
