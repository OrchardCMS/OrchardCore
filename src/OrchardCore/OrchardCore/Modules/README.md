# Introduction

The library Orchard Core Modules provides a mechanism to have a self-contained modular system where you can opt into a specific application framework and not have the design of your application be dictated to by such.

## Getting started

In Visual Studio, create a new web application.

Install `OrchardCore.Application.Cms.Targets` into the project by managing the project NuGet packages.

Next, within `Startup.cs`, modify the `ConfigureServices` method, add this line:

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

## Additional frameworks

You can add your favourite application framework to the pipeline, easily. The below implementations are designed to work side by side, so if you want Asp.Net Mvc and Nancy within your pipeline, just add both.

The modular framework wrappers below are designed to work directly with the modular application framework, so avoid just adding the raw framework and expect it to just work.

### Asp.Net Mvc

Install `OrchardCore.Application.Mvc.Targets` into the project by managing the project NuGet packages.

Next, within `Startup.cs`, modify the method `ConfigureServices` to look like this:

```csharp
            // Add ASP.NET MVC and support for modules
            services
                .AddOrchardCore()
                .AddMvc()
                ;
```

!!! note
    Note the addition of `.AddMvc()`

Asp.Net Mvc is now part of your pipeline.

You can find a sample application here: [OrchardCore.Mvc.Web](../../../OrchardCore.Mvc.Web/Startup.cs)

### NancyFx

Install `OrchardCore.Application.Nancy.Targets` into the project by managing the project NuGet packages.

Next, within `Startup.cs`, modify the method `ConfigureServices` to look like this:

```csharp
            // Add Nancy and support for modules
            services
                .AddOrchardCore()
                .AddNancy()
                ;
);
```

!!! note
    Note the addition of `.AddNancy()`

NancyFx is now part of your pipeline. What this means is that Nancy modules will be automatically discovered.

You can find an sample application here: [OrchardCore.Nancy.Web](../../../OrchardCore.Nancy.Web/Startup.cs)