---
title: External sign-in
description: Let people sign in with Microsoft, Microsoft Entra ID, Google, Facebook, GitHub, X or any OpenID Connect provider, and decide what happens the first time they do.
technical_manual:
  - reference/modules/Microsoft.Authentication/README.md
  - guides/microsoft-entra-id-integration/README.md
  - reference/modules/Google/README.md
  - reference/modules/Facebook/README.md
  - reference/modules/GitHub/README.md
  - reference/modules/X/README.md
  - reference/modules/OpenId/README.md
  - topics/security/login.md
---

# External sign-in

Instead of a password kept by your site, people can sign in with an account they already have elsewhere: a Microsoft or work account, Google, Facebook, GitHub, X, or your organization's own identity provider. Each service you set up adds a button to the login page. This page explains how to connect each service and how to decide what happens the first time someone uses one.

| Service | Menu | Permission | Feature |
| --- | --- | --- | --- |
| Microsoft account | **Settings > Security > Authentication > Microsoft** | *Manage Microsoft Authentication settings* | **Microsoft Account Authentication** (`OrchardCore.Microsoft.Authentication.MicrosoftAccount`) |
| Microsoft Entra ID | **Settings > Security > Authentication > Microsoft Entra ID** | *Manage Microsoft Authentication settings* | **Microsoft Entra ID (Azure Active Directory) Authentication** (`OrchardCore.Microsoft.Authentication.AzureAD`) |
| Google | **Settings > Security > Authentication > Google** | *Manage Google Authentication settings* | **Google Authentication** (`OrchardCore.Google.GoogleAuthentication`) |
| Facebook | **Settings > Security > Authentication > Meta** | *View and edit the Facebook app.* | **Meta Login** (`OrchardCore.Facebook.Login`) |
| GitHub | **Settings > Security > Authentication > GitHub** | *Manage GitHub Authentication settings* | **GitHub Authentication** (`OrchardCore.GitHub.Authentication`) |
| X | **Settings > Security > Authentication > Sign In with X (Twitter)** | *Manage Sign in with X (Twitter) settings* | **Sign in with X (Twitter)** (`OrchardCore.Twitter.Signin`) |
| Any OpenID Connect provider | **Settings > OpenID Connect > Authentication Client** | *View and edit the OpenID Connect client settings.* | **OpenID Connect Client Integration** (`OrchardCore.OpenId.Client`) |

