# Users (`OrchardCore.Users`)

The Users module enables authentication UI and user management.

## Features

The module contains the following features apart from the base feature:

- Users Change Email: Allows users to change their email address.
- Users Registration: Allows new users to sign up to the site and ask to confirm their email.
- External User Authentication: Enables a way to authenticate users using an external identity provider.
- Reset Password: Allows users to reset their password.
- User Time Zone: Provides a way to set the time zone per user.
- Custom User Settings: See [its own documentation page](CustomUserSettings/README.md).
- [Users Authentication Ticket Store](./TicketStore.md): Stores users authentication tickets on server in memory cache instead of cookies. If distributed cache feature is enabled it will store authentication tickets on distributed cache.
- Two-Factor Authentication Services: Provides Two-factor core services. This feature cannot be manually enabled or disable as it is enabled by dependency on demand.
- Two-Factor Email Method: Allows users to two-factor authenticate using an email.
- Two-Factor Authenticator App Method: Allows users to two-factor authenticate using any Authenticator App.
- User Localization: Allows ability to configure user culture per user from admin UI.

### User Display Name Shape

The `UserDisplayName` shape has been introduced to render a user's display name in a consistent and cache-friendly way. This is frequently used in admin content lists.

If you override the affected shapes (see the changes in the [relevant pull request](https://github.com/OrchardCMS/OrchardCore/pull/18329/files)), we recommend you make use of `UserDisplayName` too.

To use this shape:

1. Add the `OrchardCore.DisplayManagement` package to your project if you haven't already.
2. In `_ViewImports.cshtml`, add:

```csharp
@addTagHelper *, OrchardCore.DisplayManagement
```

You can then display a user's name like this:

```html
<user-display-name 
    user-name="@(contentItem.Author)" 
    display-type="SummaryAdmin"
    cache-id="user-display-name-author" />
```

This ensures user names are rendered consistently while making use of OrchardCore's caching system for performance.

!!! note
    You may add additional HTML attributes, such as `title`, to show a tooltip for the username badge.

## Two-factor Authentication

Starting with version 1.7, OrchardCore is shipped with everything you need to secure your app with two-factor authentication. To use two-factor authentication, simply enable "Two-Factor Email Method" and/or "Two-Factor Authenticator App Method" features. You can configure the process based on your need by navigating to `Settings` → `Security` → `User Login`. Click on the "Two-Factor Authentication" tab and update the settings as needed.

## User Localization

The feature adds the ability to configure the culture per user from the admin UI.

This feature adds a `RequestCultureProvider` to retrieve the current user culture from its claims. This feature will set a new user claim with a `CultureClaimType` named "culture". It also has a culture option to fall back to other ASP.NET Request Culture Providers by simply setting the user culture to "Use site's culture" which will also be the selected default value.

## Custom Paths

If you want to specify custom paths to access the authentication related urls, you can change them by using this option in the `appsettings.json`:

``` json
  "OrchardCore": {
    "OrchardCore_Users": {
      "LoginPath": "Login",
      "LogoffPath": "Users/LogOff",
      "ChangePasswordUrl": "ChangePassword",
      "ChangePasswordConfirmationUrl": "ChangePasswordConfirmation",
      "ExternalLoginsUrl": "ExternalLogins",
      "ExternalLoginsUrl": "ExternalLogins",
      "TwoFactorAuthenticationPath": "TwoFactor"
    }
  }
```

## Audit Trail Integration

By enabling the "Users Audit Trail" feature within this module, user events such as user creation, updating, or deletion are logged in Admin > Tools > Audit Trail. By default, the event stores the user's name and ID beyond the common Audit Trail data.

It's also possible to include a partial JSON snapshot of the `User` object. To prevent storing particularly sensitive data, this functionality is limited out of the box. You have to go to Admin > Settings > Security > User Audit Trail and select which properties or custom user settings should be stored. The following options are available:

- Store: Stores the value of the property as string.
- ErasingRedactor: Stores an empty sting instead of the value. This is to indicate that the property exists for the `User` object in question.
- HmacRedactor: Uses "HMAC SHA-256" to encode the data before storing it, as a hash or fingerprint.
- PartialAsteriskRedactor: Stores the value as string, but the middle characters are redacted. For example `SampleUser` becomes `S********r`.

You can also create your own redactor simply by adding a singleton https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.compliance.redaction.redactor service.

Note that when a user is deleted, all `User` snapshots are cleared out from existing Audit Trail events to comply with regulations about personal information retention.

## Recipe Configuration

User module settings can be configured using the `Settings` recipe step:

### Login Settings

```json
{
  "steps": [
    {
      "name": "settings",
      "LoginSettings": {
        "UseSiteTheme": false,
        "DisableLocalLogin": false,
        "AllowChangingUsername": false,
        "AllowChangingEmail": false,
        "AllowChangingPhoneNumber": true
      }
    }
  ]
}
```

| Property | Type | Description |
|----------|------|-------------|
| `UseSiteTheme` | Boolean | Whether to use the site theme for the login page. |
| `DisableLocalLogin` | Boolean | Whether to disable local username/password login. |
| `AllowChangingUsername` | Boolean | Whether to allow users to change their username. |
| `AllowChangingEmail` | Boolean | Whether to allow users to change their email address. |
| `AllowChangingPhoneNumber` | Boolean | Whether to allow users to change their phone number. Default: `true`. |

### Registration Settings

```json
{
  "steps": [
    {
      "name": "settings",
      "RegistrationSettings": {
        "UsersMustValidateEmail": true,
        "UsersAreModerated": false,
        "UseSiteTheme": false
      }
    }
  ]
}
```

| Property | Type | Description |
|----------|------|-------------|
| `UsersMustValidateEmail` | Boolean | Whether users must validate their email address before activation. |
| `UsersAreModerated` | Boolean | Whether new user registrations require administrator approval. |
| `UseSiteTheme` | Boolean | Whether to use the site theme for the registration page. |

### Reset Password Settings

```json
{
  "steps": [
    {
      "name": "settings",
      "ResetPasswordSettings": {
        "AllowResetPassword": true,
        "UseSiteTheme": false
      }
    }
  ]
}
```

| Property | Type | Description |
|----------|------|-------------|
| `AllowResetPassword` | Boolean | Whether to allow users to reset their password. |
| `UseSiteTheme` | Boolean | Whether to use the site theme for the reset password page. |

### Change Email Settings

```json
{
  "steps": [
    {
      "name": "settings",
      "ChangeEmailSettings": {
        "AllowChangeEmail": true
      }
    }
  ]
}
```

| Property | Type | Description |
|----------|------|-------------|
| `AllowChangeEmail` | Boolean | Whether to allow users to change their email address. |

### External Authentication Settings

```json
{
  "steps": [
    {
      "name": "settings",
      "ExternalLoginSettings": {
        "UseExternalProviderIfOnlyOneDefined": false,
        "UseScriptToSyncProperties": false,
        "SyncPropertiesScript": ""
      },
      "ExternalRegistrationSettings": {
        "DisableNewRegistrations": false,
        "NoPassword": false,
        "NoUsername": false,
        "NoEmail": false,
        "UseScriptToGenerateUsername": false,
        "GenerateUsernameScript": ""
      }
    }
  ]
}
```

### Two-Factor Authentication Settings

```json
{
  "steps": [
    {
      "name": "settings",
      "TwoFactorLoginSettings": {
        "RequireTwoFactorAuthentication": false,
        "AllowRememberClientTwoFactorAuthentication": true,
        "NumberOfRecoveryCodesToGenerate": 5,
        "UseSiteTheme": false
      },
      "RoleLoginSettings": {
        "RequireTwoFactorAuthenticationForSpecificRoles": false,
        "Roles": [
          "Administrator"
        ]
      },
      "AuthenticatorAppLoginSettings": {
        "UseEmailAsAuthenticatorDisplayName": false,
        "TokenLength": 6
      },
      "EmailAuthenticatorLoginSettings": {
        "Subject": "Your verification code",
        "Body": "Your code is {{ Code }}"
      },
      "SmsAuthenticatorLoginSettings": {
        "Body": "Your verification code is {{ Code }}"
      }
    }
  ]
}
```

## Videos

<iframe width="560" height="315" src="https://www.youtube-nocookie.com/embed/78m04Inmilw" title="YouTube video player" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture" allowfullscreen></iframe>

<iframe width="560" height="315" src="https://www.youtube-nocookie.com/embed/ZgDkWUi2HGs" title="YouTube video player" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture" allowfullscreen></iframe>

<iframe width="560" height="315" src="https://www.youtube-nocookie.com/embed/83-6Kj-IXPw" title="YouTube video player" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture; web-share" allowfullscreen></iframe>

<iframe width="560" height="315" src="https://www.youtube-nocookie.com/embed/BbJG_wdHbak" title="YouTube video player" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture; web-share" allowfullscreen></iframe>

<iframe width="560" height="315" src="https://www.youtube-nocookie.com/embed/FmgZHpFHCcg" title="YouTube video player" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture; web-share" allowfullscreen></iframe>

<iframe width="560" height="315" src="https://www.youtube-nocookie.com/embed/b-lHY0NxZNI" title="YouTube video player" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture; web-share" allowfullscreen></iframe>
