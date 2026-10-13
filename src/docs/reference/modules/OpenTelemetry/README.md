# OpenTelemetry (`OrchardCore.OpenTelemetry`)

`OrchardCore.OpenTelemetry` collects the traces, metrics and logs of an Orchard Core application with [OpenTelemetry](https://opentelemetry.io/), and exports them over OTLP to any compatible backend: the Aspire dashboard, Jaeger, Prometheus, Grafana, Azure Monitor, Datadog, Honeycomb, Elastic, and many more.

The telemetry is multi-tenant aware: spans, log records and HTTP server metrics are tagged with the name of their tenant, and resource attributes identify each instance of the application, so that the behavior of every tenant and every instance of a web farm can be told apart.

This video shows how to enable it, the traces, metrics and logs of two instances serving two tenants in the Aspire dashboard, and how to configure it.

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
        .WithTracing(tracing => tracing.AddConsoleExporter()));
```

### Why it isn't a tenant feature

OpenTelemetry is configured for the whole application rather than enabled per tenant like a module feature, because what it observes lives outside of any tenant:

- The listeners of traces and metrics are global to the process, so a pipeline built by each tenant would collect the telemetry of all the tenants, and export it once per tenant.
- The HTTP request is traced before its tenant is resolved, the service container of a tenant is built before the tenant has any service, and the .NET runtime metrics belong to the process.
- A tenant is rebuilt when its settings or features change, which would reset its metrics and drop the telemetry not yet exported.
- Where the telemetry is exported is a decision of whoever hosts the application, not of the administrator of a tenant.

Instead, the telemetry of every tenant is tagged with its name, so that it can be filtered by tenant in any backend.

### Trying it locally

The `OrchardCore.AspireHost` project of the repository runs `OrchardCore.Cms.Web` with the [Aspire dashboard](https://learn.microsoft.com/dotnet/aspire/fundamentals/dashboard/overview), which shows its traces, metrics and structured logs. Set it as the startup project and run it, or run `dotnet run --project src/OrchardCore.AspireHost`, then open the dashboard from the link the host prints. The host configures the OTLP endpoint of the application, so nothing else is needed. It also starts a ClamAV container for the antivirus feature, which requires Docker or another container runtime.

### Sending it to Grafana

Grafana receives the three signals over OTLP, and stores the traces in Tempo, the metrics in Prometheus or Mimir, and the logs in Loki.

To try it locally, the `grafana/otel-lgtm` image runs an OpenTelemetry Collector with Grafana, Tempo, Prometheus and Loki:

```bash
docker run --rm -d -p 3000:3000 -p 4317:4317 -p 4318:4318 --name lgtm grafana/otel-lgtm
```

Then run the application with `OTEL_EXPORTER_OTLP_ENDPOINT=http://localhost:4317`, open Grafana at `http://localhost:3000`, and use its Explore page.

With Grafana Cloud, the telemetry is sent over HTTP to the OTLP endpoint of the stack, shown in its OpenTelemetry configuration in the Grafana Cloud portal, with the instance ID of the stack and an access token encoded in Base64 as `{instance ID}:{token}`:

```bash
OTEL_EXPORTER_OTLP_PROTOCOL=http/protobuf
OTEL_EXPORTER_OTLP_ENDPOINT=https://otlp-gateway-{region}.grafana.net/otlp
OTEL_EXPORTER_OTLP_HEADERS=Authorization=Basic%20{Base64 credentials}
```

Prometheus and Mimir store the metrics with their names and attributes converted: dots become underscores, and the unit is added as a suffix, so `orchardcore.shape.render.duration` becomes `orchardcore_shape_render_duration_seconds`, with an `orchardcore_shape_type` label, and the `service.name` and `service.instance.id` resource attributes become the `job` and `instance` labels. For instance:

| Question | Query |
| --- | --- |
| Which shapes are the slowest? | `topk(10, histogram_quantile(0.95, sum by (le, orchardcore_shape_type) (rate(orchardcore_shape_render_duration_seconds_bucket[5m]))))` |
| Which routes run the most database operations? | `topk(10, histogram_quantile(0.95, sum by (le, http_route) (rate(orchardcore_request_db_operations_bucket[5m]))))` |
| How fast does each tenant respond? | `histogram_quantile(0.95, sum by (le, orchardcore_tenant_name) (rate(http_server_request_duration_seconds_bucket[5m])))` |
| How much memory does each instance use? | `sum by (instance) (dotnet_process_memory_working_set_bytes)` |

