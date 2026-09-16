# Site settings management API (`OrchardCore.Settings`)

The site settings management API reads and updates the safe core properties of the current
tenant's site settings document. The **Settings** feature (`OrchardCore.Settings`) is always
enabled and advertises the Remote Management capability `settings`.

## Authentication and permissions

Every core endpoint uses Orchard Core's `Api` authentication scheme, requires
`AccessRemoteManagement`, requires `ManageSettings`, and disables antiforgery validation.
Neither the remote-management permission nor a valid bearer token grants settings access by
itself.

The API never returns or accepts the site salt, super-user identity, home route, document
identity, or arbitrary site settings properties.

## Base route

`/api/settings`

## Endpoints

| Method | Path | Description | Permission |
| --- | --- | --- | --- |
| `GET` | `/api/settings` | Get safe core site settings | `ManageSettings` |
| `PUT` | `/api/settings` | Merge safe core site settings | `ManageSettings` |
| `GET` | `/api/settings/schema` | Get the core update schema and explicitly contributed discovery schemas | `ManageSettings` |

## Site settings representation

Responses use lower-camel property names:

| Property | Type | Writable | Description |
| --- | --- | --- | --- |
| `siteName` | string or null | Yes | Site display name |
| `pageTitleFormat` | string or null | Yes | Liquid page-title format |
| `baseUrl` | string, empty string, or null | Yes | Fully qualified public base URL |
| `timeZoneId` | string or null | Yes | Site time-zone identifier |
| `pageSize` | integer | Yes | Default page size; minimum `1` |
| `maxPageSize` | integer | Yes | Maximum page size; `0` means unlimited |
| `maxPagedCount` | integer | Yes | Maximum paged item count; `0` means unlimited |
| `calendar` | string or null | Yes | Calendar identifier |
| `resourceDebugMode` | string | Yes | `FromConfiguration`, `Enabled`, or `Disabled` |
| `useCdn` | boolean | Yes | Whether resources use the configured CDN |
| `cdnBaseUrl` | string or null | Yes | CDN base URL |
| `appendVersion` | boolean | Yes | Whether resource URLs include a version query string |
| `cacheMode` | string | Yes | `FromConfiguration`, `Enabled`, `DebugEnabled`, or `Disabled` |

Unknown properties are rejected. In particular, `siteSalt`, `superUser`, `homeRoute`,
`properties`, and document identity or version fields are not part of this contract.

## Get site settings

`GET /api/settings`

This operation has no request body or query parameters.

```bash
pomi settings show
```

`200 OK` returns the current safe core values:

```json
{
  "siteName": "Acme",
  "pageTitleFormat": "{% page_title Site.SiteName, position: \"after\", separator: \" - \" %}",
  "baseUrl": "https://cms.example.com",
  "timeZoneId": "Etc/UTC",
  "pageSize": 10,
  "maxPageSize": 100,
  "maxPagedCount": 0,
  "calendar": null,
  "resourceDebugMode": "FromConfiguration",
  "useCdn": false,
  "cdnBaseUrl": null,
  "appendVersion": true,
  "cacheMode": "FromConfiguration"
}
```

## Update site settings

`PUT /api/settings`

The request body is required and must be a JSON object. Every property is optional. An omitted
property preserves its current value; an explicit `null` is applied only to nullable string
properties. Unknown properties and `null` for value types are rejected during JSON binding.

```bash
pomi settings update --json '{"siteName":"Acme","pageSize":25}'
```

Equivalent HTTP request:

```http
PUT /api/settings HTTP/1.1
Authorization: ******
Content-Type: application/json

{
  "siteName": "Acme",
  "pageSize": 25
}
```

The complete merged state is validated before the mutable site settings document is changed.
`pageSize` must be at least `1` and must not exceed a positive `maxPageSize`.
`maxPageSize` and `maxPagedCount` must be nonnegative. A nonempty `baseUrl` must be an absolute
URI. Enum values must use one of the names listed in the representation table; matching is
case-insensitive.

