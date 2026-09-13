# Content definition management API (`OrchardCore.ContentTypes`)

The content definition management API lists and manages content types, content parts, and
content fields. Enable the **Content Types** feature (`OrchardCore.ContentTypes`). The feature
advertises the Remote Management capability `content-definitions`.

## Authentication and authorization

Every endpoint uses Orchard Core's `Api` authentication scheme, requires the
`AccessRemoteManagement` permission, and disables antiforgery validation. See
[Remote Management authentication](../../modules/RemoteManagement/README.md) for configuring
and sending API credentials.

In addition, read operations require `ViewContentTypes`; create, update, and delete operations
require the security-critical `EditContentTypes` permission. An unauthenticated request returns
`401 Unauthorized`; a failed permission check returns `403 Forbidden` as Problem Details.

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.4",
  "title": "Forbidden",
  "status": 403,
  "detail": "You do not have sufficient permissions to complete this request"
}
```

## Base route

`/api/content-definition`

All request and response bodies use `application/json`. Property names shown below are exact.

### Common headers

| Header | Required | Value |
| --- | --- | --- |
| `Authorization` | Yes | Credentials for the configured `Api` scheme, for example `Bearer <token>`. |
| `Content-Type` | For requests with a body | `application/json` |
| `Accept` | No | `application/json` |

## Endpoints

| Method | Path | Permission | Purpose |
| --- | --- | --- | --- |
| `GET` | `/types` | `ViewContentTypes` | List content types |
| `GET` | `/types/{name}` | `ViewContentTypes` | Get a content type |
| `POST` | `/types` | `EditContentTypes` | Create a content type |
| `PUT` | `/types/{name}` | `EditContentTypes` | Replace a content type |
| `DELETE` | `/types/{name}` | `EditContentTypes` | Delete a content type |
| `GET` | `/part-types` | `ViewContentTypes` | List registered part types |
| `GET` | `/field-types` | `ViewContentTypes` | List registered field types |
| `GET` | `/settings` | `ViewContentTypes` | List module-specific settings contracts |
| `GET` | `/settings/{name}` | `ViewContentTypes` | Get a module-specific settings JSON Schema |
| `GET` | `/parts` | `ViewContentTypes` | List content parts |
| `GET` | `/parts/{name}` | `ViewContentTypes` | Get a content part |
| `POST` | `/parts` | `EditContentTypes` | Create a content part |
| `PUT` | `/parts/{name}` | `EditContentTypes` | Replace a content part |
| `DELETE` | `/parts/{name}` | `EditContentTypes` | Delete a content part |
| `GET` | `/parts/{partName}/fields` | `ViewContentTypes` | List a part's fields |
| `GET` | `/parts/{partName}/fields/{fieldName}` | `ViewContentTypes` | Get a field |
| `POST` | `/parts/{partName}/fields` | `EditContentTypes` | Create a field |
| `PUT` | `/parts/{partName}/fields/{fieldName}` | `EditContentTypes` | Replace a field |
| `DELETE` | `/parts/{partName}/fields/{fieldName}` | `EditContentTypes` | Delete a field |

## Shared contracts

### Paging

All list endpoints accept these optional query parameters:

| Name | Type | Default | Constraints |
| --- | --- | --- | --- |
| `skip` | integer | `0` | Must be at least `0`. |
| `take` | integer | `50` | Must be from `1` through `200`. |

A list returns:

```json
{
  "skip": 0,
  "take": 50,
  "totalCount": 1,
  "items": []
}
```

`totalCount` counts the complete source collection before paging. Invalid paging returns
`400 Bad Request`. Its `detail` is exactly
`Skip must be zero or greater and take must be greater than zero.` or
`Take cannot exceed 200.`.

### Definition names

Technical names are strings of at most 128 letters or digits and must begin with a letter.
They are trimmed before validation. A content type name cannot be one of the built-in
`ContentItem` property names (`Id`, `ContentItemId`, `ContentItemVersionId`, `ContentType`,
`Published`, `Latest`, `ModifiedUtc`, `PublishedUtc`, `CreatedUtc`, `Owner`, `Author`, or
`DisplayText`). Part and field names may use those reserved names.

### Definition bodies

Content type:

| Property | Type | Required | Notes |
| --- | --- | --- | --- |
| `name` | string | Yes for create | Technical name. On update, an omitted or blank value becomes the route `name`. |
| `displayName` | string | Yes | Trimmed; must be unique, case-insensitively. |
| `settings` | object | No | Defaults to `{}`. May contain the built-in `ContentTypeSettings` object and feature-contributed settings. |
| `parts` | array | No | Defaults to `[]`; items use the attached-part contract below. |

Attached part:

| Property | Type | Required | Notes |
| --- | --- | --- | --- |
| `name` | string | No | Attached name; defaults to `partName`. A renamed part must be reusable. |
| `partName` | string | Yes | Existing part definition name. A different part must be attachable. |
| `settings` | object | No | Defaults to `{}`; may contain `ContentTypePartSettings` and contributed settings. |

Content part:

| Property | Type | Required | Notes |
| --- | --- | --- | --- |
| `name` | string | Yes for create | Technical name. On update, blank becomes the route `name`. |
| `settings` | object | No | Defaults to `{}`; may contain `ContentPartSettings` and contributed settings. |
| `fields` | array | No | Defaults to `[]`; field names must be unique within the part. |

Content field:

| Property | Type | Required | Notes |
| --- | --- | --- | --- |
| `name` | string | Yes for create | Field name. On update, blank becomes route `fieldName`. |
| `fieldName` | string | Yes | Registered content field type, such as `TextField`. |
| `settings` | object | No | Defaults to `{}`; may contain `ContentPartFieldSettings` and field-specific contributed settings. |

Built-in setting members are:

* `ContentTypeSettings`: `creatable`, `listable`, `draftable`, `versionable`, `stereotype`,
  `securable`, `description`, `category`, and `thumbnailPath`.
* `ContentTypePartSettings`: `displayName`, `description`, `position`, `displayMode`, and
  `editor`.
* `ContentPartSettings`: `attachable`, `reusable`, `displayName`, `description`, and
  `defaultPosition`.
* `ContentPartFieldSettings`: `displayName`, `description`, `editor`, `displayMode`, and
  `position`.

The four container names above are Pascal-cased exactly as shown. Settings are extensible:
other enabled features can add arbitrary members. The base definition DTO
preserves these values but cannot describe every module-specific settings
object.

Discover contracts contributed by enabled modules:

```bash
pomi content settings list --take 200
pomi content settings show AutoroutePartSettings
pomi content settings show ContentPickerFieldSettings
```

Each result contains `name`, `scope` (`ContentType`, `ContentTypePart`,
`ContentPart`, or `ContentPartField`), `appliesTo`, and a standalone `schema`.
For example, the Autoroute feature contributes `AutoroutePartSettings` for the
`AutoroutePart` attachment, while Flows contributes `FlowPartSettings` and
`BagPartSettings`. Modules opt into this catalog, so an open settings bag does
not imply that every accepted setting has a discoverable contract.

Content Fields contributes `ContentPickerFieldSettings` with scope
`ContentPartField`, applying to `ContentPickerField`. It includes `Multiple`,
`DisplayedContentTypes`, and the inherited `Required` and `Hint` settings.
A picker defaults to one item; configure `Multiple: true` before sending an
array with several IDs. Follow the [multi-select picker example](../../modules/ContentFields/README.md#multi-select-content-picker)
for a complete field definition, item payload, CLI commands, and discovery
fallback on servers without this contract.

### Errors

Missing resources return `404 Not Found` Problem Details:

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.5",
  "title": "Not Found",
  "status": 404
}
```

