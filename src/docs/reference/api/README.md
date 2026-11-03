# Management API reference

The Orchard Core management APIs expose tenant administration operations through a tenant-specific OpenAPI document. The [`OrchardCore.RemoteManagement`](../modules/RemoteManagement/README.md) feature publishes discovery metadata and adds command-line projection metadata for the `pomi` CLI.

Each API belongs to the feature that owns the underlying resource. Consequently, the operations available to a tenant depend on its enabled features.

This reference covers 30 feature-owned management resource groups and two protocol groups. A tenant exposes only the groups contributed by its enabled features.

## Before you begin

1. Enable and configure [Remote Management](../modules/RemoteManagement/README.md#enable-and-configure).
2. Use the public [discovery endpoint](discovery/README.md) to locate the tenant's authentication authority and authenticated management manifest.
3. Request the `orchardcore.management` scope and authenticate with a supported OpenID Connect grant.
4. Fetch the OpenAPI document identified by the authenticated manifest.

All paths in this reference are relative to the tenant URL. For example, a tenant mounted at `https://cms.example.com/acme` exposes `/api/features` at `https://cms.example.com/acme/api/features`.

## Authentication

Except where a page explicitly marks an operation as anonymous, send an API access token:

```http
Authorization: Bearer <access-token>
Accept: application/json
```

Remote clients should request the `orchardcore.management` scope, which identifies the `orchardcore` resource. Endpoint authorization evaluates the authenticated principal's permission claims rather than checking the scope directly. The principal must have **Access remote management API** and the operation-specific permission listed on the API page. A missing or invalid token results in `401 Unauthorized`; an authenticated principal without a required permission receives `403 Forbidden`.

See [Authentication](authentication/README.md) for the OpenID Connect endpoints and grants used by remote clients.

## API groups

### Protocol

- [Discovery and manifest](discovery/README.md)
- [Authentication](authentication/README.md)

### Content

- [Content definitions](content-definitions/README.md)
- [Content items](content-items/README.md)
- [Content localizations](content-localizations/README.md)
- [Layers](layers/README.md)
- [Media](media/README.md)
- [Placements](placements/README.md)
- [Queries](queries/README.md)
- [Shortcode templates](shortcode-templates/README.md)
- [Templates](templates/README.md)
- [Themes](themes/README.md)
- [Sitemaps](sitemaps/README.md)
- [URL Rewriting](url-rewriting/README.md)
- [Workflows](workflows/README.md)

### Administration

- [Audit Trail](audit-trail/README.md)
- [Background tasks](background-tasks/README.md)
- [Custom settings](custom-settings/README.md)
- [Deployment](deployment/README.md)
- [Email](email/README.md)
- [Features](features/README.md)
- [Home Route](home-route/README.md)
- [Indexes](indexes/README.md)
- [OpenID applications and scopes](openid/README.md)
- [Localization and dynamic translations](localization/README.md)
- [Rate Limits](rate-limits/README.md)
- [Recipes](recipes/README.md)
- [Remote Deployment](deployment-remote/README.md)
- [Roles](roles/README.md)
- [Site settings](settings/README.md)
- [Users](users/README.md)

### Hosting

- [Tenants](tenants/README.md)

## Request and response conventions

- A management action that isn't the creation, replacement or deletion of the resource uses `POST` with a colon before the name of the action, for example `POST /api/content/{contentItemId}:publish` or `POST /api/content:validate`. A path segment names a resource, including a sub-resource replaced by `PUT` such as `/api/content/{contentItemId}/draft`. Routes that predate the management APIs keep their form.
- JSON property names use the casing shown in request and response examples. They are generated from the shipped endpoint contracts.
- Send JSON request bodies with `Content-Type: application/json`.
- Binary upload operations document their required content type on their API page.
- Collection operations that support paging use zero-based `skip` and a bounded `take`; their pages define the exact defaults and envelope.
- Validation failures use the response representation documented for the operation. Do not assume that all modules return one global error shape.
- Failures are reported as [Problem Details](#error-codes), with a stable `code` to tell them apart.
- Content items, query parameters, workflow definitions, and other extensible resources may contain feature-contributed properties. Use the tenant's OpenAPI document and schema operations as the authoritative contract for those dynamic portions.
- Endpoint summary permissions are not always the complete authorization contract. Resource pages document additional per-item or related-resource checks, including whether inaccessible resources are filtered, rejected, or preserved.

## Error codes

Error responses use the RFC 9457 `application/problem+json` format and carry a `code` extension for the shared failure kinds. Match this value rather than the `title` and `detail`, which are localized and can change:

| `code` | Status | Meaning |
|---|---|---|
| `validation` | `400` | The request is invalid. Validation failures also list their `errors` per property. |
| `unauthorized` | `401` | The request has no valid credentials. |
| `forbidden` | `403` | The caller isn't allowed to perform the request. |
| `not_found` | `404` | The resource doesn't exist. |
| `conflict` | `409` | The request conflicts with the current state of the resource. |
| `precondition_failed` | `412` | The `If-Match` header doesn't match the current version of the resource. See [Optimistic concurrency](#optimistic-concurrency). |
| `rate_limited` | `429` | Too many requests were sent. The `Retry-After` header tells when to retry, when the limiter knows. See [Rate limits](#rate-limits). |

The `code` is absent on other status codes, and on a response whose `Accept` header excludes JSON. An operation can return a more specific `code`, which its page documents. `pomi` keeps the `code` in `error.details.code` of its JSON error output.

## Optimistic concurrency

Two clients reading a resource and then replacing it can silently overwrite each other's change. To detect this, send back the version you read in the `If-Match` header of the update:

```http
GET /api/roles/Editor
```

```http
HTTP/1.1 200 OK
ETag: "90a4c79054827d59ffac501494fc6462"
```

```http
PUT /api/roles/Editor
If-Match: "90a4c79054827d59ffac501494fc6462"
Content-Type: application/json

{ "roleDescription": "Content editors" }
```

- Reading, creating and updating the resource returns its current version in the `ETag` header. It is a strong entity tag computed from the state of the resource, so saving the same state yields the same version.
- `If-Match` is optional. Without it, an update applies unconditionally, as it does for a client unaware of versions.
- When the resource changed since, the update answers `412 Precondition Failed` with the `precondition_failed` code and the current version in the `ETag` header, and changes nothing. Read the resource again, reapply your change and retry.
- `If-Match: *` matches any existing resource. An unknown resource answers `404`.
- The version covers what an update can replace. It doesn't change when something else does, such as a login attempt on a user, the features enabled on the tenant for a role, or the position of a URL rewriting rule. Secrets aren't returned, so changing one alone doesn't change the version either, and an update omitting it keeps it.
- The check detects a stale write. It doesn't serialize two requests running at the very same time, the last of which still wins.

These resources support it, and declare the headers in the OpenAPI document: [roles](roles/README.md), [users](users/README.md), [tenants](tenants/README.md), [OpenID applications and scopes](openid/README.md), [content types and parts](content-definitions/README.md), [queries](queries/README.md), [layers](layers/README.md), [index definitions](indexes/README.md), [placements](placements/README.md), [templates](templates/README.md), [URL rewriting rules](url-rewriting/README.md), [workflow types](workflows/README.md), [sitemaps](sitemaps/README.md), [site settings and their sections](settings/README.md).

With `pomi`, update commands have an `--if-match` option generated from the OpenAPI document, and `--show-etag` writes the version to the standard error:

```bash
pomi roles show Editor --show-etag --output json
pomi roles update Editor --if-match '"90a4c79054827d59ffac501494fc6462"' --stdin < role.json
```

MCP tools can't send this header, so their updates apply unconditionally.

## Rate limits

With the [Rate Limits](../modules/RateLimits/README.md) feature, a rejected request gets a `429 Too Many Requests` response with the `rate_limited` code. Its `Retry-After` header gives the number of seconds to wait, unless the limiter has no window, as a concurrency limiter doesn't.

## Retrying mutations

Deleting a resource that doesn't exist returns `404 Not Found` on every management API, with the exception of the legacy routes that predate them, so a client repeating a delete must accept this status. A successful deletion returns what the page of the resource documents.

A create request for a resource whose identity the client chooses, such as a name, is retry-safe: repeating it with an equivalent definition returns `200 OK` with the stored resource and changes nothing, and a different definition under the same identity returns `409 Conflict`.

Do not infer other retry behavior from the HTTP method alone. Each resource page documents the implemented contract for repeated mutations, including:

- the status returned for an identical retry;
- the fields used to decide whether an existing resource is equivalent or conflicting;
- whether lifecycle transitions converge when the requested state already exists;
- whether destination creation or replacement is atomic during concurrent requests;
- stable client-supplied identifiers that make creation retry-safe;
- partial-progress recovery for multi-step operations.

Some operations are intentionally not idempotent. In particular, a create operation that only uses a server-generated identifier, an upload without a stable resource identity, or a command that starts an execution can produce a new resource or side effect on each successful request. The operation's API page calls out these exceptions.

## OpenAPI and CLI projection

The tenant OpenAPI document is available at the URL returned as `openApiUrl` by the authenticated manifest. Standard OpenAPI operation metadata defines the HTTP contract, including the `Bearer` security scheme, which is optional on the operations allowing anonymous access. The `x-oc-cli` extension projects selected operations into `pomi <resource> <verb>` commands; it does not change endpoint authorization or HTTP behavior.

API clients should use OpenAPI operation IDs and routes rather than deriving HTTP behavior from CLI command names.

GraphQL uses the built-in [`pomi graphql` commands](../modules/Apis.GraphQL/README.md#use-graphql-from-the-cli),
which send documents and introspection queries directly to the GraphQL endpoint.
They do not project GraphQL operations through OpenAPI.
