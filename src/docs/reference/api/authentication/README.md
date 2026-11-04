# Remote management authentication (`OrchardCore.OpenId`)

## Purpose and feature

Orchard Core remote management uses OAuth 2.0/OpenID Connect to obtain bearer tokens for management APIs. Enable **Remote Management** (`OrchardCore.RemoteManagement`). It depends on the dependency-only **OpenID Connect Remote Management Configuration** feature (`OrchardCore.OpenId.RemoteManagement`), which in turn enables OpenID management, server, and token validation.

This page documents the protocol endpoints configured for remote management. They are authentication protocol endpoints, not management operations: they do not require an existing management bearer token or the `AccessRemoteManagement` permission. The access token they issue is subsequently presented to management operations.

## Authentication and exact authorization model

The remote-management setup creates or repairs:

- scope `orchardcore.management`, with resource `orchardcore`;
- no client applications; client registration is owned by the CLI and MCP features;
- authorization-code, client-credentials, and refresh-token server flows;
- PKCE enforcement;
- local token validation for the current tenant;
- the fixed endpoint paths documented below.

Enable `OrchardCore.RemoteManagement.Cli` and use its configuration action to register `orchardcore-cli` and enable device authorization. The `orchardcore-cli` application has no secret, requires PKCE, uses explicit consent, and is permitted to use authorization code, device code, refresh token, logout, and revocation. Its registered loopback redirect URI is `http://127.0.0.1/callback`, and its post-logout redirect URI is `http://127.0.0.1/`. Native loopback clients may select an ephemeral port, as the Pomi CLI does.

The application is permitted to request `email`, `openid`, `profile`, `roles`, and `orchardcore.management`. The remote-management manifest advertises `openid`, `profile`, `roles`, `orchardcore.management`, and, when refresh tokens are enabled, `offline_access`.

The CLI stores human access and refresh tokens in Windows Credential Manager on
Windows. On macOS, Linux, and other Unix-like systems, it uses plaintext files
under `~/.orchardcore/credentials`; the directory is restricted to mode `0700`
and each token file to `0600`. File permissions isolate other operating-system
users but do not provide encryption at rest.

Legacy macOS Keychain and Linux Secret Service entries are not read or migrated;
contexts require one new interactive login after switching storage.

!!! important
    Enabling the server's client-credentials flow does not turn the public `orchardcore-cli` application into a confidential client. Create a separate confidential OpenID application with a secret, client-credentials and token-endpoint permissions, the management scope, and suitable application roles/permission claims.

## Base route

All paths are relative to the tenant base URL:

```text
https://host/{tenant-prefix}/
```

Remote-management setup fixes the protocol paths. OpenID discovery remains the authoritative source for endpoint URLs, especially with path-based tenants or a custom issuer.

## Endpoint summary

| Method | Path | Kind | Purpose |
| --- | --- | --- | --- |
| `GET` | `/.well-known/openid-configuration` | Protocol discovery | Discover issuer and endpoint URLs |
| `GET`, `POST` | `/connect/authorize` | Interactive protocol | Start authorization-code + PKCE sign-in |
| `POST` | `/connect/device` | Protocol | Start device authorization |
| `GET`, `POST` | `/connect/verify` | Interactive protocol | Enter, approve, or deny a device user code |
| `POST` | `/connect/token` | Protocol | Exchange an authorization/device/refresh grant or use client credentials |
| `GET`, `POST` | `/connect/logout` | Interactive protocol | End an authenticated browser session |
| `POST` | `/connect/revoke` | Protocol | Revoke a token |

The server may expose unrelated endpoints configured independently, such as userinfo, introspection, or pushed authorization. Remote-management setup preserves such settings but does not require or add them.

## Setup and configuration

An administrator with `ManageRemoteManagementConfiguration` can open the **Remote Management** admin page at:

```text
https://host/{tenant-prefix}/Admin/RemoteManagement
```

The page reports seven shared readiness checks: fixed server endpoints, authorization
code, client credentials, refresh tokens, PKCE, local validation, and management
scope. **Configure Remote Management** repairs shared settings without registering
applications or enabling device authorization.

