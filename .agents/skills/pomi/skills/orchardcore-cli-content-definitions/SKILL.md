---
name: orchardcore-cli-content-definitions
description: Designs and manages Orchard Core content types, reusable parts, fields, and modeling patterns through `pomi`. Use for Content Types, Flow, Bag, Lists, Taxonomies, Menus, Alias, Autoroute, Sitemaps, widgets, or planning carousel, blog, news, and landing-page models rendered with Liquid.
---

# Pomi CLI Content Definitions

Before issuing commands, read the [shared context, authentication, and output rules](../orchardcore-cli/references/shared-rules.md).
For first-time access or login problems, follow [authentication and contexts](../orchardcore-cli/references/authentication.md).
These rules apply even when this specialist is selected directly.

Design the model before authoring content or templates. Work against the target
tenant context because registered parts, fields, settings, and schemas depend on
its enabled features.

## Content-driven composition standard

Default to this hierarchy for editable pages:

```text
Page content type
└── FlowPart.Widgets (ordered page sections)
    ├── Hero widget
    ├── RichTextSection widget
    ├── FeatureGrid widget
    │   └── named BagPart "Features"
    │       └── FeatureCard widgets
    └── Carousel widget
        └── named BagPart "Slides"
            └── CarouselSlide widgets
```

Use `FlowPart` as the primary ordered page-section composer. Every direct Flow
item must be a focused content type with stereotype `Widget`. Do not model page
layout as one `HtmlBodyPart`, one large template-specific part, or raw markup.

For a section that owns an ordered collection, attach a named `BagPart` to that
section widget. Use semantic names such as `Features`, `Slides`, `TeamMembers`,
`Testimonials`, or `Questions`; avoid generic names such as `Items` when the
domain has a clearer term. Restrict `BagPartSettings.ContainedContentTypes` to
the inner component types that belong in the section.

Build inner component types first. Give each component only content-bearing
fields such as heading, text, media, link, icon, or accessible label. Make
reusable embedded components widgets so their display pipeline and
`Widget__<Type>` templates remain predictable. Put section-level content and
behavior—heading, introduction, item limit, semantic variant—on the section
widget, not on every child.

Keep presentation out of content:

- use semantic choice fields such as `Emphasis = primary|secondary`, not
  arbitrary CSS class fields;
- use HTML/Markdown fields only for authored prose, never for grids, wrappers,
  navigation, spacing, or responsive markup;
- map semantic variants to classes and layout in Liquid templates;
- keep global navigation in Menu content, not Flow;
- use independently stored content plus queries/ListPart when collection items
  need their own routes, workflow, reuse, or large-scale paging.

For a composable content site, inspect and enable the required subset of:
`OrchardCore.Contents`, `OrchardCore.ContentTypes`,
`OrchardCore.ContentFields`, `OrchardCore.Title`,
`OrchardCore.Autoroute`, `OrchardCore.Alias`, `OrchardCore.Flows`,
`OrchardCore.Lists`, `OrchardCore.Widgets`, `OrchardCore.Taxonomies`,
`OrchardCore.Menu`, and `OrchardCore.Sitemaps`. Refresh discovery afterward.

## Schema-first workflow

```bash
pomi --context site content part-types list --take 200
pomi --context site content field-types list --take 200
pomi --context site content types schema
pomi --context site content parts schema
pomi --context site content fields schema
```

Enable missing features, run `pomi api refresh`, and repeat discovery. Never copy
settings blindly from another tenant.

The safe definition DTO preserves unknown settings values but its base schema
cannot describe every enabled module. Discover contributed contracts:

```bash
pomi content settings list --take 200
pomi content settings show AutoroutePartSettings
pomi content settings show FlowPartSettings
pomi content settings show BagPartSettings
pomi content settings show ContentPickerFieldSettings
```

