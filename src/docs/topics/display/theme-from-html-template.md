# Create an Orchard Core theme from an HTML template

Many websites start from a static HTML template, whether hand-crafted or downloaded from providers like [Start Bootstrap](https://startbootstrap.com/) or [HTML5 UP](https://html5up.net/). This guide explains how to turn such a template into a fully working Orchard Core theme.

This is exactly how the built-in `TheBlogTheme` was created: it is a port of the [Start Bootstrap Clean Blog](https://startbootstrap.com/theme/clean-blog) template. You can use its [source code](https://github.com/OrchardCMS/OrchardCore/tree/main/src/OrchardCore.Themes/TheBlogTheme) as a real-world reference while following this guide.

## Prerequisites

- An Orchard Core CMS application ([Create a CMS application](../../guides/create-cms-application/README.md)).
- An empty theme project added to your solution ([Create a Theme](../../getting-started/theme.md)).
- The HTML template you want to convert, with its CSS, JavaScript and image assets.

In the examples below, the theme is named `MyTheme`.

## 1. Copy the static assets

Copy the template's assets into the `wwwroot` folder of your theme project:

```text
MyTheme/
  wwwroot/
    css/
      styles.css
    js/
      scripts.js
    img/
      ...
    favicon.ico
  Views/
  Manifest.cs
  MyTheme.csproj
```

Anything in `wwwroot` is served by the static file middleware under the theme's name. For example, `wwwroot/css/styles.css` is available at `/MyTheme/css/styles.css`.

## 2. Declare the resources

Instead of hardcoding `<link>` and `<script>` tags, register the template's stylesheets and scripts as named resources. This lets Orchard Core handle versioning (cache busting), minified vs. non-minified files, CDN fallbacks, and dependency ordering.

Create a `ResourceManagementOptionsConfiguration.cs` file in the theme project:

```csharp
using Microsoft.Extensions.Options;
using OrchardCore.ResourceManagement;

namespace MyTheme;

public sealed class ResourceManagementOptionsConfiguration : IConfigureOptions<ResourceManagementOptions>
{
    private static readonly ResourceManifest _manifest;

    static ResourceManagementOptionsConfiguration()
    {
        _manifest = new ResourceManifest();

        _manifest
            .DefineStyle("MyTheme")
            .SetUrl("~/MyTheme/css/styles.min.css", "~/MyTheme/css/styles.css")
            .SetVersion("1.0.0");

        _manifest
            .DefineScript("MyTheme")
            .SetUrl("~/MyTheme/js/scripts.min.js", "~/MyTheme/js/scripts.js")
            .SetVersion("1.0.0");
    }

    public void Configure(ResourceManagementOptions options)
    {
        options.ResourceManifests.Add(_manifest);
    }
}
```

Then register it in a `Startup.cs` file:

```csharp
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Modules;

namespace MyTheme;

public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection serviceCollection)
    {
        serviceCollection.AddResourceConfiguration<ResourceManagementOptionsConfiguration>();
    }
}
```

If the template depends on a library like Bootstrap, define it as a separate resource and declare it as a dependency with `SetDependencies()`, so it is always included in the right order. See [Resources](../../reference/modules/Resources/README.md) for all the options (CDN URLs, integrity hashes, etc.).

## 3. Convert the HTML page into a Layout

The layout is the markup shared by every page of the site: the `<head>` element, header, navigation, footer, and the container in which each page renders its own content.

Take the template's main HTML page (usually `index.html`), copy it to `Views/Layout.liquid` (or `Views/Layout.cshtml` if you prefer Razor), and transform it:

1. **Keep the overall markup** — the point of using a template is to preserve its design.
2. **Replace the hardcoded `<link>` and `<script>` tags** with the resources declared in step 2.
3. **Replace the hardcoded page content** with `{% render_body %}`.
4. **Add zones** with `{% render_section %}` where widgets should be able to appear (header, footer, sidebars).

A typical result looks like this:

```liquid
<!DOCTYPE html>
<html lang="{{ Culture.Name }}">
<head>
    <meta charset="utf-8">
    <meta name="viewport" content="width=device-width, initial-scale=1, shrink-to-fit=no">
    <title>{{ "PageTitle" | shape_new | shape_stringify }}</title>
    {% resources type: "Meta" %}
    {% link type: "image/x-icon", rel: "shortcut icon", href: "~/MyTheme/favicon.ico" %}
    {% style name: "MyTheme" %}
    {% script name: "MyTheme", at: "Foot" %}
    {% resources type: "HeadScript" %}
    {% resources type: "HeadLink" %}
    {% resources type: "Stylesheet" %}
    {% render_section "HeadMeta", required: false %}
</head>
<body dir="{{ Culture.Dir }}">
    <nav class="navbar navbar-expand-lg" id="mainNav">
        <div class="container">
            <a class="navbar-brand" href="{{ '~/' | href }}">{{ Site.SiteName }}</a>
            {% shape "menu", alias: "alias:main-menu" %}
        </div>
    </nav>
    {% render_section "Header", required: false %}
    <div class="container">
        {% render_section "Messages", required: false %}
        {% render_body %}
    </div>
    <footer>
        {% render_section "Footer", required: false %}
    </footer>
    {% resources type: "FootScript" %}
</body>
</html>
```

Some notable replacements:

| In the HTML template | In the theme layout |
| --- | --- |
| `<title>My Site</title>` | `{{ "PageTitle" \| shape_new \| shape_stringify }}` |
| Hardcoded site name | `{{ Site.SiteName }}` |
| `<link href="css/styles.css">` | `{% style name: "MyTheme" %}` |
| `<script src="js/scripts.js">` | `{% script name: "MyTheme", at: "Foot" %}` |
| Static `<ul>` navigation menu | `{% shape "menu", alias: "alias:main-menu" %}` |
| Page-specific markup | `{% render_body %}` |
| Areas meant for reusable blocks | `{% render_section "Footer", required: false %}` |

The `{% resources %}` tags render the stylesheets, scripts, and meta tags that other modules (and your own templates) register at runtime, so keep them even if the layout does not use them directly.

The zones declared with `render_section` must also be listed in the site's zones (a setup recipe can do this with the `settings` step, or they can be edited in the admin under `Design → Settings → Zones`) so that widgets can be placed in them using layers. See [Layers](../../reference/modules/Layers/README.md).

### Styling the menu

The `menu` shape renders the main menu content items as a `<ul>` list. If the template's navigation requires specific CSS classes, override the menu templates in your theme (`Views/Menu.liquid`, `Views/MenuItem.liquid`, `Views/MenuItemLink.liquid`). `TheBlogTheme` contains examples of [these overrides](https://github.com/OrchardCMS/OrchardCore/tree/main/src/OrchardCore.Themes/TheBlogTheme/Views).

## 4. Template the content

With the layout in place, each page's specific markup now has to come from content items. This is where the template's inner pages (a blog post page, an article page, a list page) are converted into content templates.

For each content type, create a template in the `Views` folder named after the [alternate](../../reference/modules/Templates/README.md#available-templates) you want to override. The most common ones:

| File | Renders |
| --- | --- |
| `Content-BlogPost.liquid` | A `BlogPost` content item in detail view |
| `Content-BlogPost.Summary.liquid` | A `BlogPost` content item in a list |
| `Content-Article.liquid` | An `Article` content item in detail view |
| `Widget-Paragraph.liquid` | A `Paragraph` widget |

Copy the corresponding markup from the HTML template into these files, then replace the sample text and images with the content item's fields and parts:

```liquid
<article>
    <h1>{{ Model.ContentItem.DisplayText }}</h1>
    {{ Model.Content.HtmlBodyPart | shape_render }}
</article>
```

Use the [placement](../../reference/modules/Placement/README.md) file (`placement.json`) to control which shapes are rendered, in which order, and to hide the ones the design doesn't need.

To understand what data and shapes are available in a given template, enable the [Shape Tracing](shapes.md) tooling, or browse the templates of the built-in themes.

## 5. Optional: create a setup recipe

If the theme is meant to be reusable, add a [recipe](../../reference/modules/Recipes/README.md) that creates the content types, menu, widgets, layers, and zones the theme expects, and sets it as the default theme. The built-in themes each ship with such a recipe (see `blog.recipe.json` in [the recipes folder](https://github.com/OrchardCMS/OrchardCore/tree/main/src/OrchardCore.Themes/TheBlogTheme/Recipes)) — it is what runs when you select the corresponding option during setup.

## Summary

- Copy the template assets to `wwwroot` and declare them as named resources.
- Convert the main HTML page into `Views/Layout.liquid`, replacing static parts with `render_body`, `render_section` zones, resource tags, and the menu shape.
- Convert the inner pages into content templates using alternates, and tune the output with placement.
- Optionally ship a setup recipe so the theme works out of the box.