Enable `OrchardCore.RemoteManagement.Cli` for the separate **Configure Pomi CLI**
action, which additionally registers the native application and device flow. Enable
`OrchardCore.RemoteManagement.Mcp` for **Configure MCP authentication**. Once shared
authentication is ready, OAuth-capable MCP clients discover the tenant's
`/connect/mcp/register` endpoint and register public PKCE applications automatically.
The configuration page also offers manual client IDs and callbacks under
**Advanced**. Admin configuration POST actions require
`ManageRemoteManagementConfiguration` and antiforgery tokens. The bounded,
anonymous registration endpoint never grants roles or issues access tokens.
See [MCP client configuration](../../modules/RemoteManagement/README.md#mcp-server).

The `RemoteManagement` recipe configures shared settings; `RemoteManagementCli`
adds Pomi registration. `RemoteManagementMcpConfiguration` is a recipe step with
explicit `clientId` and `redirectUris` values for MCP registration. All configuration
services use the current tenant name for local token validation.

The configuration action sets:

| Setting | Exact value |
| --- | --- |
| Authorization endpoint | `/connect/authorize` |
| Token endpoint | `/connect/token` |
| Logout endpoint | `/connect/logout` |
| Device authorization endpoint (CLI action) | `/connect/device` |
| End-user verification endpoint (CLI action) | `/connect/verify` |
| Revocation endpoint | `/connect/revoke` |
| Authorization code flow | Enabled |
| Client credentials flow | Enabled |
| Device authorization flow (CLI action) | Enabled |
| Refresh token flow | Enabled |
| Require PKCE | Enabled |
| Validation tenant | Current tenant name |
| Validation authority/metadata address | null (local server) |

## Operations

### OpenID Connect discovery

```http
GET /.well-known/openid-configuration
```

No authentication, query, header, or body parameter is required.

```bash
curl 'https://cms.example.com/tenant-a/.well-known/openid-configuration'
```

`200 OK` returns OpenIddict server metadata. Fields relevant to the Pomi CLI include:

```json
{
  "issuer": "https://cms.example.com/tenant-a/",
  "authorization_endpoint": "https://cms.example.com/tenant-a/connect/authorize",
  "token_endpoint": "https://cms.example.com/tenant-a/connect/token",
  "device_authorization_endpoint": "https://cms.example.com/tenant-a/connect/device",
  "end_session_endpoint": "https://cms.example.com/tenant-a/connect/logout",
  "revocation_endpoint": "https://cms.example.com/tenant-a/connect/revoke",
  "grant_types_supported": [
    "authorization_code",
    "client_credentials",
    "refresh_token",
    "urn:ietf:params:oauth:grant-type:device_code"
  ],
  "response_types_supported": [
    "code"
  ],
  "code_challenge_methods_supported": [
    "plain",
    "S256"
  ],
  "scopes_supported": [
    "email",
    "offline_access",
    "openid",
    "phone",
    "profile",
    "roles"
  ]
}
```

OpenIddict can add further metadata, including signing-key and algorithm fields. Custom persisted scopes such as `orchardcore.management` are accepted even when not present in the static built-in scope list. The CLI requires `issuer` and `token_endpoint`; browser login additionally requires `authorization_endpoint`, and device login requires `device_authorization_endpoint`. It rejects a discovery issuer that differs from the manifest authority after normalizing only a trailing slash.

Errors are protocol-host errors; a disabled or invalid OpenID server normally leaves this route unavailable (`404`).

### Authorization endpoint

```http
GET  /connect/authorize
POST /connect/authorize
```

Requests are anonymous at the protocol boundary, but interactive completion requires an Orchard Core user session and explicit consent unless a permanent authorization already permits the scopes.

| Query/form parameter | Type | Required | Accepted value/constraint |
| --- | --- | --- | --- |
| `client_id` | string | Yes | `orchardcore-cli` for the provisioned public client. |
| `response_type` | string | Yes | `code`. |
| `redirect_uri` | URI | Yes | Registered native loopback redirect; the CLI uses `http://127.0.0.1:{ephemeral-port}/callback`. |
| `scope` | space-separated string | Yes for management | Include `orchardcore.management`; normally `openid profile roles offline_access orchardcore.management`. |
| `state` | string | Recommended/used by CLI | Opaque anti-forgery value returned unchanged. |
| `code_challenge` | string | Yes | PKCE challenge derived from the verifier. |
| `code_challenge_method` | string | Yes | `S256` used by the CLI; server configuration also registers `plain`. |
| `prompt` | string | No | OpenID prompt values; `none` cannot display login or consent, while `login` forces authentication. |
| `max_age` | integer seconds | No | `0` or an expired age forces authentication. |

```bash
curl -G \
  --data-urlencode 'client_id=orchardcore-cli' \
  --data-urlencode 'response_type=code' \
  --data-urlencode 'redirect_uri=http://127.0.0.1:49152/callback' \
  --data-urlencode 'scope=openid profile roles offline_access orchardcore.management' \
  --data-urlencode 'state=opaque-state' \
  --data-urlencode 'code_challenge=BASE64URL_SHA256_VERIFIER' \
  --data-urlencode 'code_challenge_method=S256' \
  'https://cms.example.com/tenant-a/connect/authorize'
```

Meaningful responses:

- `302 Found` to the Orchard login flow if there is no suitable cookie session.
- `200 OK` HTML consent page for an explicit-consent request.
- `302 Found` to the registered redirect URI with `code` and `state` after approval:

```http
Location: http://127.0.0.1:49152/callback?code=AUTHORIZATION_CODE&state=opaque-state
```

- OAuth/OpenID errors are returned through OpenIddict, commonly by redirecting with `error` and `error_description`. `prompt=none` without a user session yields `login_required`; required consent yields `consent_required`.
- A consent form posts the original protocol parameters, an antiforgery token, and exactly one of `submit.Accept=Yes` or `submit.Deny=No`.

Authorization codes are single-use.

### Device authorization endpoint

```http
POST /connect/device
Content-Type: application/x-www-form-urlencoded
```

| Form parameter | Type | Required | Constraint |
| --- | --- | --- | --- |
| `client_id` | string | Yes | `orchardcore-cli`. |
| `scope` | space-separated string | No by protocol; required for management scope | Request only scopes permitted to the application. |

```bash
curl -X POST -H 'Content-Type: application/x-www-form-urlencoded' \
  --data-urlencode 'client_id=orchardcore-cli' \
  --data-urlencode 'scope=openid profile roles offline_access orchardcore.management' \
  'https://cms.example.com/tenant-a/connect/device'
```

`200 OK` returns:

```json
{
  "device_code": "DEVICE_CODE",
  "user_code": "ABCD-EFGH",
  "verification_uri": "https://cms.example.com/tenant-a/connect/verify",
  "verification_uri_complete": "https://cms.example.com/tenant-a/connect/verify?user_code=ABCD-EFGH",
  "expires_in": 900,
  "interval": 5
}
```

`device_code`, `user_code`, and `verification_uri` are required by the CLI parser. If omitted by a provider, the CLI defaults `expires_in` to `900` seconds and `interval` to `5` seconds; these are client fallbacks, not server guarantees.

Protocol validation errors are JSON, normally HTTP `400`:

```json
{
  "error": "invalid_client",
  "error_description": "The specified client application is invalid."
}
```

### End-user verification endpoint

```http
GET  /connect/verify
POST /connect/verify
```

This is an interactive HTML endpoint, not a JSON API.

| Query/form parameter | Type | Required | Constraint |
| --- | --- | --- | --- |
| `user_code` | string | No for initial page; required to identify a request | Code returned by `/connect/device`. |
| `submit.Accept` | string | POST approval only | Presence selects the approval action; the UI sends `Yes`. |
| `submit.Deny` | string | POST denial only | Presence selects the denial action; the UI sends `No`. |
| `__RequestVerificationToken` | string | POST decision | Antiforgery value generated by the HTML form. |

```bash
curl -G --data-urlencode 'user_code=ABCD-EFGH' \
  'https://cms.example.com/tenant-a/connect/verify'
```

Responses:

- `200 OK` HTML code-entry page when no code is supplied.
- `200 OK` HTML error page with `invalid_token` for an unknown code.
- `302 Found` to login when a valid device request exists but there is no Orchard user session.
- `200 OK` HTML confirmation page for a valid code.
- approval completes the OpenIddict device authorization; denial returns an OpenIddict access-denied result and redirects to `/`.

The confirmation POST flows all original protocol parameters back to the endpoint and is antiforgery-protected.

### Token endpoint

```http
POST /connect/token
Content-Type: application/x-www-form-urlencoded
```

#### Authorization code + PKCE

| Form parameter | Type | Required |
| --- | --- | --- |
| `grant_type` | string | Yes; exactly `authorization_code` |
| `client_id` | string | Yes |
| `code` | string | Yes |
| `redirect_uri` | URI | Yes; same redirect used for authorization |
| `code_verifier` | string | Yes |

```bash
curl -X POST -H 'Content-Type: application/x-www-form-urlencoded' \
  --data-urlencode 'grant_type=authorization_code' \
  --data-urlencode 'client_id=orchardcore-cli' \
  --data-urlencode 'code=AUTHORIZATION_CODE' \
  --data-urlencode 'redirect_uri=http://127.0.0.1:49152/callback' \
  --data-urlencode 'code_verifier=PKCE_VERIFIER' \
  'https://cms.example.com/tenant-a/connect/token'
```

#### Device code

| Form parameter | Type | Required |
| --- | --- | --- |
| `grant_type` | string | Yes; exactly `urn:ietf:params:oauth:grant-type:device_code` |
| `client_id` | string | Yes |
| `device_code` | string | Yes |

```bash
curl -X POST -H 'Content-Type: application/x-www-form-urlencoded' \
  --data-urlencode 'grant_type=urn:ietf:params:oauth:grant-type:device_code' \
  --data-urlencode 'client_id=orchardcore-cli' \
  --data-urlencode 'device_code=DEVICE_CODE' \
  'https://cms.example.com/tenant-a/connect/token'
```

Poll no faster than `interval`. Before approval, the endpoint returns errors including:

```json
{
  "error": "authorization_pending",
  "error_description": "The authorization request is still pending."
}
```

The CLI handles `authorization_pending`, adds five seconds after `slow_down`, and stops on `access_denied` or `expired_token`.

#### Refresh token

| Form parameter | Type | Required |
| --- | --- | --- |
| `grant_type` | string | Yes; exactly `refresh_token` |
| `client_id` | string | Yes |
| `refresh_token` | string | Yes |

```bash
curl -X POST -H 'Content-Type: application/x-www-form-urlencoded' \
  --data-urlencode 'grant_type=refresh_token' \
  --data-urlencode 'client_id=orchardcore-cli' \
  --data-urlencode 'refresh_token=REFRESH_TOKEN' \
  'https://cms.example.com/tenant-a/connect/token'
```

#### Client credentials

Use a separately configured confidential client. For application registration,
role and scope selection, and `pomi` examples, follow
[Client credentials for automation](../../modules/RemoteManagement/README.md#client-credentials-for-automation).

| Form parameter | Type | Required | Constraint |
| --- | --- | --- | --- |
| `grant_type` | string | Yes | Exactly `client_credentials`. |
| `client_id` | string | Yes | Confidential application ID. |
| `client_secret` | string | Yes | Application secret. |
| `scope` | space-separated string | Yes for management | Include `orchardcore.management`; `offline_access` is rejected. |

```bash
curl -X POST -H 'Content-Type: application/x-www-form-urlencoded' \
  --data-urlencode 'grant_type=client_credentials' \
  --data-urlencode 'client_id=automation-client' \
  --data-urlencode 'client_secret=CLIENT_SECRET' \
  --data-urlencode 'scope=orchardcore.management' \
  'https://cms.example.com/tenant-a/connect/token'
```

A successful grant returns `200 OK`:

```json
{
  "access_token": "ACCESS_TOKEN",
  "token_type": "Bearer",
  "expires_in": 3600,
  "refresh_token": "REFRESH_TOKEN",
  "id_token": "ID_TOKEN",
  "scope": "openid profile roles offline_access orchardcore.management"
}
```

`refresh_token` is present only when the grant and requested scopes allow it. `id_token` is associated with OpenID user grants, not client credentials. Token validation failures are OAuth JSON errors, normally `400`, such as `invalid_grant`, `invalid_client`, or `invalid_scope`. If the Rate Limits feature is active, the token route is assigned the password-authentication sliding-window policy (10 requests per IP) and can return `429 Too Many Requests`.

### Logout endpoint

```http
GET  /connect/logout
POST /connect/logout
```

| Query/form parameter | Type | Required | Constraint |
| --- | --- | --- | --- |
| `id_token_hint` | string | No | Valid ID token can identify the session and, when confirmation is not required, permit immediate logout. |
| `post_logout_redirect_uri` | URI | No | Must be registered; `http://127.0.0.1/` is registered for `orchardcore-cli`. |
| `state` | string | No | Opaque value returned to the client by OpenIddict. |
| `submit.Accept` / `submit.Deny` | string | Confirmation POST | UI decision fields. |
| `__RequestVerificationToken` | string | Confirmation POST | Antiforgery value. |

```bash
curl -G \
  --data-urlencode 'id_token_hint=ID_TOKEN' \
  --data-urlencode 'post_logout_redirect_uri=http://127.0.0.1/' \
  'https://cms.example.com/tenant-a/connect/logout'
```

Responses are browser-oriented: `200 OK` HTML confirmation, or `302 Found` after sign-out. An unauthenticated session may be completed immediately. With no post-logout URI, accepted logout redirects to the tenant root.

### Revocation endpoint

```http
POST /connect/revoke
Content-Type: application/x-www-form-urlencoded
```

| Form parameter | Type | Required | Accepted value/constraint |
| --- | --- | --- | --- |
| `client_id` | string | Yes for the public CLI request | `orchardcore-cli`. |
| `token` | string | Yes | Token to revoke. |
| `token_type_hint` | string | No | CLI sends `refresh_token` when revoking its stored refresh token. |

```bash
curl -X POST -H 'Content-Type: application/x-www-form-urlencoded' \
  --data-urlencode 'client_id=orchardcore-cli' \
  --data-urlencode 'token=REFRESH_TOKEN' \
  --data-urlencode 'token_type_hint=refresh_token' \
  'https://cms.example.com/tenant-a/connect/revoke'
```

Successful revocation returns `200 OK` with no required response body. OpenIddict returns OAuth JSON for invalid client requests. Per OAuth token revocation behavior, clients should not depend on the endpoint revealing whether the supplied token existed.

## Using the access token

Request the `orchardcore.management` scope, then send the token to a management operation:

```bash
curl -H 'Authorization: Bearer ACCESS_TOKEN' \
  'https://cms.example.com/tenant-a/api/management/manifest'
```

The scope identifies the `orchardcore` resource. Authorization still evaluates Orchard permission claims: the token's user or application roles must grant `AccessRemoteManagement` and the target API's domain permission.

## Endpoint coverage and sources

This page covers **10 configured protocol route/method operations**: discovery; GET/POST authorization; device authorization; GET/POST verification; token; GET/POST logout; and revocation. It also documents the admin setup surface without treating it as a remote protocol API.

Source-derived from:

- `OrchardCore.OpenId/Startup.cs`, `Manifest.cs`, and `Configuration/OpenIdServerConfiguration.cs`
- `OrchardCore.OpenId/RemoteManagementConfigurationService.cs`, status model, controller, views, and tenant configuration service
- `OrchardCore.OpenId/Controllers/AccessController.cs`
- `OrchardCore.OpenId/Services/OpenIdServerService.cs`
- `OrchardCore.RemoteManagement/Recipes/remote-management.recipe.json`
- `OrchardCore.Cli/OAuthClient.cs` and its parsers
- `OpenIdAuthenticationTests.cs`, `RemoteManagementConfigurationServiceTests.cs`, and `OAuthClientTests.cs`