Each contract reports its definition scope, target part/field type, and JSON
Schema. Only settings providers contributed by enabled modules are available;
absence from this catalog does not mean a field setting is unsupported. For
rosters, calendars, and other multi-select relationships, follow the
[canonical content picker example](references/modeling-patterns.md#multi-select-content-picker).
Set `ContentPickerFieldSettings.Multiple` on each field definition before
sending multiple `ContentItemIds`; configure `DisplayedContentTypes` for editor
choices. These settings are separate from the content item's value schema.

Create definitions in dependency order:

1. Create reusable custom parts and fields.
2. Create inner block/widget types such as `FeatureCard` or `CarouselSlide`.
3. Create collection-section widgets and attach their named Bags.
4. Create other section widgets.
5. Create page types and attach a constrained `FlowPart`.
6. Read every stored definition back before authoring content.

## Commands

```bash
pomi content types list --skip 0 --take 200
pomi content types show Article
pomi content types create --body-file article-type.json
pomi content types update Article --body-file article-type.json
pomi content types delete Article --force

pomi content parts list --skip 0 --take 200
pomi content parts show ArticleDetails
pomi content parts create --body-file article-details.json
pomi content parts update ArticleDetails --body-file article-details.json
pomi content parts delete ArticleDetails --force

pomi content fields list ArticleDetails
pomi content fields show ArticleDetails Summary
pomi content fields create ArticleDetails --body-file summary-field.json
pomi content fields update ArticleDetails Summary --body-file summary-field.json
pomi content fields delete ArticleDetails Summary --force
```

Create/update bodies are full definitions, not patches. Keep technical names
stable and retry the identical request after ambiguous failures.

## Base definition shapes

Custom reusable part:

```json
{
  "name": "ArticleDetails",
  "settings": {
    "ContentPartSettings": {
      "attachable": true,
      "reusable": true,
      "displayName": "Article details"
    }
  },
  "fields": [
    {
      "name": "Summary",
      "fieldName": "TextField",
      "settings": {
        "ContentPartFieldSettings": {
          "displayName": "Summary",
          "position": "0"
        }
      }
    }
  ]
}
```

Content type:

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
      "stereotype": "Content"
    }
  },
  "parts": [
    { "name": "TitlePart", "partName": "TitlePart", "settings": {} },
    {
      "name": "AutoroutePart",
      "partName": "AutoroutePart",
      "settings": {
        "AutoroutePartSettings": {
          "AllowCustomPath": true,
          "Pattern": "{{ ContentItem.DisplayText | slugify }}",
          "AllowUpdatePath": true
        }
      }
    },
    { "name": "ArticleDetails", "partName": "ArticleDetails", "settings": {} }
  ]
}
```

Validate these examples against the live schema. Field-specific settings such
as required, length, editor, multiple-selection, and content-type restrictions
are feature-contributed and tenant-specific.

## Modeling decisions

- Use **TitlePart** for editor-facing and display titles.
- Use **AutoroutePart** for public URLs; use **AliasPart** for stable lookup keys.
- Use **FlowPart** by default for ordered heterogeneous page sections editable
  as focused widgets.
- Use **BagPart** for ordered embedded items constrained to specific content
  types. Attach it with a semantic name inside a collection-section widget.
- Use **ListPart** plus **ContainedPart** for independently stored children with
  their own lifecycle and routes.
- Use **TaxonomyField** to classify content with reusable hierarchical terms.
- Use **ContentPickerField** for explicit editorial relationships.
- Use **MenuPart** and menu-item stereotypes for navigation, not as a generic
  page-section container.
- Use sitemap source/content-type settings to publish public routable content;
  do not treat a sitemap as the primary content hierarchy.
- Prefer small reusable parts over one content type containing many unrelated
  fields.
- Put layout composition in Liquid templates, not in stored HTML fields.

Every type placed in Flow must use the exact Widget stereotype:

```json
{
  "name": "Hero",
  "displayName": "Hero",
  "settings": {
    "ContentTypeSettings": {
      "stereotype": "Widget",
      "creatable": false,
      "listable": false
    }
  },
  "parts": []
}
```

Apply this to `Hero`, `FeatureGrid`, `FeatureCard`, `Carousel`, and
`CarouselSlide` when they are embedded widgets.

The remote content API has no dedicated "add to list" operation. Include a
`ContainedPart` object in each child payload with the parent
`ListContentItemId`, `ListContentType`, and order. If the live authoring path
cannot weld this registered part, provision the relationship through a recipe
or the admin list editor.

Flow and named Bag settings live under the attachment's settings object:

```json
{
  "name": "FlowPart",
  "partName": "FlowPart",
  "settings": {
    "FlowPartSettings": {
      "ContainedContentTypes": ["Hero", "Carousel", "FeatureGrid"]
    }
  }
}
```

```json
{
  "name": "Slides",
  "partName": "BagPart",
  "settings": {
    "BagPartSettings": {
      "ContainedContentTypes": ["CarouselSlide"],
      "DisplayType": "Detail"
    }
  }
}
```

Confirm exact JSON casing with `content settings show` and the stored definition
because definition settings are extensible. The base DTO schema intentionally
uses an open settings bag; do not interpret that as a complete module contract.

Read [modeling-patterns](references/modeling-patterns.md) for concrete carousel, blog/news, landing
page, taxonomy, menu, and Liquid-shape designs.

Use [orchardcore-cli-menus](../orchardcore-cli-menus/SKILL.md) when creating menu content or overriding the
official Menu/MenuItem/MenuItemLink templates.

## Verify the model

```bash
pomi content parts show ArticleDetails
pomi content types show Article
pomi content items schema Article
```

The content-item schema is the authoritative authoring contract and includes
the attached CLR-backed part and field descriptions. Confirm that privileged or
transient editor commands are absent.

Remote management currently has no dedicated sitemap document/source API.
`SitemapPart` can model per-item inclusion settings, but creating a sitemap and
its content-type source requires a recipe, deployment plan, admin UI, or a
future sitemap management API.

Official references (live tenant schemas take precedence):
[content-definitions API](https://github.com/OrchardCMS/OrchardCore/blob/main/src/docs/reference/api/content-definitions/README.md) and the module references below:

- [ContentTypes](https://github.com/OrchardCMS/OrchardCore/blob/main/src/docs/reference/modules/ContentTypes/README.md)
- [Flow](https://github.com/OrchardCMS/OrchardCore/blob/main/src/docs/reference/modules/Flow/README.md)
- [Lists](https://github.com/OrchardCMS/OrchardCore/blob/main/src/docs/reference/modules/Lists/README.md)
- [Taxonomies](https://github.com/OrchardCMS/OrchardCore/blob/main/src/docs/reference/modules/Taxonomies/README.md)
- [Menu](https://github.com/OrchardCMS/OrchardCore/blob/main/src/docs/reference/modules/Menu/README.md)
- [Alias](https://github.com/OrchardCMS/OrchardCore/blob/main/src/docs/reference/modules/Alias/README.md)
- [Sitemaps](https://github.com/OrchardCMS/OrchardCore/blob/main/src/docs/reference/modules/Sitemaps/README.md)
