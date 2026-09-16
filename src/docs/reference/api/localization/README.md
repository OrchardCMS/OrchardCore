# Localization API

Manage tenant cultures, inspect translated JavaScript UI strings, and edit database-backed translations. `OrchardCore.Localization` contributes culture settings and UI strings (capability `localization`). `OrchardCore.DataLocalization` additionally contributes dynamic translations (capability `localization-translations`). Enable [Remote Management](../../modules/RemoteManagement/README.md) to use these authenticated APIs. Only culture and settings operations are projected into `pomi localization`; string inspection and translation editing remain HTTP/OpenAPI operations.

These APIs serve different kinds of localization:

- **Culture settings** select the tenant's default and supported cultures.
- **UI strings** read registered `IJSLocalizer` groups using installed PO catalogs, such as `media-gallery`. Adding a culture does not install or update its translations.
- **Dynamic translations** edit the same database document as the Data Localization admin UI, for registered descriptors such as content type display names, field labels, and permission descriptions. They do not modify PO files or translated content items.

See [Localization](../../modules/Localize/README.md), [Data Localization](../../modules/DataLocalization/README.md), and [Content Localization](../../modules/ContentLocalization/README.md) for those features.

## Authentication and permissions

Send a bearer token as described in [Authentication](../authentication/README.md). Every endpoint requires `AccessRemoteManagement`, plus the permission below. CLI discovery also needs access to the tenant's OpenAPI document (`ViewOpenApiContent` when document access is protected).

| Method | Tenant-relative route | Operation | Additional permission |
| --- | --- | --- | --- |
| GET | `/api/localization/cultures` | List supported or available cultures | `ManageCultures` |
| GET | `/api/localization/cultures/available` | List available cultures | `ManageCultures` |
| PUT | `/api/localization/cultures/{culture}` | Add one supported culture | `ManageCultures` |
| DELETE | `/api/localization/cultures/{culture}` | Remove one supported culture | `ManageCultures` |
| GET | `/api/localization/settings` | Read culture settings | `ManageCultures` |
| PUT | `/api/localization/settings` | Replace culture settings | `ManageCultures` |
| GET | `/api/localization/strings` | List advertised UI string groups | `ManageCultures` |
| GET | `/api/localization/strings/{groupName}` | Read a UI string group | `ManageCultures` |
| GET | `/api/localization/messages` | List standard PO catalog entries by context/key/prefix | `ManageCultures` |
| POST | `/api/localization/messages/resolve` | Resolve a standard localized message, optionally pluralized and formatted | `ManageCultures` |
| GET | `/api/localization/translations` | List dynamic translation descriptors and stored values | `ViewDynamicTranslations` |
| PUT | `/api/localization/translations` | Set one dynamic translation | `ManageTranslations` or `ManageTranslations_{culture}` |
| DELETE | `/api/localization/translations` | Remove one dynamic translation | `ManageTranslations` or `ManageTranslations_{culture}` |

For example, `ManageTranslations_fr` allows editing French translations, but does not allow changing tenant settings or editing English. Grant `ViewDynamicTranslations` separately when that identity should also list translations. `ManageTranslations` implies the read permission.

## Get started with the CLI

With an authenticated context pointing at the intended tenant:

```bash
pomi features enable OrchardCore.Localization
pomi api refresh
pomi localization --help
pomi localization cultures list
pomi localization settings show
```

For everyday changes, add or remove a single culture without replacing the other settings:

```bash
pomi localization cultures available
pomi localization cultures add fr
pomi localization cultures remove de --force
```

`cultures list` shows enabled cultures with readable names and default flags.
`cultures available` lists all cultures recognized by the server, with supported
and default flags. Both lists support `--skip` and `--take` (up to 200 rows).
`settings show` returns the whole configuration, including parent-culture
fallback. Use `settings update` when changing the default culture or replacing
the full configuration. All these commands live under `pomi localization`.

To set English as the default, support English and French, and enable parent
fallback, create `cultures.json`:

```json
{
  "defaultCulture": "en",
  "supportedCultures": ["en", "fr"],
  "fallBackToParentCulture": true
}
```

Apply it:

```bash
pomi localization settings update --body-file cultures.json
```

This replaces the complete supported-culture list. Read current settings first
and retain any other cultures you need. JSON bodies can also be supplied with
`--stdin`; use `--output json` for scripts.

`pomi localization` exposes culture/settings management and standard PO message
listing and resolution. Generic JavaScript groups and database translations remain
HTTP-only. Use the
[Data Localization admin UI](../../modules/DataLocalization/README.md) or the
HTTP APIs below for database-backed translations. Their permissions and behavior
are independent of CLI command discovery.

Media retains its dedicated UI-label command:

```bash
pomi media localizations show
```

