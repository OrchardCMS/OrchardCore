---
name: orchardcore-cli-settings
description: Reads and updates Orchard Core Site Settings, typed module sections, Custom Settings, cultures, and URL rewrite rules through `pomi`. Use for tenant-wide configuration, schema-safe partial settings updates, custom-settings content types, feature-contributed settings, and validating settings without overwriting protected or unknown values.
---

# Pomi CLI Settings

Before issuing commands, read the [shared context, authentication, and output rules](../orchardcore-cli/references/shared-rules.md).
For first-time access or login problems, follow [authentication and contexts](../orchardcore-cli/references/authentication.md).
These rules apply even when this specialist is selected directly.

Site Settings are one tenant document with a safe management projection. Custom
Settings are named content-type-backed sections embedded in that document.
Typed module sections have explicit providers and their own permissions; they are
not arbitrary site properties or Custom Settings content types.

## Email and SMTP

Enable `OrchardCore.Email.Smtp`, refresh discovery, and use an application with
`AccessRemoteManagement` and `ManageEmailSettings`. Read `settings sections schema smtp`
and configure the tenant provider with `settings sections update smtp --body-file smtp.json`
over HTTPS. Include `isEnabled: true` when configuring a disabled provider. The
`email` section lists enabled provider names and selects `defaultProvider`; null
restores runtime fallback. The host-owned Default SMTP provider is separate.

SMTP password input is write-only. Omit it to retain the stored credential; use
`clearPassword: true` to remove it. Never combine clearing and setting. Keep the
input file private and do not place credentials in command arguments. Readback
contains `hasPassword`, not the secret or encrypted value. Invalid patches preserve
settings, and equivalent updates do not refresh options.

Use `pomi email test --body-file email-test.json` with `to`, `subject`, `body`, and
optionally `provider`. This sends one plain-text message through the existing email
service. Obtain authorization for external delivery; prefer tenant-local pickup
for automated validation. HTTP 204 means provider acceptance, not inbox delivery.
Do not automatically retry uncertain test requests: each attempt can send mail.
No `pomi login` is needed when the creation workflow already returned an
application-credentials context.

## User and authentication policies

Use typed `user-*` sections for tenant policies, with `AccessRemoteManagement`,
`ManageUsers`, and HTTPS. Discover the schema before changing values:

```sh
pomi settings sections schema user-registration
pomi settings sections update user-registration --body '{"usersMustValidateEmail":true,"usersAreModerated":true}'
pomi settings sections show user-registration
```

Sections cover login, registration, password reset, email changes, external login
and registration, MFA requirements, role-specific MFA, authenticator-app options,
and email/SMS verification templates. External authentication is enabled through
a provider feature; MFA services are enabled through an MFA method feature.
These dependency-only features cannot be enabled directly.

Omitted fields are preserved. Nullable strings can be cleared with null; Boolean
and integer fields cannot. Invalid patches preserve the old values. MFA recovery
counts must be positive and authenticator-app code length must be six. Enabled
role-specific MFA requires existing assignable roles. Liquid templates use the
existing editor validation. Scripts use the existing external-authentication
semantics and are not executed by the update request.

Read back and verify the affected workflow. For example, disabling password reset
makes the public ForgotPassword route unavailable. These commands configure
policies; they do not enroll users, disclose MFA keys/recovery codes, or perform
password recovery. Keep the application context returned by automated setup;
no interactive `pomi login` is needed. User-specific custom settings and provider
credentials are separate administration workflows.

## Rate limits

Enable `OrchardCore.RateLimits`, refresh discovery, and use an application with
`AccessRemoteManagement` and `ManageRateLimits`. Discover the four built-in
contracts with `pomi rate-limits limiter-types list`. These tenant policies are
separate from host-contributed rate limits configured in code.

