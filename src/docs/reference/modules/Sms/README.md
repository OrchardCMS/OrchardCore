# SMS (`OrchardCore.Sms`)

This module provides the infrastructure necessary to send messages using an `SMS` service.

## SMS Settings

Enabling the `SMS` feature will add a new settings page under `Configurations` >> `Settings` >> `SMS`. You can utilize these settings to set up the default SMS provider configuration. The following are the providers that are readily accessible.

| Provider | Description |
| --- | --- |
| `Log` | This particular provider is exclusively meant for debugging purposes and should never be used in a production environment. It permits the message to be written to the logs. |
| `Twilio` | Opting for this provider enables the utilization of Twilio service for sending SMS messages. By choosing this provider, you will need to input your Twilio account settings. |

!!! note
    After enabling the SMS feature, you must configure the default provider in order to send SMS messages.

## Other Providers

The `OrchardCore.Sms` module provides you with the capability to integrate additional providers for dispatching SMS messages. To achieve this, you can easily create an implementation of the `ISmsProvider` interface and then proceed to register it using the following approach.

```csharp
    services.AddSmsProvider<YourCustomImplemenation>("A technical name for your implementation")
```

## Workflows

When both the `SMS` and `Workflows` features are enabled at the same time, a new "Send SMS" workflow task will become available to allow you to send SMS message using workflow.

## SMS Notification (`OrchardCore.Notifications.Sms`)

This feature provides you a way to send user notifications using SMS based on user preferences. [Click here](../Notifications/README.md) to read more about notifications.

## Credits

### Google's libphonenumber library.

<https://github.com/twcclegg/libphonenumber-csharp>
