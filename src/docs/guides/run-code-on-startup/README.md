# How to run tasks on application startup from a module

The `Startup` classes are used to initialize the services and piece of middleware. 
They are called when a tenant is initialized.

The interface `OrchardCore.Modules.IModularTenantEvents` provides a way to define user code that will 
be executed when the tenant is first hit (tenant activation).

All tenants are lazy-loaded, meaning that when the app starts the event handlers are 
not invoked. They are instead called when the first request is processed.

In the following example the class `MyStartupTaskService` inherits from `ModularTenantEvents` 
to implement `IModularTenantEvents`.

```csharp
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OrchardCore.Modules;

public class MyStartupTaskService : ModularTenantEvents
{
    private readonly ILogger<MyStartupTaskService> _logger;

    public MyStartupTaskService(ILogger<MyStartupTaskService> logger)
    {
        _logger = logger;
    }

    public override Task ActivatingAsync()
    {
        _logger.LogInformation("A tenant has been activated.");

        return Task.CompletedTask;
    }
}
```

Then this class is registered on the `ConfigureServices()` method of the module's __Startup.cs__ file.

```csharp
services.AddScoped<IModularTenantEvents, MyStartupTaskService>();
```

!!! note
    `ActivatingAsync` events are invoked in the order of their registration, which is derived from
    the modules dependency graph. The `ActivatedAsync` events are invoked in the reverse order.

When ran from the terminal, you should see output like the following after the first request is processed:

```
info: MyStartupTaskService[0]
      A tenant has been activated.
```