Use `rate-limits policies create --body-file policy.json` with a unique `name`,
`scope` (`Global`, `Endpoint`, or `Group`), and the appropriate `path` or
`groupName`. Creation is disabled; identical name/definition retries return its ID.
Use `policies show <id>` and retain its `definition` for complete metadata updates.
Configure children with `policies limiters add <id> --body-file limiter.json`:
`{"id":"window","source":"FixedWindow","values":{"permitLimit":60,"windowSeconds":60,"queueLimit":0}}`.
Use stable limiter IDs; discover each source schema instead of guessing fields.

Enable with `policies enable <id>`. Disable before changing the policy target or
limiter settings; `policies update <id>` replaces metadata/target, and
`policies limiters update <id> <limiter-id>` replaces complete source settings.
Read back, re-enable, and verify HTTP 429 plus recovery after disabling on a narrow
test path. Never use a test prefix covering management/token endpoints. Active
metadata edits remain permitted. Identical retries are unchanged; 409 indicates
an active-policy edit or conflicting identity. Unknown limiter sources remain
opaque. Delete using `--force` only within the requested cleanup scope.

## URL rewrite rules

Enable `OrchardCore.UrlRewriting` and use an application with
`ManageUrlRewritingRules` to manage `url-rewriting rules`. These are ordered
resources, separate from the site settings document.

```bash
pomi url-rewriting rules sources
pomi url-rewriting rules list --take 200
pomi url-rewriting rules show redirect-about
pomi url-rewriting rules validate --body-file rule.json
pomi url-rewriting rules create --body-file rule.json
```

Use the built-in `Rewrite` or `Redirect` source. The complete definition includes
`name`, `source`, `pattern` and `substitutionPattern`, plus the source's options.
Use a stable explicit `id` when automating creation: identical retries return the
stored rule; reusing an ID for different data returns 409. Without an ID, each
creation generates a new rule. Display names need not be unique.

Read `definition` from `show` before `update <id>`. Updates replace the complete
definition and preserve source/order. Omitted options reset to case-sensitive
matching, `Append`, `Found` (Redirect) and false `skipFurtherRules` (Rewrite).
Query and redirect enums use names. Null does not clear a required field. Extension
sources without an API definition remain opaque; do not send arbitrary metadata.

Use `move <id> --body '{"position":0}'` to move a rule to the beginning of the
complete list. Verify persisted order and actual HTTP behavior, including status,
capture substitution and query-string handling. Rewrites retain the target
endpoint's authorization. The configured admin prefix is excluded from matching.
Delete with `delete <id> --force`; missing deletes and unchanged updates are no-ops.
Use narrow test paths so a rule does not unintentionally intercept management URLs.

## Site Settings

```bash
pomi settings show
pomi settings schema
pomi settings update --body-file settings.json
```

Example:

```json
{
  "siteName": "Contoso News",
  "pageSize": 25,
  "timeZoneId": "America/Los_Angeles"
}
```

Use `--body`, `--body-file`, or `--stdin`; the current CLI does not define a
`--json` option. Read the live schema because enabled features contribute
settings and the API intentionally omits unsafe server internals.

Read the current representation first, change only documented writable
properties, update, then read it back:

```bash
pomi settings show > current-settings.json
pomi settings update --body-file desired-settings.json
pomi settings show
```

## Typed module settings sections

Discover the enabled sections available to this identity, then read the selected
section's values, ownership and actual update schema:

```bash
pomi settings sections list
pomi settings sections show https
pomi settings sections schema https
pomi settings sections update https --body-file https-settings.json
```

Send only the fields to change, without a `values` wrapper. Omission preserves
existing values; arrays replace the supplied field. Use null only when the live
section schema explicitly permits reset/clear. Do not copy redacted or read-only
properties into an update. `source`, `isReadOnly`, `readOnlyReason`,
`redactedProperties` and `readOnlyProperties` describe what this API can manage.
An omitted secret is not evidence that no secret is configured.

The `https` section requires `OrchardCore.Https` and `ManageHttps`, plus remote
management access. Select a working HTTPS context before changing it. The API
preserves the admin editor's refusal to make HTTPS changes over HTTP; it does not
configure host certificates, listeners or proxy trust. An explicit redirect port
must be 1–65535. For example, this changes HSTS mode and restores automatic port
detection while retaining the current redirection flags:

