# Data Localization (`OrchardCore.DataLocalization`)

This module provides a database-backed localization system for translating dynamic content that cannot be handled by static PO files, such as:

- Content Type and Content Field display names
- Permission descriptions that are not translated with PO files, and permission categories
- Any custom dynamic strings via `ILocalizationDataProvider`

## Features

- **UI Localization Overrides** (`OrchardCore.DataLocalization.Ui`): Optional database overrides for UI strings discovered from embedded POT catalogs
- **Translation Editor**: Vue3-based admin UI to edit translations per culture
- **Statistics Dashboard**: Track translation progress by culture and category
- **Per-Culture Permissions**: Assign translation rights to specific cultures
- **Extensible Providers**: Add custom data sources via `ILocalizationDataProvider`

## Getting Started

1. Enable the **Data Localization** feature in the admin under **Tools → Features**
2. Navigate to **Settings → Localization → Dynamic Translations** in the admin menu
3. Select a culture from the dropdown
4. Edit translations for each category (Permissions, Content Types, etc.)
5. Use the **Statistics** page to track translation progress

## UI Localization Overrides

Enable **UI Localization Overrides** in addition to Data Localization, and open
**Settings → Localization → UI Translations**. Application and module assemblies must
be built with localization extraction enabled. The editor discovers
`{AssemblyName}.Localization.pot` resources in these assemblies and their references;
external PO files do not define the editable catalog.

Choose a configured supported culture. Browse all extracted entries, including entries
without database translations, and filter by assembly, override status, identifier,
context, override text, or extracted comments, flags and source references. Identifiers and contexts
are case-sensitive. Entries with the same identifier in different contexts remain independent.

Saving an override replaces the PO translation for that culture and context.
**Restore fallback** removes it, restoring the standard PO, parent-culture (when enabled),
and source-string fallback. Existing culture fallback order is preserved: an exact-culture
PO translation still wins over a parent-culture override. UI overrides are stored separately
from dynamic data translations and do not change the `d` Liquid filter.

The feature uses the existing localization manager and localizers, so string, HTML, Razor,
DataAnnotations and Liquid `t` localization share the overrides. Overrides are **plain text**:
HTML localizers encode translation text and continue encoding format arguments. This does not
change the existing HTML behavior of trusted deployed PO translations. Composite-format
translations cannot introduce arguments absent from the extracted source.

Plural entries require all forms selected by Orchard's configured integer plural rule, in
zero-based rule order. The editor provides the culture-specific number of inputs, not the
two placeholder forms in the POT file. Partial or empty forms are rejected; remove the
entire override to restore fallback.

### Permissions and transfer

The feature reuses Data Localization permissions. `ViewDynamicTranslations` grants catalog
and export access; `ManageTranslations` grants edits/imports for all cultures, and
`ManageTranslations_{culture}` grants edits/imports for that configured culture.
Culture-specific translators without the view permission can browse and export only their
authorized cultures.
All mutation endpoints require anti-forgery tokens.

**Export overrides** downloads a UTF-8 PO file containing only the selected culture's
database overrides, preserving contexts and plural identifiers. Import targets the selected
culture and merges entries, leaving other overrides untouched. A wholly empty `msgstr`
entry removes its override. All entries are validated before any changes are stored:
unknown identifiers/contexts, mismatched plural sources, duplicate entries, fuzzy entries,
malformed fields, invalid format strings and incomplete plural translations reject the
entire import. Imports are limited to 2 MB.

### Storage, caching and extension points

`UiTranslationsDocument` uses Orchard's tenant-scoped document manager, including immutable
snapshots, concurrency checks and distributed-cache versioning. The runtime pins a committed
snapshot for each shell scope and caches overlaid dictionaries by snapshot and fallback
dictionary identity. Subsequent scopes see committed updates and removals through the
document manager's normal invalidation mechanism; no application restart is required.

Use `IUiLocalizationCatalog` to discover available identifiers and metadata, and
`IUiTranslationsManager` to validate, update, import and export translations from automation
or other admin workflows. These services do not bypass catalog validation. The framework's
`ITranslationOverrideProvider` hook applies versioned overlays after standard providers
without modifying shared PO dictionaries or replacing core localizers. Disabling the
feature leaves the documents stored but stops applying overrides.
Custom overlays must mark untrusted values in `CultureDictionary.PlainTextTranslations`
so HTML localizers encode them.

## Admin UI

### Translation Editor

The translation editor displays all translatable strings grouped by category:

- **Culture Selector**: Choose which culture to edit translations for
- **Search**: Filter strings by original text or translation
- **Category Filter**: Focus on a specific category
- **Auto-save**: Toggle automatic saving (enabled by default, saves after 2 seconds of inactivity)
- **Save Button**: Manually save all changes

