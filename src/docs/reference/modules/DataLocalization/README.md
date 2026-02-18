# Data Localization (`OrchardCore.DataLocalization`)

This module provides a database-backed localization system for translating dynamic content that cannot be handled by static PO files, such as:

- Content Type and Content Field display names
- Permission names and descriptions
- Any custom dynamic strings via `ILocalizationDataProvider`

## Features

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

| Permission | Description |
|------------|-------------|
| `ViewTranslations` | View translations and statistics (read-only access) |
| `ManageTranslations` | Edit translations for all cultures |
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

### Content Type Provider

Provides content type display names for translation.

- **Context**: `Content Types`
- **Strings**: Display names of all content types

### Content Field Provider

Provides content field display names for translation.

- **Context**: `Content Fields`
- **Strings**: Display names of all content fields

### Permissions Provider

Provides permission descriptions for translation.

- **Context**: `Permissions`
- **Strings**: Descriptions of all non-template permissions

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

| Key Type | Example Key | Displayed if untranslated | Result |
|----------|-------------|---------------------------|--------|
| ✅ Display value | `"Welcome Message"` | `Welcome Message` | Good - readable |
| ❌ Technical ID | `"welcome_msg_001"` | `welcome_msg_001` | Bad - confusing to users |
| ❌ Code constant | `"CONTENT_TYPE_ARTICLE"` | `CONTENT_TYPE_ARTICLE` | Bad - not user-friendly |

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

| Use Case | Recommended Approach |
|----------|---------------------|
| Static UI strings in views/code | PO files (`OrchardCore.Localization`) |
| Content type/field display names | Data Localization |
| Permission descriptions | Data Localization |
| Admin menu items | Data Localization (with custom provider) |
| User-defined content | `OrchardCore.ContentLocalization` module |
| Database-stored dynamic strings | Data Localization (with custom provider) |

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

| Feature | `IStringLocalizer` (T) | `IDataLocalizer` (D) |
|---------|------------------------|----------------------|
| Source | PO files (static) | Database (dynamic) |
| HTML encoding | Auto-encoded by Razor | Auto-encoded by Razor |
| Pluralization | Supported | Not supported |
| Arguments | `T["Hello {0}", name]` | `D["Hello {0}", "Context", name]` |
| Use case | Static UI strings | Dynamic data (content types, permissions) |
