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

- Enable the `OrchardCore.Users.Registration` feature when you also want local site registration.
- New external-user creation and profile generation are controlled from the Users module's [`ExternalRegistrationSettings`](../Users/README.md#external-authentication-settings).
- An existing user can link the account through the External Logins link from the user menu.

## Recipe Configuration

GitHub authentication settings can be configured using the `Settings` recipe step:

```json
{
  "steps": [
    {
      "name": "settings",
      "GitHubAuthenticationSettings": {
        "ClientID": "your-client-id",
        "ClientSecret": "your-client-secret",
        "CallbackPath": "/signin-github",
        "SaveTokens": false
      }
    }
  ]
}
```

| Property       | Type    | Description                                                                  |
|----------------|---------|------------------------------------------------------------------------------|
| `ClientID`     | String  | The Client ID from the GitHub OAuth application. **Required.**               |
| `ClientSecret` | String  | The Client Secret from the GitHub OAuth application. **Required.**           |
| `CallbackPath` | String  | The request path where the user-agent will be returned after authentication. |
| `SaveTokens`   | Boolean | Whether to save the access and refresh tokens.                               |

## GitHub Settings Configuration

The `OrchardCore.GitHub` module allows the user to use configuration values to override the settings configured from the admin area by calling the `ConfigureGitHubSettings()` extension method on `OrchardCoreBuilder` when initializing the app.

The following configuration values can be customized:

```json
{
  "OrchardCore_GitHub": {
    "ClientID": "",
    "ClientSecret": "",
    "CallbackPath": "/signin-github",
    "SaveTokens": false
  }
}
```

For more information please refer to [Configuration](../../modules/Configuration/README.md).
