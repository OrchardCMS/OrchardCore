# Health Checks (`OrchardCore.HealthChecks`)

The Health Checks module exposes the health checks registered with ASP.NET Core through a tenant endpoint.
It registers the health check infrastructure and a response writer, but it does not add a health check by itself.
When no other enabled feature registers a check, the endpoint reports the tenant as healthy.

The feature has no administration UI or site settings.
Enable and configure it independently for every tenant that needs to be monitored.

## Enable the feature

Enable **Health Checks** from **Tools** > **Features** in the tenant's administration area, or enable `OrchardCore.HealthChecks` from a recipe:

```json
{
  "name": "feature",
  "enable": [
    "OrchardCore.HealthChecks"
  ],
  "disable": []
}
```

## Health check endpoint

The default endpoint is:

```text
/health/live
```

The endpoint belongs to the tenant pipeline.
Use the tenant's host and URL prefix when addressing it:

```text
https://example.com/health/live
https://example.com/customer-a/health/live
https://customer-a.example.com/health/live
```

Only tenants with the feature enabled expose the endpoint.
Each request runs all health checks registered in that tenant's service container; the module does not filter registrations by tag.

For example:

```bash
curl --include https://example.com/health/live
```

## Configuration

Configure the endpoint through the tenant-aware `OrchardCore_HealthChecks` configuration section:

```json
{
  "OrchardCore_HealthChecks": {
    "Url": "/health/live",
    "ShowDetails": false
  }
}
```

| Setting | Default | Description |
|---------|---------|-------------|
| `Url` | `/health/live` | The route mapped inside each enabled tenant. A tenant URL prefix is applied before this route. |
| `ShowDetails` | `false` | Returns the default compact ASP.NET Core response when `false`, or Orchard Core's JSON response when `true`. |

The same settings can be supplied through environment variables:

```text
OrchardCore__OrchardCore_HealthChecks__Url=/health/live
OrchardCore__OrchardCore_HealthChecks__ShowDetails=false
```

To target a named tenant, include its shell name:

```text
OrchardCore__CustomerA__OrchardCore_HealthChecks__Url=/health/ready
```

See [Configuration](../Configuration/README.md#ishellconfiguration-via-environment-variables) for configuration sources and tenant-specific environment variable patterns.
The route and response writer are selected when the tenant pipeline is built, so reload the tenant or restart the application after changing these settings.

## Response and status behavior

### Compact response

With the default `ShowDetails: false`, the endpoint uses the ASP.NET Core health check response:

| Aggregate status | HTTP status |
|------------------|-------------|
| `Healthy` | `200 OK` |
| `Degraded` | `200 OK` |
| `Unhealthy` | `503 Service Unavailable` |

The response contains only the aggregate status, such as:

```text
Healthy
```

This mode is suitable for monitors that determine availability from the HTTP status code.

### Detailed response

With `ShowDetails: true`, the endpoint returns JSON containing the aggregate status, total duration, and the name, status, and description of every registered check:

```json
{
  "Status": "Unhealthy",
  "Duration": "00:00:00.0420000",
  "HealthChecks": [
    {
      "Name": "Dependency",
      "Status": "Unhealthy",
      "Description": "The dependency did not respond."
    }
  ]
}
```

In detailed mode, `Healthy`, `Degraded`, and `Unhealthy` all return `200 OK`.
A monitor must parse the top-level `Status` property instead of relying on the HTTP status code.
The response does not include a check's exception, data dictionary, or tags.

Both response modes disable response caching.

!!! warning
    The endpoint does not require authentication. Detailed descriptions can disclose dependency names or operational information, so enable `ShowDetails` only when the endpoint is restricted to trusted monitoring clients.

## Checks contributed by Orchard Core features

The Health Checks feature registers the endpoint but no check implementation.
Other enabled Orchard Core features can contribute checks:

| Feature | Registration | Behavior |
|---------|--------------|----------|
| `OrchardCore.Redis` | `Redis Health Check` | Connects to Redis and verifies that it responds to a ping. |
| `OrchardCore.Sms` | `SMS Health Check` | Validates the configured Twilio credentials against the Twilio service. |

These checks are registered only when both their owning feature and `OrchardCore.HealthChecks` are enabled for the tenant.
Because a probe executes every registered check, account for external calls and timeouts when choosing the polling interval.

## Add a custom health check

Health checks use the standard ASP.NET Core registration APIs.
Register an `IHealthCheck` from a feature that is enabled in the same tenant:

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using OrchardCore.Modules;

namespace MyModule;

[RequireFeatures("OrchardCore.HealthChecks")]
public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services
            .AddHealthChecks()
            .AddCheck<DependencyHealthCheck>("Dependency");
    }
}

public sealed class DependencyHealthCheck : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var result = HealthCheckResult.Healthy("The dependency is available.");

        return Task.FromResult(result);
    }
}
```

`RequireFeatures` prevents the startup registration from loading when the Health Checks feature is disabled.
Alternatively, declare `OrchardCore.HealthChecks` as a dependency of the custom feature, so enabling the custom feature also enables the endpoint.

The Orchard Core endpoint runs every registration.
Tags supplied to `AddCheck` remain available to ASP.NET Core, but this module does not use them to create separate liveness and readiness endpoints.

For check registration options and `IHealthCheck` implementation patterns, see [Health checks in ASP.NET Core](https://learn.microsoft.com/aspnet/core/host-and-deploy/health-checks).

## Replace the detailed response writer

When `ShowDetails` is enabled, the module resolves `IHealthChecksResponseWriter` from the tenant service container.
A feature that depends on `OrchardCore.HealthChecks` can replace the default scoped implementation:

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OrchardCore.HealthChecks.Services;

services.Replace(
    ServiceDescriptor.Scoped<IHealthChecksResponseWriter, CustomHealthChecksResponseWriter>());
```

Implement `WriteResponseAsync(HttpContext, HealthReport)` to produce the required response.
This extension point affects only detailed mode; compact mode continues to use the ASP.NET Core default response writer.

## Monitoring and security

- Use compact mode when a load balancer, container platform, or uptime monitor expects an unhealthy probe to return a non-success status.
- In detailed mode, configure the monitor to parse the JSON `Status`.
- Include the tenant host and URL prefix in the probe URL.
- Restrict the endpoint with network policy, a reverse proxy, or another perimeter control when it should not be public.
- An endpoint policy from the [Rate Limits](../RateLimits/README.md) module can limit repeated requests to the health check path.

The module exposes one configurable endpoint that runs all registered checks.
If an application needs separate liveness and readiness semantics, add appropriately filtered endpoints in application or module code instead of treating this endpoint as both without considering its registrations.

## Troubleshooting

| Symptom | What to check |
|---------|---------------|
| `404 Not Found` | Confirm that `OrchardCore.HealthChecks` is enabled for the requested tenant, use that tenant's host and URL prefix, and verify the configured `Url`. |
| An unhealthy detailed response returns `200 OK` | This is the expected behavior when `ShowDetails` is enabled. Parse the JSON `Status`, or disable detailed mode to receive `503 Service Unavailable`. |
| The response is `Healthy` but lists no checks | The feature provides the endpoint but no check implementation. Enable a feature that contributes a check or register a custom `IHealthCheck`. |
| A custom check is missing | Confirm that its feature is enabled in the same tenant and that its startup registration loads only after or together with `OrchardCore.HealthChecks`. |
| Configuration changes have no effect | Reload the tenant or restart the application so the tenant pipeline and endpoint are rebuilt. |
