# Pomi CLI

Pomi (`pomi`) creates local Orchard Core sites and manages tenants through their APIs. Discover
commands, authenticate interactively, and manage content, media, themes,
templates, and tenant settings from your terminal.

## Install

Official releases publish `OrchardCore.Cli` and its six native implementations
through the Orchard Core release workflow to NuGet.org:

```sh
dotnet tool install --global OrchardCore.Cli --version <release-version>
pomi --version
pomi
```

Installation requires the .NET 10 SDK or later. Running the installed native
executable needs no separate .NET runtime. Linux, Windows, and macOS are
supported on x64 and Arm64. Use `dotnet tool update` to upgrade.

Standalone archives and SHA-256 checksums are attached to
[official releases](https://github.com/OrchardCMS/OrchardCore/releases).
Main CI also builds and verifies all six platforms and retains `pomi-<rid>`
and `pomi-tool-<rid>` artifacts for 30 days. Those artifacts validate development
changes; native tools are published to the package registry only for releases.
For a downloaded tool artifact, keep both `.nupkg` files together and install
with `--add-source ./pomi-packages --version <artifact-version>`.

## Get started

Create and initialize a local CMS with the template embedded in this CLI build:

```sh
pomi install ./MyOrchardSite --site-name "My Orchard Site" --email admin@example.com --run
```

Enter the administrator password at the masked prompt. This uses SQLite and
the SaaS setup recipe, then starts the site at `https://localhost:5001`.
For local HTTPS, prepare the certificate with `dotnet dev-certs https --trust`.
Multiple listen addresses use a quoted list, for example
`--urls "https://localhost:5001;http://localhost:5000"`.
Omit `--run` to stop after setup. Local installation requires the matching
.NET SDK (currently .NET 10); `pomi doctor` reports whether it is available.
The template is embedded; restoring the site's dependencies still requires
NuGet access or a populated package cache. Nuget.org is added by default, and
parent/user NuGet sources remain available. For official preview dependencies,
configure Cloudsmith in an inherited NuGet configuration or add
`--source https://nuget.cloudsmith.io/orchardcore/preview/v3/index.json`.
Use `--clear-sources` to ignore inherited package sources and retain only
nuget.org plus any explicit `--source`.
`--site-time-zone` takes an IANA/TZDB ID such as `Europe/Paris` or
`America/Los_Angeles` (default `UTC`). Run `pomi install --help` for database,
recipe, URL, and secret-input options.

Follow the [remote management guide](https://github.com/OrchardCMS/OrchardCore/blob/main/src/docs/guides/remote-management/README.md)
to prepare a tenant and authenticate. Run `pomi --help` to discover commands
available in the local cache, or `pomi doctor` to inspect local configuration.

The default `--output auto` uses human-readable messages in a terminal and JSON
when redirected to a pipe or file. In a terminal, changes report completion and
useful details with full setup URLs, and lists remain tables. Use `--output json`
explicitly in scripts, or `--output human` to keep readable messages when
redirecting. Other formats are `table`, `csv`, `tsv`, `yaml`, `toml`, and `none`.

For device authorization across separate processes, use `pomi login device start`,
`pomi login device show <session-id>`, and `pomi login device wait <session-id>`.
Each returns one JSON result with `--output json`; `start` and `show` support
opt-in `--qr always` PNG output. See the
[device login guide](https://github.com/OrchardCMS/OrchardCore/blob/main/src/docs/guides/remote-management/README.md#start-and-complete-device-login-separately).

## GraphQL

With GraphQL enabled on the selected tenant:

```sh
pomi graphql execute --query '{ __typename }'
pomi graphql execute --file query.graphql --variables-file variables.json
pomi graphql schema --output json > graphql-schema.json
```

These built-in GraphQL commands call GraphQL directly, reuse the current context/login,
and need no OpenAPI refresh. GraphQL errors return exit code 4 while preserving
partial data and errors in the JSON response. Human output shows readable errors
on stderr and any partial data on stdout. See the
[GraphQL CLI reference](https://github.com/OrchardCMS/OrchardCore/blob/main/src/docs/reference/modules/Apis.GraphQL/README.md#use-graphql-from-the-cli)
for permissions, stdin, operation names, custom endpoint paths, and mutations.

## Existing installations

The NuGet package ID remains `OrchardCore.Cli`; updating it installs the `pomi`
command. Existing contexts, cached metadata, credentials, and `OC_*` environment
variables continue to work. Update scripts to invoke `pomi` and regenerate shell
completion with `pomi completion --shell <shell>`. The executable is `pomi` on
macOS/Linux and `pomi.exe` on Windows.
