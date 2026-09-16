# Layers API

The Layers API manages tenant-stored layer definitions through `ILayerService`. A layer decides
whether its widgets render on a page. This API manages definitions, rules and widget placement. Widget content
is a separate resource. The admin and frontend read the same `LayersDocument` and rule types.

## Availability and authorization

Enable `OrchardCore.Layers` and configure remote management authentication. Each operation requires
an authenticated bearer principal with both `AccessRemoteManagement` and `ManageLayers`.
Application principals and user principals use the same permissions. An admin session cookie
alone does not authorize these endpoints. Requests lacking authentication return 401; callers
without the required permissions receive 403 before a layer document is read or modified.

`OrchardCore.RemoteManagement.Cli` exposes the commands below. The MCP feature independently
exposes eligible tools such as `layers_list`, `layers_create` and `layers_update`; they invoke the
same endpoint handlers and permission checks in process. Disabling Layers removes its API,
capability, commands and tools from discovery. Refresh metadata after changing enabled features.

## Operations

Paths are relative to the tenant URL, including any tenant prefix.

| Method and path | Pomi command | Result |
| --- | --- | --- |
| `GET /api/layers` | `layers list` | Paged definitions with `skip`, `take`, `totalCount`, `items`. |
| `GET /api/layers/by-name?name={name}` | `layers show <name>` | Complete stored definition; 404 when absent. |
| `GET /api/layer-conditions` | `layers conditions` | Registered condition descriptors and property schemas. |
| `POST /api/layers/validate` | `layers validate` | `isValid` and path-keyed `errors`; never saves or evaluates conditions. |
| `POST /api/layers` | `layers create` | 201 with the saved definition and tenant-relative Location; 409 on a different existing definition. |
| `PUT /api/layers/by-name?name={name}` | `layers update <name>` | 200 with the replacement; 404 when absent. |
| `DELETE /api/layers/by-name?name={name}` | `layers delete <name> --force` | 204, including an absent layer; 409 while any latest or published widget references it. |

List accepts a case-insensitive `search` over names/descriptions, nonnegative `skip` (default 0),
and `take` from 1 to 200 (default 50). Results are sorted by name. Invalid paging returns 400.
Names match case-insensitively. A query parameter preserves names containing slashes, spaces or percent signs without route decoding ambiguities. Updates require the body name to match the name query parameter exactly and
preserve the stored name's spelling. Renaming is not supported because widgets reference names.

## Definitions and conditions

Create and update accept the complete definition as JSON:

```json
{
  "name": "News visitors",
  "description": "Public news widgets",
  "conditions": [
    {
      "name": "AllConditionGroup",
      "properties": { "displayText": "News page and anonymous visitor" },
      "conditions": [
        {
          "name": "UrlCondition",
          "properties": {
            "value": "/news",
            "operation": "StringStartsWithOperator",
            "caseSensitive": false
          }
        },
        { "name": "IsAnonymousCondition", "properties": {} }
      ]
    }
  ]
}
```

Read the live `layers conditions` result before authoring JSON. Each descriptor contains `name`,
`canWrite`, `supportsChildren` and a JSON Schema for `properties`. The built-in Boolean, Homepage,
authentication, URL, culture, role, content type, JavaScript, All and Any conditions are writable.
Registered extension conditions remain discoverable and readable, but are not writable through
this contract. Their module must supply a supported management implementation before writing them.

The rule requires every root condition to match. All/Any groups use the existing Rules service;
empty rules and empty groups do not match. Only group conditions accept children. Boolean
conditions require a JSON boolean `value`; string comparisons require string `value` and a
registered supported `operation`. `caseSensitive` defaults to false. JavaScript requires a
nonempty `script`; validation parses it without execution. Runtime errors, available scripting
methods, the current URL/culture/principal and content display context still affect evaluation.
Structural validity is not proof that a condition matches a specific page.

Names must contain 1–256 characters, without surrounding whitespace or control characters.
Definitions allow at most 256 conditions and 16 nested groups. The server generates omitted
`conditionId` values. Preserve returned IDs when editing/reordering conditions; supplied IDs
must be unique within the rule and use at most 128 ASCII letters, digits, hyphens or underscores.
Unknown condition names, properties, invalid values and duplicate IDs are rejected before saving.
Mutation validation failures return HTTP 400 with path-keyed validation errors. The validation
operation instead returns HTTP 200 with `isValid: false` and the same errors.

## Replacement, retry and verification

Updates replace the description and the entire conditions array. Omitted description becomes
null, and omitted conditions means an empty rule; read the existing definition before editing.
An identical create retry returns the existing definition, including its generated IDs. A
different definition under the same case-insensitive name returns 409. Identical update retries
preserve generated IDs and avoid another document update. Explicitly supplied IDs are part of
the requested identity and must match for a retry to be equivalent. Omitted optional properties
use the same defaults as their normalized readback.

Concurrent edits have last-writer semantics; there is no conditional ETag update contract.
After an uncertain response, read the layer before retrying. Delete refuses referenced layers
without moving, deleting or unpublishing any widgets. Remove or move those widgets explicitly
before deleting the definition.

