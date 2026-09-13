# Sitemaps (`OrchardCore.Sitemaps`)

The sitemaps module provides automatic generation of sitemaps.

It supports creation of XML Sitemap files and XML Sitemap Index files conforming to a standard sitemap protocol.

For more information about sitemaps see [sitemaps.org](https://www.sitemaps.org/)

## General Concepts

Sitemaps are configured by creating a Sitemap and adding Sitemap Sources.

Sitemap Indexes are configured by creating a Sitemap Index and selecting which Sitemaps are contained within the index.

## How to create a Sitemap

- Ensure the Sitemaps feature is enabled.

- Go to _Tools -> Search Engine Optimization -> Sitemaps_

- Create a new Sitemap.

- Set the path for the Sitemap, note that the path must end in `.xml`

- Give it a name.

- Select Edit.

- Add a Sitemap Source to it.

- The sitemap can now be browsed to and will be served on the specified path.

## Sitemap Content Types Source

The Content Types Source will provide a sitemap for your content items,
on a per Content Type basis.

You can choose to Index All Content Types, or specify the Content Types.

You may also select the default Priority, and Change Frequency, either for all Content Types,
or individual Content Types.

You may also choose to Limit Items.

The Limit Items option is generally used in combination with a Sitemap Index to limit the size of Sitemaps,
and make maintaining the Sitemap easier.

Google and Bing limit the size of a sitemap to either 50,000 items, or 10MB,
whichever is reached first.

If you need to limit the quantity of Content Items in a Sitemap

- Uncheck Index All Content Types.

- Check Limit items.

- Select the Content Type to index.

- Choose to Skip `x` number of Content Items and Take `x` number of Content Items.

For the remaining Content Items, create another Sitemap, and repeat choosing different values for Skip and Take as appropriate.

For other Content Types, create another Sitemap and include all these Sitemaps in a Sitemap index.

!!! note
    The only content types listed for inclusion on a sitemap are those with the `AutoroutePart` attached.
    To include content items routed without `Autoroute` implement a `IRouteableContentTypeProvider`

## SitemapPart

Add the SitemapPart to a Content Type to provide sitemap configuration at a Content Item level.

Settings here can override any Sitemap configuration.

- Check to override the Sitemap configuration.

- Exclude the Content Item.

- Alter the Priority.

- Alter the Change Frequency.

!!! note
    You do not have to add the SitemapPart to a Content Type for it to be part of a Sitemap.

## Localized Sitemaps

To support the google hreflang sitemap extensions, enable the Localized Content Items Sitemap feature.

This will automatically include any localized content items in your sitemap.

Refer [Google Sitemap Extensions](https://support.google.com/webmasters/answer/189077) for more information
on this protocol.

## Decoupled Razor Pages

To include Content Types displayed with Razor Pages, enable the Sitemaps for Decoupled Razor Pages feature.

In your `Program.cs`, configure the `SitemapsRazorPagesOptions` to support the routes for your Content Types.

```csharp
builder.Services.Configure<SitemapsRazorPagesOptions>(options =>
{
    options.ConfigureContentType("DecoupledBlogPost", o =>
    {
        o.PageName = "DecoupledBlogPost";
        o.RouteValues = (contentItem) => new { area = "OrchardCore.Sitemaps", slug = contentItem.ContentItemId };
    });
});    
```

!!! note
    Be sure to include the area in the route values.

## Sitemap Cache

Sitemaps are cached on a tenant by tenant basic in the `wwwroot/sm-cache` folder.

The cache is automatically cleared when content items are published.

To clear the cache manually use the _Tools -> Search Engine Optimization -> Sitemaps Cache_ feature.

## `robots.txt` File

When both `SEO` and `Sitemaps` features are enabled and no `robots.txt` file is found on the filesystem, the sitemap indexes and sitemaps are added to the `robots.txt` file by default. This can be changed by navigating to **Settings** → **Search** → **Search Engine Optimization** → **Robots**.

## Recipe Configuration

Sitemaps robots settings can be configured using the `Settings` recipe step:

```json
{
  "steps": [
    {
      "name": "settings",
      "SitemapsRobotsSettings": {
        "IncludeSitemaps": true
      }
    }
  ]
}
```

| Property          | Type    | Description                                                              |
|-------------------|---------|--------------------------------------------------------------------------|
| `IncludeSitemaps` | Boolean | Whether to include sitemap URLs in the robots.txt file. Default: `true`. |

## Video

<iframe width="560" height="315" src="https://www.youtube-nocookie.com/embed/fG_rFD0wffw" frameborder="0" allow="accelerometer; autoplay; encrypted-media; gyroscope; picture-in-picture" allowfullscreen></iframe>

## CREDITS

### IDeliverable.Seo

<https://github.com/IDeliverable/IDeliverable.Seo>  

Copyright (c) IDeliverable, Ltd.

BSD-3

## Remote administration

With Remote Management enabled, sitemap administration is available through
OpenAPI, Pomi and MCP. Operations require API bearer authentication,
`AccessRemoteManagement`, and `ManageSitemaps`.

```sh
pomi sitemaps list
pomi sitemaps show <id>
pomi sitemaps create --stdin < sitemap.json
pomi sitemaps update <id> --stdin < sitemap.json
pomi sitemaps disable <id>
pomi sitemaps enable <id>
pomi sitemaps delete <id> --force
pomi sitemaps sources types
pomi sitemaps sources schema CustomPathSitemapSource
pomi sitemaps sources list <id>
pomi sitemaps sources create <id> --stdin < source.json
pomi sitemaps sources update <id> <sourceId> --stdin < source.json
pomi sitemaps sources delete <id> <sourceId> --force
```

A sitemap definition contains `name`, `path`, `kind` (`Sitemap` or `SitemapIndex`),
`enabled`, and `containedSitemapIds`. An omitted/empty path derives a slug ending
in `.xml`. Updates replace metadata and retain sources. Indexes accept distinct
existing regular sitemap IDs and cannot contain other indexes. Existing admin
path validation and the sitemap manager are shared with the API. Child changes
and deletion also invalidate referencing index caches.

A source write contains `type` and a complete `configuration` object. Registered
`CustomPathSitemapSource` and `ContentTypesSitemapSource` contracts are supported.
Use `sources schema` for their fields; change frequencies use enum names and
priorities range from zero to ten. IDs and update timestamps are server-owned.
Unknown source extensions expose identity only. Invalid writes preserve existing
sources. Source updates regenerate the sitemap cache identifier.

```json
{
  "type": "CustomPathSitemapSource",
  "configuration": { "path": "/contact", "priority": 5, "changeFrequency": "Monthly" }
}
```

Create operations generate IDs. After an uncertain create response, list/read back
before retrying to avoid duplicate sources. Identical updates and status changes
avoid unnecessary writes; deleting an already absent source is unchanged.

When SEO is also enabled, `settings sections show/schema/update sitemaps-robots`
exposes `includeSitemaps` using `ManageSeoSettings`. The existing robots provider
requires a site `baseUrl`. A physical `robots.txt` still overrides generated output.
Verify the public `.xml` and `robots.txt` responses after configuration changes.
