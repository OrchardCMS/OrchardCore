# Sitemaps (`OrchardCore.Sitemaps`)

The sitemaps module provides automatic generation of sitemaps.

It supports creation of XML Sitemap files and XML Sitemap Index files conforming to a standard sitemap protocol.

For more information about sitemaps see [sitemaps.org](https://www.sitemaps.org/)

## General Concepts

Sitemaps are configured by creating a Sitemap and adding Sitemap Sources.

Sitemap Indexes are configured by creating a Sitemap Index and selecting which Sitemaps are contained within the index.

## How to create a Sitemap

- Ensure the Sitemaps feature is enabled.

- Go to _Configuration -> SEO -> Sitemaps_

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

To clear the cache manually use the _Configuration -> SEO -> Sitemaps Cache_ feature.

## `robots.txt` File.

When both `SEO` and `Sitemaps` features are enabled and no `robots.txt` file is found on the filesystem, the sitemap indexes and sitemaps are added to the `robots.txt` file by default. This can be changed by navigating to **Configuration** >> **Settings** >> **SEO**.

## Video

<iframe width="560" height="315" src="https://www.youtube-nocookie.com/embed/fG_rFD0wffw" frameborder="0" allow="accelerometer; autoplay; encrypted-media; gyroscope; picture-in-picture" allowfullscreen></iframe>

## CREDITS

### IDeliverable.Seo

<https://github.com/IDeliverable/IDeliverable.Seo>  

Copyright (c) IDeliverable, Ltd. 

BSD-3
