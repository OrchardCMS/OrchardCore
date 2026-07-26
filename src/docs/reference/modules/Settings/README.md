# Settings (`OrchardCore.Settings`)

The `OrchardCore.Settings` module provides the **site settings** infrastructure: a single, per-tenant container of configuration that other modules contribute to. It is always enabled and cannot be disabled.

Site settings are where global, site-wide options live (site name, time zone, page size, …) as opposed to per-content-item data. Modules extend these settings by adding their own groups (for example, the SMTP settings, the search settings, or the reCAPTCHA settings all appear as additional sections of the site settings).

For user-defined settings managed from the admin without writing code, see [Custom Settings](../CustomSettings/README.md). For application-level (`appsettings.json`) configuration, see [Configuration](../Configuration/README.md).

## General settings

The built-in **General** group is available in the admin under **Settings** > **General** (requires the `Manage settings` permission). It exposes options such as:

| Setting             | Description                                                                 |
|---------------------|-----------------------------------------------------------------------------|
| `SiteName`          | The display name of the site.                                               |
| `PageTitleFormat`   | The format used to build the page `<title>` (Liquid).                       |
| `BaseUrl`           | The absolute base URL used when an absolute URL must be generated.          |
| `TimeZoneId`        | The default time zone of the site.                                          |
| `Calendar`          | The default calendar system.                                                |
| `PageSize`          | The default number of items per page.                                       |
| `MaxPageSize`       | The maximum page size a client can request.                                 |
| `MaxPagedCount`     | The maximum number of items that can be paged through.                      |
| `UseCdn` / `CdnBaseUrl` | Whether to serve framework assets from a CDN, and the CDN base URL.     |
| `ResourceDebugMode` | Whether to serve debuggable (non-minified) versions of resources.           |
| `AppendVersion`     | Whether to append a version token to static asset URLs for cache busting.   |
| `CacheMode`         | The default caching behavior for resources.                                 |

A separate **Debugging** group exposes diagnostic options.

## Permissions

| Permission               | Description                                                                  |
|--------------------------|------------------------------------------------------------------------------|
| `Manage settings`        | Grants access to all site settings groups.                                   |
| `Manage group settings`  | Grants access to a specific settings group only. Used to scope access per section. |

Both are granted to the `Administrator` role by default. Modules that add a settings group typically authorize against `Manage group settings` for their own group id.

## Accessing settings in code

Read and write site settings through `ISiteService`:

```csharp
public class MyService
{
    private readonly ISiteService _siteService;

    public MyService(ISiteService siteService) => _siteService = siteService;

    public async Task<string> GetSiteNameAsync()
    {
        var site = await _siteService.GetSiteSettingsAsync();
        return site.SiteName;
    }
}
```

Custom settings classes are stored in the `Properties` bag of the site settings and retrieved with `site.As<TSettings>()`. A module exposes its own group in the admin by implementing a `SiteDisplayDriver` (a display driver for `ISite`) with a matching `GroupId`.

## Accessing settings in templates

In Liquid, the current site settings are available through the `Site` accessor:

```liquid
{{ Site.SiteName }}
{{ Site.PageSize }}
```

## Recipes

Site settings can be set from a recipe using the `settings` step. Recognized keys map to the built-in properties; any other key is stored in the site settings `Properties` bag, which lets modules import their own settings.

```json
{
  "steps": [
    {
      "name": "settings",
      "SiteName": "My Orchard Core Site",
      "PageSize": 10,
      "TimeZoneId": "Europe/Paris",
      "UseCdn": false
    }
  ]
}
```

## Deployment

Use the **Site Settings** deployment step to export and import the site settings between environments as part of a deployment plan.

## Time zone select list customization

The built-in site settings editor, setup screen, and user time zone editor all resolve their time zone `<option>` items through `ITimeZoneSelectListProvider`.

Replace the default `DefaultTimeZoneSelectListProvider` service to change the rendered labels, ordering, or filtering of the available time zones anywhere Orchard Core consumes that shared list.
