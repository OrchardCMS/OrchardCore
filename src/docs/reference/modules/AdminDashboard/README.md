# Admin Dashboard (`OrchardCore.AdminDashboard`)

Admin widgets are created using content items and can be secured like any other content item. To create a content type as a dashboard widget, set the stereotype to `DashboardWidget`.

### Creating a New Widget

To create a new widget of the recently created content type, perform the following steps:

1. Log in with a user with `ManageAdminDashboard` permission and navigate to the admin dashboard (accessible at `/admin` endpoint).
2. Locate and click on the `Manage Dashboard` button. If the button is not visible, ensure that you have the `ManageAdminDashboard` permission.
3. Click the `Add Widget` button and select the newly created content type.
4. Complete the form and click the `Publish` button.

Please note that users lacking the `ManageAdminDashboard` permission will require the `AccessAdminDashboard` permission to view the admin dashboard, in addition to the `ViewContent` or permission.

### Widget Configuration

Each widget has the following customizable settings:

| Option    | Description                                                   |
| --------- | ------------------------------------------------------------- |
| `Position`| A numeric value indicating the widget's order on the page.    |
| `Width`   | An integer value between 1 and 6, representing the widget's width on the screen. Please note that 1 indicates (1/6) of the screen, while 6 signifies full screen width. |
| `Height`  | An integer value between 1 and 6, representing the widget's height on the screen. Please note that 1 indicates (1/6) of the screen, while 6 signifies full screen height. |

### Styling

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

### Videos

<iframe width="560" height="315" src="https://www.youtube-nocookie.com/embed/MQuiXEnyEBw" frameborder="0" allow="accelerometer; autoplay; encrypted-media; gyroscope; picture-in-picture" allowfullscreen></iframe>

<iframe width="560" height="315" src="https://www.youtube-nocookie.com/embed/c7aiCPi2-BM" frameborder="0" allow="accelerometer; autoplay; encrypted-media; gyroscope; picture-in-picture" allowfullscreen></iframe>
