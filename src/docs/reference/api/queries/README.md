# Query management API (`OrchardCore.Queries`)

## Purpose and feature

The Query management API lists query sources and creates, validates, reads, updates, deletes, and executes named queries. Enable **Queries** (`OrchardCore.Queries`). Enable a source feature, such as **SQL Queries** (`OrchardCore.Queries.Sql`), to make that source available.

The operations are advertised as the `queries` remote-management capability, version `1.0`.

## Authentication and authorization

All operations use the Orchard Core `Api` bearer scheme:

```http
Authorization: Bearer ACCESS_TOKEN
```

Every operation requires an authenticated token and `AccessRemoteManagement`.

- Listing sources/queries, reading definitions, validation, creation, update, and deletion also require `ManageQueries`.
- Execution requires the dynamic `ExecuteApi_{query-name}` permission. `ManageQueries` and `ExecuteApiAll` imply every per-query execution permission.

An absent or invalid token produces `401 Unauthorized` and a `WWW-Authenticate` challenge. A valid principal missing `AccessRemoteManagement`, `ManageQueries`, or the required execution permission produces `403 Forbidden`.

## Base route

Routes are relative to the tenant base URL:

```text
https://host/{tenant-prefix}/api/queries
```

## Endpoint summary

| Method | Path | Purpose | Additional permission |
| --- | --- | --- | --- |
| `GET` | `/api/queries` | List named queries | `ManageQueries` |
| `POST` | `/api/queries` | Create a named query | `ManageQueries` |
| `GET` | `/api/queries/sources` | List enabled query sources | `ManageQueries` |
| `POST` | `/api/queries/validate` | Validate without saving | `ManageQueries` |
| `GET` | `/api/queries/named/{name}` | Get one definition | `ManageQueries` |
| `PUT` | `/api/queries/named/{name}` | Update a definition | `ManageQueries` |
| `DELETE` | `/api/queries/named/{name}` | Delete a definition | `ManageQueries` |
| `POST` | `/api/queries/named/{name}/execute` | Execute a named query | `ExecuteApi_{query-name}` |

## Common representations

JSON property names on API DTOs are camel-cased. Query source property bags retain their source-defined casing.

### Query definition

| Property | Type | Required for create | Meaning and constraints |
| --- | --- | --- | --- |
| `name` | string | Yes | Non-blank and unique using ordinal, case-sensitive comparison. |
| `source` | string | Yes | Name of an enabled `IQuerySource`, matched case-insensitively. |
| `schema` | object, boolean, or null | No | Optional result JSON Schema. Arrays, strings, and numbers are rejected. |
| `parameterSchema` | object, boolean, or null | No | Optional parameter JSON Schema, with the same type constraint. |
| `returnContentItems` | boolean | No | Defaults to `false`. |
| `properties` | object or null | Source-dependent | Source-specific definition. A null value is stored as `{}`. |

Representative SQL definition:

```json
{
  "name": "PublishedArticles",
  "source": "Sql",
  "schema": {
    "type": "array"
  },
  "parameterSchema": {
    "type": "object",
    "properties": {
      "limit": {
        "type": "integer"
      }
    }
  },
  "returnContentItems": true,
  "properties": {
    "SqlQueryMetadata": {
      "Template": "select * from Document where Type = 'Article'"
    }
  }
}
```

For the built-in `Sql` source, `properties.SqlQueryMetadata` is required, must be an object, and its Pascal-cased string property `Template` is required and non-blank. The template must be valid Liquid and parse as SQL for the tenant's configured dialect, schema, and table prefix. Liquid output statements such as `{{ ... }}` produce a warning; SQL parameters are preferred.

### Paging envelope

Both list operations return:

```json
{
  "skip": 0,
  "take": 20,
  "totalCount": 1,
  "items": []
}
```

`skip` defaults to `0` and must be at least `0`. `take` defaults to `20` and must be from `1` through `200`.

### Errors