Each category is displayed as an accordion section showing:

- The original string (key)
- An input field for the translated value
- Translation progress indicators

### Statistics Dashboard

The statistics dashboard shows translation completion progress:

- **Overall Progress**: Total translation progress across all cultures
- **By Culture**: Progress bar and completion count for each supported culture
- **By Category**: Detailed breakdown per category for a selected culture

Progress bars are color-coded:

- 🟢 Green (≥75%): Good progress
- 🟡 Yellow (25-74%): In progress
- 🔴 Red (<25%): Needs attention

## Permissions

| Permission                     | Description                                                                 |
|--------------------------------|-----------------------------------------------------------------------------|
| `ViewTranslations`             | View translations and statistics (read-only access)                         |
| `ManageTranslations`           | Edit translations for all cultures                                          |
| `ManageTranslations_{culture}` | Edit translations for a specific culture (e.g., `ManageTranslations_fr-FR`) |

### Permission Hierarchy

```
ViewTranslations                      (Read-only access)
ManageTranslations                    (Full edit access - implies ViewTranslations)
├── ManageTranslations_fr-FR          (Edit French - implies ViewTranslations)
├── ManageTranslations_es-ES          (Edit Spanish - implies ViewTranslations)
└── ManageTranslations_{culture}      (Dynamically generated per supported culture)
```

### Default Role Assignments

- **Administrator**: `ManageTranslations` (full access)
- **Editor**: `ViewTranslations` (read-only)

### Assigning Culture-Specific Permissions

To allow a user to only edit translations for a specific culture:

1. Go to **Access Control → Roles**
2. Edit the desired role
3. Under **Data Localization**, check the culture-specific permission (e.g., "Manage fr-FR translations")

## Built-in Data Providers

The module includes these built-in `ILocalizationDataProvider` implementations:

### Admin Menu Provider

Provides admin menu display names for translation.

Admin menu providers, including content-type admin menu nodes, require both `OrchardCore.DataLocalization` and `OrchardCore.AdminMenu`. Content type and content field display names remain available for translation when `OrchardCore.Contents` and `OrchardCore.DataLocalization` are enabled, even if `OrchardCore.AdminMenu` is disabled.

- **Context**: `Admin Menus` or `Admin Menus:{menuName}`
- **Strings**: Display names of all admin menu / sub menu items

### Content Type Provider

Provides content type display names for translation.

- **Context**: `Content Types`
- **Strings**: Display names of all content types

### Content Field Provider

Provides content field display names for translation.

- **Context**: `Content Fields:{fieldName}`
- **Strings**: Display names of all content fields

### Permissions Provider

Provides permission descriptions and categories for translation.

