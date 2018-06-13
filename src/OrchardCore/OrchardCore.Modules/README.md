# Introduction

The library Orchard Core Modules provides a mechanism to have a self-contained modular system where you can opt in to a specific application framework and not have the design of your application be dictated to by such.

## Getting started

First, create a brand new web application.

Install OrchardCore.Application.Cms.Targets into the project by managing the project NuGet packages.

Within this new application we are initially going to focus on `Startup.cs`.

Okay so first, let's open up `Startup.cs`.

Within the `ConfigureServices` method, add this line:

```csharp
services.AddOrchardCms(); 
```

Next, at the end of the `Configure` method, replace this block: 

```csharp
app.Run(async (context) => 
{
    await context.Response.WriteAsync("Hello World!"); 
});
```

with this line: 

```csharp
app.UseOrchardCore();
```

That's it. Erm, wait, what? Okay so right now you must be thinking, well what the hell does this do? Good question.

`AddModuleServices` will add the container middleware to your application pipeline; this means, in short, that it will look for a folder called `Modules` within your application, for all folders that contain the manifest file `Module.txt`. If you looked on the file system it would look like this:

```
MyNewWebApplication
  \ Modules
    \ Module1
    \ Module2
```

Once it has found that manifest file, and said file is valid, it will then look for all classes that inherit from `StartupBase`, instantiate them and then call the methods on here. An example of one is:

```csharp
public class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<ISomeInterface, SomeImplementedClass>();
    }
}
```

By doing this you allow your modules to be self-contained, completely decoupled from the Hosting application.

!!! note
    If you drop a new module in, then you will need to restart the application for it to be found.

## Add Extra Locations
By default module discovery is linked to the `Modules` folder. This however can be extended.

Within the `Startup.cs` file in your host, within the `ConfigureServices` method, add:

```csharp
services.AddExtensionLocation("SomeOtherFolderToLookIn");
```

## Add Extra Manifest filenames
By default the module manifest file is `Module.txt`. This however can be extended.

Within the `Startup.cs` file in your host, within the `ConfigureServices` method, add:

```csharp
services.AddManifestDefinition("ManifestMe.txt", "module");
```

## Additional framework
You can add your favourite application framework to the pipeline, easily. The below implementations are designed to work side by side, so if you want Asp.Net Mvc and Nancy within your pipeline, just add both.

The modular framework wrappers below are designed to work directly with the modular application framework, so avoid just adding the raw framework and expect it to just work.

### Asp.Net Mvc
Within your hosting application add a reference to `OrchardCore.Mvc.Core`.

Next, within `Startup.cs` modify the method `AddModuleServices` to look like this:

```csharp
services.AddModuleServices(configure => configure
    .AddMvcModules(services.BuildServiceProvider())
    .AddConfiguration(Configuration)
);
```

!!! note 
    Note the addition of `.AddMvcModules(services.BuildServiceProvider())`

That's it, done. Asp.Net Mvc is now part of your pipeline.

### NancyFx
Within your hosting application add a reference to `OrchardCore.Nancy.Core`

Next, within `Startup.cs` modify the method `Configure` to look like this:

```csharp
app.UseModules(modules => modules
    .UseNancyModules()
);
```

!!! note 
    Note the addition of `.UseNancyModules()`

That's it, done. NancyFx is now part of your pipeline. What this means is that Nancy modules will be automatically discovered.

!!! note 
    There is no need to register a Nancy Module within its own Startup class.
