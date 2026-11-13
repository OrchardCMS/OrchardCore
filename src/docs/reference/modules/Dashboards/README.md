# Dashboards (`OrchardCore.Dashboards`)

The `OrchardCore.Dashboards` module provides dashboards: admin pages made of widgets that users add, arrange and resize. Each widget is a regular content item, so it can be edited, secured, versioned and localized like any other content.

A module registers a **dashboard type** for each page of widgets it needs. The [Admin Dashboard](../AdminDashboard/README.md), the landing page of the admin, is a dashboard type, and so is **My Dashboard**, where every user picks the widgets they want to see. Your own module can register a dashboard for its area, for example a sales dashboard, or a dashboard of its own for every user.

## Features

| Feature | Description |
|---------|-------------|
| `OrchardCore.Dashboards` | Serves the dashboards, their widgets and the `HtmlDashboardWidget` widget type. Enabled as a dependency by the features that register dashboard types. |
| `OrchardCore.Dashboards.User` | **My Dashboard**: gives every user with the `ManageOwnDashboard` permission a dashboard of their own, in the admin menu. |

## Dashboard types

A dashboard type has:

| Property | Description |
|----------|-------------|
| `Name` | The technical name, stored on its widgets and used in its URLs. |
| `DisplayName` | The title of the dashboard. |
| `Scope` | Who its widgets belong to, see [Scopes](#scopes). Defaults to `Site`. |
| `ViewPermission` | The permission required to view a `Site` dashboard. When not set, the `ManagePermission` is required. |
| `ManagePermission` | The permission required to add, arrange and remove its widgets. When not set, nobody can manage the dashboard. |

Every dashboard type is served at `/Admin/dashboards/{name}`, and arranged at `/Admin/dashboards/{name}/manage`.

### Scopes

| Scope | Description |
|-------|-------------|
| `Site` | The dashboard has one set of widgets, shared by everyone who can view it. The users with the `ManagePermission` arrange it for everyone. A user who manages the dashboard sees every widget; a user who only views it also needs the `ViewContent` permission (or the view permission of the widget type) on a widget to see it. |
| `User` | Every user has their own set of widgets, which only they see and arrange. A user needs the `ManagePermission` to have one. |

On a `User` dashboard, users manage their widgets without content permissions on the widget types: owning a widget of a dashboard they can manage grants them the permissions to view, edit, publish and delete it, and being able to manage a `User` dashboard grants them the permission to create widgets. The widget editor only adds a widget to a dashboard the user can manage. Other users, even with content permissions on the widget type, don't see these widgets on their own dashboards.

### Registering a dashboard type

Register a type from the `Startup` of your feature with `AddDashboardType()`, from the `OrchardCore.Dashboards.Abstractions` package, and make your module depend on the `OrchardCore.Dashboards` feature:

```csharp
public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddPermissionProvider<SalesPermissions>();

        services.AddDashboardType("Sales", LocalizationSource.Create<Startup>("Sales Dashboard"), type =>
        {
            type.ViewPermission = SalesPermissions.ViewSalesDashboard;
            type.ManagePermission = SalesPermissions.ManageSalesDashboard;
        });
    }
}
```

A dashboard of its own for every user only needs the `User` scope and the permission to have one:

```csharp
services.AddDashboardType("MyWork", LocalizationSource.Create<Startup>("My Work"), type =>
{
    type.Scope = DashboardScope.User;
    type.ManagePermission = MyWorkPermissions.ManageOwnWorkDashboard;
});
```

The types are stored in the `DashboardOptions`, so a dashboard type has no dependencies and keeps no state: it is shared by every request of the tenant. Registering a type with the name of another type replaces it.

To link to the dashboard, add an item to the admin menu pointing to the `Index` action of the `Dashboard` controller of the `OrchardCore.Dashboards` area, with the name of the type as the `type` route value:

```csharp
builder.Add(S["Sales Dashboard"], dashboard => dashboard
    .Action("Index", "Dashboard", new RouteValueDictionary
    {
        { "area", "OrchardCore.Dashboards" },
        { "type", "Sales" },
    })
    .Permission(SalesPermissions.ViewSalesDashboard)
    .LocalNav());
```

### Default type

`DashboardOptions.DefaultType` is the type of the widgets saved without one, for example by a recipe or by the content editor opened without a dashboard. The Admin Dashboard sets it to `Admin`, so widgets keep landing on the admin dashboard when nothing else is said.

### Using the dashboards from code

The `IDashboardManager` service returns the registered types (`GetType()`, `GetTypes()`), checks the access of a user to a dashboard (`CanViewAsync()`, `CanManageAsync()`), and returns the widgets a user sees on a dashboard, ordered by position (`GetWidgetsAsync()`).

## Widgets

To create a content type that can be used as a widget on the dashboards, set its stereotype to `DashboardWidget`. Every widget type can be added to every dashboard type.

Adding the `DashboardPart` to the type is optional: the module attaches it automatically to every type whose stereotype is `DashboardWidget`. The `DashboardPart` stores which dashboard a widget is on, and its place on it:

| Property | Description |
|----------|-------------|
| `DashboardType` | The name of the dashboard type of the widget, set when the widget is created and never changed afterwards. |
| `OwnerId` | The identifier of the user the widget belongs to, on a `User` dashboard. |
| `Position` | A numeric value indicating the order of the widget on the dashboard. |
| `Width` | The number of grid columns the widget spans, from 1 to the number of columns of the screen (6 on large screens). |
| `Height` | The number of grid rows the widget spans. |

### Adding a widget

1. Navigate to the dashboard, and click the **Manage Dashboard** button. It is shown to the users who can manage the dashboard.
2. Click **Add Widget** and select a widget type.
3. Complete the form and click **Publish**.

In manage mode, drag a widget by its handle to change its position, and resize it from its right and bottom edges. The changes are saved to both the latest and published versions of the widget, so the arrangement is shown immediately on the dashboard.

### Built-in widget types

| Content type | Description |
|--------------|-------------|
| `HtmlDashboardWidget` | A widget whose body is authored with the `HtmlBodyPart`. Useful for links, notes or any custom HTML. |

### Provisioning widgets with a recipe

Widgets are content items, so they can be created from a recipe using the `content` step. Set the `DashboardType` of the `DashboardPart` to the dashboard of the widget; a widget without one is added to the [default type](#default-type).

```json
{
  "steps": [
    {
      "name": "content",
      "data": [
        {
          "ContentItemId": "[js: uuid()]",
          "ContentType": "HtmlDashboardWidget",
          "DisplayText": "Orchard Core",
          "Latest": true,
          "Published": true,
          "HtmlDashboardWidget": {},
          "DashboardPart": {
            "DashboardType": "Admin",
            "Position": 0.0,
            "Width": 1.0,
            "Height": 2.0
          },
          "HtmlBodyPart": {
            "Html": "<p>Welcome!</p>"
          },
          "TitlePart": {
            "Title": "Orchard Core"
          }
        }
      ]
    }
  ]
}
```

## Templates

| Template | Description |
|----------|-------------|
| `DashboardWidget-{ContentType}.DetailAdmin.cshtml` | The widget of a content type on a dashboard. See the [Admin Dashboard styling](../AdminDashboard/README.md#styling) for an example. |
| `DashboardUnavailable-{DashboardType}.cshtml` | Rendered in place of a dashboard the user can't view, or that has no widgets they can see. The default `DashboardUnavailable.cshtml` shows a message; the Admin Dashboard renders the default landing page of the admin. |

While a dashboard is rendered, the `DashboardFeature` request feature tells the widget shapes which dashboard they are on (`DashboardType`) and whether it is in manage mode (`IsManageRequest`):

```razor
@inject Microsoft.AspNetCore.Http.IHttpContextAccessor HttpContextAccessor
@{
    var dashboardFeature = HttpContextAccessor.HttpContext.Features.Get<OrchardCore.Dashboards.DashboardFeature>();
}
```
