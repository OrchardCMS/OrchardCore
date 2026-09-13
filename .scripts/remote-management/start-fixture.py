#!/usr/bin/env python3
"""Start an isolated loopback Orchard tenant for CLI tests. Never use production data."""
import json
import os
from pathlib import Path
import secrets
import socket
import ssl
import subprocess
import tempfile
import time
import urllib.request

repo = Path(__file__).resolve().parents[2]
root = Path(tempfile.mkdtemp(prefix="pomi-cli-fixture-"))
os.chmod(root, 0o700)
(root / "Recipes").mkdir()
(root / "wwwroot").mkdir()
# Reuse the shipped Blank recipe while removing its full-line JSON comments.
blank = (repo / "src/OrchardCore.Themes/TheAdmin/Recipes/blank.recipe.json").read_text()
recipe = json.loads("\n".join(line for line in blank.splitlines() if not line.lstrip().startswith("//")))
recipe["name"] = "CliFixture"
# Keep tenant static serving enabled to prove it exposes no management API.
recipe["steps"][0]["enable"] += ["OrchardCore.RemoteManagement.Cli", "OrchardCore.Tenants", "OrchardCore.Tenants.FileProvider", "OrchardCore.Workflows", "OrchardCore.Queries.Sql", "OrchardCore.Localization", "OrchardCore.DataLocalization", "OrchardCore.Apis.GraphQL"]
secret = secrets.token_urlsafe(32)
recipe["steps"] += [
    {"name": "settings", "LayerSettings": {"Zones": ["Content", "Footer"]}},
    {"name": "RemoteManagementConfiguration"},
    {"name": "RemoteManagementCliConfiguration"},
    {"name": "Roles", "Roles": [{"Name": "CliDiscovery", "Permissions": ["AccessRemoteManagement"]}]},
    {"name": "OpenIdApplication", "ClientId": "cli-discovery", "ClientSecret": secret,
     "DisplayName": "Disposable discovery-only client", "Type": "confidential", "ConsentType": "implicit",
     "AllowClientCredentialsFlow": True, "RoleEntries": [{"Name": "CliDiscovery"}],
     "ScopeEntries": [{"Name": "orchardcore.management"}]},
    {"name": "OpenIdApplication", "ClientId": "cli-fixture", "ClientSecret": secret,
     "DisplayName": "Disposable CLI fixture", "Type": "confidential", "ConsentType": "implicit",
     "AllowClientCredentialsFlow": True, "RoleEntries": [{"Name": "Administrator"}],
     "ScopeEntries": [{"Name": "orchardcore.management"}]},
    {"name": "OpenIdApplication", "ClientId": "cli-denied", "ClientSecret": secret,
     "DisplayName": "Disposable denied client", "Type": "confidential", "ConsentType": "implicit",
     "AllowClientCredentialsFlow": True, "ScopeEntries": [{"Name": "orchardcore.management"}]},
]
# Separate identities verify resource permissions without the admin wildcard.
for suffix, permissions in [
    ("graphql-reader", ["ExecuteGraphQL"]),
    ("layers", ["ManageLayers"]),
    ("feature-profiles", ["ManageTenantFeatureProfiles"]),
    ("tenants", ["ManageTenants"]),
    ("openid-applications", ["ManageApplications"]),
    ("openid-scopes", ["ManageScopes"]),
    ("home-route", ["SetHomeRoute"]),
    ("culture-picker", ["ManageContentCulturePicker"]),
    ("content-localizer", ["LocalizeContent", "EditContent", "ViewContent", "PreviewContent"]),
    ("content-localizer-no-edit", ["LocalizeContent", "ViewContent", "PreviewContent"]),
    ("content-reader", ["ViewContent"]),
    ("user-policy", ["ManageUsers"]),
    ("remote-clients", ["ManageRemoteClients"]),
    ("remote-instances", ["ManageRemoteInstances"]),
    ("remote-export", ["ExportRemoteInstances", "Export"]),
    ("remote-export-no-data", ["ExportRemoteInstances"]),
    ("content-export-no-edit", ["Export"]),
    ("widgets", ["ManageLayers", "EditContent", "PublishContent", "ViewContent", "PreviewContent"]),
    ("widgets-editor", ["ManageLayers", "EditContent", "ViewContent", "PreviewContent"]),
    ("indexes", ["ManageIndexes"]),
    ("shortcodes", ["ManageShortcodeTemplates"]),
    ("https", ["ManageHttps"]),
    ("email", ["ManageEmailSettings"]),
    ("url-rewriting", ["ManageUrlRewritingRules"]),
    ("cors", ["ManageCorsSettings"]),
    ("sitemaps", ["ManageSitemaps"]),
    ("seo", ["ManageSeoSettings"]),
    ("audit-trail", ["ViewAuditTrail"]),
    ("background-tasks", ["ManageBackgroundTasks"]),
    ("rate-limits", ["ManageRateLimits"]),
    ("security-headers", ["ManageSecurityHeadersSettings"]),
    ("placements", ["ManagePlacements"]),
    ("translator-fr", ["ViewDynamicTranslations", "ManageTranslations_fr"]),
    ("translation-reader", ["ViewDynamicTranslations"]),
    ("media-profiles", ["ManageMediaProfiles"]),
    ("media-cache", ["ManageAssetCache"]),
    ("media-settings", ["ManageMediaApiSettings"]),
    ("media", ["ManageMediaContent", "ManageMediaFolder"]),
    ("media-restricted", ["ManageMediaContent", "ManageMediaFolder", "UploadRestrictedMedia"]),
    ("media-no-folder", ["ManageOwnMediaContent"]),
]:
    role = "Cli-" + suffix
    discovery_permissions = [] if suffix == "graphql-reader" else ["AccessRemoteManagement", "ViewOpenApiContent"]
    recipe["steps"] += [
        {"name": "Roles", "Roles": [{"Name": role, "Permissions": [*discovery_permissions, *permissions]}]},
        {"name": "OpenIdApplication", "ClientId": "cli-" + suffix, "ClientSecret": secret,
         "DisplayName": "Disposable " + suffix + " client", "Type": "confidential", "ConsentType": "implicit",
         "AllowClientCredentialsFlow": True, "RoleEntries": [{"Name": role}],
         "ScopeEntries": [{"Name": "orchardcore.management"}]},
    ]
