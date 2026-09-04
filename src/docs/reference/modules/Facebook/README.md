# Facebook/Meta (`OrchardCore.Facebook`)

## Facebook/Meta Module

`OrchardCore.Facebook` provides the following features:

- Meta Core Components
- Meta Login
- Meta Widgets

Configuration can be set through the _Settings -> Integrations -> Meta App_ settings menu in the admin dashboard.

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

- Enable the `OrchardCore.Users.Registration` feature when you also want local site registration.
- New external-user creation and profile generation are controlled from the Users module's [`ExternalRegistrationSettings`](../Users/README.md#external-authentication-settings).
- An existing user can link the account through the External Logins link from the user menu.

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

This feature provides you a way to add Meta Pixel tracking to your site. Simply navigate to _Settings -> Integrations -> Meta Pixel_ settings and provide your `Pixel Identifier`.

### Meta Conversions API

The Meta Pixel feature also lets you configure the [Meta Conversions API](https://developers.facebook.com/docs/marketing-api/conversions-api), which sends events to Meta directly from the server instead of (or in addition to) the browser-side pixel. This is useful for events that should not depend on client-side JavaScript, such as events triggered from a background process or ones affected by ad blockers/browser tracking prevention.

From the _Settings -> Integrations -> Meta Pixel_ settings page, in addition to the `Pixel Identifier`, you can configure:

- **Conversions API Access Token**: The access token generated from the [Events Manager](https://business.facebook.com/events_manager2) for the pixel's dataset. This value is stored encrypted.
- **Test Event Code**: Optional. The test event code from the Events Manager's "Test Events" tab. When set, events are sent tagged for testing and show up in the test console instead of counting as production data. Remove it before going live.

#### Meta Conversions API Event workflow task

When the `OrchardCore.Workflows` feature is also enabled, a **Meta Conversions API Event** task becomes available in the workflow editor, under the **Meta** category. Add it to any workflow to send a server-side event (e.g. `Purchase`, `Lead`) to Meta when that workflow runs.

The task accepts:

- **Event Name**: The [standard or custom event name](https://developers.facebook.com/docs/meta-pixel/reference) to send (e.g. `Purchase`, `Lead`). Supports workflow expressions.
- **Event Source URL**: Optional. The URL of the page or resource associated with the event. Supports workflow expressions.
- **Action Source**: Where the event happened, e.g. `Website`, `App`, `Email`, etc., as required by the Conversions API.
- **Event ID**: Optional. A unique identifier for the event, used to [deduplicate](https://developers.facebook.com/docs/meta-pixel/implementation/conversions-api-deduplicate) events also sent by the browser-side Meta Pixel for the same action. Supports workflow expressions.

The task produces two outcomes, `Done` and `Failed`, depending on whether the API call succeeded.

## Recipe Configuration

Facebook settings can be configured using the `Settings` recipe step:

```json
{
  "steps": [
    {
      "name": "settings",
      "FacebookSettings": {
        "AppId": "your-app-id",
        "AppSecret": "your-app-secret",
        "FBInit": true,
        "FBInitParams": "status: true,\nxfbml: true,\nautoLogAppEvents: true",
        "SdkJs": "sdk.js",
        "Version": "v3.2"
      },
      "FacebookLoginSettings": {
        "CallbackPath": "/signin-facebook",
        "SaveTokens": false
      },
      "FacebookPixelSettings": {
        "PixelId": "your-pixel-id",
        "ConversionsApiAccessToken": "your-conversions-api-access-token",
        "ConversionsApiTestEventCode": "TEST12345"
      }
    }
  ]
}
```

### Core Settings

| Property       | Type    | Description                                                        |
|----------------|---------|--------------------------------------------------------------------|
| `AppId`        | String  | The Facebook Application ID. **Required.**                         |
| `AppSecret`    | String  | The Facebook Application Secret. **Required.**                     |
| `FBInit`       | Boolean | Whether to initialize the Facebook JavaScript SDK on the frontend. |
| `FBInitParams` | String  | Additional parameters for `FB.init()` call.                        |
| `SdkJs`        | String  | The name of the SDK JavaScript file to load. **Required.**         |
| `Version`      | String  | The Facebook Graph API version to use (e.g., `v3.2`).              |

### Login Settings

| Property       | Type    | Description                                                                  |
|----------------|---------|------------------------------------------------------------------------------|
| `CallbackPath` | String  | The request path where the user-agent will be returned after authentication. |
| `SaveTokens`   | Boolean | Whether to save the access and refresh tokens.                               |

### Pixel Settings

| Property                      | Type   | Description                                                                                          |
|-------------------------------|--------|--------------------------------------------------------------------------------------------------------|
| `PixelId`                     | String | The Meta Pixel identifier.                                                                             |
| `ConversionsApiAccessToken`   | String | The Meta Conversions API access token, generated from the Events Manager. Stored encrypted.            |
| `ConversionsApiTestEventCode` | String | Optional test event code from the Events Manager "Test Events" tab. Remove it before going live.       |

## Meta Settings Configuration

The `OrchardCore.Facebook` module allows the user to use configuration values to override the settings configured from the admin area by calling the `ConfigureFacebookSettings()` extension method on `OrchardCoreBuilder` when initializing the app.

The following configuration values can be customized:

```json
{
  "OrchardCore_Facebook": {
    "AppId": "",
    "AppSecret": "",
    "FBInit": false,
    "FBInitParams": "status:true,xfbml:true,autoLogAppEvents:true",
    "SdkJs": "sdk.js",
    "Version": "v3.2"
  }
}
```

For more information please refer to [Configuration](../Configuration/README.md).