Errors use `application/problem+json`. A paging error is:

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "Bad Request",
  "status": 400,
  "detail": "Take cannot exceed 200."
}
```

A missing named query produces:

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.5",
  "title": "Not Found",
  "status": 404
}
```

Permission details use the shared localized Problem Details response described by the management API.

## Operations

### List named queries

```http
GET /api/queries
```

| Query parameter | Type | Required | Default/constraints |
| --- | --- | --- | --- |
| `search` | string | No | Passed as the query-name search value. |
| `source` | string | No | Restricts results to a source. |
| `skip` | integer | No | `0`; must be at least `0`. |
| `take` | integer | No | `20`; range `1`–`200`. |

```bash
curl -G -H 'Authorization: Bearer ACCESS_TOKEN' \
  --data-urlencode 'search=Article' \
  --data-urlencode 'source=Sql' \
  --data-urlencode 'skip=0' \
  --data-urlencode 'take=20' \
  'https://cms.example.com/tenant-a/api/queries'
```

`200 OK` returns definitions sorted by the query manager before paging:

```json
{
  "skip": 0,
  "take": 20,
  "totalCount": 1,
  "items": [
    {
      "name": "PublishedArticles",
      "source": "Sql",
      "schema": null,
      "parameterSchema": {
        "type": "object"
      },
      "returnContentItems": true,
      "properties": {
        "SqlQueryMetadata": {
          "Template": "select * from Document where Type = 'Article'"
        }
      }
    }
  ]
}
```

Other responses: `400` for invalid paging, `401`, `403`.

### List query sources

```http
GET /api/queries/sources
```

| Query parameter | Type | Required | Default/constraints |
| --- | --- | --- | --- |
| `skip` | integer | No | `0`; must be at least `0`. |
| `take` | integer | No | `20`; range `1`–`200`. |

```bash
curl -G -H 'Authorization: Bearer ACCESS_TOKEN' \
  --data-urlencode 'take=20' \
  'https://cms.example.com/tenant-a/api/queries/sources'
```

`200 OK` returns sources ordered by `name`, ordinal case-insensitively:

```json
{
  "skip": 0,
  "take": 20,
  "totalCount": 1,
  "items": [
    {
      "name": "Sql",
      "displayName": "SQL",
      "description": "Runs a SQL template after Liquid rendering against the current tenant database.",
      "propertiesSchema": {
        "type": "object",
        "properties": {
          "SqlQueryMetadata": {
            "type": "object",
            "required": [
              "Template"
            ],
            "properties": {
              "Template": {
                "type": "string",
                "description": "The SQL template to render and execute."
              }
            }
          }
        }
      }
    }
  ]
}
```

`displayName` falls back to the source name. `description` and `propertiesSchema` are null when no `IQuerySourceApiDescriptorProvider` supplies them.

Other responses: `400` for invalid paging, `401`, `403`.

### Create a named query

```http
POST /api/queries
Content-Type: application/json
```

The request body is a query definition. `name` and `source` are required; source-specific validation also applies.

```bash
curl -X POST -H 'Authorization: Bearer ACCESS_TOKEN' \
  -H 'Content-Type: application/json' \
  -d '{
    "name":"PublishedArticles",
    "source":"Sql",
    "returnContentItems":true,
    "properties":{
      "SqlQueryMetadata":{
        "Template":"select * from Document where Type = '\''Article'\''"
      }
    }
  }' \
  'https://cms.example.com/tenant-a/api/queries'
```

`name` and `source` are trimmed, source casing is canonicalized to the enabled source name, and a
null `properties` value is normalized to `{}`. If the normalized name already exists, exact
equality of `name`, canonical `source`, `returnContentItems`, `schema`, `parameterSchema`, and
`properties` makes the request an equivalent retry. An equivalent retry returns the existing
definition with `201 Created`; a different definition with the same name returns `409 Conflict`.

`201 Created` returns the saved or equivalent existing definition and a URL-escaped resource
location:

