# Facebook/Meta (`OrchardCore.Facebook`)

## Facebook/Meta Module

`OrchardCore.Facebook` provides the following features:

- Meta Core Components
- Meta Login
- Meta Widgets

Configuration can be set through the _Configuration -> Meta_ settings menu in the admin dashboard.

## Core Components

Registers the Meta App with site.

Available settings are:

- AppId: Meta application id.
- AppSecret: The application secret.
- Javascript SDK Version: The FB SDK to load
- Javascript Sdk js: The sdk js file to load
- Init on every page: If set the sdk is loaded on every page, otherwise on demand.
- Parameters for the FB.init() call: a comma separated key-values that are passed to the FB.init() function

Check [Meta SDK for JavaScript](https://developers.facebook.com/docs/javascript/quickstart) for more info.

AppId and AppSecret settings are available in the [Meta for developers application's](https://developers.facebook.com/apps) page, under Basic Settings.

It registers the sdk with ResourceManager (resources: fb and fbsdk), so you can use it from liquid or razor templates

## Meta Login

Authenticates users from Meta.  
If the site allows to register new users, a local user is created and the Meta login is linked.  
If a local user with the same email is found, then the external login is linked to that account, after authenticating.

The Meta Login Product should be enabled in the [Meta for developers page](https://developers.facebook.com/apps) for web apps, and a valid OAuth redirect URI must be set.

Available settings are:

- CallbackPath: The request path within the application's base path where the user-agent will be returned. The middleware will process this request when it arrives.  
If no value is provided, setup Meta app to use the default path /signin-facebook.

## Users Registration

- If you want to enable new users to register to the site through their Meta login, the `OrchardCore.Users.Registration` feature must be enabled and setup accordingly.
- An existing user can link his account to his Meta login through the External Logins link from User menu

## Meta Social Plugins Widgets

This modules adds a `FacebookPlugin` part that can be used to integrate the [Social Plugins](https://developers.facebook.com/docs/plugins)  
It defines the following widgets:

- Chat
- Comments
- Continue With
- Like
- Quote
- Save
- Share

## Meta Pixel

This feature provides you a way to add Meta Pixel tracking to your site. Simply navigate to _Configuration -> Settings -> Meta Pixel_ settings and provide your `Pixel Identifier`.

## Meta Settings Configuration

The `OrchardCore.Facebook` module allows the user to use configuration values to override the settings configured from the admin area by calling the `ConfigureFacebookSettings()` extension method on `OrchardCoreBuilder` when initializing the app.

The following configuration values can be customized:

```json
    "OrchardCore_Facebook": {
      "AppId": "",
      "AppSecret": "",
      "FBInit": false,
      "FBInitParams": "status:true,
xfbml:true,
autoLogAppEvents:true",
      "SdkJs": "sdk.js",
      "Version": "v3.2"
    }
```

For more information please refer to [Configuration](../../core/Configuration/README.md).
