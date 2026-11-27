"""Takes the screenshots of the documentation from a running Orchard Core site.

Each entry of SHOTS names an image of the docs, most under src/docs/user-manual/, and the admin page it shows. Run the
script again whenever a screen changes, and commit the images that change.

The site must be set up with the Blog recipe, with an `admin` account and the features the pages describe turned on,
and with "Display titles in top bar" checked and the "Menu" actions layout in Settings > Admin (see "Screenshots" in
src/docs/contributing/contributing-documentation.md). A guide that sets up a site of its own is taken in a tenant of
TENANTS, or on another site of SITES with --site; the comment above its shots says what the site must hold.

Usage:
    pip install playwright pillow
    python .scripts/docs/screenshots.py --url http://localhost:5000 --password <admin password> [--site <site>] [--only <name> ...]

The script uses the Microsoft Edge or Google Chrome installed on the machine. Without either, run
`python -m playwright install chromium` and pass `--browser chromium`.
"""

import argparse
import io
import os
import sys
from urllib.parse import urljoin

from PIL import Image
from playwright.sync_api import sync_playwright

ROOT = os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", "..", "src", "docs", "user-manual")
VIEWPORT = {"width": 1280, "height": 800}
QUALITY = 82
# The admin theme's default scheme, picked for every page whatever the site's default scheme is.
SCHEME = "gray"

# The sites the shots are taken from, each with its own App_Data, picked with --site.
SITES = {
    "blog": "the Blog recipe site described above",
    "blazor": "a site set up with the Headless site recipe, named BlazorCMS, as in the 'Create a Blazor CMS' guide",
}

# The tenants of a site that shots are taken in, with their URL prefix. Each is set up with the `admin` account, and
# with "Display titles in top bar" and the "Menu" actions layout in Settings > Admin.
TENANTS = {
    # Blog site: the 'Microsoft Entra ID integration' guide, set up with the Blog recipe.
    "EntraId": "entra-id",
    # Blog site: the 'Decoupled CMS' guide, set up with the Blank site recipe.
    "Decoupled": "decoupled",
    # Blog site: the admin theme page, set up with the Blog recipe, with "Horizontal form layout" checked on the Gray scheme.
    "Horizontal": "horizontal",
    # Blazor site: the tenant the 'Create a Blazor CMS' guide creates, set up with the Headless site recipe.
    "FirstTenant": "tenant01",
}

# The results of the search page of TheBlogTheme, under its banner.
SEARCH_RESULTS = [("scroll", "input[name='Terms']", "start")]

