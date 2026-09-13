---
name: orchardcore-cli-content-items
description: Authors and manages Orchard Core content items through `pomi`. Use for schema-driven JSON creation, drafts, updates, validation, publishing, unpublishing, rendering, deletion, version history, version restoration, content localization, ownership, and organizing tenant content by type, route, alias, taxonomy, and containment.
---

# Pomi CLI Content Items

Before issuing commands, read the [shared context, authentication, and output rules](../orchardcore-cli/references/shared-rules.md).
For first-time access or login problems, follow [authentication and contexts](../orchardcore-cli/references/authentication.md).
These rules apply even when this specialist is selected directly.

Author against the live content-type schema. Content item JSON uses Pascal-cased
well-known properties and type-specific part/field members.

For page content, expect `FlowPart.Widgets` to hold the ordered section widgets.
Collection-section widgets should hold inner components in semantic named Bags
such as `Features.ContentItems` or `Slides.ContentItems`. Preserve this
hierarchy during updates; do not flatten section and child fields onto the page.

## Authoring workflow

```bash
pomi --context site content items schema Article > article.schema.json
pomi --context site content items validate --body-file article.json
pomi --context site content items create-draft --body-file article.json
```

Read the returned `ContentItemId`, inspect and render the draft. Publish only
when the user requested publication:

```bash
pomi content items show <id> --version draft
pomi content items render <id> --version draft --display-type Detail
pomi content items publish <id>
```

Do not derive payloads from examples alone. Regenerate the schema after changing
definitions or enabled features.

`AutoroutePart.SetHomepage` is intentionally absent because it is a privileged
transient editor command. Publish the intended item, then use:

```bash
pomi settings set-home-content <content-item-id>
```

This command requires the Home Route feature and its dedicated permission.

## Minimal payload pattern

```json
{
  "ContentType": "Article",
  "DisplayText": "CLI-managed article",
  "TitlePart": {
    "Title": "CLI-managed article"
  },
  "AutoroutePart": {
    "Path": "news/cli-managed-article"
  },
  "ArticleDetails": {
    "Summary": {
      "Text": "A concise summary."
    }
  }
}
```

Use the actual part attachment names from `pomi content types show Article`, not
only CLR type names. Use the schema descriptions to select the correct field
value property, such as `Text`, `Html`, `Markdown`, `Paths`, or referenced IDs.

`show` defaults to a published version. Use `--version draft` for an existing
draft or `--version latest` to inspect the newest version. A draft read returns
404 if there is no draft; use `draft <id>` only when creating one is intended.
`validate --body-file item.json` validates a new item and requires `ContentType`
in the body. For a partial edit of an existing item, use `validate-update <id>`;
the server resolves its content type. It does not persist changes or run update
workflows. Typed values must match the schema: `TitlePart.Title` is a string,
not an object. Do not interpret a type-validation error as a missing feature.

Updates merge supplied part/field properties. Omitted properties retain their
values; explicit JSON `null` clears nullable values, including publish/archive
schedules. Arrays replace the complete array, so preserve embedded IDs/items
that should remain. Validate the candidate and read it back after saving; use
the live schema to determine which properties allow null.

## Localized variants

Use `OrchardCore.ContentLocalization` and the source content type's `LocalizationPart`.
Discover configured cultures, then create or reuse a variant through the localization
manager; do not manufacture a localization set ID or clone an item by editing raw part JSON.

```bash
pomi localization cultures list
pomi content localizations list <sourceId>
pomi content localizations create <sourceId> --body-file culture.json
pomi content localizations list <sourceId> --version published
```

`culture.json` contains only the selected configured culture:

```json
{ "culture": "fr" }
```

The result contains `item` (identity/version metadata) and `created`. A new variant
is a draft; a retry reuses the existing editable variant without overwriting its body.
Read `item.contentItemId` with `content items show --version latest`, edit it and
publish only when requested. The source is the latest version; use the content
version workflow first if a different source version is required.

List defaults to latest and filters each variant by view/preview permission. A
published-only list can contain fewer items. Creation requires source localization
permission and content-type edit permission; retrying an existing target also
requires edit permission on that target. The source must have `LocalizationPart`.
Culture matching is case-insensitive; use a name from the configured culture list.
A missing/disabled feature or part is unavailable, not a reason to write arbitrary
localization IDs. A 409 means a published matching variant has a latest draft
with changed culture/set membership; inspect and resolve that draft. Coordinate
concurrent requests for the same set/culture because sequential retry behavior
does not provide a distributed lock.

