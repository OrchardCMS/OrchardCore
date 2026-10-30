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

## POT template build integration

The canonical documentation is `src\docs\reference\modules\Localize\README.md`, under **Translation templates**. `SKILL.md` contains the normal update/review/export workflow. The implementation lives in `src\OrchardCore\OrchardCore.Localization.Build\OrchardCore.Localization.Build.props` and `.targets`.

- `<AssemblyName>.pot` is a checked-in source artifact next to the project file, not a translated PO file or an `obj\` output.
- A normal build embeds an existing template as `<AssemblyName>.Localization.pot`. It does not extract or update strings and does not require the worker to be built.
- `UpdateLocalizationCatalog` updates one project. `dotnet build -p:UpdateLocalizationCatalogs=true` updates the projects being built, including project references.
- `OrchardCore.Commons.props` imports the targets in this repository, excluding compiler tooling projects. Outside the repository, each participating project references `OrchardCore.Localization.Build` with `PrivateAssets="all"` and the matching Orchard Core version.
- The worker targets `DefaultTargetFramework` from `src\OrchardCore.Build\TargetFrameworks.props`. Its runtime (or a later major version) must be installed even if a consuming project targets .NET Standard. It loads Roslyn and Razor from the SDK driving the build; Roslyn must be at least 5.9 (SDK 10.0.4xx). Do not add application runtime dependencies on the extraction tooling.
- Updates use `.request` and `.stamp.json` files under `obj\<Configuration>\<TargetFramework>\Localization\`. These are build artifacts, not files to commit. `clean` removes the cache, not the checked-in template.
- Templates are deterministic, UTF-8 without BOM, LF, sorted, and timestamp-free. Format metadata includes `csharp-format` and `#. Format arguments:` comments where applicable. Preserve these along with contexts and plural forms when uploading templates.
- CI in `.github\workflows\pr_ci.yml` and `main_ci.yml` updates templates and checks `git status --porcelain -- '*.pot'`, including additions and deletions. An ordinary successful build is not proof that templates are current.

### Build properties and targets

| Property / target | Behavior |
|-------------------|----------|
| `GenerateLocalizationCatalog` | Defaults to `true`; `false` disables both embedding and updating. |
| `UpdateLocalizationCatalogs` | Defaults to `false`; `true` updates templates during builds. |
| `UpdateLocalizationCatalog` | Explicit target to update the current project, using the first framework of a multi-targeted project. |
| `LocalizationCatalogPath` | Template path, defaulting to `<project directory>\<AssemblyName>.pot`. |
| `LocalizationCatalogLineNumbers` | Defaults to `false`; `true` adds source line numbers and creates more diff churn. |
| `LocalizationStrict` | Defaults to `false`; `true` makes extraction warnings fail updates. Errors always fail. |
| `LocalizationCatalogOutputPath` | Copies templates to `<directory>\<TargetFramework>\<AssemblyName>.pot`. Use an absolute path shared by all projects; does not regenerate templates or remove obsolete exports. |
| `LocalizationViewPrefix` | View context prefix; modules/themes default to their assembly name. |
| `LocalizationToolPath` / `LocalizationDotNetHostPath` | Override the extraction worker DLL / dotnet host. |
| `LocalizationRoslynDirectory` / `LocalizationRazorCompilerPath` | Override SDK compiler locations when automatic discovery is insufficient. |
| `GetLocalizationCatalogs` | Returns `@(LocalizationCatalog)` for this project, with `AssemblyName`, `LogicalName`, and `TargetFramework` metadata. Does not aggregate references. |

Do not set a single shared `LocalizationCatalogPath` for multiple projects: they own separate templates. Export to a dedicated directory instead; projects with the same assembly name and target framework still need separate destinations. Exporting an application collects only templates from built projects, not templates embedded in prebuilt NuGet assemblies.

### Extraction inputs and limits