Each of these features turns on **External Authentication** (`OrchardCore.Users.ExternalAuthentication`), which holds the shared options described in [Decide what happens at the first sign-in](#decide-what-happens-at-the-first-sign-in).

!!! tip "Don't see a menu?"
    Turn the feature on in **Tools > Features** (see [Features](../getting-started/features.md)). The menu also needs the permission in the table. See [Roles and permissions](../getting-started/roles-and-permissions.md).

## What people see

Once a service is set up, the login page shows a column **Use another service to log in** with one button per service: **Microsoft Account**, **Google**, **Facebook**, **GitHub**, **X (Twitter)**, and the display name you choose for Microsoft Entra ID and for an OpenID Connect provider. Clicking a button takes the person to the service to sign in, then back to your site. A signed-in person finds **External Logins** in their user menu to link or remove services, see [Signing in](../getting-started/signing-in.md#manage-linked-services).

A button appears only when its settings are complete. If a button is missing after you save, check that the ID and the secret are both filled in.

## Before you start: what to get from the service

Every service needs you to register your site with it first, on the service's developer or admin portal. This is usually done by whoever manages your Microsoft, Google, Meta, GitHub or X account; the Technical Manual pages listed at the bottom point to each portal.

From the service you need:

- **An ID** for your site, called Application ID, Client ID or API key depending on the service.
- **A secret**, a password for your site that the service generates. Keep it private.

You give the service:

- **The callback address**: your site's address followed by the **CallbackPath** shown in the settings. For example, with the default path for Google, `https://www.example.com/signin-google`. The service sends people back to this address after they sign in.

!!! warning
    Saving any of these settings pages may restart the site. The page warns: *The website might be restarted upon saving the settings, potentially leading to temporary unresponsiveness during the process.* Save at a quiet time.

### Keep secrets in Secrets

When the **Secrets** feature is on, each secret box has a list above it: **User-specified value** (type the secret; it is encrypted and stored with the settings) or **Use a secret** (pick a secret kept in the site's secret store, or click **New secret** to create one in a new tab and then reload the list). See [Secrets](../settings/secrets.md).

## Connect a Microsoft account

1. Open **Settings > Security > Authentication > Microsoft**.
2. Fill in the fields and click **Save**.

| Field | What it does |
| --- | --- |
| **AppId** | The Application ID from the Microsoft developer portal. Needed for the button to appear. |
| **Secret Key** | The application secret from the same portal. Needed for the button to appear. |
| **CallbackPath** | Where Microsoft sends people back. Leave empty to use `/signin-microsoft`. |
| **Store tokens** | Keeps the sign-in tokens Microsoft returns with the user's account, for features or custom code that call Microsoft on their behalf. Leave off unless you need it. |

## Connect Microsoft Entra ID

Use this for work or school accounts in your organization's Microsoft Entra ID (formerly Azure Active Directory).

1. Open **Settings > Security > Authentication > Microsoft Entra ID**.
2. Fill in the fields and click **Save**.

| Field | What it does |
| --- | --- |
| **Display Name** | The text of the button on the login page, for example *Contoso staff*. Needed for the button to appear. |
| **AppId** | The Application ID of the app registration in Microsoft Entra ID. Needed for the button to appear. |
| **TenantId** | The Tenant (Directory) ID of the app registration. For an app registration that accepts several organizations, type `common` or `organizations`. Needed for the button to appear. |
| **CallbackPath** | Where Entra ID sends people back. Leave empty to use `/signin-oidc`. |
| **Store tokens** | Keeps the tokens Entra ID returns with the user's account. |

## Connect Google

1. Open **Settings > Security > Authentication > Google**.
2. Fill in the fields and click **Save**.

| Field | What it does |
| --- | --- |
| **Client ID** | The client ID from the Google API console. |
| **API secret key** | The client secret from the Google API console. |
| **CallbackPath** | Leave empty to use `/signin-google`. |
| **Store tokens** | Keeps the tokens Google returns with the user's account. |

## Connect Facebook (Meta)

Facebook sign-in uses the app you register for all Meta features.

1. Open **Settings > Integrations > Meta App** and fill in **Application Identifier** and **Secret Key** from the Meta developer portal. Click **Save**. The other fields on that page are for Meta's social plugins, see [Analytics and social](../settings/analytics-and-social.md).
2. Open **Settings > Security > Authentication > Meta**.
3. Optionally change **CallbackPath** (empty means `/signin-facebook`) and tick **Store tokens**.
4. Click **Save**.

The Meta page reminds you that the **Meta Integration settings** must be configured for sign-in to work.

## Connect GitHub

1. Open **Settings > Security > Authentication > GitHub**.
2. Fill in **Client ID** and **Client secret** from your GitHub app.
3. Optionally change **CallbackPath** (empty means `/signin-github`) and tick **Store tokens**.
4. Click **Save**.

## Connect X (Twitter)

Sign-in with X uses the keys of your X app.

1. Open **Settings > Integrations > X (Twitter)** and fill in **API key**, **API secret key**, **Access Token** and **Access Token Secret** from the *Keys and tokens* tab of your X app. Click **Save**.
2. Open **Settings > Security > Authentication > Sign In with X (Twitter)**.
3. Optionally change **CallbackPath** and tick **Store tokens**. When **CallbackPath** is empty, the site uses `/signin-twitter`, even though the box shows `/signin-x` as an example. Give X the path the site really uses.
4. Click **Save**.

## Connect any OpenID Connect provider

Many identity providers, such as Okta, Auth0, Keycloak or another Orchard Core site acting as a server, speak OpenID Connect. Setting this up usually takes a developer or the provider's administrator next to you: the right flow depends on the provider.

1. Open **Settings > OpenID Connect > Authentication Client**.
2. Fill in the fields and click **Save**. The site checks the values and shows an error if something is missing, for example *The authority cannot be null or empty.*

| Field | What it does |
| --- | --- |
| **Display Name** | The text of the button on the login page. |
| **Authority** | The address of the provider, given by the provider. Required; it must be a full address without `?` or `#` parts. |
| **ClientId** | The client ID the provider gave your site. |
| **CallbackPath** | Where the provider sends people back. Empty means `/signin-oidc`. |
| **SignedOut CallbackPath** | Where the provider sends people after they sign out there. Empty means `/signout-callback-oidc`. |
| **SignedOut Redirect Uri** | The page people land on after signing out. Empty means the home page. |
| **Scopes** | Extra information to request, separated by spaces, beyond `openid` and `profile`, for example `email roles`. |
| **Store tokens?** | Keeps the provider's tokens with the user's account. |
| Flow options | Tick the response type the provider expects, under **Code Authentication Flow** (**Use 'code' response type**, the usual choice), **Hybrid Authentication Flow** or **Implicit Authentication Flow**. |
| **Client Secret** | The client secret from the provider, for the code and hybrid flows. |
| **Response Mode** | How the provider returns the answer: `form_post` or `query`. |
| **Advanced Parameters** | Extra name and value pairs sent to the provider with each sign-in request. Click **Add a parameter** to add a row. |

## Decide what happens at the first sign-in

The first time someone signs in with a service, the site has to match them to an account:

- **An account with the same email exists**: the page **Link your account.** shows that account's **Username** and asks for its **Password**. After **Register**, the service is linked and the person is signed in. If the existing account is disabled, unconfirmed or awaiting approval, the usual messages apply.
- **No account matches**: the page **Create a new account.** asks for **Username**, **Email**, **Password** and **Confirm password**, then **Register**. Fields can be skipped with the options below.

Set the options in two places.

### Registration options

Open **Settings > Security > Registration** and the **External Authentication** tab.

| Option | What it does |
| --- | --- |
| **Disable new external registration** | Only people who already have an account can sign in with a service. Others see *New registrations are disabled for this site.* |
| **Do not ask username** | The person isn't asked for a user name; one is generated. |
| **Do not ask email address** | The person isn't asked for an email when the service provides one. |
| **Do not create local password** | The person isn't asked for a password; they can only sign in with the service. |
| **Use a script to generate userName based on external provider claims** | Builds the user name with a short script based on what the service returns. A script box and **Reset Script** appear. This is usually written by a developer. |

When all three "Do not ask" options are on, the account is created without showing the form.

### Login options

Open **Settings > Security > User Login** and the **External Login** tab.

| Option | What it does |
| --- | --- |
| **Use external provider for login.** | When only one service is set up, the login page sends people straight to it instead of showing the page. Configure the registration options above too. |
| **Use a script to set user roles, user claims and user properties based on external provider claims** | Runs a script at each external sign-in, for example to give the role *Editor* to people in a certain group of your identity provider. Usually written by a developer; **Reset Script** restores the example. |

To allow only external sign-in, tick **Disable local password login.** on the **General** tab of the same page, see [Login and registration](login-and-registration.md). Make sure at least one administrator has a linked account first.
