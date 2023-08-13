# Users (`OrchardCore.Users`)

The Users module enables authentication UI and user management.

## Features

The module contains the following features apart from the base feature:

- Users Change Email: Allows users to change their email address.
- Users Registration: Allows external users to sign up to the site and ask to confirm their email.
- Reset Password: Allows users to reset their password.
- User Time Zone: Provides a way to set the time zone per user.
- Custom User Settings: See [its own documentation page](CustomUserSettings/README.md).
- [Users Authentication Ticket Store](./TicketStore.md): Stores users authentication tickets on server in memory cache instead of cookies. If distributed cache feature is enabled it will store authentication tickets on distributed cache.
- Two-Factor Authentication Services: Provices Two-factor core services. This feature cannot be manually enabled or disable as it is enabled by dependency on demand.
- Two-Factor Email Method: Allows users to two-factor authenticate using an email.
- Two-Factor Authenticator App Method: Allows users to two-factor authenticate using any Authenticator App.

## Two-factor Authentication

Starting with version 1.7, OrchardCore is shipped with everything you need to secure your app with two-factor authentication. To use two-factor authentication, simply enable "Two-Factor Email Method" and/or "Two-Factor Authenticator App Method" features. You can configure the process based on your need by navigating to `Security` >> `Settings` >> `User Login`. Click on the "Two-Factor Authentication" tab and update the settings as needed.

## Custom Paths

If you want to specify custom paths to access the authentication related urls, you can change them by using this option in the appsettings.json:

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

## Videos

<iframe width="560" height="315" src="https://www.youtube-nocookie.com/embed/78m04Inmilw" title="YouTube video player" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture" allowfullscreen></iframe>

<iframe width="560" height="315" src="https://www.youtube-nocookie.com/embed/ZgDkWUi2HGs" title="YouTube video player" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture" allowfullscreen></iframe>

<iframe width="560" height="315" src="https://www.youtube-nocookie.com/embed/83-6Kj-IXPw" title="YouTube video player" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture; web-share" allowfullscreen></iframe>

<iframe width="560" height="315" src="https://www.youtube-nocookie.com/embed/BbJG_wdHbak" title="YouTube video player" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture; web-share" allowfullscreen></iframe>