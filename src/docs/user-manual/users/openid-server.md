---
title: OpenID Connect server
description: Let other applications sign people in through your site and call its APIs, by setting up the OpenID Connect server, its applications and its scopes.
technical_manual:
  - reference/modules/OpenId/README.md
  - topics/security/README.md
---

# OpenID Connect server

Orchard Core can act as an identity provider. Other applications, such as a mobile app, a single-page app, another website, a script or another Orchard Core tenant, then send people to your site to sign in, or ask your site for a token to call its APIs. OpenID Connect and OAuth 2.0 are the standards behind this.

This is a technical area. An administrator can do everything below in the admin, but the choices (which flows, which addresses, which scopes) depend on how the other application is built. Set it up together with the developer of that application. To let people sign in to your site with *another* provider, see [External sign-in](external-sign-in.md) instead.

| Screen | Menu | Permission | Feature |
| --- | --- | --- | --- |
| Server settings | **Settings > OpenID Connect > Authorization Server** | *View and edit the OpenID Connect server settings.* | **OpenID Connect Authorization Server** (`OrchardCore.OpenId.Server`) |
| Token validation | **Settings > OpenID Connect > Token Validation** | *View and edit the OpenID Connect validation settings.* | **OpenID Connect Token Validation** (`OrchardCore.OpenId.Validation`) |
| Applications | **Access Control > OpenID Connect > Applications** | *View, add, edit and remove the OpenID Connect applications.* | **OpenID Connect Management UI** (`OrchardCore.OpenId.Management`) |
| Scopes | **Access Control > OpenID Connect > Scopes** | *View, add, edit and remove the OpenID Connect scopes.* | **OpenID Connect Management UI** (`OrchardCore.OpenId.Management`) |

Turning on the Authorization Server also turns on the Management UI and **OpenID Connect Core Services** (`OrchardCore.OpenId`). By default only the Administrator role has these permissions.

## Words you'll meet

| Word | Meaning |
| --- | --- |
| **Application** (client) | A program allowed to use your server. It has a **Client Id** and, if it can keep a secret, a **Client Secret**. |
| **Flow** | The way an application gets a token. For example the *authorization code* flow sends a person to your login page; the *client credentials* flow lets a program get a token on its own, without a person. |
| **Scope** | A named piece of access an application asks for, such as `profile`, `email` or `roles`. |
| **Endpoint** | An address on your site that applications talk to, such as `/connect/token`. |
| **Token** | The proof an application receives and sends with its requests. |

## Configure the server

1. Open **Settings > OpenID Connect > Authorization Server**.
2. Set the options on **Configure OpenID Connect server settings**.
3. Click **Save**.

Saving may restart the site for a moment. If a combination isn't allowed, the page says why, for example *The authorization code flow cannot be enabled when the authorization and token endpoints are disabled.*

### Endpoints

Each option turns on one address on your site. Turn on only what your applications use.

| Option | Address |
| --- | --- |
| **Enable Token Endpoint** | `/connect/token`, where applications get tokens. Needed by most flows. |
| **Enable Authorization Endpoint** | `/connect/authorize`, where people are sent to sign in and give consent. |
| **Enable Pushed Authorization Endpoint** | `/connect/par`. Needs the authorization endpoint. |
| **Enable End Session Endpoint** | `/connect/logout`, so applications can sign people out of your site. |
| **Require logout confirmation** | When unticked, people are signed out without a confirmation prompt if the application proves which session it is ending. |
| **Enable User Info Endpoint** | `/connect/userinfo`, where applications read information about the signed-in person. |
| **Enable Introspection Endpoint** | `/connect/introspect`, where applications check whether a token is valid. |
| **Enable Revocation Endpoint** | `/connect/revoke`, where applications cancel a token. |

### Flows

Tick the flows the server accepts: **Allow Authorization Code Flow**, **Allow Implicit Flow**, **Allow Hybrid Flow**, **Allow Password Flow**, **Allow Refresh Token Flow** and **Allow Client Credentials Flow**. Each has a link to its specification. At least one flow must be on. The refresh token flow needs the password, authorization code or hybrid flow. Each application then picks among the flows you allow here.

### Proof Key for Code Exchange and Pushed Authorization Requests

- **Require Proof Key for Code Exchange** forces PKCE, an extra safety check, for every application, even those that don't have their own **Require Proof Key for Code Exchange** box ticked.
- **Require Pushed Authorization Requests** forces PAR for every application. The pushed authorization endpoint must be on.

### Advanced Options

*The following options are optional and intended for advanced use only.*

| Option | What it does |
| --- | --- |
| **Authority** | The public address of your site as an identity server. Leave empty to use the site's own address. |
| **Encryption Certificate Store Location**, **Encryption Certificate Store Name**, **Encryption Certificate** | A certificate installed on the server, used to encrypt tokens. Leave on **None** to let the site create and manage its own keys. Installing certificates is a job for your IT team. |
| **Signing Certificate Store Location**, **Signing Certificate Store Name**, **Signing Certificate** | The same for the certificate that signs tokens. |
| **Disable Rolling Refresh Tokens** | A refresh token can be reused until it expires, instead of being replaced each time it is used. |
| **Token Format** | **ASP.NET Core Data Protection** or **JSON Web Token (JWT)**. Ask the application's developer which one they expect. |
| **Disable Access Token Encryption (only for JWT tokens)** | JWT access tokens are signed but not encrypted, so other services can read them. Only with the JWT format. |
| **Use Reference Access Tokens** | Access tokens are kept in the database and applications receive a short reference instead. Useful when tokens carry many claims. Can't be used when other tenants validate tokens from this server. |

