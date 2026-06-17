---
name: orchardcore-tester
description: Tests OrchardCore CMS features through browser automation. Use when the user needs to build, run, setup, or test OrchardCore functionality including admin features, content management, media library, and module testing.
---

# OrchardCore Feature Testing

This skill guides you through testing OrchardCore CMS features using browser automation with `playwright-cli`.

## Prerequisites

- OrchardCore repository (working directory)
- .NET SDK 10.0+ installed
- `playwright-cli` skill available, with a browser engine installed.
  On macOS (or any machine without Chrome) use webkit:
  ```bash
  playwright-cli --browser webkit install
  ```
  Then pass `--browser webkit` on the first `open` of a session. See
  `references/playwright-cli.md`.

> The examples below show PowerShell and bash. The repo targets macOS/.NET 10;
> bash works everywhere. Use whichever matches your shell.

## Core Workflow

Testing an OrchardCore feature follows these steps:

1. **Build** the application
2. **Run** the application server (background)
3. **Setup** a test site (AutoSetup is the recommended unattended path)
4. **Test** the feature via browser
5. **Verify** results and clean up

## TL;DR (fastest reliable path)

```bash
# 1. build
dotnet build src/OrchardCore.Cms.Web -c Debug -f net10.0

# 2. fresh state + pick a port
rm -rf src/OrchardCore.Cms.Web/App_Data
PORT=$(( (RANDOM % 1000) + 5000 )); echo -n $PORT > .orchardcore-port

# 3. run with AutoSetup (provisions Default tenant on first request, no wizard)
cd src/OrchardCore.Cms.Web
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
cd ../..

# 4. trigger + confirm
curl -s -o /dev/null "http://localhost:$PORT/"
grep -m1 "successfully provisioned" src/OrchardCore.Cms.Web/autosetup-console.log
```

Then log in (see Step 4 — the login password field needs the native-setter
workaround). Full AutoSetup details and gotchas: `references/autosetup.md`.

## Step 1: Build

```powershell
dotnet build src/OrchardCore.Cms.Web/OrchardCore.Cms.Web.csproj -c Debug -f net10.0
```

## Step 2: Run Application (Background)

Since multiple agents may run OrchardCore from different worktrees simultaneously, use a random port and run in background.

### Get or Create Session Port

```powershell
# Check for existing port file, or generate random port (5000-5999)
$portFile = ".orchardcore-port"
if (Test-Path $portFile) {
    $port = Get-Content $portFile
} else {
    $port = Get-Random -Minimum 5000 -Maximum 6000
    $port | Out-File $portFile -NoNewline
}
Write-Host "Using port: $port"
```

### Start Application in Background

```powershell
# Start OrchardCore in background process
$proc = Start-Process dotnet `
    -ArgumentList "run","-f","net10.0","--no-build","--urls","http://localhost:$port" `
    -WorkingDirectory "src/OrchardCore.Cms.Web" `
    -PassThru -NoNewWindow

# Save PID for later cleanup
$proc.Id | Out-File ".orchardcore-pid" -NoNewline
Write-Host "Started OrchardCore (PID: $($proc.Id)) on http://localhost:$port"
```

### Wait for Application Ready

```powershell
# Poll until app responds (max 60 seconds)
$port = Get-Content ".orchardcore-port"
$timeout = 60; $elapsed = 0
while ($elapsed -lt $timeout) {
    try {
        $response = Invoke-WebRequest -Uri "http://localhost:$port" -UseBasicParsing -TimeoutSec 2
        Write-Host "Application ready at http://localhost:$port"
        break
    } catch {
        Start-Sleep -Seconds 2
        $elapsed += 2
    }
}
```

### Stop Application

```powershell
# Stop the background process
if (Test-Path ".orchardcore-pid") {
    $pid = Get-Content ".orchardcore-pid"
    Stop-Process -Id $pid -Force -ErrorAction SilentlyContinue
    Remove-Item ".orchardcore-pid" -Force
    Write-Host "Stopped OrchardCore (PID: $pid)"
}
```

```bash
# bash: find the process actually listening on the port and kill it.
# (Backgrounding dotnet via a subshell makes $! unreliable, so resolve by port.)
PORT=$(cat .orchardcore-port)
PID=$(lsof -ti tcp:$PORT -sTCP:LISTEN | head -1)
[ -n "$PID" ] && kill "$PID" && echo "Stopped OrchardCore (PID: $PID)"
rm -f .orchardcore-pid
```

## Step 3: Setup Test Site

A fresh `App_Data` is uninitialized, so the site must be provisioned. There are
two paths.

### Option A — AutoSetup (recommended, unattended)

Provision the `Default` tenant from configuration on first request — **no
browser, no wizard**. `OrchardCore.Cms.Web` already wires AutoSetup in
(`Program.cs` → `.AddSetupFeatures("OrchardCore.AutoSetup")`); you just supply
env vars when starting the app (see the TL;DR above, or run them inline with
`dotnet run` from Step 2).

Key rules (full details in `references/autosetup.md`):

- Prefix is `OrchardCore__OrchardCore_AutoSetup__Tenants__0__<Option>` — `__` is
  the separator, the single `_` in `OrchardCore_AutoSetup` is **literal**.
- **No quotes around values** in bash inline-env form (quotes become part of the
  value and corrupt the admin password/email).
- `SiteTimeZone` is **required** (e.g. `America/Los_Angeles`).

Confirm success with the log line:

```
The AutoSetup successfully provisioned the site 'TestSite'.
```

```bash
grep -m1 "successfully provisioned" src/OrchardCore.Cms.Web/autosetup-console.log
```

### Option B — Interactive setup wizard (browser)

Start the app **without** AutoSetup env vars, then drive the wizard with
`playwright-cli`. The plain inputs (Site Name, User Name, Email) fill normally,
but the **password and confirmation fields cannot be filled with `fill`/`type`**
— use the native-setter `eval` workaround. Full step-by-step:
`references/setup-wizard.md`. Quick shape:

```bash
PORT=$(cat .orchardcore-port)
playwright-cli --browser webkit open "http://localhost:$PORT/"
playwright-cli snapshot
# fill <sitename>, <username>, <email>; pick Blog from the recipe dropdown
# set passwords via native setter (see below / setup-wizard.md), then click Finish Setup
```

**Reset for fresh setup**:
```bash
rm -rf src/OrchardCore.Cms.Web/App_Data
```
```powershell
Remove-Item -Recurse -Force src/OrchardCore.Cms.Web/App_Data
```

## Step 4: Test Features

**Important**: Replace `5000` with the actual port from `.orchardcore-port` file.

### Login to Admin

The username fills normally, but the **login password field
(`input[name="LoginForm.Password"]`) cannot be filled with `fill`/`type`** — use
the native-setter `eval` (same limitation as the setup wizard; see
`references/playwright-cli.md`).

```bash
PORT=$(cat .orchardcore-port)
playwright-cli --browser webkit open "http://localhost:$PORT/Login"
playwright-cli snapshot                       # get the username/login-button refs

