# Remote management verification toolkit

See the [module/feature coverage inventory](../../design/remote-management/coverage-inventory.md)
and [prioritized delivery plan](../../design/remote-management/coverage-plan.md) for API and command gaps.

Run these scripts from a checkout of the feature branch. Python 3 and the
repository's .NET SDK are required. These tools create disposable loopback
tenants with synthetic credentials; do not point them at real tenant data.
The fixture root is an owner-only temporary directory. Keep its `fixture.json`,
recipe, database, logs, and credential files out of source control and reports.

## Repeat the functional checks

```bash
dotnet build src/OrchardCore.Cms.Web -m:1
dotnet build src/OrchardCore.Cli
python3 .scripts/remote-management/start-fixture.py
```

Keep that process running. It prints a path to `fixture.json` when ready.
In a second terminal, substitute that path below:

```bash
python3 .scripts/remote-management/verify-fixture.py <fixture.json>
python3 .scripts/remote-management/smoke-fixture.py <fixture.json>
python3 .scripts/remote-management/localization-smoke.py <fixture.json>
python3 .scripts/remote-management/api-revision-smoke.py <fixture.json>
python3 .scripts/remote-management/content-versions-smoke.py <fixture.json>
python3 .scripts/remote-management/content-validation-smoke.py <fixture.json>
python3 .scripts/remote-management/shared-content-smoke.py <fixture.json>
python3 .scripts/remote-management/tenant-install-smoke.py <fixture.json>
python3 .scripts/remote-management/graphql-smoke.py <fixture.json>
python3 .scripts/remote-management/catalog-smoke.py <fixture.json>
python3 .scripts/remote-management/openid-discovery-smoke.py <fixture.json>
python3 .scripts/remote-management/openid-scopes-smoke.py <fixture.json>
python3 .scripts/remote-management/openid-applications-smoke.py <fixture.json>
python3 .scripts/remote-management/home-route-smoke.py <fixture.json>
python3 .scripts/remote-management/layers-smoke.py <fixture.json>
python3 .scripts/remote-management/widgets-smoke.py <fixture.json>
python3 .scripts/remote-management/shortcodes-smoke.py <fixture.json>
python3 .scripts/remote-management/placements-smoke.py <fixture.json>
```

`verify-fixture.py` checks discovery, command/operation ID uniqueness, every
projected route with anonymous, no-management, and discovery-only identities,
existing-user authorization, and malformed folder input. Explicit exceptions
match APIs that resolve missing resources before per-resource authorization;
these are not counted as evidence that existing resources are protected.
The existing-user probes provide separate checks for that distinction.

`smoke-fixture.py` drives real CLI processes through content definition and
unpublished content creation, validation, readback, confirmation refusal,
least-privilege role creation, password environment input, account lifecycle,
and cleanup. Unique names allow reruns. Failure leaves only fixture resources
for diagnosis; discard the fixture afterward.

`content-versions-smoke.py` verifies all five dynamically discovered version commands,
newest-first paging, source preservation, restoration into new drafts, explicit
draft replacement, publication protection, purge confirmation, missing-version
retries, and anonymous denial. Set `OC_FIXTURE_BINARY` to an older native CLI
to verify server commands are available without rebuilding that CLI.

`content-validation-smoke.py` sends invalid part properties through validation
and save commands, checking HTTP 400, field paths, human/JSON error output,
failing exit codes, missing content types, and unchanged published content/history.
It also verifies valid partial-update validation still succeeds.

`shared-content-smoke.py` verifies nested menu identities and reordering, list
membership and ordering, nested Flow/Bag updates, taxonomy term hierarchies,
and setting/clearing publication and archive schedules through real CLI commands.
It isolates its CLI context and removes the synthetic items/types it creates.
It enables the required features on the disposable tenant and leaves them enabled.
These checks prove validation and persisted JSON round-trips; they do not prove
editor permission parity, rendered output, or execution of scheduled actions.