### Use keys from Secrets

With the **OpenID Connect Secrets** feature (`OrchardCore.OpenId.Secrets`, which needs **Secrets**), the page has a section **RSA Keys from Secrets**. Pick a **Signing Key Secret** and an **Encryption Key Secret**, which are RSA keys or certificates kept in the site's secret store. When set, they are used instead of the certificate options above. See [Secrets](../settings/secrets.md).

## Add an application

1. Open **Access Control > OpenID Connect > Applications** and click **New application**.
2. Fill in the form on **Create a new application** (see the table).
3. Click **Save**.

| Field | What it does |
| --- | --- |
| **Display Name** | Required. The name shown in the list and on the consent page. |
| **Type** | **Confidential client** for applications that run on a server and can keep a secret; they must send their secret with each token request. **Public client** for apps that run on people's devices or in the browser and can't keep a secret. |
| **Client Id** | Required and unique. The application's identifier. Use the buttons to generate a value or copy it. |
| **Client Secret** | Required for a confidential client, not allowed for a public one. Use the buttons to show, generate or copy it. Copy it before you click **Save** and give it to the developer securely; the site won't show it again. |
| Flows | Tick the flows this application uses. Only the flows allowed on the server are shown. **Allow Client Credentials Flow** is only for confidential clients, and **Allow Refresh Token Flow** needs the password, authorization code or hybrid flow. Each flow shows *Recommended Parameters* for the developer. |
| **Redirect Uris** | Shown for the authorization code, implicit and hybrid flows. The addresses your site may send people back to after they sign in, separated by spaces. |
| **Allow End Session Endpoint** | Lets the application sign people out. Then fill in **Post-Logout Redirect Uris**, the addresses people may land on afterwards, separated by spaces. |
| **Require Proof Key for Code Exchange** | Forces PKCE for this application. Check that the application supports it first. |
| **Require Pushed Authorization Requests** | Forces PAR for this application. Check that the application supports it first. |
| **Consent type** | **Explicit consent** asks the person to approve the request (the recommended option). **Implicit consent** treats the request as approved without a consent page. **External consent** rejects requests unless an approval was already granted by code. |
| **Allowed Scopes** | Tick the scopes the application may ask for. |
| **Client Credentials Roles** | Shown with the client credentials flow. The roles the application acts with when it gets a token on its own, which decide what it may do on your site. |

The application appears in the list. Use **Search** to find one by name, and **Clear** to show all again.

## Edit or delete an application

- Click **Edit** on the row, change the fields on **Edit an application**, and click **Save**. The **Client Secret** box is empty: type a value only to replace the secret. Switching from a public to a confidential client requires a new secret. The edit page also has **Allow Introspection Endpoint** and **Allow Revocation Endpoint**, which let this application use those endpoints.
- Click **Delete** on the row and confirm. The application can no longer get tokens.

## Manage scopes

A scope is a named piece of access. A site may already list **Email**, **Phone**, **Profile** and **Roles**, which give applications the person's email address, phone number, profile and roles.

1. Open **Access Control > OpenID Connect > Scopes** and click **New scope**, or **Edit** on a row.
2. Fill in the fields.
3. Click **Save**.

| Field | What it does |
| --- | --- |
| **Display Name** | Required. A readable name, shown in the list. |
| **Name** | Required and unique. The value applications send, for example `api`. |
| **Description** | Optional. What the scope gives access to. |
| **Additional resources (API audiences)** | The APIs a token with this scope may be used for, separated by spaces. To let another tenant of this installation accept the tokens, add `oct:` followed by the tenant name, for example `oct:Marketing`. A scope can't name its own tenant this way. |

To delete a scope, click **Delete** on its row and confirm. Remove it from applications that use it first.

## Validate tokens

The **Token Validation** feature lets your site accept tokens, so that an application can call your site's APIs with a token. The tokens can come from this site's own server, from another tenant of the same installation, or from another server.

1. Open **Settings > OpenID Connect > Token Validation**.
2. Fill in the fields on **Configure OpenID Connect validation settings**.
3. Click **Save**. Saving may restart the site for a moment.

| Field | What it does |
| --- | --- |
| **Authorization server tenant** | The name of the tenant whose server issues the tokens, for example this same site. Fill in this **or** **Authority**: exactly one is required. When the Authorization Server feature is on in this site, the box starts with this site's own tenant name. For another tenant, that tenant's server needs a scope listing this tenant as a resource (see above). |
| **Authority** | The address of a remote OpenID Connect server. HTTPS is highly recommended. |
| **OpenID Connect Metadata Address** | For advanced cases, the address of the server's metadata when it isn't at the usual place. Can't be used with a tenant. |
| **Audience** | The name tokens must be meant for. Required with **Authority**; not allowed with a tenant, where the tenant name is used. Can't start with `oct:`. |
| **Disable token type validation** | Accepts JWT tokens from servers that don't send the `at+jwt` type. Only with **Authority**. |

If a combination isn't allowed, the page says why when you save, for example *Either a tenant or an authority must be registered.*
