# Health Checks (`OrchardCore.HealthChecks`)

This module enables the health checks feature from ASP.NET Core.

## Health check endpoint

The health check endpoint is available at `/health/live` for each  tenant that needs to be checked.

## Extending health checks

More information about health checks in ASP.NET Core can be found here: <https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/health-checks>

## Health Checks Configuration

The following configurations are available and can be customized:

```json
    "OrchardCore_HealthChecks": {
      "Url": "/health/live",
      "ShowDetails": true
    },
```

The supported extensions described as following:

| Extension | Description |
| --- | --- |
| `Url` | The relative URL of the health checks endpoint |
| `ShowDetails` | Whether or not to display a detailed information about each health check provider registered in the system, this including: name, description and status. |

