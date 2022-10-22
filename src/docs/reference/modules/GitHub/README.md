# GitHub (`OrchardCore.GitHub`)

This module adds GitHub Authentication to OrchardCore.

## Authenticate with GitHub

Authenticates users with their GitHub Account.

Create an OAuth app in the [GitHub Developer applications](https://GitHub.com/settings/developers).  
In the app details, you must configure the Authorization Callback URL. The default url in OrchardCore is [tenant]/signin-github.

Configuration can be set through the _GitHub -> Authenticate with GitHub_ settings menu in the admin dashboard.

Available settings are:

- ClientID: Client ID found in your GitHub app.
- Client Secret: The secret key of your GitHub app.
- CallbackPath: The request path within the application's base path where the user-agent will be returned. The middleware will process this request when it arrives.  
If no value is provided, setup Authorization callback URL in GitHub app to use the default path /signin-github.

## Users Registration

- If you want to enable new users to register to the site through their GitHub account, the `OrchardCore.Users.Registration` feature must be enabled and setup accordingly.
- An existing user can link his account to his GitHub account through the External Logins link from User menu.

## GitHub Settings Configuration

The `OrchardCore.GitHub` module allows the user to use configuration values to override the settings configured from the admin area by calling the `ConfigureGitHubSettings()` extension method on `OrchardCoreBuilder` when initializing the app.

The following configuration values can be customized:

```json
    "OrchardCore_GitHub": {
      "ClientID": "",
      "ClientSecret": "",
      "CallbackPath": "/signin-github",
      "SaveTokens": false
    }
```

For more information please refer to [Configuration](../../core/Configuration/README.md).
