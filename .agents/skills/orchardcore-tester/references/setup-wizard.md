# Setup Wizard Reference

## Overview

When OrchardCore runs without existing data (no `App_Data` folder), it presents a setup wizard at the root URL.

## Prerequisites

Before setup, ensure:
1. Application is built: `dotnet build` from repo root
2. No existing data: Delete `src/OrchardCore.Cms.Web/App_Data` if present
3. Application is running: `cd src/OrchardCore.Cms.Web && dotnet run -f net10.0`

## Setup Wizard Form Fields

| Field | Description | Recommended Value |
|-------|-------------|-------------------|
| Site Name | Display name for the site | "Test Site" |
| Recipe | Pre-configured feature set | **"Blog"** (recommended) |
| Database | Database provider | "Sqlite" (default, easiest) |
| Table Prefix | Database table prefix | Leave empty |
| Connection String | DB connection (if not Sqlite) | N/A for Sqlite |
| User Name | Admin username | "admin" |
| Email | Admin email | "admin@test.com" |
| Password | Admin password | "Password1!" |
| Password Confirmation | Confirm password | "Password1!" |

## Recipe Options

| Recipe | Description | Use Case |
|--------|-------------|----------|
| **Blog** | Blog with common features enabled | **Recommended for testing** |
| Headless | API-only, no frontend | API testing |
| Agency | Agency website template | Theme testing |
| Coming Soon | Simple landing page | Minimal testing |
| SaaS | Multi-tenant SaaS setup | Tenant testing |
| Blank | No features enabled | Custom setup |

**Why "Blog"?** It pre-enables: Media, Content Management, Admin UI, Markdown, Autoroute, and other commonly tested features.

## Setup Workflow with playwright-cli

First, get the port from the session file:
```powershell
$port = Get-Content ".orchardcore-port"
```

Then proceed with setup:
```bash
# 1. Navigate to root (shows setup wizard)
playwright-cli open http://localhost:$port
playwright-cli snapshot

# 2. Fill site name
playwright-cli fill [sitename-ref] "Test Site"

# 3. Select recipe - find and click "Blog" option
# Look for select/dropdown with recipes, select "Blog"
playwright-cli select [recipe-ref] "Blog"

# 4. Keep Sqlite default (usually pre-selected)

# 5. Fill admin credentials
playwright-cli fill [username-ref] "admin"
playwright-cli fill [email-ref] "admin@test.com"
playwright-cli fill [password-ref] "Password1!"
playwright-cli fill [password-confirm-ref] "Password1!"

# 6. Submit setup
playwright-cli click [finish-setup-ref]

# 7. Wait for setup to complete and verify
playwright-cli snapshot
```

## Post-Setup Verification

After setup completes:
1. You should be redirected to the site homepage or admin
2. Navigate to `/Admin` to verify admin access
3. Check `/Admin/Features` to see enabled features

## Resetting for Fresh Setup

To start over with a clean installation:

```powershell
# Stop the running application
if (Test-Path ".orchardcore-pid") {
    Stop-Process -Id (Get-Content ".orchardcore-pid") -Force -ErrorAction SilentlyContinue
    Remove-Item ".orchardcore-pid" -Force
}

# Delete App_Data
Remove-Item -Recurse -Force src/OrchardCore.Cms.Web/App_Data

# Restart application (see SKILL.md Step 2)
```

## Common Setup Issues

| Issue | Solution |
|-------|----------|
| Setup page doesn't appear | Check if App_Data exists; delete it |
| "Port already in use" | Stop other instances or use different port |
| Recipe dropdown empty | Check browser console for JS errors |
| Setup fails silently | Check terminal/console for .NET errors |
