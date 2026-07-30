---
name: orchardcore-tenants
description: Works with OrchardCore multi-tenancy — shells, ShellSettings, creating/configuring tenants programmatically, running code in a tenant scope, feature profiles, and data isolation. Use when the user needs to create or manage tenants, run code inside a tenant, pre-configure tenants, or understand shell lifecycle.
---

# OrchardCore Tenants (Multi-tenancy)

This skill guides you through OrchardCore multi-tenancy following project conventions.

OrchardCore runs many isolated sites ("tenants") in one process. Each tenant = a **shell**: its own DI container, configuration, enabled features, and (optionally) its own database or table prefix. The `Default` tenant is special — it's created first and manages the others.

## Core concepts

| Type | Role |
|------|------|
| `ShellSettings` | a tenant's identity + config (`Name`, `State`, `RequestUrlHost`, `RequestUrlPrefix`, DB info) |
| `ShellContext` | a running tenant: its `ServiceProvider`, blueprint, request pipeline |
| `ShellScope` | a unit-of-work scope inside a tenant (resolve services here) |
| `IShellHost` | manages all shells: create, reload, list, get scopes |
| `IShellSettingsManager` | persists `ShellSettings` |

### Tenant states

`Uninitialized` → (setup) → `Running`. Also `Initializing`, `Disabled`, `Invalid`. A tenant configured but not yet set up is `Uninitialized` and appears in the Tenants list awaiting setup.

## Decide what you're doing

| Goal | Approach |
|------|----------|
| Create a tenant in code | `IShellSettingsManager.CreateDefaultSettings()` + `IShellHost.UpdateShellSettingsAsync` |
| Run code inside a tenant | `IShellHost.GetScopeAsync(name)` + `scope.UsingAsync(...)` |
| Pre-declare tenants without UI | `appsettings`/configuration tenant section |
| Restrict a tenant's features | Feature Profiles |
| Isolate tenant data | separate connection string, or shared DB + table prefix/schema |

## Workflow A: create a tenant programmatically

```csharp
using var shellSettings = _shellSettingsManager
    .CreateDefaultSettings()
    .AsUninitialized()
    .AsDisposable();

shellSettings.Name = "acme";
shellSettings.RequestUrlPrefix = "acme";        // site at /acme
shellSettings.RequestUrlHost = "";              // or a host header

shellSettings["ConnectionString"] = connectionString;
shellSettings["DatabaseProvider"] = "Sqlite";
shellSettings["TablePrefix"] = "acme";
shellSettings["RecipeName"] = "Blog";           // recipe run at setup
shellSettings["Secret"] = Guid.NewGuid().ToString();

await _shellHost.UpdateShellSettingsAsync(shellSettings);
```

`UpdateShellSettingsAsync` stamps a new `VersionId`, saves, and reloads the shell. The tenant is now `Uninitialized` — it still needs **setup** (admin user, run recipe), done via the Setup UI/API or `ISetupService`.

Or via the Tenants API: `POST /api/tenants/create` then `POST /api/tenants/setup`.

## Workflow B: run code in a tenant scope

To resolve tenant-scoped services (content manager, session, etc.) for a specific tenant:

```csharp
var shellScope = await _shellHost.GetScopeAsync("acme");
await shellScope.UsingAsync(async scope =>
{
    var contentManager = scope.ServiceProvider.GetRequiredService<IContentManager>();
    // ... work inside the "acme" tenant ...
});
```

Inside the callback, `ShellScope.Services` / `ShellScope.Current` resolve to this tenant. Deferred tasks run on scope disposal. Never resolve tenant services off the root provider — you'll get the wrong (or no) tenant.

## Workflow C: pre-configure tenants via configuration

In `appsettings.json` (or any config source), declare a tenant so it exists before any UI interaction:

```json
{
  "OrchardCore": {
    "acme": {
      "State": "Uninitialized",
      "RequestUrlPrefix": "acme",
      "ConnectionString": "...",
      "DatabaseProvider": "SqlConnection"
    }
  }
}
```

Per-tenant module config goes under the tenant name too:

```json
{ "OrchardCore": { "Default": { "OrchardCore_Media": { /* ... */ } } } }
```

## Quick Reference

### IShellHost

| Member | Purpose |
|--------|---------|
| `GetOrCreateShellContextAsync(settings)` | get/build a tenant's context |
| `GetScopeAsync(settings\|name)` | a scope to run code in |
| `UpdateShellSettingsAsync(settings)` | save + reload a tenant |
| `ReloadShellContextAsync(settings)` | rebuild after config change |
| `ListShellContexts()` / `TryGetSettings(name, out)` | enumerate / look up |

### ShellSettings state extensions

`AsUninitialized()`, `AsRunning()`, `AsDisabled()`, plus `IsRunning()`, `IsUninitialized()`, etc.

### Settings keys (bag)

`ConnectionString`, `DatabaseProvider`, `TablePrefix`, `Schema`, `RecipeName`, `FeatureProfile`, `Secret`, `Category`, `Description`.

### Data isolation options

| Strategy | How |
|----------|-----|
| Separate database | distinct `ConnectionString` per tenant |
| Shared DB, table prefix | same DB + unique `TablePrefix` |
| Shared DB, schema | `Schema` per tenant (SQL Server) |

`OrchardCore_Tenants` config can enforce: `RequireTablePrefix`, `TablePrefixPattern`, `SchemaPattern` (templated with `ShellSettings`).

### Feature Profiles

Feature `OrchardCore.Tenants.FeatureProfiles`. Profiles are include/exclude rules limiting which features a tenant may enable:

```json
{
  "name": "FeatureProfiles",
  "FeatureProfiles": {
    "minimal": { "FeatureRules": [ { "Rule": "Exclude", "Expression": "OrchardCore.MiniProfiler" } ] }
  }
}
```

Assign via `shellSettings["FeatureProfile"]`.

## Gotchas

- Creating settings ≠ a usable site. A new tenant is `Uninitialized`; it must be **set up** (recipe + admin user) before it runs.
- Always `using`/dispose the `ShellSettings` from `CreateDefaultSettings()` (`.AsDisposable()`).
- Only the `Default` tenant (`ShellSettings.DefaultShellName`) can manage other tenants.
- Resolve services through a `ShellScope`, not the application root provider — tenant DI is a child container per shell.
- Each tenant runs each module's `Startup.ConfigureServices` independently; services are per-tenant singletons, not app-wide.

## References

- `references/shells.md` — shell lifecycle, scope mechanics, container factory, config sources
- `src/docs/reference/modules/Tenants/README.md` (repo)
- `src/docs/reference/modules/Configuration/README.md` (repo)
- `src/docs/reference/modules/Shells/README.md` (repo)
- `AGENTS.md` (repo root) — build commands