- C#: semantic calls to string/HTML/view localizers, supported factory calls and plural helpers, and context-bearing `LocalizationSource` declarations, including statically resolvable references. Keep singular and plural forms constant and provide exactly two source plural forms for gettext.
- Razor: views/pages are compiled with the SDK's Razor compiler. Their generated code is extracted, but is not checked by the IDE localization analyzers.
- Liquid: literal inputs to the `t` filter are parsed with the existing Liquid parser without rendering templates or running module startup. Inputs come from evaluated embedded-resource, content, and none items. Additional inputs can be declared with a `LocalizationLiquid` MSBuild item. Custom Liquid syntax needs a configured parser when calling the extraction library directly.
- DataAnnotations: explicit constant `ValidationAttribute.ErrorMessage` values (including derived attributes) use `OrchardCore.Localization.DataAnnotations.DataAnnotationsDefaultErrorMessages`. Applicable member/display-name `{0}` fallback messages are included. Resource-backed messages, display labels by themselves, and messages assigned in custom attribute constructors are not inferred. Default framework messages come from the existing marker catalog.
- JavaScript/TypeScript: no direct extraction. Declare browser strings in a C# `IJSLocalizer` implementation, which supplies translated values to the frontend.
- Multi-targeted projects use their first framework. `DEBUG` and `TRACE` are undefined during extraction, so configuration-only strings under those symbols are omitted.

`SkipLocalizationExtractionAttribute` is declared in `OrchardCore.Abstractions`, in the `OrchardCore.Localization` namespace. Applied to a class or method, it excludes nested declarations/lambdas too; marked partial classes are excluded across parts. It is not inherited by derived classes or overrides, and unmarked callers of marked helpers are still extracted. Reserve it for implementations/forwarders, not application strings.

### Diagnostics

IDE/build analyzers ship in `OrchardCore.Abstractions`; extraction diagnostics are reported only when updating templates. Suppressing an analyzer does not make a dynamic key extractable.

| Code | Severity | Action |
|------|----------|--------|
| `OCLOC101` | Warning | A constant `LocalizationSource` lacks a context. Use `Create<T>` or `Create(value, typeof(T))` for a static type; a code fix is available. |
| `OCLOC102` | Info | A source uses a type other than its declaring/enclosing type. Confirm that the shared context is intentional. |
| `OCLOC103` | Warning | A source value is not constant. Declare the key where it originates; mark only genuine forwarding infrastructure to skip extraction. |
| `OCLOC104` | Warning | A localizer key or plural form is not constant. Use constant keys/forms; locally suppress only intentionally dynamic calls and document their source. |
| `OCLOC001` | Warning | Extraction cannot resolve a constant key or source value. |
| `OCLOC002` | Warning | Extraction cannot resolve a localizer context or source type; use a typed localizer, `Create<T>`, or `typeof`. |
| `OCLOC003` | Warning | A plural forms array does not contain exactly two constant source forms. |
| `OCLOC004` | Warning | An empty key was used; empty `msgid` is reserved for the gettext header. |
| `OCLOC005` | Error | One context/key has conflicting singular/plural declarations. Fix the declarations, not the generated template. |
| `OCLOC006` | Error or warning | Razor compiler discovery/loading/compilation failed; inspect the underlying diagnostic. |
| `OCLOC007` | Error | Liquid parsing failed. |
| `OCLOC008` | Error or warning | C# extraction compilation or a source generator failed, or a generator assembly could not be loaded. |
| `OCLOC009` | Error | Missing worker/SDK compiler, cancellation, or unexpected extraction failure. |
| `OCLOC010` | Warning | A translator note or argument name is not constant (C#) or literal (Liquid); the message is extracted without it. |
| `OCLOC011` | Warning | An `Arg(name, value, min, max)` range is not constant, not `0 <= min <= max`, or not on a `Plural` count; the message is extracted without the `range` flag. |

Use `LocalizationStrict=true` to fail on extraction warnings. IDE analyzer severities can be configured in `.editorconfig`; `dotnet build -warnaserror` promotes analyzer warnings. Do not globally suppress diagnostics just to obtain a successful build.

### Programmatic extraction and verification

`OrchardCore.Localization.Extraction` provides `LocalizationExtractor`, `ExtractionOptions`, `LocalizationCatalog`, and `PotWriter` for tooling. `OrchardCore.Localization.Tools` is the command-line build worker; normal consumers use the build package/targets, not its request-file protocol directly.

When changing extraction or build targets, run:

```powershell
dotnet test --project test\OrchardCore.Localization.Extraction.Tests\OrchardCore.Localization.Extraction.Tests.csproj
```

For analyzer changes, run the main tests with `--filter-class '*AnalyzerTests'`. For routine application string edits, update the affected POT files and review their complete diff; do not run the entire extraction suite unnecessarily. Repeating the update with unchanged sources should leave the templates unchanged.

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

Plural-Forms header in the PO sets how many forms and the selection expression, e.g. Arabic uses `nplurals=6`. Orchard Core evaluates it (`PluralFormsParser`) for the translations of that file; files without a valid header use the culture's `IPluralRuleProvider` rule.

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