`tenant-install-smoke.py` verifies the dynamically discovered install command,
password environment/stdin input, human and JSON results, complete tenant URLs,
duplicate-name rejection, creation validation, setup failure/recovery, and
unchanged local contexts when provisioning is omitted. It also verifies opt-in
application provisioning, authorization denial, automatic context authentication,
and provisioning additional clients for existing tenants. It creates tenants only
inside the disposable host.

`localization-smoke.py` exercises six culture/settings CLI operations and the
dedicated Media label command. It also verifies that the five string-related
operations stay in OpenAPI without CLI metadata, and tests them through HTTP,
including group discovery, paging, translation retries, French-only editing
permissions, read-only access, and anonymous rejection. It
replaces culture settings on the disposable fixture with English and French.

`api-revision-smoke.py` checks revision headers and HEAD authorization, then
disables and re-enables Templates through a separate HTTP client. It verifies
that the CLI refreshes its still-fresh cache and discovers the restored command
without an explicit refresh. Run it with the current CLI build.

`graphql-smoke.py` verifies direct queries, full and single-type introspection,
variables/stdin, errors, and permissions against Orchard without refreshing
OpenAPI. It uses a fresh temporary CLI configuration, so earlier smoke tests
can safely populate the fixture's shared OpenAPI cache. `graphql-native-smoke.py <native-pomi-path>` additionally tests partial
results on HTTP 200/400/401, JSON and human output, redirects, and input handling
against an isolated loopback server; it runs in every opt-in native build.

`catalog-smoke.py` checks operation IDs, command paths and MCP tool names for
uniqueness with CLI/MCP enabled together, Tus disabled, Templates disabled,
features restored, and MCP enabled without the CLI feature. It restores the
explicit feature states it toggles; newly enabled dependencies can remain on
the disposable fixture. Its `catalog-smoke.json` records names and IDs without
credentials. It checks discovery, not every tool's execution or resource permissions.

`layers-smoke.py` checks layer definitions, normalized retries, invalid rules,
all HTTP permission gates, and MCP read/update equivalence using a restricted
Layers application principal. It composes a page/widget through existing content
APIs and verifies public output changes when the layer condition changes. It
also checks referenced deletion, unique catalogs, module disable/restore, and
MCP invocation with CLI disabled. It cleans up its items/types/layer and isolated
CLI context; feature and theme changes remain on the disposable fixture.

The wrapper supplies credentials only through the child process environment:

```bash
python3 .scripts/remote-management/pomi-fixture.py <fixture.json> --help
```

Set `OC_FIXTURE_BINARY` to the absolute native `pomi` path to test that executable.
Set `OC_FIXTURE_CONFIG_HOME` to isolate another evaluator's context/cache, or
`OC_FIXTURE_HUMAN=1` to suppress the wrapper's client credentials.

To verify human authentication, add a context using the wrapper, then run
`OC_FIXTURE_HUMAN=1 .../pomi-fixture.py <fixture.json> login --grant device`
(with `python3` before the script). Approve the code on the fixture's login
page. The fixture's generated administrator password is in its protected
state file; never copy it into reports, command arguments, or screenshots.
Then run:

```bash
python3 .scripts/remote-management/verify-login.py <fixture.json>
```

This verifies reuse from a new CLI process, expires only the fixture's cached
access-token timestamp, checks refresh rotation and Unix file modes, logs out,
and confirms that the revoked refresh token is rejected. It intentionally
ends that fixture login. Browser PKCE can be checked with `login --no-browser`
on the same computer; callback state/issuer/path/length and S256 verifier
behavior are additionally covered by CLI tests.

Stop the fixture with Ctrl+C in its original terminal. Its temporary directory
is retained for diagnosis; remove that one printed directory when finished.

## Unit tests, documentation, and native packaging

```bash
dotnet test --project test/OrchardCore.Cli.Tests
dotnet test --project test/OrchardCore.Tests -- --filter-class '*Management*' '*ContentApi*' '*Schema*' '*RemoteManagement*'
python3 -m mkdocs build --strict
dotnet publish src/OrchardCore.Cli -c Release -r osx-arm64 -o /tmp/pomi-native
python3 .scripts/remote-management/native-smoke.py /tmp/pomi-native osx-arm64
```

