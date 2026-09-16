# Liquid template patterns

## Contents

- [Content detail](#content-detail)
- [Summary cards](#summary-cards)
- [Flow and Bag](#flow-and-bag)
- [Lists and feeds](#lists-and-feeds)
- [Resources and media](#resources-and-media)

## Content detail

```liquid
<article class="article">
  <header>
    <h1>{{ Model.ContentItem.DisplayText }}</h1>
  </header>
  {{ Model.Content | shape_render }}
</article>
```

Render the existing part/field shape when it provides sanitization, formatting,
placement, or alternates that direct property access would bypass.

## Summary cards

```liquid
<article class="card">
  <h2>
    <a href="{{ Model.ContentItem | display_url }}">
      {{ Model.ContentItem.DisplayText }}
    </a>
  </h2>
  {{ Model.Content["ArticleDetails-Summary"] | shape_render }}
</article>
```

Confirm filter and shape names in the installed Liquid documentation and
runtime. Do not hard-code an Autoroute path when a display URL filter is
available.

## Flow and Bag

Prefer rendering the Flow/Bag shape so placement and widget wrappers remain
active:

```liquid
<main class="landing-page">
  {{ Model.Content.FlowPart | shape_render }}
</main>
```

Use direct iteration only for a deliberately custom aggregate:

```liquid
{% for item in Model.ContentItem.Content.Slides.ContentItems %}
  <section class="slide">
    <h2>{{ item.Content.TitlePart.Title }}</h2>
  </section>
{% endfor %}
```

Verify `ContentItems` and attached-part names in the content-item schema.

## Lists and feeds

Use the list/query result already supplied to the shape. Apply display types to
child shapes instead of rebuilding every child from raw JSON. Keep paging,
publication state, taxonomy filtering, and sorting in the list/query layer.

## Resources and media

```liquid
{% assign stylesheet = "assets/styles/site-v1.css" | asset_url %}
{% style name:"tenant-site", src:stylesheet %}

{% assign media = Model.ContentItem.Content.ArticleDetails.HeroImage %}
{% if media.Paths.first %}
  {% assign media_url = media.Paths.first | asset_url | append_version %}
  <img src="{{ media_url }}" alt="{{ media.MediaTexts.first }}">
{% endif %}
```

Requirements:

- Upload CSS through Media before referencing it; follow the [Media skill](../../orchardcore-cli-media/SKILL.md) for
  folder selection and restricted-extension permissions.
- Upload media before saving media field paths.
- Include alternative text.
- Keep editor-controlled attributes in normal Liquid markup so they are
  HTML-encoded; do not pass them to `img_tag`.
- Render registered resources in the layout.
- Use versioned filenames for updated assets because public cache lifetimes are long.
