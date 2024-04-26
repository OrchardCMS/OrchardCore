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

It is suggested to put your localization files in the `/Localization/` folder if you are using docker.  
Especially if mounting a volume at `/App_Data/` as mounting hides pre-existing files.

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
},
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

### Extract translations to PO files

In order to generate the .po files, you can use [this tool](https://github.com/OrchardCoreContrib/OrchardCoreContrib.PoExtractor).

The simpler way to use it is to install it with this command:

```bash
dotnet tool install --global OrchardCoreContrib.PoExtractor
```

Then, you will be able to run this command to generate the .po files:

``` bash
extractpo <INTPUT_PATH> <OUTPUT_PATH> [-l|--language {"C#"|"VB"}] [-t|--template {"razor"|"liquid"}]
```

## Video

<iframe width="560" height="315" src="https://www.youtube-nocookie.com/embed/cwKa1OA48-4" title="YouTube video player" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture" allowfullscreen></iframe>