Create and update validation failures return `400 Bad Request` validation Problem Details.
Error keys use model property names for top-level errors and paths such as
`parts[0].partName` or `fields[0].fieldName` for nested errors.

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "detail": "The specified content field type does not exist.",
  "errors": {
    "fields[0].fieldName": [
      "The specified content field type does not exist."
    ]
  }
}
```

Validation detects missing/invalid technical names, duplicate type technical or display
names, duplicate part names, duplicate field names, unknown or non-attachable parts, renaming
non-reusable attached parts, and unknown field types. These conflicts are validation errors;
the API does not return `409 Conflict`.

Create operations are retry-safe when the client keeps the same technical name and complete
definition. Names and display names are trimmed, null settings and collections are normalized to
empty values, and attached `partName` references use the stored definition's casing. If a
definition with that name already exists, deep equality of the complete normalized definition,
including array order, settings, attached parts or fields, field types, and values, returns the
existing definition with `201 Created`. A different definition with the same name returns `400`
validation Problem Details without changing the existing definition.

## Content type operations

### List content types

`GET /api/content-definition/types`

Parameters are the shared `skip` and `take` query parameters. No body is accepted.

```bash
curl -H "Authorization: Bearer $TOKEN" \
  "https://localhost:5001/api/content-definition/types?skip=0&take=50"