# Each shot:
#   file:      the image, relative to src/docs/user-manual/ (an image of the Technical Manual starts with ../).
#   path:      the address to open, relative to the site, or to the tenant.
#   steps:     optional actions after opening the page: ("follow", selector) opens the address of the first matching
#              link, even a hidden one; ("click", selector); ("fill", selector, value); ("type", selector, value) types
#              the value key by key; ("editor", selector, text) sets the text of a code editor (CodeMirror);
#              ("select", selector, value);
#              ("check", selector); ("uncheck", selector); ("scroll", selector[, "start" or "end"]) scrolls the first
#              matching element to the middle, the top or the bottom of its container; ("wait", milliseconds). Steps never save a form.
#   anonymous: true to take the shot signed out.
#   full_page: true to capture the whole page instead of the window (cut at 1600 pixels).
#   site:      the site of SITES the shot is taken from, "blog" when omitted.
#   tenant:    the tenant of TENANTS the shot is taken in, the site itself when omitted.
# A guide that goes through several steps shows the site at each step: its shots are listed in the order of the guide,
# and the comment above a group says what the site must hold, so a group can be taken again with --only.
SHOTS = [
    # Getting started
    {"file": "getting-started/images/dashboard.webp", "path": "/Admin"},
    {"file": "getting-started/images/content-list.webp", "path": "/Admin/Contents/ContentItems"},
    {"file": "getting-started/images/features.webp", "path": "/Admin/Features"},
    {"file": "getting-started/images/role-editor.webp", "path": "/Admin/Roles/Edit/Editor"},
    {"file": "getting-started/images/login.webp", "path": "/Login", "anonymous": True},
    {"file": "getting-started/images/edit-profile.webp", "path": "/Admin/Users/Edit"},
    {"file": "getting-started/images/change-password.webp", "path": "/Admin", "steps": [("follow", "a:has-text('Change password')")]},
    # Needs a tenant that isn't set up yet: its Setup link carries the setup token.
    {"file": "getting-started/images/setup.webp", "path": "/Admin/Tenants", "steps": [("follow", "a:has-text('Setup')")]},
    # Write and publish
    {"file": "content/images/content-items.webp", "path": "/Admin/Contents/ContentItems"},
    {"file": "content/images/create-article.webp", "path": "/Admin/Contents/ContentTypes/Article/Create"},
    {"file": "content/images/blog-post-editor.webp", "path": "/Admin/Contents/ContentTypes/BlogPost/Create", "full_page": True},
    {"file": "content/images/page-editor.webp", "path": "/Admin/Contents/ContentTypes/Page/Create"},
    {"file": "content/images/editor-buttons.webp", "path": "/Admin/Contents/ContentTypes/Article/Create", "full_page": True},
    {"file": "content/images/content-types.webp", "path": "/Admin/ContentTypes/List"},
    {"file": "content/images/edit-content-type.webp", "path": "/Admin/ContentTypes/Edit/BlogPost"},
    {"file": "content/images/add-parts.webp", "path": "/Admin/ContentTypes/AddPartsTo/Article"},
    {"file": "content/images/add-field.webp", "path": "/Admin/OrchardCore.ContentTypes/Admin/AddFieldTo/Article"},
    {"file": "content/images/blog-items.webp", "path": "/Admin/Contents/ContentItems/?Options.SelectedContentType=Blog",
     "steps": [("follow", "a:has-text('BlogPost Items')")]},
    {"file": "content/images/taxonomy.webp", "path": "/Admin/Contents/ContentItems/Taxonomy",
     "steps": [("follow", "a[href*='/Edit']:has-text('Edit')")]},
    {"file": "content/images/cultures.webp", "path": "/Admin/Settings/localization"},
    {"file": "content/images/audit-trail.webp", "path": "/Admin/AuditTrail"},
    {"file": "content/images/audit-trail-settings.webp", "path": "/Admin/Settings/AuditTrail"},
    {"file": "content/images/social-meta-settings.webp", "path": "/Admin/Settings/SocialMetaSettings"},
    {"file": "content/images/robots.webp", "path": "/Admin/Settings/robotsSettings"},
    # Media
    {"file": "media/images/media-library.webp", "path": "/Admin/Media", "steps": [("wait", 1500)]},
    {"file": "media/images/media-profiles.webp", "path": "/Admin/MediaProfiles"},
    {"file": "media/images/media-options.webp", "path": "/Admin/Media/Options"},
    {"file": "media/images/media-cache.webp", "path": "/Admin/MediaCache"},
    # Search and discovery
    {"file": "search/images/queries.webp", "path": "/Admin/Queries/Index"},
    {"file": "search/images/run-sql-query.webp", "path": "/Admin/Queries/Sql/Query"},
    {"file": "search/images/run-lucene-query.webp", "path": "/Admin/Lucene/query"},
    {"file": "search/images/indexes.webp", "path": "/Admin/indexing"},
    {"file": "search/images/site-search-settings.webp", "path": "/Admin/Settings/search"},
    {"file": "search/images/sitemaps.webp", "path": "/Admin/Sitemaps/List"},
    {"file": "search/images/sitemaps-cache.webp", "path": "/Admin/SitemapsCache/List"},
    {"file": "search/images/blog-feed.webp", "path": "/Admin/Contents/ContentItems/?Options.SelectedContentType=Blog",
     "steps": [("follow", "a[href*='/Edit']:has-text('Edit')")], "full_page": True},
    # Design
    {"file": "design/images/themes.webp", "path": "/Admin/Themes"},
    {"file": "design/images/layers.webp", "path": "/Admin/Layers"},
    {"file": "design/images/zones.webp", "path": "/Admin/Settings/zones"},
    {"file": "design/images/templates.webp", "path": "/Admin/Templates"},
    {"file": "design/images/create-template.webp", "path": "/Admin/Templates/Create"},
    {"file": "design/images/placements.webp", "path": "/Admin/Placements"},
    {"file": "design/images/shortcodes.webp", "path": "/Admin/Shortcodes"},
    {"file": "design/images/menu.webp", "path": "/Admin/Contents/ContentItems/Menu",
     "steps": [("follow", "a[href*='/Edit']:has-text('Edit')")]},
    {"file": "design/images/admin-menus.webp", "path": "/Admin/AdminMenu/List"},
    {"file": "design/images/dashboard.webp", "path": "/Admin"},
    # Forms and automation
    {"file": "forms-and-automation/images/workflows.webp", "path": "/Admin/Workflows/Types"},
    {"file": "forms-and-automation/images/workflow-designer.webp", "path": "/Admin/Workflows/Types",
     "steps": [("follow", "a[href*='/Workflows/Types/Edit/']"), ("wait", 1500)]},
    {"file": "forms-and-automation/images/data-pipelines.webp", "path": "/Admin/DataPipelines"},
    {"file": "forms-and-automation/images/data-pipeline-runs.webp", "path": "/Admin/DataPipelines/Runs"},
    {"file": "forms-and-automation/images/shared-links.webp", "path": "/Admin/SharedLinks"},
    {"file": "forms-and-automation/images/notifications.webp", "path": "/Admin/notifications"},
    # Users and access
    {"file": "users/images/users.webp", "path": "/Admin/Users/Index"},
    {"file": "users/images/create-user.webp", "path": "/Admin/Users/Create"},
    {"file": "users/images/roles.webp", "path": "/Admin/Roles/Index"},
    {"file": "users/images/role-editor.webp", "path": "/Admin/Roles/Edit/Editor"},
    {"file": "users/images/user-login-settings.webp", "path": "/Admin/Settings/userLogin"},
    {"file": "users/images/registration-settings.webp", "path": "/Admin/Settings/userRegistration"},
    {"file": "users/images/reset-password-settings.webp", "path": "/Admin/Settings/userResetPassword"},
    {"file": "users/images/microsoft-account.webp", "path": "/Admin/Settings/OrchardCore.Microsoft.Authentication.MicrosoftAccount"},
    {"file": "users/images/google-authentication.webp", "path": "/Admin/Settings/OrchardCore.Google.GoogleAuthentication"},
    {"file": "users/images/github-authentication.webp", "path": "/Admin/Settings/OrchardCore.GitHub.Authentication"},
    {"file": "users/images/openid-server.webp", "path": "/Admin/OpenId/ServerConfiguration"},
    {"file": "users/images/openid-applications.webp", "path": "/Admin/OpenId/Application"},
    {"file": "users/images/openid-scopes.webp", "path": "/Admin/OpenId/Scope"},
    {"file": "users/images/openid-validation.webp", "path": "/Admin/OpenId/ValidationConfiguration"},
    # Site settings
    {"file": "settings/images/general.webp", "path": "/Admin/Settings/general"},
    {"file": "settings/images/admin-settings.webp", "path": "/Admin/Settings/admin"},
    {"file": "settings/images/debugging.webp", "path": "/Admin/Settings/debugging"},
    {"file": "settings/images/email.webp", "path": "/Admin/Settings/email"},
    {"file": "settings/images/email-test.webp", "path": "/Admin/Email/Test"},
    {"file": "settings/images/sms.webp", "path": "/Admin/Settings/sms"},
    {"file": "settings/images/sms-test.webp", "path": "/Admin/sms/test"},
    {"file": "settings/images/security-headers.webp", "path": "/Admin/Settings/SecurityHeaders"},
    {"file": "settings/images/cors.webp", "path": "/Admin/Cors"},
    {"file": "settings/images/recaptcha.webp", "path": "/Admin/Settings/recaptcha"},
    {"file": "settings/images/rate-limits.webp", "path": "/Admin/RateLimits"},
    {"file": "settings/images/reverse-proxy.webp", "path": "/Admin/Settings/ReverseProxy"},
    {"file": "settings/images/google-analytics.webp", "path": "/Admin/Settings/OrchardCore.Google.Analytics"},
    {"file": "settings/images/google-tag-manager.webp", "path": "/Admin/Settings/OrchardCore.Google.TagManager"},
    {"file": "settings/images/meta-app.webp", "path": "/Admin/Settings/OrchardCore.Facebook"},
    {"file": "settings/images/meta-pixel.webp", "path": "/Admin/Settings/facebook-pixel"},
    {"file": "settings/images/secrets.webp", "path": "/Admin/Secrets/Index"},
    {"file": "settings/images/secret-stores.webp", "path": "/Admin/Secrets/Stores/Index"},
    {"file": "settings/images/background-tasks.webp", "path": "/Admin/BackgroundTasks"},
    {"file": "settings/images/url-rewriting.webp", "path": "/Admin/UrlRewriting/Index"},
    {"file": "settings/images/graphiql.webp", "path": "/Admin/GraphQL", "steps": [("wait", 2000)]},
    # Administration
    {"file": "administration/images/deployment-plans.webp", "path": "/Admin/DeploymentPlan/Index"},
    {"file": "administration/images/package-import.webp", "path": "/Admin/DeploymentPlan/Import/Index"},
    {"file": "administration/images/json-import.webp", "path": "/Admin/DeploymentPlan/Import/Json"},
    {"file": "administration/images/recipes.webp", "path": "/Admin/Recipes"},
    {"file": "administration/images/remote-clients.webp", "path": "/Admin/Deployment/RemoteClient/Index"},
    {"file": "administration/images/remote-instances.webp", "path": "/Admin/Deployment/RemoteInstance/Index"},
    {"file": "administration/images/tenants.webp", "path": "/Admin/Tenants"},
    {"file": "administration/images/create-tenant.webp", "path": "/Admin/Tenants/Create"},
    {"file": "administration/images/feature-profiles.webp", "path": "/Admin/TenantFeatureProfiles"},
    # Technical Manual: Secrets. Needs a secret to list and edit.
    {"file": "../reference/modules/Secrets/images/features-secrets.webp", "path": "/Admin/Features",
     "steps": [("fill", "#search-box", "Secrets"), ("wait", 500)]},
    {"file": "../reference/modules/Secrets/images/admin-menu.webp", "path": "/Admin/Secrets/Index",
     "steps": [("scroll", "#left-nav li.current")]},
    {"file": "../reference/modules/Secrets/images/secrets-list.webp", "path": "/Admin/Secrets/Index"},
    {"file": "../reference/modules/Secrets/images/secret-create.webp", "path": "/Admin/Secrets/Create?type=TextSecret"},
    {"file": "../reference/modules/Secrets/images/secret-edit.webp", "path": "/Admin/Secrets/Index",
     "steps": [("follow", "a[href*='/Secrets/Edit/']")]},
    # Guide 'Implement a full-text search'. Needs the 'Blog - Lucene Search' recipe of TheBlogTheme.
    {"file": "../guides/implement-fulltext-search/images/lucene-feature.webp", "path": "/Admin/Features",
     "steps": [("fill", "#search-box", "Lucene"), ("wait", 500)]},
    {"file": "../guides/implement-fulltext-search/images/indexes.webp", "path": "/Admin/indexing"},
    {"file": "../guides/implement-fulltext-search/images/create-index.webp", "path": "/Admin/indexing/create/Lucene/Content"},
    {"file": "../guides/implement-fulltext-search/images/search-settings.webp", "path": "/Admin/Settings/search"},
    {"file": "../guides/implement-fulltext-search/images/anonymous-role.webp", "path": "/Admin/Roles/Edit/Anonymous",
     "steps": [("type", "#search-box", "index"), ("wait", 500)]},
    {"file": "../guides/implement-fulltext-search/images/search-feature.webp", "path": "/Admin/Features",
     "steps": [("fill", "#search-box", "frontend search"), ("wait", 500)]},
    {"file": "../guides/implement-fulltext-search/images/search-page.webp", "path": "/search?Terms=man", "anonymous": True,
     "steps": SEARCH_RESULTS},
    {"file": "../guides/implement-fulltext-search/images/blog-post-full-text.webp", "path": "/Admin/ContentTypes/Edit/BlogPost",
     "steps": [("check", "#ContentTypeDefinition_IncludeFullTextTemplate"),
               ("scroll", "#ContentTypeDefinition_IncludeFullTextTemplate")]},
    # Guide 'Lucene query syntax'. Needs the search of the guide above, and the blog posts 'Lucenes Lorem' (subtitle
    # 'What is Lorem Ipsum?', the Lorem Ipsum text as body) and 'Getting started with Lucene'.
    {"file": "../guides/lucene-query-syntax/images/operator-and.webp", "path": "/search?Terms=lorem%20and%20man", "anonymous": True,
     "steps": SEARCH_RESULTS},
    {"file": "../guides/lucene-query-syntax/images/required-operator.webp", "path": "/search?Terms=%2BMan%20explore", "anonymous": True,
     "steps": SEARCH_RESULTS},
    {"file": "../guides/lucene-query-syntax/images/operator-not.webp", "path": "/search?Terms=Lucene%20NOT%20Lorem", "anonymous": True,
     "steps": SEARCH_RESULTS},
    {"file": "../guides/lucene-query-syntax/images/prohibition-operator.webp", "path": "/search?Terms=Lucene%20-Lorem", "anonymous": True,
     "steps": SEARCH_RESULTS},
    {"file": "../guides/lucene-query-syntax/images/match-all.webp", "path": "/search?Terms=*%3A*", "anonymous": True,
     "steps": SEARCH_RESULTS},
    {"file": "../guides/lucene-query-syntax/images/fuzzy-search.webp", "path": "/search?Terms=Lucen~", "anonymous": True,
     "steps": SEARCH_RESULTS},
    {"file": "../guides/lucene-query-syntax/images/text-field.webp", "path": "/search?Terms=Subtitle%3AWhat%20is%20Lorem%20Ipsum%3F", "anonymous": True,
     "steps": SEARCH_RESULTS},
    {"file": "../guides/lucene-query-syntax/images/one-character-wildcard.webp", "path": "/search?Terms=lu%3Fene", "anonymous": True,
     "steps": SEARCH_RESULTS},
    {"file": "../guides/lucene-query-syntax/images/multi-character-wildcard.webp", "path": "/search?Terms=Lucene*", "anonymous": True,
     "steps": SEARCH_RESULTS},
    {"file": "../guides/lucene-query-syntax/images/range-searches.webp", "path": "/search?Terms=A%20TO%20P", "anonymous": True,
     "steps": SEARCH_RESULTS},
    {"file": "../guides/lucene-query-syntax/images/regular-expressions.webp", "path": "/search?Terms=%2Ftype.*ing%3F%2F", "anonymous": True,
     "steps": SEARCH_RESULTS},
    # Technical Manual: Workflows. Needs the 'Article approval' workflow (Http Request Event 'Article submitted', Notify
    # Task 'Notify the editors', Signal Event 'Wait for approval', Notify Task 'Notify the author', one under the other),
    # run once.
    {"file": "../reference/modules/Workflows/images/workflow-designer.webp", "path": "/Admin/Workflows/Types",
     "steps": [("follow", "a[href*='/Workflows/Types/Edit/']:has-text('Article approval')"), ("wait", 1500),
               ("type", "#wfd-toolbox-search", "notify"), ("click", ".wfd-node:has(h4:text-is('Notify the author'))"), ("wait", 800)]},
    {"file": "../reference/modules/Workflows/images/workflow-instance.webp", "path": "/Admin/Workflows/Types",
     "steps": [("follow", "a[href*='/Instances/Index']:near(a:has-text('Article approval'))"),
               ("follow", "a[href*='/Workflow/Details/']"), ("wait", 1500),
               ("click", ".wfd-node:has(h4:text-is('Wait for approval'))"), ("wait", 800)]},
    # Technical Manual: Layers. Needs the layer 'This is a Layer', with a Homepage condition.
    {"file": "../reference/modules/Layers/images/create-layer.webp", "path": "/Admin/Layers/Create",
     "steps": [("fill", "input[name='Name']", "This is a Layer"),
               ("fill", "textarea[name='Description']", "Add a description. Say it is part of an onion or something.")]},
    {"file": "../reference/modules/Layers/images/add-widget.webp", "path": "/Admin/Layers",
     "steps": [("click", ".layer-zone button.dropdown-toggle"), ("wait", 300)]},
    {"file": "../reference/modules/Layers/images/layer-rules.webp", "path": "/Admin/Layers/Edit?name=This%20is%20a%20Layer",
     "steps": [("scroll", "button[data-bs-target='#modalConditions']"), ("click", "button[data-bs-target='#modalConditions']"),
               ("wait", 500)]},
    # Technical Manual: Themes. Needs the Project type (Title, a Url text field, Markdown with the Wysiwyg editor), the
    # Portfolio type (Title, Autoroute, a Bag of projects) and the portfolio 'My Projects' at /portfolio with two projects.
    {"file": "../reference/modules/Themes/images/portfolio-page.webp", "path": "/portfolio", "anonymous": True, "full_page": True},
    {"file": "../reference/modules/Themes/images/new-content-type.webp", "path": "/Admin/ContentTypes/Create",
     "steps": [("type", "input[name='DisplayName']", "Project")]},
    {"file": "../reference/modules/Themes/images/project-type.webp", "path": "/Admin/ContentTypes/Edit/Project",
     "steps": [("scroll", "#fields", "start")]},
    {"file": "../reference/modules/Themes/images/portfolio-type.webp", "path": "/Admin/ContentTypes/Edit/Portfolio",
     "steps": [("scroll", "#fields", "start")]},
    # Topic: Content relationships. Needs the FAQ (a Bag of FAQ items), Product and Bundle (a content picker of products
    # and a price) types, with the items they show, and the 'About us' page with a flow of widgets.
    {"file": "../topics/content-relationships/images/faq-page-editor.webp", "path": "/Admin/Contents/ContentItems/?Options.SelectedContentType=FAQ",
     "steps": [("follow", "a[href*='/Edit']:has-text('Frequently Asked Questions')")]},
    {"file": "../topics/content-relationships/images/product-bundle.webp", "path": "/Admin/Contents/ContentItems/?Options.SelectedContentType=Bundle",
     "steps": [("follow", "a[href*='/Edit']:has-text('Space Invaders Christmas Bundle')")]},
    {"file": "../topics/content-relationships/images/page-editor-with-flow-part.webp", "path": "/Admin/Contents/ContentItems/?Options.SelectedContentType=Page",
     "steps": [("follow", "a[href*='/Edit']:has-text('About us')")], "full_page": True},
    {"file": "../topics/content-relationships/images/blog-posts-list.webp", "path": "/Admin/Contents/ContentItems/?Options.SelectedContentType=Blog",
     "steps": [("follow", "a:has-text('BlogPost Items')")]},
    {"file": "../topics/content-relationships/images/blog-post-tags-categories.webp", "path": "/Admin/Contents/ContentItems/?Options.SelectedContentType=BlogPost",
     "steps": [("follow", "a[href*='/Edit']:has-text('Man must explore')"), ("scroll", "[name$='TagNames'], .tags-editor, [id*='Tags']")]},
    {"file": "../topics/content-relationships/images/taxonomy-editor.webp", "path": "/Admin/Contents/ContentItems/?Options.SelectedContentType=Taxonomy",
     "steps": [("follow", "a[href*='/Edit']:has-text('Categories')")]},
    # Technical Manual: Forms. Needs the 'Contact Form' page (a Form widget with a validation summary, Name, Email and
    # Message fields, a reCAPTCHA and a Submit! button) and the 'Contact Form Workflow' it posts to.
    {"file": "../reference/modules/Forms/images/contact-form-editor.webp", "path": "/Admin/Contents/ContentItems/?Options.SelectedContentType=Page",
     # The action holds the token of the workflow, shown as a placeholder.
     "steps": [("follow", "a[href*='/Edit']:has-text('Contact Form')"), ("fill", "input[name$='FormPart.Action']", "/workflows/invoke/{token}")],
     "full_page": True},
    {"file": "../reference/modules/Forms/images/contact-form.webp", "path": "/contact-form", "anonymous": True,
     "steps": [("scroll", "form", "start")]},
    {"file": "../reference/modules/Forms/images/contact-form-workflow.webp", "path": "/Admin/Workflows/Types",
     "steps": [("follow", "a[href*='/Workflows/Types/Edit/']:has-text('Contact Form Workflow')"), ("wait", 1500),
               ("click", "[data-cy='toolbox-collapse']"), ("click", "[data-cy='panel-collapse']"),
               ("click", "[data-cy='zoom-fit']"), ("wait", 800)]},
    # Guide 'Create a Blazor CMS': the setup screen of a tenant that isn't set up yet, filled in.
    {"file": "../guides/create-blazor-cms/images/setup.webp", "path": "/Admin/Tenants",
     "steps": [("follow", "a:has-text('Setup')"), ("fill", "#SiteName", "BlazorCMS"), ("click", "#recipeButton"),
               ("click", "a[data-recipe-name='Headless']"), ("fill", "#UserName", "admin"), ("fill", "#Email", "admin@example.com")]},
    # Guides 'Create a CMS application' and 'Decoupled CMS': the setup screen of a tenant that isn't set up yet, filled in.
    {"file": "../guides/create-cms-application/images/setup-screen.webp", "path": "/Admin/Tenants",
     "steps": [("follow", "a:has-text('Setup')"), ("fill", "#SiteName", "MySite"), ("click", "#recipeButton"),
               ("click", "a[data-recipe-name='Blog']"), ("fill", "#UserName", "admin"), ("fill", "#Email", "admin@example.com")]},
    {"file": "../guides/create-cms-application/images/blog-home-page.webp", "path": "/", "anonymous": True},
    {"file": "../guides/decoupled-cms/images/setup.webp", "path": "/Admin/Tenants",
     "steps": [("follow", "a:has-text('Setup')"), ("fill", "#SiteName", "My Site"), ("click", "#recipeButton"),
               ("click", "a[data-recipe-name='Blank']"), ("fill", "#UserName", "admin"), ("fill", "#Email", "admin@example.com")]},
    # Guide 'Decoupled CMS', in a tenant set up with the Blank site recipe, with the Markdown, Alias and Preview features.
    # First with a 'Blog Post' type without parts:
    {"file": "../guides/decoupled-cms/images/new-content-type.webp", "path": "/Admin/ContentTypes/Create", "tenant": "Decoupled",
     "steps": [("type", "#DisplayName", "Blog Post")]},
    {"file": "../guides/decoupled-cms/images/add-content-parts.webp", "path": "/Admin/ContentTypes/AddPartsTo/BlogPost", "tenant": "Decoupled",
     "steps": [("check", "li:has(label:text-is('Title')) input[type='checkbox']"),
               ("check", "li:has(label:text-is('MarkdownBody')) input[type='checkbox']")]},
    # Then with the Title and Markdown Body parts, the Markdown Body using the Wysiwyg editor:
    {"file": "../guides/decoupled-cms/images/edit-content-type.webp", "path": "/Admin/ContentTypes/Edit/BlogPost", "tenant": "Decoupled",
     "steps": [("scroll", ".ta-content", "end")]},
    {"file": "../guides/decoupled-cms/images/edit-markdownbody.webp", "path": "/Admin/ContentTypes/Edit/BlogPost", "tenant": "Decoupled",
     "steps": [("follow", "a[href*='/ContentParts/MarkdownBodyPart/Edit']")]},
    {"file": "../guides/decoupled-cms/images/edit-blogpost.webp", "path": "/Admin/Contents/ContentTypes/BlogPost/Create", "tenant": "Decoupled"},
    # Then with the Alias part after the Title, the Preview part with the pattern of the guide, and the blog post 'This
    # is a new day' with its generated alias 'this-is-a-new-day':
    {"file": "../guides/decoupled-cms/images/content-items.webp", "path": "/Admin/Contents/ContentItems", "tenant": "Decoupled"},
    {"file": "../guides/decoupled-cms/images/edit-alias.webp", "path": "/Admin/Contents/ContentItems", "tenant": "Decoupled",
     "steps": [("follow", "a[href*='/Edit']:has-text('This is a new day')"), ("fill", "input[name='AliasPart.Alias']", "new-day")]},
    {"file": "../guides/decoupled-cms/images/alias-pattern.webp", "path": "/Admin/ContentTypes/Edit/BlogPost", "tenant": "Decoupled",
     "steps": [("follow", "a[href*='/ContentParts/AliasPart/Edit']")]},
    {"file": "../guides/decoupled-cms/images/generated-alias.webp", "path": "/Admin/Contents/ContentItems", "tenant": "Decoupled",
     "steps": [("follow", "a[href*='/Edit']:has-text('This is a new day')")]},
    {"file": "../guides/decoupled-cms/images/preview-pattern.webp", "path": "/Admin/ContentTypes/Edit/BlogPost", "tenant": "Decoupled",
     "steps": [("follow", "a[href*='/ContentParts/PreviewPart/Edit']")]},
    # Guide 'Microsoft Entra ID integration', in a tenant set up with the Blog recipe. Taken before enabling the
    # Microsoft Entra ID Authentication and Users Registration features:
    {"file": "../guides/microsoft-entra-id-integration/images/entra-id-feature.webp", "path": "/Admin/Features", "tenant": "EntraId",
     "steps": [("fill", "#search-box", "Entra"), ("wait", 500)]},
    {"file": "../guides/microsoft-entra-id-integration/images/users-registration-feature.webp", "path": "/Admin/Features", "tenant": "EntraId",
     "steps": [("fill", "#search-box", "Registration"), ("wait", 500)]},
    # Then with both features enabled, and the Entra ID settings of the guide, with placeholder ids: the display name
    # 'My Company Microsoft account', the external registration without password, user name or email, and the user name
    # script of the guide.
    {"file": "../guides/microsoft-entra-id-integration/images/entra-id-settings.webp", "tenant": "EntraId",
     "path": "/Admin/Settings/OrchardCore.Microsoft.Authentication.AzureAD"},
    {"file": "../guides/microsoft-entra-id-integration/images/registration-settings.webp", "path": "/Admin/Settings/userRegistration", "tenant": "EntraId"},
    {"file": "../guides/microsoft-entra-id-integration/images/external-registration-settings.webp", "tenant": "EntraId",
     "path": "/Admin/Settings/userRegistration", "steps": [("click", "[role='tab']:has-text('External Authentication')"), ("wait", 500)],
     "full_page": True},
    {"file": "../guides/microsoft-entra-id-integration/images/login.webp", "path": "/Login", "tenant": "EntraId", "anonymous": True},
    {"file": "../reference/modules/Microsoft.Authentication/images/entra-id-login-button.webp", "path": "/Login", "tenant": "EntraId", "anonymous": True},
    # Technical Manual: Admin theme, in the tenant with the horizontal form layout.
    {"file": "../reference/themes/images/horizontal-form-layout.webp", "path": "/Admin/Contents/ContentTypes/BlogPost/Create",
     "tenant": "Horizontal", "full_page": True},
    # Guide 'Integrate Meta plugins'. Needs the Meta Widgets feature.
    {"file": "../guides/integrate-facebook-plugins/images/meta-widgets-feature.webp", "path": "/Admin/Features",
     "steps": [("fill", "#search-box", "Meta"), ("wait", 500)]},
    {"file": "../guides/integrate-facebook-plugins/images/meta-app-settings.webp", "path": "/Admin/Settings/OrchardCore.Facebook"},
    {"file": "../guides/integrate-facebook-plugins/images/add-meta-widget.webp", "path": "/Admin/Layers",
     "steps": [("click", ":nth-match(.layer-zone button.dropdown-toggle, 2)"), ("wait", 300)]},
    {"file": "../guides/integrate-facebook-plugins/images/meta-quote-widget.webp",
     "path": "/Admin/Contents/ContentTypes/FacebookQuote/Create?returnUrl=%2FAdmin%2FLayers&LayerMetadata.Zone=Footer&LayerMetadata.Position=11",
     "steps": [("fill", "#LayerMetadata_Title", "FB Quote"), ("select", "#LayerMetadata_LayerMetadata_Layer", "Always")]},
    # Guide 'Create a Blazor CMS', on its own site, with the 'Markdown Page' type and the 'ORCHARD CORE' item of the
    # guide. First without the Tenants feature:
    {"file": "../guides/create-blazor-cms/images/content-items.webp", "path": "/Admin/Contents/ContentItems", "site": "blazor"},
    {"file": "../guides/create-blazor-cms/images/enable-tenants.webp", "path": "/Admin/Features", "site": "blazor",
     "steps": [("fill", "#search-box", "manage tenants"), ("wait", 500)]},
    # Then with the Tenants feature:
    {"file": "../guides/create-blazor-cms/images/tenants.webp", "path": "/Admin/Tenants", "site": "blazor"},
    {"file": "../guides/create-blazor-cms/images/first-tenant.webp", "path": "/Admin/Tenants/Create", "site": "blazor",
     "steps": [("fill", "#Name", "FirstTenant"), ("fill", "#RequestUrlPrefix", "tenant01"), ("select", "#RecipeName", "Headless"),
               ("select", "#DatabaseProvider", "Sqlite")]},
    # Then with the tenant FirstTenant, not set up yet:
    {"file": "../guides/create-blazor-cms/images/setup-first-tenant.webp", "path": "/Admin/Tenants", "site": "blazor"},
    {"file": "../guides/create-blazor-cms/images/setup-screen-first-tenant.webp", "path": "/Admin/Tenants", "site": "blazor",
     "steps": [("follow", "a:has-text('Setup')"), ("fill", "#SiteName", "FirstTenant"), ("fill", "#UserName", "admin"),
               ("fill", "#Email", "admin@example.com")]},
    # Then with FirstTenant set up, with the base url of the guide:
    {"file": "../guides/create-blazor-cms/images/tenant-site-settings.webp", "path": "/Admin/Settings/general", "site": "blazor",
     "tenant": "FirstTenant"},
    {"file": "../guides/create-blazor-cms/images/import-content-tenant.webp", "path": "/Admin/DeploymentPlan/Import/Json", "site": "blazor",
     "tenant": "FirstTenant",
     "steps": [("editor", ".CodeMirror", """{
  "steps": [
    {
      "name": "Content",
      "data": [
        {
          "ContentType": "MarkdownPage",
          "DisplayText": "ORCHARD CORE FOR TENANT 01",
          "AliasPart": { "Alias": "orchard-core" },
          "TitlePart": { "Title": "ORCHARD CORE" }
        }
      ]
    }
  ]
}""")]},
]


