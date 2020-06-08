# Microsoft Authentication (`OrchardCore.Microsoft.Authentication`)

This module configures Orchard to support Microsoft Account and/or Microsoft Azure Active Directory accounts.

## Microsoft Account

Authenticates users with their Microsoft Account. 
If the site allows to register new users, a local user is created and the Microsoft Account is linked.
If a local user with the same email is found, then the external login is linked to that account, after authenticating.

You should create an app in the [Application Registration Portal](https://apps.dev.microsoft.com) and add the web platform.

Give a name for your App, create a secret that you will use it as AppSecret in Orchard, and allow the implicit flow. The default callback at Orchard is [tenant]/signin-microsoft or can be set as needed.

Configuration can be set through the _Microsoft Authentication -> Microsoft Account_ settings menu in the admin dashboard.

Available settings are:

- AppId: Application id in the Application Registration Portal.
- AppSecret: The application secret that will be used by Orchard.
- CallbackPath: The request path within the application's base path where the user-agent will be returned. The middleware will process this request when it arrives.
If no value is provided, setup Microsoft Account app to use the default path /signin-microsoft.

## Azure Active Directory

Authenticates users with their Azure AD Account.
If the site allows to register new users, a local user is created and the Azure AD account is linked.
If a local user with the same email is found, then the external login is linked to that account, after authenticating.

You can configure The Azure AD through the [Azure Portal](https://portal.azure.com) for your tenant.

Create a Web app/API App registration. The default call back in Orchard is /signin-oidc

Available settings are:

- DisplayName: The display name of the provider.
- AppId: Provide the Application ID from the properties of the above app
- TenantId: Provide the Directory ID value from the Azure Active Directory properties
- CallbackPath: The request path within the application's base path where the user-agent will be returned. The middleware will process this request when it arrives.
If no value is provided, setup Azure AD app to use the default path /signin-oidc.

#### Recipe Step

The Azure Active Directory can be set during recipes using the settings step. Here is a sample step:

```json
{
	"name": "azureADSettings",
	"appId": "86eb5541-ba2b-4255-9344-54eb73cec375",
	"tenantId": "4cc363b6-5254-4b8c-bc1b-e951a5fc85ac",
	"displayName": "Orchard Core AD App",	
	"callbackPath": "/signin-oidc"
},
```


## Users Registration

- If you want to enable new users to register to the site through their Microsoft Account and/or Microsoft Azure AD login, the `OrchardCore.Users.Registration` feature must be enabled and setup accordingly.
- An existing user can link his account to his Microsoft Account and/or Microsoft Azure AD login through the External Logins link from User menu
