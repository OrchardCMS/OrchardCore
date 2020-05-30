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
msgid "1 book"
msgid_plural "{0} books"
msgstr[0] "[1 livre]"
msgstr[1] "[{0} livres]"
```

### Usage

- Import the `using Microsoft.Extensions.Localization` namespace.
- Inject an instance of `IStringLocalizer` or `IViewLocalizer` (represented as the `T` variable in the following example).

```csharp
T.Plural(count, "1 book", "{0} books")
```

### Extract translations to PO files

In order to generate the .po files, you can use [this tool](https://github.com/lukaskabrt/PoExtractor).

The simpler way to use it is to install it with this command:

```bash
dotnet tool install --global PoExtractor.OrchardCore
```

Then, you will be able to run this command to generate the .po files:

``` bash
extractpo-oc C:\Path\OrchardCore C:\temp\OrchardCore --liquid
```
