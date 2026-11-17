# OpenTelemetry (`OrchardCore.OpenTelemetry`)

`OrchardCore.OpenTelemetry` collects the traces, metrics and logs of an Orchard Core application with [OpenTelemetry](https://opentelemetry.io/), and exports them over OTLP to any compatible backend: the Aspire dashboard, Jaeger, Prometheus, Grafana, Azure Monitor, Datadog, Honeycomb, Elastic, and many more.

The telemetry is multi-tenant aware: spans, log records and HTTP server metrics are tagged with the name of their tenant, and resource attributes identify each instance of the application, so that the behavior of every tenant and every instance of a web farm can be told apart.

This video shows how to enable it, the traces, metrics and logs of two instances serving two tenants in the Aspire dashboard, and how to configure it.

<iframe width="560" height="315" src="https://www.youtube-nocookie.com/embed/r6aAa3TrLus" title="OpenTelemetry in Orchard Core" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture" allowfullscreen></iframe>

## Enabling OpenTelemetry

Reference the `OrchardCore.OpenTelemetry` package and call `AddOpenTelemetry()` on the Orchard Core builder:

```csharp
builder.Services
    .AddOrchardCms()
    .AddOpenTelemetry();
```

Calling `AddOpenTelemetry()` collects the telemetry even when nothing exports it, as exporters can also be added with the callback or by the application. When OTLP is the only exporter, check that an endpoint is configured first, which is what the `OrchardCore.Cms.Web` application of the repository does, so that setting one is enough to export its telemetry:

```csharp
var orchardCore = builder.Services.AddOrchardCms();

if (builder.Configuration.HasOtlpEndpoint())
{
    orchardCore.AddOpenTelemetry();
}
```

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

Prometheus and Mimir store the metrics with their names and attributes converted: dots become underscores, and the unit is added as a suffix, so `http.server.request.duration` becomes `http_server_request_duration_seconds`, with an `orchardcore_tenant_name` label, and the `service.name` and `service.instance.id` resource attributes become the `job` and `instance` labels. For instance:

| Question | Query |
| --- | --- |
| Which shapes are the slowest, once the `OrchardCore.DisplayManagement` meter is added? | `topk(10, histogram_quantile(0.95, sum by (le, orchardcore_shape_type) (rate(orchardcore_display_shape_duration_seconds_bucket[5m]))))` |
| How fast does each tenant respond? | `histogram_quantile(0.95, sum by (le, orchardcore_tenant_name) (rate(http_server_request_duration_seconds_bucket[5m])))` |
| How much memory does each instance use? | `sum by (instance) (dotnet_process_memory_working_set_bytes)` |
| Which tenants run the most database commands, once the `YesSql` meter is added? | `topk(10, sum by (orchardcore_tenant_name) (rate(db_client_operation_duration_seconds_count[5m])))` |

In Tempo, a TraceQL query such as `{ span.orchardcore.tenant.name = "Default" && duration > 500ms }` finds the slow requests of a tenant, `{ name =~ "Render shape.*" }` the slow shapes, and `{ span.db.system.name != nil && duration > 100ms }` the slow database commands, once the `YesSql` activity source is added.

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
| `Tracing:SamplingRatio` | The ratio, from `0` to `1`, of the traces that are sampled. A trace started by a sampled parent, e.g. in another service, is always sampled. When not set, the standard `OTEL_TRACES_SAMPLER` value is used, and otherwise all traces are sampled. |
| `Tracing:Sources` | The names of additional `ActivitySource` to collect traces from. See [Other libraries](#other-libraries). |
| `Tracing:SlowShapeThreshold` | The duration from which the rendering of a shape is traced with its own span. Defaults to `00:00:00.050` (50 milliseconds), `00:00:00` traces every shape. |
| `Metrics:Enabled` | Whether metrics are collected. Defaults to `true`. |
| `Metrics:IncludeTenantName` | Whether the HTTP server metrics are tagged with the name of the tenant that served the request. Defaults to `true`. With many tenants, disable it to limit the number of metric series. |
| `Metrics:Meters` | The names of additional `Meter` to collect metrics from. See [Other libraries](#other-libraries). |
| `Logging:Enabled` | Whether logs are collected. Defaults to `true`. The log levels of the `Logging` section apply. |
| `Otlp:Endpoint` | The endpoint of the OTLP receiver for all signals, e.g. `http://localhost:4317` for gRPC or `http://localhost:4318` for HTTP. An endpoint that isn't an absolute HTTP or HTTPS URI stops the application from starting. |
| `Otlp:Protocol` | `Grpc` or `HttpProtobuf`. Defaults to the `OTEL_EXPORTER_OTLP_PROTOCOL` value, and otherwise to `Grpc`. |

The standard [OTLP exporter values](https://opentelemetry.io/docs/languages/sdk-configuration/otlp-exporter/) are also supported, from the configuration of the application, e.g. environment variables. The `Otlp` section takes precedence over them.

| Value | Description |
| --- | --- |
| `OTEL_EXPORTER_OTLP_ENDPOINT` | The endpoint of the OTLP receiver for all signals. Over HTTP, the path of each signal is added to it. |
| `OTEL_EXPORTER_OTLP_PROTOCOL` | `grpc` or `http/protobuf`. Defaults to `grpc`. |
| `OTEL_EXPORTER_OTLP_HEADERS` | The headers sent to the receiver, e.g. to pass an API key to a hosted backend. |
| `OTEL_EXPORTER_OTLP_TIMEOUT` | The time to wait for the receiver, in milliseconds. |
| `OTEL_EXPORTER_OTLP_COMPRESSION` | `gzip` or `none`. |
| `OTEL_EXPORTER_OTLP_METRICS_TEMPORALITY_PREFERENCE` | `cumulative`, `delta` or `lowmemory`. Defaults to `cumulative`. Some backends require `delta`. |
| `OTEL_EXPORTER_OTLP_METRICS_DEFAULT_HISTOGRAM_AGGREGATION` | `explicit_bucket_histogram` or `base2_exponential_bucket_histogram`. Defaults to `explicit_bucket_histogram`. |

Each of the first five values also exists for a single signal, which takes precedence, with `TRACES`, `METRICS` or `LOGS` after `OTLP`. For instance, `OTEL_EXPORTER_OTLP_TRACES_ENDPOINT` only sets the endpoint of the traces, and is used as is, without adding a path to it. Only the signals with an endpoint are exported.

The other standard values are read by the OpenTelemetry SDK itself:

- `OTEL_SDK_DISABLED` turns everything off.
- `OTEL_TRACES_SAMPLER` and `OTEL_TRACES_SAMPLER_ARG` set the sampler, when `Tracing:SamplingRatio` isn't set.
- `OTEL_SERVICE_NAME` and `OTEL_RESOURCE_ATTRIBUTES` take precedence over the values of the section.
- `OTEL_METRIC_EXPORT_INTERVAL`, `OTEL_METRIC_EXPORT_TIMEOUT` and the `OTEL_BSP_*` and `OTEL_BLRP_*` settings of the batches of traces and logs are only read from environment variables.

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
| `Render shape {type}` | Internal | `orchardcore.tenant.name`, `orchardcore.shape.type`, `orchardcore.shape.binding` (the template or the shape method that rendered it) |

A `Render shape` span is only emitted for a shape whose rendering took longer than `Tracing:SlowShapeThreshold`. It's created once the rendering is known to be slow, so it's a child of the request rather than of the shape that rendered it.

A failed operation has the `Error` status, the `error.type` attribute and an `exception` event. A background task run is the root of its own trace.

The spans of ASP.NET Core (one for each HTTP request, also tagged with the `orchardcore.tenant.name` of the tenant that served it) and of HttpClient (one for each outgoing HTTP request) are collected too.

Every span started in the scope of a tenant, including the spans of other libraries, is tagged with `orchardcore.tenant.name`.

A recipe step that reports errors without throwing an exception has no `exception` event, and its `error.type` is the `RecipeExecutionException` that makes the recipe fail.

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
| `orchardcore.shape.cache.lookups` | Counter | `{lookup}` | `orchardcore.tenant.name`, `orchardcore.shape.type`, `orchardcore.shape.cache.result` | Number of lookups of the content of a cached shape: `hit` or `miss`. Only recorded when the Dynamic Cache feature is enabled. |

`error.type` is only set when the operation failed.

The metrics built in .NET are collected too, from these meters:

| Meter | Metrics |
| --- | --- |
| `Microsoft.AspNetCore.Hosting` | `http.server.request.duration`, also tagged with `orchardcore.tenant.name` (see `Metrics:IncludeTenantName`), and `http.server.active_requests`. |
| `Microsoft.AspNetCore.Server.Kestrel`, `Microsoft.AspNetCore.MemoryPool`, `Microsoft.AspNetCore.Routing`, `Microsoft.AspNetCore.Diagnostics`, `Microsoft.AspNetCore.RateLimiting`, `Microsoft.AspNetCore.Http.Connections` | Connections and memory pools, route matching, handled exceptions, rate limiting, and SignalR connections. |
| `Microsoft.AspNetCore.Authentication`, `Microsoft.AspNetCore.Authorization`, `Microsoft.AspNetCore.Identity` | Authentications, authorization attempts, sign-ins and user operations. |
| `System.Net.Http`, `System.Net.NameResolution` | `http.client.request.duration`, connections and DNS lookups. |
| `System.Runtime` | The health of each instance: `dotnet.gc.*`, `dotnet.process.cpu.time`, `dotnet.process.memory.working_set`, `dotnet.thread_pool.*`, `dotnet.exceptions`... |

### Other libraries

Each library is responsible for its own telemetry, and Orchard Core doesn't add any for it. OpenTelemetry only collects the activity sources and meters it's told about, so to collect what a library emits, add its names to the `Tracing:Sources` and `Metrics:Meters` settings. Names ending with `*` match every source or meter starting with that text:

```json
{
  "OrchardCore": {
    "OpenTelemetry": {
      "Tracing": {
        "Sources": [ "YesSql", "Elastic.Transport", "MailKit.Net.SmtpClient" ]
      },
      "Metrics": {
        "Meters": [ "YesSql" ]
      }
    }
  }
}
```

These libraries, used by Orchard Core modules, emit their own telemetry:

| Library | Used by | `Tracing:Sources` | `Metrics:Meters` |
| --- | --- | --- | --- |
| YesSql | All databases | `YesSql` | `YesSql` |
| Npgsql | PostgreSQL databases | `Npgsql` | `Npgsql` |
| MySqlConnector | MySQL databases | `MySqlConnector` | `MySqlConnector` |
| Elasticsearch client | Elasticsearch | `Elastic.Transport` | |
| MailKit | SMTP email | `MailKit.Net.SmtpClient` | |
| Azure SDK | Azure Blob Storage, Key Vault, Communication Services, AI Search | `Azure.*` | |

Names of libraries that emit nothing are ignored. A library that doesn't emit telemetry itself needs an instrumentation package from its own maintainers or from OpenTelemetry, added with the callback of `AddOpenTelemetry()`, e.g. `OpenTelemetry.Instrumentation.SqlClient` for the SQL Server client.

#### Database telemetry

[YesSql](https://github.com/sebastienros/yessql#telemetry) reports the same telemetry with every database: a span for each command with its table, a span for each flush of a session, the `db.client.operation.duration` histogram, and the number of commands and documents of each session in the `yessql.session.commands` and `yessql.session.documents` histograms. A session running many commands reveals an N+1 query pattern.

- The spans and the metrics of the store of a tenant are tagged with `orchardcore.tenant.name`, whatever the value of `Metrics:IncludeTenantName`.
- The SQL text is only added to the spans of YesSql with the [`IncludeQueryTextInTelemetry`](../Data/README.md#yessql-options) YesSql option. The spans of the `Npgsql` activity source always include it.
- The commands run directly on a connection, such as those of the SQL queries of the Queries module, aren't reported by YesSql.

The `Npgsql` and `MySqlConnector` names add what YesSql can't see, such as the state of the connection pools. The Npgsql meter also records `db.client.operation.duration`, so collecting it with the YesSql meter counts each command twice. Drop the instrument of Npgsql with a view, which also drops its measurements of the commands YesSql doesn't run:

```csharp
builder.Services
    .AddOrchardCms()
    .AddOpenTelemetry(openTelemetry => openTelemetry
        .WithMetrics(metrics => metrics.AddView(instrument =>
            YesSqlTelemetry.IsDuplicateDriverInstrument(instrument) ? MetricStreamConfiguration.Drop : null)));
```

### Logs

The logs written through `ILogger` are exported with their structured attributes and scopes, and those written in the scope of a tenant carry `orchardcore.tenant.name`. The OpenTelemetry logger is added next to the other logging providers, such as NLog or Serilog.

## Investigating performance

The shapes slower than `Tracing:SlowShapeThreshold` are traced by default, which tells which templates make a page slow. To see every shape, nested as it's rendered, the `OrchardCore.DisplayManagement` activity source emits a `Render shape {type}` span for each rendered shape, nested like the shapes, with the `orchardcore.shape.type` attribute and the `orchardcore.shape.binding` attribute, the template or the shape method that rendered it. The meter of the same name records the `orchardcore.display.shapes_rendered` counter, and the `orchardcore.display.shape.duration` histogram of the rendering durations by shape type. As a page renders many shapes, they are only collected when they are added to the configuration, and a shape traced this way doesn't get a second span when it's slow:

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

To find the slow parts of a site:

1. Find the slowest shape types with the `orchardcore.display.shape.duration` histogram, filtered by shape type.
2. Open a slow request in the traces, and its `Render shape` spans, with the templates that rendered them.
3. Compare the time of a shape with the time of its child shapes: the time a shape spends itself is the work of its template, driver or shape method, such as loading content.
4. Add `YesSql` to `Tracing:Sources` to see the [database commands](#database-telemetry) nested in these spans, and tell a slow query from many small ones.

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

As these sources only depend on `System.Diagnostics`, the Orchard Core telemetry can also be watched without OpenTelemetry, e.g. with `dotnet-counters monitor --counters OrchardCore,System.Runtime`. The `orchardcore.tenants` and `orchardcore.tenants.loaded` gauges are the exception, as they are only observed once `AddOrchardCoreInstrumentation()` or `AddOpenTelemetry()` has configured the metrics.
