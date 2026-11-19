# Creating a Dashboard Type

The [Dashboards](../../reference/modules/Dashboards/README.md) module turns pages of the admin into dashboards: users add widgets to them, then drag and resize the widgets in a grid. The admin landing page is one of them, and every module can register its own types of dashboards, for its area or for every user.

## What you will build

You will build a module that registers two dashboards:

- **Sales**: a dashboard shared by the sales team. Everyone with a `ViewSalesDashboard` permission sees it, and the users with a `ManageSalesDashboard` permission arrange it for everyone.
- **My Work**: a dashboard of their own for every user with a `ManageOwnWorkDashboard` permission, made of the widgets they choose.

You will also add a widget type of your own, link the dashboards from the admin menu, provision their default widgets with a recipe and customize their templates.

## What you will need

- An Orchard Core CMS site and a module referenced by it, see [Creating a Custom Module](../create-install-custom-module/README.md). This guide uses a module named `MyModule`.
- The current version of the .NET SDK.

## Referencing the Dashboards module

A dashboard type is registered with the `OrchardCore.Dashboards.Abstractions` package. Add it to the module:

```bash
dotnet add MyModule package OrchardCore.Dashboards.Abstractions
```

The dashboards are served by the `OrchardCore.Dashboards` feature, so make your module depend on it in its `Manifest.cs`. Enabling your module then enables the dashboards:

```csharp
[assembly: Module(
    Name = "My Module",
    Author = "The Orchard Core Team",
    Version = "1.0.0",
    Description = "Dashboards for the sales team.",
    Dependencies =
    [
        "OrchardCore.Dashboards",
    ],
    Category = "Content Management"
)]
```

## Declaring the permissions

A dashboard type has a permission to view it and a permission to manage it, that is to add, arrange and remove its widgets. Declare them in a permission provider, and grant them to roles by default with stereotypes:

```csharp
using OrchardCore;
using OrchardCore.Localization;
using OrchardCore.Security.Permissions;

namespace MyModule;

public sealed class SalesPermissions : IPermissionProvider
{
    public static readonly Permission ManageSalesDashboard = new("ManageSalesDashboard", LocalizationSource.Create<SalesPermissions>("Manage the sales dashboard"));
    public static readonly Permission ViewSalesDashboard = new("ViewSalesDashboard", LocalizationSource.Create<SalesPermissions>("View the sales dashboard"), [ManageSalesDashboard]);
    public static readonly Permission ManageOwnWorkDashboard = new("ManageOwnWorkDashboard", LocalizationSource.Create<SalesPermissions>("Manage their own work dashboard"));

    private readonly IEnumerable<Permission> _allPermissions =
    [
        ViewSalesDashboard,
        ManageSalesDashboard,
        ManageOwnWorkDashboard,
    ];

    public Task<IEnumerable<Permission>> GetPermissionsAsync()
        => Task.FromResult(_allPermissions);

    public IEnumerable<PermissionStereotype> GetDefaultStereotypes() =>
    [
        new PermissionStereotype
        {
            Name = OrchardCoreConstants.Roles.Administrator,
            Permissions = _allPermissions,
        },
        new PermissionStereotype
        {
            Name = OrchardCoreConstants.Roles.Editor,
            Permissions = [ViewSalesDashboard, ManageOwnWorkDashboard],
        },
    ];
}
```

`ViewSalesDashboard` is implied by `ManageSalesDashboard`, so the users who arrange the dashboard can also view it.

## Registering the dashboard types

Register the types from the `Startup` class of the module with `AddDashboardType()`. It takes the technical name of the type, which is stored on its widgets and used in its URLs, and the name shown to users as the title of the dashboard:

