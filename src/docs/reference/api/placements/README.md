# Placements API

`OrchardCore.Placements` manages tenant-owned shape placement overrides through the existing
`PlacementsManager` and selected `IPlacementStore`. It contributes the `placements` capability,
`pomi placements` commands and eligible MCP tools. Theme/module `placement.json` files are separate
sources and are not edited by this API. Tenant overrides take precedence over those files.

## Authentication and permissions

All operations require bearer authentication, **Access remote management API**
(`AccessRemoteManagement`) and **Manage placements** (`ManagePlacements`). Application principals
and human users use the same permissions. Missing/invalid tokens return `401`; authenticated
principals without either required permission receive `403`.

## Routes

Paths are relative to the tenant prefix. Named operations require the string `shapeType` query
parameter, with a case-insensitive lookup. This avoids interpreting a shape key as a URL path.

| Method | Path | Purpose | Permission |
| --- | --- | --- | --- |
| GET | `api/placements` | List stored overrides | ManagePlacements |
| GET | `api/placements/by-shape?shapeType={shapeType}` | Show a complete override | ManagePlacements |
| GET | `api/placement-filters` | List registered filter keys | ManagePlacements |
| POST | `api/placements/validate` | Validate without rendering or persistence | ManagePlacements |
| POST | `api/placements` | Create a nonempty override | ManagePlacements |
| PUT | `api/placements/by-shape?shapeType={shapeType}` | Replace rules or remove an override with an empty array | ManagePlacements |
| DELETE | `api/placements/by-shape?shapeType={shapeType}` | Remove an override if present | ManagePlacements |

Every route also requires `AccessRemoteManagement`.

## Definition and rule semantics

The root JSON definition has two required properties: `shapeType`, a nonblank key of at most 256
characters without surrounding whitespace/control characters, and `nodes`, an ordered array of
placement rules. Unknown root members are rejected. The shape key is preserved as first stored;
shape types need not already occur in a rendered page.

```json
{
  "shapeType": "HtmlBodyPart",
  "nodes": [
    {"place":"Content:1","displayType":"Detail","contentType":["Article"]},
    {"place":"-","displayType":"Detail","contentType":"Article","path":"~/private*"}
  ]
}
```

Each rule uses the existing placement format:

| Property | Type | Meaning |
| --- | --- | --- |
| `place` | string or null | Location such as `Content:1`; `-` hides the shape. Layout zones, grouping and positioning follow the existing placement grammar. |
| `shape` | string or null | Replacement shape type. |
| `alternates` | string array or null | Alternate shape names appended by a matching rule. Each name must be nonblank. |
| `wrappers` | string array or null | Wrapper names appended by a matching rule. Each name must be nonblank. |
| `displayType` | string or null | Optional exact display-type match, commonly `Detail`, `Summary` or `Edit`. |
| `differentiator` | string or null | Optional exact differentiator match. |
| Registered filter keys | provider-defined JSON | Additional rule properties interpreted by enabled `IPlacementNodeFilterProvider` implementations. |

A non-null rule must supply at least one nonempty `place`, `shape`, `alternates` or `wrappers`.
Rules containing only filters are invalid. Unknown/unregistered filter keys are rejected by the
shared admin/API validator, preventing misspellings from silently broadening a match.

The built-in `path`, `contentType` and `contentPart` filters accept a non-null string or an array
of non-null strings. Arrays match any supplied value; an empty array matches nothing. `path`
normalizes application-relative paths and supports a trailing `*` prefix match. `contentType`
can match a content type or stereotype and supports trailing `*`; `contentPart` checks an attached
part. Other registered filters retain their provider-defined expression semantics.

