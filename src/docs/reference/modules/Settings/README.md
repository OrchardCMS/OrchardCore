# Settings (`OrchardCore.Settings`)

The `OrchardCore.Settings` module provides the **site settings** infrastructure: a single, per-tenant container of configuration that other modules contribute to. It is always enabled and cannot be disabled.

Site settings are where global, site-wide options live (site name, time zone, page size, …) as opposed to per-content-item data. Modules extend these settings by adding their own groups (for example, the SMTP settings, the search settings, or the reCAPTCHA settings all appear as additional sections of the site settings).

For user-defined settings managed from the admin without writing code, see [Custom Settings](../CustomSettings/README.md). For application-level (`appsettings.json`) configuration, see [Configuration](../Configuration/README.md).

For bearer-authenticated automation, see the [site settings management API](../../api/settings/README.md).

## General settings

The built-in **General** group is available in the admin under **Settings** > **General** (requires the `Manage general settings` permission). It exposes options such as:

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

| Permission                  | Description                                                               |
|-----------------------------|---------------------------------------------------------------------------|
| `Manage settings`           | Grants both built-in settings permissions for backward compatibility.     |
| `Manage general settings`   | Grants access to the built-in General settings group.                      |
| `Manage debugging settings` | Grants access to the built-in Debugging settings group.                    |
| `Manage group settings`     | Internal resource permission used to authorize registered and custom settings groups. |

The assignable permissions are granted to the `Administrator` role by default. Modules that contribute a settings group must register the permission that grants access to the group and enforce the same permission in both the edit and update methods of their display driver:

```csharp
services
    .AddSiteDisplayDriver<MySettingsDisplayDriver>()
    .AddSiteSettingsPermission(MySettingsDisplayDriver.GroupId, Permissions.ManageMySettings);
```

Registering the group permission lets the shared settings controller authorize the route without requiring the broader `Manage settings` permission. The display-driver checks remain necessary to prevent unauthorized settings from rendering or updating when multiple drivers contribute to the same group.

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

This accessor has all properties of `ISite`, with two security limitations:

1. `ISite.SiteSalt` may not be accessed. If you try to use `{{ Site.SiteSalt }}` in Liquid, you will get `[REDACTED]` instead of the actual value.
2. `ISite.Properties` is filtered. By default, this object appears empty in Liquid. You can configure the `OrchardCore.Settings.SettingsLiquidOptions` object to specify which properties should be exposed through Liquid. For example using _appsettings.json_:
   ```json
   {
     "OrchardCore": {
       "OrchardCore_Settings_Liquid": {
         "PermittedSiteProperties": [
           "ExternalRegistrationSettings",
           "ExternalLoginSettings",
           "CurrentThemeName",
           "CurrentAdminThemeName",
           "LayerSettings"
         ]
       }
     }
   }
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
