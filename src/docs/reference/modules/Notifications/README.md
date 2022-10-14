# Notifications (`OrchardCore.Notifications`)

This module provides the infrastructure necessary to send notification to the app users.

## Notification Methods

The feature `OrchardCore.Notifications` is an ondemand feature which register the services necessary to allow the app to send notification. To send notications you'll need to enabled a notification feature that provides a notification service. For example, enabling `OrchardCore.Notification.Email` will enable your app to sent the users notifications via Email. Of course, you must configure the [SMTP Settings](../Email/README.md).

## Adding Custom Notification Provider
Its quite easy to add a new notification provider to be able to add different notification methods like Push or SMS notification. To do that, you can simple implement `INotificationMethodProvider` interface and then register it as we do in `OrchardCore.Notifications.Email` feature

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

!! Be sure to list `OrchardCore.Notifications` as dependency on your custom feature so that it gets enabled ondemand.

## Workflow Activities
When `OrchardCore.Workflows` feature is enabled, you'll see new activites that would allow you to notify users using workflows. Here are some of the available workflow activities
 - Notify Content's Owner Task
 - Notify User Task