```csharp
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Dashboards;
using OrchardCore.Localization;
using OrchardCore.Modules;
using OrchardCore.Security.Permissions;

namespace MyModule;

public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddPermissionProvider<SalesPermissions>();

        // One set of widgets, shared by the sales team.
        services.AddDashboardType("Sales", LocalizationSource.Create<Startup>("Sales Dashboard"), type =>
        {
            type.ViewPermission = SalesPermissions.ViewSalesDashboard;
            type.ManagePermission = SalesPermissions.ManageSalesDashboard;
        });

        // A set of widgets of their own for every user.
        services.AddDashboardType("MyWork", LocalizationSource.Create<Startup>("My Work"), type =>
        {
            type.Scope = DashboardScope.User;
            type.ManagePermission = SalesPermissions.ManageOwnWorkDashboard;
        });
    }
}
```

That's all a dashboard needs. Run the site, enable your module and navigate to:

- `/Admin/dashboards/Sales` to view the sales dashboard, and `/Admin/dashboards/Sales/manage` to arrange it.
- `/Admin/dashboards/MyWork` to view your own work dashboard, and `/Admin/dashboards/MyWork/manage` to arrange it.

In manage mode, the **Add Widget** button lists the widget types, for example the built-in **Html Dashboard Widget**. A widget added from a dashboard belongs to that dashboard, and on the **My Work** dashboard to the user who added it.

!!! note
    A dashboard type has no dependencies and keeps no state: it is stored in the `DashboardOptions` and shared by every request of the tenant. Registering a type with the name of another type replaces it.

### Choosing a scope

| Scope | Widgets | Who views the dashboard | Who arranges it |
|-------|---------|-------------------------|-----------------|
| `DashboardScope.Site` (default) | One set, shared. | The users with the `ViewPermission`, or the `ManagePermission` when no `ViewPermission` is set. A user who only views the dashboard also needs the `ViewContent` permission on a widget to see it. | The users with the `ManagePermission`. |
| `DashboardScope.User` | One set per user. | Each user sees only their own widgets. | Each user with the `ManagePermission` arranges their own. |

On a `User` dashboard, users don't need content permissions on the widget types: they can create widgets, and view, edit, publish and delete the widgets they own. Other users, even administrators, don't see them on their own dashboards.

## Linking the dashboards from the admin menu

The dashboards are served by the `Index` action of the `Dashboard` controller, in the `OrchardCore.Dashboards` area, with the name of the type as the `type` route value. Add them to the admin menu with a [navigation provider](../add-admin-menu/README.md):

```csharp
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace MyModule;

public sealed class AdminMenu : AdminNavigationProvider
{
    private static readonly RouteValueDictionary s_salesRouteValues = new()
    {
        { "area", "OrchardCore.Dashboards" },
        { "type", "Sales" },
    };

    private static readonly RouteValueDictionary s_myWorkRouteValues = new()
    {
        { "area", "OrchardCore.Dashboards" },
        { "type", "MyWork" },
    };

    internal readonly IStringLocalizer S;

    public AdminMenu(IStringLocalizer<AdminMenu> stringLocalizer)
    {
        S = stringLocalizer;
    }

    protected override ValueTask BuildAsync(NavigationBuilder builder)
    {
        builder
            .Add(S["Sales Dashboard"], sales => sales
                .Action("Index", "Dashboard", s_salesRouteValues)
                .Permission(SalesPermissions.ViewSalesDashboard)
                .LocalNav())
            .Add(S["My Work"], myWork => myWork
                .Action("Index", "Dashboard", s_myWorkRouteValues)
                .Permission(SalesPermissions.ManageOwnWorkDashboard)
                .LocalNav());

        return ValueTask.CompletedTask;
    }
}
```

Register it in the `Startup` class:

```csharp
services.AddNavigationProvider<AdminMenu>();
```

## Adding a widget type

Any content type whose stereotype is `DashboardWidget` can be added to the dashboards: create one from **Content Definition > Content Types**, or from a migration of your module. The `DashboardPart`, which stores the dashboard, position and size of a widget, is attached to the type automatically.

