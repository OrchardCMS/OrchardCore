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

## Hub authentication

Browsers cannot send an `Authorization` header during a WebSocket handshake, so SignalR clients send bearer tokens using the standard `access_token` query string parameter. When a hub is annotated with `AllowApiTokenAuthentication`, the module promotes that token to an `Authorization` header and authenticates the request against the Orchard Core `Api` authentication scheme before authorization runs. Hubs that are not annotated keep the default behavior, where only the host-configured schemes (such as the admin cookie) are evaluated. Cookie authenticated requests are always left untouched.

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
