# Notifications (`OrchardCore.Notifications`)

The `Notifications` module offers a comprehensive infrastructure for managing notifications effectively. It includes a centralized notification center and streamlined mechanisms for dispatching notifications to users within the application.

Orchard Core also provides `INotifier` for transient UI notifications rendered by the active theme. Use `INotificationService` for persistent user notifications in the notification center, and use `INotifier` for per-request status messages such as success, warning, and error messages shown after an admin action.

## Configurations

You can customize the default notification options through the configuration provider using the following settings:

```json
{
  "OrchardCore": {
    "Notifications": {
      "TotalUnreadNotifications": 10,
      "MaxEvaluatedUnreadNotifications": 100,
      "DisableNotificationHtmlBodySanitizer": false,
      "AbsoluteCacheExpirationSeconds": 3600,
      "SlidingCacheExpirationSeconds": 0
    }
  }
}
```

Available Options and Their Definitions:

| **Property**                           | **Description**                                                                                                                                                                                                                                                                                                                                  |
|----------------------------------------|--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `TotalUnreadNotifications`             | Specifies the maximum number of unread notifications displayed in the navigation bar. Default is 10.                                                                                                                                                                                                                                             |
| `MaxEvaluatedUnreadNotifications`      | Specifies the maximum number of unread notifications checked by the [visibility handlers](#hiding-notifications-the-user-can-no-longer-see) to count the unread notifications of the navigation bar. A larger count is shown as this number followed by a plus sign. Only used when a module registers a visibility handler. Default is 100.                                     |
| `DisableNotificationHtmlBodySanitizer` | Allows you to disable the default sanitization of the `HtmlBody` in notifications generated from workflows.                                                                                                                                                                                                                                      |
| `AbsoluteCacheExpirationSeconds`       | Specifies the absolute maximum duration, in seconds, for which the top unread user notifications are cached when caching is enabled. A value of 0 does not disable caching but indicates that there is no fixed expiration time for the cache. You can set this value to define a maximum lifespan for the cached data before it is invalidated. |
| `SlidingCacheExpirationSeconds`        | Gets or sets how long a cache entry can be inactive (e.g. not accessed) before it will be removed. This will not extend the entry lifetime beyond the absolute expiration (if set). To disable sliding expiration, you can set this value to 0.                                                                                                  |

## Notification Methods

There are many methods to send notifications to a user (e.g., Email, Web Push, Mobile Push, SMS, etc.). Any notification sent will be centralized in the notification center before being dispatched to users according to their specified preferences.

!!! info
When multiple notification methods are enabled, the user can opt in/out of any method they wish to receive by editing their profile.

### Email Notifications

The `Email Notifications` feature offers a means to inform users by dispatching notifications through email.

!!! note
When using `Email Notifications` feature, you must also configure the [Email Service](../Email/README.md).

### SMS Notifications

The `SMS Notifications` feature offers a means to inform users by dispatching notifications by phone via an SMS provider.

!!! note
When using `SMS Notifications` feature, you must also configure the [SMS Services](../Sms/README.md).

## Adding Custom Notification Provider

To add a new notification method like `Web Push`, `Mobile Push`, or `SMS`, you can simply implement the `INotificationMethodProvider` interface. Then, register your new implementation. For example, in the `Email Notifications` feature, we register the email notification provider like this:

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

You can send a notification to a user via code by injecting `INotificationService`, then calling the `SendAsync(...)` method. Alternatively, you can use workflows to notify a user about an event that took place.

```csharp
using OrchardCore.Notifications;
using OrchardCore.Notifications.Models;

public sealed class PublishNotificationService
{
    private readonly INotificationService _notificationService;

    public PublishNotificationService(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    public Task NotifyAsync(IUser user)
    {
        return _notificationService.SendAsync(user, new NotificationMessage
        {
            Subject = "Content published",
            Summary = "Your content item has been published successfully.",
        });
    }
}
```

The notification is stored in the notification center of the user, then sent with each notification method the user receives. `SendAsync()` returns a `NotificationSendResult`, whose `Notification` property is the stored notification.

A message has these properties, of which only the `Subject` is required:

| Property | Description |
|---|---|
| `Subject` | The title of the notification. It cannot contain HTML. |
| `Summary` | A short description shown under the subject in the navbar and the notification center. It can contain HTML. |
| `TextBody`, `HtmlBody`, `IsHtmlPreferred` | The body sent by the notification methods, for example in an email. The notification center shows it on the page of the notification. |
| `Url` | The page the notification points to. See [Linking a notification to a page](#linking-a-notification-to-a-page). |
| `Type` | The kind of notification, for example `Mention`. See [Notification types](#notification-types). |
| `ReferenceId` | The id of the record the notification is about, for example the id of a comment. See [Managing sent notifications](#managing-sent-notifications). |

## Using the notification center as the inbox of a module

A module that notifies users about events aimed at them, such as "you were mentioned in a comment", "a task was assigned to you" or "your approval is needed", can use the notification center as its inbox instead of building its own page. The rest of this section uses a module with comments that notifies the users mentioned in a comment as an example. For a step-by-step guide to adding a type of your own, see [Creating a custom notification type](#creating-a-custom-notification-type).

### Linking a notification to a page

Most notifications point at something, such as a comment, a content item or an order. Set the `Url` of the message to the page instead of adding a link to the summary:

```csharp
await _notificationService.SendAsync(user, new NotificationMessage
{
    Subject = "Alice mentioned you",
    Summary = "In a comment on 'Release planning'.",
    Url = $"~/projects/{projectId}/comments/{commentId}",
});
```

The URL is either an application-relative URL starting with `~/`, resolved against the URL prefix of the tenant when the notification is opened, or an absolute `http` or `https` URL. Any other URL, for example a `javascript:` URL, is ignored and a warning is logged.

The subject of the notification links to it in the navbar and in the notification center. Opening the link goes through `/Notifications/Open/{notificationId}`, which marks the notification as read, then redirects to its URL. A notification without a URL opens its page in the notification center, which shows its body.

### Notification types

Set a `Type` to tell the notifications of a module apart, and a `ReferenceId` to find the notifications of a record again:

```csharp
await _notificationService.SendAsync(user, new NotificationMessage
{
    Subject = "Alice mentioned you",
    Url = $"~/projects/{projectId}/comments/{commentId}",
    Type = "Mention",
    ReferenceId = commentId,
});
```

Both are stored in the `NotificationIndex`, so the notification center can filter by type and code can query by reference. The type is limited to 64 characters and the reference id to 255 characters.

Register the type in the startup of the module, to give it a display name:

```csharp
services.AddNotificationType("Mention", LocalizationSource.Create("Mentions", typeof(Startup)), type =>
{
    type.Description = LocalizationSource.Create("When someone mentions you in a comment.", typeof(Startup));
});
```

The display name of a registered type is shown on its notifications, as a badge in the notification center and in the line under the summary in the navbar. The type is listed in the **Type** filter of the notification center, and appears in the notification preferences of the users. The notification center also accepts the type in its search text, so a link such as "View all my mentions" can open it already filtered:

```text
/Admin/notifications?q=type:Mention
/Admin/notifications?q=type:Mention status:Unread
```

A notification can also carry data for its display drivers. `NotificationMessage` is an entity: the properties stored on it with `Put()` are copied to the stored notification.

```csharp
var message = new NotificationMessage { Subject = "Alice mentioned you", Type = "Mention", ReferenceId = commentId };
message.Put(new MentionInfo { ProjectName = project.Name, AuthorName = "Alice" });
```

### Managing sent notifications

The record a notification is about changes: it can be edited, resolved or deleted. Inject `INotificationManager` to find the notifications of a record and keep them in sync. Each method refreshes the unread notifications of the affected users.

```csharp
public sealed class CommentNotificationsHandler
{
    private readonly INotificationManager _notificationManager;

    public CommentNotificationsHandler(INotificationManager notificationManager)
    {
        _notificationManager = notificationManager;
    }

    // The mentions of a deleted comment go away.
    public Task CommentDeletedAsync(string commentId)
        => _notificationManager.DeleteAsync(new NotificationFilter { Type = "Mention", ReferenceId = commentId });

    // The mentions of an edited comment get the new excerpt.
    public async Task CommentEditedAsync(string commentId, string excerpt)
    {
        var notifications = await _notificationManager.ListAsync(new NotificationFilter { Type = "Mention", ReferenceId = commentId });

        foreach (var notification in notifications)
        {
            notification.Summary = excerpt;
            await _notificationManager.UpdateAsync(notification);
        }
    }

    // A resolved comment no longer needs the attention of the mentioned users.
    public Task CommentResolvedAsync(string commentId)
        => _notificationManager.MarkAsReadAsync(new NotificationFilter { Type = "Mention", ReferenceId = commentId });
}
```

`NotificationFilter` combines the criteria that are set: `UserId`, `Type`, `ReferenceId` and `IsRead`. At least one of `UserId`, `Type` and `ReferenceId` must be set. `INotificationManager` also has `FindByIdAsync()`, `DeleteAsync()`, `MarkAsReadAsync()` and `MarkAsUnreadAsync()` for a single notification, and `IsRead()`.

### Customizing how a notification is shown

A notification is displayed by `IDisplayManager<Notification>`, with its display drivers, like other Orchard Core records. The display types are:

| Display type | Where | Zones |
|---|---|---|
| `Header` | An item of the navbar notifications menu. | `Content`, `Meta`, `Actions` |
| `SummaryAdmin` | A row of the notification center, in the `List` layout. | `Checkbox`, `Content`, `Tags`, `Meta`, `Actions`, `ActionsMenu` |
| `SummaryAdminGrid` | A row of the notification center, in the `Grid` layout. | `Select`, `Subject`, `Received`, `Actions`, `ActionsMenu` |
| `DetailAdmin` | The page of a notification in the notification center. | `Content`, `Meta`, `Actions` |

These built-in shapes fill the zones, and can be moved or hidden with placement, or overridden by a template of the same name:

| Shape | Display types | Zone |
|---|---|---|
| `NotificationsSubject_Header` | `Header` | `Content` |
| `NotificationsSubject_SummaryAdmin` | `SummaryAdmin`, `SummaryAdminGrid` | `Content`, `Subject` |
| `NotificationsBody_DetailAdmin` | `DetailAdmin` | `Content` |
| `NotificationsRestricted_SummaryAdmin` | All, when the notification is restricted | `Content`, `Subject` |
| `NotificationsType_Header` | `Header` | `Meta` |
| `NotificationsType_SummaryAdmin` | `SummaryAdmin`, `SummaryAdminGrid`, `DetailAdmin` | `Tags`, `Subject`, `Meta` |
| `NotificationsMeta_Header` | `Header` | `Meta` |
| `NotificationsMeta_SummaryAdmin` | `SummaryAdmin`, `SummaryAdminGrid`, `DetailAdmin` | `Meta`, `Received` |
| `NotificationsActions_SummaryAdmin`, `NotificationsButtonActions_SummaryAdmin` | `SummaryAdmin`, `SummaryAdminGrid` | `Actions`, `ActionsMenu` |
| `NotificationsActions_DetailAdmin` | `DetailAdmin` | `Actions` |

To add fields, badges or buttons to the notifications of a type, add a display driver that only handles that type:

```csharp
public sealed class MentionNotificationDisplayDriver : DisplayDriver<Notification>
{
    public override bool CanHandleModel(Notification notification)
        => notification.Type == "Mention";

    public override IDisplayResult Display(Notification notification, BuildDisplayContext context)
    {
        if (!notification.TryGet<MentionInfo>(out var mention))
        {
            return null;
        }

        return Combine(
            View("MentionNotificationProject", mention)
                .Location("Header", "Meta:5")
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Tags:10")
                .Location(OrchardCoreConstants.DisplayType.SummaryAdminGrid, "Subject:10"),
            View("MentionNotificationReply", notification)
                .Location("Header", "Actions:5")
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Actions:10")
                .Location(OrchardCoreConstants.DisplayType.SummaryAdminGrid, "Actions:10")
        );
    }
}
```

```csharp
services.AddDisplayDriver<Notification, MentionNotificationDisplayDriver>();
```

To replace the whole template of a type instead, add a template for one of these alternates, in your module or theme: `Notification__{Type}` for every display type, or `Notification_{DisplayType}__{Type}`, for example `Notification-Mention.Header.cshtml` or `Notification-Mention.SummaryAdmin.cshtml`.

The notification is also available on the shape as `Model.Notification`, and how it is shown to the current user as `Model.NotificationVisibility`.

### Hiding notifications the user can no longer see

A user can lose access to the record a notification is about after it was sent, for example when they are removed from the team of a project. Implement `INotificationVisibilityHandler` to check it each time a notification is shown: in the navbar, in the unread count, in the notification center and when the notification is opened.

```csharp
public sealed class MentionVisibilityHandler : INotificationVisibilityHandler
{
    private readonly IProjectMembershipService _membershipService;

    public MentionVisibilityHandler(IProjectMembershipService membershipService)
    {
        _membershipService = membershipService;
    }

    public async Task EvaluateAsync(NotificationVisibilityContext context, CancellationToken cancellationToken = default)
    {
        // Every handler is called for every shown notification: return early for the other types.
        if (context.Notification.Type != "Mention" || !context.Notification.TryGet<MentionInfo>(out var mention))
        {
            return;
        }

        if (!await _membershipService.IsMemberAsync(mention.ProjectId, context.User))
        {
            context.Hide();
        }
    }
}
```

```csharp
services.AddScoped<INotificationVisibilityHandler, MentionVisibilityHandler>();
```

A handler decides one of these, and the most restrictive decision of all handlers wins:

| Decision | Effect |
|---|---|
| Nothing | The notification is shown. |
| `context.Restrict()` | The notification is listed and counted, but its subject, summary, body and URL are replaced by a message saying the user no longer has access to it. The user can still mark it as read or delete it. |
| `context.Hide()` | The notification is neither listed nor counted, and cannot be opened. |

The decisions are kept for the rest of the request. When no handler is registered, the notifications are counted and paged by the database as before.

When a handler is registered, the unread count of the navbar is computed from the unread notifications the handlers did not hide, up to `MaxEvaluatedUnreadNotifications` (100 by default): a larger count is shown as that number followed by a plus sign. In the notification center, the hidden notifications are removed from the shown page, and its total count only excludes the hidden notifications of that page.

!!! tip
    A handler is a safety net that runs on every display. When the change is known, for example when a user leaves a team, also delete or update their notifications with `INotificationManager`, so they are not loaded only to be hidden. The navbar is cached per user: `INotificationManager` refreshes it, while a visibility change alone is only reflected when the cache entry expires.

### Notification preferences per type

When types are registered and at least one notification method is enabled, the notification preferences of a user list every type with a checkbox per method, so they can receive mention emails but not other emails. A method the user turns off for every type stays off.

A module chooses which methods are on by default for its type. For example, to only send approvals by email when the user asks for it:

```csharp
services.AddNotificationType("Approval", LocalizationSource.Create("Approvals", typeof(Startup)), type =>
{
    type.DisabledMethods.Add("Email");
});
```

The preferences only store the choices that differ from the defaults of the type, so a change of the defaults applies to the users who did not choose. The notification itself is always stored in the notification center: the preferences choose the methods that also send it.

## Creating a custom notification type

The notification module doesn't know about mentions, approvals or any other kind of notification: each module registers the types it sends. A type gets, without more code, a label on its notifications, a value in the **Type** filter of the notification center, links that open the notification center filtered on it, and a row in the notification preferences of the users. Every other part described below is optional, and several modules can register their own types side by side.

This walkthrough adds a `TaskAssigned` type to a module that manages tasks, to notify a user when a task is assigned to them.

### 1. Register the type

Register the type in the startup of the feature that sends it. The name is stored on the notifications, so keep it in a constant and don't change it once notifications were sent.

```csharp
public static class TaskNotificationConstants
{
    public const string TaskAssigned = "TaskAssigned";
}
```

```csharp
public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddNotificationType(TaskNotificationConstants.TaskAssigned, LocalizationSource.Create("Assigned tasks", typeof(Startup)), type =>
        {
            type.Description = LocalizationSource.Create("When a task is assigned to you.", typeof(Startup));

            // Optional: only send these notifications by SMS when the user asks for it.
            type.DisabledMethods.Add("SMS");
        });
    }
}
```

| Option | Description |
|---|---|
| Display name | The label of the type, in the notification center, the navbar and the preferences. |
| `Description` | Shown under the type in the notification preferences of the users. |
| `DisabledMethods` | The notification methods, for example `Email` or `SMS`, that are off for this type until a user turns them on. Every other method is on. |

### 2. Send notifications of the type

Set the `Type` of the message, the `ReferenceId` of the record it is about, and a `Url` to open. Data the display of the type needs can be stored on the message with `Put()`: it is copied to the stored notification.

```csharp
public sealed class TaskAssignedInfo
{
    public string ProjectName { get; set; }
    public string AssignedBy { get; set; }
    public DateTime? DueUtc { get; set; }
}
```

```csharp
var message = new NotificationMessage
{
    Subject = $"{currentUserName} assigned you a task",
    Summary = task.DisplayText,
    Url = $"~/tasks/{task.ContentItemId}",
    Type = TaskNotificationConstants.TaskAssigned,
    ReferenceId = task.ContentItemId,
};

message.Put(new TaskAssignedInfo
{
    ProjectName = project.DisplayText,
    AssignedBy = currentUserName,
    DueUtc = dueUtc,
});

await _notificationService.SendAsync(assignee, message);
```

A workflow can send notifications of the type too: the **Notify User** and **Notify Content's Owner** tasks have a **Type** and a **URL** field. See [Workflow Activities](#workflow-activities).

### 3. Show the data of the type

Add a display driver that only handles notifications of the type, and place its shapes in the display types where they should show. The display types and their zones are listed in [Customizing how a notification is shown](#customizing-how-a-notification-is-shown).

```csharp
public sealed class TaskAssignedNotificationDisplayDriver : DisplayDriver<Notification>
{
    public override bool CanHandleModel(Notification notification)
        => notification.Type == TaskNotificationConstants.TaskAssigned;

    public override IDisplayResult Display(Notification notification, BuildDisplayContext context)
    {
        if (!notification.TryGet<TaskAssignedInfo>(out var info))
        {
            return null;
        }

        return Combine(
            View("TaskAssignedNotificationProject", info)
                .Location(NotificationConstants.DisplayTypes.Header, "Meta:5")
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Tags:10")
                .Location(OrchardCoreConstants.DisplayType.SummaryAdminGrid, "Subject:10")
                .Location(OrchardCoreConstants.DisplayType.DetailAdmin, "Meta:5"),
            View("TaskAssignedNotificationDue", info)
                .Location(OrchardCoreConstants.DisplayType.DetailAdmin, "Content:5")
        );
    }
}
```

```csharp
services.AddDisplayDriver<Notification, TaskAssignedNotificationDisplayDriver>();
```

With a template for each shape, in the `Views` folder of the module:

```cshtml title="TaskAssignedNotificationProject.cshtml"
@model TaskAssignedInfo

<span class="small text-body-secondary">@Model.ProjectName &middot; @T["by {0}", Model.AssignedBy]</span>
```

```cshtml title="TaskAssignedNotificationDue.cshtml"
@model TaskAssignedInfo

@if (Model.DueUtc != null)
{
    <p>@T["Due on {0}", await DisplayAsync(await New.DateTime(Utc: Model.DueUtc, Format: "D"))]</p>
}
```

In the navbar, the `Meta` zone is one line of small muted text, with a dot between its shapes: render plain text there rather than badges, and keep buttons for the `Actions` zone.

To render the notifications of the type with a template of your own instead, override `Notification-TaskAssigned.Header.cshtml`, `Notification-TaskAssigned.SummaryAdmin.cshtml` or `Notification-TaskAssigned.DetailAdmin.cshtml`, in the module or in a theme.

### 4. Keep the notifications in sync with the record

When the record changes, update the notifications sent about it with `INotificationManager`, using the type and the reference id. For example, from a content handler of the tasks:

```csharp
public sealed class TaskNotificationsContentHandler : ContentHandlerBase
{
    private readonly INotificationManager _notificationManager;

    public TaskNotificationsContentHandler(INotificationManager notificationManager)
    {
        _notificationManager = notificationManager;
    }

    // The assignments of a deleted task go away.
    public override async Task RemovedAsync(RemoveContentContext context)
    {
        if (context.ContentItem.ContentType == "Task")
        {
            await _notificationManager.DeleteAsync(new NotificationFilter
            {
                Type = TaskNotificationConstants.TaskAssigned,
                ReferenceId = context.ContentItem.ContentItemId,
            });
        }
    }
}
```

`MarkAsReadAsync()` fits a task that was completed, and `ListAsync()` with `UpdateAsync()` a task that was renamed. See [Managing sent notifications](#managing-sent-notifications).

### 5. Hide the notifications the user can no longer see (optional)

When a user can lose access to the records of the type, for example when they leave the project of the task, register an `INotificationVisibilityHandler` that hides or restricts the notifications of the type. See [Hiding notifications the user can no longer see](#hiding-notifications-the-user-can-no-longer-see).

### 6. Link to the notifications of the type

Link to the notification center filtered on the type, for example from the page of the module, to give users a list of their notifications of that type:

```cshtml
<a href="@Url.Content("~/Admin/notifications?q=type:TaskAssigned")">@T["My assigned tasks"]</a>
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

Besides the subject, summary and bodies, both tasks have an optional **URL**, the page the notification opens, which accepts Liquid, and an optional **Type**, see [Notification types](#notification-types).

## Videos

<iframe width="560" height="315" src="https://www.youtube-nocookie.com/embed/HMXPzkWE0ww" title="YouTube video player" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture; web-share" allowfullscreen></iframe>

<iframe width="560" height="315" src="https://www.youtube-nocookie.com/embed/Lj2g-bBy-I0" title="YouTube video player" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture; web-share" allowfullscreen></iframe>

<iframe width="560" height="315" src="https://www.youtube-nocookie.com/embed/_3pTgV4oTxU" title="YouTube video player" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture; web-share" allowfullscreen></iframe>

<iframe width="560" height="315" src="https://www.youtube-nocookie.com/embed/lGEsdPzHcog" title="YouTube video player" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture; web-share" allowfullscreen></iframe>

<iframe width="560" height="315" src="https://www.youtube-nocookie.com/embed/b-lHY0NxZNI" title="YouTube video player" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture; web-share" allowfullscreen></iframe>
