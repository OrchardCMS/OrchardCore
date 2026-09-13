# Content localizations API

The Content Localization feature lists localized variants and creates a localized draft through
`IContentLocalizationManager`. The API and existing admin localization action use
`IContentLocalizationService` for the source/type permission checks, configured culture validation
and existing-variant handling. Cloning and localization handlers remain in the existing manager.

## Availability and permissions

Enable `OrchardCore.ContentLocalization` and configure remote management authentication. Each
request requires an API bearer principal with `AccessRemoteManagement`; an admin cookie alone is
insufficient. Pomi discovers `content localizations` through `OrchardCore.RemoteManagement.Cli`.
MCP independently exposes `content_localizations_list` and `content_localizations_create` and
runs the same endpoint handlers and permission checks in process. Disabling Content Localization
removes its API, capability, commands and tools. Refresh the catalog after feature changes.

Listing requires `ViewContent` for a published source or `PreviewContent` for a draft source.
Each returned variant is independently checked with the same version-specific permission.
Inaccessible variants are omitted, including their IDs and display text. Content type and
ownership variations of these permissions apply normally.

Creation requires `LocalizeContent` (including its ownership variation) on the latest source and
`EditContent` on its content type, evaluated for the caller as owner of a new item. Reusing an
existing variant additionally requires `EditContent` on its latest version. These checks are
shared with the admin action. A caller need not have publication permission to create a draft.
Configured cultures constrain targets; this feature has no separate per-culture content grant.
The UI-string translation permissions belong to a different feature and do not authorize
content localization.

## Operations

Paths are relative to the tenant URL, including its tenant prefix.

| Method and path | Pomi command | Result |
| --- | --- | --- |
| `GET /api/content/{contentItemId}/localizations` | `content localizations list <contentItemId> [--version latest\|published]` | Authorized variant summaries, ordered by culture and item ID. |
| `POST /api/content/{contentItemId}/localizations` | `content localizations create <contentItemId>` | 200 with `item` and `created`; a new variant is an unpublished draft. |

List defaults to `latest`. `published` selects only published versions, including when a newer
draft exists. There is one result per content item; published and latest versions are not
returned twice. A latest draft that has moved to another localization set is excluded from the
old set. A missing source or a source without `LocalizationPart` returns 404. A known source
without the required permission returns 403. No list operation modifies content.

Each summary includes `contentItemId`, `contentItemVersionId`, `contentType`, `displayText`,
`localizationSet`, `culture`, `published` and `latest`. Full content is available through the
[content items API](../content-items/README.md) using its own permissions, including
`AccessContentApi`. A minimal localizer grant alone does not authorize those separate endpoints.

## Create or reuse a variant

Send a culture that is configured on the tenant:

```json
{
  "culture": "fr"
}
```

```bash
pomi localization cultures list
pomi content localizations list <sourceId>
pomi content localizations create <sourceId> --body-file culture.json
pomi content localizations list <sourceId>
```

Culture matching is case-insensitive and new variants use the configured spelling. An empty
string selects the invariant culture only when it is configured. A missing/null or unsupported
culture returns 400. Unknown JSON properties are rejected. The request accepts no replacement
content, localization set ID or publication flag.

The manager clones the latest source, preserves its localization set and runs the registered
content/localization handlers. It does not publish the new item or change an existing variant's
body. The response reports `created: true` for a new draft. A sequential retry for an existing
editable variant returns its latest version with `created: false`; it does not create another
item/version. If a published matching variant has a latest draft with a different culture or set,
the API returns 409 so that the draft can be resolved first. This does not provide a concurrency
lock across simultaneous localization requests; coordinate parallel writers to the same set/culture.

Inspect, edit and publish the returned item explicitly through the content APIs. Existing
localization handlers can change part values; for example, Autoroute clears the cloned path
so it can be regenerated or explicitly assigned for the translated item. The admin action retains
its notifications and redirect flow while using the same localization service. Culture-picker
and request-culture settings are separate configuration workflows. UI-string translation APIs
retain their existing API-only policy.