[Content localizations API](https://github.com/OrchardCMS/OrchardCore/blob/main/src/docs/reference/api/content-localizations/README.md)
is a versioned reference; the live tenant contract takes precedence. UI-string
translations and culture-picker settings are separate workflows.

## Lifecycle commands

```bash
pomi content items list --content-type Article --status published --skip 0 --take 50
pomi content items show <id> --version latest
pomi content items save --body-file item.json
pomi content items create-draft --body-file item.json
pomi content items update <id> --body-file item.json
pomi content items update-draft <id> --body-file item.json
pomi content items draft <id>
pomi content items publish <id>
pomi content items unpublish <id>
pomi content items validate --body-file item.json
pomi content items validate-update <id> --body-file item.json
pomi content items render <id> --version draft --display-type Detail
pomi content items delete <id> --force
```

Run `pomi content items --help` for installation-specific filters and options.

## Specific versions

Refresh metadata after a server upgrade and check `pomi content versions --help`.
`list` takes a logical `ContentItemId`; the other commands take a returned
`ContentItemVersionId`. Never substitute one kind of ID for the other.

```bash
pomi content versions list <content-item-id> --take 50 --output json
pomi content versions show <version-id> --output json
pomi content versions render <version-id> --display-type Detail
pomi content versions restore <version-id>
pomi content versions delete <archived-version-id> --force
```

Restoration creates a new unpublished draft with a new version ID, preserving
both the source and existing published version. Inspect the current draft on
409; use `--replace-draft true` only if replacing that draft is authorized.
The replaced draft becomes archived. Do not automatically retry a restore after
an uncertain response; inspect the latest version first. Publish separately only
when the user requests it. Rendering uses current templates, not old templates.

Deletion permanently purges only an archived version (`Latest=false`,
`Published=false`). It cannot delete a published version or the current draft.
Do not delete or unpublish the logical item just to bypass this protection.
Missing versions return 404 for reads/restoration and 204 for deletion.
Archived content requires preview permission even if it was once public.

## State and identity rules

- Omit generated version IDs, state flags, and timestamps from new payloads.
- Keep `ContentType` on creates; it cannot be changed by update.
- Treat `Author` as server-controlled.
- Omit `Owner` to use or preserve the authenticated user. Changing it requires
  `EditContentOwner`.
- Preserve returned unknown part/field data when performing a full update.
- Use a stable client-supplied `ContentItemId` only when intentionally relying
  on retry-safe creation and the live schema/endpoint permits it.
- Prefer `create-draft` followed by validation/render/publish for editorial
  workflows.

## Organize content

- Use Autoroute paths for public information architecture.
- Use aliases for stable machine lookup independent of route changes.
- Store taxonomy term content item IDs in taxonomy fields.
- For List children, include `ContainedPart.ListContentItemId`,
  `ContainedPart.ListContentType`, and `ContainedPart.Order` in the child
  payload; there is no dedicated remote list-membership command.
- Keep embedded Flow/Bag widgets inside the parent payload.
- Reference uploaded media by the exact media path returned by
  `pomi media files upload`.
- Use `ContentPickerField` IDs for explicit related-content links.
- Model page layout as typed section widgets in `FlowPart.Widgets`.
- Put repeated embedded components in a named Bag on their owning section
  widget, for example `FeatureGrid.Features.ContentItems`.
- Give every embedded Flow/Bag widget or component a unique stable
  `ContentItemId`, `ContentType`, and schema-valid content object. The API
  generates only the root ID; editors and nested updates key embedded items by
  their IDs.
- Do not store layout markup in `HtmlBodyPart`; reserve HTML/Markdown fields for
  authored prose within a semantic component.

For bulk authoring, keep one JSON file per logical item, capture returned IDs in
an external deployment manifest, and order operations so referenced taxonomy,
media, menu, or parent items exist first.

## Verify

```bash
pomi content items show <id> --version latest
pomi content items render <id> --version latest --display-type Detail
pomi content items list --content-type Article --status published
```

Inspect public URLs separately; a successful API save does not prove template,
media, navigation, or CSS correctness.

Official reference (live tenant schemas take precedence): [content-items API](https://github.com/OrchardCMS/OrchardCore/blob/main/src/docs/reference/api/content-items/README.md).