```bash
pomi layers conditions --output json
pomi layers schema --operation create
pomi layers validate --body-file news-layer.json --output json
pomi layers create --body-file news-layer.json --output json
pomi layers show "News visitors" --output json
pomi layers update "News visitors" --body-file news-layer.json --output json
```

Verify the saved definition and the rendered pages for the intended URL, culture and visitor
identity. A successful save alone does not establish visibility. See the
[Layers module](../../modules/Layers/README.md) and [Rules module](../../modules/Rules/README.md).

## Shared module services

`ILayerService` provides name validation, lookup, creation, metadata/rule updates and guarded
deletion for both the API and the existing layer admin actions. Names must be nonblank, at most
256 characters and contain no surrounding whitespace or control characters. Metadata-only admin
edits preserve the existing rule and condition IDs. HTTP retry responses remain an API concern.

`IRuleManagementService` describes and builds condition trees and exposes non-executing condition
validation. The JavaScript editor uses the same required-value and syntax checks, then retains
its execution preview in the current user/request context. API validation does not run scripts.

## Widget placement

Enable `OrchardCore.Layers` to expose the `layer-widgets` capability. Create widget
content through the existing content API, then attach it to an existing layer
and configured zone with these operations. Placement updates do not create
content or publish drafts.

| Method | Route | Pomi command | Behavior |
| --- | --- | --- | --- |
| GET | `/api/layer-widgets` | `layers widgets list` | Lists authorized attached widgets, with version, layer, zone and paging filters. |
| GET | `/api/layer-widget-zones` | `layers widgets zones` | Returns configured tenant zone names. |
| GET | `/api/layer-widgets/{contentItemId}` | `layers widgets show <id>` | Shows placement for the latest or published version. |
| PUT | `/api/layer-widgets/{contentItemId}` | `layers widgets update <id>` | Attaches or replaces placement on the latest and published versions. |

All operations require bearer authentication, `AccessRemoteManagement` and
`ManageLayers`. Reading published widgets also requires resource `ViewContent`;
reading drafts requires resource `PreviewContent`. Lists remove unauthorized
resources before calculating totals or paging. Updates require `EditContent` on
**every** affected version and `PublishContent` on the published version because
the placement change affects public output immediately. All permissions and
validation are checked before either version is changed. The content API's
separate `AccessContentApi` permission is needed when using content commands to
create or edit widget bodies, not for these placement operations.

The list defaults to `version=latest`, `skip=0`, `take=50`. `version=published`
selects published content. `take` must be between 1 and 200. Optional `layer`
matching is case-insensitive; `zone` matching is exact. Results are ordered by
zone, numeric position and content item identity. The total counts only matching,
authorized placements. Unattached widgets do not appear. Show accepts the same
`version` choices and returns 404 for missing content or an unattached version.

PUT accepts a complete placement object:

```json
{
  "layer": "Always",
  "zone": "Content",
  "position": 2.5,
  "renderTitle": false
}
```

`layer`, `zone` and `position` are required. The content type of each affected
version must have the `Widget` stereotype. The layer must exist; matching is
case-insensitive and its canonical name is stored. The zone must appear in
`layers widgets zones` with its exact case. Configure zones in the Layers admin
settings; they are tenant settings, not a list of zones inferred from the active
theme. Confirm the theme renders the selected zone. `position` must be a finite
number; negative and fractional values are allowed. Smaller positions render
first within a zone. Use distinct positions when deterministic public ordering
is required. `renderTitle` defaults to false. Unknown request properties are
rejected, and invalid requests return 400 without modifying either version.

A successful PUT returns the normalized placement with HTTP 200. Repeating the
same placement is a successful no-op. If a published item has a newer draft,
both versions receive the placement fields, while their content bodies, version
identities and publication flags remain unchanged. An item with only a draft
stays unpublished. Placement does not create new versions or historical snapshots;
use the content version APIs for content lifecycle operations.

`ILayerWidgetService` shares validation with the existing widget editor and
persistence/cache invalidation with the Layers drag-and-drop action. The editor
retains its normal content save/publish lifecycle. Drag-and-drop changes only
position and zone on both versions, preserving each version's layer and title
setting, and applies the same resource permission checks as the API. Full API
updates apply all four placement fields. Recipe content imports retain their
existing semantics. Changes invalidate the tenant's widget cache after the
ambient session commits.

```bash
pomi layers widgets zones
pomi layers widgets list --version published --layer Always
pomi layers widgets show <widget-id> --version latest
pomi layers widgets schema --operation update
pomi layers widgets update <widget-id> --file widget-placement.json
```

With the MCP feature enabled, the equivalent tools are `layers_widgets_list`,
`layers_widgets_zones`, `layers_widgets_show` and `layers_widgets_update`. They
use the same endpoint handlers and authorization, including content permissions,
and remain available when the CLI feature is disabled. Disabling Layers removes
both its definition and widget placement operations from discovery.

Deleting a layer is refused while either a latest or published widget references
it, including when a draft has been assigned to a different layer.
