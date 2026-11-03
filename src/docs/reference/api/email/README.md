# Email management API

The **Email** feature (`OrchardCore.Email`) lets a remote client test the delivery of emails. It advertises the `email` remote-management capability and contributes the `pomi email` command and an MCP tool. It also contributes the `email` section of the [site settings API](../settings/README.md), which selects the default provider.

See the [Email module](../../modules/Email/README.md). Provider credentials belong to the owning provider, such as the [`smtp` section](../../modules/Email.Smtp/README.md#remote-smtp-administration).

## Authentication and permissions

Every operation requires an `Api` scheme bearer token, **Access remote management API** (`AccessRemoteManagement`) and **Manage Email Settings** (`ManageEmailSettings`). An absent or invalid token returns `401`, a missing permission returns `403`.

## Base route

`/api/email`

## Endpoints

| Method | Endpoint | Operation ID | Pomi command | Permission |
| --- | --- | --- | --- | --- |
| `POST` | `/api/email:test` | `ApiTestEmailDelivery` | `email test` | `ManageEmailSettings` |

## Test the delivery

`POST /api/email:test`

Sends one plain-text message through the same email service as the admin test form.

```json
{
  "provider": "SMTP",
  "to": "recipient@example.com",
  "subject": "Delivery test",
  "body": "Test message from Orchard Core."
}
```

| Property | Type | Required | Description |
| --- | --- | --- | --- |
| `provider` | string | No | An enabled provider name, at most 256 characters. Omit it to use the default provider. |
| `to` | string | Yes | One valid recipient address, at most 320 characters. |
| `subject` | string | Yes | 1 to 256 characters, without line breaks. |
| `body` | string | Yes | Plain text of 1 to 16384 characters. |

Read `availableProviders` in the `email` settings section to discover the names of the enabled providers.

```bash
pomi email test --body-file email-test.json
```

| Status | Meaning |
| --- | --- |
| `204` | The provider accepted the message. This doesn't prove that it reached the inbox. |
| `400` | A missing body, a validation problem keyed by property, or a provider that is invalid or disabled. |
| `502` | The provider couldn't send the message. Its diagnostics are never returned, as they can contain credentials or connection details. |

Sending isn't idempotent: repeating a request sends another message.

## Errors

Failures use Problem Details, except invalid input, which uses a validation problem keyed by property.

## Sources

- [Endpoint mapping and handler](https://github.com/OrchardCMS/OrchardCore/blob/main/src/OrchardCore.Modules/OrchardCore.Email/Endpoints/EmailManagementEndpoints.cs)
- [Email module](../../modules/Email/README.md)
