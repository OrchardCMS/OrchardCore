# Shell & tenant reference

## Objects

| Type | File (abstractions) | Holds |
|------|---------------------|-------|
| `ShellSettings` | `Shell/ShellSettings.cs` | `Name`, `State`, `RequestUrlHost`, `RequestUrlPrefix`, `TenantId`, `VersionId`, bag values |
| `ShellContext` | `Shell/Builders/ShellContext.cs` | `ShellSettings`, `ShellBlueprint`, `ServiceProvider`, pipeline; ref-counts active scopes |
| `ShellScope` | `Shell/Scope/ShellScope.cs` | per-request/unit-of-work scope; `Current`, `Services`, `Context` statics |
| `IShellHost` | `Shell/IShellHost.cs` | lifecycle of all shells |
| `IShellSettingsManager` | `Shell/IShellSettingsManager.cs` | load/save/remove settings, `CreateDefaultSettings()` |

`ShellSettings.DefaultShellName = "Default"` — the management tenant.

## Tenant states

```csharp
public enum TenantState
{
    Uninitialized, // declared, not set up
    Initializing,  // setup in progress
    Running,       // live
    Disabled,      // set up but turned off
    Invalid        // bad settings
}
```

Transitions via extensions: `AsUninitialized()`, `AsRunning()`, `AsDisabled()` (and `IsRunning()` etc.).

## Where settings live

- Defaults for all tenants: `App_Data/tenants.json`.
- Per tenant: `App_Data/Sites/{tenant}/appsettings.json`.
- Plus any configuration source (appsettings, env, Azure/DB shell providers — see the Shells module). Config is layered: regular sources first, then `tenants.json`, then the per-site file.

## Creating a tenant

```csharp
using var settings = _shellSettingsManager
    .CreateDefaultSettings()   // seed from configuration
    .AsUninitialized()
    .AsDisposable();

settings.Name = "acme";
settings.RequestUrlPrefix = "acme";
settings["ConnectionString"] = "...";
settings["DatabaseProvider"] = "Sqlite";
settings["TablePrefix"] = "acme";
settings["RecipeName"] = "Blog";
settings["Secret"] = Guid.NewGuid().ToString();

await _shellHost.UpdateShellSettingsAsync(settings);
```

`UpdateShellSettingsAsync` (in `ShellHost`):

```csharp
settings.VersionId = IdGenerator.GenerateId();
await _shellSettingsManager.SaveSettingsAsync(settings);
await ReloadShellContextAsync(settings);
```

The tenant is now `Uninitialized`. **Setup** (create DB schema, admin user, run the recipe) happens separately — via the Setup UI, `POST /api/tenants/setup`, or `ISetupService`. Setup parameters: `SiteName`, `AdminUsername`, `AdminEmail`, `AdminPassword`, `DatabaseProvider`, `DatabaseConnectionString`, `DatabaseTablePrefix`.

## Running code in a scope

```csharp
public static Task<ShellScope> GetScopeAsync(this IShellHost shellHost, string tenant)
    => shellHost.GetScopeAsync(shellHost.GetSettings(tenant));
```

```csharp
var scope = await _shellHost.GetScopeAsync("acme");
await scope.UsingAsync(async s =>
{
    var svc = s.ServiceProvider.GetRequiredService<IMyService>();
    await svc.DoWorkAsync();
});
```

`ShellScope.UsingAsync` activates the shell, runs your delegate, then terminates the scope (flushing deferred tasks). Statics inside the delegate: `ShellScope.Current`, `ShellScope.Services`, `ShellScope.Context`. There's also `ShellScope.UsingChildScopeAsync(tenant, execute)`.

## Per-tenant DI

`ShellContainerFactory` builds each tenant's `IServiceProvider` as a **child container** off the application services:

```csharp
var tenantServiceCollection = _serviceProvider.CreateChildContainer(_applicationServices);
// ShellSettings registered as a singleton in the tenant container
// each module Startup.ConfigureServices runs per tenant
```

Implications:
- A service registered as singleton is one-per-tenant, not one-per-app.
- Resolving from the root provider gives app-level services only — never tenant content/session.

## Feature Profiles

Feature `OrchardCore.Tenants.FeatureProfiles` registers `IFeatureProfilesService` and a `FeatureProfiles` recipe step. A profile lists `FeatureRules` (`Include`/`Exclude` + expression). Assign to a tenant via `shellSettings["FeatureProfile"]` (comma-separated for multiple). Applied when the shell builds its feature set.

## Tenants module surface

- Admin: `AdminController` route `Tenants/{action}/{id?}` — Index/Create/Edit/Enable/Disable/Delete/Setup.
- API: `TenantApiController` route `api/tenants` — `create`, `edit`, `setup`, `enable/{name}`, `disable/{name}`, remove.
- URL routing: `RequestUrlPrefix` (path segment) and/or `RequestUrlHost` (host header, comma/space separated for multiple).

## Data isolation matrix

| Want | Set |
|------|-----|
| Fully separate DBs | unique `ConnectionString` per tenant |
| One DB, separate tables | shared DB + unique `TablePrefix` |
| One DB, separate schema | `Schema` per tenant (SQL Server) |

Enforce via `OrchardCore_Tenants`: `RequireTablePrefix`, `TablePrefixPattern` (e.g. `{{ ShellSettings.Name }}`), `SchemaPattern`.