`200 OK` returns the complete safe representation after the merge. Repeating an identical
request also returns `200`; when the requested values already match, the document is not written
and no tenant reload is requested. A successful changed update writes the single site settings
document through `ISiteService`, so the operation has no partial per-property persistence.

Validation failures return `400 Bad Request` validation Problem Details and do not persist or
request a tenant reload:

```json
{
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "pageSize": [
      "The page size must be greater than zero."
    ]
  }
}
```

## Get the settings schema

`GET /api/settings/schema`

```bash
pomi settings schema
```

`200 OK` returns:

```json
{
  "core": {
    "$schema": "https://json-schema.org/draft/2020-12/schema",
    "title": "Site settings update",
    "type": "object",
    "additionalProperties": false,
    "properties": {}
  },
  "sections": []
}
```

`core` is the authoritative schema for `PUT /api/settings`. It includes descriptions, defaults,
enum values, URI format, and numeric constraints.

Enabled features can implement `ISiteSettingsManagementSchemaProvider` to contribute explicit
schemas to `sections`. Providers receive the authenticated principal and must return only schemas
that principal may discover. These entries are marked `readable: false` and `writable: false`
because the core endpoint does not expose their values or update them. The API never infers a
schema by reflecting over, enumerating, or serializing the site settings `Properties` bag.

## Errors

| Status | Meaning |
| --- | --- |
| `400 Bad Request` | Missing or malformed JSON, unknown property, invalid null, or validation failure |
| `401 Unauthorized` | Missing or invalid `Api` bearer credentials |
| `403 Forbidden` | Missing `AccessRemoteManagement` or `ManageSettings` |

## Endpoint coverage and sources

This page covers all three endpoints mapped by
`SiteSettingsManagementEndpoints`. The contract is implemented by
`SiteSettingsManagementService`, `SiteSettingsManagementModels`, and `SiteSettingsValidator`,
and is covered by `SiteSettingsManagementTests`.

## Typed module settings sections

The `settings-sections` capability exposes only sections explicitly registered by
enabled modules through `ISiteSettingsSectionProvider`. This is separate from the
core settings object and from Custom Settings content types. A discovery-only
`ISiteSettingsManagementSchemaProvider` contribution does not enable section
reads or writes.

| Method | Route | Pomi command |
| --- | --- | --- |
| GET | `/api/settings/sections` | `settings sections list` |
| GET | `/api/settings/sections/{name}` | `settings sections show <name>` |
| GET | `/api/settings/sections/{name}/schema` | `settings sections schema <name>` |
| PUT | `/api/settings/sections/{name}` | `settings sections update <name>` |

Section endpoints require API bearer authentication and `AccessRemoteManagement`.
Each provider declares its read and update permissions. Discovery filters out
sections whose read permission the caller lacks. Show and schema require read
permission; updates require both permissions because they return safe readback.
A general `ManageSettings` grant does not replace a module's declared permission.
A missing/disabled section returns 404; a known but unauthorized section returns
403. Names are matched case-insensitively and readback uses the canonical name.

Descriptors identify the name, owning feature, scope, HTTPS requirement and reload
behavior. Readback includes:

```json
{
  "name": "https",
  "source": "tenant",
  "isReadOnly": false,
  "readOnlyReason": null,
  "values": {
    "strictTransportSecurityMode": "Disabled",
    "requireHttps": false,
    "requireHttpsPermanent": false,
    "sslPort": null
  },
  "redactedProperties": [],
  "readOnlyProperties": []
}
```

`source` is explicitly reported as `tenant`, `configuration` or `mixed` by the
provider. An entirely read-only section, or a patch containing an individually
read-only property, returns 409 without writing. Read-only status describes
configuration ownership; authorization and transport requirements are additional
checks. Providers must omit secret values and identify their property names in
`redactedProperties`. An omitted secret does not mean that no secret is configured.
Readback is never a serialization of the whole site document.

