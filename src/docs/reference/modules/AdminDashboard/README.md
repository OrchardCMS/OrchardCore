# Admin Dashboard (`OrchardCore.AdminDashboard`)

The `OrchardCore.AdminDashboard` module lets you compose the landing page of the admin area (`/admin`) from widgets. Each widget is a regular content item, so it can be edited, secured, versioned and localized like any other content.

This page describes the options available to customize the dashboard: permissions, the built-in widget types, widget configuration, arranging widgets, and how to override their look.

## Permissions

| Permission             | Description                                                                 |
|------------------------|-----------------------------------------------------------------------------|
| `AccessAdminDashboard` | Allows viewing the admin dashboard and its widgets.                         |
| `ManageAdminDashboard` | Allows adding, editing, arranging and removing dashboard widgets. Implies `AccessAdminDashboard`. |

By default `ManageAdminDashboard` is granted to the `Administrator` role, while `AccessAdminDashboard` is also granted to the `Editor`, `Moderator`, `Author` and `Contributor` roles.

A user with only `AccessAdminDashboard` additionally needs the `ViewContent` permission (or a per-type view permission) on a widget for that widget to be displayed.

## Dashboard widgets

Admin widgets are created using content items and can be secured like any other content item. To create a content type that can be used as a dashboard widget, set its stereotype to `DashboardWidget`.

Adding the `DashboardPart` to the type is optional: the module attaches it automatically to every type whose stereotype is `DashboardWidget`. `DashboardPart` is what stores the position and size of the widget.

### Creating a New Widget

To create a new widget of the recently created content type, perform the following steps:

1. Log in with a user with `ManageAdminDashboard` permission and navigate to the admin dashboard (accessible at `/admin` endpoint).
2. Locate and click on the `Manage Dashboard` button. If the button is not visible, ensure that you have the `ManageAdminDashboard` permission.
3. Click the `Add Widget` button and select the newly created content type.
4. Complete the form and click the `Publish` button.

Please note that users lacking the `ManageAdminDashboard` permission will require the `AccessAdminDashboard` permission to view the admin dashboard, in addition to the `ViewContent` or permission.

### Built-in widget types

The module ships with a few widget types you can use out of the box:

| Content type           | Description                                                                 |
|------------------------|-----------------------------------------------------------------------------|
| `HtmlDashboardWidget`  | A widget whose body is authored with the `HtmlBodyPart`. Useful for links, notes or any custom HTML. |
| `ContentsMetadata`     | Displays the metadata (author, dates, …) of content items.                  |
| `ContentsTags`         | Displays the tags of content items.                                         |

You can create your own widget types at any time by adding a content type with the `DashboardWidget` stereotype.

### Widget Configuration

Each widget has the following customizable settings, stored on the `DashboardPart`:

| Option     | Description                                                                                                                                                               |
|------------|---------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `Position` | A numeric value indicating the widget's order on the page.                                                                                                                |
| `Width`    | An integer value between 1 and 6, representing the widget's width on the screen. Please note that 1 indicates (1/6) of the screen, while 6 signifies full screen width.   |
| `Height`   | An integer value between 1 and 6, representing the widget's height on the screen. Please note that 1 indicates (1/6) of the screen, while 6 signifies full screen height. |

### Arranging widgets

In `Manage Dashboard` mode, widgets can be reordered and resized directly from the UI:

- Drag a widget by its handle to change its `Position`.
- Resize a widget to change its `Width` and `Height`.

Changes are persisted to each widget's `DashboardPart` when saved. Both the latest and published versions are updated so the arrangement is reflected immediately on the dashboard.

## Styling

If you wish to modify the look of your widget, consider incorporating a template named `DashboardWidget-{ContentType}.DetailAdmin.cshtml`, where `{ContentType}` represents the specific technical name of your content type. Below is an illustration of a template that introduces spacing around the widget:

```
<div class="card h-100 @string.Join(' ', Model.Classes.ToArray())">
    @if (Model.Header != null || Model.Leading != null || Model.ActionsMenu != null)
    {
        <div class="card-header">
            @await DisplayAsync(Model.Leading)
            @await DisplayAsync(Model.Header)
            @if (Model.ActionsMenu != null)
            {
                <div class="btn-group float-end" title="@T["Actions"]">
                    <button type="button" class="btn btn-sm " data-bs-toggle="dropdown" aria-haspopup="true" aria-expanded="false">
                        <i class="fa-solid fa-ellipsis-v" aria-hidden="true"></i>
                    </button>
                    <div class="actions-menu dropdown-menu">
                        @await DisplayAsync(Model.ActionsMenu)
                    </div>
                </div>
            }
        </div>
    }
    <div class="dashboard-body-container card-body p-2 h-100">
        @if (Model.Tags != null || Model.Meta != null)
        {
            <div class="dashboard-meta">
                @await DisplayAsync(Model.Meta)
                @await DisplayAsync(Model.Tags)
            </div>
        }
        @await DisplayAsync(Model.Content)
    </div>
    @if (Model.Footer != null)
    {
        <div class="card-footer">
            @await DisplayAsync(Model.Footer)
        </div>
    }
</div>
```

## Provisioning widgets with a recipe

Dashboard widgets are content items, so they can be created from a recipe using the `content` step. This is handy to ship a default dashboard with your site. The following sample adds an `HtmlDashboardWidget` with a list of links, positioned first and two rows tall:

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
            "Position": 0.0,
            "Width": 1.0,
            "Height": 2.0
          },
          "HtmlBodyPart": {
            "Html": "<ul class=\"list-group list-group-flush\">\n\t<li class=\"list-group-item\"><a href=\"https://orchardcore.net\" target=\"_blank\">Orchard Core site</a></li>\n\t<li class=\"list-group-item\"><a href=\"https://docs.orchardcore.net\" target=\"_blank\">Orchard Core docs</a></li>\n</ul>"
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

A ready-to-use `dashboard-widgets-samples` recipe is included with the module.

### Videos

<iframe width="560" height="315" src="https://www.youtube-nocookie.com/embed/MQuiXEnyEBw" frameborder="0" allow="accelerometer; autoplay; encrypted-media; gyroscope; picture-in-picture" allowfullscreen></iframe>

<iframe width="560" height="315" src="https://www.youtube-nocookie.com/embed/c7aiCPi2-BM" frameborder="0" allow="accelerometer; autoplay; encrypted-media; gyroscope; picture-in-picture" allowfullscreen></iframe>
