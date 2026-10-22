# Adding a Menu Item to the Admin Navigation from a Module

The `INavigationProvider` interface is the entry point to every task related to handling admin navigation menu items.  
In order to add menu items from your module you just need to create a class that implements that interface.

## What you will build

You will build a module that will add a menu item at the root level and two child menu items.  
Each menu item will point to its own view.

## What you will need

- The current version of the .NET SDK. You can download it from here <https://dotnet.microsoft.com/download>.
- A text editor and a terminal where you can type dotnet commands.

## Creating an Orchard Core CMS site and module

There are different ways to create sites and modules for Orchard Core. You can learn more about them [here](../../getting-started/templates/README.md). In this guide we will use our "Code Generation Templates".

You can install the latest released templates using this command:

```dotnet new install OrchardCore.ProjectTemplates@3.0.1-*```

!!! note
    To use the development branch of the template add `--nuget-source https://nuget.cloudsmith.io/orchardcore/preview/v3/index.json`

Create an empty folder that will contain your site. Open a terminal, navigate to that folder and run this:

```dotnet new occms -n MySite```

This creates a new Orchard Core CMS site in a new folder named `MySite`.
We can now create a new module with the following command:

```dotnet new ocmodulecms -n MyModule```

The module is created in the `MyModule` folder.
The next step is to reference the module from the application, by adding a project reference:

```dotnet add MySite reference MyModule```

We also need a reference to the `OrchardCore.Admin` package in order to be able to implement the required interfaces:

```dotnet add .\MyModule\MyModule.csproj package OrchardCore.Admin --version 3.0.1-*```

!!! note
    To use the development branch of the template add ` --source https://nuget.cloudsmith.io/orchardcore/preview/v3/index.json --version 3.0.1-*`

## Adding our controller and views

### Adding the controller

Create a `DemoNavController.cs` file to the `.\MyModule\Controllers` folder, with these contents:

#### DemoNavController.cs

```csharp
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OrchardCore.Admin;

namespace MyModule.Controllers;

[Admin]
public sealed class DemoNavController : Controller
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
```

!!! tip
   The `[Admin]` attribute ensures the controller is using the Admin theme and users have the permission to access it.  
   Another way to have this behavior would have been to name this class `AdminController`.

### Adding the views

Create a folder `.\MyModule\Views\DemoNav`, and add to it these two files:

#### ChildOne.cshtml

```html
<p>View One</p>
```

#### ChildTwo.cshtml

```html
<p>View Two</p>
```

## Adding the menu items

Now you just need to add a class that implements `INavigationProvider` interface.  
By convention, we call these classes `AdminMenu.cs` and put it in the root of our module's folder.

### AdminMenu.cs

```csharp
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace MyModule;

public sealed class AdminMenu : INavigationProvider
{
    internal readonly IStringLocalizer S;

    public AdminMenu(IStringLocalizer<AdminMenu> localizer)
    {
        S = localizer;
    }

    public ValueTask BuildNavigationAsync(string name, NavigationBuilder builder)
    {
        // We want to add our menus to the "admin" menu only.
        if (!String.Equals(name, "admin", StringComparison.OrdinalIgnoreCase))
        {
            return ValueTask.CompletedTask;
        }

        // Adding our menu items to the builder.
        // The builder represents the full admin menu tree.
        builder
            .Add(S["My Root View"], S["My Root View"].PrefixPosition(),  rootView => rootView               
                .Add(S["Child One"], S["Child One"].PrefixPosition(), childOne => childOne
                    .Action("ChildOne", "DemoNav", new { area = "MyModule"}))
                .Add(S["Child Two"], S["Child Two"].PrefixPosition(), childTwo => childTwo
                    .Action("ChildTwo", "DemoNav", new { area = "MyModule"})));

        return ValueTask.CompletedTask;
    }
}
```

!!! note
    We suggest to use the `PrefixPosition` extension method for the second parameter (`position`) if you want to keep an alphabetical sort when the strings will be translated in other languages.

### Positioning menu items

The second parameter of `Add(...)` is the **position**, the same position mechanism used everywhere in Orchard Core (for example, [shape placement](../../reference/modules/Placement/README.md#position-ordering-rules)). Menu items — at every level of the tree — are sorted by it. The full ordering, from first to last, is:

```
start  <  before  <  numbers  <  after  <  free-text (PrefixPosition)  <  end
```

- **Numeric positions** (`"1"`, `"2.5"`, `"10"`) are compared numerically and give you precise, explicit placement. Use them for top-level items and for pinning a specific item to a known slot.
- **`before` / `after`** are anchors *within* the numeric range: `before` sorts ahead of all numbers, `after` sorts after all numbers. They both support sub-ordering, e.g. `"after.50"` then `"after.100"`.
- **`PrefixPosition()`** produces a **non-numeric** value that alphabetizes an item against its siblings (surviving translation). Because it is not a number, it sorts **after `after`**. This is exactly what you want for leaf items in a shared submenu (they line up alphabetically after any pinned, numbered items), which is why the guide uses it above.
- **`start` / `end`** are true terminals: `start` sorts before *everything* (including `before`), and `end` sorts after *everything* (including `PrefixPosition()` values). They also support sub-ordering (`"end.50"` before `"end.100"`).

!!! warning "`after` is not the last position; `end` is"
    Because `PrefixPosition()` values sort after `after`, an item positioned with `"after"` (or `"after.100"`) is **not** guaranteed to be last — any sibling using `PrefixPosition()` will come after it. If you need an item to be genuinely first or last regardless of its siblings, use `start` / `end` instead of `before` / `after`. For this reason the built-in **Settings** and **Tools** top-level menus use `"end.100"` and `"end.50"`, so custom top-level items added with `PrefixPosition()` appear *before* them.

!!! tip
    For a **top-level** menu item you almost always want a deterministic slot, so prefer a numeric or `end`/`start` position over `PrefixPosition()`. Reserve `PrefixPosition()` for the leaf items inside a submenu, where alphabetical ordering is the goal.

Then you have to register this service in the `Startup.cs` file of the module.

At the top of the `Startup.cs` file, add this `using` statement:

```csharp
using OrchardCore.Navigation;
```

Add this line to the `ConfigureServices()` method:

```csharp
services.AddNavigationProvider<AdminMenu>();
```

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

Open a browser on <https://localhost:5001>

If you have not already set up your site, select __Blank Site__ as the recipe, and use __SQLite__ as the database.

Once your site is ready, you should see a __The page could not be found.__ message which is expected for a __Blank Site__ recipe.

Enter the Admin section by opening <https://localhost:5001/admin> and logging in.

Using the left menu go to __Tools: Features__, search for your module, __MyModule__, and enable it.

Now your module is enabled and you should see a new entry on the admin.  
Click on the new menu items to render the Views we created earlier.

## Summary

You just learned how to add menu items to the Admin Navigation.