```http
Location: /api/queries/named/PublishedArticles
```

```json
{
  "name": "PublishedArticles",
  "source": "Sql",
  "schema": null,
  "parameterSchema": null,
  "returnContentItems": true,
  "properties": {
    "SqlQueryMetadata": {
      "Template": "select * from Document where Type = 'Article'"
    }
  }
}
```

`400 Bad Request` joins all validation messages into `detail`, for example:

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "Bad Request",
  "status": 400,
  "detail": "The query name is required. The query source is required."
}
```

Other responses: `401`, `403`, or `409` when the name belongs to a different definition.

### Validate a definition

```http
POST /api/queries/validate
Content-Type: application/json
```

The request body uses the query-definition schema. Unlike create, `name` is optional, but when supplied it is checked for uniqueness. `source` remains required. Nothing is persisted.

```bash
curl -X POST -H 'Authorization: Bearer ACCESS_TOKEN' \
  -H 'Content-Type: application/json' \
  -d '{
    "source":"Sql",
    "properties":{
      "SqlQueryMetadata":{
        "Template":"select * from Document where Id = {{ id }}"
      }
    }
  }' \
  'https://cms.example.com/tenant-a/api/queries/validate'
```

`200 OK` is returned for valid and invalid definitions; inspect `isValid` and `errors`:

```json
{
  "isValid": true,
  "errors": [],
  "warnings": [
    "Potentially unsafe Liquid output expressions ('{{ ... }}') were detected. Prefer SQL parameters when injecting user input."
  ]
}
```

An invalid definition is also `200 OK`:

```json
{
  "isValid": false,
  "errors": [
    "The SQL template is required."
  ],
  "warnings": []
}
```

Possible common errors cover missing/unavailable source, invalid result or parameter schema types, duplicate name, and source-specific validation.

Other responses: `401`, `403`.

### Get a named query

```http
GET /api/queries/named/{name}
```

| Path parameter | Type | Required | Description |
| --- | --- | --- | --- |
| `name` | string | Yes | Stored query name; URL-encode reserved characters. |

```bash
curl -H 'Authorization: Bearer ACCESS_TOKEN' \
  'https://cms.example.com/tenant-a/api/queries/named/PublishedArticles'
```

`200 OK` returns the query definition:

```json
{
  "name": "PublishedArticles",
  "source": "Sql",
  "schema": null,
  "parameterSchema": null,
  "returnContentItems": true,
  "properties": {
    "SqlQueryMetadata": {
      "Template": "select * from Document where Type = 'Article'"
    }
  }
}
```

Other responses: `401`, `403`, `404`.

### Update a named query

```http
PUT /api/queries/named/{name}
Content-Type: application/json
```

`name` in the path identifies the existing definition. The body is a query definition. Omitted or null body properties `name`, `source`, `schema`, `parameterSchema`, and `properties` inherit their stored values. Because `returnContentItems` is a non-nullable boolean, omitting it supplies `false` rather than inheriting its old value.

A blank body `name` inherits the route name. A nonblank body `name` must equal the route name
using ordinal, case-sensitive comparison. A different value, including different casing, returns
`400` before lookup or mutation. This management API does not expose a rename operation.
Repeating a successful update converges the definition to the same values and returns `200`.

```bash
curl -X PUT -H 'Authorization: Bearer ACCESS_TOKEN' \
  -H 'Content-Type: application/json' \
  -d '{
    "name":"PublishedArticles",
    "returnContentItems":true,
    "properties":{
      "SqlQueryMetadata":{
        "Template":"select * from Document where Type = '\''Article'\''"
      }
    }
  }' \
  'https://cms.example.com/tenant-a/api/queries/named/PublishedArticles'
