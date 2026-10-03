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

### Build-time embedded translation templates

The `OrchardCore.Localization.Build` package extracts localizable strings during a normal build and embeds a gettext template (`.pot`) in the assembly. Add it to each project whose strings should be extracted:

```xml
<PackageReference Include="OrchardCore.Localization.Build" PrivateAssets="all" />
```

Use the package version matching your Orchard Core version (or a centrally managed package version). The build worker requires .NET 10, including when the consuming project targets .NET Standard 2.0. Its dependencies are build-only and are not copied into the application's publish output.

Extraction covers semantic C# localizer calls, Razor views and pages, and literal inputs to the Liquid `t` filter. C# constants, typed localizer contexts, and supported plural calls are preserved. Liquid uses Orchard Core's existing parser and traverses expressions without rendering templates or executing module startup code. Custom Liquid syntax requires a configured parser when using the extraction library directly.

The template is generated at `$(IntermediateOutputPath)/Localization/$(AssemblyName).pot` and embedded with the manifest resource name `$(AssemblyName).Localization.pot`. Entries, references, and format flags are written deterministically, without timestamps. Unchanged inputs reuse the cached catalog and diagnostics; unchanged output is not rewritten. `Clean` removes generated files. Design-time builds skip extraction, and multi-target projects generate separate outputs for each target framework.

| Build property | Purpose |
| --- | --- |
| `GenerateLocalizationCatalog` | Defaults to `true` when the package is imported; set to `false` to disable extraction. |
| `LocalizationStrict` | Defaults to `false`; set to `true` to fail on extraction warnings as well as errors. |
| `LocalizationCatalogPath` | Overrides the generated POT path. Keep custom paths distinct for each target framework. |
| `LocalizationViewPrefix` | Overrides the view context prefix; module and theme builds default to the assembly name. |
| `LocalizationToolPath` | Overrides the worker DLL path. |
| `LocalizationDotNetHostPath` | Overrides the `dotnet` host used to run the worker. |

Liquid files are discovered from evaluated embedded-resource, content, and none items. Additional inputs can be supplied explicitly:

```xml
<ItemGroup>
  <LocalizationLiquid Include="Templates/**/*.liquid" />
</ItemGroup>
```

The `GetLocalizationCatalogs` MSBuild target returns generated `LocalizationCatalog` items with assembly name, manifest resource name, and target framework metadata. It does not collect catalogs from referenced projects automatically.

In the Orchard Core source tree, projects using `OrchardCore.Commons.props` (including framework projects, modules, and themes) enable extraction automatically. No project-specific imports are required. The localization tooling and source generator are excluded, and tooling dependencies are built without extraction to avoid circular build dependencies.

```bash
dotnet build src/OrchardCore.Modules/OrchardCore.Admin/OrchardCore.Admin.csproj
```

Pass `-p:GenerateLocalizationCatalog=false` to disable catalog generation for a build and its referenced projects.

!!! note
    Embedded POT files are source templates, not translated PO files. They do not change runtime translation lookup. Dynamic keys or unresolved contexts produce diagnostics rather than guessed entries. JavaScript extraction, automatic cross-project catalog collection, and runtime translation overrides are not included.

### Extract translations with an external tool

In order to generate the .po files, you can use [this tool](https://github.com/OrchardCoreContrib/OrchardCoreContrib.PoExtractor).

The simpler way to use it is to install it with this command:

```bash
dotnet tool install --global OrchardCoreContrib.PoExtractor
```

Then, you will be able to run this command to generate the .po files:

``` bash
extractpo <INTPUT_PATH> <OUTPUT_PATH> [-l|--language {"C#"|"VB"}] [-t|--template {"razor"|"liquid"}]
```

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
