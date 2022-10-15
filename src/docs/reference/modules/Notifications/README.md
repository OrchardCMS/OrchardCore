# Notifications (`OrchardCore.Notifications`)

The `Notifications` module provides the infrastructure necessary to send notification to the app users.

## Notification Methods

There are many methods to send notifications to a user (e.x., Web, Email, Push, SMS, etc.) OrchardCore is shipped with the following implementations 
- `Web Notification` provides you a way to send web notifications to your users.
- `Email Notification` provides you a way to send notification via Email.

The feature `OrchardCore.Notifications` is an ondemand feature which registers all the services necessary to allow the app to send notification. To send web notifications, you'll need to enable `OrchardCore.Notification.Web` feature. Similarly, you can enable `OrchardCore.Notification.Email` to send notification via email service. To use `Email Notifications`, you must also configure the [SMTP Settings](../Email/README.md). When multiple notification methods are enabled, the user can opt-in or opt-out any method he/she wish to receive by editing their profile.

!!! warning
    Don't create a Content Type with the name `WebNotification`, as it's used by the `Web Notifications` feature.

## Adding Custom Notification Provider
To add a new notification method like Push or SMS, you can simple implement the `INotificationMethodProvider` interface and then register it as we do in `OrchardCore.Notifications.Email` feature

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

!!! info
    Be sure to list `OrchardCore.Notifications` as dependency on your custom feature so that it gets enabled ondemand.

## How to send a notification

You can send notification to a user via code by injecting `INotificationManager` then calling the `SendAsync` method. Alternatively, you can use workflows to notify a user about an event that took place.

## Workflow Activities
When `OrchardCore.Workflows` feature is enabled, you'll see new activites that would allow you to notify users using workflows. Here are some of the available workflow activities
 - Notify Content's Owner Task
 - Notify User Task
