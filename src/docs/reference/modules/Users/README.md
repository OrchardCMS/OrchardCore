# Users (`OrchardCore.Users`)

The Users module enables authentication UI and user management.

## Features

The module contains the following features apart from the base feature:

- Users Change Email: Allows users to change their email address.
- Users Registration: Allows external users to sign up to the site and ask to confirm their email.
- Reset Password: Allows users to reset their password.
- User Time Zone: Provides a way to set the time zone per user.
- Custom User Settings: See [its own documentation page](CustomUserSettings/README.md).

## Custom Paths

If you want to specify custom paths to access the authentication related urls, you can change them by using this option in the appsettings.json:

``` json
  "OrchardCore": {
    "OrchardCore_Users": {
      "LoginPath": "Login",
      "LogoffPath": "Users/LogOff",
      "ChangePasswordUrl": "ChangePassword",
      "ExternalLoginsUrl": "ExternalLogins"
    }
  }
```
