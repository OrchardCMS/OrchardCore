# Setup Wizard Reference (Interactive Browser Setup)

## Overview

When OrchardCore runs without existing data (no initialized `App_Data`), it
presents a setup wizard at the root URL. This page covers driving that wizard
with `playwright-cli`.

> **Prefer AutoSetup.** For unattended provisioning, use the AutoSetup
> environment-variable path in `references/autosetup.md` — it avoids the wizard's
> password-field limitation entirely. Use the interactive wizard only when you
> specifically need to exercise the wizard UI.

## Prerequisites

1. Application is built: `dotnet build src/OrchardCore.Cms.Web -c Debug -f net10.0`
2. No existing data: delete `src/OrchardCore.Cms.Web/App_Data` if present
3. Application is running **without** AutoSetup env vars (so the wizard appears)
4. Browser installed for `playwright-cli` (`--browser webkit` on macOS) — see
   `references/playwright-cli.md`

## Setup Wizard Form Fields

| Field | Description | Recommended Value |
|-------|-------------|-------------------|
| Site Name | Display name for the site | "Test Site" |
| Recipe | Pre-configured feature set (dropdown) | **"Blog"** |
| Default Time Zone | Site time zone (`<select>`, pre-filled) | leave default |
| Database | Database provider (`<select>`) | "Sqlite" (default) |
| Table Prefix | Database table prefix | leave empty |
| Connection String | DB connection (non-Sqlite only) | N/A for Sqlite |
| User Name | Admin username | "admin" |
| Email | Admin email | "admin@test.com" |
| Password | Admin password | "Password1!" |
| Password Confirmation | Confirm password | "Password1!" |

## Recipe Options

| Recipe | Use Case |
|--------|----------|
| **Blog** | **Recommended for testing** (Media, content management, admin UI, Markdown, Autoroute, etc.) |
| Headless | API-only testing |
| Agency | Theme testing |
| Coming Soon | Minimal testing |
| SaaS | Tenant testing |
| Blank | No features enabled |

The recipe control is a **dropdown button** backed by a hidden `RecipeName`
input. Selecting an item runs JS that sets `RecipeName`.

## Setup Workflow with playwright-cli (verified)

Replace `$PORT` with the actual port.

```bash
PORT=$(cat .orchardcore-port)

# 1. Open the wizard and capture refs
playwright-cli --browser webkit open "http://localhost:$PORT/"
playwright-cli snapshot      # note refs for the fields below

# 2. Plain text inputs: fill works normally
playwright-cli fill <sitename-ref> "Test Site"
playwright-cli fill <username-ref> "admin"
playwright-cli fill <email-ref>    "admin@test.com"

# 3. Recipe: open the dropdown, then click the "Blog" item
playwright-cli click <recipe-button-ref>
playwright-cli snapshot      # find the "Blog" link ref
playwright-cli click <blog-link-ref>
playwright-cli eval 'document.getElementById("RecipeName").value'   # -> "Blog"

# 4. Database (Sqlite) and Time Zone already have valid defaults — leave them.

# 5. Password fields: fill DOES NOT WORK here (see playwright-cli.md).
#    Use the native-setter eval for BOTH password and confirmation.
playwright-cli eval '((p)=>{var s=Object.getOwnPropertyDescriptor(window.HTMLInputElement.prototype,"value").set; s.call(p,"Password1!"); p.dispatchEvent(new Event("input",{bubbles:true})); p.dispatchEvent(new Event("change",{bubbles:true})); return p.value.length})(document.getElementById("Password"))'
playwright-cli eval '((p)=>{var s=Object.getOwnPropertyDescriptor(window.HTMLInputElement.prototype,"value").set; s.call(p,"Password1!"); p.dispatchEvent(new Event("input",{bubbles:true})); p.dispatchEvent(new Event("change",{bubbles:true})); return p.value.length})(document.getElementById("PasswordConfirmation"))'

# verify both
playwright-cli eval 'document.getElementById("Password").value+"|"+document.getElementById("PasswordConfirmation").value'

# 6. Submit
playwright-cli click <finish-setup-ref>

# 7. Verify: page title becomes "Blog - Test Site" (or similar)
playwright-cli eval 'window.location.pathname+" | "+document.title'
```

> The `eval` calls print a benign `result is not a function` message because the
> arrow-IIFE returns a number — the value is still set. Always confirm with the
> follow-up read in step 5.

## Why the password workaround is needed

The password inputs are standard `<input type="password">` elements inside a
Bootstrap `.input-group` with a show/hide toggle. With `playwright-cli` +
webkit, `fill`/`type`/`.value` are silent no-ops on these inputs (they work on
the plain Site Name / User Name / Email inputs). The native-setter `eval` is the
reliable way to populate them. Full details and selectors:
`references/playwright-cli.md`.

## Post-Setup Verification

1. After submit you are redirected to the site homepage; the title reflects the
   recipe and site name (e.g. "Blog - Test Site").
2. `src/OrchardCore.Cms.Web/App_Data/Sites/Default/` now exists.
3. Log in at `/Login` (the login password field needs the same native-setter
   workaround) and confirm `/Admin` loads without redirecting to `/Login`.

## Resetting for Fresh Setup

```bash
# Stop the running app (see SKILL.md), then:
rm -rf src/OrchardCore.Cms.Web/App_Data
# Restart the app (without AutoSetup env vars to get the wizard).
```

## Common Setup Issues

| Issue | Solution |
|-------|----------|
| Setup page doesn't appear | `App_Data` already initialized — delete it. |
| Wizard appears but you wanted unattended | You set AutoSetup env vars but `App_Data` already had a tenant; reset `App_Data`. |
| Password field stays empty | Use the native-setter `eval`, not `fill`/`type` (see above). |
| "Port already in use" | Stop other instances or use a different port. |
| Recipe dropdown does nothing | Click the dropdown button first, then the item link; verify the hidden `RecipeName`. |
| Submit appears to do nothing | Confirm both password fields are set and meet the policy; check the browser console (`playwright-cli console error`). |
