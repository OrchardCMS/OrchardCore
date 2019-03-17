# Run a task on tenant activation

The `Startup` classes are used to initialize the services and piece of middleware. 
They are called when a tenant is initialized.

The interface `OrchardCore.Modules.IModularTenantEvents` provides a way to define user code that will 
be executed when the tenant is first hit (tenant activation) and when it is deactivated.

In the following example the class `MyStartupTaskService` implements `IModularTenantEvents`.

## What you will build ##

You will build a module that will print out a message with the name of the tenant when the tenant is activated or deactivated.

## What you will need ##

- Latest versions (current) for both Runtime and SDK of .NET Core. You can download them from here [https://www.microsoft.com/net/download/core](https://www.microsoft.com/net/download/core).

- A text editor and a terminal where you cant type dotnet commands.

## Creating an Orchard Core site and module ##

There are different ways to create sites and modules for Orchard Core. You can learn more about them [here](../../templates/README.md). In this guide we will use our "Code Generation Templates". 

You can install the latest templates using this command:

```dotnet new -i OrchardCore.ProjectTemplates::1.0.0-beta3-* --nuget-source https://www.myget.org/F/orchardcore-preview/api/v3/index.json```


>Note: At the time of writing this Orchard Core is still in beta. This command install the latest available templates. For the majority of the scenarios we would still recommend that.

Create an empty folder that will contain your site. Open a terminal, navigate to that folder and run this:

```dotnet new occms -n MySite```

```dotnet new ocmodulecms -n MyModule.OrchardCore```

```dotnet add MySite/MySite.csproj reference MyModule.OrchardCore/MyModule.OrchardCore.csproj```

The first two commands will create a new Orchard Core Cms application ready to be setup, and a module.
The last command will create a reference to the module on the application.


## Adding our service to the module ##

Add a "Services" folder inside the "MyModule.OrchardCore" folder.
Add a file "MyStartupTaskService.cs" to that folder, with the following content:

```csharp
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OrchardCore.Environment.Shell;
using OrchardCore.Modules;

namespace MyModule.OrchardCore.Services
{
    public class MyStartupTaskService : IModularTenantEvents
    {
        private readonly ILogger<MyStartupTaskService> _logger;
        private readonly ShellSettings _shellSettings;

        public MyStartupTaskService(
            ILogger<MyStartupTaskService> logger,
            ShellSettings shellSettings)
        {
            _logger = logger;
            _shellSettings = shellSettings;
        }

        public Task ActivatedAsync()
        {
            _logger.LogWarning($"Tenant {_shellSettings.Name} Activated.");
            return Task.CompletedTask;
        }

        public Task ActivatingAsync()
        {
            return Task.CompletedTask;
        }

        public Task TerminatedAsync()
        {
            _logger.LogWarning($"Tenant {_shellSettings.Name} Terminated.");
            return Task.CompletedTask;
        }

        public Task TerminatingAsync()
        {
            return Task.CompletedTask;
        }
    }
}

```

Then you have to register your service on the "Startup.cs" file of the module.

First add this `using` statement:

```csharp
using MyModule.OrchardCore.Services; 
```

And then register the service in the `ConfigureServices` Method:

```csharp
services.AddScoped<IModularTenantEvents, MyStartupTaskService>();
```

## Testing the service on the Default tenant ##

From the root of the folder containing both projects, run this command:

```
dotnet run --project .\MySite\MySite.csproj
```
Your application should be built and run.

So you can browse to http:\\localhost:5000 and setup your site.

For this example you could use Sqlite as database.

We use the "Software as a Service" recipe because it enables the "Tenants" module which will make easier testing our module. If you use another recipe you could enable the "Tenants" module later using the admin.

Once your site is setup, login to the admin http:\\localhost:5000\admin

Then go to "Configuration", search for your module "MyModule.OrchardCore" and enable it.

Now your module is enabled and we are ready to test your service.

Go to "Configuration:Tenants".

There will be one single tenant called "Default".
Click the "Reload" button and look at your terminal window, you should see these messages printed out:

```
MyModule.OrchardCore.Services.MyStartupTaskService[0] Tenant 'Default' Terminated

MyModule.OrchardCore.Services.MyStartupTaskService[0] Tenant 'Default' Activated
```

## Testing the service on other tenants ##

Create another tenant "Tenant2" using the "Tenants" module.

Setup that second tenant. Login to it and enable your module. Remember that modules can be enabled or disabled per tenant, so you have to enable it on each one of them or you won't see the messages.

Go back to the admin of the "Default" tenant, and go again to "Configuration:Tenants".

Now click "Reload" on your second tenant. The messages are not printed. Why is that?

That is because tenants are lazy-loaded, meaning that when the app starts the event handlers are 
not invoked. They are instead called when the first request is processed.

So what you have to do in order to test your module is just browse to that tenant. As soon as you do that you will see the message for that specific tenant printed out.

```
MyModule.OrchardCore.Services.MyStartupTaskService[0] Tenant 'Tenant2' Activated
```

## Summary ##

You just created an application that can run any task on its startup or shut down.
Even better, you can do that per tenant, thanks to the modular and multitenant capabilities of Orchard Core.