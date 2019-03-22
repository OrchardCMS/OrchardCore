# Adding a Menu to the Admin from a Module

The `INavigationProvider` is the entry point to every task related to handling admin menu items.

In order to add menu items from your module you just need to create a class that implements that interface.

## What you will build ##

You will build a module that will add a menu item at the root level and two child menu items. Each menu item will point to its own view

## What you will need ##

- Latest versions for both Runtime and SDK of .NET Core. You can download them from here [https://www.microsoft.com/net/download/core](https://www.microsoft.com/net/download/core).

- A text editor and a terminal where you cant type dotnet commands.

## Creating an Orchard Core site and module ##

There are different ways to create sites and modules for Orchard Core. You can learn more about them [here](../../templates/README.md). In this guide we will use our "Code Generation Templates". 

You can install the latest templates using this command:

```dotnet new -i OrchardCore.ProjectTemplates::1.0.0-beta3-* --nuget-source https://www.myget.org/F/orchardcore-preview/api/v3/index.json```


>Note: At the time of writing this, Orchard Core is still in beta. This command install the latest available templates. For the majority of the scenarios we would still recommend that.

Create an empty folder that will contain your site. Open a terminal, navigate to that folder and run this:

```dotnet new occms -n MySite```

```dotnet new ocmodulecms -n MyModule```

```dotnet add MySite/MySite.csproj reference MyModule/MyModule.csproj```

The first two commands will create a new Orchard Core Cms application ready to be setup, and a module.
The last command will create a reference to the module on the application.

In order to add views to the Admin we need that our module references the `OrchardCore.Admin` package. So you need to run this command:

```dotnet add .\MyModule\MyModule.csproj package OrchardCore.Admin --source https://www.myget.org/F/orchardcore-preview/api/v3/index.json --version 1.0.0-*```


## Adding our controller and views ##

Add a "DemoNavController.cs" file to the ".\MyModule\Controllers" folder, with these contents:


```csharp
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OrchardCore.Admin;

namespace MyModule.Controllers
{
    [Admin]
    public class DemoNavController : Controller
    {
        public ActionResult ChildOne()
        {
            return View();
        }

        public ActionResult ChildTwo()
        {
            return View();
        }
    }
}
```

The `Admin` attribute on will make this controller accesible from the Admin only.

Create a folder ".\MyModule\Views\DemoNav", and add to it these two views:

"ChildOne.cshtml"
```csharp
<p>View One</p>
```

"ChildTwo.cshtml"
```csharp
<p>View Two</p>
```

## Adding the menu items ##

Now you just need to add a class that implements `INavigationProvider` interface.
By convention, we call these classes `AdminMenu.cs` and put it in the root of our module's folder.


```csharp
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace MyModule
{
    public class AdminMenu : INavigationProvider
    {
        public AdminMenu(IStringLocalizer<AdminMenu> localizer)
        {
            T = localizer;
        }
        public IStringLocalizer T { get; set; }

        public Task BuildNavigationAsync(string name, NavigationBuilder builder)
        {
            // We want to add our menus to the "admin" menu only.
            if (!String.Equals(name, "admin", StringComparison.OrdinalIgnoreCase)){
                return Task.CompletedTask;
            }

            // Adding our menu items to the builder.
            // The builder represents the full admin menu tree.
            builder
            .Add(T["My Root View"], "after",  rootView => rootView               
                .Add(T["Child One"],"1", childOne => childOne
                    .Action("ChildOne", "DemoNav", new { area = "MyModule"}))
                .Add(T["Child Two"], "2", childTwo => childTwo
                    .Action("ChildTwo", "DemoNav", new { area = "MyModule"})));

            return Task.CompletedTask;
        }
    }
}
```

Then you have to register your `INavigationProvider` on the "Startup.cs" file of the module.

Add this `using` statement to the "Startup.cs" file:

```csharp
using OrchardCore.Navigation;
```

Add this line to the `ConfigureServices()` Method:

```csharp
services.AddScoped<INavigationProvider, AdminMenu>();
```

## Running your app and testing your menu items ##

You are ready now to test your work.

From the root of the folder containing both projects, run this command:

```
dotnet run --project .\MySite\MySite.csproj
```
Your application should be built and run.

Now you can browse to it and setup your site.

For this example you will probably want to use Sqlite as database.

For simplicity, use the "Blank Site" recipe.

Once your site is setup, you will see a "The page could not be found." message. That's expected for the "Blank Site" recipe.

Login to the admin at "/admin".

Then using the left menu go to "Configuration: Modules", search for your module, "MyModule", and enable it.

Now your module is enabled and you should see a new entry on the admin. And by using it you should be able to navigate through your two views.


## Summary ##

You just learned how to add menus to the Admin, one of the most common tasks when developing Orchard Core Modules.

For the sake of clarity we used just a little bit of the features available trough the `NavigationBuilder` object.

If you want to learn more there are plenty of examples on the Orchard Core Github Repository. Just search for "AdminMenu.cs" files.
