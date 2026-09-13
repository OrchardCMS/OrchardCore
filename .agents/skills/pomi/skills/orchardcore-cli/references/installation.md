# Install a local CMS

Install the executable first using [CLI installation](cli-installation.md) if
`pomi` is unavailable. Apply the [shared operating rules](shared-rules.md). For a new tenant in an
existing application, use [tenant installation](tenants.md) instead.

Use `pomi install <directory>` to create a new standalone application **or initial
SaaS host** and initialize its Default tenant. This static command needs no tenant context or
login. It embeds this CLI build's `occms` template and uses matching Orchard
dependency versions; no template download or version selection is needed.
Run `pomi doctor` to check for the required stable .NET SDK (currently .NET 10).
Dependencies still need a NuGet feed or populated package cache. Nuget.org is
added by default; parent and user-level NuGet sources remain available. For
official preview dependencies, use an inherited Cloudsmith configuration or
pass `--source https://nuget.cloudsmith.io/orchardcore/preview/v3/index.json`.
Use `--clear-sources` only when the user wants to exclude inherited package
sources; it leaves nuget.org and an explicit `--source`.

## Choose the site to create

Use the same installer for these two new-host workflows. The password variable
in these examples must already be populated securely and saved for the user as
described below. For automated site building, use `--enable-remote-management`:
it enables CLI/OpenID dependencies even with Blank, registers an administrative
application, and saves a context with client credentials. Omit the option for
setup-only work without additional remote management; the recipe remains authoritative.
Check live help for the flag and use matching CLI/server packages. An older build
without provisioning support is not a reason to silently switch to interactive login.

For a new site without a requested recipe, prefer SQLite and Blank:

```bash
pomi install ./MySite \
  --recipe-name Blank --database-provider Sqlite \
  --site-name "My Site" --user-name admin --email admin@example.com \
  --password-env OC_SITE_PASSWORD --enable-remote-management --output json > ./MySite.install.json
```

For a new SaaS host, create and initialize its **Default tenant** with SaaS:

```bash
pomi install ./MySaaS \
  --recipe-name SaaS --database-provider Sqlite \
  --site-name "My SaaS" --user-name admin --email admin@example.com \
  --password-env OC_SITE_PASSWORD --enable-remote-management --output json > ./MySaaS.install.json
```

SaaS is the setup recipe, not a different .NET project template. The SaaS
recipe enables tenant management; it does not create the requested child tenants.
After the install command succeeds, use its `context` and `url` fields. Pomi
chooses an unused context name and preserves an existing current context. The
JSON result contains no application secret. Start the host using the process
lifecycle guidance below before issuing remote commands:

```bash
SITE_URL="$(python3 -c 'import json; print(json.load(open("MySaaS.install.json"))["url"])')"
CONTEXT="$(python3 -c 'import json; print(json.load(open("MySaaS.install.json"))["context"])')"
pomi context list --output json
pomi --context "$CONTEXT" api refresh --output json
pomi --context "$CONTEXT" tenants install --help
```

Use `MySite.install.json` for the standalone example. Require a nonempty returned
context and verify its tenant URL; do not guess a name or fall back to the current
context. `api refresh` obtains an application token automatically. Do not add
another context or run `pomi login` or device authorization. Update the private
administrator credential record with the actual URL and context name.
A child tenant can use Blank or Blog independently of the host's SaaS recipe.
Do not attempt `pomi tenants install` before a host exists.

Let Pomi instantiate its embedded template, build the project, and perform Auto
Setup. Do not write the initial `.csproj` or Auto Setup settings yourself, run
`dotnet new occms`, or recreate these steps with shell/HTTP scripts unless the
user explicitly requests manual scaffolding. If prerequisites or feeds fail,
resolve the reported blocker using this guide and the CLI installation reference;
do not silently switch to a manual installation.

## Credentials and options