```json
{
  "strictTransportSecurityMode": "Disabled",
  "sslPort": null
}
```

Use `Enabled` to enable HSTS or `FromConfiguration` to follow the environment
(enabled in Production). `requireHttps` controls redirects;
`requireHttpsPermanent` selects 308 rather than 307. Read back the result and
verify actual HTTPS/HTTP responses. The update reports `changed` and
`reloadRequested`; an equivalent retry should report both false. A tenant reload
rebuilds its pipeline and does not restart the host process.

Treat missing/disabled sections as unavailable. Do not fall back to writing
arbitrary settings JSON or modifying configuration owned by the host. The core
`settings schema` provider list is discovery-only and does not grant write access
to a module section.

### Security headers

Discover `security-headers` using `settings sections list` after enabling
`OrchardCore.Security`. Read the schema and existing values before updating. The
application needs `ManageSecurityHeadersSettings`; the section may be read-only
when host configuration owns it. A read-only update returns 409.

Use `settings sections update security-headers --stdin` with optional
`contentSecurityPolicy`, `permissionsPolicy` and `referrerPolicy` properties.
Supplied policy maps replace the whole map; omission preserves it and `{}` clears
it. Null maps are invalid. For CSP, null removes a directive except `sandbox` and
`upgrade-insecure-requests`, where it enables the flag. The existing Permissions
Policy editor's `()` sentinel removes a directive; it does not explicitly deny
that permission in the emitted header. Inspect the schema's supported referrer
policy names. The module always emits `X-Content-Type-Options: nosniff`.

Confirm readback and real response headers after changes. The policy applies to
admin and API responses too. Successful unchanged retries do not reload the tenant.
The admin editor and API share validation; do not bypass it with a recipe to work
around an invalid value.

### CORS policies

The `cors` section requires `OrchardCore.Cors` and `ManageCorsSettings` in addition
to remote-management access. Read its current policies and schema before updating:

```bash
pomi settings sections show cors
pomi settings sections schema cors
pomi settings sections update cors --body-file cors.json
```

```json
{
  "policies": [
    {
      "name": "Frontend",
      "allowedOrigins": ["https://frontend.example.com"],
      "allowedMethods": ["GET"],
      "allowedHeaders": ["Authorization"],
      "isDefaultPolicy": true
    }
  ]
}
```

A supplied `policies` array replaces the complete collection; preserve other
policies that should remain. Omission preserves it, `[]` removes all tenant
policies, and null is invalid. Each supplied policy is complete: omitted flags
are false and lists empty. Names must be unique and at most one policy can be
default; otherwise the first policy is used. Use HTTP(S) origins without paths
or trailing slashes and HTTP tokens for methods/headers. Do not combine any
origin (including literal `*`) with credentials.

Read back the stored tenant policy fields and verify actual preflight/simple
response headers from the expected browser origin. A changed policy reloads the
tenant; an equivalent retry does not. This section does not enumerate host-added
CORS options. CORS does not grant authentication or API permissions.

## Layer zones

With `OrchardCore.Layers` enabled, discover `layer-zones` through `settings sections`.
Read the existing list before replacing it; preserve zones unless removal is intended.

```bash
pomi settings sections show layer-zones
pomi settings sections schema layer-zones
pomi settings sections update layer-zones --body-file zones.json
pomi layers widgets zones
```

Use `{"zones":["Content","Footer"]}` to replace the list. Omission preserves it;
`[]` clears it; null is invalid. Names follow the admin editor's space/comma splitting,
with order, case and duplicates preserved. Equivalent retries do not save or reload.
Changing available zones neither creates theme sections nor moves/deletes existing
widgets. Verify the theme provides the corresponding sections before placing widgets.
Requires `ManageLayers` and remote-management access.

## Content culture picker

With `OrchardCore.ContentLocalization.ContentCulturePicker` enabled, discover the
`content-culture-picker` section. It requires `ManageContentCulturePicker` and
remote-management access; content-localization permission alone is insufficient.

