# Common Features Testing Reference

## Media Library

**URL**: `/Admin/Media`

### Testing Media Upload

```bash
# Navigate to Media
playwright-cli open http://localhost:5000/Admin/Media
playwright-cli snapshot

# Upload a file (find upload button/area)
playwright-cli upload ./test-image.jpg

# Verify upload appears in library
playwright-cli snapshot
```

### Media Library UI Elements
- Upload button/dropzone: Look for "Upload" text or drag-drop area
- Folder tree: Left sidebar navigation
- Media grid: Main content area with thumbnails
- Search: Input field for filtering media

---

## Content Items

**URL**: `/Admin/Contents/ContentItems`

### Creating Content

```bash
# Navigate to content items
playwright-cli open http://localhost:5000/Admin/Contents/ContentItems
playwright-cli snapshot

# Click "New" and select content type
playwright-cli click [new-button-ref]
playwright-cli snapshot

# Select content type (e.g., "Blog Post")
playwright-cli click [blogpost-ref]
playwright-cli snapshot

# Fill title and content
playwright-cli fill [title-ref] "Test Blog Post"
playwright-cli fill [markdown-ref] "This is test content."

# Publish
playwright-cli click [publish-ref]
playwright-cli snapshot
```

### Content Item UI Elements
- New button: Creates new content
- Content type selector: Modal/dropdown after clicking New
- Title field: Usually first prominent text input
- Editor: Markdown editor, HTML editor, or WYSIWYG
- Save Draft / Publish buttons: Action buttons at bottom or top

---

## Features Management

**URL**: `/Admin/Features`

### Enabling a Feature

```bash
# Navigate to Features
playwright-cli open http://localhost:5000/Admin/Features
playwright-cli snapshot

# Search for feature
playwright-cli fill [search-ref] "Media"
playwright-cli snapshot

# Click Enable button next to feature
playwright-cli click [enable-button-ref]
playwright-cli snapshot

# Verify feature is now enabled (button changes to Disable)
```

### Features UI Elements
- Search box: Filter features by name
- Category tabs/filters: Group features by category
- Enable/Disable buttons: Toggle feature state
- Feature cards: Show name, description, dependencies

---

## User Management

**URL**: `/Admin/Users/Index`

### Creating a User

```bash
# Navigate to Users
playwright-cli open http://localhost:5000/Admin/Users/Index
playwright-cli snapshot

# Click Add User
playwright-cli click [add-user-ref]
playwright-cli snapshot

# Fill user details
playwright-cli fill [username-ref] "testuser"
playwright-cli fill [email-ref] "test@test.com"
playwright-cli fill [password-ref] "Password1!"
playwright-cli fill [confirm-password-ref] "Password1!"

# Save
playwright-cli click [save-ref]
playwright-cli snapshot
```

---

## Content Types

**URL**: `/Admin/ContentTypes`

### Creating a Content Type

```bash
# Navigate to Content Types
playwright-cli open http://localhost:5000/Admin/ContentTypes
playwright-cli snapshot

# Click Create new type
playwright-cli click [create-type-ref]
playwright-cli snapshot

# Fill type details
playwright-cli fill [displayname-ref] "Product"
playwright-cli fill [technicalname-ref] "Product"

# Add parts (e.g., TitlePart)
playwright-cli click [add-part-ref]
playwright-cli snapshot
```

---

## Workflows (if enabled)

**URL**: `/Admin/Workflows`

### Workflow UI Elements
- Workflow list: Shows available workflows
- Create button: New workflow
- Workflow designer: Visual editor for workflow steps
- Activity palette: Drag-and-drop activities

---

## Themes

**URL**: `/Admin/Themes`

### Switching Themes

```bash
# Navigate to Themes
playwright-cli open http://localhost:5000/Admin/Themes
playwright-cli snapshot

# Find theme and click "Make Current"
playwright-cli click [make-current-ref]
playwright-cli snapshot
```

---

## Verification Patterns

### Check for Success Messages
After actions, look for:
- Toast notifications (usually top-right)
- Alert boxes with success styling (green)
- Page redirect to expected location

### Check for Errors
Look for:
- Red/danger styled alerts
- Validation messages near form fields
- Console errors: `playwright-cli console error`

### Verify Page State
```bash
# Take snapshot to see current state
playwright-cli snapshot

# Check console for JavaScript errors
playwright-cli console error

# Evaluate page title
playwright-cli eval "document.title"
```
