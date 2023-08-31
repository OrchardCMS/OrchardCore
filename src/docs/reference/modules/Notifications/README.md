# Notifications (`OrchardCore.Notifications`)

The `Notifications` module provides the infrastructure necessary to send notification to the app users.

## Notification Methods

There are many methods to send notifications to a user (e.x., Email, Web Push, Mobile Push, SMS, etc.). Any notification should appear in the notification center.

!!! info
When multiple notification methods are enabled, the user can opt-in/out any method they wishes to receive by editing their profile.

### Email Notifications

The `Email Notifications` feature offers a means to inform users by dispatching notifications through email.

!!! note
When using `Email Notifications` feature, you must also configure the [SMTP Service](../Email/README.md).

### Email Notifications

The `SMS Notifications` feature offers a means to inform users by dispatching notifications through phone via SMS provider.

!!! note
When using `Email SMS` feature, you must also configure the [SMS Provider](../Sms/README.md).

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