```bash
pomi settings sections show content-culture-picker
pomi settings sections schema content-culture-picker
pomi settings sections update content-culture-picker --body-file picker.json
```

The Boolean fields are `setCookie` (cookie on language selection),
`redirectToHomepage` (fallback when the current item has no target-culture variant),
and `setCookieOnContentRequest` (cookie when visiting localized content). Omission
preserves values; null is invalid. Equivalent retries do not save, and changes do
not reload the tenant. Turning off cookie writing does not delete visitors' existing
cookies. Cookie lifetime remains host configuration. Verify actual redirect targets
and response cookies after changing these settings.

## Frontend search

Enable `OrchardCore.Search` and discover `frontend-search`. Reads and writes require
`ManageSearchSettings` and remote-management access; managing indexes alone is not
sufficient. Discover the administrative index name before selecting the default.

```bash
pomi indexes list
pomi settings sections show frontend-search
pomi settings sections schema frontend-search
pomi settings sections update frontend-search --body-file search-settings.json
```

```json
{
  "defaultIndexProfileName": "Articles",
  "pageTitle": "Search articles",
  "placeholder": "Search by keyword"
}
```

Use the administrative profile name, not its opaque ID or provider resource name.
New nonempty selections must exist in this tenant. Null/blank clears the default;
null clears either text override. Omitted fields preserve their values, including
an unchanged stale default while editing text. Unknown properties and non-string,
non-null values are rejected atomically. Equivalent retries do not save settings.

These updates share the existing admin editor's validation and assignment. They do
not enable a search provider, rebuild an index or grant public query permissions.
Verify the existing `/search` page with an identity authorized to query the index;
a successful settings update alone does not prove that public search is available.
For imports, generic Settings recipes retain their ordering behavior so an index
can be defined later in the recipe.

