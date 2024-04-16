# X (Twitter) (`OrchardCore.Twitter`)

This module adds Twitter for Websites features to OrchardCore.

## X (Twitter) Integration

Integrate with X. Provides a client to integrate with X API
Configuration can be set through the _X -> X Integration_ menu in the admin dashboard.

Available settings are:

- API Key: API key found in the keys and tokens tab of your twitter app.
- API Secret Key: The API secret key of your twitter app.
- Access Token: Access token key found in the keys and tokens tab of your twitter app.
- Access Token Secret: The Access token secret key of your twitter app.

### Workflows

If the OrchardCore.Workflows is enabled, a new Task is added to Update X Status

## Sign in with X (Twitter)

Authenticates users with their X Account.

Create an app in the [Twitter Developer Platform](https://developer.x.com) and enable Sign in with X.  
In the app details, you must configure the Callback URL. The default url in OrchardCore is [tenant]/signin-x.

Configuration can be set through the _X -> Sign in with X_ settings menu in the admin dashboard.

Available settings are:

- ConsumerKey: API key found in the keys and tokens tab of your twitter app.
- ConsumerSecret: The API secret key of your twitter app.
- CallbackPath: The request path within the application's base path where the user-agent will be returned. The middleware will process this request when it arrives.
If no value is provided, setup Callback URL in Twitter app to use the default path /signin-x.

### Users Registration

- If you want to enable new users to register to the site through their Twitter account, the `OrchardCore.Users.Registration` feature must be enabled and setup accordingly.
- An existing user can link his account to his Twitter account through the External Logins link from User menu.

## X (Twitter) Settings Configuration

The `OrchardCore.Twitter` module allows the user to use configuration values to override the settings configured from the admin area by calling the `ConfigureTwitterSettings()` extension method on `OrchardCoreBuilder` when initializing the app.

The following configuration values can be customized:

```json
    "OrchardCore_X": {
      "ConsumerKey": "",
      "ConsumerSecret": "",
      "AccessToken": "",
      "AccessTokenSecret": ""
    }
```

For more information please refer to [Configuration](../../core/Configuration/README.md).