def tenant_url(url, tenant):
    return url if tenant == "Default" else f"{url}/{TENANTS[tenant]}"


def sign_in(page, url, user, password):
    page.goto(url + "/Login")
    page.fill("input[name='LoginForm.UserName']", user)
    page.fill("input[name='LoginForm.Password']", password)
    page.click("button[type='submit']")
    page.wait_for_load_state("networkidle")
    if "/Login" in page.url:
        raise SystemExit(f"Sign-in failed at {url}: check --user and --password.")


def capture(page, url, shot):
    page.goto(url + shot["path"])
    page.wait_for_load_state("networkidle")

    for step in shot.get("steps", []):
        if step[0] == "follow":
            page.goto(urljoin(page.url, page.get_attribute(step[1], "href")))
            page.wait_for_load_state("networkidle")
        elif step[0] == "click":
            page.click(step[1])
            page.wait_for_load_state("networkidle")
        elif step[0] == "fill":
            page.fill(step[1], step[2])
        elif step[0] == "type":
            page.locator(step[1]).press_sequentially(step[2])
        elif step[0] == "editor":
            page.locator(step[1]).first.evaluate("(element, text) => element.CodeMirror.setValue(text)", step[2])
        elif step[0] == "select":
            page.select_option(step[1], step[2])
        elif step[0] == "check":
            page.check(step[1], force=True)
        elif step[0] == "uncheck":
            page.uncheck(step[1], force=True)
        elif step[0] == "scroll":
            block = step[2] if len(step) > 2 else "center"
            # Scroll at once: the admin theme scrolls smoothly, which would still be moving when the shot is taken.
            page.locator(step[1]).first.evaluate("(element, block) => element.scrollIntoView({ block, behavior: 'instant' })", block)
        elif step[0] == "wait":
            page.wait_for_timeout(step[1])

    # Let fonts and scripts settle, keep the mouse away from hover effects, and take the focus off the first box.
    page.evaluate("document.activeElement && document.activeElement.blur()")
    page.mouse.move(0, 0)
    page.wait_for_timeout(300)

    if shot.get("full_page"):
        # The admin menu and the action bar are fixed to the window: make the window as tall as the page (cut at 1600
        # pixels) so they are laid out as on screen, instead of stitching the page together.
        height = min(page.evaluate("document.documentElement.scrollHeight"), 1600)
        page.set_viewport_size({"width": VIEWPORT["width"], "height": height})
        page.wait_for_timeout(300)
        png = page.screenshot()
        page.set_viewport_size(VIEWPORT)
    else:
        png = page.screenshot()

    target = os.path.normpath(os.path.join(ROOT, shot["file"]))
    os.makedirs(os.path.dirname(target), exist_ok=True)
    Image.open(io.BytesIO(png)).convert("RGB").save(target, "WEBP", quality=QUALITY, method=6)
    return target