```

`200 OK` returns content types ordered by technical name:

```json
{
  "skip": 0,
  "take": 50,
  "totalCount": 1,
  "items": [
    {
      "name": "Article",
      "displayName": "Article",
      "settings": {
        "ContentTypeSettings": {
          "creatable": true,
          "listable": true,
          "draftable": true,
          "versionable": true,
          "stereotype": null,
          "securable": false,
          "description": null,
          "category": null,
          "thumbnailPath": null
        }
      },
      "parts": []
    }
  ]
}
```

Also returns `400` for invalid paging, `401`, or `403`.

### Get a content type

`GET /api/content-definition/types/{name}`

| Parameter | Location | Type | Required | Notes |
| --- | --- | --- | --- | --- |
| `name` | path | string | Yes | Content type technical name; lookup is case-insensitive. |

```bash
curl -H "Authorization: Bearer $TOKEN" \
  https://localhost:5001/api/content-definition/types/Article
```

`200 OK` returns one content type:

```json
{
  "name": "Article",
  "displayName": "Article",
  "settings": {
    "ContentTypeSettings": {
      "creatable": true,
      "listable": true,
      "draftable": true,
      "versionable": true,
      "securable": false
    }
  },
  "parts": [
    {
      "name": "Article",
      "partName": "Article",
      "settings": {}
    }
  ]
}
```

Also returns `401`, `403`, or `404`.

### Create a content type

`POST /api/content-definition/types`

The body is the content type contract. `name` and `displayName` are required.

```bash
curl -X POST -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "PressRelease",
    "displayName": "Press release",
    "settings": {
      "ContentTypeSettings": {
        "creatable": true,
        "listable": true,
        "draftable": true,
        "versionable": true
      }
    },
    "parts": [
      {
        "name": "Body",
        "partName": "BodyPart",
        "settings": {}
      }
    ]
  }' \
  https://localhost:5001/api/content-definition/types
```

`201 Created` returns the created definition in the same shape and sets `Location` to
`/api/content-definition/types/PressRelease`. An identical normalized retry also returns the
existing definition with `201 Created`:

```json
{
  "name": "PressRelease",
  "displayName": "Press release",
  "settings": {
    "ContentTypeSettings": {
      "creatable": true,
      "listable": true,
      "draftable": true,
      "versionable": true,
      "securable": false
    }
  },
  "parts": [
    {
      "name": "Body",
      "partName": "BodyPart",
      "settings": {}
    }
  ]
}
```

Also returns `400` validation Problem Details, `401`, or `403`.

### Update a content type

`PUT /api/content-definition/types/{name}`

`name` is the required current technical name. The required JSON body uses the content type
contract and replaces its parts and settings. A blank body `name` uses the route name. A nonblank
body `name` must equal the route name using ordinal, case-sensitive comparison; otherwise the
request returns `400` without changing either definition. This management API does not expose a
rename operation. Repeating a successful replacement leaves the same complete definition and
returns `200`.

```bash
curl -X PUT -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "PressRelease",
    "displayName": "Press release",
    "settings": {
      "ContentTypeSettings": {
        "creatable": true,
        "listable": true,
        "draftable": true,
        "versionable": true
      }
    },
    "parts": []
  }' \
  https://localhost:5001/api/content-definition/types/PressRelease
```

`200 OK` returns the complete updated definition:

```json
{
  "name": "PressRelease",
  "displayName": "Press release",
  "settings": {
    "ContentTypeSettings": {
      "creatable": true,
      "listable": true,
      "draftable": true,
      "versionable": true,
      "securable": false
    }
  },
  "parts": []
}
```

Also returns `400`, `401`, `403`, or `404`.

### Delete a content type

`DELETE /api/content-definition/types/{name}`

`name` is a required string path parameter. No body is accepted.

```bash
curl -X DELETE -H "Authorization: Bearer $TOKEN" \
  https://localhost:5001/api/content-definition/types/PressRelease
```

`204 No Content` has no response body. Deleting an already absent type is a convergent retry and
also returns `204`. Other responses are `401` or `403`.

## Registered type operations

### List registered content part types

`GET /api/content-definition/part-types`

Accepts shared `skip` and `take`; no body. Results are ordered by `name`.

```bash
curl -H "Authorization: Bearer $TOKEN" \
  "https://localhost:5001/api/content-definition/part-types?take=50"
