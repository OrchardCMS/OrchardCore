# Notifications (`OrchardCore.Notifications`)

The `Notifications` module offers a comprehensive infrastructure for managing notifications effectively. It includes a centralized notification center and streamlined mechanisms for seamlessly dispatching notifications to users within the application.
## Configurations 

You can customize the default notification options through the configuration provider using the following settings:

```json
"OrchardCore_Notifications": {
    "TotalUnreadNotifications": 10,
    "DisableNotificationHtmlBodySanitizer": false,
    "TotalUnreadNotifications": 3600
}
```

Available Options and Their Definitions:

| **Property**                             | **Description**                                                                                                                                                     |
|------------------------------------------|---------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `TotalUnreadNotifications`               | Specifies the maximum number of unread notifications displayed in the navigation bar. Default is 10.                                                                 |
| `DisableNotificationHtmlBodySanitizer`   | Allows you to disable the default sanitization of the `HtmlBody` in notifications generated from workflows.                                                          |
| `AbsoluteCacheExpirationSeconds`         | Specifies the absolute maximum duration, in seconds, for which the top unread user notifications are cached when caching is enabled. A value of 0 does not disable caching but indicates that there is no fixed expiration time for the cache. You can set this value to define a maximum lifespan for the cached data before it is invalidated. |
| `SlidingCacheExpirationSeconds`          | Gets or sets how long a cache entry can be inactive (e.g. not accessed) before it will be removed. This will not extend the entry lifetime beyond the absolute expiration (if set). To disable sliding expiration, you can set this value to 0. |

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