See the [frontend search contract](https://github.com/OrchardCMS/OrchardCore/blob/main/src/docs/reference/modules/Search/README.md#remote-frontend-search-settings).

## Custom Settings

Discover only sections the current identity is authorized to manage:

```bash
pomi custom-settings list --skip 0 --take 200
pomi custom-settings show BlogSettings
pomi custom-settings schema BlogSettings
pomi custom-settings update BlogSettings --body-file blog-settings.json
```

Construct the payload from the named section's schema. Unknown and unauthorized
names both return `404`; do not use the distinction to infer hidden resources.

## Design custom settings

1. Define a content type with
   `ContentTypeSettings.stereotype` set exactly to `CustomSettings`.
2. Attach focused reusable parts/fields.
3. Confirm the section appears in `custom-settings list`.
4. Read its dynamic schema.
5. Update the entire named section with valid nested values.

Use Custom Settings for tenant-wide editorial configuration such as branding
media, contact information, social links, feature flags, feed limits, or footer
content. Do not use them for collections of independently managed content.

## Localization

Discover `pomi localization --help` after enabling `OrchardCore.Localization`
and refreshing metadata. It exposes culture and settings management only.

```bash
pomi localization cultures list
pomi localization cultures available --take 200
pomi localization cultures add fr
pomi localization cultures remove de --force
pomi localization settings show --output json
pomi localization settings schema --operation update
pomi localization settings update --body-file cultures.json
```

`cultures list` returns enabled cultures; `cultures available` discovers names
that can be enabled (page with `--skip` and `--take`). Prefer `cultures add`
and `cultures remove` for individual changes; adding is idempotent. Removing
the default requires selecting a different default in settings first.

Culture settings **replace** the supported-culture list. Preserve existing
cultures unless removal is requested; the default must remain in the list.
Use names returned by culture discovery, not time zone identifiers. These
operations require `ManageCultures` and remote-management access.

There are no generic string, PO catalog, or database translation commands.
For Media UI labels, use `pomi media localizations show`. This reads the
server-resolved labels, not PO catalog entries. For translation editing, use
the Data Localization admin UI or its documented HTTP API; enabling that feature
does not add translation commands to Pomi.

## Safety

- Operate on one explicit context at a time.
- Do not submit secrets unless the owning module explicitly exposes a secure
  writable contract.
- Preserve settings not represented by the safe schema.
- Treat nested JSON type failures as validation errors; correct the exact path.
- Read back after updates and verify user-facing behavior separately.

Official references (live tenant schemas take precedence):
[Content culture picker settings](https://github.com/OrchardCMS/OrchardCore/blob/main/src/docs/reference/api/settings/README.md#content-culture-picker-section),
[Layer zone settings](https://github.com/OrchardCMS/OrchardCore/blob/main/src/docs/reference/api/settings/README.md#layer-zones-section),
[localization API](https://github.com/OrchardCMS/OrchardCore/blob/main/src/docs/reference/api/localization/README.md),
[settings API](https://github.com/OrchardCMS/OrchardCore/blob/main/src/docs/reference/api/settings/README.md),
[custom-settings API](https://github.com/OrchardCMS/OrchardCore/blob/main/src/docs/reference/api/custom-settings/README.md),
[CORS module](https://github.com/OrchardCMS/OrchardCore/blob/main/src/docs/reference/modules/Cors/README.md),
[URL Rewriting module](https://github.com/OrchardCMS/OrchardCore/blob/main/src/docs/reference/modules/UrlRewriting/README.md),
[Security module](https://github.com/OrchardCMS/OrchardCore/blob/main/src/docs/reference/modules/Security/README.md),
[Settings module](https://github.com/OrchardCMS/OrchardCore/blob/main/src/docs/reference/modules/Settings/README.md), and
[CustomSettings module](https://github.com/OrchardCMS/OrchardCore/blob/main/src/docs/reference/modules/CustomSettings/README.md).

## Robots and sitemaps

The `robots` section exposes `allowAllAgents`, `disallowAdmin`, and
`additionalRules` with `ManageSeoSettings`. Omission preserves values; null clears
additional rules. A physical robots.txt is reported as read-only configuration
ownership. Verify public `/robots.txt` after a change. `sitemaps-robots` controls
`includeSitemaps` when both SEO and Sitemaps are enabled; configure site `baseUrl`
for generated sitemap links.

Use `sitemaps list/show/create/update/enable/disable/delete` for regular maps and
indexes. Complete definitions contain name/path/kind/enabled/containedSitemapIds.
Indexes may contain distinct regular maps, never other indexes. Source commands
are `sitemaps sources types/schema/list/create/update/delete`; inspect the typed
schema before writing `type` plus complete `configuration`. Update/delete source
commands take sitemap ID followed by source ID. Built-in custom-path and content-type
sources are supported only when registered; other extensions expose identity.

Create generates an ID: read back after an uncertain result before retrying.
Updates/status changes and absent deletes can be retried without unnecessary
writes. Verify public XML, including indexes after changing child paths/status.
All sitemap operations require `ManageSitemaps` and remote access. Follow the
existing mutation confirmation policy; deletion commands require `--force`.

### Custom settings on a user

Enable `OrchardCore.Users.CustomUserSettings`, refresh the API, and run
`pomi users settings types`. Inspect `pomi users settings schema <name>` before
sending JSON to `pomi users settings update <userId> <name> --stdin`, then read
back with `pomi users settings show <userId> <name>`. Keep these values separate
from site-level custom settings. Use the existing user ID; creating a standalone
content item does not configure a user's settings. Updates require HTTPS, the
settings-type permission, and permission to edit that user. Passwords, roles,
and MFA enrollment data are outside this content envelope.

### OpenID tenant configuration

Use `settings sections schema/show/update openid-server`, `openid-client`, or
`openid-validation` for the feature-owned configuration. Existing OpenID services
validate updates; HTTPS and the corresponding ManageServerSettings,
ManageClientSettings, or ManageValidationSettings permission are required.
Changed settings reload the tenant. Preserve working token endpoints and flows
when automating through the current context. Client secrets and extra parameters
are write-only; use private JSON input, omit to retain, and send null to clear.
Read presence flags and verify readback/discovery after the reload. This does not
replace application/scopes commands or modify host-owned certificate material.
