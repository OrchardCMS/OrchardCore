# Custom settings management API (`OrchardCore.CustomSettings`)

The custom settings management API discovers, reads, updates, and describes named custom
settings resources. Enable the **Custom Settings** feature (`OrchardCore.CustomSettings`). The
feature advertises the Remote Management capability `custom-settings`.

A resource exists because a content type has the `CustomSettings` stereotype. Its value is an
embedded `ContentItem` envelope stored under that content type name in the tenant site settings
document. It is not an independently versioned content item, so this API does not expose create,
delete, publish, or draft operations.

## Authentication and permissions

Every endpoint uses Orchard Core's `Api` authentication scheme, requires
`AccessRemoteManagement`, requires an authenticated user, and disables antiforgery validation.

The dynamic `ManageCustomSettings_<name>` permission is evaluated for every definition. The
`AccessRemoteManagement` permission does not imply access to any custom settings resource.
Listing and shared settings schema discovery omit unauthorized definitions. Resource-specific
operations return `404 Not Found` for both an unauthorized definition and an unknown name or
content type without the `CustomSettings` stereotype, so existence is not disclosed.

Authorization is completed before the site settings document is read or loaded for update and
before content update or validation handlers run.

## Base route

`/api/custom-settings`

## Endpoints

| Method | Path | Description | Permission |
| --- | --- | --- | --- |
| `GET` | `/api/custom-settings` | List authorized custom settings definitions | Dynamic permission for each returned definition |
| `GET` | `/api/custom-settings/{name}` | Get one safe custom settings envelope | `ManageCustomSettings_<name>` |
| `PUT` | `/api/custom-settings/{name}` | Merge and validate one custom settings envelope | `ManageCustomSettings_<name>` |
| `GET` | `/api/custom-settings/{name}/schema` | Get the definition-derived JSON Schema | `ManageCustomSettings_<name>` |

## Representations

### Definition metadata

List items use lower-camel property names:

```json
{
  "name": "BlogSettings",
  "displayName": "Blog settings",
  "description": "Configures the blog."
}
```

### Safe content envelope

The show and update operations use the same safe content envelope as the value stored in the
site settings document:

```json
{
  "ContentType": "BlogSettings",
  "DisplayText": "Blog settings",
  "BlogSettingsPart": {
    "PostsPerPage": 10,
    "FeaturedTags": [
      "news"
    ]
  }
}
```

`ContentType`, `DisplayText`, and the parts attached to the selected content type are the only
accepted top-level properties. `ContentType` is optional in an update, but when present it must
exactly match the canonical, ordinal, case-sensitive content type name. Route lookup is
case-insensitive, but responses and storage always use the canonical definition name. Unknown
top-level properties are rejected.

The API never returns or accepts `Id`, `ContentItemId`, `ContentItemVersionId`, `Latest`,
`Published`, `ModifiedUtc`, `PublishedUtc`, `CreatedUtc`, `Owner`, or `Author`. A request cannot
change the content type or write another site settings property.

Part and field properties are dynamic. Use the schema operation for the current tenant's enabled
features and content definition.

## List custom settings

`GET /api/custom-settings`

| Query parameter | Type | Default | Behavior |
| --- | --- | --- | --- |
| `search` | string | none | Case-insensitive match against name, display name, or description |
| `skip` | integer | `0` | Values below zero are normalized to zero |
| `take` | integer | `50` | Clamped to the inclusive range `1` through `200` |

```bash
pomi custom-settings list --search blog --skip 0 --take 50
```

Authorization filtering occurs before counting and paging. Results are ordered by display name
and then name using ordinal comparison.

`200 OK` returns the standard paging envelope:

```json
{
  "skip": 0,
  "take": 50,
  "totalCount": 1,
  "items": [
    {
      "name": "BlogSettings",
      "displayName": "Blog settings",
      "description": "Configures the blog."
    }
  ]
}
```

