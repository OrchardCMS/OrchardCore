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

## Pluralization

This module also provides support for pluralization.
It is necessary to reference the `OrchardCore.Localization.Abstractions` package in order to be able to use it.

### Plural rules of PO files

The translations of a PO file select their plural form with the `Plural-Forms` header of the file, when it has one:

```po
msgid ""
msgstr ""
"Language: cs\n"
"Plural-Forms: nplurals=3; plural=(n==1) ? 0 : (n>=2 && n<=4) ? 1 : 2;\n"
```

The rule of a file only applies to its own translations, as a culture dictionary merges the PO files of all the modules and of the application. When the same entry is defined by several files, the rule of the file whose translation is used applies.

The `plural` expression supports the gettext syntax: the `n` count, integer literals, parentheses and the `?:`, `||`, `&&`, `!`, `==`, `!=`, `<`, `>`, `<=`, `>=`, `+`, `-`, `*`, `/` and `%` C operators. It is interpreted with the absolute value of the count. A division or a modulo by zero evaluates to `0`, and a form outside of the `0` to `nplurals - 1` range selects the first form, like gettext does.

The rule of the culture, provided by the `IPluralRuleProvider` services described below, applies to:

- The translations of a PO file without a `Plural-Forms` header, or with an invalid or unsupported one.
- The translations that are not loaded from a PO file, such as the data translations.
- The plural forms passed to `Plural` when no translation exists.

### Culture-specific rules

`DefaultPluralRuleProvider` first looks for a rule for the requested culture, then checks each `CultureInfo.Parent` until it finds a rule. For example, `zh-Hans-CN` falls back through `zh-Hans` to `zh`. The invariant culture and cultures without a registered ancestor have no rule in this provider.

