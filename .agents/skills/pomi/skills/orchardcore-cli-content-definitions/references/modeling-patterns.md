# Content modeling patterns

## Contents

- [Core part selection](#core-part-selection)
- [Multi-select content picker](#multi-select-content-picker)
- [Carousel](#carousel)
- [Blog and news feed](#blog-and-news-feed)
- [Landing page](#landing-page)
- [Taxonomy and menus](#taxonomy-and-menus)
- [Liquid contract](#liquid-contract)

## Core part selection

| Requirement | Prefer | Avoid |
| --- | --- | --- |
| Ordered mixed page sections | `FlowPart` with widget types | A single HTML blob |
| Ordered repeated structured cards | `BagPart` constrained to card types | Parallel arrays of values |
| Independent child pages/items | `ListPart` + child `ContainedPart` | Embedded Bag items |
| Classification/filtering | `TaxonomyField` | Free-text category names |
| Explicit related content | `ContentPickerField` | IDs in a `TextField` |
| Stable programmatic lookup | `AliasPart` | Depending on mutable routes |
| Public route | `AutoroutePart` | Hard-coded template URLs |

Use widgets for embedded Flow/Bag components. Use ordinary `Content` stereotype
for independently managed documents.

## Flow-first page architecture

Treat a page as ordered content sections, not as a designed HTML document.
Attach one constrained `FlowPart` to the page and create a widget type for each
semantic section: `Hero`, `RichTextSection`, `FeatureGrid`, `Carousel`,
`Testimonials`, `FaqSection`, `LatestNews`, and `CallToAction`.

Use a named Bag only inside a section that owns repeated embedded components:

| Section widget | Named Bag | Inner component |
| --- | --- | --- |
| `FeatureGrid` | `Features` | `FeatureCard` |
| `Carousel` | `Slides` | `CarouselSlide` |
| `TeamSection` | `TeamMembers` | `TeamMember` |
| `Testimonials` | `Testimonials` | `Testimonial` |
| `FaqSection` | `Questions` | `FaqItem` |

Create the inner component before its section. Give both stereotype `Widget`,
then constrain the Bag by concrete content type and set the Bag display type
used by its templates. Constrain the page Flow to section widgets, not inner
cards/slides, so editors cannot place a card directly at page level.

Prefer independently stored content plus a query or ListPart instead of Bag
when children need routes, localization/workflow independent of the page,
cross-page reuse, unbounded growth, or server-side paging.

Never store section wrapper markup, Bootstrap/Tailwind grids, responsive
classes, or JavaScript initialization in content fields. Store semantic values
and let `Widget__<Type>` templates own design.

## Multi-select content picker

A `ContentPickerField` accepts at most one ID by default, even though its value
is a `ContentItemIds` array. Set `Multiple` to `true` on the **field definition**
before assigning a roster, calendar, or other collection of related items.
Set `DisplayedContentTypes` to the technical type names editors should be able
to select. This filters editor choices; it is not a server-side type validation
or authorization rule. Alternatively, use `DisplayAllContentTypes: true` to
show all types without a stereotype. Nonempty `DisplayedStereotypes` takes
precedence over `DisplayedContentTypes`.

Assume a `Team` content type has a `Team` part and `Person` is an existing
content type. Save this as `roster-field.json`:

```json
{
  "name": "Roster",
  "fieldName": "ContentPickerField",
  "settings": {
    "ContentPartFieldSettings": {
      "displayName": "Team roster"
    },
    "ContentPickerFieldSettings": {
      "Multiple": true,
      "Required": false,
      "DisplayAllContentTypes": false,
      "DisplayedContentTypes": ["Person"]
    }
  }
}
```

`name` identifies this field; `fieldName` identifies its registered field type.
Keep the contributed settings container and its members Pascal-cased as shown.
These JSON properties are not generated CLI options such as `--multiple`.

```bash
pomi content settings show ContentPickerFieldSettings
pomi content fields create Team --body-file roster-field.json
pomi content fields show Team Roster --output json
pomi content items schema Team
```

For an existing field, read its definition first, preserve its other settings,
merge the picker settings above, and save the complete definition to the file.
Use `pomi content fields update Team Roster --body-file roster-field.json`
instead of `create`. Definition updates replace the supplied definition; do
not overwrite an existing field with an incomplete example.

Save a content payload as `team.json`, replacing the example IDs with actual
`Person` content item IDs (not version IDs):

```json
{
  "ContentType": "Team",
  "Team": {
    "Roster": {
      "ContentItemIds": ["PERSON_ITEM_ID_1", "PERSON_ITEM_ID_2"]
    }
  }
}
```

Include any other fields required by your `Team` definition, then validate
before creating content:

```bash
pomi content items validate --body-file team.json
```

For a calendar field, use a different field name such as `Calendar` and
`DisplayedContentTypes: ["Event"]`, with IDs of existing `Event` items. Each
picker field needs its own `Multiple: true` setting. `Required: true` additionally
requires at least one ID; otherwise an empty array is allowed.

`content settings list` lists contracts registered by enabled modules, not every
setting the server accepts. Content Fields contributes `ContentPickerFieldSettings`;
older servers or other fields may have no registered contract. If it is missing,
check `pomi content field-types list --take 200` and the field's module documentation,
then read the stored definition and validate a representative multi-item payload.
Do not conclude that an absent contract means its settings are unsupported.

## Carousel

Create a `CarouselSlide` widget with:

- `TitlePart` or a text heading field;
- `MediaField Image`;
- `TextField Caption`;
- `LinkField Link`;
- optional `TextField AccessibleLabel`.

Attach a `BagPart` named `Slides` to a `Carousel` widget and constrain it to
`CarouselSlide`. Place `Carousel` instances in a page's `FlowPart`.

Use Bag when slides belong only to that carousel. Use a
`ContentPickerField` when slides must be reused and managed independently.

Liquid outline:

```liquid
<section class="carousel" aria-label="{{ Model.ContentItem.DisplayText }}">
  {% for slide in Model.ContentItem.Content.Slides.ContentItems %}
    {% assign image = slide.Content.CarouselSlide.Image.Paths.first %}
    <article class="carousel__slide">
      <img src="{{ image | asset_url }}" alt="{{ slide.Content.CarouselSlide.Image.MediaTexts.first }}">
      <h2>{{ slide.Content.TitlePart.Title }}</h2>
      <p>{{ slide.Content.CarouselSlide.Caption.Text }}</p>
    </article>
  {% endfor %}
</section>
```

Confirm actual property paths with `pomi content items schema Carousel`; field
JSON differs by field type and enabled version.

For the parent item contract, Flow stores embedded widgets under
`FlowPart.Widgets`; a named Bag stores embedded items under
`<AttachmentName>.ContentItems`. Obtain the schema for every embedded type and
include each nested item's `ContentType` and a unique stable `ContentItemId`.
The API generates an ID only for the root item.

```json
{
  "ContentType": "ProductLaunch",
  "FlowPart": {
    "Widgets": [
      {
        "ContentItemId": "1c18a63e-1d7d-43b7-9e99-034079f01e6f",
        "ContentType": "FeatureGrid",
        "Features": {
          "ContentItems": [
            {
              "ContentItemId": "d3113f06-feb7-4111-8a34-311055b66a89",
              "ContentType": "FeatureCard",
              "FeatureCard": {
                "Heading": { "Text": "Fast setup" }
              }
            }
          ]
        }
      },
      {
        "ContentItemId": "5c642103-f74e-42bc-b8ad-74aee2a6c802",
        "ContentType": "Carousel",
        "Slides": {
          "ContentItems": [
            {
              "ContentItemId": "ac623eda-916f-41b3-b625-95f922aadd89",
              "ContentType": "CarouselSlide",
              "CarouselSlide": {
                "Heading": { "Text": "Welcome" },
                "Image": {
                  "Paths": ["images/hero.png"],
                  "MediaTexts": ["Product overview"]
                }
              }
            }
          ]
        }
      }
    ]
  }
}
```

## Blog and news feed

Create:

- `Article`: Title, Autoroute, Markdown/HTML body, summary, hero image,
  taxonomy fields, publication date, and optional author picker.
- `Blog`: Title, Autoroute, and `ListPart` restricted to `Article`.
- taxonomy content for categories and tags.

Use List/Contained when articles are children of one blog. Use taxonomy/query
selection when articles can appear in multiple feeds. A news home page should
normally render a query result, not duplicate articles inside a Bag.

Liquid should render summaries and public URLs from content shapes when
possible. Keep publication filtering in the list/query API, not in template
string comparisons.

## Landing page

Create a `LandingPage` with Title, Autoroute, SEO, and Flow. Create focused
widget types such as:

- `Hero`: heading, body, background/foreground media, primary link;
- `FeatureGrid`: Bag of `FeatureCard`;
- `CallToAction`: heading, text, link, visual style choice;
- `LatestNews`: query identifier and item limit.

Use Flow to order sections. Use Bag only inside widgets with repeated children.
Do not add every possible section field directly to `LandingPage`.

## Taxonomy and menus

Use a taxonomy for semantic classification and filtering. Terms are content
items; retain their IDs when assigning a `TaxonomyField`. Read the live content
schema before sending term IDs.

Use Menu content and menu-item types for navigation order and links. Link menu
items to routable content rather than copying page titles and URLs into
unrelated fields. Keep navigation structure separate from taxonomy unless the
same hierarchy genuinely serves both purposes.

Configure sitemaps from routable published types. Exclude utility widgets,
embedded Bag items, and non-public content.

## Liquid contract

Use custom Templates for presentation and keep definitions presentation-neutral.
Template names use shape alternates such as:

- `Content__Article`
- `Content_Summary__Article`
- `Widget__Hero`
- field templates selected from the runtime field alternate set

Verify available shapes/alternates in the target site. Escape plain text by
default, use Orchard Liquid filters for URLs/media/shapes, and render rich
content only through the field/shape contract that already sanitizes it.

Pair every model with:

1. its stored content definition;
2. `pomi content items schema <type>`;
3. one valid draft fixture;
4. one custom Liquid template;
5. public-route verification.
