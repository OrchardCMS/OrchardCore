# SMS (`OrchardCore.Sms`)

This module provides SMS settings configuration.

## SMS Settings

Enabling the `SMS` feature will add a new settings page under `Configurations` >> `Settings` >> `SMS`. You can utilize these settings to set up the default SMS provider configuration. The following are the providers that are readily accessible.

| Provider | Description |
| --- | --- |
| `Log` | This particular provider is exclusively meant for debugging purposes and should never be used in a production environment. It permits the message to be written to the logs. |
| `Twilio` | Opting for this provider enables the utilization of Twilio service for sending SMS messages. Edit the SMS settings to enable this provider. |

!!! note
    After enabling the SMS feature, you must configure the default provider in order to send SMS messages.

## Configuring the Twilio Providers

To enable the [Twilio](https://www.twilio.com) provider, navigate to `Configurations` >> `Settings` >> `SMS`. Click on the `Twilio` tab, click the Enable checkbox and provider your Twilio account info. Then in the `Providers` tab, select Twilio as your default provider.

## Additional Available Providers

- [Azure Communication](../Sms.Azure/README.md) service provider.

## Adding Custom Providers

The `OrchardCore.Sms` module provides you with the capability to integrate additional providers for dispatching SMS messages. To achieve this, you can easily create an implementation of the `ISmsProvider` interface and then proceed to register it using one of the following approaches:

If your provider does not require any settings like the `LogProvider`, you may register it like this.

```csharp
services.AddSmsProvider<YourCustomImplemenation>("A technical name for your implementation")
```

However, if you have a complex provider like the Twilio provider, you may implement `IConfigureOptions<SmsProviderOptions>` and register it using the following extensions

```csharp
services.AddSmsProviderOptionsConfiguration<YourCustomImplemenation>()
```

Here is and example of how we register Twilio complex provider:

```csharp
public class TwilioProviderOptionsConfigurations : IConfigureOptions<SmsProviderOptions>
{
    private readonly ISiteService _siteService;

    public TwilioProviderOptionsConfigurations(ISiteService siteService)
    {
        _siteService = siteService;
    }

    public void Configure(SmsProviderOptions options)
    {
        var typeOptions = new SmsProviderTypeOptions(typeof(TwilioSmsProvider));

        var site = _siteService.GetSiteSettingsAsync().GetAwaiter().GetResult();
        var settings = site.As<TwilioSettings>();

        typeOptions.IsEnabled = settings.IsEnabled;

        options.TryAddProvider(TwilioSmsProvider.TechnicalName, typeOptions);
    }
}
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
            To = "+17023451234",
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
