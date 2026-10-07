# JavaScript Localization (`IJSLocalizer`)

The `IJSLocalizer` infrastructure allows modules to expose PO-file-backed translations to their JavaScript / TypeScript assets, using the same `.po` files already used for server-side Razor or Liquid templates.

## Overview

When a Vue, TypeScript, or plain JavaScript component needs translated strings, it cannot call `IStringLocalizer` directly.  
`IJSLocalizer` bridges this gap: each module registers one implementation that knows how to map a **group name** to a dictionary of localized strings, which a Razor view or layout can serialize and pass into a scoped module entry point before the script runs.

## Interfaces and classes

| Type                                      | Location                                  | Description                                                                                                            |
|-------------------------------------------|-------------------------------------------|------------------------------------------------------------------------------------------------------------------------|
| `IJSLocalizer`                            | `OrchardCore.Localization.Abstractions`   | Contract that returns an `IDictionary<string, string>` for a requested group.                                          |
| `LocalizationOrchardHelperExtensions`     | `OrchardCore.Localization.Abstractions`   | Razor / Liquid helper (`Orchard.GetJSLocalizations(…)`) that aggregates all registered `IJSLocalizer` implementations. |
| `getTranslations()` / `setTranslations()` | `.scripts/bloom/helpers/localizations.ts` | Framework-agnostic helpers for seeding and reading a shared translations record in JavaScript or TypeScript.           |

## Registering a custom implementation

Create a class that implements `IJSLocalizer` and inject `IStringLocalizer<T>` to load translated strings from PO files.

```csharp
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Localization;
using OrchardCore.Localization;

namespace MyModule.Services;

public sealed class MyModuleJSLocalizer(IStringLocalizer<MyModuleJSLocalizer> S) : IJSLocalizer
{
    public IDictionary<string, string> GetLocalizations(string group)
    {
        if (string.Equals(group, "my-module", StringComparison.OrdinalIgnoreCase))
        {
            return new Dictionary<string, string>
            {
                { "Save",   S["Save"].Value },
                { "Cancel", S["Cancel"].Value },
                { "Delete", S["Delete"].Value },
            };
        }

        // Return null for groups this implementation does not own.
        return null;
    }
}
```

Then register it in your module's `Startup.cs`:

```csharp
public override void ConfigureServices(IServiceCollection services)
{
    // Use AddScoped so IStringLocalizer<T> is resolved per-request (culture-aware).
    services.AddScoped<IJSLocalizer, MyModuleJSLocalizer>();
}
```

## Passing localizations to the browser

In a Razor view or layout, call `Orchard.GetJSLocalizations(…)` and pass the serialized result as data to your root component. This keeps the localizations scoped to that app instance instead of using an inline script bootstrap.

```cshtml
@{
    var localizations = Orchard.GetJSLocalizations("my-module");
}

<my-module-app localizations="@Json.Serialize(localizations)"></my-module-app>
```

In this example, `<my-module-app>` is the tag your app renders for its root Vue component in Razor markup. Razor HTML-encodes the attribute value, and the component parses the JSON string once before seeding the shared translations store.

### Multiple groups at once

You can request translations from multiple groups in a single call. The dictionaries are merged — later registrations win on key conflicts.

```cshtml
@{
    var localizations = Orchard.GetJSLocalizations("my-module", "shared-ui");
}
```

---

## Using localizations in JavaScript or TypeScript

Orchard Core ships framework-agnostic helpers at `.scripts/bloom/helpers/localizations.ts`.

### The shared translations store

```typescript
const translations: Record<string, string> = {};

export function getTranslations(): Record<string, string> {
    return translations;
}

export function setTranslations(t: Record<string, string>): void {
    Object.assign(translations, t);
}
```

The module keeps a single shared object alive for the lifetime of the JavaScript module. Seed it once near the root of your app, then read it anywhere in your front-end code.

### Seeding translations in the root component

Initialize the store in your root component with the serialized localizations passed from Razor. Because the value is passed through an HTML attribute, the component receives it as a string and parses it once before seeding the shared store:

```typescript
import { setTranslations } from "@orchardcore/bloom/helpers/localizations";

const props = defineProps({
    localizations: {
        type: String,
        required: true,
    },
});

try {
    setTranslations(JSON.parse(props.localizations));
} catch (error) {
    console.warn("Failed to parse localizations:", error);
}
```

### Reading translations in modules, services, or components

```typescript
import { getTranslations } from "@orchardcore/bloom/helpers/localizations";

const t = getTranslations();

console.log(t["Save"]);
```

If your UI framework needs reactivity, wrap `getTranslations()` in framework-specific state inside your app. The shared Orchard helper stays framework agnostic.

### Media App pattern

The Vue 3 Media App on `skrypt/vue-3` uses the helper in a framework-agnostic way:

1. Razor serializes translations and passes them to the root component.
2. The root component parses and stores them with `setTranslations(...)`.
3. Components and services read them through `getTranslations()`.

#### Razor

```cshtml
@{
    var jsLocalizations = Orchard.GetJSLocalizations("media-gallery");
}

<div id="media-gallery">
    <media-gallery
        translations="@Json.Serialize(jsLocalizations).ToString()"
        base-path="@Href("~")">
    </media-gallery>
</div>
```

#### Root component

```typescript
import { getTranslations, setTranslations } from "@orchardcore/bloom/helpers/localizations";

const props = defineProps({
    translations: {
        type: String,
        required: true,
    },
});

const translations = getTranslations();

if (props.translations) {
    try {
        setTranslations(typeof props.translations === "string" ? JSON.parse(props.translations) : props.translations);
    } catch (e) {
        console.warn("Failed to parse translations:", e);
    }
}

const t = translations;
```

#### Anywhere else in the app

```typescript
import { getTranslations } from "@orchardcore/bloom/helpers/localizations";

const t = getTranslations();
```

---

## PO file format

`IJSLocalizer` uses standard Orchard Core PO files for the actual translation strings, exactly the same format used for server-side strings. Place PO files at:

```
/Localization/<ModuleId>/<CultureName>.po
```

Example `fr.po` for the strings above:

```po
msgid "Save"
msgstr "Enregistrer"

msgid "Cancel"
msgstr "Annuler"

msgid "Delete"
msgstr "Supprimer"
```

See [Install localization files](../../../guides/install-localization-files/README.md) for full PO file placement rules.

## Multiple implementations

More than one `IJSLocalizer` can be registered at the same time. `GetJSLocalizations` calls each registered implementation once per requested group, in the same order the groups were passed, and merges the results. Each implementation returns `null` (or an empty dictionary) for groups it does not own.

This allows a host application to combine translations from multiple independent modules without any coupling between them:

```csharp
// Module A
services.AddScoped<IJSLocalizer, ModuleAJSLocalizer>();

// Module B (registered independently)
services.AddScoped<IJSLocalizer, ModuleBJSLocalizer>();
```

```cshtml
@* Merges translations from both Module A and Module B *@
var localizations = Orchard.GetJSLocalizations("module-a", "module-b");
```

## Extracting strings with PoExtractor

The string keys inside `GetLocalizations` are plain `IStringLocalizer` calls, which means they are detected automatically by [PoExtractor](https://github.com/OrchardCoreContrib/OrchardCoreContrib.PoExtractor):

```bash
dotnet tool install --global OrchardCoreContrib.PoExtractor
extractpo src/MyModule output/Localization -l C#
```
