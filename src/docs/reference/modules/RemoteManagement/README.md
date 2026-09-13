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
OpenID settings are preserved. Existing tenants using Pomi should explicitly enable
this feature to retain CLI discovery after upgrading.

The shared **Configure Remote Management** action checks and repairs authorization
code with PKCE, refresh tokens, client credentials, local token validation, and the
`orchardcore.management` scope. It creates neither a CLI nor an MCP application.
Each client action configures these shared requirements as well, so it can be used
on a freshly enabled tenant.

For automated deployments, the **Orchard Core Remote Management** recipe configures
only shared authentication. Use **Orchard Core Remote Management CLI**
(`RemoteManagementCli`) for Pomi; its `RemoteManagementCliConfiguration` step creates
or repairs the CLI application. Both resolve the current tenant automatically.

The public bootstrap document is available at `/.well-known/orchardcore-management`. It contains only the protocol version, authentication authority, client ID, and supported grants. The authenticated `/api/management/manifest` endpoint additionally returns tenant identity, compatibility ranges, capabilities, OpenAPI coordinates, and documentation index information.

Access to the authenticated manifest and management APIs requires the **Access remote management API** permission. Each contributed operation also enforces its own Orchard Core permission.

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

The CLI does not read or migrate tokens previously written to macOS Keychain or
Linux Secret Service, avoiding an operating-system credential prompt. Sign in
once per context after upgrading; legacy entries can be removed with the
platform's credential-management tools. Tokens from older name-only file
credentials also require one new login because the current key binds the tenant
URL, authority, and client ID. Those obsolete files are not migrated or removed
by a logout of the new login.

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

In CI, inject the secret from the CI system's secret store. For a local test in
Bash or Zsh, this prompt reads it without displaying it or putting its literal
value in shell history:

```bash
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
The same variables apply to every `pomi` command in that environment, so use
`--context` explicitly and supply credentials registered in that target tenant.

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
`OC_CLIENT_SECRET`.

When finished with the local test, clear the variables:

```bash
unset OC_CLIENT_ID OC_CLIENT_SECRET
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
| A command asks for login after a successful check | The check did not save a session. Supply `OC_CLIENT_ID` and `OC_CLIENT_SECRET` to the command's process. |
| Authentication targets an unexpected tenant or client | Check `pomi context show`, the explicit `--context`, and any inherited `OC_CLIENT_ID` / `OC_CLIENT_SECRET` variables. |

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

The default `--output auto` writes human-readable messages in a terminal (tables for lists) and JSON when redirected to a pipe or file. Use `--output human` to keep readable messages even when redirected. Use `--output json` explicitly for automation, or `--output table|csv|tsv|yaml|toml|none` for other representations. Tables may shorten long non-URL cells; HTTP and HTTPS URLs remain complete. JSON preserves the full response. TOML omits properties whose value is `null`; root arrays and scalar values are emitted under `items` and `value`, respectively. JSON request bodies can come from `--body`, `--body-file`, or `--stdin`. Binary request bodies use `--file` or `--stdin`. Discovered option and argument names use lower kebab-case. List commands use zero-based `--skip` and `--take` options for paging.

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
for installing from the fork's workflow artifacts with the .NET 10 SDK or later.

The executable is named `pomi` (`pomi.exe` on Windows). Its NuGet package ID
remains `OrchardCore.Cli`. Existing contexts, credential storage, and `OC_*`
environment variables continue to work. After updating, change script invocations
to `pomi` and regenerate shell completion. The server authentication client ID
`orchardcore-cli` and OpenAPI extension `x-oc-cli` remain stable for compatibility.

Set `OC_CONFIG_HOME` to an absolute directory to isolate contexts, caches, and Unix file credentials for testing. The default installation keeps the shared token-file location described above. `pomi login --no-browser` prints the PKCE login URL instead of launching a browser; open it on the same computer as the CLI.

Generate shell completion with `pomi completion --shell bash|zsh|fish|pwsh`. The generated script uses the CLI's cached command tree; no separate suggestion service is required.

## Raw API and documentation search

`pomi api invoke` is the escape hatch for an authenticated endpoint not projected as a dynamic command:

```bash
pomi api invoke GET /api/example
```

The CLI maintains a local cache of the search index published by `docs.orchardcore.net`:

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
formats, including `--output none`. HTTP and MCP clients still receive the one-time
response and must store it privately. This flag does not replace endpoint authorization.

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
when two local configuration directories use the same context name. Older
name-only credential entries are not reused; sign in once after upgrading.
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
by clients and are **not verified identities**. Registration issues no token,
assigns no roles, creates no user, and does not modify Pomi or shared settings.
The signed-in user must consent and still needs remote management and each
operation's permissions.

Automatic applications require exact registered HTTPS callbacks, except that
native clients may use loopback HTTP callbacks (including ephemeral loopback
ports supported by OpenIddict). Wildcards, fragments, and embedded credentials
are rejected. Authorization and token requests must specify the current tenant's
MCP `resource`; another tenant's resource is rejected. Issued tokens include the
MCP URL in their audiences alongside the existing management resources.

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

Registration is anonymous but bounded to **10 attempts per minute per tenant
per application instance**, with no queue. Rate-limited requests return `429`
with `Retry-After: 60`. Automatic registration stops at **1,000 total OpenID
applications** by default; capacity or unconfigured authentication returns `503`.
The application count and creation are protected by the configured tenant
lock. Deploy a distributed lock provider for coordination across nodes, and
apply any additional global rate limits at your ingress.

Configure the following tenant settings in `appsettings.json` (or through the
host's Orchard configuration providers):

```json
{
  "OrchardCore": {
    "OrchardCore_OpenId": {
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
for example, `features_list`, `features_show`, and `features_disable`. After changing
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
text with a JSON object containing the HTTP `statusCode` and response `body` string.
HTTP errors set MCP `isError` to `true`. Operation permissions are evaluated at call
time; discovery does not promise that a caller can invoke every listed tool.

Only named endpoints carrying `CliOperationMetadata` are exposed. Hidden CLI
operations, stream inputs, binary responses, multipart bodies, and header/cookie parameters are excluded.
Duplicate tool names are excluded to prevent ambiguous dispatch. Write operations
are marked as potentially destructive using MCP tool annotations; clients should
obtain user confirmation before invoking them. Annotations are advisory and do not
add an interactive confirmation step on the server. Use Pomi or the HTTP APIs for
file transfers.
