---
name: orchardcore-localization
description: Localizes OrchardCore apps — UI strings (IStringLocalizer/IViewLocalizer S, T, H), LocalizationSource, updating and exporting checked-in POT templates, localization analyzers, PO files and pluralization, content translation (LocalizationPart, culture picker), and supported-culture configuration. Use when adding or changing localizable strings, updating POT files, fixing OCLOC diagnostics, translating strings, adding a culture, or making content multilingual.
---

# OrchardCore Localization

This skill guides you through OrchardCore localization following project conventions.

Three layers, often confused:

- **UI/code localization** — translate hard-coded strings in C#/Razor/Liquid via `IStringLocalizer` (`S`), `IViewLocalizer` (`T`), `IHtmlLocalizer` (`H`). Checked-in **POT templates** describe the source strings and are embedded in assemblies; translated **PO files** supply runtime translations.
- **Content localization** — translate *content items* across cultures via `LocalizationPart` and the `OrchardCore.ContentLocalization` module.
- **Culture configuration** — which cultures the site supports and how the request culture is chosen.

## Decide which layer

| Goal | Layer |
|------|-------|
| Translate a label/message in code or a view | UI localization (`S`/`T`/`H`) + PO file |
| Pluralize ("1 item" / "{0} items") | `S.Plural(...)` + PO `msgid_plural` |
| Add, change, remove, or move a source string | Update the owning project's checked-in POT template (Workflow D) |
| Export source strings for Crowdin or another translation platform | Export POT templates (Workflow D), then deploy translated PO files |
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

In Razor: `@T["Welcome to Orchard Core"]`. In a class: `S["A value is required for {0}.", name]`. Keep keys as compile-time constants and pass values as format arguments, not interpolated keys. In Liquid: `{{ "Welcome to Orchard Core" | t }}`.

### Step 2: Pluralize when counting

```csharp
S.Plural(count, "{0} book", "{0} books");
// IHtmlLocalizer: H.Plural(count, "{0} book", "{0} books")
```

### Step 3: Reuse shared sources for strings declared without a localizer

Strings declared before a localizer is available, such as deployment step categories and titles, are `LocalizationSource` instances translated at display time with `IStringLocalizerFactory.Localize()`. Don't declare them in the class that uses them:

- Use `CommonLocalizationSources` (`OrchardCore.Abstractions`, namespace `OrchardCore.Localization`) for text that several projects use with the same meaning, such as `CommonLocalizationSources.ContentManagement`, or that is general to every module, such as `CommonLocalizationSources.Settings`. Look for an existing member before adding one. Add a member only when another project uses the text too; a module name or a category used by one project goes in that project's class.
- Declare the other strings in an `internal static class {Area}LocalizationSources` at the root of the module, for example `LuceneLocalizationSources`, with `LocalizationSource.Create("...", typeof({Area}LocalizationSources))`, which extraction tools can find.
- Keep permission descriptions in their `*Permissions` class, and strings specific to a view in the view, with `T`.

```csharp
[JsonIgnore] // Not inherited from the base property; LocalizationSource can't be serialized.
public override LocalizationSource Category => CommonLocalizationSources.Search;

[JsonIgnore]
public override LocalizationSource Title => LuceneLocalizationSources.LuceneSearchIndexes;
```

The PO context is the class that declares the source, for example `msgctxt "OrchardCore.Localization.CommonLocalizationSources"`.

Resolve a source at display time with `stringLocalizerFactory.Localize(source, args)` or `htmlLocalizerFactory.Localize(source, args)`, using the current tenant's factory. Pass arguments to the factory extension, not to `LocalizationSource.Create`; never cache a translated result in a static field. For plurals, use `stringLocalizerFactory.Plural(count, source, "{0} items")` (or the HTML factory equivalent), with a constant plural form. The singular source must have a context; `count` supplies `{0}` automatically.

### Step 4: Provide PO translations

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

### Step 5: Update and commit the source templates

After adding, changing, removing, or moving a localized string, update the affected project's POT file using Workflow D below. Context changes and moving shared sources can change translation identities even when the displayed text stays the same.

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

## Workflow D: update and export POT templates

### Step 1: Use the existing build integration

In this repository, `OrchardCore.Commons.props` already imports the localization targets for framework projects, modules, and themes. Do not add redundant imports or package references. Outside the repository, each project whose strings need extraction must reference `OrchardCore.Localization.Build` with `PrivateAssets="all"` and a version matching Orchard Core.

Each project's template is `<AssemblyName>.pot`, next to its project file. A **normal build only embeds an existing template** as `<AssemblyName>.Localization.pot`; it neither extracts strings nor refreshes a stale template. A project without a template embeds nothing. `GenerateLocalizationCatalog=true` enables this integration, not automatic extraction.

### Step 2: Regenerate from source

Run commands from the repository root. Prefer updating just the affected project:

```powershell
dotnet msbuild src\OrchardCore.Modules\OrchardCore.Admin\OrchardCore.Admin.csproj -t:UpdateLocalizationCatalog
```

Use `-restore` with `dotnet msbuild` when dependencies need restoring. The target updates only that project's template, not those of its references. For an application and its project references, or the entire solution:

```powershell
dotnet build src\OrchardCore.Cms.Web\OrchardCore.Cms.Web.csproj -p:UpdateLocalizationCatalogs=true
dotnet build OrchardCore.slnx -p:UpdateLocalizationCatalogs=true
```

Choose the appropriate scope, rather than running every command. Use the SDK selected by `global.json`: extraction loads that SDK's Roslyn and Razor compilers. The source-tree worker is built on demand; no separate PoExtractor installation is needed.

Updates create or rewrite the template, or remove it if no messages remain. Unchanged inputs reuse the cache under `obj\`; unchanged output is not rewritten. Do not hand-edit generated entries, headers, or references. Templates use deterministic ordering, UTF-8 and LF, no timestamps, and file references without line numbers by default. Multi-targeted projects extract using their first target framework; `DEBUG` and `TRACE` are excluded during extraction.

### Step 3: Review the result and diagnostics

```powershell
git status --short -- '*.pot'
git --no-pager diff -- '*.pot'
```

Review new files too (a normal `git diff` does not show untracked templates). Check the expected `msgctxt`, `msgid`, `msgid_plural`, source references, format flags, and format-argument comments. Keep `msgstr` values empty in POT files; translations belong in culture-specific PO files. Commit added, modified, and deleted POT files with their source changes. CI updates the solution's templates and fails if any are uncommitted or out of date.

Extraction covers C#, Razor, literal Liquid `t` inputs, context-bearing `LocalizationSource` declarations, and explicit constant DataAnnotations validation messages. It does not extract JavaScript/TypeScript directly: expose their strings through a C# `IJSLocalizer` implementation. Fix dynamic keys or unresolved contexts at the source instead of inserting guessed POT entries.

IDE/build analyzers report `OCLOC101` (constant source missing a context), `OCLOC102` (different declaring context, informational), `OCLOC103` (nonconstant source value), and `OCLOC104` (nonconstant localizer key or plural forms). The update worker reports `OCLOC001`-`OCLOC009`; see the reference for their meanings. Extraction errors always fail; `-p:LocalizationStrict=true` also fails on extraction warnings.

Use `[SkipLocalizationExtraction]` only on forwarding infrastructure whose keys are declared elsewhere; it excludes marked classes/methods and nested declarations from extraction and analyzer diagnostics, not application calls to those helpers. For an intentionally dynamic application call, locally suppress `OCLOC104` with an explanation of where its keys originate. Do not hide missing application strings with broad suppressions.

### Step 4: Export templates when requested

Build with an **absolute**, dedicated output directory to collect existing templates without regenerating them:

```powershell
$output = Join-Path $PWD 'artifacts\localization'
dotnet build OrchardCore.slnx "-p:LocalizationCatalogOutputPath=$output"
```

Files are copied to `<output>\<TargetFramework>\<AssemblyName>.pot`. Add `-p:UpdateLocalizationCatalogs=true` only when an update is also intended. Use an empty export directory for an exact current snapshot; export does not remove stale files. Only built projects contribute templates, not prebuilt NuGet dependencies. Upload POT files to the translation platform, preserve contexts/plurals/placeholders, and download translated PO files to a supported runtime localization location. Embedded POT files alone do not change runtime translations.

For configuration properties, custom Liquid inputs, extraction diagnostics, and library entry points, see `references/localization.md`.

## Quick Reference

### Localizers

| Symbol | Type | Output |
|--------|------|--------|
| `S` | `IStringLocalizer<T>` | plain string |
| `T` | `IViewLocalizer` | encoded view string |
| `H` | `IHtmlLocalizer<T>` | trusted translation markup; format arguments are HTML-encoded |

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
- Ordinary builds do not refresh POT templates; always explicitly update and commit them with string/context changes.
- A gettext source plural has exactly two constant forms; translated PO files can have more forms, selected by the culture's plural rule.
- Set `GenerateLocalizationCatalog=false` only when intentionally disabling both embedding and updating, not as a fix for extraction errors.

## References

- `references/localization.md` — POT configuration and diagnostics, extraction APIs, PO discovery order, contexts, plural forms, content localization internals, startup wiring
- `src/docs/reference/modules/Localize/README.md` (repo)
- `src/docs/reference/modules/ContentLocalization/README.md` (repo)
- `src/docs/reference/modules/DataLocalization/README.md` (repo)
- `src/docs/reference/modules/Localize/javascript-localization.md` (repo) — `IJSLocalizer` bridge; no direct JavaScript extraction
- `AGENTS.md` (repo root) — build commands
