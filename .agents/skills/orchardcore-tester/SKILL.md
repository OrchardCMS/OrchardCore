---
name: orchardcore-tester
description: Tests OrchardCore CMS features through browser automation. Use when the user needs to build, run, setup, or test OrchardCore functionality including admin features, content management, media library, and module testing.
---

# OrchardCore Feature Testing

This skill guides you through testing OrchardCore CMS features using browser automation with `playwright-cli`.

## Prerequisites

- OrchardCore repository (working directory)
- .NET SDK 10.0+ installed
- `playwright-cli` skill available

## Core Workflow

Testing an OrchardCore feature follows these steps:

1. **Build** the application
2. **Run** the application server (background)
3. **Setup** a test site (if needed)
4. **Test** the feature via browser
5. **Verify** results and clean up

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

## Step 3: Setup Test Site

**Check if setup is needed**: Navigate to the app URL - if you see a setup wizard, the site needs setup.

**Reset for fresh setup** (optional):
```powershell
Remove-Item -Recurse -Force src/OrchardCore.Cms.Web/App_Data
```

**Setup workflow**:
```bash
# Get the port
$port = Get-Content ".orchardcore-port"

playwright-cli open http://localhost:$port
playwright-cli snapshot
# Fill: Site Name = "Test Site", Recipe = "Blog", Username = "admin", 
# Email = "admin@test.com", Password = "Password1!"
# Click "Finish Setup"
```

See `references/setup-wizard.md` for detailed field mapping.

## Step 4: Test Features

**Important**: Replace `5000` with the actual port from `.orchardcore-port` file.

### Login to Admin

```bash
playwright-cli open http://localhost:$port/Login
playwright-cli snapshot
# Fill username: admin, password: Password1!
# Click login button
playwright-cli open http://localhost:$port/Admin
playwright-cli snapshot
```

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

- `references/admin-navigation.md` - Admin URLs and UI patterns
- `references/setup-wizard.md` - Site setup details
- `references/common-features.md` - Feature-specific testing guides
- `references/debugging.md` - Log files and troubleshooting
- `AGENTS.md` (repo root) - Build and development instructions
