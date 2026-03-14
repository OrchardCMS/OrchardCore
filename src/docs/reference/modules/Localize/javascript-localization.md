# JavaScript Localization (`IJSLocalizer`)

The `IJSLocalizer` infrastructure allows modules to expose PO-file-backed translations to their JavaScript / TypeScript assets, using the same `.po` files already used for server-side Razor or Liquid templates.

## Overview

When a Vue, TypeScript, or plain JavaScript component needs translated strings, it cannot call `IStringLocalizer` directly.  
`IJSLocalizer` bridges this gap: each module registers one implementation that knows how to map a **group name** to a dictionary of localized strings, which a Razor layout then serializes into the page (e.g. as a JSON variable) before the script runs.

## Interfaces and classes

| Type | Location | Description |
|------|----------|-------------|
| `IJSLocalizer` | `OrchardCore.Localization.Abstractions` | Contract that returns a `Dictionary<string, string>` for requested groups. |
| `NullJSLocalizer` | `OrchardCore.Localization.Abstractions` | Default no-op implementation that always returns an empty dictionary. |
| `LocalizationOrchardHelperExtensions` | `OrchardCore.Localization.Abstractions` | Razor / Liquid helper (`Orchard.GetJSLocalizations(…)`) that aggregates all registered `IJSLocalizer` implementations. |
| `useLocalizations()` | `.scripts/bloom/helpers/localizations.ts` | Vue 3 composable that wraps translations in a reactive object so components update automatically when the locale changes. |

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
    public Dictionary<string, string> GetLocalizations(string[] groups)
    {
        if (groups.Contains("my-module", StringComparer.OrdinalIgnoreCase))
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

In a Razor view or layout, call `Orchard.GetJSLocalizations(…)` and serialize the result as a JavaScript variable so your front-end code can consume it without extra HTTP requests.

```cshtml
@using System.Text.Json

@{
    var localizations = Orchard.GetJSLocalizations("my-module");
}

<script>
    window.__localizations = @Html.Raw(JsonSerializer.Serialize(localizations));
</script>
```

### Multiple groups at once

You can request translations from multiple groups in a single call. The dictionaries are merged — later registrations win on key conflicts.

```cshtml
@{
    var localizations = Orchard.GetJSLocalizations("my-module", "shared-ui");
}
```

---

## Using localizations in a Vue 3 app

Orchard Core ships a ready-made Vue 3 composable at `.scripts/bloom/helpers/localizations.ts`.

### The `useLocalizations()` composable

```typescript
import { reactive } from "vue";

const translations = reactive<Record<string, string>>({});

export function useLocalizations() {
    const setTranslations = (t: Record<string, string>) => {
        Object.assign(translations, t);
    };

    return { translations, setTranslations };
}
```

The singleton `translations` object is **reactive** — any component that reads from it will automatically re-render if the translations are swapped at runtime.

### Seeding translations in the app entry point

Call `setTranslations` once in the root component (or in `main.ts`) using the variable serialized by the Razor view:

```typescript
// main.ts
import { createApp } from "vue";
import App from "./App.vue";
import { useLocalizations } from "@orchardcore/bloom/helpers/localizations";

const app = createApp(App);

const { setTranslations } = useLocalizations();
setTranslations((window as any).__localizations ?? {});

app.mount("#app");
```

### Reading translations inside a component

```vue
<script setup lang="ts">
import { useLocalizations } from "@orchardcore/bloom/helpers/localizations";

const { translations: t } = useLocalizations();
</script>

<template>
  <button>{{ t["Save"] }}</button>
  <button>{{ t["Cancel"] }}</button>
</template>
```

Because `translations` is a Vue reactive object, the template updates automatically if `setTranslations` is called again (e.g. after a locale change via an API call).

---

## Using localizations in a React app

React does not use Vue's reactive system. The simplest approach is to pass the plain `window.__localizations` object through **React Context**, or access it directly since page-level translations are typically set once before the bundle runs.

### Minimal direct access

```tsx
// localizations.ts (React project)
export const t = (window as any).__localizations as Record<string, string> ?? {};
```

```tsx
// SaveButton.tsx
import { t } from "./localizations";

export function SaveButton() {
    return <button>{t["Save"]}</button>;
}
```

### Context-based approach (recommended for larger apps)

```tsx
// LocalizationsContext.tsx
import { createContext, useContext } from "react";

export const LocalizationsContext = createContext<Record<string, string>>({});

export function useT() {
    return useContext(LocalizationsContext);
}
```

```tsx
// main.tsx
import React from "react";
import ReactDOM from "react-dom/client";
import App from "./App";
import { LocalizationsContext } from "./LocalizationsContext";

const localizations = (window as any).__localizations ?? {};

ReactDOM.createRoot(document.getElementById("app")!).render(
    <LocalizationsContext.Provider value={localizations}>
        <App />
    </LocalizationsContext.Provider>
);
```

```tsx
// SaveButton.tsx
import { useT } from "./LocalizationsContext";

export function SaveButton() {
    const t = useT();
    return <button>{t["Save"]}</button>;
}
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

See [Install localization files](../../guides/install-localization-files/README.md) for full PO file placement rules.

## Multiple implementations

More than one `IJSLocalizer` can be registered at the same time. `GetJSLocalizations` calls each of them in registration order, and merges their results. Each implementation returns `null` (or an empty dictionary) for groups it does not own — this is how `NullJSLocalizer` behaves.

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