```

`200 OK` returns descriptors:

```json
{
  "skip": 0,
  "take": 50,
  "totalCount": 1,
  "items": [
    {
      "name": "BodyPart",
      "attachable": true,
      "reusable": false,
      "schema": {
        "type": "object",
        "properties": {}
      }
    }
  ]
}
```

The `schema` is generated from each registered CLR part type and therefore depends on enabled
features. Also returns `400`, `401`, or `403`.

### List registered content field types

`GET /api/content-definition/field-types`

Accepts shared `skip` and `take`; no body. Results are ordered by `name`.

```bash
curl -H "Authorization: Bearer $TOKEN" \
  "https://localhost:5001/api/content-definition/field-types?take=50"
```

`200 OK` returns descriptors:

```json
{
  "skip": 0,
  "take": 50,
  "totalCount": 1,
  "items": [
    {
      "name": "TextField",
      "schema": {
        "type": "object",
        "properties": {
          "Text": {
            "type": ["string", "null"]
          }
        }
      }
    }
  ]
}
```

The generated `schema` is installation dependent. Also returns `400`, `401`, or `403`.

## Content part operations

### List content parts

`GET /api/content-definition/parts`

Accepts shared `skip` and `take`; no body. Results are ordered by technical name.

```bash
curl -H "Authorization: Bearer $TOKEN" \
  "https://localhost:5001/api/content-definition/parts?skip=0&take=50"
```

`200 OK` returns:

```json
{
  "skip": 0,
  "take": 50,
  "totalCount": 1,
  "items": [
    {
      "name": "ContactPart",
      "settings": {
        "ContentPartSettings": {
          "attachable": true,
          "reusable": true,
          "displayName": "Contact"
        }
      },
      "fields": []
    }
  ]
}
```

Also returns `400`, `401`, or `403`.

### Get a content part

`GET /api/content-definition/parts/{name}`

`name` is a required string path parameter; lookup is case-insensitive.

```bash
curl -H "Authorization: Bearer $TOKEN" \
  https://localhost:5001/api/content-definition/parts/ContactPart
```

`200 OK` returns:

```json
{
  "name": "ContactPart",
  "settings": {
    "ContentPartSettings": {
      "attachable": true,
      "reusable": true,
      "displayName": "Contact"
    }
  },
  "fields": [
    {
      "name": "Email",
      "fieldName": "TextField",
      "settings": {}
    }
  ]
}
```

Also returns `401`, `403`, or `404`.

### Create a content part

`POST /api/content-definition/parts`

The required body uses the content part contract.

```bash
curl -X POST -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "ContactPart",
    "settings": {
      "ContentPartSettings": {
        "attachable": true,
        "reusable": true,
        "displayName": "Contact"
      }
    },
    "fields": [
      {
        "name": "Email",
        "fieldName": "TextField",
        "settings": {}
      }
    ]
  }' \
  https://localhost:5001/api/content-definition/parts
```

`201 Created` returns the complete definition and sets `Location` to
`/api/content-definition/parts/ContactPart`. An identical normalized retry also returns the
existing definition with `201 Created`:

```json
{
  "name": "ContactPart",
  "settings": {
    "ContentPartSettings": {
      "attachable": true,
      "reusable": true,
      "displayName": "Contact"
    }
  },
  "fields": [
    {
      "name": "Email",
      "fieldName": "TextField",
      "settings": {}
    }
  ]
}
```

Also returns `400`, `401`, or `403`.

### Update a content part

`PUT /api/content-definition/parts/{name}`

`name` is the required current name. The required content part body replaces all fields and
settings; a blank body `name` becomes the route name. A nonblank body `name` must equal the route
name using ordinal, case-sensitive comparison. A different value, including different casing,
returns `400` without mutation; this management API does not rename parts.
Repeating a successful replacement leaves the same complete definition and returns `200`.

```bash
curl -X PUT -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "ContactPart",
    "settings": {
      "ContentPartSettings": {
        "attachable": true,
        "reusable": false,
        "displayName": "Contact details"
      }
    },
    "fields": []
  }' \
  https://localhost:5001/api/content-definition/parts/ContactPart
```

`200 OK` returns the updated definition:

```json
{
  "name": "ContactPart",
  "settings": {
    "ContentPartSettings": {
      "attachable": true,
      "reusable": false,
      "displayName": "Contact details"
    }
  },
  "fields": []
}
```

Also returns `400`, `401`, `403`, or `404`.

### Delete a content part

`DELETE /api/content-definition/parts/{name}`

`name` is a required string path parameter. No body is accepted.

```bash
curl -X DELETE -H "Authorization: Bearer $TOKEN" \
  https://localhost:5001/api/content-definition/parts/ContactPart