# username: plain fill works
playwright-cli fill <username-ref> "admin"

# password: fill is a no-op here — set it via the native setter + events
playwright-cli eval '((p)=>{var s=Object.getOwnPropertyDescriptor(window.HTMLInputElement.prototype,"value").set; s.call(p,"Password1!"); p.dispatchEvent(new Event("input",{bubbles:true})); p.dispatchEvent(new Event("change",{bubbles:true})); return p.value.length})(document.querySelector("input[name=\"LoginForm.Password\"]"))'

# submit, then confirm /Admin loads (does NOT redirect back to /Login)
playwright-cli click <login-button-ref>
playwright-cli open "http://localhost:$PORT/Admin"
playwright-cli eval 'window.location.pathname'   # -> "/Admin" when authenticated
```

> The `eval` prints a benign `result is not a function` message (it returns a
> number); the value is still set — verify with
> `playwright-cli eval 'document.querySelector("input[name=\"LoginForm.Password\"]").value'`.

### Common Test Scenarios

**Test Media Library**:
```bash
playwright-cli open http://localhost:$port/Admin/Media
playwright-cli snapshot
```

**Test Content Creation**:
```bash
playwright-cli open http://localhost:$port/Admin/Contents/ContentItems
playwright-cli snapshot
# Click New, select content type, fill fields, publish
```

**Enable a Feature**:
```bash
playwright-cli open http://localhost:$port/Admin/Features
playwright-cli snapshot
# Search for feature, click Enable
```

See `references/common-features.md` for detailed workflows.

## Step 5: Verify Results

After each action:
```bash
# Check page state
playwright-cli snapshot

# Check for JavaScript errors
playwright-cli console error

# Verify page title
playwright-cli eval "document.title"
```

### Debugging with Log Files

Console output is not visible when running in background. Use log files instead:

```powershell
# View last 50 lines of today's log
Get-Content "src/OrchardCore.Cms.Web/App_Data/logs/orchard-log-$(Get-Date -Format 'yyyy-MM-dd').log" -Tail 50

# Search for errors
Select-String -Path "src/OrchardCore.Cms.Web/App_Data/logs/orchard-log-$(Get-Date -Format 'yyyy-MM-dd').log" -Pattern "ERROR|Exception" -Context 2,5
```

See `references/debugging.md` for more debugging techniques.

## Quick Reference

| Task | URL Path |
|------|----------|
| Admin Dashboard | `/Admin` |
| Features | `/Admin/Features` |
| Content Items | `/Admin/Contents/ContentItems` |
| Media Library | `/Admin/Media` |
| Users | `/Admin/Users/Index` |
| Themes | `/Admin/Themes` |

## Session Files

| File | Purpose |
|------|---------|
| `.orchardcore-port` | Persisted port number for session |
| `.orchardcore-pid` | Process ID for cleanup |

## Default Credentials

- **Username**: admin
- **Email**: admin@test.com
- **Password**: Password1!

## References

- `references/autosetup.md` - **Unattended AutoSetup** (env-var recipe, gotchas, troubleshooting)
- `references/playwright-cli.md` - playwright-cli specifics: browser install, `eval` constraint, the password-field workaround
- `references/setup-wizard.md` - Interactive browser setup wizard (with password workaround)
- `references/admin-navigation.md` - Admin URLs and UI patterns
- `references/common-features.md` - Feature-specific testing guides
- `references/debugging.md` - Log files and troubleshooting
- `AGENTS.md` (repo root) - Build and development instructions
