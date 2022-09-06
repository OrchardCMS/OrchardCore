# Google (`OrchardCore.Google`)

This module adds Google features to OrchardCore.

## Google Analytics

Enable Google Analytics Feature to integrate tracking on the front end site.

Navigate to [Google Analytics](https://analytics.google.com/analytics/web) portal and select the Analytics account you want to use.

Copy the Tracking ID from the Admin->Tracking Info->Tracking Code link.

Το configure Orchard Core, go to _Google -> Google Analytics_ settings menu in the admin dashboard and enter the Tracking ID.

## Google Tag Manager

Enable Google Tag Manager Feature to integrate on the front end site.

Navigate to [Google Tag Manager](https://tagmanager.google.com/) portal and create a Tag Manager account.

Copy the _Container ID_ generated for you to use on your website.

Το configure Orchard Core, go to _Google -> Google Tag Manager_ settings menu in the admin dashboard and enter the Container ID.

## Google Authentication

Enable Google Authentication to allow users to use their Google Account to login.

Create a project in the [Google API Console](https://console.developers.google.com/projectselector/apis/library).

Add the Google+ API to your project. Navigate to Credentials and Create Credentials for your site.

In the 'Which API are you using' question select the Google+ API

In the 'Where will you be calling the API from' question select the Web server (e.g. node.js, Tomcat).

In the 'What data will you be accessing' question select 'User data'

Now click the 'What credentials do I need?' button and set the ClientID.  
You must also set the authorized redirect URI to point to your Orchard instance. The default url in OrchardCore is [tenant]/signin-google

The next step is to parameterize the consent screen that will appear to the user.

Now you can download your credentials.

Configuration can be set through the _Google -> Google Authentication_ settings menu in the admin dashboard.

Available settings are:

+ ClientID: The client_id field value in the downloaded json file.
+ ClientSecret: The client_secret field value in the downloaded json file.
+ CallbackPath: The request path within the application's base path where the user-agent will be returned. The middleware will process this request when it arrives.  
If no value is provided, setup Callback URL in Google API to use the default path /signin-google.

## Users Registration

+ If you want to enable new users to register to the site through their Google account, the `OrchardCore.Users.Registration` feature must be enabled and setup accordingly.
+ An existing user can link his account to his Google account through the External Logins link from User menu.

## Google Settings Configuration

The `OrchardCore.Google` module allows the user to use configuration values to override the settings configured from the admin area by calling the `ConfigureGoogleSettings()` extension method on `OrchardCoreBuilder` when initializing the app.

The following configuration values can be customized:

```json
    "OrchardCore_Google": {
      "ClientID": "",
      "ClientSecret": "",
      "CallbackPath": "/signin-google",
      "SaveTokens": false
    }
```

For more information please refer to [Configuration](../../core/Configuration/README.md).
