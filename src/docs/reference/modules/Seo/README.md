# SEO (`OrchardCore.Seo`)

Provides Search Engine Optimization (SEO) features:

- Meta description, keywords, robots, and custom meta tags
- Canonical URL
- [Open Graph](https://ogp.me/) metadata
- [Twitter Card Tags](https://developer.twitter.com/en/docs/twitter-for-websites/cards/overview/markup)
- [Google schema](https://developers.google.com/search/docs/advanced/structured-data/intro-structured-data)

## `robots.txt` File
Starting at version 1.7, the feature of creating a robots.txt file via site settings was introduced. This feature allows website owners to easily define the directives for search engine crawlers and other web robots accessing their site. By default, the following settings are provided in the robots.txt file:

    User-agent: *
        This directive applies to all web robots.
    Disallow: /Admin/
        This directive specifies that web robots should not access the /admin directory, which is commonly used for administrative purposes.

These default settings aim to provide a basic configuration that ensures search engines can access the necessary files and directories while restricting access to sensitive areas of the site. However, website owners can modify these settings according to their specific requirements by navigating to the admin dashboard then **Configuration** >> **Settings** >> **SEO**.

!!! note
    If the [Sitemaps](../Sitemaps) feature is enabled, all sitemap indexes and sitemaps are added to the `robots.txt` by default. 

!!! warning
    If the site's [filesystem](../Tenants/#static-file-provider-feature) contains a `robots.txt`, this file will take precedence and the site settings to generate the files will be ignored.

## Video

<iframe width="560" height="315" src="https://www.youtube-nocookie.com/embed/bDf96bg-mBU" title="YouTube video player" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture" allowfullscreen></iframe>

<iframe width="560" height="315" src="https://www.youtube-nocookie.com/embed/IchtAdYQF7g" title="YouTube video player" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture; web-share" allowfullscreen></iframe>