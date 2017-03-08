# ASP.Net Modules

Modules provides a mechinism to have a self contained modular system where you can opt in to a specific application framework and not have the design of you application be dictated to by such.

## Getting started

First, create a brand new web application.

Within this new application we are initially going to focus on two files, `project.json` and `Startup.cs`. If you dont have etiher of these... start again!

Okay so first lets open up `project.json`.

Within the ConfigureServices method add these lines

```c#
services.AddModuleServices(configure => configure
    .AddConfiguration(Configuration)
);
```

Next, at the end of the Configure method, add these lines

```c#
app.UseModules();
```

Thats it. Erm, wait, what? Okay so right now you must be thinking, well what the hell does this do? good question.

AddModuleServices will add the container middleware to you application pipeline, this means in short that It will look in to a folder called Modules within you application for all folders that contain the mainfest file `Module.txt`. IF you looked on the file system it would look like this:

```
MyNewWebApplicaion
  \ Modules
    \ Module1
    \ Module2
```

Once it has found that manifest file, and said file is valid, it will then look for all classes that inherit off of `StartupBase`, instansiate them and then call the methods on here. An example of one is

```c#
public class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<ISomeInterface, SomeImplementedClass>();
    }
}
```

By doing this you allow your modules to be self contained, completely decoupled from the Hosting applicaiton.

> Note: If you drop a new module in, then you will need to restart the application for it to be found.

## Add Extra Locations
By default module discovery is linked to the `Modules` folder. This however can be extended.

Within the `Startup.cs` file in your host, within the `ConfigureServices` method, add

```c#
services.AddExtensionLocation("SomeOtherFolderToLookIn");
```

## Add Extra Manifest filenames
By default the module manifest file is `Module.txt`. This however can be extended.

Within the `Startup.cs` file in your host, within the `ConfigureServices` method, add

```c#
services.AddManifestDefinition("ManifestMe.txt", "module");
```

## Additional framework
You can add your favourite application framework to the pipeline, easily. The below implementations are designed to work side by side, so if you want Asp.Net Mvc and Nancy within your pipeline, just add both.

The modular framework wrappers below are designed to work directly with the modular application framework, so avoid just adding the raw framework and expect it to just work.

### Asp.Net Mvc
Within your hosting application add a reference to `Microsoft.AspNetCore.Mvc.Modules`

Next, within `Startup.cs` modify the method `AddModuleServices` to look like this

```c#
services.AddModuleServices(configure => configure
    .AddMvcModules(services.BuildServiceProvider())
    .AddConfiguration(Configuration)
);
```

> Note the addition of `.AddMvcModules(services.BuildServiceProvider())`

Thats it, done. Asp.Net Mvc is now part of your pipeline

### NancyFx
Within your hosting application add a reference to `Microsoft.AspNetCore.Nancy.Modules`

Next, within `Startup.cs` modify the method `Configure` to look like this

```c#
app.UseModules(modules => modules
    .UseNancyModules()
);
```

> Note the addition of `.UseNancyModules()`

Thats it, done. NancyFx is now part of your pipeline. What this means is that Nancy modules will be automatically discovered.

> Note. There is no need to register a Nancy Module within its own Startup class.