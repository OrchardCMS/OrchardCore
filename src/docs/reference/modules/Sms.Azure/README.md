# Azure SMS (`OrchardCore.Sms.Azure`)

This module provides an SMS provider for sending SMS through [Azure Communication Services SMS](https://learn.microsoft.com/en-us/azure/communication-services/concepts/sms/concepts).

## Azure Communication Service Settings

Enabling this module will introduce a new tab labeled 'Azure' within the SMS settings, allowing you to configure the service. To access these settings, navigate to `Configuration` → `Settings` → `Sms` and click on the 'Azure' tab. The following are the available settings

| Provider | Description |
| --- | --- |
| `Azure` | This provider enables tenant-specific Azure Communication Services for sending SMS. Configure the SMS settings to activate this provider. |
| `DefaultAzure` | This provider sets default Azure Communication Service configurations for all tenants.|


## Default Azure SMS Communication Service Configuration

You may configure the Default Azure SMS Service provider by the configuration provider using the following settings:

```json
"OrchardCore_Sms_AzureCommunicationServices": {
    "PhoneNumber": "",
    "ConnectionString": ""
}
```

For more information about configurations, please refer to [Configuration](../../core/Configuration/README.md).

!!! note
    Configuration of the Default Azure SMS provider is not possible through Admin Settings. Utilize the configuration provider for the necessary setup. The provider will appear only if the configuration exists.
