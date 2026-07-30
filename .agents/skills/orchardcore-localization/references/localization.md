# Localization reference

## The three layers

| Layer | Module | Translates | Storage |
|-------|--------|-----------|---------|
| UI/code | `OrchardCore.Localization` | hard-coded strings | PO files |
| Content | `OrchardCore.ContentLocalization` | content items | content DB (`LocalizationPart`) |
| Data | `OrchardCore.DataLocalization` | data-driven values | DB-backed translations |

## Localizers and conventions

```csharp
internal readonly IStringLocalizer S;   // IStringLocalizer<ThisClass>
protected readonly IHtmlLocalizer H;    // IHtmlLocalizer<ThisClass>
```

- `S` — services, drivers, handlers, controllers. `S["Text {0}", arg]`.
- `T` — `IViewLocalizer`, already exposed on `RazorPage`/`RazorPage<TModel>` as the `T` property; `@T["..."]` in any view.
- `H` — when the message contains markup that must not be HTML-encoded.

Context is the localizer's generic type for `S`/`H`, or the view's path for `T`.

## Pluralization

```csharp
S.Plural(count, "{0} book", "{0} books");
H.Plural(count, "{0} book", "{0} books");
```

Backed by `IPluralStringLocalizer.GetTranslation(name, args)` returning the chosen plural form. PO file:

```po
msgctxt "MyModule.MyClass"
msgid "{0} book"
msgid_plural "{0} books"
msgstr[0] "{0} livre"
msgstr[1] "{0} livres"
```

Plural-Forms header in the PO sets how many forms and the selection expression, e.g. Arabic uses `nplurals=6`.

## PO file discovery order

`ModularPoFileLocationProvider` yields, for a culture `fr-CA`:

1. `{Extension}/Localization/fr-CA.po` — per module, in extension order.
2. `/Localization/fr-CA.po` — global.
3. `App_Data/Sites/{tenant}/Localization/fr-CA.po` — tenant-specific.
4. NuGet-package overrides: `/Localization/{ExtensionId}/fr-CA.po`, `/Localization/{ExtensionId}-fr-CA.po`, `/Localization/fr-CA/{ExtensionId}.po`.
5. Every `*.po` in `/Localization/fr-CA/`.

Resources path is configured by `AddPortableObjectLocalization(o => o.ResourcesPath = "Localization")`.

## msgctxt rules

| Source | Context |
|--------|---------|
| Service / driver / class | `{Namespace}.{ClassName}` — e.g. `OrchardCore.Admin.AdminMenu` |
| Razor view | `{Namespace}.{ViewPathWithoutExtension}` — e.g. `OrchardCore.Admin.Views.AdminDashboard`, `TheAdmin.Views.Layout` |

The context must match exactly or the lookup misses and the `msgid` is shown verbatim.

## Configuring cultures

```csharp
public class LocalizationSettings
{
    public string DefaultCulture { get; set; }
    public string[] SupportedCultures { get; set; }
    public bool FallBackToParentCulture { get; set; }
}
```

`ILocalizationService`: `GetDefaultCultureAsync()`, `GetSupportedCulturesAsync()`, `GetAllCulturesAndAliases()`, `FallBackToParentCultures`.

Startup applies them:

```csharp
localizationOptions
    .SetDefaultCulture(defaultCulture)
    .AddSupportedCultures(supportedCultures)
    .AddSupportedUICultures(supportedCultures);
app.UseRequestLocalization(localizationOptions);
```

## Content localization internals

```csharp
public class LocalizationPart : ContentPart, ILocalizable
{
    public string LocalizationSet { get; set; }
    public string Culture { get; set; }
}
```

- `LocalizationPartHandler.CreatingAsync` generates `LocalizationSet` (unique id) and defaults `Culture` to the site default if unset.
- `LocalizedContentItemIndex` (`MapIndex`) indexes `ContentItemId`, `LocalizationSet`, `Culture`, `Published`, `Latest` for cross-culture queries.

`IContentLocalizationManager`:

```csharp
Task<ContentItem> GetContentItemAsync(string localizationSet, string culture);
Task<IEnumerable<ContentItem>> GetItemsForSetAsync(string localizationSet);
Task<ContentItem> LocalizeAsync(ContentItem content, string targetCulture);
Task<IEnumerable<ContentItem>> GetItemsForSetsAsync(IEnumerable<string> sets, string culture);
```

## Culture providers

- `ContentRequestCultureProvider` — resolves culture from the localized route; added with `options.AddInitialRequestCultureProvider(...)` so it runs first.
- `AdminCookieCultureProvider : CookieRequestCultureProvider` — cookie `admin_culture_{VersionId}`, applies only on admin paths; returns null elsewhere.

## Liquid filters

| Filter | Result |
|--------|--------|
| `localization_set: "fr"` | translated content item(s) for a `LocalizationSet`; nil culture → current request culture |
| `switch_culture_url` | URL that switches to a given culture |

## Recipe steps

UI cultures — `settings` step with `LocalizationSettings` (above).

Content culture picker — `settings` step:

```json
{
  "name": "settings",
  "ContentCulturePickerSettings": { "RedirectToHomepage": false, "SetCookie": true },
  "ContentRequestCultureProviderSettings": { "SetCookie": false }
}
```

## Startup wiring

```csharp
services.AddPortableObjectLocalization(o => o.ResourcesPath = "Localization")
        .AddDataAnnotationsPortableObjectLocalization();
services.Replace(ServiceDescriptor.Singleton<ILocalizationFileLocationProvider, ModularPoFileLocationProvider>());
services.AddScoped<ILocalizationService, LocalizationService>();
```