def main():
    parser = argparse.ArgumentParser(description="Take the screenshots of the documentation.")
    parser.add_argument("--url", default="http://localhost:5000")
    parser.add_argument("--site", default="blog", choices=SITES, help="the site --url points to")
    parser.add_argument("--user", default="admin")
    parser.add_argument("--password", default=os.environ.get("OC_DOCS_PASSWORD"))
    parser.add_argument("--browser", default="msedge", help="msedge, chrome or chromium")
    parser.add_argument("--only", nargs="+", help="take only the shots whose file contains one of these texts")
    args = parser.parse_args()

    if not args.password:
        raise SystemExit("Pass --password or set OC_DOCS_PASSWORD.")

    url = args.url.rstrip("/")
    shots = [
        shot for shot in SHOTS
        if shot.get("site", "blog") == args.site and (not args.only or any(text in shot["file"] for text in args.only))
    ]
    failed = []

    with sync_playwright() as playwright:
        channel = None if args.browser == "chromium" else args.browser
        browser = playwright.chromium.launch(channel=channel)
        options = {"viewport": VIEWPORT, "color_scheme": "light", "locale": "en-US"}
        pages = {}

        def page_for(tenant, anonymous):
            # One browser context per tenant, signed in or not, opened the first time a shot needs it.
            if (tenant, anonymous) not in pages:
                context = browser.new_context(**options)
                # The admin theme remembers the menu sections opened on earlier pages: start every page afresh.
                context.add_init_script("try { localStorage.clear(); sessionStorage.clear(); } catch (e) {}")
                # The admin theme reads the scheme a user picked from the '{tenant}-TheAdmin-scheme' cookie.
                context.add_cookies([{"name": f"{tenant}-TheAdmin-scheme", "value": SCHEME, "url": url}])
                if not anonymous:
                    sign_in(context.new_page(), tenant_url(url, tenant), args.user, args.password)
                pages[(tenant, anonymous)] = context.new_page()
            return pages[(tenant, anonymous)]

        for shot in shots:
            try:
                tenant = shot.get("tenant", "Default")
                page = page_for(tenant, bool(shot.get("anonymous")))
                target = capture(page, tenant_url(url, tenant), shot)
                print(f"{shot['file']}  {os.path.getsize(target) // 1024} KB")
            except Exception as error:  # noqa: BLE001 - report every failing shot, then carry on.
                failed.append(shot["file"])
                print(f"{shot['file']}  FAILED: {str(error).splitlines()[0]}", file=sys.stderr)

        browser.close()

    if failed:
        raise SystemExit(f"{len(failed)} screenshot(s) failed.")


if __name__ == "__main__":
    main()
