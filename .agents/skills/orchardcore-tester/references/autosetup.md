# AutoSetup (Unattended Setup) Reference

This is the **recommended way to provision a test site for an agent**. AutoSetup
provisions the `Default` tenant on first request using configuration values, so
you never have to drive the browser setup wizard (which has a password-field
limitation — see `setup-wizard.md`).

`OrchardCore.Cms.Web` already wires AutoSetup in: `Program.cs` calls
`.AddSetupFeatures("OrchardCore.AutoSetup")`. You only need to supply the
configuration. The simplest way is environment variables.

## How it works

- The `OrchardCore.AutoSetup` feature (`src/OrchardCore.Modules/OrchardCore.AutoSetup/`)
  reads the configuration section named `OrchardCore_AutoSetup`.
- `AutoSetupMiddleware` triggers setup when the matched tenant is **uninitialized**
  (i.e. a fresh `App_Data`).
- On success you get the log line:
  `The AutoSetup successfully provisioned the site 'TestSite'.`

## Environment-variable recipe (verified working)

Set these before `dotnet run`. The configuration key is built from:

```
OrchardCore__OrchardCore_AutoSetup__Tenants__0__<OptionName>
```

Key rules (these bite people):

- `__` (double underscore) is the **section separator** in .NET configuration.
- The single `_` in `OrchardCore_AutoSetup` is **literal** — it is part of the
  section name, not a separator. So the prefix is
  `OrchardCore__OrchardCore_AutoSetup__`.
- `Tenants__0__` indexes the first tenant in the `Tenants` array.
- **Do NOT wrap values in quotes.** With env vars the quotes become literal
  characters in the value, which silently corrupts the admin password/email and
  makes login fail. Pass values bare.
- `SiteTimeZone` is **required**. Omitting it fails validation with
  `The SiteTimeZone field is required.`

### bash / macOS / Linux

```bash
cd src/OrchardCore.Cms.Web
PORT=$(cat ../../.orchardcore-port)   # or any free port

OrchardCore__OrchardCore_AutoSetup__AutoSetupPath= \
OrchardCore__OrchardCore_AutoSetup__Tenants__0__ShellName=Default \
OrchardCore__OrchardCore_AutoSetup__Tenants__0__SiteName=TestSite \
OrchardCore__OrchardCore_AutoSetup__Tenants__0__SiteTimeZone=America/Los_Angeles \
OrchardCore__OrchardCore_AutoSetup__Tenants__0__AdminUsername=admin \
OrchardCore__OrchardCore_AutoSetup__Tenants__0__AdminEmail=admin@test.com \
OrchardCore__OrchardCore_AutoSetup__Tenants__0__AdminPassword=Password1! \
OrchardCore__OrchardCore_AutoSetup__Tenants__0__DatabaseProvider=Sqlite \
OrchardCore__OrchardCore_AutoSetup__Tenants__0__RecipeName=Blog \
dotnet run -f net10.0 --no-build --urls "http://localhost:$PORT" > autosetup-console.log 2>&1 &
```

The very first HTTP request to `/` triggers AutoSetup, so curl the root once and
then wait for the provisioning log line:

```bash
curl -s -o /dev/null "http://localhost:$PORT/"
# Poll the log until provisioning completes:
grep -m1 "successfully provisioned" autosetup-console.log
```

### PowerShell

```powershell
cd src/OrchardCore.Cms.Web
$port = Get-Content ..\..\.orchardcore-port
$env:OrchardCore__OrchardCore_AutoSetup__AutoSetupPath = ""
$env:OrchardCore__OrchardCore_AutoSetup__Tenants__0__ShellName = "Default"
$env:OrchardCore__OrchardCore_AutoSetup__Tenants__0__SiteName = "TestSite"
$env:OrchardCore__OrchardCore_AutoSetup__Tenants__0__SiteTimeZone = "America/Los_Angeles"
$env:OrchardCore__OrchardCore_AutoSetup__Tenants__0__AdminUsername = "admin"
$env:OrchardCore__OrchardCore_AutoSetup__Tenants__0__AdminEmail = "admin@test.com"
$env:OrchardCore__OrchardCore_AutoSetup__Tenants__0__AdminPassword = "Password1!"
$env:OrchardCore__OrchardCore_AutoSetup__Tenants__0__DatabaseProvider = "Sqlite"
$env:OrchardCore__OrchardCore_AutoSetup__Tenants__0__RecipeName = "Blog"
dotnet run -f net10.0 --no-build --urls "http://localhost:$port"
```

> In PowerShell, assigning `""` to `$env:...` is fine; the value is genuinely
> empty. The "no quotes" rule above is specifically about the bare inline-env
> form used in bash, where `KEY="value"` keeps the quotes in the value.

## Tenant options

All keys go under `...__Tenants__0__`. From `TenantSetupOptions`:

| Option | Required | Notes |
|--------|----------|-------|
| `ShellName` | yes | `Default` for the root tenant. Letters only, no spaces. |
| `SiteName` | yes | Display name, e.g. `TestSite`. |
| `AdminUsername` | yes | e.g. `admin`. |
| `AdminEmail` | yes | e.g. `admin@test.com`. |
| `AdminPassword` | yes | e.g. `Password1!` (must satisfy the password policy). |
| `DatabaseProvider` | yes | `Sqlite` is easiest (no connection string). Others: `SqlConnection`, `Postgres`, `MySql`. |
| `RecipeName` | yes | e.g. `Blog`, `Headless`, `Agency`, `Blank`. |
| `SiteTimeZone` | yes | e.g. `America/Los_Angeles`. **Validation fails if missing.** |
| `DatabaseConnectionString` | only for non-Sqlite providers | |
| `DatabaseTablePrefix` | no | |
| `DatabaseSchema` | no | |
| `RequestUrlHost` / `RequestUrlPrefix` | no | For non-default tenants. |
| `FeatureProfile` | no | |

Top-level option:

| Option | Notes |
|--------|-------|
| `AutoSetupPath` | Empty = trigger on **any** request (recommended for testing). If set, must start with `/`. |

## Confirming success

Success is the log line (in the console output / redirected log file):

```
info: OrchardCore.AutoSetup.Services.AutoSetupService[0]
      The AutoSetup successfully provisioned the site 'TestSite'.
```

A provisioned site also creates `src/OrchardCore.Cms.Web/App_Data/Sites/Default/`.
After provisioning, `/` returns `302` (redirect to the homepage) and `/Login`
returns `200`. You can then log in normally (see SKILL.md "Login to Admin").

## Troubleshooting

| Symptom | Cause / fix |
|---------|-------------|
| `The SiteTimeZone field is required.` | Add `...__Tenants__0__SiteTimeZone=...`. |
| `The single 'Default' tenant should be provided.` | `ShellName` must be `Default` for the root tenant. |
| `The field 'Tenants' should contain at least one tenant.` | The `OrchardCore_AutoSetup` section wasn't bound — check the literal `_` in the prefix and the `Tenants__0__` index. |
| Provisioning succeeds but login fails with the configured password | A value was quoted in bash inline-env form, so the quotes ended up in the password. Pass values bare (no surrounding quotes). |
| Setup never triggers | `App_Data` already has an initialized tenant. Reset it (delete `App_Data`) before starting. |
| `DatabaseProvider` field is required | Use a valid provider value (`Sqlite`, `SqlConnection`, `Postgres`, `MySql`). |

## Resetting

```bash
# stop the app first (see SKILL.md), then:
rm -rf src/OrchardCore.Cms.Web/App_Data
```