The password must meet the [setup password policy](setup-password.md). That
reference includes a cryptographic generator guaranteed to meet the standard
policy. The password environment variable must already be provided by the user,
secret manager, or an authorized generation step. Save administrator
credentials in a private file accessible to the user before setup, following
[credential handoff](setup-password.md#credential-handoff). The masked interactive prompt, `--password-file`, and
`--password-stdin` are alternatives. Connection strings use the analogous
`--connection-string-*` options. `--connection-string-env` takes a variable
name, not the secret value. There is no inline `--connection-string` option.
See the [safe connection-string examples](tenants.md#schema-properties-and-secret-cli-options)
for the three input forms; local installation uses the same options. Only one
secret may consume stdin.

If no database provider is specified, recommend SQLite and use the default
`Sqlite`. For another requested provider, recommend a unique `--table-prefix`
following the [database choice rules](shared-rules.md#database-choice-for-new-sites-and-tenants).
Do not replace the user's chosen provider. The executable defaults to the SaaS
recipe, administrator `admin`, and UTC, but **pass the recipe explicitly**:
prefer Blank for an unspecified new site, SaaS for a SaaS host, and Blog for a
requested blog. A blog-like directory or site name
does not select the Blog recipe. Consult
`pomi install --help` for other database, recipe, and URL options. `--source`
adds a dependency feed alongside nuget.org. Only an explicitly requested project
template other than `occms` needs direct template tooling. SaaS, Blank, and Blog
are all supported recipes within `pomi install`. Time zones use IANA/TZDB IDs such as `Europe/Paris` and
`America/Los_Angeles`, or `UTC`.

By default Pomi chooses and reserves an available random HTTPS localhost port.
Read `url` and `listenUrl` from the installation result; do not assume port 5001
or guess a new port after setup. Pomi persists these addresses in the generated
application settings and project launch profiles. Explicit `--urls` ports are
checked and a busy/unavailable address fails before setup. HTTPS needs a certificate.
Use `dotnet dev-certs https --trust` for local development, or explicitly choose
HTTP with `--urls http://localhost:5000`. Multiple addresses use one quoted
semicolon-separated argument: `--urls "https://localhost:5001;http://localhost:5000"`.

## Start and connect

Without `--run`, installation stops its temporary setup host after completion.
The JSON `tenantState` describes persisted tenant initialization, not a running
server process.
Add `--run` only when the user wants the site left running in the foreground;
Ctrl+C stops it. For an agent-managed development server, install without
`--run`, then use the environment's supported persistent process/session mechanism
with `dotnet run --project ./MySaaS --no-launch-profile`
(replace the path as needed). This uses the selected URLs saved by the installer. Starting an already installed project with
`dotnet run` is separate from scaffolding it. Do not treat a foreground server
waiting for requests as a failed installation. Existing nonempty destinations are refused. On failure, inspect
the preserved project and output before deciding how to proceed; do not blindly
retry setup against a partly initialized database. Administrator passwords are
not persisted in configuration, but Orchard persists database connection settings.
With `--enable-remote-management`, Pomi requires confirmation of application
provisioning before saving the context. If provisioning fails after setup, inspect
the installed site's state and fix its feature profile or configuration. Follow
[management after setup](tenants.md#enable-management-after-setup) for existing
tenants; do not recreate or reset the installed site. Without the flag, recipe
behavior is unchanged and no automatic context is created.

After installation, see [tenant management and Remote Management setup](tenants.md)
and the official [Remote Management reference](https://github.com/OrchardCMS/OrchardCore/blob/main/src/docs/reference/modules/RemoteManagement/README.md).

## Official distribution

Install released Pomi tools from NuGet.org with `dotnet tool install --global
OrchardCore.Cli --version <release-version>` (as one command), or use the standalone
archive from the official Orchard Core release. Main CI's six-platform artifacts
are validation builds, not published preview tools. Their embedded template needs
server packages with the same artifact version. Do not assume a native validation
artifact can restore its dependencies from Cloudsmith.

Libraries, modules, themes, and project templates from main use Orchard Core's
existing Cloudsmith preview publication workflow. Tagged releases publish the
complete versioned package set, including native tools, to NuGet.org.
