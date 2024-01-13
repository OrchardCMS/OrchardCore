# SMS (`OrchardCore.Sms`)

This module provides SMS settings configuration.

## SMS Settings

Enabling the `SMS` feature will add a new settings page under `Configurations` >> `Settings` >> `SMS`. You can utilize these settings to set up the default SMS provider configuration. When running in a development mode, we enable `Log` provider to allow you to log the SMS message to the log file for testing. However, for the `SMS` to be production ready, you must enable an SMS provider feature like `OrchardCore.Sms.Twilio`.

## Adding Custom Providers

The `OrchardCore.Sms` module provides you with the capability to integrate additional providers for dispatching SMS messages. To achieve this, you can easily create an implementation of the `ISmsProvider` interface and then proceed to register it using the following approach.

```csharp
    services.AddSmsProvider<YourCustomImplemenation>("A technical name for your implementation")
```

## Sending SMS Message

An SMS message can be send by injecting `ISmsService` and invoke the `SendAsync` method. For instance

```c#
public class TestController
{
    private readonly ISmsService _smsService;

    public TestController(ISmsService smsService)
    {
        _smsService = smsService;
    }

    public async Task SendSmsMessage()
    {
        var message = new SmsMessage
        {
            To = "17023451234",
            Message = "It's easy to send an SMS message using Orchard!",
        };

        var result = await _smsService.SendAsync(message);

        if (result.Succeeded) 
        {
            // message was sent!

            return Ok(result);
        }

        return BadRequest(result);
    }
}
```

## Workflows

When both the `SMS` and `Workflows` features are enabled at the same time, a new "Send SMS" workflow task will become available to allow you to send SMS message using workflow.

## SMS Notification (`OrchardCore.Notifications.Sms`)

This feature provides you a way to send user notifications using SMS based on user preferences. [Click here](../Notifications/README.md) to read more about notifications.

## Credits

### Google's libphonenumber library

<https://github.com/twcclegg/libphonenumber-csharp>
