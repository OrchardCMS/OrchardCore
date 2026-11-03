# Remote Management (`OrchardCore.RemoteManagement`)

<img src="../../branding/assets/logo/pomi/svg/pomi-terminal-logo-light.svg#only-light" alt="Pomi — Orchard Core command-line interface" width="160" />
<img src="../../branding/assets/logo/pomi/svg/pomi-terminal-logo-dark.svg#only-dark" alt="Pomi — Orchard Core command-line interface" width="160" />

The Remote Management module exposes a versioned management protocol and tenant-specific OpenAPI document for the Orchard Core command-line interface (`pomi`). Enabled Orchard Core features contribute resource commands and JSON Schemas, so the commands available for one tenant may differ from another tenant.

The CLI uses OpenAPI as its management protocol. GraphQL remains available for application queries but is not required by `pomi`.

Start with the illustrated [first-tenant walkthrough](../../../guides/remote-management/README.md).

## API reference

- [Management API overview](../../api/README.md)
- [Discovery and manifest](../../api/discovery/README.md)
- [Authentication](../../api/authentication/README.md)

Before exposing a tenant, read [Security and hardening](#security-and-hardening).

## Enable and configure

The shared **Remote Management** feature (`OrchardCore.RemoteManagement`) enables
OpenAPI, permissions, discovery, and OpenID authentication services. It does not
register client applications. Choose the client feature for your workflow:

| Feature | Responsibility |
| --- | --- |
| `OrchardCore.RemoteManagement` | Shared server configuration, validation, and management scope |
| `OrchardCore.RemoteManagement.Cli` | Pomi discovery metadata, CLI application, and device authorization |
| `OrchardCore.RemoteManagement.Mcp` | Tenant MCP endpoint and MCP application registration |

For Pomi, enable **Remote Management CLI** under **Configuration → Features**.
Open **Settings → Remote Management**, then **Configure Pomi CLI**. The CLI action
configures shared authentication and the `orchardcore-cli` public native application,
its loopback callback, and device authorization. Existing applications and unrelated
OpenID settings are preserved.

The shared **Configure Remote Management** action checks and repairs the authorization
code flow, refresh tokens, client credentials, local token validation, and the
`orchardcore.management` scope. It creates neither a CLI nor an MCP application.
The Pomi and MCP applications require PKCE individually, so the tenant's other authorization
code clients keep working. A tenant validating tokens from an external identity provider or
another tenant can't be configured until its OpenID validation settings are changed, because
that would break the clients using those tokens.
Each client action configures these shared requirements as well, so it can be used
on a freshly enabled tenant.

These actions rewrite the OpenID server settings, so they require **View and edit the OpenID Connect
server settings** (`ManageServerSettings`) in addition to `ManageRemoteManagementConfiguration`. An endpoint path is only set when it is
empty, and any path counts as configured: clients discover the endpoints from the server's metadata,
so a customized path keeps working.

For automated deployments, the **Orchard Core Remote Management** recipe configures
only shared authentication. Use **Orchard Core Remote Management CLI**
(`RemoteManagementCli`) for Pomi; its `RemoteManagementCliConfiguration` step creates
or repairs the CLI application. Both resolve the current tenant automatically.

The public bootstrap document is available at `/.well-known/orchardcore-management`. It contains only the protocol version, authentication authority, client ID, and supported grants. The authenticated `/api/management/manifest` endpoint additionally returns tenant identity, compatibility ranges, capabilities, OpenAPI coordinates, and documentation index information.

Access to the authenticated manifest and management APIs requires the **Access remote management API** permission. Each contributed operation also enforces its own Orchard Core permission. `GET /api/management/me` (`pomi management whoami`) returns the tenant, the user or client, the scopes and the effective permissions of the caller.

## Install the CLI

The `pomi` project can be run as a framework-dependent application during development:

```bash
dotnet run --project src/OrchardCore.Cli -- --help
```

Publish a self-contained executable for the current platform:

```bash
dotnet publish src/OrchardCore.Cli -c Release -r <runtime-identifier>
```

For example, use `osx-arm64`, `osx-x64`, `linux-arm64`, `linux-x64`, `win-arm64`, or `win-x64`.

## Contexts and login

A context identifies one exact tenant URL. Names are shared across sessions,
not scoped to a project directory. After installing a site or tenant, run
`pomi context list --output json` and choose a site-specific name absent from
`contexts[].name` (case-insensitive). Append a suffix if necessary; do not
assume `default` is available or required for the `Default` tenant. `context add`
can update existing records, so use a fresh name for a newly initialized site,
even when it reuses an old URL. Recheck names before saving if other work intervened.

Set `CONTEXT` to that unused name and `SITE_URL` to the exact tenant URL,
including any path prefix:

```bash
pomi context add "$CONTEXT" "$SITE_URL" --current
pomi --context "$CONTEXT" login
```

Browser login uses OAuth authorization code with PKCE and a temporary loopback listener. Credentials are renewed silently with refresh tokens stored in Windows Credential Manager on Windows. On macOS, Linux, and other Unix-like systems, tokens are stored as plaintext owner-only files under `~/.orchardcore/credentials`.

The Unix token directory uses mode `0700` and token files use mode `0600`.
These permissions prevent access by other operating-system users but do not
encrypt tokens at rest. Protect the user account and home directory as you
would for other development credentials.

The CLI doesn't use macOS Keychain or Linux Secret Service, which avoids an
operating-system credential prompt. A stored token is bound to its tenant URL,
authority, and client ID.

For a terminal without a browser, use device authorization:

```bash
pomi login --grant device
```

For multiple tenants, run one device flow per named context and approve each
code as the intended tenant-local user:

```bash
pomi --context news login --grant device
pomi --context marketing login --grant device
```

Tokens are stored separately per context and must not be copied between
tenants. Browser credentials should be entered only in the tenant's HTTPS login
page or by a trusted password manager; never put passwords in command
arguments, logs, screenshots, chat, or browser automation scripts.

For unattended jobs, follow [Client credentials for automation](#client-credentials-for-automation). Implicit and password grants are not supported. The CLI uses OAuth access/refresh tokens and does not use or persist ID-token claims as an identity assertion.

Manage multiple tenants with named contexts:

```bash
pomi context list
pomi context use staging
pomi --context production content items list
pomi logout production
```

Delete one context with `pomi context delete <name> --force`. To delete every saved context and its stored credentials, use `pomi context clear`; confirm the interactive prompt, or pass `--force` for non-interactive use:

```bash
pomi context clear --force
```

A context represents and authenticates to exactly one tenant. From a context for the Default tenant, an administrator can prepare another running tenant for remote management:

```bash
pomi tenants enable-remote-management Site1
```

The command enables Remote Management and its dependencies, configures the tenant's OpenID server and CLI application, and grants **Access remote management API** to the tenant's Administrator role. Its output includes the tenant URL. Register that URL as a separate context and authenticate directly as a user of the tenant:

```bash
pomi context add site1 https://cms.example.com/site1 --current
pomi login
pomi content items list
```

Direct authentication ensures tenant-local roles and permissions are enforced and newly created content is associated with the authenticated tenant user. Each context stores separate credentials. Discovery caches are keyed by tenant URL, so aliases for the same URL share metadata; `--help` reflects that tenant's enabled features.

## Provision application credentials during installation

`pomi install` and `pomi tenants install` accept `--enable-remote-management` to
opt into enabling the CLI/OpenID features and saving a new context with dedicated
administrative application credentials. No interactive login is needed. Without
the flag, installation follows the recipe and creates the administrator account
as usual. Existing running tenants can use
`pomi tenants enable-remote-management <tenant> --provision-client`.

See [optional unattended remote management](../../../guides/remote-management/README.md#optional-unattended-remote-management)
for examples, credential storage, and revocation. The administrator user and its
password are independent of the application credentials.

## Client credentials for automation

Use client credentials when a CI job, scheduled script, or service needs to run
`pomi` without a person opening a browser. The application authenticates as itself;
its tenant-local roles determine what it can do. There is no user consent page
or administrator password in this flow.

This walkthrough creates an application named `orchard-automation` for the tenant
at `https://cms.example.com/tenant-a/`. Replace that URL with your exact tenant
URL, including its path prefix.

### 1. Prepare the tenant and application role

In the target tenant's admin UI, enable and [configure Remote Management](#enable-and-configure).
This enables the token endpoint and client-credentials grant, creates the
`orchardcore.management` scope, and configures local token validation. It does
**not** create a confidential automation application. Keep the public
`orchardcore-cli` application for browser and device login.

Open **Access Control → Roles**, create a role named `Automation`, and grant
**Access remote management API** (`AccessRemoteManagement`). Add the permissions
required by your intended commands. For the `pomi features list` example below,
also grant **Manage Features** (`ManageFeatures`). That permission also permits
feature changes; it is not a read-only permission. Save the role.

The scope permits management API access, while role permissions authorize the
individual operations. Assigning the scope alone is not sufficient. The
**Roles** feature must be enabled to create and assign application roles.

In the legacy admin navigation, **Roles** is under **Security**.

### 2. Register a confidential application

Open **Access Control → OpenID Connect → Applications** and create an application.
With legacy navigation, use **Security → OpenID Connect → Management → Applications**.

| Field | Value |
| --- | --- |
| Display Name | `Orchard automation` |
| Application type | **Web application**; this is the application registration type even when the caller is `pomi` |
| Client type | **Confidential client** |
| Client Id | `orchard-automation` |
| Client Secret | Generate a secret using the button beside the field and store it in your secret manager |
| Flows | Select **Allow Client Credentials Flow**; leave other flows unchecked for this application |
| Allowed Scopes | Select `orchardcore.management` |
| Client Credentials Roles | Select `Automation` |

Save the application. This flow does not require redirect URIs, a browser
callback, or refresh tokens. If the client-credentials checkbox or management
scope is missing, complete step 1 in this same tenant first.

### 3. Supply credentials and register the context

Configure these environment variables for the process running `pomi`:

| Variable | Value |
| --- | --- |
| `OC_CLIENT_ID` | `orchard-automation` |
| `OC_CLIENT_SECRET` | The secret saved in step 2 |
| `OC_CONTEXT` | The name of the context these credentials belong to |

In CI, inject the secret from the CI system's secret store. For a local test in
Bash or Zsh, this prompt reads it without displaying it or putting its literal
value in shell history:

```bash
export OC_CONTEXT=production-automation
export OC_CLIENT_ID=orchard-automation
printf 'Client secret: '
IFS= read -r -s OC_CLIENT_SECRET
printf '\n'
export OC_CLIENT_SECRET
```

Add a context for the exact tenant URL:

```bash
pomi context add production-automation https://cms.example.com/tenant-a/ --current
```

A context stores the tenant address and discovery metadata, not your automation
secret. Setting these variables before discovery also lets the CLI fetch the
authenticated manifest and tenant-specific commands without browser login.

These credentials are only valid in one tenant, so Pomi limits where it sends them:

| Situation | Credentials sent to the token endpoint |
| --- | --- |
| `OC_CONTEXT` names the context | `OC_CLIENT_ID` and `OC_CLIENT_SECRET`, instead of any stored login of the context |
| `OC_CONTEXT` names another context | The stored login of the context, if any |
| `OC_CONTEXT` is not set and the context has a stored login | The stored login |
| `OC_CONTEXT` is not set, the context has no stored login, and the command names it, with `--context` or a context argument | `OC_CLIENT_ID` and `OC_CLIENT_SECRET` |
| `OC_CONTEXT` is not set, the context has no stored login, and it is the only saved context | `OC_CLIENT_ID` and `OC_CLIENT_SECRET` |
| `OC_CONTEXT` is not set, the context has no stored login, and it is the current one among several | None |

When it doesn't send them, Pomi writes a warning to standard error that gives the reason. Set
`OC_CONTEXT` whenever these variables are present in an environment that can reach several
tenants. It only limits the variables: use `--context` or `pomi context use` to select the context
of a command. Variables supplied without `OC_CONTEXT` are still sent to a context named by a
command, so supply them only to the processes that manage their tenant.

### 4. Verify authentication and run a command

An optional login check verifies that the server accepts the application:

```bash
pomi --context production-automation login --grant client-credentials \
  --client-id orchard-automation \
  --client-secret-env OC_CLIENT_SECRET
```

Success reports the context, grant type, issuer, and token expiry without
printing the access token. This checks authentication; it does not prove that
the application has permission to perform every management operation.

**Client-credentials login does not establish a saved session.** Neither the
secret nor the token is persisted. Keep `OC_CLIENT_ID` and `OC_CLIENT_SECRET`
available for subsequent commands, which obtain tokens automatically. A
preceding `pomi login` is optional:

```bash
pomi --context production-automation features list --output json
```

The environment credentials take precedence over saved browser/device
credentials. Use `--output json` for scripts that parse results; the default
`--output auto` uses human output in a terminal and JSON when redirected.

For the login check, `--client-secret-env` may name a different variable, or
`--client-secret-stdin` may read the secret from standard input. For example,
with a protected file provided by your secret manager:

```bash
pomi --context production-automation login --grant client-credentials \
  --client-id orchard-automation \
  --client-secret-stdin < /path/to/client-secret.txt
```

These explicit secret options are also available on `pomi api invoke`. They apply
to that invocation only; dynamic resource commands use `OC_CLIENT_ID` and
`OC_CLIENT_SECRET` as described in step 3.

When finished with the local test, clear the variables:

```bash
unset OC_CONTEXT OC_CLIENT_ID OC_CLIENT_SECRET
```

`pomi logout` removes saved human credentials; it does not disable an automation
application or clear environment variables. Change the application's secret in
the admin UI and update your secret store when rotating credentials. Already
issued access tokens may remain valid until expiry.

### Provisioning during tenant setup

`pomi tenants setup` does not currently accept a client ID or client secret.
`pomi tenants enable-remote-management` configures the server and public CLI
application but does not provision a confidential client.

For repeatable provisioning, a custom setup recipe can enable the required
features, run `RemoteManagementConfiguration`, create the application role, and
include this [OpenID application recipe step](../OpenId/README.md#openid-connect-client-integration-configuration):

```json
{
  "name": "OpenIdApplication",
  "ClientId": "orchard-automation",
  "DisplayName": "Orchard automation",
  "Type": "Confidential",
  "ApplicationType": "web",
  "ClientSecret": "[js: configuration('Automation:ClientSecret')]",
  "AllowClientCredentialsFlow": true,
  "ScopeEntries": [{ "Name": "orchardcore.management" }],
  "RoleEntries": [{ "Name": "Automation" }]
}
```

Create the `Automation` role with the required permissions before this step.
Provide `Automation:ClientSecret` through the target tenant's
[configuration](../Configuration/README.md) using your deployment's secret
configuration provider. The recipe's
[`configuration` function](../Scripting/README.md#recipes-orchardcorerecipes)
reads that value without embedding a literal secret in the recipe. JavaScript
recipe expressions require **JavaScript Scripting** (`OrchardCore.Scripting.JavaScript`).
Configuration is resolved by the **Orchard server process**, not the computer
running `pomi`; exporting `OC_CLIENT_SECRET` in your CLI shell does not send it to
a remote server's setup recipe.

Select the recipe with `pomi tenants setup Site1 --recipe-name <recipe-name>`
alongside the other required setup arguments. The recipe must already be
available on the server, and a recipe configured when the tenant was created
takes precedence.

### Troubleshooting client credentials

| Symptom | What to check |
| --- | --- |
| `invalid_client` | The application exists in the selected tenant, its type is confidential, and its client ID and secret match. |
| `unauthorized_client` or an unsupported-grant error | Client credentials must be enabled on both the server and the application. |
| `invalid_scope` | The application allows `orchardcore.management`, and that scope exists in the target tenant. Do not request `offline_access` for this flow. |
| `403` when fetching metadata | The application's selected role needs **Access remote management API**. |
| Login succeeds but a command returns `403` | The application role also needs that operation's permissions. |
| A command asks for login after a successful check | The check did not save a session. Supply `OC_CLIENT_ID` and `OC_CLIENT_SECRET` to the command's process, with `OC_CONTEXT` naming the context. |
| Authentication targets an unexpected tenant or client | Check `pomi context show`, the explicit `--context`, and any inherited `OC_CONTEXT` / `OC_CLIENT_ID` / `OC_CLIENT_SECRET` variables. |
| A warning says that `OC_CLIENT_ID` and `OC_CLIENT_SECRET` were not used | Pomi kept them from a context they may not belong to. Set `OC_CONTEXT` to the context they belong to, as described in [step 3](#3-supply-credentials-and-register-the-context). |

See the [client-credentials HTTP contract](../../api/authentication/README.md#client-credentials)
for token endpoint parameters and OAuth error responses.

## Dynamic commands

The CLI downloads the selected tenant's OpenAPI document and maps operations carrying `x-oc-cli` metadata to noun-and-verb commands:

```text
pomi <resource> <verb> [arguments] [options]
```

To create and initialize a tenant in one operation, use
[`pomi tenants install`](../../api/tenants/README.md#install-a-tenant):

```bash
pomi tenants install Blog --request-url-prefix blog --recipe-name Blog \
  --site-name "My Blog" --user-name admin --email admin@example.com
```

It prompts securely for the password and returns the initialized tenant's URL.
The current context must target the Default tenant. It does not create a new
context or sign in to the new tenant. The separate create and setup commands
remain available when these stages need to happen independently. Other examples:

```bash
pomi tenants create --name TenantA --request-url-prefix tenant-a --recipe-name SaaS
pomi tenants setup TenantA --site-name "Tenant A" --user-name admin --email admin@example.com
pomi content items list
pomi content items show 4abc...
pomi features enable OrchardCore.Media
pomi queries execute RecentPosts --body '{ "parameters": {} }'
```

`pomi tenants create` creates an uninitialized tenant but no user account. Run
`pomi tenants setup` to execute the selected recipe and create the initial
administrator. It securely prompts for the password by default; automation can
use `--password-env`, `--password-stdin`, or `--password-file`. After setup,
enable Remote Management from the Default tenant context, add the initialized
tenant URL as its own context, and authenticate directly:

```bash
pomi tenants setup TenantA --site-name "Tenant A" --user-name admin --email admin@example.com
pomi tenants enable-remote-management TenantA
pomi context add tenant-a https://cms.example.com/tenant-a --current
pomi login
```

Next-command hints omit `--context` when the suggested context is already the
current default. Hints targeting another context include its name explicitly,
so copying the command keeps the intended tenant selected.

The default `--output auto` writes human-readable messages in a terminal (tables for lists) and JSON when redirected to a pipe or file. Use `--output human` to keep readable messages even when redirected. Use `--output json` explicitly for automation, or `--output table|csv|tsv|yaml|toml|none` for other representations. Tables may shorten long non-URL cells; HTTP and HTTPS URLs remain complete. JSON preserves the full response. TOML omits properties whose value is `null`; root arrays and scalar values are emitted under `items` and `value`, respectively. JSON request bodies can come from `--body`, `--body-file`, or `--stdin`. Binary request bodies use `--file` or `--stdin`. Discovered option and argument names use lower kebab-case. List commands that page their results use zero-based `--skip` and `--take` options.

Every resource that accepts a request body exposes a `schema` verb. When all input operations use the same shape, the schema is returned directly:

```bash
pomi content types schema
pomi content parts schema
```

Use `--operation` when a resource accepts different request shapes:

```bash
pomi users schema --operation create
pomi media files schema --operation move-batch
```

The result is a standalone JSON Schema extracted from the tenant's OpenAPI document. Content items use the tenant-aware `pomi content items schema <content-type>` command so attached parts and fields reflect the selected content type.

Content-definition schemas include the built-in settings contracts while allowing settings contributed by other features. For example, `pomi content parts schema` describes `ContentPartSettings.attachable` and `ContentPartSettings.reusable`, so an attachable reusable part can be submitted without relying on an existing definition as an example:

```json
{
  "name": "ArticleDetails",
  "settings": {
    "ContentPartSettings": {
      "attachable": true,
      "reusable": true
    }
  },
  "fields": []
}
```

Dynamic discovery also requires `ViewOpenApiContent` when the tenant protects
its OpenAPI document. `AccessRemoteManagement` alone does not grant access to
that document or the resource-specific operations it describes.

### Media and custom assets

Use Media for images and custom CSS, JavaScript, or SVG assets. Uploads use the
same store, folder permissions, extension policy, and size limits as the admin
Media library. Inspect the caller's permitted extensions first:

```bash
pomi media constraints show --output json
pomi media folders create --name assets
pomi media folders create --path assets --name styles
pomi media files upload site-v1.css --path assets/styles --file ./site.css
pomi media files show assets/styles/site-v1.css --output json
pomi media files list --path assets/styles --output table
```

Media file commands preserve resource paths and also return direct, absolute
URLs, including configured CDN URLs. File list tables show both **Path** and
**URL**; upload, copy, and move results include the destination URL. See the
[media representations](../../api/media/README.md#file-or-folder).

Choose your own folder convention in the client. Paths are media-store-relative;
the positional upload argument is a base filename. CSS, JavaScript, and SVG
require `UploadRestrictedMedia` by default, in addition to `ManageMediaContent`
and permission for the destination folder. Extensions absent from both configured
extension lists are rejected even with that permission. The constraints response
includes restricted extensions only for callers permitted to upload them.

Use the response's `url` for public access and `filePath` in media fields or the
Liquid `asset_url` filter. Uploads reject existing files, so use versioned names
for updates. Files appear in the admin Media library. See the
[Media API](../../api/media/README.md) for authorization and mutation details.

Media storage is extensible through `IMediaFileStore`; its default is local disk.
To share uploads across nodes, configure the same shared backend, such as
[Azure Blob](../Media.Azure/README.md) or [Amazon S3](../Media.AmazonS3/README.md),
for the tenant on all nodes. Uploading to a local store does not replicate files.
Tenant static files remain deployment assets and have no management API.

Every static and discovered command supports `--help`. OpenAPI metadata is cached by tenant URL and ETag to reduce discovery requests. Help and completion read the cache offline. Before an online dynamic command, the CLI checks the tenant's API revision with a lightweight authenticated `HEAD` request. When enabled features or module builds change, it refreshes the manifest and OpenAPI document before parsing the command. This also discovers features enabled in the admin UI or by another client. Successful mutations still expire discovery metadata for the next online command, including changes to resource schemas. Older servers without revision headers keep the time-based cache behavior. See [API revision and cache freshness](../../api/discovery/README.md#api-revision-and-cache-freshness) for the protocol and its limits. Use `pomi api refresh --force` to bypass the cache and `pomi api compatibility` for protocol checks.

`pomi --version` prints the CLI version number. Use `pomi doctor` to inspect the CLI version, platform, local storage, and caches, or `pomi doctor --output json` for structured diagnostics. Both commands work offline; `doctor` does not test server connectivity.

The CLI is distributed as standalone native archives and NativeAOT .NET tool
packages for Linux, Windows, and macOS on x64 and Arm64. See
[installation instructions](../../../guides/remote-management/README.md#install-as-a-net-tool)
for installing with the .NET 10 SDK or later.

The executable is named `pomi` (`pomi.exe` on Windows). Its NuGet package ID is
`OrchardCore.Cli`, its server authentication client ID `orchardcore-cli`, and its
OpenAPI extension `x-oc-cli`.

Each request has 30 seconds to complete. Operations that do their work before they respond, such as `tenants install`,
`tenants setup` and `recipes execute`, have 10 minutes. A binary upload and the body of a response saved with
`--output-file` have no limit. To change the limit, in seconds and up to one day, pass `--request-timeout` to a command
calling the API, or set `OC_REQUEST_TIMEOUT` for every request of the process, which the option takes precedence over.
A request that runs out of time exits with code `2`, and a change the server already started may still complete.

Set `OC_CONFIG_HOME` to an absolute directory to isolate contexts, caches, and Unix file credentials for testing. The default installation keeps the shared token-file location described above. `pomi login --no-browser` prints the PKCE login URL instead of launching a browser; open it on the same computer as the CLI.

Generate shell completion with `pomi completion --shell bash|zsh|fish|pwsh`. The generated script uses the CLI's cached command tree; no separate suggestion service is required.

## Raw API and documentation search

`pomi api invoke` is the escape hatch for an authenticated endpoint not projected as a dynamic command:

```bash
pomi api invoke GET /api/example
```

The CLI maintains a local cache of the documentation search index that the manifest of the
selected context names in `documentationIndexUrl`. Without a context, or when the manifest doesn't
name a secure address, it uses the index published by `docs.orchardcore.net`:

```bash
pomi docs update
pomi docs search "content definition"
pomi docs show <result-id>
```

Documentation is treated as untrusted text and is never executed.

## Compatibility

The protocol has an independent semantic version. The CLI rejects unsupported major versions and warns when a server uses a newer compatible minor version. Use the manifest compatibility range rather than the Orchard Core product version when deciding whether a CLI binary can manage a tenant.

Dynamic command names and operation IDs are public compatibility surfaces. Feature authors should preserve them or provide aliases when an HTTP route changes.

## Contribute a management command

Reference `OrchardCore.RemoteManagement.Abstractions` from a feature that owns a management API. Keep the endpoint's standard OpenAPI operation ID, tags, parameters, request schema, responses, summary, and description authoritative, then add only the CLI-specific projection:

```csharp
endpoints.MapGet("/api/example/widgets", ListWidgetsAsync)
    .WithName("ListWidgets")
    .WithSummary("Lists widgets.")
    .WithCliCommand(new CliOperationMetadata(["widget"], "list")
    {
        Capability = "example.widgets",
    });
```

The Remote Management OpenAPI transformer emits this metadata as `x-oc-cli`. Use `Arguments` for positional ordering, `InputMode` for complex bodies, `DefaultJsonBody` when a command should send a default body if none is supplied, `RequiresConfirmation` for destructive operations, aliases for compatibility, and `TableColumns` for optional table output.

More properties describe how Pomi presents an operation, so that it doesn't need to know the
resource:

| Property | `x-oc-cli` | Effect |
| --- | --- | --- |
| `ResourceName` | `resourceName` | The singular name in the summary of a change, as in `Tenant 'Demo' created successfully.` |
| `CompletedAction` | `completedAction` | The phrase for a verb Pomi can't put in the past tense, as in `Tenant 'Demo' reloaded successfully.` It requires `ResourceName`. |
| `SummaryProperties` | `summaryProperties` | The response properties that human-readable output shows after a change, instead of all of them. |
| `ItemsProperty` | `itemsProperty` | The response property holding the rows of a list, for table output without `TableColumns`. Without it, Pomi looks for `items`. |
| `SetupPasswordProperty` | `setupPasswordProperty` | The body property holding the password of a new administrator. Pomi requires it and applies the setup password rules before sending the request. |
| `RequestSchemaPath` | `requestSchemaPath` | The path returning the JSON Schema of a request body that depends on the arguments, which the OpenAPI document can't describe, such as `api/settings/sections/{name}/schema`. Pomi refuses `--body` when it contains a property that this schema marks `writeOnly`. |
| `NextSteps` | `nextSteps` | The commands suggested after the operation succeeds, see below. |
| `LongRunning` | `longRunning` | The operation does its work before it responds, which can take minutes. Pomi waits up to 10 minutes for its response instead of 30 seconds. Don't set it on an operation that queues its work and returns `202`. |
| `ReadOnly` | `readOnly` | An operation using a method that usually changes something, such as a `POST` that validates its input, changes nothing. Its MCP tool is annotated as read-only, and Pomi doesn't report a change. |

Human-readable output shows the first of the `NextSteps` whose condition matches the response:

```csharp
NextSteps =
{
    new CliNextStepMetadata
    {
        Description = "start this tenant.",
        WhenProperty = "state",
        WhenValue = "Disabled",
        Arguments = { "tenants", "start", "{name}" },
    },
},
```

An argument written as `{property}` is replaced by that response property, and quoted for the shell. `OptionalArguments`
adds arguments when a response property is empty, and `Note` adds text after the command. Nothing is suggested when a
value is missing, starts with `-` or contains a control character. Set `SavesContext` instead of `Arguments` to suggest
saving a context for the tenant that the response describes through its `name` and `url` or `primaryUrl`.

Set `SecretProperties` to the body properties that must not appear on a command line. Pomi replaces
their options with `--<property>-env`, `--<property>-file` and `--<property>-stdin`, prompts for a
required one without echoing it, and removes `--body` from the command, leaving `--body-file` and
`--stdin`. It treats a property the same way when its JSON Schema in the OpenAPI document has
`writeOnly: true`, or contains a value that does, and doesn't print a property marked `writeOnly` if a
response returns it.

Set `FileResponse = true` for responses that Pomi must stream to disk. The transformer
emits `fileResponse: true`; Pomi requires `--output-file` and reserves a new private
file before sending the request. Existing files are never overwritten and failed
transfers remove incomplete output. File-response operations are excluded from MCP
tool discovery, including JSON files that must retain their original bytes.

Set `SecretResponse = true` for a JSON response containing one-time credentials.
The transformer emits `secretResponse: true`, and Pomi requires
`--secret-output-file <new-path>`. It reserves an owner-only file before sending
the operation, refuses existing paths, saves the complete JSON response privately,
and returns only `secretOutputFile` in normal output. This applies to all output
formats, including `--output none`. HTTP clients still receive the one-time response
and must store it privately. The operation is not exposed as an MCP tool. This flag
does not replace endpoint authorization.

Set `ProvisioningProperty` on an operation that can create an administrative application,
to the name of the boolean parameter or body property requesting it. Pomi then offers it as
an option and saves the returned credentials in a new context. Such an operation also needs
`ExcludedFromMcp = true` when it can return credentials depending on its input.

Register an `IRemoteManagementCapabilityProvider` when the feature also needs to report a versioned capability in the authenticated manifest. CLI metadata describes discoverability only; endpoint authorization remains mandatory.

## Endpoint and credential boundaries

Use the exact externally reachable tenant URL when adding a context, including
its path prefix. HTTPS is required; HTTP is accepted only for local loopback
development. Embedded URL credentials and fragments are rejected. The
bootstrap manifest must identify that same tenant, and authenticated API and
OpenAPI requests must remain inside its origin and path prefix. OpenID
endpoints must remain on the advertised authority's origin. Automatic HTTP
redirects are disabled, including redirects from token endpoints; fix the
configured public URL when a proxy returns a redirect.

Credentials are keyed by context name, exact tenant URL, authority, and client
ID. Reusing a context name for another tenant is rejected: choose a different
name or explicitly delete the old context. This also separates credentials
when two local configuration directories use the same context name.
Unix credentials remain shared owner-only files under `~/.orchardcore/credentials`,
so ordinary CLI commands do not prompt for operating-system keychain access.

## MCP server

Enable **Remote Management MCP** (`OrchardCore.RemoteManagement.Mcp`) on each tenant
that should accept Model Context Protocol clients. This feature depends on Remote
Management and uses its OpenID Connect configuration. Enabling Remote Management
alone does not expose an MCP endpoint.

Connect a Streamable HTTP MCP client to `https://your-site/mcp`, or
`https://your-site/tenant-prefix/mcp` for a tenant with a URL prefix. The server uses
the official C# MCP SDK in stateless mode; requests do not retain a tenant session
or caller identity between HTTP requests.

### Authentication and permissions

Supply an OAuth access token in `Authorization: Bearer <access-token>` on every
request. Tokens use the same tenant OpenID validation and API authentication scheme
as Pomi. The authenticated user or application role needs **Access remote management
APIs**, and each tool call additionally enforces the original operation's permissions.
Access tokens must carry the `orchardcore.management` scope, on management APIs too.
An administrator browser cookie does not authorize MCP requests.

The tenant publishes OAuth protected resource metadata at
`/.well-known/oauth-protected-resource/mcp`, relative to its URL prefix. Unauthorized
MCP requests include this URL in the `WWW-Authenticate` challenge. The metadata
identifies the MCP URL, the tenant's configured authorization server, and the
`orchardcore.management` scope.

### Automatic client registration

Open **Settings → Remote Management → Configure MCP authentication**. If shared
Remote Management authentication is not ready, select **Configure MCP
authentication** once. If it is already configured (for example, for Pomi), you
can immediately connect an OAuth-capable MCP client using only the tenant's
`/mcp` URL. No callback URLs or shared MCP client identifier are needed.

The client follows this flow:

1. Request `/mcp` and receive `401 Unauthorized` with a `WWW-Authenticate`
   challenge identifying the protected-resource metadata and required scope.
2. Discover the tenant's authorization server and its `registration_endpoint`.
3. Register its own callback URLs and receive a unique public client ID.
4. Open the tenant's sign-in and consent pages using authorization code flow
   with PKCE (`S256`) and the tenant's `/mcp` URL as the OAuth `resource`.
5. Exchange the authorization code for a token, then retry the MCP request.

Start the connection from your **MCP client**, which initiates the interactive
OAuth flow; `/mcp` is not a browser sign-in page. Clients must support OAuth discovery and
RFC 7591 Dynamic Client Registration for this automatic setup. Client ID Metadata
Documents are not currently supported. Clients requiring pre-registration can
use the manual workflow below.

Every registration creates a separate OpenID application with an identifier
such as `orchardcore-mcp-<random-id>`. No application is created until a client
registers. Review or remove these in **Access Control → OpenID Connect →
Applications**, also linked from the MCP configuration page. Names are supplied
by clients and are **not verified identities**, so the consent page warns about them
and shows the host that receives the access. Registration issues no token,
assigns no roles, creates no user, and does not modify Pomi or shared settings.
The signed-in user must consent and still needs remote management and each
operation's permissions.

Automatic applications require exact registered HTTPS callbacks, except that
native clients may use loopback HTTP callbacks (including ephemeral loopback
ports supported by OpenIddict). Wildcards, fragments, and embedded credentials
are rejected. Authorization and token requests must specify the current tenant's
MCP `resource`; another tenant's resource is rejected. Issued tokens include the
MCP URL in their audiences alongside the existing management resources, and `/mcp` answers
`403` when the token of such a client doesn't target that exact URL, such as a token requested
for another host of the tenant. Tokens of other applications, such as client credentials
applications, preconfigured MCP clients and Pomi, have no MCP audience and aren't subject to this check.

As anyone can register such a client, its tokens only work on `/mcp`: the management APIs and the other
token-authenticated endpoints of the tenant answer `403` to them, whatever the permissions of the user who
consented. The tools of `/mcp` keep working, as they run inside the `/mcp` request. Clients that need the APIs
use a [manually registered](#manual-registration-and-unattended-clients) application.

### Registration endpoint and limits

`POST /connect/mcp/register` is relative to the tenant URL prefix and accepts
JSON using RFC 7591 property names. A successful request returns `201 Created`
with `client_id`, `client_id_issued_at`, and the accepted metadata. It never
returns a client secret or a registration-management token.

```json
{
  "client_name": "My MCP client",
  "redirect_uris": ["http://127.0.0.1:5123/callback"],
  "grant_types": ["authorization_code", "refresh_token"],
  "response_types": ["code"],
  "token_endpoint_auth_method": "none",
  "scope": "openid offline_access orchardcore.management"
}
```

`redirect_uris` is required (1–8 URLs, at most 2,048 characters each). Defaults
are `authorization_code`, `code`, `none`, and `orchardcore.management` for the
last four properties. Only authorization-code and optional refresh-token grants
are accepted. Optional scopes are `openid`, `profile`, `roles`, and
`offline_access`; `orchardcore.management` must be included. Client names are
limited to 100 characters, scopes to 256, and request bodies to 16 KiB.
Unsupported metadata such as requested roles, client IDs, secrets, or remote
logo/document URLs is ignored; it is never fetched or granted.

Registration is anonymous but bounded to **10 attempts per minute per remote address**,
and **100 per minute per tenant per application instance**, with no queue. Rate-limited
requests return `429` with `Retry-After: 60`. Behind a reverse proxy, forward the client
address with the Reverse Proxy feature, otherwise all callers share the limit of the proxy's
address. Automatic registration stops at **1,000 automatically registered
applications** by default; capacity or unconfigured authentication returns `503`. When full,
registrations older than a day that no user authorized are removed to make room.
The application count and creation are protected by the configured tenant
lock. Deploy a distributed lock provider for coordination across nodes, and
apply any additional global rate limits at your ingress.

Configure the following tenant settings in `appsettings.json` (or through the
host's Orchard configuration providers, or for one tenant in its own configuration):

```json
{
  "OrchardCore": {
    "OpenId": {
      "Mcp": {
        "AllowDynamicClientRegistration": true,
        "MaximumApplications": 1000
      }
    }
  }
}
```

Set `AllowDynamicClientRegistration` to `false` to stop advertising the endpoint
and return `404` for registrations. This does not delete existing applications
or revoke their tokens; manage those separately in OpenID application management.

Invalid metadata returns `400` with OAuth `invalid_client_metadata` or
`invalid_redirect_uri`. Responses are not cacheable. Each successful registration
creates a new application: clients should persist and reuse their client ID
for that authorization server. Retrying after an uncertain response can create
another application; administrators can remove unused registrations.

### Manual registration and unattended clients

Expand **Advanced: register a client manually** on the MCP configuration page.
Enter an identifier (default `orchardcore-mcp`) and the exact callbacks supplied
by that client, then select **Register MCP client**. This configures shared
authentication and creates a public application with PKCE and explicit consent.
It preserves existing roles and unrelated permissions when updating an application
previously created by this action. Saving replaces its complete callback list.
Identifiers owned by other applications, including Pomi, cannot be overwritten.
Revisit a manual registration using the page's `clientId` query parameter.

The same manual setup is available in recipes after enabling the MCP feature:

```json
{
  "name": "RemoteManagementMcpConfiguration",
  "clientId": "orchardcore-mcp",
  "redirectUris": "https://your-mcp-client.example/oauth/callback"
}
```

Follow it with `ReloadTenant` to apply shared settings. For automatic registration,
use the shared `RemoteManagementConfiguration` step and `ReloadTenant`; there is
no particular client to pre-register.

For machine-to-machine clients, configure a confidential OpenID application
through OpenID application management and assign the required roles. Automatic
registration never enables client-credentials or password grants. Existing
preconfigured clients and externally supplied bearer tokens remain supported.
Requests carrying an `Origin` header must match the tenant URL's scheme and host.

### Tools

Tools are projected from the active tenant's management endpoints and generated
OpenAPI document. Tool names join the CLI command group and verb with underscores:
for example, `features_list`, `features_show`, and `features_enable`. After changing
enabled features, clients should refresh `tools/list`; stateless servers do not send
unsolicited tool-list change notifications.

Inputs group OpenAPI parameters by location to avoid naming collisions:

```json
{
  "name": "features_show",
  "arguments": {
    "path": { "featureId": "OrchardCore.Contents" }
  }
}
```

Use `query` for query parameters and `body` for a JSON request body. The schemas in
`tools/list` specify required values and contain the referenced JSON Schema definitions.
Query arrays are passed as repeated query parameters. Existing default JSON bodies
from CLI metadata also apply when the client omits `body`.

Calls execute the existing endpoint with the caller's tenant services, bearer token,
authorization policies, endpoint filters, and API validation. A tool result contains
text with a JSON object containing the HTTP `statusCode` and the response `body`, as
JSON when the response is JSON and as a string otherwise.
HTTP errors set MCP `isError` to `true`. Operation permissions are evaluated at call
time; discovery does not promise that a caller can invoke every listed tool.
A response larger than `MaximumResponseBytes` (4 MiB by default) is turned into a tool
error, as responses are held in memory.

Only named endpoints carrying `CliOperationMetadata` are exposed. Hidden CLI
operations, stream inputs, binary responses, multipart bodies, and header/cookie parameters are excluded.
Duplicate tool names are excluded to prevent ambiguous dispatch.

Tool arguments and results reach the model's context and the client's transcript, so
operations that take or return credentials are excluded too: those with `SecretProperties`
or `SecretResponse`, such as creating users or rotating client secrets, those setting
`ExcludedFromMcp`, such as enabling remote management, which can return a new client secret,
and those whose OpenAPI input schema declares a `writeOnly` value. Settings sections take a
free-form body, so `settings_sections_update` stays a tool but refuses the sections whose
schema declares a `writeOnly` value, such as `smtp` and the OpenID sections.
Use Pomi or the HTTP APIs for these operations and for file transfers.

The operations for which Pomi asks a confirmation (`RequiresConfirmation`), such as executing
a recipe, deleting or stopping a tenant, importing a deployment package or any deletion, are
left out too: a model can invoke a tool without asking anyone. Enable them for a tenant with
`AllowConfirmationRequiredTools`, which is off by default. These settings configure the tools:

```json
{
  "OrchardCore": {
    "RemoteManagement": {
      "Mcp": {
        "AllowConfirmationRequiredTools": false,
        "MaximumResponseBytes": 4194304
      }
    }
  }
}
```

Other write operations, and these when enabled, are marked as potentially destructive using
MCP tool annotations; clients should obtain user confirmation before invoking them.
Annotations are advisory and do not add an interactive confirmation step on the server.
Plain creations are not marked.

## Security and hardening

Remote management gives clients the same administrative power as the admin UI, from outside of it. This section gathers what to decide before enabling it, and links to the details.

### Who can enable remote management

**Manage remote management configuration** (`ManageRemoteManagementConfiguration`) controls who can change how a tenant is managed remotely. The Administrator role has it by default, next to **Access remote management API** (`AccessRemoteManagement`). Grant it only to people who could already administer the tenant.

| Action | Required permissions |
| --- | --- |
| The actions of **Settings → Remote Management**, on the tenant itself | `ManageRemoteManagementConfiguration` and `ManageServerSettings`, see [Enable and configure](#enable-and-configure) |
| [`POST /api/tenants/{tenantName}:enable-remote-management`](../../api/tenants/README.md#enable-remote-management-for-a-tenant) and [`:disable-remote-management`](../../api/tenants/README.md#disable-remote-management-for-a-tenant), from the Default tenant | `ManageTenants` and `ManageRemoteManagementConfiguration` |
| `enableRemoteManagement` when [installing a tenant](../../api/tenants/README.md#install-a-tenant), from the Default tenant | `ManageTenants` and `ManageRemoteManagementConfiguration` |

The operations from the Default tenant record an event in the **Tenant remote management** category of its audit trail when **Audit Trail** is enabled there. Enable it to keep track of them. Disabling remote management stops the APIs and `/mcp`, but keeps the OpenID applications, their scopes and the settings: delete the applications you no longer want in the OpenID administration.

The feature only opens a door: `AccessRemoteManagement` and the `orchardcore.management` scope don't authorize any operation by themselves, and each operation checks its own permissions. Creating or updating a role or an OpenID application can only grant the permissions the caller holds, see the [Roles](../../api/roles/README.md) and [OpenID](../../api/openid/README.md) APIs.

### What `provisionClient` grants

`provisionClient`, `enableRemoteManagement` and Pomi's `--provision-client` and `--enable-remote-management` create a confidential application that has the **administrator role of the target tenant**. It keeps this access independently of any user account, and even when the password of the administrator changes.

- The secret is returned once, with `Cache-Control: no-store`. Pomi saves it in a new context and leaves it out of its output. See [what Pomi stores](../../../guides/remote-management/README.md#optional-unattended-remote-management).
- Each request creates another application: it doesn't rotate the previous ones. Delete the application to revoke its access, as `pomi logout` only removes the local copy.
- The operation is written to the log, and to the audit trail as above. It isn't an MCP tool, as it returns a secret.
- For a job that needs less, register a [limited application](#client-credentials-for-automation) rather than provisioning one.

### MCP clients

- **Automatic registration is anonymous.** Anyone who can reach a tenant with MCP authentication configured can create a public OpenID application, within [its limits](#registration-endpoint-and-limits). This grants nothing by itself, as a signed-in user with the required permissions must still consent, but client names are [not verified](#automatic-client-registration): a consent request can impersonate a known client. Train the users who approve them to check the host that the consent page shows.
- **Turn it off when you don't need it.** Set `AllowDynamicClientRegistration` to `false`, lower `MaximumApplications`, and use [manual registration](#manual-registration-and-unattended-clients) for a known list of clients. Turning it off doesn't delete the existing applications or revoke their tokens.
- **Destructive operations are left out by default.** A model can invoke a tool without asking anyone, so the operations for which Pomi asks a confirmation aren't tools unless `AllowConfirmationRequiredTools` is set, see [Tools](#tools). Setting it lets a model delete data or execute recipes with the permissions of the user who consented, and the annotations marking these tools are only advisory. Set it only for clients that ask before each call.
- **Secrets stay out of tools.** Tool arguments and results reach the model's context and the client's transcript, so the [operations taking or returning credentials](#tools) aren't tools, and `settings_sections_update` refuses the sections that accept secrets. Use Pomi or the HTTP APIs for them.
- **Audience.** `/mcp` requires the token of an automatically registered client to target that exact URL, see [Automatic client registration](#automatic-client-registration). The same token is refused by the management APIs of the tenant, so that an application anyone can register can only use `/mcp`.

Grant the people who connect MCP clients only the permissions they want a model to use, as a tool runs with theirs.

### Credentials on the client

On macOS, Linux and other Unix-like systems, Pomi stores tokens and application secrets as **plaintext** owner-only files under `~/.orchardcore/credentials`. Windows uses Windows Credential Manager. On a shared machine or a backed-up home directory, treat these files as administrative access to the tenant, see [Contexts and login](#contexts-and-login). In CI, inject the secret from a secret store instead, and set `OC_CONTEXT` so that it can't reach another tenant, see [Client credentials for automation](#client-credentials-for-automation).

Use HTTPS for every context; HTTP is only accepted for loopback addresses. See [Endpoint and credential boundaries](#endpoint-and-credential-boundaries).

### Rate limits

Enable the [Rate Limits](../RateLimits/README.md) feature on tenants reachable from the internet.

- It limits the device authorization and verification endpoints to 30 requests per minute per remote address, as they don't require an authenticated client. See [Rate limiting](../OpenId/README.md#rate-limiting).
- [Automatic MCP client registration](#registration-endpoint-and-limits) has its own limits of 10 attempts per minute per remote address and 100 per tenant, with or without this feature.
- Behind a reverse proxy, forward the client address with the [Reverse Proxy](../ReverseProxy/README.md) feature, otherwise all callers share the budget of the proxy's address.

The seeded Default Global Policy also limits discovery, management and token requests, so [budget automation traffic](../../api/rate-limits/README.md) accordingly.
