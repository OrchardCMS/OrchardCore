# SignalR (`OrchardCore.SignalR`)

The SignalR module provides the shared infrastructure required to host and consume [SignalR](https://learn.microsoft.com/aspnet/core/signalr/introduction) hubs in Orchard Core. Any module that declares a hub can depend on this module instead of registering SignalR itself, which keeps hub hosting, client resources, authentication, and scale-out backplanes consistent across the application.

## Features

- **`OrchardCore.SignalR`** — Registers SignalR with a camel-cased JSON protocol, the SignalR JavaScript client as a named resource (`signalr`), and hub authentication.
- **`OrchardCore.SignalR.Redis`** — Uses Redis as the SignalR backplane, enabling multi-instance deployments. Each tenant's traffic is isolated on a dedicated Redis channel prefix. Depends on `OrchardCore.Redis`.
- **`OrchardCore.SignalR.Azure`** — Uses the Azure SignalR Service as the backplane, enabling multi-instance deployments.

## Declaring a hub in another module

A module cannot reference the SignalR module directly, so the shared types it needs live in the `OrchardCore.SignalR.Core` project. Reference that project and declare the hub as usual:

```csharp
using Microsoft.AspNetCore.SignalR;
using OrchardCore.SignalR;

namespace MyModule.Hubs;

[AllowApiTokenAuthentication]
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

## Hub authentication

Browsers cannot send an `Authorization` header during a WebSocket handshake, so SignalR clients send bearer tokens using the standard `access_token` query string parameter. When a hub is annotated with `AllowApiTokenAuthentication`, the module promotes that token to an `Authorization` header and authenticates the request against the Orchard Core `Api` authentication scheme before authorization runs. Hubs that are not annotated keep the default behavior, where only the host-configured schemes (such as the admin cookie) are evaluated. Cookie authenticated requests are always left untouched.

Signed-in browser clients are authenticated through the regular authentication cookie, and nothing extra is required. Headless clients (single-page apps, mobile apps, and service-to-service callers) authenticate with an access token instead: enable the **OpenID Token Validation** feature (`OrchardCore.OpenId.Validation`) so the `Api` scheme can validate the token. The token is only *authenticated* — the identity behind it still needs whatever permissions the hub requires.

The `AllowApiTokenAuthentication` attribute never weakens a hub's authorization requirements. A hub decorated with `[Authorize]` still rejects callers that fail the policy, and a hub that allows anonymous connections still accepts them when no token is supplied. The attribute only lets an otherwise anonymous request that carries a valid bearer token be associated with the token's user before authorization runs.

Send the token from a browser client via `accessTokenFactory`:

```js
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/hubs/my-hub", {
        accessTokenFactory: () => accessToken
    })
    .build();
```

Or from a .NET client via `AccessTokenProvider`:

```csharp
var connection = new HubConnectionBuilder()
    .WithUrl("https://www.example.com/hubs/my-hub", options =>
    {
        options.AccessTokenProvider = () => Task.FromResult(accessToken);
    })
    .Build();
```

## Backplane configuration

The Redis backplane reuses the `OrchardCore.Redis` connection string (`OrchardCore_Redis:Configuration`):

```json
{
  "OrchardCore_Redis": {
    "Configuration": "your-redis-host:6379"
  }
}
```

The Azure SignalR Service backplane uses its own connection string:

```json
{
  "SignalR": {
    "Azure": {
      "ConnectionString": "Endpoint=https://<your-service>.service.signalr.net;AccessKey=...;Version=1.0;"
    }
  }
}
```

If a backplane feature is enabled but its connection string is missing, a warning is logged at startup and SignalR keeps working in single-instance (in-memory) mode — messages then only reach clients connected to the same instance.

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

> The vendored resource and the Bloom bundle pin `@microsoft/signalr` independently (currently `10.0.0` and `^8.0.0` respectively). Both interoperate with the same hub because the SignalR JSON wire protocol is stable across these versions, but keep this in mind when auditing the client version an app actually ships.