PUT accepts the fields to change directly, without a `values` wrapper. Get the
section schema before preparing a payload: the generic operation schema cannot
know which enabled section will be selected. Omitted fields retain their values.
Supplied arrays replace the field's entire array. A JSON null clears or resets
only properties whose section schema explicitly allows null; it is not a generic
reset command. Unknown fields and invalid values return 400 without a partial
write. A successful update returns `section`, `changed`, `reloadRequested` and an
empty `errors` object. An equivalent retry returns `changed: false` and should
not request another reload. Requesting a reload does not restart the host process;
it rebuilds this tenant's pipeline after the current request completes.

```bash
pomi settings sections list
pomi settings sections show https
pomi settings sections schema https
pomi settings sections update https --file https-settings.json
```

Eligible MCP tools are `settings_sections_list`, `settings_sections_show`,
`settings_sections_schema` and `settings_sections_update`. They call the same
handlers and preserve module permissions, ownership checks and HTTPS requirements.
They remain available with the MCP feature enabled and the CLI feature disabled.

### HTTPS section

The `https` section is available with `OrchardCore.Https` and requires
`ManageHttps`. Its properties are tenant-owned. TLS listeners/certificates,
forwarded-header trust and host HSTS options remain host configuration.

| Property | Accepted values | Omitted / null behavior |
| --- | --- | --- |
| `strictTransportSecurityMode` | `Disabled`, `Enabled`, `FromConfiguration` | Omission preserves; null is invalid. |
| `requireHttps` | Boolean | Omission preserves; null is invalid. |
| `requireHttpsPermanent` | Boolean | Omission preserves; null is invalid. |
| `sslPort` | Integer 1–65535 or null | Omission preserves; null restores automatic port detection. |

`FromConfiguration` is the stored enum name for following the host environment:
HSTS is enabled in Production and disabled in other environments. It does not
read an arbitrary configuration section. HSTS middleware excludes its configured
local host names. A permanent HTTPS redirect uses 308; the temporary redirect
uses 307. `sslPort` selects a redirect destination, not a listening port.

As in the existing HTTPS editor, updates must arrive over HTTPS. An HTTP update
that reaches the API is refused with 403, including an empty patch. When HTTPS
redirection is enabled, HTTP requests may be redirected before reaching the API.
Use a working HTTPS management context before changing these settings. Behind a
TLS-terminating proxy, the host must already have correctly configured trusted
forwarded headers; the section does not infer security from an arbitrary header.

The editor and API use the same validation and field mutation code. Both reject
invalid ports and enum values, and only changed settings request a tenant reload.
The editor's normal Settings controller still owns saving its document; the
section provider saves through the same site service. Recipe imports retain their
existing behavior and are not routed through remote update validation.

### CORS section

