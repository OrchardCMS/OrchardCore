"""Takes the screenshots of the User Manual from a running Orchard Core site.

Each entry of SHOTS names an image under src/docs/user-manual/ and the admin page it shows. Run the script again
whenever a screen changes, and commit the images that change.

The site must be set up with the Blog recipe, with an `admin` account and the features the pages describe turned on
(see "Screenshots" in src/docs/contributing/contributing-documentation.md).

Usage:
    pip install playwright pillow
    python .scripts/docs/screenshots.py --url http://localhost:5000 --password <admin password> [--only <name>]

The script uses the Microsoft Edge or Google Chrome installed on the machine. Without either, run
`python -m playwright install chromium` and pass `--browser chromium`.
"""

import argparse
import io
import os
import sys

from PIL import Image
from playwright.sync_api import sync_playwright

ROOT = os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", "..", "src", "docs", "user-manual")
VIEWPORT = {"width": 1280, "height": 800}
QUALITY = 82

# Each shot:
#   file:      the image, relative to src/docs/user-manual/.
#   path:      the address to open, relative to the site.
#   steps:     optional actions after opening the page: ("follow", selector) opens the address of the first matching
#              link, even a hidden one; ("click", selector); ("fill", selector, value); ("wait", milliseconds).
#   anonymous: true to take the shot signed out.
#   full_page: true to capture the whole page instead of the window (cut at 1600 pixels).
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
]


def sign_in(page, url, user, password):
    page.goto(url + "/Login")
    page.fill("input[name='LoginForm.UserName']", user)
    page.fill("input[name='LoginForm.Password']", password)
    page.click("button[type='submit']")
    page.wait_for_load_state("networkidle")
    if "/Login" in page.url:
        raise SystemExit("Sign-in failed: check --user and --password.")


def capture(page, url, shot):
    page.goto(url + shot["path"])
    page.wait_for_load_state("networkidle")

    for step in shot.get("steps", []):
        if step[0] == "follow":
            page.goto(url + page.get_attribute(step[1], "href").removeprefix(url))
            page.wait_for_load_state("networkidle")
        elif step[0] == "click":
            page.click(step[1])
            page.wait_for_load_state("networkidle")
        elif step[0] == "fill":
            page.fill(step[1], step[2])
        elif step[0] == "wait":
            page.wait_for_timeout(step[1])

    # Let fonts and scripts settle, keep the mouse away from hover effects, and take the focus off the first box.
    page.evaluate("document.activeElement && document.activeElement.blur()")
    page.mouse.move(0, 0)
    page.wait_for_timeout(300)

    if shot.get("full_page"):
        height = min(page.evaluate("document.documentElement.scrollHeight"), 1600)
        png = page.screenshot(clip={"x": 0, "y": 0, "width": VIEWPORT["width"], "height": height}, full_page=True)
    else:
        png = page.screenshot()

    target = os.path.normpath(os.path.join(ROOT, shot["file"]))
    os.makedirs(os.path.dirname(target), exist_ok=True)
    Image.open(io.BytesIO(png)).convert("RGB").save(target, "WEBP", quality=QUALITY, method=6)
    return target


def main():
    parser = argparse.ArgumentParser(description="Take the screenshots of the User Manual.")
    parser.add_argument("--url", default="http://localhost:5000")
    parser.add_argument("--user", default="admin")
    parser.add_argument("--password", default=os.environ.get("OC_DOCS_PASSWORD"))
    parser.add_argument("--browser", default="msedge", help="msedge, chrome or chromium")
    parser.add_argument("--only", help="take only the shots whose file contains this text")
    args = parser.parse_args()

    if not args.password:
        raise SystemExit("Pass --password or set OC_DOCS_PASSWORD.")

    url = args.url.rstrip("/")
    shots = [shot for shot in SHOTS if not args.only or args.only in shot["file"]]
    failed = []

    with sync_playwright() as playwright:
        channel = None if args.browser == "chromium" else args.browser
        browser = playwright.chromium.launch(channel=channel)
        options = {"viewport": VIEWPORT, "color_scheme": "light", "locale": "en-US"}
        signed_in = browser.new_context(**options)
        anonymous = browser.new_context(**options)
        for context in (signed_in, anonymous):
            # The admin theme remembers the menu sections opened on earlier pages: start every page afresh.
            context.add_init_script("try { localStorage.clear(); sessionStorage.clear(); } catch (e) {}")
        sign_in(signed_in.new_page(), url, args.user, args.password)
        pages = {False: signed_in.new_page(), True: anonymous.new_page()}

        for shot in shots:
            try:
                target = capture(pages[bool(shot.get("anonymous"))], url, shot)
                print(f"{shot['file']}  {os.path.getsize(target) // 1024} KB")
            except Exception as error:  # noqa: BLE001 - report every failing shot, then carry on.
                failed.append(shot["file"])
                print(f"{shot['file']}  FAILED: {str(error).splitlines()[0]}", file=sys.stderr)

        browser.close()

    if failed:
        raise SystemExit(f"{len(failed)} screenshot(s) failed.")


if __name__ == "__main__":
    main()
