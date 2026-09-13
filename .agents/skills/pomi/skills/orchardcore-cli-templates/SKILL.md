---
name: orchardcore-cli-templates
description: Creates and manages Orchard Core custom Liquid templates, shortcode templates, shape placements and conditional layers through `pomi`. Use for content, summary, widget, field, and layout shape overrides; layer definitions, widget placement and visibility rules; rendering content models; registering CSS/media; and validating output for remotely managed tenants.
---

# Pomi CLI Templates

Before issuing commands, read the [shared context, authentication, and output rules](../orchardcore-cli/references/shared-rules.md).
For first-time access or login problems, follow [authentication and contexts](../orchardcore-cli/references/authentication.md).
These rules apply even when this specialist is selected directly.

Use the Templates module for tenant-stored Liquid shape overrides. Keep content
structure in definitions, content values in items, and presentation in Liquid.
An active site theme is required: custom templates extend the active theme's
shape table and cannot replace the absence of a site theme.

Render content-driven composition rather than rebuilding it in the page
template. A page template should render its `FlowPart`; each section should use
a `Widget__<SectionType>` template; collection-section templates should render
or iterate their named Bag. Keep grids, classes, spacing, breakpoints, and
client behavior in these Liquid templates, never in editor content.

Ensure `OrchardCore.Templates`, `OrchardCore.Liquid`, and
`OrchardCore.Resources` are enabled. Enable the part/field modules used by the
model, then refresh discovery.

For a tenant created with the `Blank` recipe, start from Safe Mode:

```bash
pomi themes list --admin false --take 200
pomi themes set-current TheTheme
pomi features enable OrchardCore.Templates
pomi features enable OrchardCore.Liquid
pomi features enable OrchardCore.Resources
pomi api refresh --force
```

Use a theme ID returned by `themes list`; `TheTheme` is the standard host
example. Confirm the root page renders before adding a custom `Layout`
template. If the root is still in Safe Mode, fix theme selection first.

## Workflow

1. Inspect the target type, schema, and representative content item.
2. Determine the exact shape/alternate template name.
3. Create the complete template JSON in a local file.
4. Create or replace the template.
5. Render an item and verify its public route, media, and CSS.

```bash
pomi content types show Article
pomi content items schema Article
pomi content items show <id> --version published
pomi templates schema --operation create
pomi templates list --search Article
```

## Manage templates

A complete minimal `Layout` template must render document metadata, resource
zones, messages, and the content section:

```liquid
<!DOCTYPE html>
<html lang="{{ Culture.Name }}" dir="{{ Culture.Dir }}">
<head>
  <meta charset="utf-8">
  <meta name="viewport" content="width=device-width, initial-scale=1">
  <title>{{ "PageTitle" | shape_new | shape_stringify }}</title>
  {% resources type: "Meta" %}
  {% resources type: "HeadLink" %}
  {% resources type: "HeadScript" %}
  {% resources type: "Stylesheet" %}
</head>
<body>
  {% render_section "Header", required: false %}
  {% render_section "Messages", required: false %}
  <main id="main-content">
    {% render_section "Content" %}
  </main>
  {% render_section "Footer", required: false %}
  {% resources type: "FootScript" %}
</body>
</html>
```

Create it as template name `Layout` before relying on tenant CSS or scripts.

Upload `assets/styles/site-v1.css` through Media first (see
[orchardcore-cli-media](../orchardcore-cli-media/SKILL.md)). Resolve it with `asset_url` so tenant prefixes and
remote storage providers work.

`article-template.json`:

```json
{
  "name": "Content__Article",
  "content": "{% assign stylesheet = \"assets/styles/site-v1.css\" | asset_url %}{% style name:\"tenant-site\", src:stylesheet %}<article class=\"article\"><h1>{{ Model.ContentItem.DisplayText }}</h1>{{ Model.Content.HtmlBodyPart | shape_render }}</article>",
  "description": "Article detail template."
}
```

```bash
pomi templates create --body-file article-template.json
pomi templates show Content__Article
pomi templates update Content__Article --body-file article-template.json
pomi templates delete Content__Article --force
```

Create uses a stable case-insensitive name. An identical retry converges;
different content under the same name conflicts. Update is a complete
replacement and the route/body names must match exactly.

## Shortcode templates

Use `OrchardCore.Shortcodes.Templates` for named Liquid snippets inserted as bracketed shortcodes.
Discover the live commands before use; code-defined providers are separate from stored templates.