Install documentation dependencies from `src/docs/requirements.txt` first.
`native-smoke.py` runs the actual executable with isolated configuration,
checks help/doctor/completion, measures five `--version` processes, and packages
binaries with completions, a verification record, and SHA-256 checksums.
Budgets are 30 MiB for the executable and 2 seconds median process startup;
the latter is a regression guard, not a cold-machine performance guarantee.

The `Remote management CI and optional packages` workflow runs server and CLI
checks on pushes. PRs run the strict solution build, server, CLI, authentication,
MCP and functional tests through `PR - CI`. Regular pushes and PR updates do not
build, pack, upload or publish native binaries or NuGet packages.

To produce packages, manually dispatch `remote_cli.yml` on the desired branch
with `publish_packages` enabled. This opt-in runs the six-platform NativeAOT
builds and native smoke/install checks, creates downloadable artifacts retained
for 30 days, and publishes the complete package set to Feedz after checks pass.
Each platform summary links to its native archive and `pomi-tool-<rid>` packages,
with the built commit and common `4.0.0-cli.<run>` version (using the current
repository version prefix). Native-specific smoke checks run only in this
opt-in path; managed CLI tests continue to run in ordinary CI.

Download instructions and platform names are in the
[getting-started guide](../../src/docs/guides/remote-management/README.md#1-download-or-build-the-cli).
Runner labels follow the
[GitHub hosted-runner reference](https://docs.github.com/en/actions/reference/runners/github-hosted-runners).

### NativeAOT .NET tool packages

Tool packing is opt-in with `-p:PackAsTool=true`. Ordinary solution packing
excludes the CLI because its installer package must not be published before
all six native implementation packages exist. The .NET 10 SDK uses
`ToolPackageRuntimeIdentifiers` to create the installer package; packing for
one RID creates only that platform's NativeAOT implementation.

```bash
dotnet pack src/OrchardCore.Cli -c Release -p:PackAsTool=true -p:Version=1.0.0-ci.1.1 -o artifacts/tool
dotnet pack src/OrchardCore.Cli -c Release -p:PackAsTool=true -p:Version=1.0.0-ci.1.1 -r osx-arm64 -o artifacts/tool
python3 .scripts/remote-management/tool-smoke.py artifacts/tool osx-arm64 1.0.0-ci.1.1
```

Run the RID-specific command on the matching OS. Keep the package version
identical across all builds. If publishing to a feed later, publish all six
implementation packages first and the installer package last.

`tool-smoke.py` validates the package types, platform mappings, native entry
point, licenses, and absence of managed runtime files. It installs from a
local package source into a disposable tool directory (with NuGet.org available
for SDK launcher packages), runs help,
version, and diagnostics with `dotnet` absent from `PATH`, then uninstalls.
The check uses isolated CLI and NuGet configuration and does not alter the
developer's global tool installation. Every platform job runs this check
before uploading the packages.

## Agent plugin

The canonical skills live under `.agents/skills/pomi/skills/orchardcore-cli*`.
The parent `.agents/skills/pomi/` is a directly installable plugin. Repository
catalogs at `.agents/plugins/marketplace.json` and `.claude-plugin/marketplace.json`
resolve to that same directory, with marketplace name `orchardcore` and plugin
name `pomi`. No build, external symlink, or generated distribution branch is
needed for Git installation. `AGENTS.md` routes repository agents to the nested
skills; the other Orchard development skills remain separate.

Bump both checked-in manifest versions when changing plugin content; marketplace
clients use them to discover updates. Run the standalone installation check:

```bash
python3 .scripts/remote-management/test-marketplace.py
# Optional real Codex installation in a disposable CODEX_HOME:
python3 .scripts/remote-management/test-marketplace.py --codex /path/to/codex
```

Build a portable plugin directory and ZIP into a **new** output directory:

```bash
python3 .scripts/remote-management/build-plugin.py /tmp/pomi-plugin
```

The result contains `pomi-plugin.zip`, `pomi-skills.zip`, commit-specific ZIPs,
`package-metadata.json`, and `SHA256SUMS`. The plugin ZIP has a `pomi-plugin/`
marketplace root with Codex and Claude Code catalogs pointing to the same
`plugins/pomi/` directory. That plugin contains both manifests,
branding, the canonical skills/references, provenance metadata, and license.
The skills-only ZIP contains `pomi-skills/skills/` plus metadata and license.

The MkDocs hook in `docs-hook.py` invokes this same builder during `on_files`.
Read the Docs uses the hook through `mkdocs.yml`; local `mkdocs build` and
`mkdocs serve` require no separate generation command. Nothing is copied into
`src/docs` or maintained as a second source. `RawMarkdownFile` classifies raw
`.md` files as assets, deliberately preserving their extension and exact bytes.
Rendered copies are generated separately under `agents/skills/`.

The [Use Pomi with an agent](../../src/docs/agents/index.md) page provides the
version-local downloads and installation steps. Files are emitted below the
selected documentation version's `downloads/` and `agents/` directories.
The builder records package version (including a source-commit suffix), source commit, dirty state, compatibility,
and a content digest; ZIPs use fixed ordering, timestamps, permissions and stored
entries to avoid compressor differences. A dirty local build is labeled and
must not be published as a release artifact. Release-tag builds remain tied to
their commit; moving branch-version URLs do not preserve prior build downloads.
Configure the active release tags and latest branch in the Read the Docs project.

CI validates the website's actual files and uploads the downloads as a
commit-named artifact. To run the same checks locally:

```bash
python -m mkdocs build --strict
python .scripts/remote-management/test-skill-distribution.py site
```

Packaging does not install or register the plugin in the current agent.
Installation requires a separately installed Pomi binary and, for remote work,
an authorized tenant context. See `.agents/skills/pomi/compatibility.json` for the supported
Pomi/server baseline; update it when those requirements change.

The main skill is a workflow router. Detailed authentication, local installation,
and tenant setup live in its `references/` directory. Every specialist links
directly to shared context/authentication/output rules, so selecting a specialist
does not require loading the router first. Sibling skills and essential
references use relative Markdown links that work in both the repository and ZIP.
Longer manuals use commit-pinned GitHub links rather than unpackaged `src/docs`
paths; live tenant schemas remain authoritative. When revising an instruction
alongside an API change, update its manual link to a published commit containing
that documentation.

The builder checks local links and heading anchors before creating the ZIP.
To verify a relocated or extracted plugin without access to this repository:

```bash
python3 .scripts/remote-management/verify-plugin-links.py /path/to/extracted/pomi
```

The checker rejects missing files, links outside the plugin, missing headings,
and unversioned source-manual URLs. External checks validate URL structure;
they do not make HTTP requests or prove remote availability.

After changing CLI commands or these skills, refresh an isolated fixture context
and check the shell examples against the actual executable's cached help:

```bash
OC_FIXTURE_BINARY=/path/to/pomi python3 .scripts/remote-management/pomi-fixture.py <fixture.json> context add skill-audit <fixture-url> --current
OC_FIXTURE_BINARY=/path/to/pomi python3 .scripts/remote-management/pomi-fixture.py <fixture.json> api refresh --force
python3 .scripts/remote-management/skill-help-smoke.py /path/to/pomi <fixture-config-home>
```

Use the loopback fixture URL and its `OC_CONFIG_HOME`; never point this audit at
production. The checker reads all ten skills and their Markdown references,
walks cached help to validate shell example command paths and option names,
and reports source lines for failures. It executes only `--help`, removes
credential environment variables, and never runs sample mutations, shell
pipelines, or redirects. Run it again against the packaged plugin with
`--skills-root <plugin-directory>/skills` to verify the shipped instructions.

This does not validate inline prose, option values, positional argument counts,
JSON semantics, permissions, or entire workflows. Review those against source
and API documentation, and run relevant functional smoke tests. Update both
skill descriptions and body text when adding/removing capabilities; bump the
plugin version and rebuild the archive. Recheck blind evaluations when a
workflow changes materially; syntax checks do not measure agent efficiency.

See [evaluations.md](evaluations.md) for the reproducible blind-evaluation
protocol and observed results. Test evidence and limitations are also recorded
in `src/docs/reference/modules/RemoteManagement/review.md`.

## Custom asset policy regression checks

With the disposable fixture running, verify the Media upload policy:

```bash
python3 .scripts/remote-management/verify-media-assets.py /tmp/pomi-cli-fixture-.../fixture.json
```

This checks OpenAPI discovery and separate identities with Media access, restricted-extension access,
and own-media permission without a user-folder identifier. It covers CSS/JavaScript/SVG denial and upload,
ordinary image upload, unknown extensions, no overwrite, copy/move extension
checks, protected user-folder denial, public asset reads, and cleanup. It also verifies that all retired
static-file management routes return 404 while tenant static serving is enabled.
The smoke fixture exercises the custom asset lifecycle through `pomi` itself.
This is a local-store check; it does not claim Azure/S3 or multi-node coverage.

## Feedz publishing

The fork's `remote_cli.yml` publishes only when manually dispatched with
`publish_packages` enabled (default: disabled). Branch pushes, PR updates and
manual runs without that option run checks without package or native-binary
publication. Superseded regular checks are cancelled; explicitly requested
publication runs are allowed to finish. Publishing runs only in
`sebastienros/OrchardCore`, after the checks and all six native builds/tests/install
checks pass. A manual packaging run also publishes to Feedz; it is not an
artifact-only mode.

Only outputs under `src` are collected, excluding test/sample packages.
The solution pack includes libraries, modules, themes, targets, and
`OrchardCore.ProjectTemplates`. The CLI adds `OrchardCore.Cli` and six
`OrchardCore.Cli.<rid>` NativeAOT implementation packages. All packages retain
their original IDs, including bundled themes such as `TheAdmin` and
`TheBlogTheme`. Only the package version changes for this feed.
`prepare-feedz.py` accepts the OrchardCore packages and existing bundled theme
IDs, and rejects unexpected IDs, missing implementations,
inconsistent dependencies, or templates stamped with a different version.
Separately published translation packs retain the version pinned in
`Directory.Packages.props`; they are restored, not republished.

After publishing, `install-smoke.py <native-pomi>` tests the embedded CMS template
against the matching published packages: Auto Setup, a directory with spaces,
environment/stdin password input, missing SDK diagnostics, overwrite refusal,
failed setup, and foreground `--run` cancellation. It uses disposable local
sites and generated test credentials. Run it only with a CLI build whose
matching Orchard dependencies are already available. Pass its preview feed with
`--source` when needed.

The installer smoke also places its preview feed in a parent `NuGet.Config`,
installs without `--source`, and verifies both installation and a subsequent
ordinary restore inherit that feed. Its explicit `--clear-sources` case checks
that an inherited-only source is excluded while nuget.org and `--source` remain.

Versions use the repository's `VersionPrefix` plus
`-cli.<workflow-run-number>` (for example `4.0.0-cli.25`). Each explicitly requested publication uses its workflow run number. Rerunning the same workflow run keeps its version;
`--skip-duplicate` allows a partial publication to resume without replacing
immutable packages.

The publishing step alone receives `FEEDZ_IO_API_KEY` from GitHub secrets. Its
only destination is `https://f.feedz.io/sebastienros/orchardcore/nuget/index.json`;
the feed's service index also advertises its symbol endpoint. Symbol packages
are staged beside their main packages for NuGet's symbol publication. The
installer is published after all native implementations. The final verification
installs the Linux tool and project templates from Feedz and restores a generated
CMS project with an isolated package cache, then builds it. Use the **Published to Feedz** job
summary for the exact version and install commands.


## Confirmation flags

Run `python3 .scripts/remote-management/force-confirmation-smoke.py <pomi-path>`
to check that `--force` confirms destructive commands without prompting, that
API `force` values use `--api-force` independently, and that context deletion
uses the same flag. The test uses a synthetic loopback API and isolated context
storage; it runs in every opt-in native build.

`shortcodes-smoke.py` verifies restricted-principal CRUD, normalized retries, validation,
usage sanitization, rendered Liquid shortcodes and cache updates, denied HTTP/MCP access,
confirmation and schema discovery, unique catalogs and independent CLI/feature gates.

`placements-smoke.py` verifies rule CRUD, registered filters, normalized retries, denied
HTTP/MCP requests, database/file store isolation, rendered matching and last-match ordering,
empty-array deletion, unique catalogs and independent CLI/feature gates.

`widgets-smoke.py` verifies attaching existing widget content, configured-zone
validation, ordering, published/draft separation, restricted edit/publish roles,
MCP moves and feature transitions against real rendered pages. The disposable
fixture configures Content and Footer zones and dedicated widget clients.

## Typed HTTPS settings verification

Use an already trusted development certificate for the optional dual HTTP/HTTPS fixture:

```bash
OC_FIXTURE_HTTPS=1 python3 .scripts/remote-management/start-fixture.py
python3 .scripts/remote-management/https-settings-smoke.py <fixture.json>
```

The fixture checks existing certificate trust and exports only its public PEM certificate
for Python's trust bundle. Kestrel uses the existing development certificate; the fixture
does not export its private key or modify system trust. See the
[SDK certificate export modes](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-dev-certs#options).
The smoke suite verifies partial updates and null reset, no-op retries, module permissions,
refusal over HTTP, real 307/308 redirects and HSTS headers, feature removal/restoration
and MCP behavior with the CLI feature disabled. HTTPS certificate verification stays enabled.

## Website composition milestone

```bash
python3 .scripts/remote-management/website-composition-smoke.py <fixture.json>
```

This creates a setup-only Blank tenant and a managed Blog tenant through
`pomi tenants install`. The managed tenant uses its returned application context,
without fixture credential overrides or `pomi login`, to create content and a widget,
a conditional layer, a shape placement and a shortcode template. Public rendering
proves their combined behavior. Existing current-context selection remains unchanged;
setup-only installation creates no context. Administrator credentials are retained in
owner-only files under the disposable fixture's composition directory. The Blog recipe
supplies configured widget zones; the script does not claim zone-settings editing or
local application scaffolding coverage.

## Content localization

```bash
python3 .scripts/remote-management/content-localization-smoke.py <fixture.json>
```

This enables Content Localization and creates synthetic localized content. It verifies
new drafts and sequential retries, preserved source content, configured cultures,
published/latest variant filtering, restricted application permissions, MCP equivalence
and independent feature removal/restoration. The fixture provides separate localizer,
localizer-without-edit and published-content-reader identities. Synthetic content remains
in the disposable tenant for inspection.

## CORS settings

```bash
python3 .scripts/remote-management/cors-settings-smoke.py <fixture.json>
```

This enables CORS and HTTPS together to verify two providers behind the four shared
section operations. It checks complete policy replacement, omitted/empty/null behavior,
validation, no-op retries and real preflight/simple response headers for default/selected
policies, origins, methods, headers and credentials. Restricted CORS, HTTPS-only and
discovery-only applications verify section permissions. It checks feature lifecycle and
MCP with the CLI feature disabled, then leaves the disposable tenant's CORS policies empty.
The unit suite also verifies the existing admin editor and invalid recipe-import defenses.

## Security header settings

Run `python3 .scripts/remote-management/security-headers-settings-smoke.py <fixture.json>`
against a disposable fixture from `start-fixture.py`. It verifies policy replacement,
validation, retries, emitted response headers, source permissions through HTTP/CLI/MCP,
and feature lifecycle with another settings section enabled.

## URL rewriting

Run `python3 .scripts/remote-management/url-rewriting-smoke.py <fixture.json>` on a
disposable fixture. It checks stable-ID creation retries, complete replacement,
source validation, runtime ordering, redirect captures/status/query policies,
in-process rewriting, permission denials, feature lifecycle and MCP without CLI.
It creates uniquely named rules and removes them when successful.

## Layer zone settings

Run `python3 .scripts/remote-management/layer-zones-settings-smoke.py <fixture.json>`.
This checks shared admin normalization, complete list replacement, invalid values,
no-op retries, existing widget-service readback, module permissions through HTTP/CLI/MCP,
and independent feature lifecycle. It restores the disposable tenant's original zones.

## Content culture picker settings

Run `python3 .scripts/remote-management/culture-picker-settings-smoke.py <fixture.json>`.
This checks partial updates, retries, permissions and feature lifecycle through HTTP/Pomi/MCP,
plus real culture-cookie responses, localized redirects and homepage fallback. It creates a
synthetic localized homepage in the disposable tenant and restores the original picker settings.

`openid-discovery-smoke.py` verifies bounded application/scope discovery with separate
management principals, explicit response fields, missing resources, invalid paging,
HTTP/Pomi/MCP equivalence, unique catalogs, and MCP with CLI disabled. The test suite
also verifies that the OpenID Management feature controls endpoint registration
without requiring the authorization-server feature.

`home-route-smoke.py` exercises the existing `settings set-home-content` command
with a restricted `SetHomeRoute` application. It verifies denied access, missing
content, retries, public rendering, preserved unpublished drafts, unique discovery
and MCP operation with the CLI feature disabled. Synthetic content remains in the
disposable tenant. Culture-picker verification uses this same homepage command.

`openid-scopes-smoke.py` verifies scope mutations, semantic retries, conflicts,
invalid/denied writes, full field replacement, HTTP/Pomi/MCP readback and MCP
with CLI disabled. It deletes the synthetic scope after checking repeated deletion.


`openid-applications-smoke.py` checks application CRUD, private secret inputs,
configuration replacement, permission enforcement and actual client authentication.
It also checks equivalent retries and MCP with the CLI feature disabled.

Run `python3 .scripts/remote-management/openid-credentials-smoke.py <fixture.json>`
to verify private one-time Pomi output, rejected destinations before mutation,
rotation/revocation and actual rejection of retired secrets. It exercises HTTP,
Pomi and MCP, preserves application settings, checks restricted principals and MCP
with CLI disabled, and removes its synthetic application on completion.

Run `python3 .scripts/remote-management/feature-profiles-smoke.py <fixture.json>`
to check Default-tenant profile CRUD, schema discovery, retries, permission boundaries
and feature lifecycle through HTTP, Pomi and MCP. The fixture has distinct profile
and tenant-management clients; MCP is also exercised with the CLI feature disabled.
The probe removes its synthetic profile on completion. Run
`python3 .scripts/remote-management/feature-profiles-runtime-smoke.py <fixture.json>`
for child provisioning without login, private administrator handoff, assignment,
rule-order/dependency enforcement and fresh-request definition changes. It removes
the synthetic assignment/profile and retains the disposable child in the fixture.

### Deployment plans

Run `python3 .scripts/remote-management/deployment-plans-smoke.py <fixture.json>`
against the isolated loopback fixture. This checks generated Pomi plan/step
commands, explicit schemas, retries, invalid patch preservation, HTTP permissions,
MCP creation/ordering and cleanup. It enables Deployment and MCP in the fixture
and removes its temporary plan on completion.

### SMTP administration

Run `smtp-smoke.py <fixture.json>` against a fixture started with
`OC_FIXTURE_HTTPS=1`. It validates typed settings, secret redaction and retries,
permission gates, MCP readback, and real tenant-local pickup messages in two
tenants. It never sends external mail. Child tenants are stopped after checks.
