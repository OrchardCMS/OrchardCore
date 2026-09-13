# Users (`OrchardCore.Users`)

The Users module enables authentication UI and user management.

## API reference

See the [User management API](../../api/users/README.md) for OpenAPI operations that list and manage users.

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

Orchard Core includes the features needed to secure your app with two-factor authentication. To use two-factor authentication, enable "Two-Factor Email Method" and/or "Two-Factor Authenticator App Method". Configure the process from `Settings` → `Security` → `User Login` on the "Two-Factor Authentication" tab.

## User Localization

The feature adds the ability to configure the culture per user from the admin UI.

This feature adds a `RequestCultureProvider` to retrieve the current user culture from its claims. This feature will set a new user claim with a `CultureClaimType` named "culture". It also has a culture option to fall back to other ASP.NET Request Culture Providers by simply setting the user culture to "Use site's culture" which will also be the selected default value.

## Time zone select list customization

The **User Time Zone** editor uses the shared `ITimeZoneSelectListProvider` service for its `<select>` items. Replace `DefaultTimeZoneSelectListProvider` if you need different labels, ordering, or filtering for time zone options across Orchard Core.

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
| `UseSiteTheme`             | Boolean | Whether to use the site theme for login and OpenID authorization, device verification, sign-out, and error pages. |
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

## Remote user policy administration

With Remote Management enabled, user features contribute typed settings sections
for their tenant-wide policies. The caller requires `AccessRemoteManagement` and
`ManageUsers`; updates require HTTPS. These policies do not expose passwords,
authenticator keys, recovery codes, or user-specific MFA enrollment.

| Section | Owning feature | Managed policy |
| --- | --- | --- |
| `user-login` | Users | Local login, editable profile fields, remember-me/cookie and theme choices |
| `user-registration` | Users.Registration | Email verification, moderation, theme |
| `user-password-reset` | Users.ResetPassword | Password-reset availability and theme |
| `user-change-email` | Users.ChangeEmail | Email-change availability |
| `user-external-login` | Users.ExternalAuthentication | Single-provider selection and property-sync script settings |
| `user-external-registration` | Users.ExternalAuthentication | New external registrations, identity-field collection and username script settings |
| `user-two-factor` | Users.2FA | MFA requirement, remembered clients, recovery-code count and theme |
| `user-role-two-factor` | Users.2FA and Roles | MFA requirement for selected assignable roles |
| `user-authenticator-app` | Users.2FA.AuthenticatorApp and Users.2FA | Authenticator display name and supported six-digit code length |
| `user-email-authenticator` | Users.2FA.Email | Verification email subject/body Liquid templates |
| `user-sms-authenticator` | Users.2FA.Sms | Verification SMS Liquid template |

Feature names in this table have the `OrchardCore.` prefix. ExternalAuthentication
is enabled by an authentication-provider feature, rather than directly. Similarly,
the MFA services feature is enabled by an MFA method feature. Disabling an owning
feature removes its section from discovery. Use the schema for the precise fields:

```sh
pomi settings sections schema user-registration
pomi settings sections update user-registration --body '{"usersMustValidateEmail":true,"usersAreModerated":true}'
pomi settings sections show user-registration
pomi settings sections update user-password-reset --body '{"allowResetPassword":true}'
pomi settings sections update user-two-factor --body '{"numberOfRecoveryCodesToGenerate":7}'
```

Updates preserve omitted fields, reject unknown fields and invalid types, and skip
unchanged writes. Booleans and integers cannot be reset with null. Nullable text
can be cleared with null; existing runtime defaults apply to empty MFA templates.
Liquid templates use the same validation as the admin editor. External-login
scripts retain the existing script semantics and are not executed by a settings
update. Policy updates share admin mutation logic; registration and external-login
options are invalidated after the settings commit.

Recovery-code counts must be positive. Authenticator-app codes are limited to the
six-digit length supported by the existing Identity implementation. Role-specific
MFA requires at least one existing assignable role when enabled. Disabling it
retains inactive role selections, including deleted roles, so stale selections
cannot prevent disabling the policy.

These are site policies. They do not perform password recovery, send verification
codes, enroll a user in MFA, or configure third-party authentication credentials.
Password complexity, lockout, and host cookie configuration continue to use their
existing configuration ownership.

### Managing custom user settings remotely

Enable `OrchardCore.Users.CustomUserSettings` to expose the settings content types
with the `CustomUserSettings` stereotype. The feature adds these Pomi commands,
HTTP endpoints, and corresponding MCP tools:

| Command | Endpoint |
| --- | --- |
| `pomi users settings types` | `GET /api/users/settings/types` |
| `pomi users settings schema <name>` | `GET /api/users/settings/types/{name}/schema` |
| `pomi users settings show <userId> <name>` | `GET /api/users/{userId}/settings/{name}` |
| `pomi users settings update <userId> <name> --stdin` | `PUT /api/users/{userId}/settings/{name}` |

Remote-management access and the existing permission for the settings type are
required. Reading or updating a user's values additionally requires the existing
resource-based `ViewUsers` or `EditUsers` permission for that user. Unauthorized
types are omitted from discovery and return 404. Updates require HTTPS.

Use `schema` before composing a JSON update. The payload accepts `ContentType`
(which must match the selected type), `DisplayText`, and declared content parts.
It excludes content identities, ownership, publication state, and all account
fields such as passwords, roles, and authenticator data. Omitted values are
preserved, arrays are replaced, and explicit null field values are merged. Parts
and field containers must be objects. Content handlers validate changed values
before the identity manager persists the owning user. No standalone content item
is created, and equivalent retries skip persistence. Other settings on the user
are preserved.

The admin editor and remote API use `CustomUserSettingsService` to construct and
attach the embedded content item. Site and user settings share the same safe
content-envelope validation and schema builder.
