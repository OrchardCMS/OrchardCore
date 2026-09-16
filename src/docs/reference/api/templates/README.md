# Templates management API

The Templates management API manages the complete custom Liquid template definitions stored by the `OrchardCore.Templates` feature. The routes and CLI commands are registered only when that feature is enabled. Enable `OrchardCore.RemoteManagement` to publish the tenant management manifest, OpenAPI document, and CLI projection metadata.

This API manages front-end custom templates. It does not manage the separate admin template collection contributed by `OrchardCore.AdminTemplates`, and it does not render or execute templates.

## Authentication and permissions

All operations require an access token authenticated with the `Api` bearer scheme. The authenticated principal must have both:

- **Access remote management API** (`AccessRemoteManagement`)
- **Manage templates** (`ManageTemplates`)

`ManageTemplates` is security-critical because custom Liquid templates control rendered output. The same permission protects reads and mutations; the Templates feature does not define a separate view permission.

## Base route

`/api/templates`

## Endpoints

| Method | Endpoint | Description | Permission |
| --- | --- | --- | --- |
| `GET` | `/api/templates` | Lists and filters custom templates. | `ManageTemplates` |
| `GET` | `/api/templates/{name}` | Gets one custom template by stable name. | `ManageTemplates` |
| `POST` | `/api/templates` | Creates a complete custom template definition. | `ManageTemplates` |
| `PUT` | `/api/templates/{name}` | Replaces a complete custom template definition. | `ManageTemplates` |
| `DELETE` | `/api/templates/{name}` | Deletes a custom template. | `ManageTemplates` |

Every endpoint also requires `AccessRemoteManagement`.

## List templates

`GET /api/templates`

Returns templates ordered by name using a case-insensitive comparison. Search is case-insensitive and matches both `name` and `description`.

The CLI command is:

```bash
pomi templates list [--search <text>] [--skip <count>] [--take <count>]
```

### Parameters

| Location | Name | Type | Required | Description |
| --- | --- | --- | --- | --- |
| Query | `search` | string | No | Text matched against template names and descriptions. Null, empty, or whitespace returns all templates. |
| Query | `skip` | integer | No | Number of matching templates to skip. Defaults to `0`; must be at least `0`. |
| Query | `take` | integer | No | Maximum number of templates to return. Defaults to `50`; must be from `1` through `200`. |

### Request

No request body is accepted.

### Responses

| Status | Meaning |
| --- | --- |
| `200 OK` | Returns the requested page. |
| `400 Bad Request` | `skip` or `take` is outside its accepted range. |
| `401 Unauthorized` | The API bearer token is missing or invalid. |
| `403 Forbidden` | The principal lacks a required permission. |

```json
{
  "skip": 0,
  "take": 50,
  "totalCount": 2,
  "items": [
    {
      "name": "Content__Article",
      "content": "<article>{{ Model.Content | shape_render }}</article>",
      "description": "Article detail template."
    },
    {
      "name": "Widget__CallToAction",
      "content": "<a href=\"{{ Model.Url }}\">{{ Model.Text }}</a>",
      "description": "Call-to-action widget."
    }
  ]
}
```

## Show a template

`GET /api/templates/{name}`

Returns the complete stored definition. Template identity is case-insensitive, so `content__article` resolves the stored `Content__Article` definition and returns its canonical stored name.

The CLI command is:

```bash
pomi templates show Content__Article
```

### Parameters

| Location | Name | Type | Required | Description |
| --- | --- | --- | --- | --- |
| Path | `name` | string | Yes | Stable template name. Matching is case-insensitive. |

### Request

No request body is accepted.

### Responses

| Status | Meaning |
| --- | --- |
| `200 OK` | Returns the complete template definition. |
| `401 Unauthorized` | The API bearer token is missing or invalid. |
| `403 Forbidden` | The principal lacks a required permission. |
| `404 Not Found` | No template has the requested name. |

```json
{
  "name": "Content__Article",
  "content": "<article>{{ Model.Content | shape_render }}</article>",
  "description": "Article detail template."
}
```

## Create a template

`POST /api/templates`

Creates a complete custom template through the existing `TemplatesManager` document service.

The client supplies the stable name. Repeating a create with the same case-insensitive name, byte-for-byte equal `content`, and ordinally equal `description` returns `201 Created` with the existing canonical representation and performs no write. A request using that name with different content or description returns `409 Conflict`.

Each successful new create updates the single Templates document once and has no partial steps. Simultaneous writers are governed by the shared document manager's configured concurrency checking; the endpoint does not add a separate cross-request lock. After an ambiguous transport or server failure, retry the identical request: it either creates the missing definition, returns the equivalent stored definition, or reports a conflicting definition.

The CLI command reads JSON from `--body`, `--body-file`, or `--stdin`:

```bash
pomi templates create --body-file template.json
```

Inspect the discoverable JSON request schema with:

```bash
pomi templates schema --operation create
```

### Parameters

| Location | Name | Type | Required | Description |
| --- | --- | --- | --- | --- |
| Body | `name` | string | Yes | Stable, case-insensitive template name. Must contain at least one non-whitespace character. The value is stored as supplied. |
| Body | `content` | string | Yes | Liquid template source. Must contain at least one non-whitespace character. |
| Body | `description` | string or null | No | Optional editor-facing description. |

### Request

```json
{
  "name": "Content__Article",
  "content": "<article>{{ Model.Content | shape_render }}</article>",
  "description": "Article detail template."
}
```

