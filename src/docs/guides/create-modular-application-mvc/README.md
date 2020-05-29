# Creating a modular ASP.NET Core application

## What you will build

You will build an application that is made of modules. The module will provide a Controller and a View while the Layout will
be provided by the main application project.

## What you will need

- The current version of the .NET Core SDK. You can download it from here [https://www.microsoft.com/net/download/core](https://www.microsoft.com/net/download/core).
- A text editor and a terminal where you can type dotnet commands.

## Creating an Orchard Core site and module

There are different ways to create sites and modules for Orchard Core. You can learn more about them [here](../../getting-started/templates/README.md).  
In this guide we will use our "Code Generation Templates".

You can install the latest released templates using this command:

```dotnet new -i OrchardCore.ProjectTemplates::1.0.0-*```

!!! note
    To use the development branch of the template add `--nuget-source https://nuget.cloudsmith.io/orchardcore/preview/v3/index.json`

Create an empty folder that will contain your site. Open a terminal, navigate to that folder and run this:

```dotnet new ocmvc -n MySite```

This creates a new ASP.NET MVC application in a new folder named `MySite`.  
We can now create a new module with the following command:

```dotnet new ocmodulemvc -n MyModule```

The module is created in the `MyModule` folder.  
The next step is to reference the module from the application, by adding a project reference:

```dotnet add MySite reference MyModule```

## Testing the resulting application

From the root of the folder containing both projects, run this command:

`dotnet run --project .\MySite\MySite.csproj`

!!! note
    If you are using the development branch of the templates, run `dotnet restore .\MySite\MySite.csproj --source https://nuget.cloudsmith.io/orchardcore/preview/v3/index.json` before running the application

Your application should now be running and contain the open ports:

```
Now listening on: https://localhost:5001
Now listening on: http://localhost:5000
Application started. Press Ctrl+C to shut down.
```

Open a browser on <https://localhost:5001/MyModule/Home/Index>  
It should display __Hello from MyModule__

> The Layout is coming from the main application project, while the controller, action and view are coming from the module project.

## Registering a custom route

By default all routes in modules are modeled like `{area}/{controller}/{action}` where `{area}` is the name of the module.  
We will change the route of the view in this module to handle the home page.

In the `Startup.cs` file of `MyModule`, add this code in the `Configure()` method.

```csharp
    routes.MapAreaControllerRoute(
        name: "Home",
        areaName: "MyModule",
        pattern: "",
        defaults: new { controller = "Home", action = "Index" }
    );
```

Restart the application and open the home page, which should display the same result as with the previous url.

## Summary

You just created an ASP.NET Core application with a module containing a Controller and a View.

## Tutorial

<https://www.youtube.com/watch?v=LoPlECp31Oo>