```csharp
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Dashboards;
using OrchardCore.Data.Migration;

namespace MyModule;

public sealed class Migrations : DataMigration
{
    private readonly IContentDefinitionManager _contentDefinitionManager;

    public Migrations(IContentDefinitionManager contentDefinitionManager)
    {
        _contentDefinitionManager = contentDefinitionManager;
    }

    public async Task<int> CreateAsync()
    {
        await _contentDefinitionManager.AlterTypeDefinitionAsync("SalesTargetWidget", type => type
            .DisplayedAs("Sales Target Widget")
            .Stereotype(DashboardConstants.WidgetStereotype)
            .Draftable()
            .Securable()
            .WithPart("TitlePart", part => part.WithPosition("0"))
            .WithPart("HtmlBodyPart", part => part
                .WithPosition("1")
                .WithEditor("Wysiwyg"))
        );

        return 1;
    }
}
```

Register the migration in the `Startup` class:

```csharp
services.AddDataMigration<Migrations>();
```

Every widget type can be added to every dashboard type. The widget is displayed with the `DetailAdmin` display type, inside a card whose zones are `Header`, `Meta`, `Tags`, `Content` and `Footer`: write [display drivers](../../reference/modules/DisplayManagement/README.md) for the parts of your widget type, with a `DetailAdmin` location, to show your own data in it, for example the sales figures of the month.

## Provisioning the default widgets

Widgets are content items, so a recipe of your module can create the widgets a dashboard starts with. Set the `DashboardType` of the `DashboardPart` to the name of the dashboard type:

```json
{
  "steps": [
    {
      "name": "content",
      "data": [
        {
          "ContentItemId": "[js: uuid()]",
          "ContentType": "SalesTargetWidget",
          "DisplayText": "Targets of the quarter",
          "Latest": true,
          "Published": true,
          "DashboardPart": {
            "DashboardType": "Sales",
            "Position": 0.0,
            "Width": 2.0,
            "Height": 1.0
          },
          "TitlePart": {
            "Title": "Targets of the quarter"
          },
          "HtmlBodyPart": {
            "Html": "<p>Reach 120 new customers.</p>"
          }
        }
      ]
    }
  ]
}
```

`Width` and `Height` are numbers of grid columns and rows. The grid has 6 columns on large screens, fewer on smaller ones.

!!! warning
    A widget without a `DashboardType` is added to the default type of the dashboards, the admin dashboard when the Admin Dashboard feature is enabled. Widgets of a `User` dashboard also need the `OwnerId` of their user, so they are created by the users rather than by a recipe.

## Customizing the templates

The dashboards are rendered with shapes, so a theme or a module can override them:

| Template | Description |
|----------|-------------|
| `DashboardWidget-SalesTargetWidget.DetailAdmin.cshtml` | The card of the widgets of the `SalesTargetWidget` type. Start from the [default card](../../reference/modules/AdminDashboard/README.md#styling). |
| `DashboardUnavailable-Sales.cshtml` | Rendered in place of the sales dashboard for a user who can't view it, or who sees none of its widgets. The default template shows a message. |

For example, to point the users who see an empty sales dashboard to the sales reports:

```razor
<div class="alert alert-info" role="alert">
    @T["The sales dashboard has no widgets yet. Meanwhile, see the sales reports."]
</div>
```

While a dashboard is rendered, the `DashboardFeature` request feature gives the widget shapes the dashboard they are on (`DashboardType`) and whether it is in manage mode (`IsManageRequest`), to render a widget differently on each dashboard.

## Reading the dashboards from code

The `IDashboardManager` service of the `OrchardCore.Dashboards` module, from the `OrchardCore.Dashboards` package, returns the registered types, checks the access of a user to a dashboard, and returns the widgets a user sees on it:

```csharp
var type = _dashboardManager.GetType("Sales");

if (await _dashboardManager.CanViewAsync(User, type))
{
    var widgets = await _dashboardManager.GetWidgetsAsync(User, type, latest: false);
}
```

## Summary

You registered a dashboard shared by a team and a dashboard of their own for every user with `AddDashboardType()`, linked them from the admin menu, added a widget type and provisioned default widgets. See the [Dashboards reference](../../reference/modules/Dashboards/README.md) for every option.
