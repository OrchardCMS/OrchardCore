# Mini Profiler (`OrchardCore.MiniProfiler`)

The Mini Profiler module integrates [MiniProfiler](https://miniprofiler.com/) with an Orchard Core tenant. It records request timings, MVC filters and views, rendered shapes, and YesSql database commands, then adds the MiniProfiler widget to authorized HTML pages.

Use it to investigate a specific performance problem. Profiling adds work to every request handled by an enabled tenant and can expose route names, SQL statements, and timing details, so it should not be broadly available on a production site.

## Enable the feature

Enable the **Mini Profiler** feature from **Configuration → Features** for each tenant that you want to profile.

The feature can also be enabled from a recipe:

```json
{
  "name": "feature",
  "enable": [ "OrchardCore.MiniProfiler" ]
}
```

For development applications, the host can make the feature always enabled for every tenant:

```csharp
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

var orchardCoreBuilder = builder.Services.AddOrchardCms();

if (builder.Environment.IsDevelopment())
{
    orchardCoreBuilder.EnableFeature("OrchardCore.MiniProfiler");
}
```

`EnableFeature()` makes the feature part of every tenant shell; it is not a per-tenant setting that can then be disabled from the admin.

!!! warning
    MiniProfiler is not automatically restricted to the Development environment. If the feature is enabled for a production tenant, its middleware profiles requests until the feature is disabled or `MiniProfilerOptions.ShouldProfile` rejects them.

## View profiling results

The module adds the widget to the `Footer` zone for MVC view and Razor Page results:

- **View Mini Profiler widget on front end pages** controls the widget on site pages.
- **View Mini Profiler widget on back end pages** controls the widget on admin pages.

The Administrator role receives both permissions by default. Grant either permission to another role from **Security → Roles** only when its members are allowed to inspect profiling data. The backend widget does not require a separate application option; it appears when the user has the backend permission.

Click a timing in the widget to inspect the request. In addition to MiniProfiler's request, MVC, and view timings, Orchard Core contributes:

- `Shape: <shape-type>` steps for shapes rendered through display management.
- SQL custom timings from the YesSql connection used by the tenant.

Requests that do not render an Orchard layout, such as APIs and decoupled responses, are still profiled but do not receive a widget. The widget also requires the active theme or admin theme to render the `Footer` zone.

## Configure MiniProfiler

The module registers the upstream `StackExchange.Profiling.MiniProfilerOptions` in each tenant container. Configure those options through `OrchardCoreBuilder.ConfigureServices()` after calling `AddOrchardCms()`:

```csharp
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.MiniProfiler;
using StackExchange.Profiling;

var builder = WebApplication.CreateBuilder(args);

var orchardCoreBuilder = builder.Services.AddOrchardCms();

if (builder.Environment.IsDevelopment())
{
    orchardCoreBuilder
        .EnableFeature("OrchardCore.MiniProfiler")
        .ConfigureServices(tenantServices =>
            tenantServices.Configure<MiniProfilerOptions>(options =>
            {
                options.ShouldProfile = request =>
                    request.HttpContext.User.Identity?.IsAuthenticated == true;

                options.ResultsAuthorizeAsync = async request =>
                {
                    var httpContext = request.HttpContext;
                    var authenticateResult = await httpContext.AuthenticateAsync();

                    if (!authenticateResult.Succeeded ||
                        authenticateResult.Principal is null)
                    {
                        return false;
                    }

                    var authorizationService = httpContext.RequestServices
                        .GetRequiredService<IAuthorizationService>();
                    var user = authenticateResult.Principal;

                    return
                        (await authorizationService.AuthorizeAsync(
                            user,
                            Permissions.ViewMiniProfilerOnFrontEnd)) ||
                        (await authorizationService.AuthorizeAsync(
                            user,
                            Permissions.ViewMiniProfilerOnBackEnd));
                };

                options.IgnoredPaths.Add("/health");
                options.PopupRenderPosition = RenderPosition.Right;
                options.PopupMaxTracesToShow = 10;
                options.PopupShowTimeWithChildren = true;
                options.ShowControls = true;
            }));
}
```

`ConfigureServices()` runs for each tenant. The example:

- only enables and configures the module in Development;
- records requests from authenticated users only;
- protects MiniProfiler result endpoints with the same Orchard Core permissions used for the widgets;
- excludes a health-check path; and
- changes global client display options.

The module does not bind `MiniProfilerOptions` from an `OrchardCore_MiniProfiler` configuration section. Bind or assign options explicitly if they need to come from application configuration.

### Security

Widget permissions and MiniProfiler result authorization are separate:

- Orchard Core permissions determine whether the `MiniProfiler` shape is added to a frontend or admin layout.
- MiniProfiler's `ResultsAuthorize` or `ResultsAuthorizeAsync` delegate protects its result endpoints.
- `ShouldProfile` determines whether a request is recorded at all.

Enabling the feature does not configure a result authorization delegate. Configure one before using the module where untrusted users can access the site; hiding the widget alone does not protect stored results. Avoid granting widget permissions to the Anonymous role.

MiniProfiler handles its resource and result routes before Orchard Core's authentication middleware. An authorization delegate based on Orchard users should therefore call `AuthenticateAsync()` explicitly, as in the example, rather than assume that `HttpContext.User` has already been populated.

MiniProfiler serves its client files and results from its own middleware under `/mini-profiler-resources` by default. `RouteBasePath` changes that path. Include the tenant path base when diagnosing a tenant that uses a URL prefix.

### Reduce profiling overhead and noise

Useful upstream options include:

- `ShouldProfile` to limit profiling by environment, identity, path, endpoint, or another request property.
- `IgnoredPaths` to omit paths such as health checks or high-volume endpoints.
- `EnableMvcFilterProfiling` and `EnableMvcViewProfiling` to disable those timing groups.
- `MvcFilterMinimumSaveMs` and `MvcViewMinimumSaveMs` to discard short MVC timings.
- `TrackConnectionOpenClose` to omit database connection open and close timings.
- `ExcludeStackTraceSnippetFromCustomTimings` to avoid collecting stack snippets for custom timings.

The Orchard Core shape and YesSql integrations are active whenever the feature is active. They do not have separate module settings.

### Customize the widget

The module renders MiniProfiler's includes without per-page overrides, so the global options control the client. These include `PopupRenderPosition`, `PopupShowTrivial`, `PopupShowTimeWithChildren`, `PopupMaxTracesToShow`, `PopupStartHidden`, `PopupToggleKeyboardShortcut`, `ShowControls`, and `ColorScheme`.

For the complete upstream option reference and storage providers, see the [MiniProfiler documentation](https://miniprofiler.com/dotnet/).

## Storage and multiple instances

By default, results are held in the tenant's in-memory cache for 30 minutes. They are lost when the process restarts and are not shared between application instances. A widget request routed to another instance may therefore be unable to load its result.

For a multi-instance deployment, use session affinity while diagnosing or replace `MiniProfilerOptions.Storage` with a shared `IAsyncStorage` implementation. Storage providers are separate MiniProfiler packages and are not registered by this Orchard Core module. Apply the same result authorization regardless of the storage provider.

## Add custom timings

Code running during a profiled request can add steps through MiniProfiler's API:

```csharp
using StackExchange.Profiling;

using (MiniProfiler.Current?.Step("Load recommendations"))
{
    await recommendationService.LoadAsync();
}
```

`MiniProfiler.Current` is `null` when the request is not being profiled, so custom instrumentation should allow for that case. Other integrations, such as Entity Framework Core or external storage providers, require their corresponding MiniProfiler packages and registration; the Orchard Core module does not add them.

## Troubleshooting

### The widget does not appear

1. Confirm that `OrchardCore.MiniProfiler` is enabled for the current tenant.
2. Confirm that the current role has the frontend or backend widget permission for the page being viewed.
3. Test an MVC view or Razor Page that uses an Orchard layout and renders its `Footer` zone.
4. Check that `ShouldProfile` and `IgnoredPaths` allow the request.
5. In browser developer tools, check requests under `/mini-profiler-resources` and account for the tenant URL prefix or a customized `RouteBasePath`.

### A result is missing

Results expire from the default in-memory storage after 30 minutes. A process restart clears them, and another application instance has a different cache. Also verify that the result authorization delegate allows the current user.

### SQL or shape timings are missing

YesSql timings are produced only for database work executed through the tenant's wrapped YesSql connection. Shape timings appear only for shapes rendered through Orchard Core display management. Other data-access libraries and rendering pipelines need their own instrumentation.

## Video

<iframe width="560" height="315" src="https://www.youtube-nocookie.com/embed/tFLZ4Ha7PZE" title="YouTube video player" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture" allowfullscreen></iframe>
