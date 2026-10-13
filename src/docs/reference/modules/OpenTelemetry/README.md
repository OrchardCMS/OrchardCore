# OpenTelemetry (`OrchardCore.OpenTelemetry`)

`OrchardCore.OpenTelemetry` collects the traces, metrics and logs of an Orchard Core application with [OpenTelemetry](https://opentelemetry.io/), and exports them over OTLP to any compatible backend: the .NET Aspire dashboard, Jaeger, Prometheus, Grafana, Azure Monitor, Datadog, Honeycomb, Elastic, and many more.

The telemetry is multi-tenant aware: spans, log records and HTTP server metrics are tagged with the name of their tenant, and resource attributes identify each instance of the application, so that the behavior of every tenant and every instance of a web farm can be told apart.

This video shows how to enable it, the traces, metrics and logs of two instances serving two tenants in the .NET Aspire dashboard, and how to configure it.

<video controls preload="metadata" width="100%" poster="videos/opentelemetry.jpg">
    <source src="videos/opentelemetry.mp4" type="video/mp4">
    <track kind="captions" src="videos/opentelemetry.vtt" srclang="en" label="English">
    Your browser does not support embedded videos. <a href="videos/opentelemetry.mp4">Download the video</a>.
</video>

## Enabling OpenTelemetry

Reference the `OrchardCore.OpenTelemetry` package and call `AddOpenTelemetry()` on the Orchard Core builder:

```csharp
builder.Services
    .AddOrchardCms()
    .AddOpenTelemetry();
```

The `OrchardCore.Cms.Web` application of the repository already does it, so that setting an OTLP endpoint is enough to export its telemetry.

The telemetry is exported over OTLP once an endpoint is configured, either in the `OrchardCore:OpenTelemetry:Otlp` section or with the standard `OTEL_EXPORTER_OTLP_ENDPOINT` configuration value, e.g. as an environment variable:

```bash
OTEL_EXPORTER_OTLP_ENDPOINT=http://localhost:4317
```

Other exporters or instrumentations can be added with the optional callback, which receives the `OpenTelemetryBuilder`:

```csharp
builder.Services
    .AddOrchardCms()
    .AddOpenTelemetry(openTelemetry => openTelemetry
        .WithTracing(tracing => tracing.AddSqlClientInstrumentation()));
```

### Why it isn't a tenant feature

OpenTelemetry is configured for the whole application rather than enabled per tenant like a module feature, because what it observes lives outside of any tenant:

- The listeners of traces and metrics are global to the process, so a pipeline built by each tenant would collect the telemetry of all the tenants, and export it once per tenant.
- The HTTP request is traced before its tenant is resolved, the service container of a tenant is built before the tenant has any service, and the .NET runtime metrics belong to the process.
- A tenant is rebuilt when its settings or features change, which would reset its metrics and drop the telemetry not yet exported.
- Where the telemetry is exported is a decision of whoever hosts the application, not of the administrator of a tenant.

Instead, the telemetry of every tenant is tagged with its name, so that it can be filtered by tenant in any backend.

### Trying it locally

The [.NET Aspire dashboard](https://learn.microsoft.com/dotnet/aspire/fundamentals/dashboard/standalone) is a free OTLP backend showing traces, metrics and structured logs. Start it with Docker:

```bash
docker run --rm -d -p 18888:18888 -p 4317:18889 -e DOTNET_DASHBOARD_UNSECURED_ALLOW_ANONYMOUS=true --name aspire-dashboard mcr.microsoft.com/dotnet/aspire-dashboard:latest
```

Then run the application with `OTEL_EXPORTER_OTLP_ENDPOINT=http://localhost:4317`, and browse `http://localhost:18888`.

## Configuration

The options are read from the `OrchardCore:OpenTelemetry` section of the application configuration:

```json
{
  "OrchardCore": {
    "OpenTelemetry": {
      "Enabled": true,
      "ServiceName": "MyCms",
      "ServiceInstanceId": "web-1",
      "Tracing": {
        "Enabled": true,
        "SamplingRatio": 0.25,
        "Sources": [ "MyCompany.MyModule" ]
      },
      "Metrics": {
        "Enabled": true,
        "IncludeTenantName": true,
        "Meters": [ "MyCompany.MyModule" ]
      },
      "Logging": {
        "Enabled": true
      },
      "Otlp": {
        "Endpoint": "http://localhost:4317",
        "Protocol": "Grpc"
      }
    }
  }
}
```

| Setting | Description |
| --- | --- |
| `Enabled` | Whether telemetry is collected and exported. Defaults to `true`. |
| `ServiceName` | The `service.name` resource attribute. Defaults to the application name. |
| `ServiceInstanceId` | The `service.instance.id` resource attribute identifying this instance of the application. Defaults to the machine name followed by the process id, which is unique for each running instance. |
| `Tracing:Enabled` | Whether traces are collected. Defaults to `true`. |
| `Tracing:SamplingRatio` | The ratio, from `0` to `1`, of the traces that are sampled. A trace started by a sampled parent, e.g. in another service, is always sampled. When not set, all traces are sampled. |
| `Tracing:Sources` | The names of additional `ActivitySource` to collect traces from. |
| `Metrics:Enabled` | Whether metrics are collected. Defaults to `true`. |
| `Metrics:IncludeTenantName` | Whether the HTTP server metrics are tagged with the name of the tenant that served the request. Defaults to `true`. With many tenants, disable it to limit the number of metric series. |
| `Metrics:Meters` | The names of additional `Meter` to collect metrics from. |
| `Logging:Enabled` | Whether logs are collected. Defaults to `true`. The log levels of the `Logging` section apply. |
| `Otlp:Endpoint` | The endpoint of the OTLP receiver, e.g. `http://localhost:4317` for gRPC or `http://localhost:4318` for HTTP. |
| `Otlp:Protocol` | `Grpc` (default) or `HttpProtobuf`. |

The standard `OTEL_EXPORTER_OTLP_ENDPOINT`, `OTEL_EXPORTER_OTLP_PROTOCOL` and `OTEL_EXPORTER_OTLP_HEADERS` [configuration values](https://opentelemetry.io/docs/languages/sdk-configuration/otlp-exporter/) are also supported, the latter to pass an API key to a hosted backend, as well as `OTEL_METRIC_EXPORT_INTERVAL`. The `Otlp` section takes precedence over them. `OTEL_SERVICE_NAME` and `OTEL_RESOURCE_ATTRIBUTES` take precedence over the values of the section.

## Collected telemetry

### Resource

Each signal carries resource attributes identifying the application and its instance: `service.name`, `service.version`, `service.instance.id`, `host.name`, `process.pid`, `process.runtime.name`, `process.runtime.version`, `deployment.environment.name` (the hosting environment) and `orchardcore.version`.

### Traces

| Source | Spans |
| --- | --- |
| Orchard Core | Building the service container (`Build tenant container`) and the request pipeline (`Build tenant pipeline`) of a tenant, activating a tenant (`Activate tenant`), running a background task (`Run background task {name}`), executing a recipe (`Execute recipe {name}`) and each of its steps (`Execute recipe step {name}`). |
| ASP.NET Core | One span for each HTTP request, tagged with the tenant that served it. |
| HttpClient | One span for each outgoing HTTP request. |
| Npgsql, MySqlConnector | The database commands sent to PostgreSQL and MySQL. For SQL Server, add the `OpenTelemetry.Instrumentation.SqlClient` instrumentation with the configuration callback. |

Every span started in the scope of a tenant, including the spans of other libraries, is tagged with `orchardcore.tenant.name`. A background task run is the root of its own trace.

### Metrics

| Metric | Description |
| --- | --- |
| `orchardcore.tenants` | Number of tenants, by `orchardcore.tenant.state`. A gauge, since every instance reports the same tenants. |
| `orchardcore.tenants.loaded` | Number of tenants whose service container is built and loaded in memory, which adds up across instances. |
| `orchardcore.tenant.container.duration` | Duration of building the service container of a tenant. |
| `orchardcore.tenant.pipeline.duration` | Duration of building the request pipeline of a tenant. |
| `orchardcore.tenant.activation.duration` | Duration of activating a tenant. |
| `orchardcore.tenant.releases` | Number of times a tenant was released, by `orchardcore.tenant.release.reason` (`reload`, `release` or `remove`). |
| `orchardcore.background_task.duration` | Duration of running a background task, by `orchardcore.background_task.name`. |

The Orchard Core metrics are tagged with `orchardcore.tenant.name`, and with `error.type` when the operation failed.

The metrics built in .NET are also collected: HTTP server and client (`http.server.request.duration`, `http.client.request.duration`, ...), Kestrel, routing, authentication, authorization and identity, and the .NET runtime (`dotnet.gc.*`, `dotnet.process.cpu.time`, `dotnet.process.memory.working_set`, `dotnet.thread_pool.*`, `dotnet.exceptions`, ...), giving the health of each instance.

### Logs

The logs written through `ILogger` are exported with their structured attributes and scopes, and those written in the scope of a tenant carry `orchardcore.tenant.name`. The OpenTelemetry logger is added next to the other logging providers, such as NLog or Serilog.

## Using an existing OpenTelemetry configuration

When the application already configures OpenTelemetry, e.g. with the `ServiceDefaults` project of a .NET Aspire solution, add the Orchard Core instrumentation to it instead of calling `AddOpenTelemetry()` on the Orchard Core builder:

```csharp
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing.AddOrchardCoreInstrumentation())
    .WithMetrics(metrics => metrics.AddOrchardCoreInstrumentation())
    .WithLogging(logging => logging.AddOrchardCoreInstrumentation());
```

Add the Orchard Core instrumentation before the exporters, so that the telemetry is tagged with the tenant before being exported.

## Emitting telemetry from a module

`OrchardCoreTelemetry.ActivitySource` and `OrchardCoreTelemetry.Meter`, from `OrchardCore.Abstractions`, emit the Orchard Core traces and metrics, and modules can use them for their own:

```csharp
using var activity = OrchardCoreTelemetry.ActivitySource.StartActivity("Import products");

try
{
    await ImportProductsAsync();
}
catch (Exception ex)
{
    OrchardCoreTelemetry.RecordException(activity, ex);

    throw;
}
```

A module can also use its own `ActivitySource` or `Meter`, and add its name to the `Tracing:Sources` or `Metrics:Meters` settings.

As these sources only depend on `System.Diagnostics`, the Orchard Core telemetry can also be watched without OpenTelemetry, e.g. with `dotnet-counters monitor --counters OrchardCore,System.Runtime`.