```bash
pomi features enable OrchardCore.Shortcodes.Templates
pomi api refresh --force
pomi shortcodes templates --help
pomi shortcodes templates schema --operation create
pomi shortcodes templates list
```

Read a definition before replacing it. Save the complete JSON contract (`name`, `content`, `hint`,
`usage`, `defaultValue`, `categories`) to a file. Names are shortcode identifiers without brackets,
stored in invariant lowercase. `content` is Liquid; `usage` is sanitized HTML for the picker.

```bash
pomi shortcodes templates validate --body-file callout.json
pomi shortcodes templates create --body-file callout.json
pomi shortcodes templates show callout --output json
pomi shortcodes templates update callout --body-file callout.json
pomi shortcodes templates delete callout --force
```

Validation parses without rendering. Create retries accept equivalent definitions; a different
existing definition conflicts. Update replaces all fields, so omitted metadata is cleared. The
body name must match the update argument. Deletion preserves content and can reveal an underlying
code-defined provider with the same name. Verify representative rendered pages after changes;
CLI or MCP success alone does not prove the shortcode produces the intended HTML.

## Shape placements

Use `OrchardCore.Placements` to control where an existing shape renders, which alternate/wrapper
it uses, or whether it is hidden. Inspect the actual shape and differentiator first; a content
part type is not always its rendered shape name.

```bash
pomi features enable OrchardCore.Placements
pomi api refresh --force
pomi placements --help
pomi placements schema --operation create
pomi placements filters
pomi placements list
pomi placements show HtmlBodyPart --output json
```

Save a complete definition to a file, for example:

```json
{
  "shapeType": "HtmlBodyPart",
  "nodes": [{"place":"-","displayType":"Detail","contentType":"Article","path":"~/private*"}]
}
```

```bash
pomi placements validate --body-file placement.json
pomi placements create --body-file placement.json
pomi placements update HtmlBodyPart --body-file placement.json
pomi placements delete HtmlBodyPart --force
```

Use only filter keys returned by the tenant. Rule order matters: later matching location/shape
values replace earlier ones, while alternates/wrappers accumulate. Create retries accept equivalent
rules; a different existing definition conflicts. Update replaces the complete ordered array;
`nodes: []` removes the override. The body shape type must match the update argument.

Read existing overrides before editing and preserve unrelated rules. Verify both matching and
nonmatching pages after changes, including shape wrappers when editing admin placement. Removing
an override exposes theme/module placement again. Database storage is the default; enabling
`OrchardCore.Placements.FileStorage` selects a separate tenant file document, without migrating
existing rules. The API never edits theme/module `placement.json` files.

## Conditional layers

Use `OrchardCore.Layers` for rules that control the visibility of widgets placed
in theme zones. A layer definition does not place a widget. Refresh discovery
and inspect `pomi layers --help` and `pomi layers conditions --output json`.
Use only conditions marked `canWrite`, with properties matching their live
schemas. Prefer the built-in URL, culture, role and authentication conditions
when they express the requested behavior.

```bash
pomi layers list --output json
pomi layers conditions --output json
pomi layers schema --operation create
pomi layers validate --body-file layer.json --output json
pomi layers create --body-file layer.json --output json
pomi layers show News --output json
pomi layers update News --body-file layer.json --output json
```

Layer JSON contains `name`, `description` and `conditions`. Each condition has
`name`, `properties`, optional `conditionId` and group-only `conditions` children.
For an always-matching layer, use
`{"name":"Always","conditions":[{"name":"BooleanCondition","properties":{"value":true}}]}`.
An empty conditions array does not match. All root conditions must match;
All/Any groups express nested logic. JavaScript validation checks syntax without
running the script, so verify its behavior on the actual page.

Updates replace the complete description and conditions. Read back first,
preserve returned condition IDs and retain every condition that should remain.
Identical creates converge; a different definition under an existing name
conflicts. Names cannot be renamed through update. Deletion requires explicit
confirmation and is refused while widgets reference the layer; do not delete
widgets merely to bypass that refusal. Verify public output for the intended
URL and visitor identity after saving, including a case that should hide the
widget. A successful validation or save does not prove visibility.

## Widget placement in layers

Use `layers widgets` to attach or move existing widget content. Create its type
with the `Widget` stereotype and use content commands for its body and publication.
Enable Layers, configure remote management and refresh discovery first.

```bash
pomi layers widgets zones
pomi layers widgets list --version latest --layer Always
pomi layers widgets show <widget-id> --version published
pomi layers widgets schema --operation update
pomi layers widgets update <widget-id> --body-file widget-placement.json
```

