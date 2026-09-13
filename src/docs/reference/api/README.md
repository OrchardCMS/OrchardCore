# Management API reference

The Orchard Core management APIs expose tenant administration operations through a tenant-specific OpenAPI document. The [`OrchardCore.RemoteManagement`](../modules/RemoteManagement/README.md) feature publishes discovery metadata and adds command-line projection metadata for the `pomi` CLI.

Each API belongs to the feature that owns the underlying resource. Consequently, the operations available to a tenant depend on its enabled features.

This reference currently covers 21 feature-owned management resource groups and two protocol groups. A tenant exposes only the groups contributed by its enabled features.

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
- [Workflows](workflows/README.md)

### Administration

- [Custom settings](custom-settings/README.md)
- [Features](features/README.md)
- [Home Route](home-route/README.md)
- [OpenID applications and scopes](openid/README.md)
- [Localization and dynamic translations](localization/README.md)
- [Recipes](recipes/README.md)
- [Roles](roles/README.md)
- [Site settings](settings/README.md)
- [Users](users/README.md)

### Hosting

- [Tenants](tenants/README.md)

## Request and response conventions

- JSON property names use the casing shown in request and response examples. They are generated from the shipped endpoint contracts.
- Send JSON request bodies with `Content-Type: application/json`.
- Binary upload operations document their required content type on their API page.
- Collection operations that support paging use zero-based `skip` and a bounded `take`; their pages define the exact defaults and envelope.
- Validation failures use the response representation documented for the operation. Do not assume that all modules return one global error shape.
- Content items, query parameters, workflow definitions, and other extensible resources may contain feature-contributed properties. Use the tenant's OpenAPI document and schema operations as the authoritative contract for those dynamic portions.
- Endpoint summary permissions are not always the complete authorization contract. Resource pages document additional per-item or related-resource checks, including whether inaccessible resources are filtered, rejected, or preserved.

## Retrying mutations

Do not infer retry behavior from the HTTP method alone. Each resource page documents the implemented contract for repeated mutations, including:

- the status returned for an identical retry;
- the fields used to decide whether an existing resource is equivalent or conflicting;
- whether deletes and lifecycle transitions converge when the requested state already exists;
- whether destination creation or replacement is atomic during concurrent requests;
- stable client-supplied identifiers that make creation retry-safe;
- partial-progress recovery for multi-step operations.

Some operations are intentionally not idempotent. In particular, a create operation that only uses a server-generated identifier, an upload without a stable resource identity, or a command that starts an execution can produce a new resource or side effect on each successful request. The operation's API page calls out these exceptions.

## OpenAPI and CLI projection

The tenant OpenAPI document is available at the URL returned as `openApiUrl` by the authenticated manifest. Standard OpenAPI operation metadata defines the HTTP contract. The `x-oc-cli` extension projects selected operations into `pomi <resource> <verb>` commands; it does not change endpoint authorization or HTTP behavior.

API clients should use OpenAPI operation IDs and routes rather than deriving HTTP behavior from CLI command names.

GraphQL uses the built-in [`pomi graphql` commands](../modules/Apis.GraphQL/README.md#use-graphql-from-the-cli),
which send documents and introspection queries directly to the GraphQL endpoint.
They do not project GraphQL operations through OpenAPI.
