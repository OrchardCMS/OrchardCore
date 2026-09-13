# Create and initialize tenants

Apply the [shared operating rules](shared-rules.md). Use
[authentication and contexts](authentication.md) if the Default tenant context
is not ready. This guide assumes the application already exists. To create the initial
application, follow [local installation](installation.md); for a new SaaS host,
use `pomi install <directory> --recipe-name SaaS`.

Use `pomi tenants install <name>` to create and set up a tenant in an existing
Orchard application. Run it from an authenticated context targeting the
`Default` tenant; it requires tenant-management permissions and no local SDK.
Run `pomi context list --output json` and set `HOST_CONTEXT` to the existing
context for the intended host's Default tenant. Its name need not be `default`.
Keep this host name separate from the new child's context name:

```bash
pomi --context "$HOST_CONTEXT" tenants install News \
  --request-url-prefix news \
  --recipe-name Blank \
  --site-name "Contoso News" \
  --user-name admin \
  --email admin@example.com \
  --password-env OC_TENANT_ADMIN_PASSWORD \
  --enable-remote-management \
  --output json > ./News.install.json
```

Recommend SQLite when no database provider is requested; `Sqlite` is the default
unless the host supplies a database preset. For a non-SQLite provider, recommend
a unique `--table-prefix`, respecting host presets and prefix patterns; follow
the [database choice rules](shared-rules.md#database-choice-for-new-sites-and-tenants).
This differs from local `pomi install`: tenant installation uses the existing server and
creates no local application. For automated site building, `--enable-remote-management`
enables the target tenant's CLI feature and OpenID dependencies and creates a
dedicated administrative application. It works with Blank as well as SaaS when
feature profiles permit the dependencies. Omit the option for recipe-driven setup
without additional remote management; no context is then created automatically.

After successful provisioned installation, use the returned `context` and verify
its `primaryUrl` against the saved context:

```bash
TENANT_CONTEXT="$(python3 -c 'import json; print(json.load(open("News.install.json"))["context"])')"
pomi context list --output json
pomi --context "$TENANT_CONTEXT" api refresh --output json
```

Require a nonempty context and use it explicitly. Pomi obtains a client-credentials
token automatically; do not run `context add`, `login`, or a device flow. Remove
host-only `OC_CLIENT_ID`/`OC_CLIENT_SECRET` overrides from these child commands'
environment so they use the saved application credentials. The current host context
is preserved. Keep the initial administrator credentials in a private handoff file
and record the actual tenant URL and context there; the application does not replace
the human account or change its password.
If creation succeeds but setup fails, inspect `pomi tenants show News`. For an
uninitialized tenant, correct the setup inputs and use `tenants setup News`;
do not blindly repeat installation or delete the partially created tenant.

For separate creation and setup (for example, to hand off an uninitialized
tenant), use:

```bash
pomi --context "$HOST_CONTEXT" tenants create \
  --name News \
  --request-url-prefix news \
  --database-provider Sqlite \
  --recipe-name Blank
```

`recipes list` excludes setup recipes. Use a setup recipe known to be installed
by the deployment (for example `Blank` in the standard CMS host), or obtain the
allowed recipe name from the operator/setup UI. Do not infer availability from
ordinary recipe discovery.

Inspect the authoritative inputs when host database presets or patterns differ:

```bash
pomi --context "$HOST_CONTEXT" tenants schema --operation install
pomi --context "$HOST_CONTEXT" tenants schema --operation create
pomi --context "$HOST_CONTEXT" tenants schema --operation setup
```

The schema lists JSON properties, not CLI switches. Always check
`pomi --context "$HOST_CONTEXT" tenants install --help` or the corresponding operation's
help before constructing its command. See the mapping and connection-string
examples below.

Set up the uninitialized tenant without opening its setup URL:

```bash
pomi --context "$HOST_CONTEXT" tenants setup News \
  --site-name "Contoso News" \
  --user-name admin \
  --email admin@example.com
```

Read the [setup password policy and compliant generator](setup-password.md)
before setup. Weak passwords are rejected before tenant creation or setup.

Both `tenants install` and `tenants setup` use a masked password prompt by
default. For automation, use exactly one of:

```bash
pomi --context "$HOST_CONTEXT" tenants setup News ... --password-env OC_TENANT_ADMIN_PASSWORD
printf '%s' "$OC_TENANT_ADMIN_PASSWORD" | pomi --context "$HOST_CONTEXT" tenants setup News ... --password-stdin
pomi --context "$HOST_CONTEXT" tenants setup News ... --password-file /run/secrets/news-admin-password
```

Prefer the prompt for interactive work, CI-injected environment variables for
automation, and owner-readable short-lived files for mounted secrets. The
secret-bearing install/setup commands intentionally have no inline `--password` or
`--body` option.

## Schema properties and secret CLI options

For `tenants install` and `tenants setup`, the JSON/CLI mappings include:

| JSON schema property | CLI input |
| --- | --- |
| `siteName` | `--site-name "Contoso News"` |
| `databaseProvider` | `--database-provider Postgres` |
| `tablePrefix` | `--table-prefix "$POMI_TABLE_PREFIX"` |
| `password` | `--password-env VARIABLE`, `--password-file PATH`, or `--password-stdin` |
| `connectionString` | `--connection-string-env VARIABLE`, `--connection-string-file PATH`, or `--connection-string-stdin` |

For `tenants install` only, JSON `enableRemoteManagement: true` maps to the
`--enable-remote-management` boolean switch. Add it to the install examples below
when continuing with automated Pomi management; it is independent of the secret
input method.

`connectionString` remains the correct JSON property name, but **there is no
`--connection-string` option** for these commands. The same applies to
`password` versus the unsupported inline `--password`. An `-env` option takes
the **name of an environment variable**, not its value: use
`--connection-string-env OC_TENANT_CONNECTION_STRING`, never
`--connection-string-env "$OC_TENANT_CONNECTION_STRING"`.

The following are **alternatives**, not a sequence. Select a provider supported
by the host (`Postgres` is shown), a recipe installed there, and a unique table
prefix in `POMI_TABLE_PREFIX`; preserve that prefix across create/setup/retries.
Passwords must meet the [setup policy](setup-password.md). Have the user or
secret manager provide the environment variables or owner-readable secret files.
Do not put connection-string values, including credentials, in shell history or
agent output.

### Install with environment variables

`OC_TENANT_CONNECTION_STRING` contains the complete connection string;
`OC_TENANT_ADMIN_PASSWORD` contains the administrator password.

```bash
pomi --context "$HOST_CONTEXT" tenants install News \
  --request-url-prefix news --recipe-name Blank --site-name "Contoso News" \
  --user-name admin --email admin@example.com --database-provider Postgres \
  --table-prefix "${POMI_TABLE_PREFIX:?Set a unique table prefix first}" \
  --password-env OC_TENANT_ADMIN_PASSWORD \
  --connection-string-env OC_TENANT_CONNECTION_STRING
```

### Install with secret files

Each file contains just its raw secret value, not a JSON object or a shell
assignment. Replace the paths with the actual mounted secret files.

```bash
pomi --context "$HOST_CONTEXT" tenants install News \
  --request-url-prefix news --recipe-name Blank --site-name "Contoso News" \
  --user-name admin --email admin@example.com --database-provider Postgres \
  --table-prefix "${POMI_TABLE_PREFIX:?Set a unique table prefix first}" \
  --password-file /run/secrets/news-admin-password \
  --connection-string-file /run/secrets/news-connection-string
```

### Set up an existing uninitialized tenant with connection-string stdin

```bash
pomi --context "$HOST_CONTEXT" tenants setup News \
  --site-name "Contoso News" --user-name admin --email admin@example.com \
  --database-provider Postgres \
  --table-prefix "${POMI_TABLE_PREFIX:?Set the original unique table prefix}" \
  --password-env OC_TENANT_ADMIN_PASSWORD --connection-string-stdin \
  < /run/secrets/news-connection-string
```

Only **one input can consume stdin**. Do not combine
`--connection-string-stdin` with `--password-stdin` or `--stdin`. The first reads
a raw connection string; `--stdin` reads the entire JSON request body. Complete
JSON can also come from an owner-readable `--body-file`, using the original
`connectionString` and `password` property names. Complete-body inputs cannot
be combined with individual body-property options. Never use inline `--body`
for these secret-bearing commands; it is intentionally unavailable. Existing
host database presets and values saved during tenant creation take precedence.

## Enable management after setup

Skip this step when installation with `--enable-remote-management` already returned
a context. For an existing running tenant or a tenant created with separate
`tenants create` / `tenants setup` commands, provision an application through the
authorized Default-tenant context:

```bash
pomi --context "$HOST_CONTEXT" tenants enable-remote-management News \
  --provision-client --output json > ./News.management.json
# Continue only after the command succeeds.
TENANT_CONTEXT="$(python3 -c 'import json; print(json.load(open("News.management.json"))["context"])')"
pomi context list --output json
pomi --context "$TENANT_CONTEXT" api refresh --output json
pomi --context "$TENANT_CONTEXT" api compatibility --output json
```

Use the returned nonempty context with its saved credentials, without host-only
credential overrides or interactive login. Each `--provision-client` call creates
a new application and context; do not repeat it to renew tokens or retry an
ambiguous response. Inspect the tenant and local contexts first. If a profile
blocks dependencies, correct the profile within the authorized task before retrying.

`tenants setup` has no `--enable-remote-management` option. If install completed but
provisioning failed, inspect the persisted tenant and use the enable command after
fixing the cause, instead of installing again. Without `--provision-client`, the
enable command only configures remote management. Manual human access then follows
[authentication and contexts](authentication.md#connect-to-an-existing-tenant).

Each application is tenant-local. Do not proxy the Default-tenant identity into a
child; use its own returned context for content and design work. The administrator
user remains available for human sign-in with the password in the handoff file.

For host presets, validation, permissions, and complete DTOs, read the versioned
[tenant API reference](https://github.com/OrchardCMS/OrchardCore/blob/main/src/docs/reference/api/tenants/README.md).

## Feature-profile definitions and assignment

Use the [feature-profile workflow](../../orchardcore-cli-automation/SKILL.md#tenant-feature-profiles)
for host-owned definitions. It needs profile-management permission on the Default
tenant. Tenant assignment still uses the existing `featureProfiles` array of profile
IDs. Read the tenant first and preserve its URL and other editable fields when
updating that array. The custom `tenants install` command may not expose profile
assignment; inspect live help and use the supported tenant update contract afterward.