- **Context**: `Permissions` or `Permissions:{groupName}`
- **Strings**: The categories of the permissions, and the descriptions that are not templates and have no PO translation context. Descriptions created with `LocalizationSource.Create(description, typeof(DeclaringType))` are translated with PO files and are not listed. See [Declaring permissions](../Roles/README.md#declaring-permissions).

### Search Provider

Provides search-related strings for translation.

- **Context**: `Search`
- **Strings**: Display names of all search-related items

## Creating Custom Providers

Implement `ILocalizationDataProvider` to add your own translatable strings:

```csharp
using OrchardCore.Localization.Data;

public class MyCustomLocalizationDataProvider : ILocalizationDataProvider
{
    public Task<IEnumerable<DataLocalizedString>> GetDescriptorsAsync()
    {
        var strings = new List<DataLocalizedString>
        {
            new DataLocalizedString("My Category", "Hello World", string.Empty),
            new DataLocalizedString("My Category", "Welcome Message", string.Empty),
        };

        return Task.FromResult<IEnumerable<DataLocalizedString>>(strings);
    }
}
```

Register in your module's `Startup.cs`:

```csharp
public override void ConfigureServices(IServiceCollection services)
{
    services.AddScoped<ILocalizationDataProvider, MyCustomLocalizationDataProvider>();
}
```

The strings will automatically appear in the Translation Editor under your specified category.

### Localization Key Guidelines

!!! warning "Important"
    The localization **key** (the `name` parameter in `DataLocalizedString`) should always be a **human-readable display value**, not a technical identifier.

When a translation does not exist for the current culture, `IDataLocalizer` returns the **key itself** as the displayed value. This means:

| Key Type        | Example Key              | Displayed if untranslated | Result                   |
|-----------------|--------------------------|---------------------------|--------------------------|
| ✅ Display value | `"Welcome Message"`      | `Welcome Message`         | Good - readable          |
| ❌ Technical ID  | `"welcome_msg_001"`      | `welcome_msg_001`         | Bad - confusing to users |
| ❌ Code constant | `"CONTENT_TYPE_ARTICLE"` | `CONTENT_TYPE_ARTICLE`    | Bad - not user-friendly  |

**Best practices:**

1. Use the actual display text as the key (e.g., `"Article"`, `"Manage Users"`)
2. Use the context parameter to disambiguate keys that might conflict (e.g., context `"Content Types"` vs `"Permissions"`)
3. If you have technical identifiers, map them to display values before creating the `DataLocalizedString`

**Example - Correct approach:**

```csharp
// Good: Key is the display value
new DataLocalizedString("Menu Items", "Dashboard", string.Empty)
new DataLocalizedString("Menu Items", "User Settings", string.Empty)
```

**Example - Incorrect approach:**

```csharp
// Bad: Key is a technical identifier - will display "menu.dashboard" if untranslated
new DataLocalizedString("Menu Items", "menu.dashboard", string.Empty)
```

## Recipe Step

Import/export translations via recipes:

### Import Translations

```json
{
  "name": "Translations",
  "Translations": {
    "fr-FR": [
      { "Context": "Permissions", "Key": "Manage HTTPS", "Value": "Gérer HTTPS" },
      { "Context": "Content Types", "Key": "Article", "Value": "Article" }
    ],
    "es-ES": [
      { "Context": "Permissions", "Key": "Manage HTTPS", "Value": "Gestionar HTTPS" }
    ]
  }
}
```

### Deployment Step

Use the **All Data Translations** deployment step to export all translations for backup or transfer between environments.

## When to Use

| Use Case                         | Recommended Approach                     |
|----------------------------------|------------------------------------------|
| Static UI strings in views/code  | PO files (`OrchardCore.Localization`)    |
| Content type/field display names | Data Localization                        |
| Permission descriptions          | Data Localization                        |
| Admin menu items                 | Data Localization (with custom provider) |
| User-defined content             | `OrchardCore.ContentLocalization` module |
| Database-stored dynamic strings  | Data Localization (with custom provider) |

## API Reference

### ILocalizationDataProvider

```csharp
public interface ILocalizationDataProvider
{
    Task<IEnumerable<DataLocalizedString>> GetDescriptorsAsync();
}
```

### DataLocalizedString

```csharp
public class DataLocalizedString
{
    public DataLocalizedString(string context, string name, string value);
    
    public string Context { get; }  // Category/group name
    public string Name { get; }     // Original string (key)
    public string Value { get; }    // Translated value
}
```

### ITranslationsManager

```csharp
public interface ITranslationsManager
{
    Task<TranslationsDocument> LoadTranslationsDocumentAsync();
    Task<TranslationsDocument> GetTranslationsDocumentAsync();
    Task RemoveTranslationAsync(string name);
    Task UpdateTranslationAsync(string name, IEnumerable<Translation> translations);
}
```

## Caching

Translations are cached by the `LocalizationManager`. When translations are updated through the admin UI, the cache is automatically invalidated. If you update translations programmatically, you may need to signal a cache refresh.

## Dependencies

- `OrchardCore.Localization` - Required for culture support and base localization infrastructure

## Using IDataLocalizer in Views

To display translated dynamic strings in Razor views, inject `IDataLocalizer`:

```cshtml
@using OrchardCore.Localization.Data
@inject IDataLocalizer D

<h1>@D["Article", "Content Types"]</h1>
<p>@D["Manage HTTPS", "Permissions"]</p>
```

### HTML Encoding

`DataLocalizedString` converts implicitly to `string`. When used with Razor's `@` syntax, the output is **automatically HTML-encoded**, making it safe against XSS attacks. Translated values should contain plain text only, not HTML markup.

```cshtml
@* Safe - HTML encoded automatically *@
<span>@D["My String", "My Context"]</span>

@* If you need the raw value (rare) *@
@{
    string translated = D["My String", "My Context"];
}
```

### Comparison with IStringLocalizer

| Feature       | `IStringLocalizer` (T) | `IDataLocalizer` (D)                      |
|---------------|------------------------|-------------------------------------------|
| Source        | PO files (static)      | Database (dynamic)                        |
| HTML encoding | Auto-encoded by Razor  | Auto-encoded by Razor                     |
| Pluralization | Supported              | Not supported                             |
| Arguments     | `T["Hello {0}", name]` | `D["Hello {0}", "Context", name]`         |
| Use case      | Static UI strings      | Dynamic data (content types, permissions) |

## Liquid filters

For more information on using data locaization filters in Liquid templates, see the [Liquid Localization filters documentation](../Liquid/README.md#localization-filters).
