# Admin Navigation Reference

## Common Admin URLs

| Path | Description |
|------|-------------|
| `/Admin` | Admin dashboard |
| `/Admin/Features` | Enable/disable modules |
| `/Admin/Contents/ContentItems` | List all content items |
| `/Admin/Contents/ContentTypes` | Manage content types |
| `/Admin/Media` | Media library |
| `/Admin/Settings` | Site settings |
| `/Admin/Users/Index` | User management |
| `/Admin/Roles/Index` | Role management |
| `/Admin/Tenants` | Multi-tenant management |
| `/Admin/Recipes` | Import/export recipes |
| `/Admin/DeploymentPlan/Index` | Deployment plans |

## Admin Menu Structure

The admin sidebar follows this hierarchy:

```
Dashboard
Content
├── Content Items
├── Content Types
├── Content Definition
└── Taxonomies
Media
├── Media Library
├── Media Options
└── Media Profiles
Design
├── Themes
├── Zones
└── Templates
Security
├── Users
├── Roles
└── Settings
Configuration
├── Features
├── Settings
├── Recipes
└── Tenants
```

## Common UI Element Patterns

### Login Form
- Username field: `input[name="UserName"]` or `#UserName`
- Password field: `input[name="Password"]` or `#Password`
- Login button: `button[type="submit"]`

### Feature Toggle (Features Page)
- Enable button: Look for button with text "Enable" near feature name
- Disable button: Look for button with text "Disable" near feature name
- Search box: `input[placeholder*="Search"]`

### Content Items List
- New button: Button containing "New"
- Edit links: Links within the content item rows
- Delete buttons: Buttons with delete/trash icons
- Pagination: Navigation at bottom of list

### Modal Dialogs
- Confirm buttons typically have class `btn-primary` or text "OK"/"Yes"
- Cancel buttons typically have class `btn-secondary` or text "Cancel"/"No"

## Admin Authentication

Default test credentials (from setup):
- **Username**: `admin`
- **Email**: `admin@test.com`  
- **Password**: `Password1!`

Login URL: `/Login` or click "Log In" from any page

## Workflow: Login to Admin

First, get the port:
```powershell
$port = Get-Content ".orchardcore-port"
```

Then login:
```bash
# Navigate to login
playwright-cli open http://localhost:$port/Login
playwright-cli snapshot

# Fill credentials
playwright-cli fill [username-ref] "admin"
playwright-cli fill [password-ref] "Password1!"
playwright-cli click [submit-ref]
playwright-cli snapshot

# Navigate to admin
playwright-cli open http://localhost:$port/Admin
playwright-cli snapshot
```
