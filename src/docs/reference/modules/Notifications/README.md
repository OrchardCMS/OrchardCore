# Notifications (`OrchardCore.Notifications`)

The `Notifications` module offers a comprehensive infrastructure for managing notifications effectively. It includes a centralized notification center and streamlined mechanisms for dispatching notifications to users within the application.

Orchard Core also provides `INotifier` for transient UI notifications rendered by the active theme. Use `INotificationService` for persistent user notifications in the notification center, and use `INotifier` for per-request status messages such as success, warning, and error messages shown after an admin action.

## Configurations

You can customize the default notification options through the configuration provider using the following settings:

```json
{
  "OrchardCore_Notifications": {
    "TotalUnreadNotifications": 10,
    "DisableNotificationHtmlBodySanitizer": false,
    "AbsoluteCacheExpirationSeconds": 3600,
    "SlidingCacheExpirationSeconds": 0
  }
}
```

Available Options and Their Definitions:

| **Property**                           | **Description**                                                                                                                                                                                                                                                                                                                                  |
|----------------------------------------|--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `TotalUnreadNotifications`             | Specifies the maximum number of unread notifications displayed in the navigation bar. Default is 10.                                                                                                                                                                                                                                             |
| `DisableNotificationHtmlBodySanitizer` | Allows you to disable the default sanitization of the `HtmlBody` in notifications generated from workflows.                                                                                                                                                                                                                                      |
| `AbsoluteCacheExpirationSeconds`       | Specifies the absolute maximum duration, in seconds, for which the top unread user notifications are cached when caching is enabled. A value of 0 does not disable caching but indicates that there is no fixed expiration time for the cache. You can set this value to define a maximum lifespan for the cached data before it is invalidated. |
| `SlidingCacheExpirationSeconds`        | Gets or sets how long a cache entry can be inactive (e.g. not accessed) before it will be removed. This will not extend the entry lifetime beyond the absolute expiration (if set). To disable sliding expiration, you can set this value to 0.                                                                                                  |

## Notification Methods

There are many methods to send notifications to a user (e.x., Email, Web Push, Mobile Push, SMS, etc.) Any notification sent will be centralized in the notification center before being dispatched to users according to their specified preferences.

!!! info
When multiple notification methods are enabled, the user can opt-in/out any method they wishes to receive by editing their profile.

### Email Notifications

The `Email Notifications` feature offers a means to inform users by dispatching notifications through email.

!!! note
When using `Email Notifications` feature, you must also configure the [Email Service](../Email/README.md).

### SMS Notifications

The `SMS Notifications` feature offers a means to inform users by dispatching notifications through phone via SMS provider.

!!! note
When using `SMS Notifications` feature, you must also configure the [SMS Services](../Sms/README.md).

## Adding Custom Notification Provider

To add a new notification method like `Web Push`, `Mobile Push` or `SMS`, you can simply implement the `INotificationMethodProvider` interface. Then, register your new implementation. For example, in the `Email Notifications` feature we register the email notification provider like this

```csharp
[Feature("OrchardCore.Notifications.Email")]
public class EmailNotificationsStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<INotificationMethodProvider, EmailNotificationProvider>();
    }
}
```

## How to send a notification

You can send notification to a user via code by injecting `INotificationService` then calling the `SendAsync(...)` method. Alternatively, you can use workflows to notify a user about an event that took place.

```csharp
using OrchardCore.Notifications;

public sealed class PublishNotificationService
{
    private readonly INotificationService _notificationService;

    public PublishNotificationService(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    public Task NotifyAsync(IUser user)
    {
        return _notificationService.SendAsync(user, new Notification
        {
            Summary = "Content published",
            Body = "Your content item has been published successfully.",
        });
    }
}
```

## UI notifications with `INotifier`

`INotifier` is used for transient UI notifications that are rendered in the `Messages` zone of the current theme. These entries survive redirects for the next request and are intended for immediate feedback in the UI.

The default `TheAdmin` and `TheTheme` themes currently render these entries as Bootstrap toasts through the `NotifyMessages` and `Message` shapes.

```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using OrchardCore.DisplayManagement.Notify;

public sealed class SampleController : Controller
{
    private readonly INotifier _notifier;
    private readonly IHtmlLocalizer H;

    public SampleController(
        INotifier notifier,
        IHtmlLocalizer<SampleController> htmlLocalizer)
    {
        _notifier = notifier;
        H = htmlLocalizer;
    }

    public async Task<IActionResult> Publish()
    {
        await _notifier.SuccessAsync(H["The content item was published successfully."]);

        await _notifier.AddAsync(
            NotifyType.Warning,
            H["Review the generated slug before sharing it."],
            new NotifyContext { Milliseconds = 10000 });

        return RedirectToAction(nameof(Index));
    }
}
```

Available helpers include:

