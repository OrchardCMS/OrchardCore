# OpenID management API

The `OrchardCore.OpenId.Management` feature exposes application and scope administration
through the `openid-management` capability. Operations use the same OpenID managers
and configured stores as the administration UI. Mutations share the existing admin
and recipe descriptor editors; read operations do not modify these resources.

## Authentication and permissions

All operations require the `Api` bearer scheme and `AccessRemoteManagement`.
Application operations additionally require `ManageApplications`; scope operations require
`ManageScopes`. An application principal uses its assigned Orchard roles, just as
other remote management operations do. Discovery does not require impersonating a user.

## Commands and routes

| Command | HTTP request |
| --- | --- |
| `pomi openid applications list --skip 0 --take 50` | `GET /api/openid/applications?skip=0&take=50` |
| `pomi openid applications show my-client` | `GET /api/openid/applications/by-client-id?clientId=my-client` |
| `pomi openid applications create --body-file application.json` | `POST /api/openid/applications` |
| `pomi openid applications update reporting-client --body-file application.json` | `PUT /api/openid/applications/by-client-id?clientId=reporting-client` |
| `pomi openid applications delete reporting-client --force` | `DELETE /api/openid/applications/by-client-id?clientId=reporting-client` |
| `pomi openid scopes list --skip 0 --take 50` | `GET /api/openid/scopes?skip=0&take=50` |
| `pomi openid scopes show orchardcore.management` | `GET /api/openid/scopes/by-name?name=orchardcore.management` |
| `pomi openid scopes create --body-file scope.json` | `POST /api/openid/scopes` |
| `pomi openid scopes update reporting --body-file scope.json` | `PUT /api/openid/scopes/by-name?name=reporting` |
| `pomi openid scopes delete reporting --force` | `DELETE /api/openid/scopes/by-name?name=reporting` |

Identifiers are query parameters so client IDs and scope names containing reserved
URL characters can be represented without treating them as route segments. Commands
encode those values. Application lookup uses the client ID; scope lookup uses the
scope name. The returned `id` is the physical identifier used by the admin UI.

Lists return `items`, `totalCount`, `skip` and `take`. Offset defaults to zero and
must be nonnegative; page size defaults to 50 and must be between 1 and 200. Ordering
comes from the configured OpenID store, matching its administrative paging behavior.
Separate count and page reads are not a snapshot when another request changes data.

An application response contains `id`, `clientId`, `displayName`, `clientType`,
`applicationType`, `consentType`, `roles`, `permissions`, `requirements`,
`redirectUris` and `postLogoutRedirectUris`. Permissions and requirements are the
registered OpenIddict strings, including grant and scope permissions. They describe
configuration; they do not prove that a grant is enabled by the current server.
Optional scalar values can be null when not configured.

A scope response contains `id`, `name`, `displayName`, `description` and `resources`.
Resources are the registered resource identifiers, including tenant resource entries
where configured. These reads do not expand or change scope registration.

Responses omit client secrets and their hashes, signing/key material, arbitrary
custom properties and private application settings. They never serialize the raw
application or scope entity or a complete descriptor.

Invalid paging or empty identifiers return `400`. Unknown identifiers return `404`.
Authentication and permission failures return `401` and `403` respectively.

## Create or update an application

Inspect the input contract with `pomi openid applications schema --operation create`.
For example, store the following definition in an owner-only file, replacing the role
with a registered least-privilege role and the secret with a securely generated value:

```json
{
  "clientId": "reporting-client",
  "displayName": "Reporting automation",
  "clientType": "confidential",
  "consentType": "implicit",
  "clientSecret": "<generated-secret>",
  "allowClientCredentialsFlow": true,
  "roles": ["ReportingReader"],
  "scopes": ["orchardcore.management"]
}
```

Pass credentials through `--body-file` or `--stdin`, and keep that file private.
These commands do not expose inline `--body` or `--client-secret` arguments. The
response is the redacted application description above; it never returns the secret.
This creates an application principal using its assigned roles. It does not change
the administrator account or require an interactive `pomi login`.

| Input | Behavior |
| --- | --- |
| `clientId`, `displayName`, `clientType` | Required. Client type is `public` or `confidential`. |
| `applicationType` | `web` (default) or `native`. Native applications must be public. |
| `consentType` | `explicit` (default), `implicit`, `external` or `systematic`. |
| `clientSecret` | Required when creating a confidential client or switching a public client to confidential. Omitted or empty preserves an existing confidential secret. Public clients reject a supplied secret; changing to public removes the stored secret. |
| `roles`, `scopes` | Replacement arrays; omission or `[]` clears them, and `null` is invalid. Role names must exist in this tenant. Scope names must be registered, or be `openid` or `offline_access`. Duplicate entries collapse. |
| `redirectUris`, `postLogoutRedirectUris` | Space or comma separated absolute URI strings, matching the editor/recipe format. Omission clears them. URI fragments are invalid. |
| `allowPasswordFlow`, `allowClientCredentialsFlow`, `allowAuthorizationCodeFlow`, `allowDeviceAuthorizationFlow`, `allowRefreshTokenFlow`, `allowHybridFlow`, `allowImplicitFlow` | Grant switches, each defaulting to false. The shared builder updates the corresponding endpoint and response-type permissions. The authorization server must also enable the selected flows. |
| `allowLogoutEndpoint`, `allowIntrospectionEndpoint`, `allowRevocationEndpoint` | Endpoint switches, each defaulting to false. |
| `requireProofKeyForCodeExchange`, `requirePushedAuthorizationRequests` | Requirement switches, each defaulting to false. |

