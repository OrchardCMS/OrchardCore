# SignalR (`OrchardCore.SignalR`)

The SignalR module provides the shared infrastructure required to host and consume [SignalR](https://learn.microsoft.com/aspnet/core/signalr/introduction) hubs in Orchard Core. Any module that declares a hub can depend on this module instead of registering SignalR itself, which keeps hub hosting, client resources, authentication, and scale-out backplanes consistent across the application.

The scale-out backplanes ship as separate modules so the base `OrchardCore.SignalR` module stays free of Redis and Azure dependencies. Install only the backplane you need:

| Module | Feature | Purpose |
| --- | --- | --- |
| `OrchardCore.SignalR` | `OrchardCore.SignalR` | Base SignalR hosting, client resources, and the authorization policy used by secured hubs. |
| `OrchardCore.SignalR.Redis` | `OrchardCore.SignalR.Redis` | Redis scale-out backplane. Brings the Redis dependencies. |
| `OrchardCore.SignalR.Azure` | `OrchardCore.SignalR.Azure` | Azure SignalR Service backplane. Brings the Azure dependencies. |

## Features

- **`OrchardCore.SignalR`** — Registers SignalR with a camel-cased JSON protocol, the SignalR JavaScript client as a named resource (`signalr`), and the `SignalR` authorization policy used by secured hubs.
- **`OrchardCore.SignalR.Redis`** — Uses Redis as the SignalR backplane, enabling multi-instance deployments. Each tenant's traffic is isolated on a dedicated Redis channel prefix. Provided by the separate `OrchardCore.SignalR.Redis` module and depends on `OrchardCore.Redis`.
- **`OrchardCore.SignalR.Azure`** — Uses the Azure SignalR Service as the backplane, enabling multi-instance deployments. Provided by the separate `OrchardCore.SignalR.Azure` module.

## Declaring a hub in another module

Declare the hub as usual:

```csharp
using Microsoft.AspNetCore.Authorization;

namespace MyModule.Hubs;

[Authorize]
public sealed class MyHub : Hub
{
}
```

The feature that hosts the hub depends on `OrchardCore.SignalR` and maps the hub in its `Startup`:

```csharp
public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
{
    routes.MapHub<MyHub>("/hubs/my-hub");
}
```

## Configuring hub options

Per-hub behavior (for example, allowing long-running operations or tuning keep-alive) is configured by binding `HubOptions<T>` in `ConfigureServices`:

```csharp
services.Configure<HubOptions<MyHub>>(options =>
{
    options.ClientTimeoutInterval = TimeSpan.FromMinutes(10);
    options.KeepAliveInterval = TimeSpan.FromSeconds(15);
});
```

Global options that apply to every hub are configured with the parameterless `HubOptions` instead:

```csharp
services.Configure<HubOptions>(options =>
{
    options.MaximumReceiveMessageSize = 128 * 1024;
});
```

## Backplane configuration

Each backplane is a separate module and only registers once its feature is enabled. The Redis backplane (`OrchardCore.SignalR.Redis`) reuses the `OrchardCore.Redis` connection string (`OrchardCore_Redis:Configuration`):

```json
{
  "OrchardCore_Redis": {
    "Configuration": "your-redis-host:6379"
  }
}
```

The Azure SignalR Service backplane (`OrchardCore.SignalR.Azure`) uses its own connection string:

```json
{
  "SignalR": {
    "Azure": {
      "ConnectionString": "Endpoint=https://<your-service>.service.signalr.net;AccessKey=...;Version=1.0;",
      "ApplicationName": "OrchardCore"
    }
  }
}
```

`ApplicationName` is optional and defaults to `OrchardCore`. Set it to a unique value when multiple Orchard Core deployments share the same Azure SignalR Service. It must start with a letter and contain only letters, numbers, and underscores. Orchard Core appends a stable tenant identifier so hubs remain isolated between tenants.

If a backplane feature is enabled but its connection string is missing, a warning is logged at startup and SignalR keeps working in single-instance (in-memory) mode — messages then only reach clients connected to the same instance.

Enable only one backplane feature per tenant. Enabling both the Azure and Redis backplanes causes tenant startup to fail with a configuration error rather than selecting one provider unpredictably.

## Client integration

There are two independent ways to talk to a hub from the browser, and they do **not** overlap — pick the one that matches how your script is delivered.

### Bundled apps (Vite/TypeScript)

Vue/TypeScript apps that are bundled with Vite (such as the Media gallery) should **not** use the `signalr` resource below. Instead, reuse the reusable client layer in Bloom under `@bloom/services/signalr`:

- `signalr-app` — a `HubConnection` wrapper that handles bearer-token negotiation, transport selection, automatic reconnect, and incoming-message interception.
- `eventbus` — a [mitt](https://github.com/developit/mitt)-based bus that re-emits hub messages (and `entity-created`/`updated`/`deleted` events) as decoupled application events.
- `useSignalRService` — a composable that bootstraps a connection from an API endpoint and token.

```ts
import SignalRApp from "@bloom/services/signalr/signalr-app";
import { signalRReceivedData } from "@bloom/services/signalr/eventbus";
```

Vite bundles `@microsoft/signalr` into the app, so bundled apps never load the module's `signalr` resource.

### Server-rendered pages

For a server-rendered view or shape template that is not part of a bundled app, load the client that ships with this module as the named `signalr` resource (with a CDN fallback), then use the global `signalR` object:

```html
<script asp-name="signalr" at="Foot"></script>
```

The client is vendored under `wwwroot/Scripts/` and is produced from the `@microsoft/signalr` dependency declared in `Assets/package.json` by running `yarn build` at the repository root.

A script that opens a connection should declare `depends-on="signalr"` so the client is guaranteed to be on the page first:

```html
<script type="text/javascript" at="Foot" depends-on="signalr">
    document.addEventListener("DOMContentLoaded", function () {
        var connection = new signalR.HubConnectionBuilder()
            .withUrl("/hubs/my-hub")
            .build();

        connection.start()
            .then(function () { console.log("Connected to the SignalR hub."); })
            .catch(function (error) { console.error("Connection failed:", error.message); });
    });
</script>
```