Rules are processed in array order. Each matching rule replaces any location/shape type it
supplies and appends its alternates/wrappers. The last matching location wins; the first rule is
not an early exit. Display type and differentiator comparisons are ordinal. Read the existing
[placement format](../../modules/Placement/README.md#format) for exact shape and differentiator names.

## List

`GET api/placements` accepts no body. Optional query parameters are `search` (case-insensitive
shape-type substring; blank matches all), `skip` (integer, default `0`, minimum `0`) and `take`
(integer, default `50`, range `1`–`200`). Invalid paging returns `400`.

`200` returns `{"skip":0,"take":50,"totalCount":1,"items":[...]}` with complete definitions in
`items`, sorted by case-insensitive shape type. `totalCount` is computed before paging.

## Show

`GET api/placements/by-shape?shapeType=HtmlBodyPart` accepts no body. `200` returns the stored
complete definition above, including the original key and ordered nodes. An absent override
returns `404`; a missing required query parameter returns `400`. This response does not include
inherited rules from theme/module files or compute effective placement for a particular request.

## Filter discovery

`GET api/placement-filters` accepts no parameters or body and returns `200` with sorted unique
keys, for example `["contentPart","contentType","path"]`. The list depends on enabled features.
It identifies available providers; it does not execute them or infer schemas for extension filters.

## Validate

`POST api/placements/validate` accepts the complete definition. `200` returns
`{"isValid":true,"errors":{}}` or errors such as:

```json
{"isValid":false,"errors":{"nodes[0]":["A valid placement must contain place, shape, wrappers or alternates."]}}
```

Validation checks naming, structural requirements, registered keys and built-in filter value types.
It does not render shapes, run filters, check whether a shape exists or check name availability.
An empty nodes array is valid for removing an override on update. A missing/null array is invalid.
Malformed JSON or incompatible property types return `400` before validation. Repeating validation
has no persistent effect.

## Create

`POST api/placements` accepts a definition with at least one rule. `201` returns the stored
complete definition and a tenant-prefixed `Location` for the show operation. An empty array or
invalid definition returns `400` validation errors.

The shape type is the stable identity. An equivalent retry returns `201` with the stored definition
without another mutation. Equivalence compares the complete serialized rule arrays, including
ordered rules, alternates/wrappers and filter values; JSON object property order is insignificant.
Omitted optional rule members deserialize to their defaults. Explicit empty arrays and null arrays
remain distinct. A differing existing definition returns `409` without replacing it.

## Update

`PUT api/placements/by-shape?shapeType=HtmlBodyPart` accepts the complete replacement. The body
`shapeType` must match the query value exactly; disagreement returns `400`. Lookup against the
stored key remains case-insensitive. Renaming is not supported by this operation.

A nonempty replacement returns `200` with the stored definition. Unchanged retries avoid another
mutation. Omitted rule members and absent rules are removed by replacement; this is not a patch.
Invalid definitions return `400`; a nonempty update to an absent shape type returns `404`.

An empty `nodes` array removes the override and returns `204`, including on retries after it is
already absent. This matches the existing admin editor's empty-array deletion behavior. Theme or
module placement rules can become effective again after an override is removed.

## Delete

`DELETE api/placements/by-shape?shapeType=HtmlBodyPart` accepts no body. It returns `204` whether
an override was removed or was already absent. Repeating deletion does not mutate an absent
entry. Pomi requires confirmation (`--force` for automation). Content, shapes and module/theme
files are retained; this removes only the selected tenant's stored override.

## Shared services and storage ownership

The API and existing admin editor use the same manager for structural/filter validation,
creation collisions, ordered-rule persistence and empty-array deletion. The editor continues to
trim its shape-name input and displays validation errors with their rule paths. Recipe and
programmatic imports retain their existing direct upsert semantics; they do not acquire editor
validation as a side effect of this API.

Database storage is the default. Enabling `OrchardCore.Placements.FileStorage` selects the existing
tenant file-document store for the same API and editor. Switching providers does not migrate or
merge documents: each provider retains its own definitions, and switching back exposes its prior
state. The API accepts no filenames or filesystem paths and cannot edit arbitrary files.

A mutation saves one selected-store document and invalidates its shared cache. There is no
per-shape ETag or cross-request compare-and-swap guarantee; coordinate concurrent writers and
reread after failures. Other failures use Problem Details. Disabling Placements removes its
routes, capability, CLI commands and MCP tools.

## Sources

- [Endpoint mappings and handlers](https://github.com/OrchardCMS/OrchardCore/blob/main/src/OrchardCore.Modules/OrchardCore.Placements/Endpoints/Management/PlacementManagementEndpoints.cs)
- [Shared manager](https://github.com/OrchardCMS/OrchardCore/blob/main/src/OrchardCore.Modules/OrchardCore.Placements/Services/PlacementsManager.cs)
- [Placements module](../../modules/Placements/README.md)