Unknown JSON properties are rejected. Updates replace all exposed settings; they retain
the physical ID, custom properties, and permissions/requirements outside those owned
by the editor. Keep `clientId` equal to the query identifier; API renames are unsupported.
The existing admin editor retains its rename behavior.

Creation returns `201` with a read location. An equivalent retry, including the supplied
credential, returns `200`; a different definition or credential under the same client
identifier returns `409`. Updates return `200` and skip saving an equivalent definition.
A missing update target returns `404`. Invalid settings return `400`, retaining the
last valid application values.

Deletion returns `204`, including when the application is already absent. A deleted
application can no longer authenticate to obtain new tokens. Application settings and
credential changes govern subsequent authentication; they do not promise immediate
invalidation of all previously issued access tokens. Existing token lifetime and
validation configuration continue to apply.

## Rotate or revoke an application shared secret

Both operations require `AccessRemoteManagement` and `ManageApplications` and apply
only to confidential applications. They preserve the application's identity, roles,
permissions, redirects and other settings, using the existing OpenID application manager.

```bash
pomi openid applications credentials rotate reporting-client --secret-output-file /private/path/new-credentials.json --force
pomi openid applications credentials revoke reporting-client --force
```

Rotation requires a new `--secret-output-file`, including with `--output none`.
Pomi creates an owner-only file before sending the request and refuses an existing
file or unavailable directory before changing the credential. It saves the complete
JSON response (`clientId` and `clientSecret`) there and prints only `secretOutputFile`.
Keep this file outside source control. Update the application's existing credential
source from the file; rotation does not replace credentials saved in other Pomi contexts.
The independent administrator account and its private handoff file are unchanged.

HTTP clients call `POST api/openid/applications/credentials:rotate?clientId=...`.
A successful response returns the new secret once with `Cache-Control: no-store`.
The previous secret becomes invalid immediately: there is no overlap period. Repeating
rotation creates another replacement, so do not automatically retry an uncertain
response. If the response is lost, explicitly rotate again using another authorized
identity or a still-valid token. Previously issued secrets cannot be recovered.

`POST api/openid/applications/credentials:revoke?clientId=...` returns `204` and replaces
the secret with an undisclosed random value. Repeated revocation succeeds. The client
remains confidential; a later rotation or an editor-supplied secret can restore shared
secret authentication. Revocation does not disable independent key or assertion
authentication. Neither operation promises immediate invalidation of issued tokens;
the tenant's token lifetime and validation settings still apply.

Public clients and empty client identifiers return `400`. A missing rotation target
returns `404`; revoking a missing target returns `204`. Rejected manager validation
restores the prior tracked application settings and returns `400`.

## Create or update a scope

```json
{
  "name": "reporting",
  "displayName": "Reporting API",
  "description": "Read reporting data",
  "resources": ["reporting-api"]
}
```

`name` and `displayName` are required. Updates replace all these editable fields:
omitting `description` clears it, and omitting `resources` or sending `[]` clears
the resource list. `null` resources, empty resource identifiers, identifiers with
spaces, and unknown body properties are invalid. Duplicate resources are collapsed.
The current tenant's reserved `oct:<tenant-name>` resource is already added by the
authorization server; the same resource restriction as the admin editor applies.

Creation returns `201` and a location pointing to the name-based read operation.
Repeating creation when the name and editable fields already match returns `200`
with the existing scope. A different definition under that name returns `409`.
Updates return `200`, preserve the physical ID and unedited extension properties,
and do not save again when the editable fields already match. The body name must
match the query name; this API does not rename scopes. An unknown update target
returns `404`. Manager validation failures return `400` without retaining rejected
values in the tracked scope.

Scope deletion returns `204`, including when the scope is already absent. Deleting
or changing a scope does not remove application permission strings or revoke
previously issued tokens. These operations manage the registered scope definition;
application grant configuration and credential lifecycle are separate concerns.

## MCP

With the tenant MCP feature enabled, the same reads are available as
`openid_applications_list`, `openid_applications_show`, `openid_scopes_list` and
`openid_scopes_show`. Scope mutations add `openid_scopes_create`,
`openid_scopes_update` and `openid_scopes_delete`. Application mutations add
`openid_applications_create`, `openid_applications_update` and
`openid_applications_delete`. Credential operations add
`openid_applications_credentials_rotate` and `openid_applications_credentials_revoke`.
MCP rotation returns the secret directly to the caller, which must store it securely
and keep it out of transcripts and logs. All use the same permission
checks and response contracts,
including when the Pomi CLI feature is disabled.
