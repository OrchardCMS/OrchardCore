# Theme Assets Reference

## Asset Pipeline

OrchardCore uses Vite for asset compilation. Assets are defined in `Assets.json` and built with `yarn build`.

## Assets.json Configuration

```json
[
  {
    "action": "vite",
    "name": "your-theme",
    "source": "Assets/",
    "tags": ["js", "css"]
  }
]
```

### Configuration Options

| Property | Description |
|----------|-------------|
| `action` | Build action (`vite`, `gulp`, `copy`) |
| `name` | Unique name for the build |
| `source` | Source directory for assets |
| `tags` | Asset types to process |

## Package.json

```json
{
  "name": "@orchardcore/your-theme",
  "version": "1.0.0",
  "private": true,
  "dependencies": {
    "bootstrap": "5.3.8"
  },
  "devDependencies": {}
}
```

## SCSS Structure

### Assets/scss/site.scss

```scss
// Import Bootstrap (optional)
@import "bootstrap/scss/bootstrap";

// Custom variables
$primary-color: #007bff;
$font-family-base: 'Segoe UI', sans-serif;

// Base styles
body {
    font-family: $font-family-base;
    line-height: 1.6;
}

// Layout
.container {
    max-width: 1200px;
    margin: 0 auto;
    padding: 0 1rem;
}

// Header
.site-header {
    background: $primary-color;
    color: white;
    padding: 1rem 0;
}

// Navigation
.main-nav {
    ul {
        list-style: none;
        padding: 0;
        margin: 0;
        display: flex;
        gap: 1rem;
    }
    
    a {
        color: inherit;
        text-decoration: none;
        
        &:hover {
            text-decoration: underline;
        }
    }
}

// Content
.content-area {
    padding: 2rem 0;
}

// Footer
.site-footer {
    background: #333;
    color: #fff;
    padding: 2rem 0;
    margin-top: 2rem;
}
```

## JavaScript

### Assets/js/site.js

```javascript
// Wait for DOM ready
document.addEventListener('DOMContentLoaded', function() {
    initializeTheme();
});

function initializeTheme() {
    // Mobile menu toggle
    const menuToggle = document.querySelector('.menu-toggle');
    const mainNav = document.querySelector('.main-nav');
    
    if (menuToggle && mainNav) {
        menuToggle.addEventListener('click', function() {
            mainNav.classList.toggle('is-open');
        });
    }
    
    // Smooth scroll for anchor links
    document.querySelectorAll('a[href^="#"]').forEach(anchor => {
        anchor.addEventListener('click', function(e) {
            e.preventDefault();
            const target = document.querySelector(this.getAttribute('href'));
            if (target) {
                target.scrollIntoView({ behavior: 'smooth' });
            }
        });
    });
}
```

## Building Assets

```bash
# From repository root
cd D:\orchardcore

# Install dependencies (first time)
corepack enable
yarn

# Build all assets
yarn build

# Build specific theme (if supported)
yarn build --filter=your-theme
```

## Output Structure

After building, assets are output to `wwwroot/`:

```
wwwroot/
├── css/
│   └── site.css          # Compiled CSS
├── js/
│   └── site.js           # Compiled JavaScript
└── images/               # Copied images
```

## Using Resources in Views

### Link CSS

```html
<!-- Direct link -->
<link asp-src="~/YourTheme/css/site.css" />

<!-- With resource manager -->
<style asp-src="~/YourTheme/css/site.css"></style>
```

### Include JavaScript

```html
<!-- In footer -->
<script asp-src="~/YourTheme/js/site.js"></script>

<!-- With resource manager -->
<script asp-src="~/YourTheme/js/site.js" at="Foot"></script>
```

### Using Resource Manager

```razor
@{
    // Register resources programmatically
    Orchard.RegisterStyle("~/YourTheme/css/site.css");
    Orchard.RegisterScript("~/YourTheme/js/site.js", "Foot");
}
```

## Third-Party Libraries

### Bootstrap Integration

```scss
// In site.scss
// Customize Bootstrap variables before import
$primary: #0d6efd;
$secondary: #6c757d;
$body-bg: #fff;
$body-color: #212529;

// Import all of Bootstrap
@import "bootstrap/scss/bootstrap";

// Or import specific modules
@import "bootstrap/scss/functions";
@import "bootstrap/scss/variables";
@import "bootstrap/scss/mixins";
@import "bootstrap/scss/grid";
@import "bootstrap/scss/utilities";
```

### Font Awesome

```json
// In package.json
{
  "dependencies": {
    "@fortawesome/fontawesome-free": "6.5.1"
  }
}
```

```scss
// In site.scss
@import "@fortawesome/fontawesome-free/scss/fontawesome";
@import "@fortawesome/fontawesome-free/scss/solid";
@import "@fortawesome/fontawesome-free/scss/regular";
```
