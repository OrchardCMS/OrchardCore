# How to run tasks on application startup from a module

You can add a class that implements IModularTenantEvents and use the ActivatedAsync or ActivatingAsync handlers.

The events are raised per tenant.
One thing to take into account is that tenants are lazy loaded. 
So, when the app starts the event handlers are not called. 
They are called when the first request arrives for each specific tenant.

An example implementation:

```csharp

public class MyStartupTaskService : IModularTenantEvents
{
    private readonly ILogger<TempDirCleanerService> _logger;

    public TempDirCleanerService(ILogger<TempDirCleanerService> logger)
    {
        _logger = logger;
    }

    public async Task ActivatedAsync()
    {
        _logger.LogInfo("logging tenant activation.");
    }

    public Task ActivatingAsync()
    {
        return Task.CompletedTask;
    }

    public Task TerminatedAsync()
    {
        return Task.CompletedTask;
    }

    public Task TerminatingAsync()
    {
        return Task.CompletedTask;
    }
}
```

Then you have to register your service on ConfigureServices() method of the module's Startup.cs file:
```csharp
services.AddScoped<IModularTenantEvents, MyStartupTaskService>();
```
