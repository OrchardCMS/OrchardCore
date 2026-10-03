---
name: orchardcore-localization
description: Localizes OrchardCore apps — UI strings (IStringLocalizer/IViewLocalizer S, T, H), PO files and pluralization, content translation (LocalizationPart, culture picker), and supported-culture configuration. Use when the user needs to translate strings, add a culture, set up PO files, or make content items multilingual.
---

# OrchardCore Localization

This skill guides you through OrchardCore localization following project conventions.

Three layers, often confused:

- **UI/code localization** — translate hard-coded strings in C#/Razor/Liquid via `IStringLocalizer` (`S`), `IViewLocalizer` (`T`), `IHtmlLocalizer` (`H`). Translations live in **PO files**.
- **Content localization** — translate *content items* across cultures via `LocalizationPart` and the `OrchardCore.ContentLocalization` module.
- **Culture configuration** — which cultures the site supports and how the request culture is chosen.

## Decide which layer

| Goal | Layer |
|------|-------|
| Translate a label/message in code or a view | UI localization (`S`/`T`/`H`) + PO file |
| Pluralize ("1 item" / "{0} items") | `S.Plural(...)` + PO `msgid_plural` |
| Translate a blog post into French | Content localization (`LocalizationPart`) |
| Add a supported language / set default | Localization settings (`LocalizationSettings`) |
| Translate data-driven values (e.g. taxonomy terms) | Data Localization (`DataLocalization` module) |

## Workflow A: localize UI strings

### Step 1: Inject the right localizer

| Localizer | Convention | Where |
|-----------|-----------|-------|
| `IStringLocalizer<T>` | field `S` | drivers, services, controllers |
| `IViewLocalizer` | property `T` | Razor views (already on `RazorPage`) |
| `IHtmlLocalizer<T>` | field `H` | when the translation contains HTML |

```csharp
public sealed class AliasPartDisplayDriver : ContentPartDisplayDriver<AliasPart>
{
    internal readonly IStringLocalizer S;

    public AliasPartDisplayDriver(IStringLocalizer<AliasPartDisplayDriver> localizer)
        => S = localizer;
}
```

In Razor: `@T["Welcome to Orchard Core"]`. In a class: `S["A value is required for {0}.", name]`.

### Step 2: Pluralize when counting

```csharp
S.Plural(count, "{0} book", "{0} books");
// IHtmlLocalizer: H.Plural(count, "{0} book", "{0} books")
```

### Step 3: Provide PO translations

PO files are discovered (in order) from each module's `Localization/`, a global `/Localization`, the tenant's `App_Data/Sites/{tenant}/Localization`, and culture folders. A PO entry:

```po
msgctxt "OrchardCore.Admin.Views.AdminDashboard"
msgid "Welcome to Orchard Core"
msgstr "Bienvenue sur Orchard Core"
```

`msgctxt` = `{Namespace}.{Class}` (services) or `{Namespace}.{ViewPath}` (views). Plurals:

```po
msgctxt "OrchardCore.AdminMenu.Views.Menu.List"
msgid "1 item"
msgid_plural "{0} items"
msgstr[0] "{0} élément"
msgstr[1] "{0} éléments"
```

See `references/localization.md` for the full PO discovery order and headers.

## Workflow B: localize content

### Step 1: Enable the feature, attach the part

Enable `OrchardCore.ContentLocalization` (Content Localization). Attach `LocalizationPart` to the content type (admin or recipe). It carries:

```csharp
public class LocalizationPart : ContentPart, ILocalizable
{
    public string LocalizationSet { get; set; } // links all translations
    public string Culture { get; set; }         // this version's culture
}
```

On create, the part handler generates a unique `LocalizationSet` and sets the default culture automatically.

### Step 2: Translate

From the editor's Localization, "Translate" creates a sibling content item sharing the same `LocalizationSet` with a different `Culture`. Programmatically:

```csharp
var translated = await _contentLocalizationManager.LocalizeAsync(contentItem, "fr-FR");
```

### Step 3: Surface translations / culture picker

`IContentLocalizationManager` fetches across a set:

```csharp
await _contentLocalizationManager.GetContentItemAsync(localizationSet, "fr");
await _contentLocalizationManager.GetItemsForSetAsync(localizationSet);
```

Liquid: `{{ Model.ContentItem.Content.LocalizationPart.LocalizationSet | localization_set: "en" }}`. The Content Culture Picker feature adds a front-end language switcher and a `ContentRequestCultureProvider` that derives the culture from the localized route.

## Workflow C: configure cultures

Recipe `settings` step:

```json
{
  "name": "settings",
  "LocalizationSettings": {
    "DefaultCulture": "en-US",
    "SupportedCultures": [ "en-US", "fr-FR", "es-ES" ],
    "FallBackToParentCulture": true
  }
}
```

The Localization module reads these at startup and calls `app.UseRequestLocalization(...)` with the configured default + supported cultures.

## Quick Reference

### Localizers

| Symbol | Type | Output |
|--------|------|--------|
| `S` | `IStringLocalizer<T>` | plain string |
| `T` | `IViewLocalizer` | encoded view string |
| `H` | `IHtmlLocalizer<T>` | HTML-safe string |

### Content localization APIs

| Member | Purpose |
|--------|---------|
| `IContentLocalizationManager.LocalizeAsync(item, culture)` | create a translation |
| `.GetContentItemAsync(set, culture)` | one translation |
| `.GetItemsForSetAsync(set)` | all translations |
| `.GetItemsForSetsAsync(sets, culture)` | batch |
| `ILocalizationService.GetDefaultCultureAsync()` / `GetSupportedCulturesAsync()` | culture config |

### Request culture providers

| Provider | Picks culture from |
|----------|--------------------|
| `ContentRequestCultureProvider` | the localized content route (added first) |
| `AdminCookieCultureProvider` | `admin_culture_*` cookie, admin paths only |
| cookie / querystring / Accept-Language | ASP.NET Core defaults |

### Liquid filters

`localization_set: "fr"` — translated item; `switch_culture_url` — culture-switch URL.

## Gotchas

- `msgctxt` must exactly match the namespace+class (or namespace+view path). Wrong context → string stays untranslated.
- `IStringLocalizer<T>` context comes from `T`'s type — inject the localizer typed to the class that owns the strings.
- `LocalizationSet` links translations; never share one set across unrelated items, and don't overwrite it.
- Add a culture to `SupportedCultures` before content can be translated into it.
- DataAnnotations need `AddDataAnnotationsPortableObjectLocalization()` (the Localization module registers it).

## References

- `references/localization.md` — PO discovery order, contexts, plural forms, content localization internals, startup wiring
- `src/docs/reference/modules/Localize/README.md` (repo)
- `src/docs/reference/modules/ContentLocalization/README.md` (repo)
- `src/docs/reference/modules/DataLocalization/README.md` (repo)
- `AGENTS.md` (repo root) — build commands