Read the existing placement before replacing all four fields:

```json
{
  "layer": "Always",
  "zone": "Content",
  "position": 2.5,
  "renderTitle": false
}
```

Use an existing layer and an exact zone returned by `zones`; do not invent a zone
from a theme name. If no suitable zone is configured, resolve the tenant's Layers
zone settings first. Zone discovery does not change settings or guarantee that
the active theme renders the zone. Use finite, distinct numeric positions for
predictable ordering within a zone; negative and fractional positions are valid.

Updates require `ManageLayers` and content edit permission on every affected
version, plus publish permission when a published version exists. A full update
applies placement to the latest and published versions without publishing draft
body edits or creating versions. Repeating an identical update is a no-op. Show
and list apply resource view/preview permissions; lists count only visible items.
Use the content lifecycle commands explicitly when publication is intended.

Verify the public route after attachment or moving. Check visibility conditions,
zone placement and ordering, and ensure any unrelated draft text remains absent
from public output. Layer deletion also protects references from published
widgets whose newer draft points to a different layer.

## Naming

Common alternates include:

```text
Content__Article
Content_Summary__Article
Widget__Hero
```

Field alternates depend on part name, field name, field shape type, content
type, and display type. Do not guess them. Follow the Templates module's field
alternate table, inspect rendered shape metadata or existing templates, and
confirm the override with `pomi content items render`.

## Liquid rules

- Escape editor-provided text by default.
- Avoid `raw` and `liquid` for ordinary values.
- Prefer `shape_render` for HTML-bearing parts and fields.
- Read direct values from
  `Model.ContentItem.Content.<Part>.<Field>.<ValueProperty>` only after checking
  the live content schema.
- Resolve media paths through Orchard filters rather than concatenating public
  URLs.
- Use `~` application-relative static paths so tenant prefixes are preserved.
- Register resources with Liquid tags and ensure the active layout renders
  `{% resources type: "Stylesheet" %}` and scripts where applicable.
- Keep query/filter business logic outside templates when a query or content
  list can provide the intended collection.
- Keep page templates structural and small. Put section design in widget
  templates and inner-card design in inner widget templates.
- Map semantic content values to design classes explicitly; never render an
  editor-provided CSS class with `raw`.

Example media rendering:

```liquid
{% assign image = Model.ContentItem.Content.ArticleDetails.HeroImage %}
{% if image.Paths.first %}
  {% assign image_url = image.Paths.first | asset_url | append_version %}
  <img src="{{ image_url }}" alt="{{ image.MediaTexts.first }}" class="article__hero">
{% endif %}
```

Do not pass editor-controlled alternative text or other attribute values to
`img_tag`; write them in normal Liquid HTML attributes so Liquid encodes them.

Read [liquid-patterns](references/liquid-patterns.md) for content, summary, Flow/Bag, list, menu,
and resource patterns.

## Verify

```bash
pomi content items render <id> --version draft --display-type Detail
pomi content items render <id> --version published --display-type Summary
curl -fsS 'https://cms.example.com/tenant-a/article-path'
pomi media files show assets/styles/site-v1.css --output json
```

Fetch the returned Media `url` to check the stylesheet response.
Check semantic HTML, encoded values, image alternative text, tenant-prefixed
URLs, missing shapes, stylesheet requests, and responsive behavior. Require:

- root route and representative content route return successful HTML;
- CSS and JavaScript URLs return the expected content type;
- every internal anchor target exists and keyboard focus reaches it;
- browser console has no errors;
- layout works at approximately 375px, 768px, and 1440px viewport widths.

Official references (live tenant schemas take precedence):
[Placements API](https://github.com/OrchardCMS/OrchardCore/blob/main/src/docs/reference/api/placements/README.md),
[Shortcode templates API](https://github.com/OrchardCMS/OrchardCore/blob/main/src/docs/reference/api/shortcode-templates/README.md),
[Layers API](https://github.com/OrchardCMS/OrchardCore/blob/main/src/docs/reference/api/layers/README.md),
[templates API](https://github.com/OrchardCMS/OrchardCore/blob/main/src/docs/reference/api/templates/README.md),
[Templates module](https://github.com/OrchardCMS/OrchardCore/blob/main/src/docs/reference/modules/Templates/README.md), and
[Liquid module](https://github.com/OrchardCMS/OrchardCore/blob/main/src/docs/reference/modules/Liquid/README.md).
