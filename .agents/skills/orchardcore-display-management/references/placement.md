# placement.json reference

`placement.json` lives at the **root** of a module or theme. It is an object whose properties are **shape types**; each maps to an array of placement rules. A rule = **filters** (which shapes) + **placement info** (what to do).

```json
{
  "TextField": [
    {
      "displayType": "Detail",
      "differentiator": "Article-MyTextField",
      "contentType": [ "Page", "BlogPost" ],
      "contentPart": [ "HtmlBodyPart" ],
      "path": [ "/mypage" ],
      "place": "Content:2",
      "alternates": [ "TextField_Title" ],
      "wrappers": [ "TextField_Title" ],
      "shape": "AnotherShape"
    }
  ]
}
```

## Filters

| Filter | Matches |
|--------|---------|
| *(property name)* | The shape's original type (`TextField`, `ContentPart`, `Parts_Contents_Publish`) |
| `displayType` | `Detail`, `Summary`, `SummaryAdmin`, `DetailAdmin`, `Edit` |
| `differentiator` | Distinguishes reused shapes — see table below |
| `contentType` | Single/array of ContentType or Stereotype. `Art*` wildcard prefix-matches |
| `contentPart` | Single/array of part names the content item must contain |
| `path` | Single/array of request paths |

Custom filters: implement `IPlacementNodeFilterProvider`.

## Placement info

| Key | Effect |
|-----|--------|
| `place` | `Zone:Position`. `-` hides the shape. Leading `/` → layout zone (`/Content:5`) |
| `alternates` | Array of alternate shape types added to metadata (selects more specific templates) |
| `wrappers` | Array of shape types wrapping the shape |
| `shape` | Replace with a different shape type |

## Differentiators (critical)

The differentiator depends on **which shape type** you target, not the part/field you have in mind.

| Shape type | Differentiator pattern | Example |
|------------|------------------------|---------|
| `BagPart`, `FlowPart`, `TitlePart` (part display, or inner `XxxPart_Edit`) | `{PartName}` | `Services`, `FlowPart`, `TitlePart` |
| `ContentPart` (part with **no** display driver) | `{PartName}` | `GalleryPart` |
| `ContentPart_Edit` (admin editor wrapper) | `{ContentType}-{PartName}` | `LandingPage-Services`, `Article-TitlePart` |
| `TextField`, `HtmlField`, … (field display/editor) | `{PartName}-{FieldName}` | `Article-Subtitle`, `Address-City` |
| `TextField_Display`, … (field display mode) | `{PartName}-{FieldName}-{FieldType}_Display__{Mode}` | `Blog-Subtitle-TextField_Display__Header` |

`PartName`: named parts use the custom name (`Services`); non-named parts use the part type (`TitlePart`, `FlowPart`, `WidgetsListPart`).

A field attached directly to a content type uses the **type** as PartName: a `Subtitle` field on `Article` → differentiator `Article-Subtitle`.

## Position format

`place` = `Zone:Position`.

| Value | Meaning |
|-------|---------|
| `Content` | Content zone, position 0 |
| `Content:5` | position 5 |
| `Content:5.1` | between 5 and 6 |
| `Content:before` | very beginning (internally -9999) |
| `Content:after` | very end (internally 9999) |
| `Content:-` is **not** valid — `-` alone as the whole `place` hides | hide: `"place": "-"` |

Ordering: `before`, `0`/empty, `1`, `1.1`, `1.2`, `2`, `10`, `after`. Numeric compare; module name does **not** affect order.

## Hiding

```json
{ "TitlePart": [{ "differentiator": "TitlePart", "place": "-" }] }
```

Hide the **whole editor row** (label + wrapper) — target the wrapper, not the inner shape:

```json
{ "ContentPart_Edit": [{ "differentiator": "LandingPage-Services", "place": "-" }] }
```

## Layout zones

A `place` starting with `/` moves the shape out of the local content zone into a **layout** zone (rendered by the theme's `Layout.cshtml` `<zone Name="...">`):

```json
{ "Parts_Contents_Publish": [{ "place": "/Sidebar:5", "displayType": "Detail" }] }
```

## Editor groupings

Group editor shapes into **tabs**, **cards**, **columns**. Modifiers attach to the `place` value:

| Modifier | Group type | Position modifier | Width modifier |
|----------|-----------|-------------------|----------------|
| `#` | Tab | `;N` | — |
| `%` | Card | `;N` | — |
| `\|` | Column | `;N` | `_N` (Bootstrap 12-grid) |

Progressive nesting allowed: Tab → Card → Column. Ungrouped shapes fall into the default `Content` group.

**Shape position (`:N`) ≠ group position (`;N`).** `:N` orders shapes within the zone; `;N` orders the groups (which tab/card/column comes first).

Tabs example — Media tab first, Content tab second:

```json
{
  "MediaField_Edit": [{ "place": "Parts:0#Media;0", "contentType": ["Article"] }],
  "HtmlField_Edit":  [{ "place": "Parts:0#Content;1", "contentType": ["Article"] }]
}
```

Columns example — 3-wide + 9-wide row:

```json
{
  "TextField_Edit": [
    { "place": "Parts:1|Sidebar_3;1", "differentiator": "MyPart-FieldA" },
    { "place": "Parts:1|Main_9;2",    "differentiator": "MyPart-FieldB" }
  ]
}
```

Column name is just a CSS label (`column-{name}`), no positional meaning. Width `_9` → `col-md-9`; change breakpoint with `_lg-9` → `col-lg-9`. All column shapes at the same grouping level share one Bootstrap `row`; separate rows need separate cards.

## Dynamic part placement

A part created in JSON (no driver) renders as `ContentPart` (detail) / `ContentPart_Summary` (summary), differentiator = part name. To send it to a custom template zone:

```json
{ "ContentPart": [{ "place": "MyGalleryZone", "differentiator": "GalleryPart" }] }
```

Then in `Content-Product.Detail.cshtml`: `@await DisplayAsync(Model.MyGalleryZone)`.

## Precedence

1. Main startup project (acts as super-theme)
2. Active theme (front-end or admin, per request)
3. Modules (ordered by dependency)