- `SuccessAsync(...)`
- `InformationAsync(...)`
- `WarningAsync(...)`
- `ErrorAsync(...)`
- `AddAsync(...)` when you need to set the `NotifyType` and optional `NotifyContext` explicitly.

`NotifyContext.Milliseconds` controls the auto-dismiss delay. A `null` value keeps the message visible until it is dismissed manually.

## Customizing the toast or restoring alert-style messages

The toast UI is only the default rendering. You can replace it by overriding the `Message` shape, and if you want to change the container or placement you can also override `NotifyMessages`.

To fully switch back to inline alert-style messages instead of toasts, override both shapes.

### `Message` shape

This shape renders a single notification entry.

=== "Razor"

    Create `Views/Message.cshtml` in your theme or admin theme:

    ```cshtml
    @{
        string type = Model.Type.ToString().ToLowerInvariant();
        var bsClassName = type switch
        {
            "information" => "info",
            "error" => "danger",
            _ => type,
        };
    }

    <div class="alert alert-@bsClassName alert-dismissible fade show message-@type" role="alert">
        @Model.Message
        <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
    </div>
    ```

=== "Liquid"

    Create `Views/Message.liquid` in your theme:

    ```liquid
    {% assign type = Model.Type | downcase %}
    {% assign bs_class = type %}
    {% if type == "information" %}
      {% assign bs_class = "info" %}
    {% elsif type == "error" %}
      {% assign bs_class = "danger" %}
    {% endif %}

    <div class="alert alert-{{ bs_class }} alert-dismissible fade show message-{{ type }}" role="alert">
        {{ Model.Message | raw }}
        <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
    </div>
    ```

=== "Admin Templates UI"

    To customize the admin toast item from the dashboard:

    1. Enable the `OrchardCore.AdminTemplates` feature.
    2. Go to **Design** -> **Admin Templates**.
    3. Create or edit the `Message` template.
    4. Paste the Liquid template shown above and save it.

    This overrides the admin `Message` shape without changing your front-end theme.

```liquid
    {% assign type = Model.Type | downcase %}
    {% assign bs_class = type %}
    {% if type == "information" %}
      {% assign bs_class = "info" %}
    {% elsif type == "error" %}
      {% assign bs_class = "danger" %}
    {% endif %}

    <div class="alert alert-{{ bs_class }} alert-dismissible fade show message-{{ type }}" role="alert">
        {{ Model.Message | raw }}
        <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
    </div>
```

### `NotifyMessages` shape

This shape renders the collection of notification entries. The default implementation adds the toast container and JavaScript that shows each Bootstrap toast. Replace it if you want inline messages in the normal `Messages` zone.

=== "Razor"

    Create `Views/NotifyMessages.cshtml` in your theme or admin theme:

    ```cshtml
    @using OrchardCore.DisplayManagement.Notify

    @foreach (var entry in (IEnumerable<NotifyEntry>)Model.Entries)
    {
        @await DisplayAsync(await New.Message(Type: entry.Type, Message: entry.Message, Milliseconds: entry.Milliseconds))
    }
    ```

=== "Liquid"

    Create `Views/NotifyMessages.liquid` in your theme:

    ```liquid
    {% for entry in Model.Entries %}
        {% shape "Message", type: entry.Type, message: entry.Message, milliseconds: entry.Milliseconds %}
    {% endfor %}
    ```

### Admin Templates UI

If you want to customize the full admin notification rendering from the dashboard instead of from a theme:

1. Enable the `OrchardCore.AdminTemplates` feature.
2. Go to **Design** -> **Admin Templates**.
3. Create or edit the `Message` template to change each notification item.
4. Create or edit the `NotifyMessages` template to change the container and placement.
5. Paste the Liquid versions shown above and save them.

Admin Templates affect only the admin UI. For the front-end, override the same shapes from your site theme.

## Workflow Activities

When `OrchardCore.Workflows` feature is enabled, you'll see new activities that would allow you to notify users using workflows. Here are some of the available workflow tasks

- Notify Content's Owner Task
- Notify User Task

## Videos

<iframe width="560" height="315" src="https://www.youtube-nocookie.com/embed/HMXPzkWE0ww" title="YouTube video player" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture; web-share" allowfullscreen></iframe>

<iframe width="560" height="315" src="https://www.youtube-nocookie.com/embed/Lj2g-bBy-I0" title="YouTube video player" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture; web-share" allowfullscreen></iframe>

<iframe width="560" height="315" src="https://www.youtube-nocookie.com/embed/_3pTgV4oTxU" title="YouTube video player" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture; web-share" allowfullscreen></iframe>

<iframe width="560" height="315" src="https://www.youtube-nocookie.com/embed/lGEsdPzHcog" title="YouTube video player" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture; web-share" allowfullscreen></iframe>

<iframe width="560" height="315" src="https://www.youtube-nocookie.com/embed/b-lHY0NxZNI" title="YouTube video player" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture; web-share" allowfullscreen></iframe>
