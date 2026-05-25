# Azure Communication Services SMS (`OrchardCore.Sms.Azure`)

This feature provides SMS providers for sending SMS through [Azure Communication Services SMS](https://learn.microsoft.com/en-us/azure/communication-services/concepts/sms/concepts).

## **Azure Communication Services** Provider Configuration

Enabling this feature will introduce a new tab labeled **Azure Communication Services** within the SMS settings, allowing you to configure the service. To access these settings from the admin dashboard, navigate to `Settings` → `Communication` → `SMS` and click on the **Azure Communication Services** tab. The following are the available settings.

| Provider       | Description                                                                                                                               |
|----------------|-------------------------------------------------------------------------------------------------------------------------------------------|
| `Azure`        | This provider enables tenant-specific Azure Communication Services for sending SMS. Configure the SMS settings to activate this provider. |
| `DefaultAzure` | This provider sets default Azure Communication Service configurations for all tenants.                                                    |


## Recipe Configuration

Azure SMS settings can be configured using the `Settings` recipe step:

```json
{
  "steps": [
    {
      "name": "settings",
      "AzureSmsSettings": {
        "IsEnabled": true,
        "ConnectionString": "endpoint=https://your-resource.communication.azure.com/;accesskey=...",
        "PhoneNumber": "+1234567890"
      }
    }
  ]
}
```

| Property           | Type    | Description                                                       |
|--------------------|---------|-------------------------------------------------------------------|
| `IsEnabled`        | Boolean | Whether the Azure SMS provider is enabled.                        |
| `ConnectionString` | String  | The Azure Communication Services connection string. **Required.** |
| `PhoneNumber`      | String  | The phone number to send SMS messages from. **Required.**         |

## **Default Azure Communication Services** Provider Configuration

You may configure the **Default Azure Communication Services** using any configuration provider via the following settings:

```json
{
  "OrchardCore_Sms_AzureCommunicationServices": {
    "PhoneNumber": "",
    "ConnectionString": ""
  }
}
```

For more information about configurations, please refer to [Configuration](../Configuration/README.md).

!!! note
    Configuration of the **Default Azure Communication Services** provider cannot be performed through Admin Settings. Instead, use the configuration provider for setup. Note that the provider will only appear if the configuration is present.
