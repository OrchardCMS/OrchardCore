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
- Passkeys: Allows users to log in with a passkey, and to use it as a two-factor authentication method. See [Passkeys](#passkeys).
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

## Email Confirmation

When **Users must verify their email address** is enabled in the User Registration settings (`UsersMustValidateEmail` in recipes), users can't sign in until they confirm their email address. If a user signs in with a valid user name and password before confirming their email, they're taken to a page that explains that their email must be confirmed, with a **Send confirmation email** action that sends a new confirmation link. This lets users recover when the original email was lost or its link expired.

The action is only offered for about 10 minutes after the successful password check, in the same browser. A short-lived cookie remembers which user passed the check; it doesn't sign the user in. A user can request a new email at most once a minute. When the [Rate Limits](../RateLimits/README.md) feature is enabled, the `ResendEmailConfirmation` route is also limited to 2 requests per minute per IP address, using the `email-confirmation` policy.

## Two-factor Authentication

Orchard Core includes the features needed to secure your app with two-factor authentication. To use two-factor authentication, enable "Two-Factor Email Method" and/or "Two-Factor Authenticator App Method". Configure the process from `Settings` → `Security` → `User Login` on the "Two-Factor Authentication" tab.

## Passkeys

A passkey is a WebAuthn credential that lets a user log in with the fingerprint, face or screen lock of their device, or with a security key, instead of a password. The private key never leaves the device or the password manager that keeps it, and the browser only uses it on the site it was created for, so a passkey can't be phished or reused from a data breach.

Enable the **Passkeys** feature to let users:

- Add, rename and remove their passkeys from the **Passkeys** page of their user menu (`/Passkeys`).
- Log in with a passkey from the login page, with the **Log in with a passkey** button or by picking one in the autofill of the username field. The user doesn't type their username: the browser offers the passkeys it holds for the site.
- Use a passkey as their second factor, when the **Two-Factor Authentication Services** are enabled by one of the two-factor methods.

### Passkeys and two-factor authentication

By default, the browser must verify the user with a biometric or a PIN before using the passkey (`UserVerificationRequirement` is `required`). Logging in with a passkey then proves both something the user has, their device, and something they are or know, so it counts as multi-factor authentication:

- Users who have two-factor authentication enabled aren't asked for a second factor after logging in with a passkey.
- Users whose role requires two-factor authentication aren't asked to set up a two-factor method when they log in with a passkey. They are still asked to when they log in with their password.

When the two-factor services are enabled, passkeys are also listed as a two-factor method on the **Two-Factor Authentication** page:

- Adding a passkey doesn't enable two-factor authentication. A user who has a passkey can enable it, and then confirm it's them with one of their passkeys after logging in with their password.
- When two-factor authentication is required for the user, adding a passkey enables it, as when configuring another method.
- Removing the last passkey removes the passkey method. It is refused when it is the only method of a user who must use two-factor authentication.

When user verification isn't required, a passkey is a single factor: a user who has two-factor authentication enabled is asked for their second factor after logging in with a passkey, as after logging in with their password.

### Administration

Administrators who can edit a user can remove all their passkeys, for instance when a device is lost, with the **Remove passkeys** action of the user in the users list. When passkeys were the only two-factor method of the user, their two-factor authentication is disabled.

### Configuration

Passkeys are bound to a domain, the relying party ID, which defaults to the host of the request. The other options of the passkeys can be set in the `OrchardCore:Users:Passkeys` section of the `appsettings.json` file, here with their default values:

```json
{
  "OrchardCore": {
    "Users": {
      "Passkeys": {
        "ServerDomain": null,
        "AuthenticatorTimeout": "00:05:00",
        "ChallengeSize": 32,
        "UserVerificationRequirement": "required",
        "ResidentKeyRequirement": "preferred",
        "AttestationConveyancePreference": null,
        "AuthenticatorAttachment": null
      }
    }
  }
}
```

| Option | Description |
| --- | --- |
| `ServerDomain` | The domain the passkeys are bound to. Set it when the site is reached through several hosts, such as `www.example.com` and `example.com`, to the common registrable domain. Changing it invalidates the passkeys that were already registered. |
| `AuthenticatorTimeout` | How long the browser waits for the user to use their passkey. |
| `ChallengeSize` | The size, in bytes, of the random challenge the passkey signs. |
| `UserVerificationRequirement` | `required`, `preferred` or `discouraged`. A passkey only counts as multi-factor when it is `required`. |
| `ResidentKeyRequirement` | `required`, `preferred` or `discouraged`. Whether new passkeys must be discoverable, so that users can log in without typing their username. |
| `AttestationConveyancePreference` | `none`, `indirect`, `direct` or `enterprise`. Whether the authenticator should provide an attestation of its make and model. |
| `AuthenticatorAttachment` | `platform` to only allow the authenticator of the device, `cross-platform` to only allow roaming authenticators such as security keys. Any authenticator is allowed when it isn't set. |

The server only accepts a passkey used from a page of the same origin (scheme, host and port) as the request. Behind a reverse proxy that terminates TLS, enable the forwarded headers so that the scheme and the host of the request are the public ones.

The browser only uses a passkey on the domain it was created for, and its subdomains. Tenants that share a host, with a different URL prefix each, share the same relying party ID: the browser may offer the passkey of one tenant on the login page of another, which refuses it.

Passkeys are a local login: they aren't offered when **Disable local login** is enabled in the login settings.

### Rate limiting

When the [Rate Limits](../RateLimits/README.md) feature is enabled, the `Passkeys/Login` route is limited to 10 requests per IP address, using the `password-authentication` policy shared with the login page.

### Storage

The passkeys are stored in the `Passkeys` property of the user document, which holds their public key and never a secret. The `UserByPasskeyIndex` index holds a hash of the credential identifier of each passkey, which `UserManager.FindByPasskeyIdAsync()` uses to make sure that a passkey is only registered once. The `UserStore` implements `IUserPasskeyStore<IUser>`, so the passkey methods of the ASP.NET Core Identity `UserManager<IUser>` are available whether the feature is enabled or not. The **All Users** deployment step doesn't export the passkeys, which are bound to the domain of the site.

## User Localization

The feature adds the ability to configure the culture per user from the admin UI.

This feature adds a `RequestCultureProvider` to retrieve the current user culture from its claims. This feature will set a new user claim with a `CultureClaimType` named "culture". It also has a culture option to fall back to other ASP.NET Request Culture Providers by simply setting the user culture to "Use site's culture" which will also be the selected default value.

## Time zone select list customization

The **User Time Zone** editor uses the shared `ITimeZoneSelectListProvider` service for its `<select>` items. Replace `DefaultTimeZoneSelectListProvider` if you need different labels, ordering, or filtering for time zone options across Orchard Core.

## Custom Paths

If you want to specify custom paths to access the authentication related urls, you can change them by using this option in the `appsettings.json`:

``` json
{
  "OrchardCore": {
    "Users": {
      "LoginPath": "Login",
      "LogoffPath": "Users/LogOff",
      "ChangePasswordUrl": "ChangePassword",
      "ChangePasswordConfirmationUrl": "ChangePasswordConfirmation",
      "ExternalLoginsUrl": "ExternalLogins",
      "TwoFactorAuthenticationPath": "TwoFactor"
    }
  }
}
```

## Audit Trail Integration

By enabling the "Users Audit Trail" feature within this module, user events such as user creation, updating, or deletion are logged in Admin > Tools > Audit Trail. By default, the event stores the user's name and ID beyond the common Audit Trail data.

It's also possible to include a partial JSON snapshot of the `User` object. To prevent storing particularly sensitive data, this functionality is limited out of the box. You have to go to Admin > Settings > Security > User Audit Trail and select which properties or custom user settings should be stored. The following options are available:

- Store: Stores the value of the property as a string.
- ErasingRedactor: Stores an empty string instead of the value. This is to indicate that the property exists for the `User` object in question.
- PartialAsteriskRedactor: Stores the value as a string, but the middle characters are redacted. For example, `SampleUser` becomes `S********r`.
- HmacRedactor: Uses "HMAC SHA-256" to encode the data before storing it, as a hash or fingerprint. This redactor is only available when both `HmacRedactorOptions.Key` and `HmacRedactorOptions.KeyId` are configured. For security reasons, these values should be unique per tenant. For the options to be loaded, you need to bind the settings manually.

You can also create your own redactor simply by adding a singleton [`Redactor`](https://learn.microsoft.com/dotnet/api/microsoft.extensions.compliance.redaction.redactor) service.

Note that when a user is deleted, all `User` snapshots are cleared out from existing Audit Trail events to comply with regulations about personal information retention.

## Documenting Filters in the Admin UI

The users admin list (**Security** → **Users**) has a **Filters** dropdown next to the search box. Its **Filter syntax** entry opens the **Available Filters** dialog, which lists every filter a user can type into the search box (`name:`, `email:`, `status:`, `role:`, `sort:`, …) as a compact grid of cards. Each card shows the filter title, its capability icons, the syntax token, and a short description.

The list works exactly like the [content items admin list filters](../Contents/README.md#documenting-filters-in-the-admin-ui); only the model type differs. There are two independent extension points: the filter *logic* and the filter *card* that documents it in the dialog.

### Registering the filter logic

Implement `IUsersAdminListFilterProvider` and add your terms to the `QueryEngineBuilder<User>`:

```csharp
public sealed class SsnUsersAdminListFilterProvider : IUsersAdminListFilterProvider
{
    public void Build(QueryEngineBuilder<User> builder)
    {
        builder
            .WithNamedTerm("ssn", builder => builder
                .OneCondition((val, query) =>
                    query.With<UserProfileIndex>(i => i.Ssn != null && i.Ssn.Contains(val))));
    }
}
```

Register it in your module's `Startup`:

```csharp
services.AddScoped<IUsersAdminListFilterProvider, SsnUsersAdminListFilterProvider>();
```

### Registering the filter card

Implement a `DisplayDriver<UserIndexOptions>` and return a `View` result placed in the `Content` zone of the `Thumbnail` display type. The position after `Content:` controls the order the card appears in.

```csharp
public sealed class SsnUsersAdminListDisplayDriver : DisplayDriver<UserIndexOptions>
{
    public override IDisplayResult Display(UserIndexOptions model, BuildDisplayContext context)
    {
        return View("UsersAdminFilters_Thumbnail__Ssn", model)
            .Location("Thumbnail", "Content:35");
    }
}
```

```csharp
services.AddDisplayDriver<UserIndexOptions, SsnUsersAdminListDisplayDriver>();
```

### The card template

The shape name `UsersAdminFilters_Thumbnail__Ssn` resolves to a Razor view named `UsersAdminFilters-Ssn.Thumbnail.cshtml` placed under `Views/Items/`. Each card is automatically wrapped in a Bootstrap card and laid out in the responsive grid, so the template only supplies the card's inner content: a title with its capability icons on the first line, the filter token below it, and a short description.

```html
@model ShapeViewModel<UserIndexOptions>
@{
    var term = Model.Value.FilterResult.FirstOrDefault(x => x.TermName == "ssn");
}

<div class="d-flex justify-content-between align-items-center gap-2">
    <h6 class="card-title fw-semibold mb-0">@T["SSN"]</h6>
    <span class="text-primary text-nowrap">
        <i class="fa-solid fa-sm fa-minus" title="@T["Accepts a single value"]" aria-hidden="true"></i>
    </span>
</div>
<div class="mt-1"><code class="small text-nowrap">@(term?.ToString() ?? "ssn:...")</code></div>
<p class="card-text small text-body-secondary mt-1 mb-0">@T["Filters on a user's social security number."]</p>
```

Use the same capability icons the built-in filters use so the shared legend at the bottom of the dialog stays accurate: `fa-check` (**Default** — may be entered with or without the term name), `fa-minus` (**Single** — accepts a single value), and `fa-bars` (**Multiple** — supports the `AND`, `OR`, and `NOT` operators and groups).

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
        "AllowRememberMe": true,
        "UsePersistentAuthenticationCookie": false,
        "AllowChangingUsername": false,
        "AllowChangingEmail": false,
        "AllowChangingPhoneNumber": true
      }
    }
  ]
}
```

| Property                   | Type    | Description                                                           |
|----------------------------|---------|-----------------------------------------------------------------------|
| `UseSiteTheme`             | Boolean | Whether to use the site theme for the login page.                     |
| `DisableLocalLogin`        | Boolean | Whether to disable local username/password login.                     |
| `AllowRememberMe`          | Boolean | Whether to show the **Remember me** option on the login form. Default: `true`. When disabled, the `UsePersistentAuthenticationCookie` setting controls all local and external sign-ins. |
| `UsePersistentAuthenticationCookie` | Boolean | Whether authentication cookies persist across browser sessions. When `AllowRememberMe` is enabled, this is the default value of the **Remember me** option. Default: `false`. |
| `AllowChangingUsername`    | Boolean | Whether to allow users to change their username.                      |
| `AllowChangingEmail`       | Boolean | Whether to allow users to change their email address.                 |
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

| Property                 | Type    | Description                                                        |
|--------------------------|---------|--------------------------------------------------------------------|
| `UsersMustValidateEmail` | Boolean | Whether users must validate their email address before activation. |
| `UsersAreModerated`      | Boolean | Whether new user registrations require administrator approval.     |
| `UseSiteTheme`           | Boolean | Whether to use the site theme for the registration page.           |

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

| Property             | Type    | Description                                                |
|----------------------|---------|------------------------------------------------------------|
| `AllowResetPassword` | Boolean | Whether to allow users to reset their password.            |
| `UseSiteTheme`       | Boolean | Whether to use the site theme for the reset password page. |

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

| Property           | Type    | Description                                           |
|--------------------|---------|-------------------------------------------------------|
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

## Commands

The Users module registers the `createUser` command, which you can run from a recipe's [`command` step](../Recipes/README.md#command).

```text
createUser /UserName:<username> /Password:<password> /Email:<email> /PhoneNumber:<phonenumber> /Roles:{rolename,rolename,...}
```

| Switch        | Description                                                                        |
|---------------|-----------------------------------------------------------------------------------|
| `UserName`    | The username of the new user.                                                     |
| `Password`    | The password of the new user. It has to satisfy the configured password rules.    |
| `Email`       | The email address of the new user, which is marked as confirmed on creation.      |
| `PhoneNumber` | The phone number of the new user. Optional.                                       |
| `Roles`       | A comma-separated list of the roles to assign to the user. Optional.              |

For example, to create an administrator during setup from a recipe:

```json
{
  "name": "command",
  "Commands": [
    "createUser /UserName:admin /Password:Password1! /Email:admin@example.com /Roles:Administrator"
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