(root / "Recipes/cli-fixture.recipe.json").write_text(json.dumps(recipe))
with socket.socket() as sock:
    sock.bind(("127.0.0.1", 0))
    port = sock.getsockname()[1]
url = f"http://127.0.0.1:{port}/"
http_url = url
listen_urls = url
ssl_context = None
certificate = None
env = os.environ.copy()
env.update({"ASPNETCORE_ENVIRONMENT": "Development", "ORCHARD_APP_DATA": str(root / "App_Data")})
if os.environ.get("OC_FIXTURE_HTTPS") == "1":
    # Use the already trusted development certificate; never change trust settings from a test.
    subprocess.run(["dotnet", "dev-certs", "https", "--check", "--trust"], check=True, capture_output=True, timeout=30)
    certificate = root / "certificate.pem"
    subprocess.run(["dotnet", "dev-certs", "https", "--export-path", str(certificate), "--format", "PEM"],
                   check=True, capture_output=True, timeout=30)
    # This exports only the public certificate for Python's trust bundle.
    # Kestrel uses the development certificate from its existing store.
    with socket.socket() as sock:
        sock.bind(("127.0.0.1", 0))
        https_port = sock.getsockname()[1]
    url = f"https://localhost:{https_port}/"
    http_url = f"http://localhost:{port}/"
    listen_urls = http_url + ";" + url
    ssl_context = ssl.create_default_context(cafile=str(certificate))
if os.environ.get("OC_FIXTURE_TENANT_REMOVAL") == "1":
    env["OrchardCore__OrchardCore_Tenants__TenantRemovalAllowed"] = "true"
setup = {"ShellName": "Default", "SiteName": "CLI review", "SiteTimeZone": "UTC",
         "AdminUsername": "admin", "AdminEmail": "admin@example.test", "AdminPassword": secrets.token_urlsafe(32) + "aA1!",
         "DatabaseProvider": "Sqlite", "RecipeName": "CliFixture"}
for key, value in setup.items():
    env["OrchardCore__OrchardCore_AutoSetup__Tenants__0__" + key] = value
log = (root / "host.log").open("w")
host = subprocess.Popen(["dotnet", str(repo / "src/OrchardCore.Cms.Web/bin/Debug/net10.0/OrchardCore.Cms.Web.dll"), "--contentRoot", str(root), "--urls", listen_urls], env=env, stdout=log, stderr=log)
state = {"url": url, "pid": host.pid, "root": str(root), "OC_CLIENT_ID": "cli-fixture", "OC_CLIENT_SECRET": secret,
         "OC_CONFIG_HOME": str(root / "cli"), "adminPassword": setup["AdminPassword"]}
if certificate:
    state.update(httpUrl=http_url, certificatePath=str(certificate))
(root / "fixture.json").write_text(json.dumps(state))
for attempt in range(90):
    if host.poll() is not None:
        raise SystemExit(f"Fixture host exited. Inspect {root / 'host.log'}")
    try:
        with urllib.request.urlopen(url + ".well-known/orchardcore-management", timeout=20, context=ssl_context) as response:
            if response.status == 200:
                print(root / "fixture.json", flush=True)
                break
    except Exception:
        time.sleep(1)
else:
    host.terminate()
    raise SystemExit(f"Fixture did not become ready. Inspect {root / 'host.log'}")

try:
    host.wait()
except KeyboardInterrupt:
    host.terminate()
    host.wait(timeout=15)