It returns the Media gallery's resolved labels in the server-selected request
culture; it does not enumerate module PO catalogs or edit translations. See the
[Media localization endpoint](../media/README.md) for HTTP culture selection.

## List cultures

`GET /api/localization/cultures` accepts no body.

| Query parameter | Type | Default | Meaning |
| --- | --- | --- | --- |
| `includeAvailable` | Boolean | `false` | Include all cultures and aliases recognized by the server's .NET globalization data |
| `skip` | Integer | `0` | Nonnegative number of rows to skip |
| `take` | Integer | `50` | Page size, 1–200 |

The `200 OK` response is sorted by culture name and contains `skip`, `take`, `totalCount`, and `items`:

```json
{
  "skip": 0,
  "take": 50,
  "totalCount": 2,
  "items": [
    { "name": "en", "displayName": "English", "isSupported": true, "isDefault": true },
    { "name": "fr", "displayName": "French", "isSupported": true, "isDefault": false }
  ]
}
```

Display names depend on the server's globalization data and request language. Culture names are .NET culture identifiers such as `en`, `en-US`, `fr`, or `fr-FR`; they are not time zone identifiers. Discover exact available names with `pomi localization cultures available`, paging as necessary. Invalid paging returns `400`.

## List available cultures

`GET /api/localization/cultures/available` accepts no body. It has the same
`skip` and `take` parameters, response shape, and validation as culture listing,
but always includes every culture recognized by the server. For example:

```bash
pomi localization cultures available --take 200
pomi localization cultures available --skip 200 --take 200
```

The `totalCount` reports the full number of available cultures. A supported
culture remains in this list with `isSupported: true`. The existing
`cultures list --include-available true` option remains an equivalent way to
request this information.

## Add or remove one culture

`PUT /api/localization/cultures/{culture}` adds one culture;
`DELETE /api/localization/cultures/{culture}` removes one. Neither accepts a body.
`culture` is an available .NET culture name, matched case-insensitively and
normalized to server casing. Use settings replacement for the empty invariant
culture name, which cannot be a route segment.

Both return `200 OK` with the complete updated settings. Other cultures, the
default, and parent fallback are preserved. Removing a culture does not delete
its PO files, database translations, or localized content. Adding one does not
install translation catalogs.

Adding an already supported culture or removing an already absent available
culture returns the current settings without saving or releasing the tenant.
Unknown names and removal of the default return `400` validation details.
Change the default with `settings update` before removing it. CLI removal asks
for confirmation; use `--force` for an unattended operation.

Changes use the same site settings service and tenant release as full settings
replacement below. They have no API-level concurrency token; coordinate
simultaneous culture edits and read back after an ambiguous failure.

## Read or replace culture settings

`GET /api/localization/settings` accepts no body and returns `200 OK` with the `cultures.json` shape above.

`PUT /api/localization/settings` requires an `application/json` body:

| Property | Type | Required | Meaning |
| --- | --- | --- | --- |
| `defaultCulture` | String | Yes | Available culture also present in `supportedCultures` |
| `supportedCultures` | String array | Yes | Complete list; 1–1000 entries, each available on the server |
| `fallBackToParentCulture` | Boolean | No | Enable supported-parent fallback; omitted means `false` |

The empty string denotes .NET's invariant culture. Names are matched case-insensitively, normalized to server casing, deduplicated, and sorted. Invalid names, an empty list, or a default outside the list return `400` validation details. A successful update returns `200` with the normalized settings.

Changed settings are saved through the site settings service and request a tenant release so request localization is reconfigured. An identical retry returns `200` without saving or releasing again: equivalence compares default culture case-insensitively, the supported-culture **set** case-insensitively, and the fallback Boolean. This is full replacement, not a patch. There is no ETag or compare-and-swap contract; coordinate concurrent settings edits and read back after an ambiguous failure.

## List UI string groups

`GET /api/localization/strings` accepts no body. `skip` defaults to `0` and
`take` to `50`, with the same bounds as culture listing. Invalid paging returns
`400`. For example, `GET /api/localization/strings` returns:

```json
{
  "skip": 0,
  "take": 50,
  "totalCount": 1,
  "items": [{ "name": "media-gallery" }]
}
```

Names come from enabled `IJSLocalizer` providers' `GetLocalizationGroups()`
method. Exact duplicates are merged and names are sorted ordinally. Group names
are case-sensitive unless the owning provider explicitly handles them otherwise.
The list is independent of the selected culture and does not resolve translations.
Use a returned name with `GET /api/localization/strings/{groupName}`. This endpoint is exposed through HTTP/OpenAPI only.