An authorized request with no visible definitions returns `200` with `totalCount: 0` and an
empty `items` array.

## Get custom settings

`GET /api/custom-settings/{name}`

| Parameter | Location | Type | Required | Description |
| --- | --- | --- | --- | --- |
| `name` | path | string | Yes | Exact custom settings content type name |

```bash
pomi custom-settings show BlogSettings
```

`200 OK` returns the safe content envelope. If the site document has no stored value yet, Orchard
Core creates the in-memory default envelope from the current content definition without
persisting an independent content item.

## Update custom settings

`PUT /api/custom-settings/{name}`

The request body is required and must be a JSON object. It is merged into the current safe
envelope; omitted parts and nested properties are preserved, explicit `null` values are applied,
and arrays are replaced rather than concatenated.

```bash
pomi custom-settings update BlogSettings --json '{
  "BlogSettingsPart": {
    "PostsPerPage": 20,
    "FeaturedTags": ["news", "events"]
  }
}'
```

Equivalent HTTP request:

```http
PUT /api/custom-settings/BlogSettings HTTP/1.1
Authorization: ******
Content-Type: application/json

{
  "BlogSettingsPart": {
    "PostsPerPage": 20
  }
}
```

After authorization and structural validation, Orchard Core constructs the content item from the
current definition and stored value, invokes content update handlers, and invokes content
validation handlers. Only a successful result replaces the selected property in the mutable site
settings document and persists it through `ISiteService`. The content item is not retained in
YesSql as an ordinary independent item. Declared content parts and content fields must be JSON
objects; scalar, array, or null values for those nodes are rejected before handlers run.

`200 OK` returns the complete safe envelope after handlers have run. The stable identity is the
custom settings content type name. Repeating the same merge is retry-safe and converges on the
same embedded resource; it may invoke handlers and write the same site settings document again.
On success, the API stores the validated safe envelope in the mutable site settings document and
deletes the temporary content item so it is not committed as an independent resource. A
content-handler validation failure cancels the YesSql session, preventing the temporary item and
handler-created persistence from being committed. The operation does not partially save
individual parts.

A structurally invalid or handler-invalid payload returns `400 Bad Request` validation Problem
Details without updating the site settings document:

```json
{
  "title": "One or more validation errors occurred.",
  "status": 400,
  "detail": "The custom settings payload is invalid.",
  "errors": {
    "BlogSettingsPart.PostsPerPage": [
      "Posts per page must be greater than zero."
    ]
  }
}
```

## Get a custom settings schema

`GET /api/custom-settings/{name}/schema`

```bash
pomi custom-settings schema BlogSettings
```

The response is a draft 2020-12 JSON Schema generated from the current content type definition
and registered content part and field CLR types. It uses the same composition infrastructure as
the content item management schema, including nested part/field schemas, descriptions, enums,
constraints, and rebased local references.

The schema removes every internal property excluded by the safe envelope, permits only
`ContentType`, `DisplayText`, and attached part names at the top level, fixes `ContentType` to the
selected name when supplied, and sets `additionalProperties` to `false`. All properties are
optional because `PUT` is a merge operation.

Custom Settings also contributes each authorized dynamic schema to
`GET /api/settings/schema`. Those shared discovery entries are marked discovery-only; values
remain available only through this resource-specific API.

## Errors

| Status | Meaning |
| --- | --- |
| `400 Bad Request` | Missing/malformed JSON, forbidden or undeclared property, content-type mismatch, or content validation failure |
| `401 Unauthorized` | Missing or invalid `Api` bearer credentials |
| `403 Forbidden` | Missing `AccessRemoteManagement` |
| `404 Not Found` | Unknown, unauthorized, or non-`CustomSettings` resource |

## Endpoint coverage and sources

This page covers all four endpoints mapped by `CustomSettingsManagementEndpoints`. The contract
is implemented by `CustomSettingsManagementService`, uses `ContentItemSchemaBuilder`, and is
covered by `CustomSettingsManagementTests`.
