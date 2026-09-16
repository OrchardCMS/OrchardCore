# Shortcode templates API

The `OrchardCore.Shortcodes.Templates` feature manages stored Liquid shortcode templates through
`ShortcodeTemplatesManager`, shared with the admin editor, recipes and rendering providers.
It contributes the `shortcode-templates` capability, `pomi shortcodes templates` commands and
eligible MCP tools. Disabling the feature removes this management surface.

This API manages database templates. Code-defined shortcode providers remain registered by
modules; a stored template can override a provider with the same name. Deleting that template
can expose the provider again. Existing content containing the shortcode is not rewritten.

## Authentication and permissions

Every operation requires bearer authentication with **Access remote management API**
(`AccessRemoteManagement`) and **Manage Shortcode Templates** (`ManageShortcodeTemplates`).
Application principals use the same permissions as administrators. Missing/invalid tokens return
`401`; authenticated principals without either permission receive `403`.

## Routes

Paths are relative to the tenant prefix. Names are passed in the `name` query parameter and
looked up case-insensitively. No operation renames a template through this API.

| Method | Path | Operation | Permission |
| --- | --- | --- | --- |
| GET | `api/shortcode-templates` | List stored templates | ManageShortcodeTemplates |
| GET | `api/shortcode-templates/by-name?name={name}` | Show a stored template | ManageShortcodeTemplates |
| POST | `api/shortcode-templates/validate` | Validate without saving or rendering | ManageShortcodeTemplates |
| POST | `api/shortcode-templates` | Create, with equivalent retries | ManageShortcodeTemplates |
| PUT | `api/shortcode-templates/by-name?name={name}` | Replace a complete definition | ManageShortcodeTemplates |
| DELETE | `api/shortcode-templates/by-name?name={name}` | Delete if present | ManageShortcodeTemplates |

All operations also require `AccessRemoteManagement`.

## Definition

Bodies and responses use camel-case JSON. Unknown members are rejected.

| Property | Type | Requirement and behavior |
| --- | --- | --- |
| `name` | string | Required shortcode identifier, without brackets or arguments; at most 256 characters. Validated by the shortcode parser and stored in invariant lowercase. |
| `content` | string | Required, nonempty Liquid template. Syntax is validated; the template is not executed by validation or save. |
| `hint` | string or null | Optional picker hint; defaults to null. |
| `usage` | string or null | Optional usage HTML; sanitized by the shared manager before storage. |
| `defaultValue` | string or null | Optional picker insertion text; defaults to null. |
| `categories` | array of strings or null | Ordered picker categories; missing or null becomes an empty array. |

```json
{
  "name": "callout",
  "content": "<aside>{{ Content }}</aside>",
  "hint": "Callout block",
  "usage": "[callout]Text[/callout]",
  "defaultValue": "[callout][/callout]",
  "categories": ["Content"]
}
```

## List

`GET api/shortcode-templates` accepts no body. Optional query parameters:

- `search`: case-insensitive substring search over name and hint; omitted/blank matches all.
- `skip`: integer offset, default `0`, minimum `0`.
- `take`: integer page size, default `50`, range `1`–`200`.

Results are ordered by case-insensitive name. `200` returns:

```json
{
  "skip": 0,
  "take": 50,
  "totalCount": 1,
  "items": [{"name":"callout","content":"<aside>{{ Content }}</aside>","hint":"Callout block","usage":"[callout]Text[/callout]","defaultValue":"[callout][/callout]","categories":["Content"]}]
}
```

Invalid paging returns `400`. The count is computed before paging.

## Show

`GET api/shortcode-templates/by-name?name=callout` requires the string `name` query parameter
and accepts no body. `200` returns the stored definition above, including its stored name and
sanitized usage HTML. An absent name returns `404`. A missing required query parameter returns `400`.

## Validate

`POST api/shortcode-templates/validate` accepts the complete definition above as JSON. It checks
name and Liquid syntax without changing a document, rendering a template or checking name
availability. Valid input returns `200` with `{"isValid":true,"errors":{}}`.

Invalid definitions also return `200`, for example:

```json
{"isValid":false,"errors":{"content":["The template content is mandatory."]}}
```

Malformed JSON or unknown members return `400`. Repeating validation has no persistent effect.

## Create

`POST api/shortcode-templates` accepts the complete definition above. `201` returns the stored
definition and a tenant-prefixed `Location` pointing to the show operation. Usage HTML is sanitized
and the name is normalized before storage. Invalid definitions return `400` validation errors.

An identical stable-name retry returns `201` and the existing definition without another document
mutation. Equivalence compares content, hint, sanitized usage, default value and ordered categories
using ordinal equality; missing/null categories normalize to an empty array. Name identity is
case-insensitive. A differing definition under an existing name returns `409` without overwriting it.

## Update

`PUT api/shortcode-templates/by-name?name=callout` requires the query name and a complete JSON
definition. The body name must match the query name exactly; disagreement returns `400`.
The stored name lookup remains case-insensitive.

A valid replacement returns `200` with the stored definition. Missing optional properties are
reset to their defaults, including clearing categories. This is a full replacement, not a patch.
Invalid definitions return `400`; an absent target returns `404`. An unchanged, normalized retry
returns the existing definition without another mutation. Changed definitions update the shared
cached document, so subsequent rendering uses the replacement.

To rename through this API, create the new name and delete the old name. These are separate
operations; an interruption can leave both names present. The existing admin editor continues
to support renaming through one shared document mutation and refuses an occupied destination.

## Delete

`DELETE api/shortcode-templates/by-name?name=callout` requires the query name and accepts no body.
It returns `204` whether the template was removed or was already absent. Repeated deletion does
not mutate an already-absent entry. Pomi requires confirmation (`--force` for automation).
Content that references the shortcode is retained. Inspect rendered pages after removing an override.

## Shared services and errors

`ShortcodeTemplatesManager` owns editor/API name and Liquid validation, save/rename collision
handling, usage HTML sanitization, persistence and cache invalidation. Admin presentation and API
status/retry responses remain in their callers. Recipe imports use the same sanitizer and
persistence path while preserving their existing ability to import definitions without editor
syntax validation.

Each save or admin rename updates one tenant document. The API provides no per-template ETag
or cross-request compare-and-swap guarantee; serialize competing writes and reread after conflicts.
Validation errors use property-path dictionaries, while other failures use Problem Details.
A disabled feature removes its routes, capability, CLI commands and MCP tools.

## Sources

- [Endpoint mappings and handlers](https://github.com/OrchardCMS/OrchardCore/blob/main/src/OrchardCore.Modules/OrchardCore.Shortcodes/Endpoints/Management/ShortcodeTemplateManagementEndpoints.cs)
- [Shared manager](https://github.com/OrchardCMS/OrchardCore/blob/main/src/OrchardCore.Modules/OrchardCore.Shortcodes/Services/ShortcodeTemplatesManager.cs)
- [Shortcodes module](../../modules/Shortcodes/README.md)
