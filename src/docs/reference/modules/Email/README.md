# Email (`OrchardCore.Email`)

This module provides the infrastructure necessary to send emails using multiple email providers.


## Settings

Enabling the `Email` feature will add a new settings page under `Settings` → `Communication` → `Email`. You can utilize these settings to set up the default Email provider. The following are the providers that are readily accessible in OrchardCore:


| Provider                                         | Description                                                                                                                                                                                                                                                                                                                                                                                                                                                      |
|--------------------------------------------------|------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| **Simple Mail Transfer Protocol (SMTP)**         | The SMTP provider utilizes the SMTP protocol for sending email messages. This provider is a component of the `OrchardCore.Email.Smtp` feature. To activate it, edit the `SMTP` tab in the email settings. For detailed information on configuring the SMTP provider, refer to the [SMTP provider documentation](../Email.Smtp/README.md).                                                                                                                        |
| **Simple Mail Transfer Protocol (Default SMTP)** | The Default SMTP provider utilization of the SMTP protocol for sending email messages. This provider is a component of the `OrchardCore.Email.Smtp` feature. To activate it, utilize the configuration provider, such as the `appsettings.json` file or environment variables. For detailed information on configuring the SMTP provider, refer to the [SMTP provider documentation](../Email.Smtp/README.md).                                                   |
| **Azure Communication Service**                  | The Azure Communication Service provider utilizes the Azure Communication Services for sending email messages. This provider is a component of the `OrchardCore.Email.Azure` feature. To activate it, edit the `Azure` tab in the email settings. For detailed information on configuring the Azure Communication provider, refer to the [Azure provider documentation](../Email.Azure/README.md).                                                               |
| **Default Azure Communication Service**          | The Default Azure Communication Service provider utilizes the Azure Communication Services for sending email messages. This provider is a component of the `OrchardCore.Email.Azure` feature. To activate it, utilize the configuration provider, such as the `appsettings.json` file or environment variables. For detailed information on configuring the Azure Communication provider, refer to the [Azure provider documentation](../Email.Azure/README.md). |


## Adding Custom Providers

The `OrchardCore.Email` module provides you with the capability to integrate additional providers for dispatching email messages. To achieve this, create an implementation of the `IEmailProvider` interface and describe it with an `[EmailProvider]` attribute, which gives its technical name and its display name:

```csharp
[EmailProvider("Custom", "My custom email provider")]
public sealed class CustomEmailProvider : IEmailProvider
{
    public Task<Result> SendAsync(MailMessage message, CancellationToken cancellationToken = default)
    {
        // ...
    }
}
```

The attribute is required: a provider without it doesn't compile (`ANNO001`). The display name is localized in the context of the provider type, like `IStringLocalizer<CustomEmailProvider>` would. The provider's module must reference the [Annotype](https://github.com/sebastienros/annotype) package directly, as analyzers don't flow to referencing projects, and the attribute has to be declared on the concrete provider class itself, even when it derives from another provider.

Orchard Core reads these values from the provider type, without creating the provider, through the registry that Annotype generates in the provider's project. `TypeMetadata` registers this registry the first time it reads metadata from one of the project's types, or when the module is loaded.

Then register the provider using one of the following approaches:

If your provider does not require any settings, you may register it using the `AddEmailProvider<>` extension. For instance:

```csharp
services.AddEmailProvider<CustomEmailProvider>();
```

However, if you have a complex provider like the included SMTP one, you may implement `IConfigureOptions<EmailProviderOptions>` and register it using the `AddEmailProviderOptionsConfiguration<>` extension. For instance:

```csharp
services.AddEmailProviderOptionsConfiguration<YourCustomImplemenation>()
```

Here is and example of how we register the SMTP complex provider:

```csharp
public class SmtpProviderOptionsConfigurations : IConfigureOptions<EmailProviderOptions>
{
    private readonly SmtpOptions _smtpOptions;

    public SmtpProviderOptionsConfigurations(IOptions<SmtpOptions> smtpOptions)
    {
        _smtpOptions = smtpOptions.Value;
    }

    public void Configure(EmailProviderOptions options)
    {
        var typeOptions = new EmailProviderTypeOptions(typeof(SmtpEmailProvider))
        {
            IsEnabled = _smtpOptions.IsEnabled
        };

        options.TryAddProvider(typeOptions);
    }
}
```

If your provider options are built from site settings or other tenant data that can change at runtime, consume them through `IOptionsMonitor<TOptions>` in the provider and any `IConfigureOptions<EmailProviderOptions>` implementation that depends on them. Register `AddSignalOptionsChangeTokenSource<TOptions>()` for any options type that should participate in post-commit refresh, then request invalidation for the affected options types with `IOptionsUpdateNotifier` when the settings editor saves changes.

For example, Orchard Core now refreshes the Azure email provider in-place by invalidating `AzureEmailOptions`, `EmailProviderOptions`, and `EmailOptions`, and the built-in email modules register signal-backed change token sources for those options types so the standard `IOptionsMonitor<TOptions>` can rebuild them from committed tenant state.

## Sending Email Messages

An Email message can be sent by injecting `IEmailService` and invoking the `SendAsync` method. For instance:

```csharp
public class TestController
{
    private readonly IEmailService _emailService;

    public TestController(IEmailService emailService)
    {
        _emailService = emailService;
    }

    public async Task SendSmsMessage()
    {
        var message = new MailMessage
        {
            To = "to-email@test.com",
            Subject = "Subject for the email",
            Body = "Body of the email",
        };

        var result = await _emailService.SendAsync(message);

        if (result.Succeeded) 
        {
            // message was sent!

            return Ok(result);
        }

        return BadRequest(result);
    }
}
```

## Testing Provider

After configuring a provider, you may test it by visiting `Tools` → `Testing` → `Email Test`.

## Events

You can easily monitor various events triggered during the message-sending process by either implementing the `IEmailServiceEvents` or inheriting from `EmailServiceEventsBase` base class, then registering your service.

## Video

<iframe width="560" height="315" src="https://www.youtube-nocookie.com/embed/FmgZHpFHCcg" title="YouTube video player" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture" allowfullscreen></iframe>

## Recipe Configuration

Email settings can be configured using the `Settings` recipe step:

```json
{
  "steps": [
    {
      "name": "settings",
      "EmailSettings": {
        "DefaultProviderName": "SMTP"
      }
    }
  ]
}
```

| Property              | Type   | Description                             |
|-----------------------|--------|-----------------------------------------|
| `DefaultProviderName` | String | The name of the default email provider. |