The built-in provider implements all explicit non-root locales in the [CLDR 49 cardinal rules](https://www.unicode.org/cldr/charts/49/supplemental/language_plural_rules.html) for the integer counts accepted by `PluralizationRuleDelegate`. All its rules use the absolute value of the count, including `int.MinValue`. Decimal and compact-exponent operands are not represented by this API. Categories that require fractional operands, such as Czech and Slovak `many`, do not need an integer translation index.

CLDR category names do not prescribe gettext numeric indexes. Orchard Core preserves established category indexes where possible and appends newly supported categories. The following table lists categories in translation-index order, starting at `msgstr[0]`:

| Cultures or rule group | Categories in index order |
| --- | --- |
| Single-form rules, including `jv`, `yo`, `yue` | `other` |
| Ordinary two-form rules, including `en`, `fa`, `vi`, `fil`, `tr`, `uz` | `one`, `other` |
| `fr`, `pt`, `ca`, `es`, `gl`, `it`, `lld`, `scn`, `vec` (and `pt-PT`) | `one`, `other`, `many` |
| `he`, `iu`, `naq`, `sat`, `se`, `sma`, `smi`, `smj`, `smn`, `sms` | `one`, `other`, `two` |
| `lv`, `prg` | `one`, `other`, `zero` |
| `blo`, `cv`, `ksh`, `lag` | `zero`, `one`, `other` |
| `bs`, `hr`, `sr`, `cs`, `sk`, `lt`, `ro`, `shi` | `one`, `few`, `other` |
| `be`, `pl`, `ru`, `uk` | `one`, `few`, `many` |
| `gd`, `gv`, `sgs` | `one`, `two`, `few`, `other` |
| `sl`, `dsb`, `hsb` | `other`, `one`, `two`, `few` |
| `br` | `one`, `other`, `two`, `few`, `many` |
| `mt` | `one`, `few`, `many`, `other`, `two` |
| `ga` | `one`, `two`, `few`, `many`, `other` |
| `cy`, `kw` | `one`, `two`, `few`, `other`, `zero`, `many` |
| `ar`, `ars` | `zero`, `one`, `two`, `few`, `many`, `other` |

The listed CLDR legacy codes (`in`, `iw`, `ji`, `jw`, `mo`, `sh`, `tl`) have explicit registrations too. This is not a general CLDR locale-alias resolver: other aliases depend on .NET culture normalization or a custom provider.

The older registrations `ach`, `anp`, `arn`, `ay`, `csb`, `gun`, `hne`, `mai`, `mfe`, `mi`, `mni`, `mnk`, `nap`, `oc`, `pms`, `rw`, `sco`, `son`, and `tt` are retained, but do not have explicit cardinal entries in CLDR 49. Their existing partitions are preserved, with the same absolute-count handling. Unsupported cultures return no rule; the localization manager ultimately uses its English-style fallback unless another provider supplies a rule.

#### Million-multiple categories and Portuguese fallback

French and Portuguese use `one` for zero and one; Catalan, Spanish, Galician, Italian, Ladin, Sicilian, Venetian, and `pt-PT` use it only for one. Their `many` category applies to nonzero integer multiples of 1,000,000. The translation indexes are:

| Index | Category | `pt` and `pt-BR` | `pt-PT` |
| --- | --- | --- | --- |
| `0` | `one` | Zero and one | One |
| `1` | `other` | All remaining counts | All remaining counts |
| `2` | `many` | Nonzero multiples of 1,000,000 | Nonzero multiples of 1,000,000 |

CLDR's [component-specific parent locales](https://unicode.org/reports/tr35/tr35.html#Parent_Locales) replace, rather than merge with, the general parent table. CLDR 49 declares an empty override table for plurals. Consequently, `pt-AO`, `pt-CH`, `pt-CV`, `pt-FR`, `pt-GQ`, `pt-GW`, `pt-LU`, `pt-MO`, `pt-MZ`, `pt-ST`, and `pt-TL` fall back to `pt` for plural rules, even though the general locale-data parent table assigns them to `pt-PT`. Only `pt-PT` has the Portuguese zero-is-`other` override.

#### Migrating plural catalogs

!!! warning
    PO catalogs with a valid `Plural-Forms` header keep selecting their translations with their own rule, whatever the rules of the provider. Review the catalogs without a header, as the provider selects their `msgstr[index]` translations. When adopting the rules of the provider in a catalog, update both the `Plural-Forms` header and every affected entry's `msgstr[index]` translations. Missing indexes throw `PluralFormNotFoundException`; a complete but differently ordered catalog can silently select the wrong translation. Do not automatically reorder translations or invent translations for new categories. Additional or legacy rules can be supplied through `IPluralRuleProvider`.

In particular, for catalogs using the rules of the provider:

- Add `many` at index `2` for `fr`, `pt`, `ca`, `es`, `gl`, and `it` catalogs (including regional fallbacks). These are integer-relevant forms, not decimal-only forms.
- Add `two` at index `2` for `he`, `sat`, and `se`; add it at index `4` for `mt`. Breton now needs five categories, and Welsh and Cornish need six, in the order above.
- Split the old single-form catalogs for `fa`, `vi`, `cgg`, `ka`, `kk`, `ky`, and `ug`. Conversely, `jv` and `yo` now use only `other` at index `0`; migrate the appropriate plural translation into that index.
- Review zero-count translations for `as`, `bn`, `doi`, `ff`, `gu`, `hi`, `hy`, `kn`, `nso`, `pa`, and `si` (now `one`), and `tr` and `uz` (now `other`). General Portuguese includes zero in `one`; a legacy `plural=(n != 1)` header does not match.
- Review Filipino last-digit groups, Macedonian ending-11 exceptions, Lithuanian `few` counts ending in `5` through `9`, and Latvian `zero` counts ending in `0` or with last two digits `11` through `19`. Maltese `102` is `other`, not `few`.
- Negative counts now select the same category as their positive magnitude in every built-in rule. Custom provider delegates are not normalized by the localization manager.

For nonnegative integer counts, matching PO headers are:

```po
# fr, pt, pt-BR, and Portuguese locales other than pt-PT
"Plural-Forms: nplurals=3; plural=(n == 0 || n == 1) ? 0 : (n != 0 && n % 1000000 == 0) ? 2 : 1;\n"
```

```po
# ca, es, gl, it, lld, scn, vec, and pt-PT
"Plural-Forms: nplurals=3; plural=(n == 1) ? 0 : (n != 0 && n % 1000000 == 0) ? 2 : 1;\n"
```

#### Plural forms of each rule

The tables below list, for each built-in rule, the cultures registered with it and the condition selecting each translation index, in index order. A culture that isn't listed uses the rule of its closest listed parent culture. The tables are generated from the `PluralRuleDescriptor` metadata, available through `DefaultPluralRuleProvider.Rules` and `DefaultPluralRuleProvider.TryGetRuleDescriptor()`. The examples are computed by evaluating the runtime rules, and automated tests validate the conditions against the rules. Like the rules, the conditions apply to the absolute value of the count, and an ending such as `01` includes leading zeros, so it matches 1 and 101 but not 11.

<!-- plural-rules:start -->
**1 form:** `ay`, `bm`, `bo`, `dz`, `hnj`, `id`, `ig`, `ii`, `in`, `ja`, `jbo`, `jv`, `jw`, `kde`, `kea`, `km`, `ko`, `lkt`, `lo`, `ms`, `my`, `nqo`, `osa`, `sah`, `ses`, `sg`, `su`, `th`, `to`, `tpi`, `tt`, `wo`, `yo`, `yue`, `zh`

| Translation | Condition | Examples |
| --- | --- | --- |
| `msgstr[0]` | All counts | 0, 1, 2, 3 |

**2 forms:** `ach`, `ak`, `am`, `arn`, `as`, `bho`, `bn`, `csw`, `doi`, `fa`, `ff`, `gu`, `gun`, `guw`, `hi`, `hy`, `kab`, `kn`, `kok`, `kok-Latn`, `ln`, `mfe`, `mg`, `mi`, `nso`, `oc`, `pa`, `pcm`, `si`, `tg`, `ti`, `vi`, `wa`, `zu`

| Translation | Condition | Examples |
| --- | --- | --- |
| `msgstr[0]` | Exactly 0 or 1 | 0, 1 |
| `msgstr[1]` | All remaining counts | 2, 3, 4, 5 |

**2 forms:** `af`, `an`, `anp`, `asa`, `ast`, `az`, `bal`, `bem`, `bez`, `bg`, `brx`, `ce`, `cgg`, `chr`, `ckb`, `da`, `de`, `dv`, `ee`, `el`, `en`, `eo`, `et`, `eu`, `fi`, `fo`, `fur`, `fy`, `gsw`, `ha`, `haw`, `hne`, `hu`, `ia`, `ie`, `io`, `jgo`, `ji`, `jmc`, `ka`, `kaj`, `kcg`, `kk`, `kkj`, `kl`, `ks`, `ksb`, `ku`, `ky`, `lb`, `lg`, `lij`, `mai`, `mas`, `mgo`, `ml`, `mn`, `mni`, `mr`, `nah`, `nap`, `nb`, `nd`, `ne`, `nl`, `nn`, `nnh`, `no`, `nr`, `ny`, `nyn`, `om`, `or`, `os`, `pap`, `pms`, `ps`, `rm`, `rof`, `rw`, `rwk`, `saq`, `sc`, `sco`, `sd`, `sdh`, `seh`, `sn`, `so`, `son`, `sq`, `ss`, `ssy`, `st`, `sv`, `sw`, `syr`, `ta`, `te`, `teo`, `tig`, `tk`, `tn`, `tr`, `ts`, `ug`, `ur`, `uz`, `ve`, `vo`, `vun`, `wae`, `xh`, `xog`, `yi`

| Translation | Condition | Examples |
| --- | --- | --- |
| `msgstr[0]` | Exactly 1 | 1 |
| `msgstr[1]` | All remaining counts | 0, 2, 3, 4 |

**2 forms:** `tzm`

| Translation | Condition | Examples |
| --- | --- | --- |
| `msgstr[0]` | Exactly 0 or 1, or from 11 to 99 | 0, 1, 11, 99 |
| `msgstr[1]` | All remaining counts | 2, 3, 10, 100 |

**3 forms:** `fr`, `pt`, `pt-BR`

| Translation | Condition | Examples |
| --- | --- | --- |
| `msgstr[0]` | Exactly 0 or 1 | 0, 1 |
| `msgstr[1]` | All remaining counts | 2, 3, 4, 5 |
| `msgstr[2]` | Multiple of 1,000,000, except 0 | 1,000,000, 2,000,000, 3,000,000, 4,000,000 |

**3 forms:** `ca`, `es`, `es-AR`, `gl`, `it`, `lld`, `pt-PT`, `scn`, `vec`

| Translation | Condition | Examples |
| --- | --- | --- |
| `msgstr[0]` | Exactly 1 | 1 |
| `msgstr[1]` | All remaining counts | 0, 2, 3, 4 |
| `msgstr[2]` | Multiple of 1,000,000, except 0 | 1,000,000, 2,000,000, 3,000,000, 4,000,000 |

**2 forms:** `is`

| Translation | Condition | Examples |
| --- | --- | --- |
| `msgstr[0]` | Ends in 1, except 11 | 1, 21, 31, 41 |
| `msgstr[1]` | All remaining counts | 0, 2, 20, 22 |

**2 forms:** `mk`

| Translation | Condition | Examples |
| --- | --- | --- |
| `msgstr[0]` | Ends in 1, except 11 | 1, 21, 31, 41 |
| `msgstr[1]` | All remaining counts | 0, 2, 20, 22 |

**2 forms:** `ceb`, `fil`, `tl`

| Translation | Condition | Examples |
| --- | --- | --- |
| `msgstr[0]` | All remaining counts | 0, 3, 5, 7 |
| `msgstr[1]` | Ends in 4, 6 or 9 | 4, 6, 9, 14 |

**3 forms:** `be`, `bs`, `hr`, `ru`, `sh`, `sr`, `uk`

| Translation | Condition | Examples |
| --- | --- | --- |
| `msgstr[0]` | Ends in 1, except 11 | 1, 21, 31, 41 |
| `msgstr[1]` | Ends in 2-4, except 12-14 | 2, 4, 22, 24 |
| `msgstr[2]` | All remaining counts | 0, 5, 20, 25 |

**3 forms:** `lt`

| Translation | Condition | Examples |
| --- | --- | --- |
| `msgstr[0]` | Ends in 1, except 11-19 | 1, 21, 31, 41 |
| `msgstr[1]` | Ends in 2-9, except 11-19 | 2, 9, 22, 29 |
| `msgstr[2]` | All remaining counts | 0, 10, 20, 30 |

**3 forms:** `cs`, `sk`

| Translation | Condition | Examples |
| --- | --- | --- |
| `msgstr[0]` | Exactly 1 | 1 |
| `msgstr[1]` | From 2 to 4 | 2, 3, 4 |
| `msgstr[2]` | All remaining counts | 0, 5, 6, 7 |

**3 forms:** `csb`, `pl`

| Translation | Condition | Examples |
| --- | --- | --- |
| `msgstr[0]` | Exactly 1 | 1 |
| `msgstr[1]` | Ends in 2-4, except 12-14 | 2, 4, 22, 24 |
| `msgstr[2]` | All remaining counts | 0, 5, 21, 25 |

**3 forms:** `lv`, `prg`

| Translation | Condition | Examples |
| --- | --- | --- |
| `msgstr[0]` | Ends in 1, except 11 | 1, 21, 31, 41 |
| `msgstr[1]` | All remaining counts | 2, 9, 22, 29 |
| `msgstr[2]` | Ends in 0 or 11-19 | 0, 10, 20, 30 |

**3 forms:** `blo`, `cv`, `ksh`, `lag`

| Translation | Condition | Examples |
| --- | --- | --- |
| `msgstr[0]` | Exactly 0 | 0 |
| `msgstr[1]` | Exactly 1 | 1 |
| `msgstr[2]` | All remaining counts | 2, 3, 4, 5 |

**3 forms:** `mnk`

| Translation | Condition | Examples |
| --- | --- | --- |
| `msgstr[0]` | Exactly 0 | 0 |
| `msgstr[1]` | Exactly 1 | 1 |
| `msgstr[2]` | All remaining counts | 2, 3, 4, 5 |

**3 forms:** `he`, `iu`, `iw`, `naq`, `sat`, `se`, `sma`, `smi`, `smj`, `smn`, `sms`

| Translation | Condition | Examples |
| --- | --- | --- |
| `msgstr[0]` | Exactly 1 | 1 |
| `msgstr[1]` | All remaining counts | 0, 3, 4, 5 |
| `msgstr[2]` | Exactly 2 | 2 |

**3 forms:** `shi`

| Translation | Condition | Examples |
| --- | --- | --- |
| `msgstr[0]` | Exactly 0 or 1 | 0, 1 |
| `msgstr[1]` | From 2 to 10 | 2, 3, 4, 10 |
| `msgstr[2]` | All remaining counts | 11, 12, 13, 14 |

**3 forms:** `mo`, `ro`

| Translation | Condition | Examples |
| --- | --- | --- |
| `msgstr[0]` | Exactly 1 | 1 |
| `msgstr[1]` | Exactly 0, or ends in 01-19, except exactly 1 | 0, 2, 19, 101 |
| `msgstr[2]` | All remaining counts | 20, 100, 120, 200 |

**6 forms:** `cy`

| Translation | Condition | Examples |
| --- | --- | --- |
| `msgstr[0]` | Exactly 1 | 1 |
| `msgstr[1]` | Exactly 2 | 2 |
| `msgstr[2]` | Exactly 3 | 3 |
| `msgstr[3]` | All remaining counts | 4, 5, 7, 8 |
| `msgstr[4]` | Exactly 0 | 0 |
| `msgstr[5]` | Exactly 6 | 6 |

**4 forms:** `gd`

| Translation | Condition | Examples |
| --- | --- | --- |
| `msgstr[0]` | Exactly 1 or 11 | 1, 11 |
| `msgstr[1]` | Exactly 2 or 12 | 2, 12 |
| `msgstr[2]` | From 3 to 19, except 11 or 12 | 3, 10, 13, 19 |
| `msgstr[3]` | All remaining counts | 0, 20, 21, 22 |

**6 forms:** `kw`

| Translation | Condition | Examples |
| --- | --- | --- |
| `msgstr[0]` | Exactly 1 | 1 |
| `msgstr[1]` | Ends in 02, 22, 42, 62, 82, 40000, 60000, 80000 or 100000, or ends in 01000-20000 and multiple of 1,000 | 2, 22, 42, 62 |
| `msgstr[2]` | Ends in 03, 23, 43, 63 or 83 | 3, 23, 43, 63 |
| `msgstr[3]` | All remaining counts | 4, 20, 24, 40 |
| `msgstr[4]` | Exactly 0 | 0 |
| `msgstr[5]` | Ends in 01, 21, 41, 61 or 81, except exactly 1 | 21, 41, 61, 81 |

**5 forms:** `mt`

| Translation | Condition | Examples |
| --- | --- | --- |
| `msgstr[0]` | Exactly 1 | 1 |
| `msgstr[1]` | Exactly 0, or ends in 03-10 | 0, 3, 10, 103 |
| `msgstr[2]` | Ends in 11-19 | 11, 19, 111, 119 |
| `msgstr[3]` | All remaining counts | 20, 102, 120, 202 |
| `msgstr[4]` | Exactly 2 | 2 |

**4 forms:** `dsb`, `hsb`, `sl`

| Translation | Condition | Examples |
| --- | --- | --- |
| `msgstr[0]` | All remaining counts | 0, 5, 100, 105 |
| `msgstr[1]` | Ends in 01 | 1, 101, 201, 301 |
| `msgstr[2]` | Ends in 02 | 2, 102, 202, 302 |
| `msgstr[3]` | Ends in 03 or 04 | 3, 4, 103, 104 |

**4 forms:** `sgs`

| Translation | Condition | Examples |
| --- | --- | --- |
| `msgstr[0]` | Ends in 1, except 11 | 1, 21, 31, 41 |
| `msgstr[1]` | Exactly 2 | 2 |
| `msgstr[2]` | Ends in 2-9, except 11-19, and except exactly 2 | 3, 9, 22, 29 |
| `msgstr[3]` | All remaining counts | 0, 10, 20, 30 |

**5 forms:** `br`

| Translation | Condition | Examples |
| --- | --- | --- |
| `msgstr[0]` | Ends in 1, except 11, 71 or 91 | 1, 21, 31, 41 |
| `msgstr[1]` | All remaining counts | 0, 5, 8, 10 |
| `msgstr[2]` | Ends in 2, except 12, 72 or 92 | 2, 22, 32, 42 |
| `msgstr[3]` | Ends in 3, 4 or 9, except 10-19, 70-79 or 90-99 | 3, 4, 9, 23 |
| `msgstr[4]` | Multiple of 1,000,000, except 0 | 1,000,000, 2,000,000, 3,000,000, 4,000,000 |

**5 forms:** `ga`

| Translation | Condition | Examples |
| --- | --- | --- |
| `msgstr[0]` | Exactly 1 | 1 |
| `msgstr[1]` | Exactly 2 | 2 |
| `msgstr[2]` | From 3 to 6 | 3, 4, 5, 6 |
| `msgstr[3]` | From 7 to 10 | 7, 8, 9, 10 |
| `msgstr[4]` | All remaining counts | 0, 11, 12, 13 |

**4 forms:** `gv`

| Translation | Condition | Examples |
| --- | --- | --- |
| `msgstr[0]` | Ends in 1 | 1, 11, 21, 31 |
| `msgstr[1]` | Ends in 2 | 2, 12, 22, 32 |
| `msgstr[2]` | Ends in 00, 20, 40, 60 or 80 | 0, 20, 40, 60 |
| `msgstr[3]` | All remaining counts | 3, 10, 13, 19 |

**6 forms:** `ar`, `ars`

| Translation | Condition | Examples |
| --- | --- | --- |
| `msgstr[0]` | Exactly 0 | 0 |
| `msgstr[1]` | Exactly 1 | 1 |
| `msgstr[2]` | Exactly 2 | 2 |
| `msgstr[3]` | Ends in 03-10 | 3, 10, 103, 110 |
| `msgstr[4]` | Ends in 11-99 | 11, 99, 111, 199 |
| `msgstr[5]` | All remaining counts | 100, 102, 200, 202 |

<!-- plural-rules:end -->

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

### Notes and argument names for translators

A localized string can carry metadata for translators: a note explaining the meaning or usage of the message, and a name for each of its format arguments. The extraction writes them as comments of the message in the translation template. They are ignored at runtime, don't change the translation lookup, and changing them doesn't invalidate existing translations.

In C# and Razor, name a format argument with the `Arg(name, value)` helper, which returns its value unchanged. It is a static method of `OrchardCore.Localization.LocalizationMetadata`, provided by `OrchardCore.Abstractions`, and of the Orchard Core Razor base page, so views can call it directly. Pass a note with the `note` parameter of the `Plural` extensions, or call `Note(note)` on the localizer, which returns the same localizer, for the indexer and the other methods:

```csharp
using static OrchardCore.Localization.LocalizationMetadata;

return H.Plural(
    note: "Relative date shown for future dates.",
    count: Arg("years", years),
    singular: "in 1 year",
    plural: "in {0} years");

return S.Note("Greeting shown on the dashboard.")["Hello {0}", Arg("user name", user.Name)];
```

```razor
<p>@T.Note("Greeting shown on the dashboard.")["Hello {0}", Arg("user name", user.Name)]</p>
```

In Liquid, the `t` filter accepts named arguments, which are passed as format arguments in their order and whose names describe their placeholders, and a `note` argument, which is not a format argument:

```liquid
{{ "in {0} years" | t: years: time.years, note: "Relative date shown for future dates." }}
```

Both examples produce the same template entry, besides its context:

```po
#. Relative date shown for future dates.
#. Format arguments: {0}: years
#, csharp-format
msgctxt "OrchardCore.Example.DateTimeHumanizer"
msgid "in 1 year"
msgid_plural "in {0} years"
msgstr[0] ""
msgstr[1] ""
```

A `LocalizationSource` takes its note when it is declared, with the `note` parameter of `LocalizationSource.Create` and of its constructor. The note is not stored in the source, so it doesn't change its value or its equality. The names of its format arguments are given where it is formatted, with `Arg` in the arguments of `Localize` and `Plural`:

```csharp
public static readonly LocalizationSource Greeting = LocalizationSource.Create<Owner>(
    "Hello {0}",
    note: "Greeting shown on the dashboard.");

var text = stringLocalizerFactory.Localize(Greeting, Arg("user name", user.Name));
```

A format argument without a name is described by its C# expression, and the `count` of a plural by `count`. A comment starting with `// TRANSLATORS:` on the line above a C# call is added as a note too. Notes and argument names must be compile-time constants, or literal strings in Liquid, to be extracted.

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
| `OCLOC010` | Warning | A translator note or a format argument name is not a compile-time constant string, or a Liquid `note` argument is not a literal string. The message is still extracted, without it. |

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
