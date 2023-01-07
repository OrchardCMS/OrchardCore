# Notifications (`OrchardCore.Notifications`)

The `Notifications` module provides the infrastructure necessary to send notification to the app users.

## Notification Methods

There are many methods to send notifications to a user (e.x., Web, Email, Push, SMS, etc.). In addition to web notification, OrchardCore is shipped with Email based notifications. To allow users to receive Email notification, enable the `Email Notifications` feature. 

!!! note
When using `Email Notifications` feature, you must also configure the [SMTP Settings](../Email/README.md). When multiple notification methods are enabled, the user can opt-in or opt-out any method they wishes to receive by editing their profile.


## Adding Custom Notification Provider
To add a new notification method like `Push` or `SMS`, you can simply implement the `INotificationMethodProvider` interface and then register it as we do in `OrchardCore.Notifications.Email` feature

```C#
[Feature("OrchardCore.Notifications.Email")]
public class EmailNotificationStartup : StartupBase
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