```

`200 OK` returns the updated definition:

```json
{
  "name": "PublishedArticles",
  "source": "Sql",
  "schema": null,
  "parameterSchema": null,
  "returnContentItems": true,
  "properties": {
    "SqlQueryMetadata": {
      "Template": "select * from Document where Type = 'Article'"
    }
  }
}
```

Other responses: `400` with joined validation details, `401`, `403`, `404` when the path name is not found.

### Delete a named query

```http
DELETE /api/queries/named/{name}
```

| Path parameter | Type | Required | Description |
| --- | --- | --- | --- |
| `name` | string | Yes | Stored query name. |

```bash
curl -X DELETE -H 'Authorization: Bearer ACCESS_TOKEN' \
  'https://cms.example.com/tenant-a/api/queries/named/PublicArticles'
```

`200 OK` returns the deleted definition:

```json
{
  "name": "PublicArticles",
  "source": "Sql",
  "schema": null,
  "parameterSchema": null,
  "returnContentItems": true,
  "properties": {
    "SqlQueryMetadata": {
      "Template": "select * from Document where Type = 'Article'"
    }
  }
}
```

If the query is already absent, the convergent retry returns `204 No Content` with no body.
Other responses are `401` or `403`.

### Execute a named query

```http
POST /api/queries/named/{name}/execute
Content-Type: application/json
```

| Input | Type | Required | Default/constraints |
| --- | --- | --- | --- |
| `name` path | string | Yes | Existing query name. |
| Request body | object | No | JSON parameter name/value pairs; defaults to `{}`. Values may be any JSON value, including null. |

```bash
curl -X POST -H 'Authorization: Bearer ACCESS_TOKEN' \
  -H 'Content-Type: application/json' \
  -d '{"limit":10}' \
  'https://cms.example.com/tenant-a/api/queries/named/PublishedArticles/execute'
```

`200 OK` normalizes the source result into:

```json
{
  "count": 2,
  "items": [
    {
      "ContentItemId": "4vyxw1",
      "DisplayText": "First article"
    },
    {
      "ContentItemId": "9ab3k2",
      "DisplayText": "Second article"
    }
  ]
}
```

`count` is an `int64` or null. It uses the result object's public `Count` when that value is an `int32` or `int64`; otherwise it counts `items`. Items preserve JSON nodes or are serialized from the returned CLR values.

The stored query is resolved before its exact `ExecuteApi_{stored-query-name}` permission is
checked; `ManageQueries` and `ExecuteApiAll` imply that permission. Each successful request
executes the query again, so retrying this command is intentionally not idempotent.

Other responses: `401`, `403`, `404`.

## Legacy query execution

The legacy controller maps:

```http
GET  /api/queries/{name}?parameters={JSON-object}
POST /api/queries/{name}?parameters={JSON-object}
POST /api/queries/{name}
Content-Type: application/json
```

It is explicitly hidden from API Explorer/OpenAPI and is not a remote-management operation. It checks the same `ExecuteApi_{query-name}` permission, but deliberately returns `404` for both missing queries and unauthorized execution. For `POST`, a raw JSON body is read only when the `parameters` query value is empty. Its response is the source-specific raw `IQueryResults` object, not the stable `{ "count", "items" }` management envelope.

New remote clients should use `POST /api/queries/named/{name}/execute`. The legacy route remains relevant to existing integrations and templates.

## Endpoint coverage and sources

This page covers all **8 OpenAPI-discovered Query management route/method operations**, plus the two hidden legacy execution methods for relationship and migration context.

Source-derived from:

- `OrchardCore.Queries/Startup.cs`, `Manifest.cs`, `Permissions.cs`, and `Services/QueriesRemoteManagementCapabilityProvider.cs`
- `OrchardCore.Queries/Endpoints/Api/QueryManagementEndpoints.cs`, conventions, and view models
- `OrchardCore.Queries/Controllers/QueryApiController.cs`
- `OrchardCore.Queries.Sql/Services/SqlQuerySourceApiDescriptorProvider.cs`
- `OrchardCore.Queries.Abstractions/QueryPermissions.cs`
- `MediaAndQueriesOpenApiDiscoveryTests.cs`
