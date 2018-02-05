# Localization (OrchardCore.Localization)

This module provides the infrastructure necessary to support the PO (Portable Object) localization file format.
It also supports plural forms.

## PO files locations

PO files are found via the following steps:

- For each module and theme all files matching `[ModuleLocation]/App_Data/Localization/[CultureName].po`
- Then all files matching `/App_Data/Localization/[CultureName].po`
- For each tenant all files matching `/App_Data/Sites/[TenantName]/Localization/[CultureName].po`

## File format

This article explains how PO files are organized, including plural forms.

<https://www.gnu.org/software/gettext/manual/html_node/PO-Files.html>


## Translation contexts

To prevent different PO files entries from overriding each other, entries define a context for each translation string.
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

```
namespace MyNamespace
{
    public class MyService : IMyService
    {
        public IStringLocalizer T { get; set; }

        public MyService(IStringLocalizer<MyService> localizer)
        {
            T = localizer;
        }

        public void DoSomething()
        {
            Console.WriteLine(T["Hello"]);
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


```
T.Plural(count, "1 book", "{0} books")
```