```

`204 No Content` has no body. Deleting an already absent part is a convergent retry and also
returns `204`. Other responses are `401` or `403`.

## Content field operations

### List a part's fields

`GET /api/content-definition/parts/{partName}/fields`

`partName` is a required string path parameter. The shared `skip` and `take` query parameters
are optional. No body is accepted.

```bash
curl -H "Authorization: Bearer $TOKEN" \
  "https://localhost:5001/api/content-definition/parts/ContactPart/fields?take=50"
```

`200 OK` returns fields ordered by name:

```json
{
  "skip": 0,
  "take": 50,
  "totalCount": 1,
  "items": [
    {
      "name": "Email",
      "fieldName": "TextField",
      "settings": {
        "ContentPartFieldSettings": {
          "displayName": "Email address",
          "position": "0"
        }
      }
    }
  ]
}
```

Also returns `400`, `401`, `403`, or `404` when the part does not exist.

### Get a field

`GET /api/content-definition/parts/{partName}/fields/{fieldName}`

Both path parameters are required strings. Lookups are case-insensitive. No body is accepted.

```bash
curl -H "Authorization: Bearer $TOKEN" \
  https://localhost:5001/api/content-definition/parts/ContactPart/fields/Email
```

`200 OK` returns:

```json
{
  "name": "Email",
  "fieldName": "TextField",
  "settings": {
    "ContentPartFieldSettings": {
      "displayName": "Email address",
      "position": "0"
    }
  }
}
```

Also returns `401`, `403`, or `404` for a missing part or field.

### Create a field

`POST /api/content-definition/parts/{partName}/fields`

`partName` is required. The required body uses the content field contract.

```bash
curl -X POST -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Email",
    "fieldName": "TextField",
    "settings": {
      "ContentPartFieldSettings": {
        "displayName": "Email address",
        "position": "0"
      }
    }
  }' \
  https://localhost:5001/api/content-definition/parts/ContactPart/fields
```

`201 Created` returns the field and sets `Location` to
`/api/content-definition/parts/ContactPart/fields/Email`. An identical normalized retry also
returns the existing field with `201 Created`:

```json
{
  "name": "Email",
  "fieldName": "TextField",
  "settings": {
    "ContentPartFieldSettings": {
      "displayName": "Email address",
      "position": "0"
    }
  }
}
```

Also returns `400`, `401`, `403`, or `404` when the part does not exist.

### Update a field

`PUT /api/content-definition/parts/{partName}/fields/{fieldName}`

Both path parameters are required. The required body uses the content field contract. A blank
body `name` becomes route `fieldName`; the field type and settings replace the previous values.
A nonblank body `name` must equal the route `fieldName` using ordinal, case-sensitive comparison.
A different value, including different casing, returns `400` without mutation; this management
API does not rename fields. Repeating a successful replacement leaves the same complete field
definition and returns `200`.

```bash
curl -X PUT -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Email",
    "fieldName": "TextField",
    "settings": {
      "ContentPartFieldSettings": {
        "displayName": "Work email",
        "position": "0"
      }
    }
  }' \
  https://localhost:5001/api/content-definition/parts/ContactPart/fields/Email
```

`200 OK` returns:

```json
{
  "name": "Email",
  "fieldName": "TextField",
  "settings": {
    "ContentPartFieldSettings": {
      "displayName": "Work email",
      "position": "0"
    }
  }
}
```

Also returns `400`, `401`, `403`, or `404` for a missing part or field.

### Delete a field

`DELETE /api/content-definition/parts/{partName}/fields/{fieldName}`

Both path parameters are required strings. No body is accepted.

```bash
curl -X DELETE -H "Authorization: Bearer $TOKEN" \
  https://localhost:5001/api/content-definition/parts/ContactPart/fields/Email
```

`204 No Content` has no body. Deleting an already absent field is a convergent retry and also
returns `204`; an absent parent part does not change that result. Other responses are `401` or
`403`.

## Endpoint coverage and sources

This page covers all 17 routes mapped by:

* `src/OrchardCore.Modules/OrchardCore.ContentTypes/Endpoints/Api/ContentDefinitionApiEndpoints.cs`

Contracts and behavior were also derived from:

* `src/OrchardCore.Modules/OrchardCore.ContentTypes/Models/Api/ContentDefinitionApiModels.cs`
* `src/OrchardCore.Modules/OrchardCore.ContentTypes/Services/ContentDefinitionApiService.cs`
* `src/OrchardCore/OrchardCore.ContentManagement.Abstractions/Metadata/Settings/`
* `test/OrchardCore.Tests/Apis/ContentManagement/ContentDefinitionApi/ContentDefinitionApiTests.cs`
* `test/OrchardCore.Tests/Apis/RemoteManagement/ContentAndWorkflowEndpointMetadataTests.cs`