The `cors` section is available with `OrchardCore.Cors` and requires `ManageCorsSettings`.
Its tenant-owned `policies` array contains named CORS policies. Omission preserves the array;
a supplied array replaces all policies and `[]` removes them. Null is invalid. Each policy
is a complete definition; omitted Boolean fields are false and omitted lists are empty.
The [CORS module reference](../../modules/Cors/README.md#remote-settings-section) documents
the fields, validation and full example. The section is not a projection of host-added
`CorsOptions` or arbitrary site properties.

The admin editor and section provider share `CorsService` validation and persistence.
Invalid names, duplicate policies, multiple defaults, invalid origins/header/method tokens
and credentialed wildcard origins are rejected without saving or reloading. A changed
save requests a tenant reload. Runtime options reuse validation to skip invalid imported
policies; the first valid policy is the default when none is explicitly selected. Verify
actual preflight and simple request headers after updating. CORS does not authenticate
callers or grant API permissions.

### Adding a section provider

Register a scoped `ISiteSettingsSectionProvider` in its owning feature. Give it a
unique stable name, explicit read/update permissions, an allowlisted JSON update
schema and safe readback. Duplicate names fail instead of silently selecting a
provider. Reuse the module's domain services and move matching admin/recipe logic
into shared code when extracting validation or mutations. Keep request binding
and presentation in the callers.

Providers must enforce their domain constraints and configuration ownership,
preserve omission/null semantics, reject unsupported fields, avoid secret values
in results and errors, and apply the required cache or shell lifecycle only after
a successful change. Return redacted readback after updates as well as reads.
Do not infer a settings contract from CLR properties or deserialize arbitrary
`ISite.Properties` entries. Establish adapter-specific tests for the existing
workflow and the API before adding a provider to discovery.

### Security header section

Enable `OrchardCore.Security` to manage `security-headers` with
`ManageSecurityHeadersSettings`. Its CSP, Permissions Policy and Referrer Policy
settings reuse the admin editor's validation. Supplied maps replace the full map;
omitted properties remain unchanged. Configuration-owned sections are read-only.
See the [security header contract and example](../../modules/Security/README.md#remote-management).

### Layer zones section

The `layer-zones` section is contributed by `OrchardCore.Layers`. All four section
operations require `AccessRemoteManagement` and `ManageLayers`. When Layers is
disabled the section disappears and named requests return 404.

Its only property is `zones`, a tenant-owned array of strings. An omitted property
preserves the list; a supplied array replaces the complete list; `[]` clears it.
Null, null array entries, non-string entries and unknown properties return 400
without saving. No arbitrary site properties are exposed.

```json
{"zones": ["Content", "Footer"]}
```

Names use the existing admin editor's normalization: each string is split on spaces
and commas and empty names are discarded. Order, case and duplicates are preserved.
For example, `["Content, Footer", "Content"]` stores three entries. An identical
normalized list is a successful 200 no-op (`changed: false`), including an empty
patch. A changed list is saved through `ISiteService`; no tenant reload is needed
(`reloadRequested: false`). These are complete-list updates without an ETag;
concurrent editors should read back the resulting list.

```bash
pomi settings sections schema layer-zones
pomi settings sections show layer-zones
pomi settings sections update layer-zones --body-file zones.json
pomi layers widgets zones
```

The admin zone editor and section provider share `LayerSettingsEditor`. Widget
placement reads the saved zones through its existing service. Changing the list
does not create theme sections, move widgets, delete widgets or change their
publication state. Preserve existing zones unless their removal is intended;
widgets already assigned to removed zones retain their placement metadata.

### Content culture picker section

The `content-culture-picker` section is available only when
`OrchardCore.ContentLocalization.ContentCulturePicker` is enabled. All four section
operations require `AccessRemoteManagement` and `ManageContentCulturePicker`.
Content localization permission alone does not grant settings administration.

| Property | Type / initial default | Behavior |
| --- | --- | --- |
| `setCookie` | Boolean / `true` | Write the culture cookie when the picker switches to a supported culture. |
| `redirectToHomepage` | Boolean / `false` | If the current item lacks a target-culture variant, try the localized homepage before returning to the current URL. |
| `setCookieOnContentRequest` | Boolean / `false` | Write the culture cookie when visiting localized content. Request-culture selection still occurs when this flag is false. |

```json
{"setCookie": true, "redirectToHomepage": true, "setCookieOnContentRequest": false}
```

All values are tenant-owned. Omitted properties retain their stored values. Null,
non-Boolean values and unknown properties return 400 without saving either settings
object. An identical retry or empty patch returns 200 with `changed: false`; changes
save both affected settings objects in the same site document with one update.
The existing runtime reads settings per request, so `reloadRequested` remains false.
Concurrent updates have no ETag precondition; read back the result after saving.

The admin picker and request-culture editors share `ContentCultureSettingsEditor`
with this section. `setCookieOnContentRequest` maps to
`ContentRequestCultureProviderSettings.SetCookie`; the other two properties map to
`ContentCulturePickerSettings`. Existing recipe imports keep their own contract.
Cookie lifetime (`CulturePickerOptions.CookieLifeTime`), supported cultures and
localized content are separate configuration/resources and are not exposed here.
Disabling a cookie-writing flag does not delete a cookie already held by a visitor.

```bash
pomi settings sections schema content-culture-picker
pomi settings sections show content-culture-picker
pomi settings sections update content-culture-picker --body-file picker.json
```

Verify language-switch redirects and response cookies after updating. Disabling the
picker feature removes this section (named requests return 404) while the separate
content localization feature can remain enabled.