For compatibility, older providers can still serve strings by name but do not
appear until they implement group discovery. Third-party modules can advertise
their own groups without changing the CLI; see
[JavaScript localization](../../modules/Localize/javascript-localization.md#advertise-groups-for-http-discovery).
The built-in Media provider advertises `media-gallery`.

## Read UI strings

`GET /api/localization/strings/{groupName}` accepts no body.

- `groupName`: required path string, at most 200 characters; an exact registered JavaScript localization group, such as `media-gallery`.
- `culture`: optional query string; a supported culture name, matched case-insensitively. Defaults to the tenant's default culture.
- `skip` and `take`: as above.

Example `200 OK` response (values depend on installed catalogs):

```json
{
  "group": "media-gallery",
  "culture": "fr",
  "skip": 0,
  "take": 50,
  "totalCount": 1,
  "items": [{ "key": "Delete", "value": "Supprimer" }]
}
```

Providers are merged using the existing JavaScript localization service and results are sorted by key. `400` indicates an unsupported culture or invalid paging/group length; `404` means no strings were supplied for that group. Use `GET /api/localization/strings` to discover advertised groups.

Source text can remain visible when a PO entry is missing or its context does not match the current localizer. Selecting French cannot repair a stale translation catalog. This response reports resolved strings, not translation coverage or provenance. It does not expose an API to upload PO files.

## Standard localization strings (PO catalogs)

These operations read the tenant's standard localization catalogs. They do not
use `ILocalizationDataProvider` and do not modify PO files. Both require remote
management access and `ManageCultures` and contribute Pomi commands (and MCP tools
when enabled). Refresh the tenant's CLI metadata after upgrading.

### List catalog entries

`GET /api/localization/messages?culture=fr&context=Shop&prefix=Cart.` lists loaded
entries for one culture. Without `culture`, Orchard's resolved request UI culture
is used. An explicit culture must be supported, or select a supported parent when
the tenant enables parent-culture fallback. Optional `context` and `key` match exactly; `prefix` matches the start
of the source key (`msgid`). All filters are ordinal and case-sensitive, combine
with AND, and are limited to 1000 characters. Omit context to include all contexts;
use an empty context to select entries without one. `skip` defaults to 0 and
`take` to 50 (maximum 200).

```bash
pomi localization messages list --culture fr --context Shop --prefix Cart. --take 100
pomi localization messages list --culture fr --key "Delete"
```

The response has `culture`, `skip`, `take`, `totalCount`, and `items`. Each item
contains `context`, `key`, and a `translations` array in the catalog's plural-form
order. Results are ordered by context, then key. Empty matches return an empty
page. This is the loaded catalog for the selected culture: parent catalogs are not
merged, and there is no source-code scan or enumeration of untranslated source strings. It does
not return PO file paths, comments, or the source `msgid_plural`, which is not
retained in Orchard's runtime dictionary.

### Resolve and format a message

`POST /api/localization/messages/resolve` is read-only. Send JSON:

```json
{
  "culture": "fr",
  "context": "Shop",
  "key": "{0} book for {1}",
  "plural": "{0} books for {1}",
  "count": 2,
  "arguments": ["Alice"]
}
```

```bash
pomi localization messages resolve --body-file message.json
```

`key` is required. Omit `culture` to use Orchard's current request UI and
formatting cultures. `context` is the
localizer base name/PO context; omit it for contextless lookup. `count` must be a
nonnegative integer and must be supplied together with the source `plural`
fallback. Keys, contexts, and plural fallbacks are limited to 1000 characters.

Resolution uses Orchard's string localizer: context fallback, configured parent
culture fallback, and the catalog's plural rule are applied server-side. Missing
translations use the supplied source text/forms. In plural mode, count occupies
`{0}` and additional arguments begin at `{1}`. Otherwise arguments begin at `{0}`.
Up to 16 JSON scalar arguments are accepted (strings, decimal numbers, booleans,
or null), each limited to 1000 characters. Numeric formatting uses the request formatting culture, or the explicit
culture selected for this lookup. With no arguments and no count, placeholders remain unchanged.

The response returns `culture`, `formattingCulture`, `context`, `key`, `count`, `template` (the selected,
unformatted translation), and `value` (the formatted result). For the example,
a French catalog might return `template: "{0} livres pour {1}"` and
`value: "2 livres pour Alice"`. `culture` identifies the selected UI culture and `formattingCulture` identifies
the culture used for numbers. Neither claims to identify the catalog where a
fallback translation was found.

Culture selection follows Orchard's configured request providers when omitted
(including applicable user/cookie settings, `Accept-Language`, and tenant defaults).
The API does not parse `Accept-Language` separately or override provider priority.
For example, send `Accept-Language: fr-FR` and omit the body's culture to let
Orchard negotiate the request. Explicit `culture: "fr-FR"` selects supported `fr`
if `fr-FR` is unsupported and parent fallback is enabled; otherwise it returns
`400`. If `fr-FR` is itself supported, resolution retains it and may find a missing
translation in the parent `fr` catalog according to the tenant's fallback setting.
Explicit culture names are limited to 100 characters.

Prefer `value` for JavaScript display: it keeps plural rules and culture formatting
consistent with server-rendered Orchard UI. The catalog listing and `template`
remain available for inspection or clients with a compatible formatter. PO plural
rules and .NET composite format strings are not interchangeable with JavaScript
`Intl` formatting or template literals. Treat returned strings as text; they are
not HTML-sanitized.

Unsupported cultures, invalid arguments/formats, templates longer than 10000
characters, and numeric components inside format items above 1000 return `400`.
These bounds prevent excessive formatting allocations. Both endpoints return
`401`/`403` under the usual management authorization rules.

## List dynamic translations

`GET /api/localization/translations` accepts no body.

| Query parameter | Type | Required/default | Meaning |
| --- | --- | --- | --- |
| `culture` | String | Required | Supported non-invariant culture, matched case-insensitively |
| `search` | String | Optional | At most 1000 characters; case-insensitive substring of context, key, or provider source value |
| `skip` | Integer | `0` | Nonnegative offset |
| `take` | Integer | `50` | Page size, 1–200 |

Returns `200 OK`, sorted by context then key:

```json
{
  "culture": "fr",
  "skip": 0,
  "take": 50,
  "totalCount": 1,
  "items": [
    { "context": "Content Types", "key": "Page", "sourceValue": "", "value": "Page française", "isTranslated": true }
  ]
}
```

Descriptors come from the enabled features' `ILocalizationDataProvider` implementations. `sourceValue` is the provider's value and may be empty; the source text may instead be the key. `value` is the exact stored translation for this culture, or `null` if none exists. `isTranslated` indicates an entry in storage. This list does not resolve parent-culture fallback. Orphaned translations whose providers no longer register a key are omitted. Invalid culture or query bounds return `400`.

## Set a dynamic translation

`PUT /api/localization/translations` requires `application/json`. For example,
if the tenant registers the `Content Types` / `Page` descriptor:

```json
{
  "culture": "fr",
  "context": "Content Types",
  "key": "Page",
  "value": "Page française"
}
```

All four strings are required:

- `culture`: a supported non-invariant culture, normalized to the configured spelling.
- `context` and `key`: nonblank, each at most 1000 characters, matching a registered descriptor **exactly, case-sensitively**.
- `value`: nonblank, at most 100000 characters. Use deletion to remove a translation.

Returns `200 OK`:

```json
{ "culture": "fr", "context": "Content Types", "key": "Page", "value": "Page française", "changed": true }
```

Invalid input returns `400` validation details. An unregistered context/key returns `404`. The endpoint updates only that pair and preserves every other entry and culture. An identical retry returns `200` with `changed: false` and performs no save. Equivalence uses normalized culture and exact context, key, and value. A different value replaces the previous one; it does not return a conflict.

Writes use the existing translations document manager and invalidate the same culture cache as the admin UI. They inherit the configured document storage and cache behavior. There is no file-system write and no new storage backend. There is no API-level concurrency token: coordinate concurrent editors and verify the result after a failed or ambiguous request.

## Delete a dynamic translation

`DELETE /api/localization/translations?culture=fr&context=Content%20Types&key=Page` accepts no body. All three query strings are required and have the same culture/context/key validation as set. Unlike set, deletion does not require the descriptor still to be registered, allowing cleanup of a known orphaned entry.

Returns `200 OK`:

```json
{ "culture": "fr", "context": "Content Types", "key": "Page", "value": null, "changed": true }
```

Only the exact pair is removed; other translations remain. Repeating deletion, including deletion of an absent pair, succeeds with `changed: false` and no save. Invalid input returns `400`. Cache invalidation and concurrency behavior are the same as set.

## Errors and source

All thirteen operations return `401` for missing/invalid bearer authentication and `403` for insufficient permissions. Operation-specific errors are described above. Validation responses contain an `errors` dictionary; other errors use Problem Details. Malformed JSON returns `400`, and unsupported request content types return `415` on body operations. Routes are relative to the tenant, including its path prefix.

Source: `OrchardCore.Localization/Endpoints/LocalizationManagementEndpoints.cs`, `OrchardCore.Localization/Endpoints/LocalizationMessageEndpoints.cs`, `OrchardCore.DataLocalization/Endpoints/TranslationManagementEndpoints.cs`, and the existing localization/site settings and translations document services. Each enabled module contributes its capability to the management manifest. The six culture/settings operations and two standard message operations carry CLI metadata in the OpenAPI document.