### Responses

| Status | Meaning |
| --- | --- |
| `201 Created` | A definition was created, or an identical stable-name retry found the existing definition. The `Location` header identifies the canonical stored name. |
| `400 Bad Request` | The body is missing, `name` is blank, or `content` is blank. |
| `401 Unauthorized` | The API bearer token is missing or invalid. |
| `403 Forbidden` | The principal lacks a required permission. |
| `409 Conflict` | The case-insensitive name exists with different content or description. |

The success body is the complete template definition shown in [Show a template](#show-a-template).

A conflict uses Problem Details:

```json
{
  "status": 409,
  "detail": "A template named 'Content__Article' already exists with a different definition."
}
```

## Update a template

`PUT /api/templates/{name}`

Replaces both persisted definition properties, `content` and `description`. The body is a complete definition rather than a patch.

The body `name` must exactly match the route `name`, including casing. A mismatch is rejected before lookup or mutation; renaming requires creating the new stable name and deleting the old one. The stored dictionary resolves the matched template case-insensitively and preserves its canonical stored name.

Repeating the same update is safe. Each request stores the same complete definition and returns `200 OK`; it does not create another resource or execute the template. The update has one Templates document write and no partial steps. Simultaneous writes use the document manager concurrency behavior described for create.

The CLI command is:

```bash
pomi templates update Content__Article --body-file template.json
```

Inspect its schema with:

```bash
pomi templates schema --operation update
```

### Parameters

| Location | Name | Type | Required | Description |
| --- | --- | --- | --- | --- |
| Path | `name` | string | Yes | Stable template name. Lookup is case-insensitive. This value must exactly equal the body `name`. |
| Body | `name` | string | Yes | Stable template identity. Must contain at least one non-whitespace character and exactly match the route value. |
| Body | `content` | string | Yes | Replacement Liquid template source. Must contain at least one non-whitespace character. |
| Body | `description` | string or null | No | Complete replacement description. Omit or send `null` to clear it. |

### Request

```json
{
  "name": "Content__Article",
  "content": "<article class=\"article\">{{ Model.Content | shape_render }}</article>",
  "description": "Updated article detail template."
}
```

### Responses

| Status | Meaning |
| --- | --- |
| `200 OK` | The definition was replaced and the complete canonical representation is returned. |
| `400 Bad Request` | The body is missing, a required value is blank, or route and body names differ. |
| `401 Unauthorized` | The API bearer token is missing or invalid. |
| `403 Forbidden` | The principal lacks a required permission. |
| `404 Not Found` | No template has the route name. |

## Delete a template

`DELETE /api/templates/{name}`

Deletes the matching template through `TemplatesManager`. Matching is case-insensitive.

Deletion is convergent. The first request for an existing template removes it in one Templates document write and returns `200 OK` with the deleted representation. Repeating the request after the template is absent returns `204 No Content` and performs no write. There are no partial steps.

The CLI marks this operation destructive and requires confirmation:

```bash
pomi templates delete Content__Article --force
```

### Parameters

| Location | Name | Type | Required | Description |
| --- | --- | --- | --- | --- |
| Path | `name` | string | Yes | Stable template name. Matching is case-insensitive. |

### Request

No request body is accepted.

### Responses

| Status | Meaning |
| --- | --- |
| `200 OK` | The template existed, was deleted, and its complete prior representation is returned. |
| `204 No Content` | The template was already absent. |
| `401 Unauthorized` | The API bearer token is missing or invalid. |
| `403 Forbidden` | The principal lacks a required permission. |

## Paging envelope

List responses use lower-camel JSON properties:

| Property | Type | Description |
| --- | --- | --- |
| `skip` | integer | Number of matching templates skipped. |
| `take` | integer | Requested page size. |
| `totalCount` | integer | Total matches before paging. |
| `items` | array | Complete template definitions in the current page. |

`totalCount` is calculated after search filtering and before `skip` and `take`.

## Errors and behavioral caveats

- Authentication is always bearer-only for these routes. Cookie authentication does not satisfy the endpoint policy.
- Endpoint authorization metadata enforces both `AccessRemoteManagement` and `ManageTemplates`; handlers also enforce `ManageTemplates` before accessing the document.
- Names identify templates case-insensitively because the existing Templates document uses a case-insensitive dictionary.
- The API validates the same required name and content invariants as the admin editor.
- This API does not manage admin templates, execute Liquid, validate Liquid syntax, or render a shape.
- Problem responses use the tenant's Problem Details representation; extensions such as `traceId` can also be present.

## Source coverage

This reference covers the routes and behavior defined by:

- `src/OrchardCore.Modules/OrchardCore.Templates/Endpoints/Management/TemplateManagementEndpoints.cs`
- `src/OrchardCore.Modules/OrchardCore.Templates/Endpoints/Management/TemplateManagementModels.cs`
- `src/OrchardCore.Modules/OrchardCore.Templates/Services/TemplatesManager.cs`
- `src/OrchardCore.Modules/OrchardCore.Templates/Models/TemplatesDocument.cs`
- `src/OrchardCore.Modules/OrchardCore.Templates/Permissions.cs`
- `src/OrchardCore.Modules/OrchardCore.Templates/Startup.cs`
- `src/OrchardCore.Modules/OrchardCore.Templates/Recipes/TemplateStep.cs`
- `src/OrchardCore.Modules/OrchardCore.Templates/Deployment/AllTemplatesDeploymentSource.cs`
