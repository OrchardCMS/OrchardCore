# Localization (`OrchardCore.Localization`)

This module provides the infrastructure necessary to support the PO (Portable Object) localization file format.  
It also supports plural forms.

## Online translations

[![Crowdin](https://badges.crowdin.net/orchard-core/localized.svg)](https://crowdin.com/project/orchard-core)

The localization files for the different cultures are available on [Crowdin](https://crowdin.com/project/orchard-core).

## PO files locations

PO files are found at these locations:

- For each module and theme all files matching `[ModuleLocation]/Localization/[CultureName].po`
- All files matching `/Localization/[CultureName].po`
- For each tenant all files matching `/App_Data/Sites/[TenantName]/Localization/[CultureName].po`
- For each module and theme all files matching  
  - `/Localization/[ModuleId]/[CultureName].po`
  - `/Localization/[ModuleId]-[CultureName].po`
  - `/Localization/[CultureName]/[ModuleId].po`

`[CultureName]` can be either the culture neutral part, e.g. `fr`, or the full one, e.g. `fr-CA`.

It is suggested to put your localization files in the `/Localization/` folder if you are using Docker.
Especially if mounting a volume at `/App_Data/`, as mounting hides pre-existing files.

!!! note
    If you edit a .po file, you need to restart the application to make your change effective.

### Publishing Localization files

The PO files need to be included in the publish output directory.
Add the following configurations to your `[Web Project].csproj` file to include them as Content.

```xml
  <ItemGroup>
    <Content Include="Localization\**" >
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </Content>
  </ItemGroup>
```

!!! note
    Translation files can be provided by a module, in that case they are embedded automatically in the module assembly unless Visual Studio added some bad item groups in the project file.

## Recipe Step

Cultures can be added during recipes using the settings step. Here is a sample step:

```json
{
  "name": "settings",
  "LocalizationSettings": {
    "DefaultCulture":  "fr",
    "SupportedCultures": [ "fr", "en" ]
  }
}
```

### Examples

- `/Localization/fr.po`
- `/Localization/fr-CA.po`
- `/Localization/es-MX.po`

## File format

This article explains how PO files are organized, including plural forms.

<https://www.gnu.org/software/gettext/manual/html_node/PO-Files.html>

## Translation contexts

To prevent entries in different PO files from overriding each other, they define a context for each translation string.  
For instance two views could use the string named `Hello` but they might have different translation. It's then necessary to
provide two entries and specify which _context_ is associated with each translation. In this case each view name is a context.

### From a View

The context string must match the view location up to the module folder.

#### View

Assuming the view's path is `TheAdmin\Views\Layout.cshtml`.

#### PO File

```
msgctxt "TheAdmin.Views.Layout"
msgid "Hello"
msgstr "Bonjour"
```

### From a Service

The context string must match the full name of the type the localizer is injecting in.

#### Source

```csharp
namespace MyNamespace
{
    public class MyService : IMyService
    {
        private readonly IStringLocalizer S;

        public MyService(IStringLocalizer<MyService> localizer)
        {
            S = localizer;
        }

        public void DoSomething()
        {
            Console.WriteLine(S["Hello"]);
        }
    }
}
```

#### PO file

```
msgctxt "MyNamespace.MyService"
msgid "Hello"
msgstr "Bonjour"
```

### Using multiple or custom contexts

A service can use multiple translation contexts by injecting `IStringLocalizerFactory` from `Microsoft.Extensions.Localization` and creating a separate localizer for each context. When messages belong to a shared resource class, resolve the localizer using that resource type:

```csharp
public MyService(IStringLocalizerFactory factory)
{
    S = factory.Create(typeof(SharedMessages));
}
```

The context is the full name of `SharedMessages`, rather than the name of the consuming service.

For a context that is independent of any class name, use `Create(string baseName, string location)`. With the PO localizer, pass the desired `msgctxt` as `baseName` and an empty `location` to preserve that context:

```csharp
private readonly IStringLocalizer S;
private readonly IStringLocalizer _sharedLocalizer;

public MyService(IStringLocalizerFactory factory)
{
    S = factory.Create(typeof(MyService));
    _sharedLocalizer = factory.Create("MySharedContext", string.Empty);
}

public void DoSomething()
{
    Console.WriteLine(S["Hello"]);
    Console.WriteLine(_sharedLocalizer["Hello"]);
}
```

Here, `S` uses `MyNamespace.MyService` as above, while `_sharedLocalizer` uses the following PO entry for the same message:

```po
msgctxt "MySharedContext"
msgid "Hello"
msgstr "Salut"
```

Choose the localizer for the context you need at each lookup. Formatting and pluralization work as usual on each localizer.

### Deferring localization with `LocalizationSource`

When no localizer is available where a string is declared, use `LocalizationSource` from `OrchardCore.Abstractions` to keep the untranslated value and the source type of its localizer:

```csharp
using OrchardCore.Localization;

var description = LocalizationSource.Create("Manage widgets", typeof(MyPermissions));
```

Prefer `LocalizationSource.Create<T>(value)` when the source type can be a generic type argument. Use `LocalizationSource.Create(value, type)` for a runtime source type or a static class, which cannot be a generic type argument. Both methods preserve the original value and type without translating or formatting them. Prefer these explicit factory calls over constructor syntax so source-extraction tools can identify the declaration without inferring the type of `new(...)`. The [analyzer rules](#analyzer-rules) check these declarations as you write them.

`Value` is the original text, used as the PO `msgid`. `Type` supplies the localizer's context, just as with `IStringLocalizer<MyPermissions>` or `IHtmlLocalizer<MyPermissions>`. For example:

```po
msgctxt "MyModule.MyPermissions"
msgid "Manage widgets"
msgstr "Gérer les widgets"
```

`LocalizationSource` is a sealed record with get-only properties. It does not translate, format, or encode the value, so a static readonly source can safely be reused across instances, tenants, and cultures. Resolve the translation at display time, using the current tenant and culture's factory:

```csharp
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;

LocalizedString text = stringLocalizerFactory.Localize(description);
LocalizedHtmlString html = htmlLocalizerFactory.Localize(description);
```

Both extensions call `factory.Create(source.Type)` and look up `source.Value`. The same source can be rendered as text or HTML; it does not commit to an encoding policy at declaration time. A string localizer returns a `LocalizedString` that Razor HTML encodes when rendered. An HTML localizer returns a `LocalizedHtmlString` that preserves markup in the translation and HTML encodes its format arguments.

Pass format arguments to `Localize`, not to `LocalizationSource.Create`, so that the original key is preserved and the translated template is formatted:

```csharp
var welcome = LocalizationSource.Create<MyMessages>("<strong>Welcome {0}</strong>");
var translated = htmlLocalizerFactory.Localize(welcome, userName);
```

With string localization, no formatting is applied when no arguments are supplied. HTML localization follows `LocalizedHtmlString`'s composite formatting rules when rendered: supply arguments for placeholders and escape literal braces as `{{` and `}}`. A source constructed without a type, for example `LocalizationSource.Create("Hello")`, has no PO context: the factory extensions return the original value, with any supplied arguments applied. A null source produces a null result.

!!! warning
    Use HTML localization only for trusted translation markup. It does not encode the translation itself. For plain labels, permission descriptions, attributes, and other text, use string localization and retain the rendering layer's encoding.

### Analyzer rules

`OrchardCore.Abstractions` ships Roslyn analyzers that check localizer calls and `LocalizationSource` declarations while you write them, so that every string can be extracted to translation templates and translated through PO files. They apply to the indexers of `IStringLocalizer`, `IHtmlLocalizer` and `IViewLocalizer`, to `GetString`, `GetHtml` and `Plural`, and to `LocalizationSource.Create(...)`, `LocalizationSource.Create<T>(...)` and the `LocalizationSource` constructors, in every project referencing the package. Members and types marked with `[SkipLocalizationExtraction]` are not analyzed, and neither are Razor views, which compile through a source generator: the extraction reports their dynamic keys instead.

| Rule | Severity | Reported when |
| --- | --- | --- |
| `OCLOC101` | Warning | A constant value is declared without a context, for example `LocalizationSource.Create("Manage widgets")`. Such a source is not extracted, and only a context-free PO entry translates it. The code fix declares the containing type as the context, with `Create<T>(value)`, or with `Create(value, typeof(T))` when the containing type is static. |
| `OCLOC102` | Info | The context is neither the declaring type nor one of its containing types, for example `LocalizationSource.Create<OtherMessages>("Manage widgets")` declared in `MyPermissions`. Sharing another type's context is legitimate when the strings belong together; the rule only points out the difference. |
| `OCLOC103` | Warning | The value is not a compile-time constant, for example `LocalizationSource.Create<MyPermissions>(name)`, so it cannot be extracted. Pass a constant, or mark a member that forwards values declared elsewhere with `[SkipLocalizationExtraction]`. |
| `OCLOC104` | Warning | A localizer key is not a compile-time constant, for example `S[message]`, `S.GetString(name)`, `S.Plural(count, singular, plural)` with a variable form, or `S.Plural(count, forms)` with forms that are not an inline array of constants, so it cannot be extracted. Pass a constant. For a key that is dynamic by design, because its values are declared and extracted where they originate, such as the outcomes a workflow designer enters, disable the rule around the call with `#pragma warning disable OCLOC104` and say where the values come from. |

A context-free declaration whose value is not a constant, or whose type is a runtime `Type`, is not reported: it forwards a value that is declared, and extracted, elsewhere, like the `Permission` constructors taking a `string` description do.

The analyzers run in the IDE and in every build, and `dotnet build -warnaserror` turns their warnings into errors. Suppress a rule locally with `#pragma warning disable OCLOC101`, or configure its severity in `.editorconfig`, for example `dotnet_diagnostic.OCLOC102.severity = none`.

### Sharing `LocalizationSource` instances

Because a source is immutable and doesn't depend on a tenant or a culture, it can be declared once and reused wherever the same text has the same meaning. A shared source is translated once, with a single PO entry, and an existing source can be found before a new one is created.

- `CommonLocalizationSources`, in `OrchardCore.Abstractions`, declares the text that is used by several modules, such as `CommonLocalizationSources.ContentManagement`, or that is general to every module, such as `CommonLocalizationSources.Settings`. A source used by a single project, even a general word such as the name of the module, belongs in that project. Its PO context is `OrchardCore.Localization.CommonLocalizationSources`.
- A module declares its other sources in an internal static class at its root, named after the module, for example `LuceneLocalizationSources` in `OrchardCore.Lucene`. Its PO context is the full name of that class.

```csharp
using OrchardCore.Localization;

namespace MyModule;

internal static class MyModuleLocalizationSources
{
    public static readonly LocalizationSource ExportMyData = LocalizationSource.Create("Export My Data", typeof(MyModuleLocalizationSources));
}
```

```po
msgctxt "OrchardCore.Localization.CommonLocalizationSources"
msgid "Content Management"
msgstr "Gestion du contenu"

msgctxt "MyModule.MyModuleLocalizationSources"
msgid "Export My Data"
msgstr "Exporter mes données"
```

Permission descriptions stay in the `*Permissions` class that declares the permissions. Text that is shown by a single view stays in the view, and is localized with `T`.

## Pluralization

This module also provides support for pluralization.
It is necessary to reference the `OrchardCore.Localization.Abstractions` package in order to be able to use it.

### Sample PO file

```
msgctxt "TheAdmin.Views.Layout"
msgid "{0} book"
msgid_plural "{0} books"
msgstr[0] "[{0} livre]"
msgstr[1] "[{0} livres]"
```

### Usage

- Import the `using Microsoft.Extensions.Localization` namespace.
- Inject an instance of `IStringLocalizer` or `IViewLocalizer` (represented as the `T` variable in the following example).

```csharp
T.Plural(count, "{0} book", "{0} books")
```
In this example
* `"{0} book"` is the singular form
* `"{0} books"` is the plural form
* `count` will determine if the singular or plural form is used and will replace the {0} placeholder

!!! warning
    You should not hardcode a number in the singular or plural forms because different languages have different rules about when each form is used.

### Pluralizing a deferred `LocalizationSource`

`IStringLocalizerFactory` and `IHtmlLocalizerFactory` also expose a `Plural` extension for a `LocalizationSource` declared without an available localizer, for example a static field evaluated before a tenant or culture is known. It requires `OrchardCore.Localization.Abstractions` and lives in the same namespaces as the plural helpers above (`Microsoft.Extensions.Localization` / `Microsoft.AspNetCore.Mvc.Localization`):

```csharp
using Microsoft.Extensions.Localization;
using OrchardCore.Localization;

private static readonly LocalizationSource _item = LocalizationSource.Create<MyMessages>("{0} item for {1}");

var result = stringLocalizerFactory.Plural(count, _item, "{0} items for {1}", ownerName);
```

The source's `Value` is the singular PO `msgid`, exactly like `Localize`. `count` is always inserted automatically as the first format argument (`{0}`); any additional arguments you pass, such as `ownerName`, follow it (`{1}`, `{2}`, ...). The HTML factory extension has the same signature and semantics, deferring HTML encoding of the arguments to render time, like `Localize`.

For a context-bearing source (`Type` set), the extension resolves `factory.Create(source.Type)` and delegates to the existing `IStringLocalizer.Plural`/`IHtmlLocalizer.Plural` extension, so the culture's PO `PluralRule` always decides the translated form, including languages with a dedicated zero form or more than two plural forms. For a context-free source (`Type` null, no PO lookup possible), the extension falls back to the English rule — singular only when `count == 1` — without resolving a factory, matching the existing context-free behavior of `Localize`. A null source returns `null` without resolving a factory, same as `Localize`.

### Translation templates

Every project keeps its translation template, a gettext `.pot` file named `<AssemblyName>.pot` next to its project file, checked in with the sources it describes, like built assets are. A normal build embeds the template in the assembly as the `<AssemblyName>.Localization.pot` manifest resource, and does nothing else: extracting the strings is a separate, on-demand operation, so building never pays for it.

The `OrchardCore.Localization.Build` package provides both. Add it to each project whose strings should be extracted:

```xml
<PackageReference Include="OrchardCore.Localization.Build" PrivateAssets="all" />
```

Use the package version matching your Orchard Core version (or a centrally managed package version). The build worker targets Orchard Core's `DefaultTargetFramework`, configured in `src/OrchardCore.Build/TargetFrameworks.props`. The corresponding .NET runtime, or a later major version, must be installed on the build machine, including when the consuming project targets .NET Standard 2.0. The worker is packaged under `tools/any/` and is resolved independently of the consuming project's target framework. Its dependencies are build-only and are not copied into the application's publish output.

#### Updating the templates

Update the template of one project with its `UpdateLocalizationCatalog` target:

```bash
dotnet msbuild src/OrchardCore.Modules/OrchardCore.Admin/OrchardCore.Admin.csproj -restore -t:UpdateLocalizationCatalog
```

Update the templates of a solution, of an application and the projects it references, or of a project and its references, by building with `UpdateLocalizationCatalogs`:

```bash
dotnet build OrchardCore.slnx -p:UpdateLocalizationCatalogs=true
```

Both write `<AssemblyName>.pot` in the project directory, or remove it when the project has no localizable string, and leave the rest of the build unchanged. Unchanged inputs reuse a cached result stored under `obj/`, so repeating the update is fast. Commit the templates with the change that adds, changes or removes strings. In the Orchard Core source tree, projects using `OrchardCore.Commons.props` (including framework projects, modules, and themes) have the targets without any project-specific import, and the continuous integration build updates every template and fails when a committed one differs from the sources, with the same message to run the command above.

The worker compiles the sources with the Roslyn compiler and the Razor compiler of the .NET SDK that drives the build, loaded from the SDK's `Roslyn/bincore` and `Sdks/Microsoft.NET.Sdk.Razor/source-generators` directories, so that it sees the sources exactly as the build does. The SDK's Roslyn version must be at least the version the worker was built against (5.9, shipped with SDK 10.0.4xx), and the worker reports the version it used in the build output.

Extraction covers semantic C# localizer calls, Razor views and pages, and literal inputs to the Liquid `t` filter. C# constants, typed localizer contexts, and supported plural calls are preserved. Liquid uses Orchard Core's existing parser and traverses expressions without rendering templates or executing module startup code. Custom Liquid syntax requires a configured parser when using the extraction library directly.

DataAnnotations validation attributes (including custom attributes derived from `ValidationAttribute`) are processed by a separate semantic extractor. Explicit constant `ErrorMessage` values, such as `[Required(ErrorMessage = "Username is required.")]`, use the `OrchardCore.Localization.DataAnnotations.DataAnnotationsDefaultErrorMessages` context that the runtime metadata provider looks up. When the message contains the member name or an explicit `DisplayAttribute.Name`, its `{0}` replacement fallback is included too. Default framework messages continue to come from the existing default-message marker catalog. Resource-backed messages are not evaluated, and resource property names or display labels are not treated as validation message keys. Messages assigned inside custom attribute constructors cannot be inferred; specify `ErrorMessage` explicitly when they need extraction. Marked `[SkipLocalizationExtraction]` scopes are excluded.

A template only depends on the sources, so that it can be compared byte for byte: entries, references, and format flags are sorted and written without timestamps; source references name the file without its line, unless `LocalizationCatalogLineNumbers` is `true`, so that editing a file only changes the template when a message, its context, or the files declaring it change; the `DEBUG` and `TRACE` constants are not defined while extracting, so strings in code compiled only in one configuration are not extracted; and a multi-targeting project extracts its template from its first target framework. Templates use LF line endings, which the repository's `.gitattributes` preserves on every platform.

| Build property | Purpose |
| --- | --- |
| `GenerateLocalizationCatalog` | Defaults to `true` when the package is imported; set to `false` to neither embed nor update the template. |
| `UpdateLocalizationCatalogs` | Defaults to `false`; set to `true` to rewrite the templates of the projects being built. |
| `LocalizationCatalogPath` | Overrides the template path; defaults to `<AssemblyName>.pot` in the project directory. |
| `LocalizationCatalogLineNumbers` | Defaults to `false`; set to `true` to write the line of each source reference. |
| `LocalizationStrict` | Defaults to `false`; set to `true` to fail an update on extraction warnings as well as errors. |
| `LocalizationCatalogOutputPath` | Optional export directory. When set, a build copies the templates of the built projects into `<directory>/<TargetFramework>/<AssemblyName>.pot`. Relative paths are resolved against each project directory; use an absolute path for a shared solution or application export. |
| `LocalizationViewPrefix` | Overrides the view context prefix; module and theme builds default to the assembly name. |
| `LocalizationToolPath` | Overrides the worker DLL path. |
| `LocalizationDotNetHostPath` | Overrides the `dotnet` host used to run the worker. |
| `LocalizationRoslynDirectory` | Overrides the directory of the Roslyn assemblies the worker loads; defaults to the `Roslyn/bincore` directory of the SDK driving the build. |
| `LocalizationRazorCompilerPath` | Overrides the path of the `Microsoft.CodeAnalysis.Razor.Compiler` assembly the worker loads; defaults to the Razor SDK's source generator, taken from the project's analyzers. |

An update reports what it could not extract as diagnostics with an `OCLOC` prefix, listed in the build output with the file and line they refer to. Warnings never fail the update unless `LocalizationStrict` is `true`; errors always do.

| Code | Severity | Meaning |
| --- | --- | --- |
| `OCLOC001` | Warning | The localization key, or a `LocalizationSource` value, is not a compile-time constant string. |
| `OCLOC002` | Warning | The localizer's context, or a `LocalizationSource` type, cannot be resolved statically. Use a typed localizer, a `Create<T>` call, or a `typeof` argument. |
| `OCLOC003` | Warning | A `Plural` call with a plural forms array does not provide exactly two constant forms, which gettext requires. |
| `OCLOC004` | Warning | An empty localization key was found; the empty key is reserved for the gettext header. |
| `OCLOC005` | Error | The same context and key are declared both with and without a plural form, or with different plural forms. |
| `OCLOC006` | Error or warning | The Razor compiler was not found or could not be loaded, or the Razor compilation reported a diagnostic, kept with its own severity. |
| `OCLOC007` | Error | A Liquid file could not be parsed. |
| `OCLOC008` | Error or warning | The extraction compilation reported a C# error, a source generator failed, or a generator assembly could not be loaded (warning). |
| `OCLOC009` | Error | The worker was not found, the SDK's compiler was not found, the extraction was canceled, or it failed unexpectedly. |

Liquid files are discovered from evaluated embedded-resource, content, and none items. Additional inputs can be supplied explicitly:

```xml
<ItemGroup>
  <LocalizationLiquid Include="Templates/**/*.liquid" />
</ItemGroup>
```

The `GetLocalizationCatalogs` MSBuild target returns the `LocalizationCatalog` item of a project that has a template, with assembly name, manifest resource name, and target framework metadata. It does not collect catalogs from referenced projects automatically.

Pass `-p:GenerateLocalizationCatalog=false` to neither embed nor update templates for a build and its referenced projects.

Localization implementations and forwarding helpers can opt out with `OrchardCore.Localization.SkipLocalizationExtractionAttribute`, provided by `OrchardCore.Localization.Abstractions`:

```csharp
[SkipLocalizationExtraction]
public LocalizedString Forward(IStringLocalizer localizer, string name)
    => localizer[name];
```

Apply the attribute to a class or method to exclude localization calls and extraction diagnostics inside that declaration, including nested declarations and lambdas. A marked partial class is excluded across all its parts. The attribute is not inherited by derived classes or overriding methods. Calls from unmarked application code to marked helper methods are still extracted. Only mark infrastructure that forwards caller-supplied keys; dynamic keys in application code continue to produce diagnostics.

!!! note
    Templates are source templates, not translated PO files. They do not change runtime translation lookup. Dynamic keys or unresolved contexts produce diagnostics rather than guessed entries. Direct JavaScript extraction and runtime translation overrides are not included.

### Export translation templates for localization platforms

The checked-in templates are the files to upload to a localization platform such as Crowdin, which replaces the separate PoExtractor tool of earlier versions. In the Orchard Core repository they are listed with `git ls-files '*.pot'`. To gather the templates of a solution or application into one directory, for example when a platform expects a flat upload, build with `LocalizationCatalogOutputPath` set to a dedicated output directory; each built project copies its template there, without updating it:

```powershell
dotnet build OrchardCore.slnx -p:LocalizationCatalogOutputPath="$PWD/artifacts/localization"
```

To export only an application and the projects built through its project references:

```powershell
dotnet build src/OrchardCore.Cms.Web/OrchardCore.Cms.Web.csproj -p:LocalizationCatalogOutputPath="$PWD/artifacts/localization"
```

Use an absolute output path so every project copies to the same directory. For other solutions, substitute your solution or application project path. Outside the Orchard Core source tree, add `OrchardCore.Localization.Build` to each project whose strings should be extracted; referencing it only from the application does not enable it in other projects. Prebuilt NuGet dependencies are not re-extracted or collected by this mechanism.

Exported files are named `<output>/<TargetFramework>/<AssemblyName>.pot`, for example `artifacts/localization/net10.0/OrchardCore.Admin.pot`. Framework subdirectories keep parallel multi-target builds from overwriting one another. Projects sharing an assembly name and target framework must use separate export directories or distinct assembly names. Only templates are copied, not request files or extraction caches.

For Crowdin or a similar service:

1. Update the templates and commit them, or export them into an empty directory to obtain a current snapshot of the solution or application templates.
2. Upload the POT files as source files, preserving `msgctxt`, plural forms, and format placeholders.
3. Configure the platform to export translated PO files for each culture.
4. Download the PO files into one of the [PO files locations](#po-files-locations) supported by Orchard Core and deploy them with the application.

## JavaScript Localization

See [JavaScript Localization (`IJSLocalizer`)](javascript-localization.md) for guidance on exposing PO-file-backed translations to JavaScript / TypeScript assets.

## Liquid filters

For more information on using locaization filters in Liquid templates, see the [Liquid Localization filters documentation](../Liquid/README.md#localization-filters).

## Video

<iframe width="560" height="315" src="https://www.youtube-nocookie.com/embed/cwKa1OA48-4" title="YouTube video player" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture" allowfullscreen></iframe>

## Recipe Configuration

Localization settings can be configured using the `Settings` recipe step:

```json
{
  "steps": [
    {
      "name": "settings",
      "LocalizationSettings": {
        "DefaultCulture": "en-US",
        "SupportedCultures": [
          "en-US",
          "fr-FR",
          "es-ES"
        ],
        "FallBackToParentCulture": true
      }
    }
  ]
}
```

| Property                  | Type            | Description                                                                 |
|---------------------------|-----------------|-----------------------------------------------------------------------------|
| `DefaultCulture`          | String          | The default culture for the site (e.g., `en-US`).                           |
| `SupportedCultures`       | Array of String | The list of supported cultures.                                             |
| `FallBackToParentCulture` | Boolean         | Whether to fall back to the parent culture when a translation is not found. |