In Tempo, a TraceQL query such as `{ span.orchardcore.tenant.name = "Default" && duration > 500ms }` finds the slow requests of a tenant, and `{ name =~ "Render shape.*" }` the slow shapes.

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
        "Sources": [ "MyCompany.MyModule" ],
        "IncludeDatabaseQueryText": true,
        "SlowShapeThreshold": "00:00:00.050"
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
| `Tracing:IncludeClientLibraries` | Whether the traces of the client libraries used by Orchard Core modules are collected: the Elasticsearch client, MailKit for SMTP, and the Azure SDK clients. Defaults to `true`. See [Client libraries](#client-libraries). |
| `Tracing:SlowShapeThreshold` | The duration from which the rendering of a shape is traced with its own span. Defaults to `00:00:00.050` (50 milliseconds), `00:00:00` traces every shape. |
| `Tracing:IncludeDatabaseQueryText` | Whether the spans of the database operations include their SQL text, in `db.query.text`. Defaults to `true`. The commands run by YesSql are parameterized, so their text holds no values. |
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

### Spans

The spans of Orchard Core come from the `OrchardCore` activity source:

| Span | Kind | Attributes |
| --- | --- | --- |
| `Build tenant container` | Internal | `orchardcore.tenant.name` |
| `Build tenant pipeline` | Internal | `orchardcore.tenant.name` |
| `Activate tenant` | Internal | `orchardcore.tenant.name` |
| `Run background task {task}` | Internal | `orchardcore.tenant.name`, `orchardcore.background_task.name` |
| `Execute recipe {recipe}` | Internal | `orchardcore.tenant.name`, `orchardcore.recipe.name` |
| `Execute recipe step {step}` | Internal | `orchardcore.tenant.name`, `orchardcore.recipe.name`, `orchardcore.recipe.step.name` |
| `Render shape {type}` | Internal | `orchardcore.tenant.name`, `orchardcore.shape.type`, `orchardcore.shape.binding` (the alternate that rendered it), `orchardcore.shape.binding.source` (e.g. the path of its template) |

Each database command run through YesSql is traced too, whatever the database provider (SQLite, SQL Server, PostgreSQL, MySQL), with a `Client` span named after its operation and table, e.g. `SELECT ContentItemIndex`, and the `db.system.name`, `db.namespace`, `db.operation.name`, `db.collection.name`, `db.query.text` and `orchardcore.tenant.name` attributes. Its duration is the one of the execution, until the first results are available.

A `Render shape` span is only emitted for a shape whose rendering took longer than `Tracing:SlowShapeThreshold`. It's created once the rendering is known to be slow, so it's a child of the request rather than of the shape that rendered it.

A failed operation has the `Error` status, the `error.type` attribute and an `exception` event. A background task run is the root of its own trace.

The spans of ASP.NET Core (one for each HTTP request, also tagged with the `orchardcore.tenant.name` of the tenant that served it, the number of database operations it ran, `orchardcore.db.operations`, and their total duration in seconds, `orchardcore.db.duration`) and of HttpClient (one for each outgoing HTTP request) are collected too, as well as the ones of the client libraries listed below.

Every span started in the scope of a tenant, including the spans of other libraries, is tagged with `orchardcore.tenant.name`.

### Metrics

The metrics of Orchard Core come from the `OrchardCore` meter:

| Instrument | Type | Unit | Attributes | Description |
| --- | --- | --- | --- | --- |
| `orchardcore.tenants` | Gauge | `{tenant}` | `orchardcore.tenant.state` | Number of tenants by state. A gauge, since every instance reports the same tenants. |
| `orchardcore.tenants.loaded` | UpDownCounter | `{tenant}` | | Number of tenants whose service container is built and loaded in memory, which adds up across instances. |
| `orchardcore.tenant.container.duration` | Histogram | `s` | `orchardcore.tenant.name`, `error.type` | Duration of building the service container of a tenant. |
| `orchardcore.tenant.pipeline.duration` | Histogram | `s` | `orchardcore.tenant.name`, `error.type` | Duration of building the request pipeline of a tenant. |
| `orchardcore.tenant.activation.duration` | Histogram | `s` | `orchardcore.tenant.name`, `error.type` | Duration of activating a tenant. |
| `orchardcore.tenant.releases` | Counter | `{release}` | `orchardcore.tenant.name`, `orchardcore.tenant.release.reason` | Number of times a tenant was released: `reload`, `release` or `remove`. |
| `orchardcore.background_task.duration` | Histogram | `s` | `orchardcore.tenant.name`, `orchardcore.background_task.name`, `error.type` | Duration of running a background task. |
| `orchardcore.shape.render.duration` | Histogram | `s` | `orchardcore.tenant.name`, `orchardcore.shape.type` | Duration of rendering a shape, including the shapes it renders. |
| `orchardcore.shape.cache.lookups` | Counter | `{lookup}` | `orchardcore.tenant.name`, `orchardcore.shape.type`, `orchardcore.shape.cache.result` | Number of lookups of the content of a cached shape: `hit` or `miss`. Only recorded when the Dynamic Cache feature is enabled. |
| `db.client.operation.duration` | Histogram | `s` | `db.system.name`, `db.operation.name`, `orchardcore.tenant.name`, `error.type` | Duration of a database command, as defined by the OpenTelemetry semantic conventions. |
| `orchardcore.request.db.operations` | Histogram | `{operation}` | `orchardcore.tenant.name`, `http.route` | Number of database operations run by an HTTP request. A route running many of them reveals an N+1 query pattern. |
| `orchardcore.request.db.duration` | Histogram | `s` | `orchardcore.tenant.name`, `http.route` | Total duration of the database operations run by an HTTP request. |

`error.type` is only set when the operation failed.

The metrics built in .NET are collected too, from these meters:

| Meter | Metrics |
| --- | --- |
| `Microsoft.AspNetCore.Hosting` | `http.server.request.duration`, also tagged with `orchardcore.tenant.name` (see `Metrics:IncludeTenantName`), and `http.server.active_requests`. |
| `Microsoft.AspNetCore.Server.Kestrel`, `Microsoft.AspNetCore.MemoryPool`, `Microsoft.AspNetCore.Routing`, `Microsoft.AspNetCore.Diagnostics`, `Microsoft.AspNetCore.RateLimiting`, `Microsoft.AspNetCore.Http.Connections` | Connections and memory pools, route matching, handled exceptions, rate limiting, and SignalR connections. |
| `Microsoft.AspNetCore.Authentication`, `Microsoft.AspNetCore.Authorization`, `Microsoft.AspNetCore.Identity` | Authentications, authorization attempts, sign-ins and user operations. |
| `System.Net.Http`, `System.Net.NameResolution` | `http.client.request.duration`, connections and DNS lookups. |
| `System.Runtime` | The health of each instance: `dotnet.gc.*`, `dotnet.process.cpu.time`, `dotnet.process.memory.working_set`, `dotnet.thread_pool.*`, `dotnet.exceptions`... |

### Client libraries

OpenTelemetry only collects the activity sources and meters it's told about: the telemetry a library emits itself is collected once its source or meter is added. These ones, used by Orchard Core modules, are collected by default:

| Library | Used by | Telemetry |
| --- | --- | --- |
| Elasticsearch client | Elasticsearch | `Elastic.Transport` spans. |
| MailKit | SMTP email | `MailKit.Net.SmtpClient` spans. |
| Azure SDK | Azure Blob Storage, Key Vault, Communication Services, AI Search | `Azure.*` spans. Their activity sources are experimental, so Orchard Core enables them with the `Azure.Experimental.EnableActivitySource` switch, unless the application already sets it or the `AZURE_EXPERIMENTAL_ENABLE_ACTIVITY_SOURCE` variable. |
| Npgsql, MySqlConnector | PostgreSQL and MySQL databases | Connection pool metrics. Their own spans aren't collected, as they would duplicate the database spans of Orchard Core. |

`Tracing:IncludeClientLibraries` turns the Elasticsearch, MailKit and Azure SDK spans off. Other libraries are added with the `Tracing:Sources` and `Metrics:Meters` settings, and the libraries that don't emit telemetry themselves need an instrumentation package. For instance, StackExchange.Redis, used by the Redis features, emits no telemetry itself, and its `OpenTelemetry.Instrumentation.StackExchangeRedis` package only traces the connections registered with it, which the ones opened by the Redis features aren't.

### Logs

The logs written through `ILogger` are exported with their structured attributes and scopes, and those written in the scope of a tenant carry `orchardcore.tenant.name`. The OpenTelemetry logger is added next to the other logging providers, such as NLog or Serilog.

## Investigating performance

The rendering of shapes is traced too: the `OrchardCore.DisplayManagement` activity source emits a `Render shape {type}` span for each rendered shape, nested like the shapes, with the `orchardcore.shape.type` attribute and the `orchardcore.shape.binding` attribute, the template or the shape method that rendered it. The meter of the same name records the `orchardcore.display.shapes_rendered` counter, and the `orchardcore.display.shape.duration` histogram of the rendering durations by shape type. As a page renders many shapes, they are only collected when they are added to the configuration:

```json
{
  "OrchardCore": {
    "OpenTelemetry": {
      "Tracing": {
        "Sources": [ "OrchardCore.DisplayManagement" ]
      },
      "Metrics": {
        "Meters": [ "OrchardCore.DisplayManagement" ]
      }
    }
  }
}
```

This video shows how to find what a page spends its time on with these traces and metrics in the Aspire dashboard: reading the shape spans of a request, comparing the time of a shape with the time of its children, and filtering the rendering durations by shape type.

<video controls preload="metadata" width="100%" poster="videos/finding-slow-shapes.jpg">
    <source src="videos/finding-slow-shapes.mp4" type="video/mp4">
    <track kind="captions" src="videos/finding-slow-shapes.vtt" srclang="en" label="English">
    Your browser does not support embedded videos. <a href="videos/finding-slow-shapes.mp4">Download the video</a>.
</video>

To find the slow parts of a site:

1. Find the slowest shape types with the `orchardcore.display.shape.duration` histogram, filtered by shape type.
2. Open a slow request in the traces, and its `Render shape` spans, with the templates that rendered them.
3. Compare the time of a shape with the time of its child shapes: the time a shape spends itself is the work of its template, driver or shape method, such as loading content.

See the [Display Management](../DisplayManagement/README.md) module for the shape rendering telemetry.

## Using an existing OpenTelemetry configuration

When the application already configures OpenTelemetry, e.g. with the `ServiceDefaults` project of an Aspire solution, add the Orchard Core instrumentation to it instead of calling `AddOpenTelemetry()` on the Orchard Core builder:

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
