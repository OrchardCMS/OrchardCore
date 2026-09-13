# Tenant management API

The tenant management API creates, reads, updates, starts, stops, and removes
Orchard Core tenants. It can also create and initialize a tenant in one request, or prepare a running tenant for direct remote
management.

Enable the **Tenants** feature (`OrchardCore.Tenants`) on the `Default` tenant.
This feature is Default-tenant-only and advertises the `tenants` remote
management capability. Remote Management must also be enabled and configured
on the `Default` tenant before a client can authenticate to these endpoints.

## Authentication and authorization

Tenant lifecycle operations use the Orchard Core API authentication scheme and require:

- an authenticated bearer token;
- the **Access remote management API** permission
  (`AccessRemoteManagement`); and
- the **Manage tenants** permission (`ManageTenants`).

In addition, every operation rejects a request made to any tenant other than
`Default`. Obtain a token and configure Remote Management as described in
[Remote Management](../../modules/RemoteManagement/README.md#contexts-and-login).
Antiforgery validation is disabled for these bearer-authenticated endpoints.

Send the token on every request:

```http
Authorization: Bearer <access-token>
```

Tenant removal has one additional server-side requirement:
`OrchardCore:OrchardCore_Tenants:TenantRemovalAllowed` must be `true`. Its
default value is `false`.

## Base route

The routes are relative to the URL of the `Default` tenant:

```text
/api/tenants
```

JSON property names below use the exact camel casing emitted by the API. JSON
responses use `application/json`; Problem Details responses use
`application/problem+json`.

## Endpoint summary

| Operation | Method | Path | Success |
| --- | --- | --- | --- |
| List tenants | `GET` | `/api/tenants` | `200 OK` |
| Get tenant | `GET` | `/api/tenants/{tenantName}` | `200 OK` |
| Create tenant | `POST` | `/api/tenants` | `201 Created`, equivalent retry `200 OK`, conflict `409 Conflict` |
| Install tenant | `POST` | `/api/tenants/{tenantName}:install` | `201 Created` |
| Set up tenant | `POST` | `/api/tenants/{tenantName}:setup` | `200 OK` |
| Update tenant | `PUT` | `/api/tenants/{tenantName}` | `200 OK` |
| Delete tenant | `DELETE` | `/api/tenants/{tenantName}` | `200 OK`, already absent `204 No Content` |
| Start tenant | `POST` | `/api/tenants/{tenantName}:start` | `200 OK` |
| Stop tenant | `POST` | `/api/tenants/{tenantName}:stop` | `200 OK` |
| Enable Remote Management | `POST` | `/api/tenants/{tenantName}:enable-remote-management` | `200 OK` |

## Resource representations

### Tenant

Every operation that returns a tenant uses this representation:

| JSON property | Type | Nullable | Meaning |
| --- | --- | --- | --- |
| `name` | string | No | Tenant name. |
| `tenantId` | string | Yes | Persisted tenant identifier. |
| `state` | string | No | `Uninitialized`, `Initializing`, `Running`, `Disabled`, or `Invalid`. |
| `category` | string | Yes | Administrative category. |
| `description` | string | Yes | Tenant description. |
| `requestUrlHost` | string | No | One or more hosts separated by commas or spaces; empty when none is configured. |
| `requestUrlPrefix` | string | No | Tenant URL prefix; empty when none is configured. |
| `databaseProvider` | string | Yes | Database provider identifier. Built-in values are `Sqlite`, `SqlConnection`, `MySql`, and `Postgres`; the providers registered in the application are authoritative. |
| `connectionString` | string | Yes | Connection string with values whose key is `Password` or `Pwd` replaced by `******`. If an existing value cannot be parsed as a connection string, it is returned unchanged. |
| `tablePrefix` | string | Yes | Database table prefix. |
| `schema` | string | Yes | Database schema. |
| `recipeName` | string | Yes | Setup recipe name. |
| `featureProfiles` | array of string | No | Assigned feature-profile names. |
| `urls` | array of string | No | Concrete URLs derived for the tenant. |
| `primaryUrl` | string | Yes | First URL in `urls`. |
| `setupUrl` | string | Yes | Setup URL for an uninitialized tenant, or `null`. |
| `canStart` | boolean | No | `true` only when `state` is `Disabled`. |
| `canStop` | boolean | No | `true` only when `state` is `Running`. |
| `canDelete` | boolean | No | `true` only when `state` is `Disabled` or `Uninitialized`. This does not reflect the `TenantRemovalAllowed` setting. |

Hosts containing `*`, `{`, or `}` are not used to construct `urls`. If no
concrete configured host remains, the API uses the host of the management
request. Each URL uses the request scheme, the application's original path
base, and the tenant prefix. Duplicate URLs are removed case-insensitively.

For an uninitialized tenant, `setupUrl` contains a time-limited token that
expires 24 hours after the response is generated. It is `null` when no primary
URL or tenant secret is available. Treat this value as a secret.

Representative tenant JSON:

```json
{
  "name": "TenantA",
  "tenantId": "4f6cfd684f764a9d86d4318d47d8732a",
  "state": "Uninitialized",
  "category": "Partners",
  "description": "Partner portal",
  "requestUrlHost": "tenant-a.example.com",
  "requestUrlPrefix": "tenant-a",
  "databaseProvider": "SqlConnection",
  "connectionString": "Server=db;Database=TenantA;User Id=orchard;Password=******",
  "tablePrefix": "TenantA",
  "schema": "dbo",
  "recipeName": "Blog",
  "featureProfiles": [
    "Partner"
  ],
  "urls": [
    "https://tenant-a.example.com/tenant-a"
  ],
  "primaryUrl": "https://tenant-a.example.com/tenant-a",
  "setupUrl": "https://tenant-a.example.com/tenant-a?token=<protected-token>",
  "canStart": false,
  "canStop": false,
  "canDelete": true
}
```

### Tenant operation

Delete and Remote Management configuration return:

| JSON property | Type | Nullable | Meaning |
| --- | --- | --- | --- |
| `name` | string | No | Tenant name. |
| `action` | string | No | `delete` or `enable-remote-management`. |
| `state` | string | No | State after the operation. |
| `url` | string | Yes | Primary tenant URL when the operation supplies one. |

## List tenants

```http
GET /api/tenants
```

Returns all matching tenant settings, including the `Default` tenant, ordered
by tenant name case-insensitively, and then applies paging. Route lookup does
not otherwise exclude `Default` as a target; each operation's state and
configuration constraints still apply.

### Parameters

| Location | Name | Type | Required | Default | Constraints |
| --- | --- | --- | --- | --- | --- |
| Header | `Authorization` | string | Yes | — | `Bearer <access-token>`. |
| Query | `skip` | integer | No | `0` | Zero or greater. |
| Query | `take` | integer | No | `20` | From `1` through `200`. |
| Query | `search` | string | No | — | Case-insensitive substring of `name` or `description`. Empty or whitespace disables this filter. |
| Query | `category` | string | No | — | Case-insensitive exact category. Empty or whitespace disables this filter. |
| Query | `state` | string | No | — | `Uninitialized`, `Initializing`, `Running`, `Disabled`, or `Invalid`. |

There is no request body. Query parameter binding is case-insensitive.

### Request

```bash
curl --get 'https://cms.example.com/api/tenants' \
  --header 'Authorization: Bearer <access-token>' \
  --data-urlencode 'category=Partners' \
  --data-urlencode 'state=Running' \
  --data-urlencode 'skip=0' \
  --data-urlencode 'take=20'
```

### Responses

- `200 OK` returns the paging envelope below.
- `400 Bad Request` is returned for invalid paging or an unparseable query
  value.
- `401 Unauthorized` is returned when bearer authentication is absent or
  fails.
- `403 Forbidden` is returned for a missing permission or when the request is
  not made to the `Default` tenant.

```json
{
  "skip": 0,
  "take": 20,
  "totalCount": 1,
  "items": [
    {
      "name": "TenantA",
      "tenantId": "4f6cfd684f764a9d86d4318d47d8732a",
      "state": "Running",
      "category": "Partners",
      "description": "Partner portal",
      "requestUrlHost": "tenant-a.example.com",
      "requestUrlPrefix": "tenant-a",
      "databaseProvider": "SqlConnection",
      "connectionString": "Server=db;Database=TenantA;User Id=orchard;Password=******",
      "tablePrefix": "TenantA",
      "schema": "dbo",
      "recipeName": "Blog",
      "featureProfiles": [
        "Partner"
      ],
      "urls": [
        "https://tenant-a.example.com/tenant-a"
      ],
      "primaryUrl": "https://tenant-a.example.com/tenant-a",
      "setupUrl": null,
      "canStart": false,
      "canStop": true,
      "canDelete": false
    }
  ]
}
```

`totalCount` is the number of items after filtering but before paging.

Invalid paging returns one of these details:

```json
{
  "title": "Bad request",
  "status": 400,
  "detail": "Skip must be zero or greater and take must be greater than zero."
}
```

```json
{
  "title": "Bad request",
  "status": 400,
  "detail": "Take cannot exceed 200."
}
```

## Get a tenant

```http
GET /api/tenants/{tenantName}
```

### Parameters

| Location | Name | Type | Required | Constraints |
| --- | --- | --- | --- | --- |
| Header | `Authorization` | string | Yes | `Bearer <access-token>`. |
| Path | `tenantName` | string | Yes | Existing tenant name; percent-encode it as one path segment. |

There are no query parameters and no request body.

### Request

```bash
curl 'https://cms.example.com/api/tenants/TenantA' \
  --header 'Authorization: Bearer <access-token>'
```

### Responses

- `200 OK` returns a [tenant](#tenant).
- `401 Unauthorized` is returned when bearer authentication fails.
- `403 Forbidden` is returned for authorization or Default-tenant failure.
- `404 Not Found` is returned when `tenantName` is unknown.

```json
{
  "name": "TenantA",
  "tenantId": "4f6cfd684f764a9d86d4318d47d8732a",
  "state": "Disabled",
  "category": "Partners",
  "description": "Partner portal",
  "requestUrlHost": "tenant-a.example.com",
  "requestUrlPrefix": "tenant-a",
  "databaseProvider": "SqlConnection",
  "connectionString": "Server=db;Database=TenantA;User Id=orchard;Password=******",
  "tablePrefix": "TenantA",
  "schema": "dbo",
  "recipeName": "Blog",
  "featureProfiles": [
    "Partner"
  ],
  "urls": [
    "https://tenant-a.example.com/tenant-a"
  ],
  "primaryUrl": "https://tenant-a.example.com/tenant-a",
  "setupUrl": null,
  "canStart": true,
  "canStop": false,
  "canDelete": true
}
```

```json
{
  "title": "Not Found",
  "status": 404,
  "detail": "Tenant not found: 'MissingTenant'."
}
```

## Create a tenant

```http
POST /api/tenants
Content-Type: application/json
```

Creates shell settings in the `Uninitialized` state. This operation does not
run tenant setup and does not create a user account. Use
[`pomi tenants setup`](#set-up-a-tenant) or open the returned `setupUrl` to
execute the recipe and create the tenant's initial administrator account.

### Parameters

| Location | Name | Type | Required | Constraints |
| --- | --- | --- | --- | --- |
| Header | `Authorization` | string | Yes | `Bearer <access-token>`. |
| Header | `Content-Type` | string | Yes | `application/json`. |

There are no path or query parameters.

### JSON body

| Property | Type | Required | Default | Constraints and behavior |
| --- | --- | --- | --- | --- |
| `name` | string | Yes | `""` when omitted | Nonempty and matched by `^\w+$`. An existing name is evaluated as a retry. |
| `requestUrlHost` | string or null | No | `null` | Hosts are separated by commas or spaces. The host-and-prefix combination must not conflict with another tenant. |
| `requestUrlPrefix` | string or null | No | `null` | At most one segment; `/` is rejected. The host-and-prefix combination must be unique. |
| `category` | string or null | No | `null` | No additional endpoint constraint. |
| `description` | string or null | No | `null` | No additional endpoint constraint. |
| `databaseProvider` | string or null | No | `null` | Must identify a registered provider when connection validation runs. Built-in values are `Sqlite`, `SqlConnection`, `MySql`, and `Postgres`. |
| `connectionString` | string or null | No | `null` | Validated when supplied for a provider that requires it. It may be omitted so the person running setup can supply it. |
| `tablePrefix` | string or null | No | `null` | For providers supporting prefixes, when nonempty it must match `^[A-Za-z_][A-Za-z0-9_]*$`. It is required when `RequireTablePrefix` is enabled. |
| `schema` | string or null | No | `null` | When nonempty it must match `^[A-Za-z_][A-Za-z0-9_]*$`. |
| `recipeName` | string or null | No | `null` | Saved as the setup recipe name; this endpoint does not execute it. |
| `featureProfiles` | array of string | No | `[]` | Every supplied name must identify an existing feature profile. |

The body itself is required. `name` uses the .NET `\w` character class, so
the validator accepts Unicode word characters as well as letters, digits, and
underscore; it does not accept spaces or punctuation.

Server configuration can override submitted database values:

- `TablePrefixPattern` and `SchemaPattern` generate values from the tenant
  name, host, and prefix. Generated values replace submitted values.
- A recognized root/default `DatabaseProvider` replaces the submitted
  provider. When that provider does not require a connection string, or when
  a root/default connection string is nonempty, the root/default connection
  string and schema also replace submitted values.

Retry equivalence is evaluated after these presets and patterns are applied. Name, host, prefix,
database provider, and recipe compare case-insensitively. Category, description, connection
string, table prefix, and schema compare ordinally. Null and empty strings are equivalent.
Feature profiles compare as an order-independent, case-insensitive sequence. Tenant state,
tenant ID, and secret are not compared. Concurrent creation applies the same result: equivalent
effective settings return `200`, while different effective settings return `409`.

### CLI

The CLI exposes each body property as an option while retaining `--body`,
`--body-file`, and `--stdin` for complete JSON payloads:

```bash
pomi tenants create \
  --name TenantA \
  --request-url-prefix tenant-a \
  --database-provider Sqlite \
  --table-prefix TenantA \
  --recipe-name SaaS
```

When the host presets its database provider and generates table prefixes, the
minimum practical command is:

```bash
pomi tenants create --name TenantA --request-url-prefix tenant-a --recipe-name SaaS
```

Run `pomi tenants schema --operation create` to inspect the authoritative JSON
Schema, including required properties and constraints.

After creation:

1. Run `pomi tenants setup TenantA` and supply the site name and initial
   administrator details. The stored `recipeName` is executed during this step.
3. From the Default tenant context, run
   `pomi tenants enable-remote-management TenantA`.
4. Register the initialized tenant using
   `pomi context add tenant-a <primaryUrl> --current`, then run `pomi login`.

### Request

```bash
curl --request POST 'https://cms.example.com/api/tenants' \
  --header 'Authorization: Bearer <access-token>' \
  --header 'Content-Type: application/json' \
  --data '{
    "name": "TenantA",
    "requestUrlHost": "tenant-a.example.com",
    "requestUrlPrefix": "tenant-a",
    "category": "Partners",
    "description": "Partner portal",
    "databaseProvider": "SqlConnection",
    "connectionString": "Server=db;Database=TenantA;User Id=orchard;Password=secret",
    "tablePrefix": "TenantA",
    "schema": "dbo",
    "recipeName": "Blog",
    "featureProfiles": ["Partner"]
  }'
```

### Responses

- `201 Created` returns the created [tenant](#tenant). The `Location` header is
  `/api/tenants/{percent-encoded-name}`.
- `200 OK` returns the existing tenant when an equivalent effective create request is retried.
- `409 Conflict` is returned when that tenant name already exists with
  different effective settings, or the per-tenant create/setup lock cannot be acquired.
- `400 Bad Request` returns Validation Problem Details for invalid fields, or
  Problem Details if validation or persistence fails.
- `415 Unsupported Media Type` is returned when the required body is not sent
  as JSON.
- `401 Unauthorized` is returned when bearer authentication fails.
- `403 Forbidden` is returned for authorization or Default-tenant failure.

```http
HTTP/1.1 201 Created
Content-Type: application/json
Location: /api/tenants/TenantA
```

```json
{
  "name": "TenantA",
  "tenantId": "4f6cfd684f764a9d86d4318d47d8732a",
  "state": "Uninitialized",
  "category": "Partners",
  "description": "Partner portal",
  "requestUrlHost": "tenant-a.example.com",
  "requestUrlPrefix": "tenant-a",
  "databaseProvider": "SqlConnection",
  "connectionString": "Server=db;Database=TenantA;User Id=orchard;Password=******",
  "tablePrefix": "TenantA",
  "schema": "dbo",
  "recipeName": "Blog",
  "featureProfiles": [
    "Partner"
  ],
  "urls": [
    "https://tenant-a.example.com/tenant-a"
  ],
  "primaryUrl": "https://tenant-a.example.com/tenant-a",
  "setupUrl": "https://tenant-a.example.com/tenant-a?token=<protected-token>",
  "canStart": false,
  "canStop": false,
  "canDelete": true
}
```

## Install a tenant

The optional JSON boolean `enableRemoteManagement` (default `false`) enables
Remote Management CLI and its OpenID dependencies after setup, then creates a
confidential application with the target tenant's administrator role. It works
with recipes that do not enable remote management, provided the tenant's feature
profile permits the dependencies. The initial administrator account is still
created with the supplied password.

An opted-in successful response includes `clientCredentials`, containing `clientId`
and `clientSecret`, and uses `Cache-Control: no-store`. The secret is returned only
in this response; ordinary tenant reads never expose it. Store it as a secret and
use the tenant's token endpoint with `grant_type=client_credentials` and scope
`orchardcore.management`. Pomi's `--enable-remote-management` switch saves the
credentials and a unique tenant context, then removes `clientCredentials` from
its output. Omission of the switch preserves normal recipe-driven setup.

If the tenant was installed but a feature profile prevents provisioning, the API
returns `409` with `stage: "remote-management"` and the tenant name. Correct the
profile and call the enable endpoint below; do not reinstall the tenant.


```http
POST /api/tenants/{tenantName}:install
Content-Type: application/json
```

Creates a new tenant and immediately runs setup, including the recipe and
initial administrator account. Use this operation when the tenant should be
ready to use in one command. Unlike `pomi install`, it uses an existing Orchard
application selected by the current context; it does not create a local project,
require a local .NET SDK, or start another server. It uses the server's setup
service directly and does not require the Auto Setup feature.

### CLI

Select an authenticated context for the **Default** tenant, then run:

```bash
pomi tenants install Blog \
  --request-url-prefix blog \
  --recipe-name Blog \
  --site-name "My Blog" \
  --user-name admin \
  --email admin@example.com \
  --site-time-zone Europe/Paris
```

The CLI rejects passwords that do not meet the standard setup policy before
sending an install/setup request: at least six characters, including ASCII
uppercase, lowercase, a digit, and a non-alphanumeric character. The server
also checks its configured Identity password length, character groups, and
unique-character requirements **before creating the tenant**; rejected passwords
return `400` with a `Password` validation error and leave no tenant behind.
See the [setup password guide and compliant generator](../../../agents/skills/orchardcore-cli/references/setup-password.md).

The CLI prompts securely for the password. For automation, use
`--password-env ADMIN_PASSWORD`, `--password-file <path>`, or
`--password-stdin`. Connection strings use the corresponding
`--connection-string-env`, `--connection-string-file`, and
`--connection-string-stdin` options. Only one value can consume standard input.
Schema properties are JSON field names, not generated option names.
For example, `connectionString` maps to those three secret-safe options, **not**
`--connection-string`. `--connection-string-env` takes the environment variable's
name, not its expanded value. Use `pomi tenants install --help` to inspect the
available CLI options. The [tenant guide](../../../agents/skills/orchardcore-cli/references/tenants.md#schema-properties-and-secret-cli-options)
provides complete environment-variable, secret-file, and stdin examples.

The [setup secret input rules](#set-up-a-tenant) also apply here: complete JSON
is accepted through `--stdin` or `--body-file`, and inline secret values are
not exposed as command options.

The human response reports success, the running state, and the full site URL.
Without `--enable-remote-management`, it suggests `pomi tenants enable-remote-management Blog` as a separate next step.
Installation does not create a CLI context or acquire credentials for the new
administrator. Remote Management is enabled only if the selected recipe does
so or you explicitly enable it later. Use `--output json` for structured output.

If the command is missing after updating the server, run `pomi api refresh --force`.
The command is discovered from the server's OpenAPI description.

### Parameters and body

`tenantName` is the required path parameter and uses the same name restrictions
as [tenant creation](#create-a-tenant). It is the first CLI argument, not a
`name` property in the JSON body.

| Property | Type | Required | Behavior |
| --- | --- | --- | --- |
| `siteName` | string | Yes | Site display name. |
| `userName` | string | Yes | Initial administrator user name. |
| `email` | string | Yes | Valid initial administrator email address. |
| `password` | string | Yes | Initial administrator password; existing password policies apply. |
| `recipeName` | string or null | For setup | Available setup recipe, such as `Blog`, `SaaS`, or `Blank`; must resolve after host configuration. |
| `siteTimeZone` | string or null | No | IANA/TZDB time zone, such as `Europe/Paris`, `America/Los_Angeles`, or `UTC`; defaults to the server time zone. |
| `requestUrlHost` | string or null | No | Tenant host binding, with the same rules as creation. |
| `requestUrlPrefix` | string or null | No | Single URL path segment. Together with the host it must identify an available tenant route. |
| `category` | string or null | No | Administrative category. |
| `description` | string or null | No | Administrative description. |
| `featureProfiles` | array of string | No | Assigned feature profiles; defaults to an empty array. |
| `databaseProvider` | string or null | No | Defaults to `Sqlite` when omitted. An explicit registered provider can be selected; host presets take precedence. |
| `connectionString` | string or null | Provider-dependent | Database connection string; host presets take precedence. |
| `tablePrefix` | string or null | Host-dependent | Table prefix; host requirements and patterns apply. |
| `schema` | string or null | No | Database schema; host presets and patterns apply. |

When no database provider is specified, installation uses SQLite, matching
`pomi install`. A database provider configured by the host takes precedence over
this default. Select another provider with `--database-provider` and provide
its required connection settings.

The creation and setup operations' validation and database precedence rules
apply unchanged. For example, a host requiring a table prefix also requires one
here unless it supplies a pattern.

An HTTPS request with bearer authentication can send this JSON body:

```json
{
  "requestUrlPrefix": "blog",
  "databaseProvider": "Sqlite",
  "recipeName": "Blog",
  "siteName": "My Blog",
  "userName": "admin",
  "email": "admin@example.com",
  "password": "<administrator-password>",
  "siteTimeZone": "Europe/Paris"
}
```

### Responses and retries

- `201 Created` returns the complete [tenant representation](#tenant), with
  `state: "Running"`, its `primaryUrl`, and `setupUrl: null`. The `Location`
  header is `/api/tenants/{percent-encoded-name}`. No password or token is returned.
- `409 Conflict` means the name already exists (in any state), or the per-tenant
  create/setup lock could not be acquired. An identical retry also returns `409`;
  it does not rerun the recipe or change the existing administrator.
- `400 Bad Request` reports invalid creation or setup input, setup-component
  validation errors, or creation persistence errors.
- `500 Internal Server Error` reports an exception during setup without returning
  internal exception details.
- `404 Not Found` can occur if the tenant disappears between creation and setup.
- `401 Unauthorized`, `403 Forbidden`, and `415 Unsupported Media Type` have the
  same meaning as on the other tenant endpoints.

Creation and setup are **not a single database transaction**. They share the
per-tenant distributed lock with the separate management `create` and `setup`
operations, but setup recipes can make partial progress. If setup fails after
creation, the error includes `tenantName` and `stage: "setup"`, and the tenant
is preserved. Inspect it with `pomi tenants show Blog`; if it is `Uninitialized`,
inspect whether the recipe or database has made partial progress before
retrying `pomi tenants setup Blog` with corrected inputs. An `Uninitialized`
shell state alone does not guarantee that rerunning setup is safe. Do not automatically delete a partially initialized tenant.
If the response is lost or times out, inspect the tenant state before taking
further action.

Example setup failure:

```json
{
  "title": "A validation error occurred.",
  "status": 400,
  "detail": "The tenant was created, but setup did not complete. Inspect its state and use the separate setup command if it is uninitialized.",
  "errors": { "Password": ["Password does not meet requirements."] },
  "tenantName": "Blog",
  "stage": "setup"
}
```

## Set up a tenant

```http
POST /api/tenants/{tenantName}:setup
Content-Type: application/json
```

Initializes an `Uninitialized` tenant, creates its administrator account, and
executes its setup recipe. Database and recipe values saved by tenant creation
take precedence over values in this request. Server database presets and tenant
table-prefix or schema patterns also apply. When `siteTimeZone` is omitted, the
server time zone is used.

Password complexity is checked before starting the setup recipe, using the
same rules as tenant installation. Custom modules or recipes may enforce
additional policies during user creation; early validation cannot predict
those rules or prevent unrelated recipe failures.

The command prompts for the password without echoing it when run interactively:

```bash
pomi tenants setup TenantA \
  --site-name "Tenant A" \
  --user-name admin \
  --email admin@example.com
```

For automation, provide exactly one secret source:

```bash
pomi tenants setup TenantA \
  --site-name "Tenant A" \
  --user-name admin \
  --email admin@example.com \
  --password-env OC_TENANT_ADMIN_PASSWORD
```

`--password-stdin` reads the password from standard input, and
`--password-file <path>` reads it from a file. A single trailing line ending is
removed from standard-input and file values. Prefer the masked prompt for
interactive use, an environment variable supplied by the CI secret store for
automation, or a short-lived owner-readable file when a platform mounts secrets
as files. Avoid inline `--body` values because command lines can be retained in
shell history and process listings; `--body` is therefore not exposed for this
secret-bearing operation. A complete JSON body can still be read safely from
`--stdin` or `--body-file`.

### JSON body

| Property | Type | Required | Behavior |
| --- | --- | --- | --- |
| `siteName` | string | Yes | Site name. |
| `userName` | string | Yes | Initial administrator user name. |
| `email` | string | Yes | Valid initial administrator email address. |
| `password` | string | Yes | Initial administrator password. Secret CLI input rules apply. |
| `recipeName` | string or null | No | Used when tenant creation did not save a recipe. |
| `siteTimeZone` | string or null | No | Defaults to the server time zone. |
| `databaseProvider` | string or null | No | Used when tenant creation did not save a provider. |
| `connectionString` | string or null | No | Used when tenant creation did not save a connection string. |
| `tablePrefix` | string or null | No | Used when tenant creation did not save a prefix. |
| `schema` | string or null | No | Used when tenant creation did not save a schema. |

A successful request returns the initialized [tenant](#tenant). Retrying after
the tenant reaches `Running` returns that tenant without executing setup again.
Setup requests for the same tenant are serialized with a distributed lock; a
request that cannot acquire the lock returns `409 Conflict`.
Unknown tenants return `404`; tenants in another transitional state return
`409`; invalid input or setup-component errors return Validation Problem
Details. Authentication and Default-tenant authorization requirements are the
same as other tenant management operations.

## Update a tenant

```http
PUT /api/tenants/{tenantName}
Content-Type: application/json
```

The path selects the tenant; its name cannot be changed. This is replacement
semantics for the mutable settings: omitted strings become `null`, and omitted
`featureProfiles` becomes an empty array.

`description`, `category`, `requestUrlPrefix`, `requestUrlHost`, and
`featureProfiles` are updated in every state. Database settings
(`databaseProvider`, `connectionString`, `tablePrefix`, `schema`, and
`recipeName`) are updated only while the tenant is `Uninitialized`. For other
states, submitted database values are not connection-validated and are not
persisted, although the shared table-prefix, schema, and configured-pattern
validation still runs. A returned redacted connection string can be sent back
unchanged to retain the stored password.

### Parameters

| Location | Name | Type | Required | Constraints |
| --- | --- | --- | --- | --- |
| Header | `Authorization` | string | Yes | `Bearer <access-token>`. |
| Header | `Content-Type` | string | Yes | `application/json`. |
| Path | `tenantName` | string | Yes | Existing tenant name; percent-encode it as one path segment. |

There are no query parameters. The JSON body is required and has every
property from [Create a tenant](#json-body) except `name`; all properties are
optional. The same configuration overrides apply, while database connection
validation is limited to an `Uninitialized` tenant as described above.

Repeating a successful replacement converges the mutable settings to the same effective values
and returns `200`. Settings that are immutable in the tenant's current state remain unchanged on
every retry.

### Request

```bash
curl --request PUT 'https://cms.example.com/api/tenants/TenantA' \
  --header 'Authorization: Bearer <access-token>' \
  --header 'Content-Type: application/json' \
  --data '{
    "requestUrlHost": "tenant-a.example.com",
    "requestUrlPrefix": "tenant-a",
    "category": "Customers",
    "description": "Updated partner portal",
    "databaseProvider": "SqlConnection",
    "connectionString": "Server=db;Database=TenantA;User Id=orchard;Password=******",
    "tablePrefix": "TenantA",
    "schema": "dbo",
    "recipeName": "Blog",
    "featureProfiles": ["Partner"]
  }'
```

### Responses

- `200 OK` returns the updated [tenant](#tenant).
- `400 Bad Request` returns Validation Problem Details or a persistence error.
- `415 Unsupported Media Type` is returned when the required body is not sent
  as JSON.
- `401 Unauthorized` is returned when bearer authentication fails.
- `403 Forbidden` is returned for authorization or Default-tenant failure.
- `404 Not Found` is returned when `tenantName` is unknown.

```json
{
  "name": "TenantA",
  "tenantId": "4f6cfd684f764a9d86d4318d47d8732a",
  "state": "Running",
  "category": "Customers",
  "description": "Updated partner portal",
  "requestUrlHost": "tenant-a.example.com",
  "requestUrlPrefix": "tenant-a",
  "databaseProvider": "SqlConnection",
  "connectionString": "Server=db;Database=TenantA;User Id=orchard;Password=******",
  "tablePrefix": "TenantA",
  "schema": "dbo",
  "recipeName": "Blog",
  "featureProfiles": [
    "Partner"
  ],
  "urls": [
    "https://tenant-a.example.com/tenant-a"
  ],
  "primaryUrl": "https://tenant-a.example.com/tenant-a",
  "setupUrl": null,
  "canStart": false,
  "canStop": true,
  "canDelete": false
}
```

## Start a tenant

```http
POST /api/tenants/{tenantName}:start
```

Converges a `Disabled` or already `Running` tenant to `Running`.

### Parameters

| Location | Name | Type | Required | Constraints |
| --- | --- | --- | --- | --- |
| Header | `Authorization` | string | Yes | `Bearer <access-token>`. |
| Path | `tenantName` | string | Yes | Existing disabled or running tenant name; percent-encode it as one path segment before the `:start` suffix. |

There are no query parameters and no request body.

### Request

```bash
curl --request POST 'https://cms.example.com/api/tenants/TenantA:start' \
  --header 'Authorization: Bearer <access-token>'
```

### Responses

- `200 OK` returns the tenant with `state` set to `Running`,
  `canStart: false`, and `canStop: true`.
- `400 Bad Request` is returned unless the current state is `Disabled` or
  already `Running`. An already-running tenant returns `200` without another
  transition.
- `401 Unauthorized`, `403 Forbidden`, and `404 Not Found` have the meanings
  described above.

```json
{
  "name": "TenantA",
  "tenantId": "4f6cfd684f764a9d86d4318d47d8732a",
  "state": "Running",
  "category": "Partners",
  "description": "Partner portal",
  "requestUrlHost": "tenant-a.example.com",
  "requestUrlPrefix": "tenant-a",
  "databaseProvider": "SqlConnection",
  "connectionString": "Server=db;Database=TenantA;User Id=orchard;Password=******",
  "tablePrefix": "TenantA",
  "schema": "dbo",
  "recipeName": "Blog",
  "featureProfiles": [],
  "urls": [
    "https://tenant-a.example.com/tenant-a"
  ],
  "primaryUrl": "https://tenant-a.example.com/tenant-a",
  "setupUrl": null,
  "canStart": false,
  "canStop": true,
  "canDelete": false
}
```

```json
{
  "title": "Bad request",
  "status": 400,
  "detail": "You can only start a disabled tenant."
}
```

## Stop a tenant

```http
POST /api/tenants/{tenantName}:stop
```

Converges a `Running` or already `Disabled` tenant to `Disabled`.

### Parameters

| Location | Name | Type | Required | Constraints |
| --- | --- | --- | --- | --- |
| Header | `Authorization` | string | Yes | `Bearer <access-token>`. |
| Path | `tenantName` | string | Yes | Existing running or disabled tenant name; percent-encode it as one path segment before the `:stop` suffix. |

There are no query parameters and no request body.

### Request

```bash
curl --request POST 'https://cms.example.com/api/tenants/TenantA:stop' \
  --header 'Authorization: Bearer <access-token>'
```

### Responses

- `200 OK` returns the tenant with `state` set to `Disabled`,
  `canStart: true`, `canStop: false`, and `canDelete: true`.
- `400 Bad Request` is returned unless the current state is `Running` or
  already `Disabled`. An already-disabled tenant returns `200` without another
  transition.
- `401 Unauthorized`, `403 Forbidden`, and `404 Not Found` have the meanings
  described above.

```json
{
  "name": "TenantA",
  "tenantId": "4f6cfd684f764a9d86d4318d47d8732a",
  "state": "Disabled",
  "category": "Partners",
  "description": "Partner portal",
  "requestUrlHost": "tenant-a.example.com",
  "requestUrlPrefix": "tenant-a",
  "databaseProvider": "SqlConnection",
  "connectionString": "Server=db;Database=TenantA;User Id=orchard;Password=******",
  "tablePrefix": "TenantA",
  "schema": "dbo",
  "recipeName": "Blog",
  "featureProfiles": [],
  "urls": [
    "https://tenant-a.example.com/tenant-a"
  ],
  "primaryUrl": "https://tenant-a.example.com/tenant-a",
  "setupUrl": null,
  "canStart": true,
  "canStop": false,
  "canDelete": true
}
```

```json
{
  "title": "Bad request",
  "status": 400,
  "detail": "You can only stop a running tenant."
}
```

## Delete a tenant

```http
DELETE /api/tenants/{tenantName}
```

Removes an `Uninitialized` or `Disabled` tenant through the shell removal
pipeline. The operation is available only when
`OrchardCore:OrchardCore_Tenants:TenantRemovalAllowed` is `true`.

### Parameters

| Location | Name | Type | Required | Constraints |
| --- | --- | --- | --- | --- |
| Header | `Authorization` | string | Yes | `Bearer <access-token>`. |
| Path | `tenantName` | string | Yes | Existing uninitialized or disabled tenant name; percent-encode it as one path segment. |

There are no query parameters and no request body.

### Request

```bash
curl --request DELETE 'https://cms.example.com/api/tenants/TenantA' \
  --header 'Authorization: Bearer <access-token>'
```

### Responses

- `200 OK` returns a tenant-operation response. Its reported state is
  `Disabled` for both removable source states, and `url` is `null`.
- `400 Bad Request` is returned for a non-removable state or when the removal
  pipeline fails.
- `401 Unauthorized` is returned when bearer authentication fails.
- `403 Forbidden` is returned for normal authorization failures and when
  tenant removal is disabled.
- `204 No Content` is returned when `tenantName` is already absent.

```json
{
  "name": "TenantA",
  "action": "delete",
  "state": "Disabled",
  "url": null
}
```

```json
{
  "title": "Bad request",
  "status": 400,
  "detail": "You can only delete a disabled or uninitialized tenant."
}
```

When the removal pipeline itself fails, the response title is
`An error occurred while deleting tenant 'TenantA'.` and `detail` is the
pipeline's error message.

## Enable Remote Management for a tenant

The optional query boolean `provisionClient=true` also creates a new administrative
confidential application and returns its `clientCredentials` once, with
`Cache-Control: no-store`. Omission preserves the enable/configure-only behavior.
Pomi exposes this as `--provision-client`, stores the credentials in a new context,
and omits the secret from its output. Each request creates a separate application;
it never overwrites a public CLI client or a previously provisioned application.


```http
POST /api/tenants/{tenantName}:enable-remote-management
```

The target tenant must be `Running`. The operation enables
`OrchardCore.RemoteManagement.Cli` with dependencies, verifies that the feature
was enabled, configures the tenant's OpenID Connect server and local token
validation, creates or updates the `orchardcore.management` scope and
`orchardcore-cli` public native application, grants **Access remote management
API** to administrator roles, and reloads the tenant shell.

Repeating a successful request revalidates and reapplies the same feature, OpenID Connect,
permission, and application configuration without creating duplicate scope or application
records, and returns `200`.

This does not authenticate the Default-tenant caller as a user of the target
tenant. Register the returned URL as a separate CLI context and authenticate
directly to that tenant.

### Parameters

| Location | Name | Type | Required | Constraints |
| --- | --- | --- | --- | --- |
| Header | `Authorization` | string | Yes | `Bearer <access-token>`. |
| Path | `tenantName` | string | Yes | Existing running tenant name; percent-encode it as one path segment before the `:enable-remote-management` suffix. |

There are no query parameters and no request body.

### Request

```bash
curl --request POST \
  'https://cms.example.com/api/tenants/TenantA:enable-remote-management' \
  --header 'Authorization: Bearer <access-token>'
```

### Responses

- `200 OK` returns a tenant-operation response.
- `400 Bad Request` is returned if Remote Management cannot be enabled,
  including when the active feature profile excludes it.
- `401 Unauthorized` and `403 Forbidden` have the meanings described above.
- `404 Not Found` is returned when `tenantName` is unknown.
- `409 Conflict` is returned unless the target tenant is `Running`.

```json
{
  "name": "TenantA",
  "action": "enable-remote-management",
  "state": "Running",
  "url": "https://tenant-a.example.com/tenant-a"
}
```

```json
{
  "title": "Conflict",
  "status": 409,
  "detail": "Tenant 'TenantA' must be running before remote management can be enabled."
}
```

```json
{
  "title": "Bad request",
  "status": 400,
  "detail": "Remote management could not be enabled for tenant 'TenantA'. Check its active feature profile."
}
```

## Legacy tenant API compatibility

The controller also retains the older `POST /api/tenants/create`,
`POST /api/tenants/edit`, `POST /api/tenants/disable/{tenantName}`,
`POST /api/tenants/enable/{tenantName}`, `POST /api/tenants/remove/{tenantName}`, and
`POST /api/tenants/setup` routes. They use the older controller contract rather than the
`AccessRemoteManagement` endpoint convention, but still require the `Api` authentication scheme,
the Default tenant, and `ManageTenants`.

Legacy enable and disable are convergent and return `200` when the tenant is already in the
requested state. Legacy remove returns `204` when the tenant is already absent. Legacy edit
preserves the existing tenant secret and generates one only when an uninitialized tenant has no
secret. Legacy create returns `200` for initial creation and `201` with a stable setup URL when an
existing tenant is retried. These compatibility routes are not part of the eight management
routes summarized above.

## Validation and common errors

Create and update can return these source-defined validation conditions:

- a missing tenant name or an invalid `^\w+$` name;
- a nonexistent feature profile;
- a prefix containing `/`, or a host-and-prefix combination already assigned
  to another tenant;
- an invalid configured table-prefix or schema pattern;
- a required or syntactically invalid table prefix, or a syntactically invalid
  schema;
- an unsupported database provider, unreachable or invalid connection,
  untrusted server certificate, or database/prefix/schema already in use; and
- an existing SQLite database file already in use.

The connection validator runs for a new tenant and for an update to an
uninitialized tenant. For a provider that requires a connection string, it is
skipped when that string is empty so setup can collect it later.

Persistence failures return:

```json
{
  "title": "Bad request",
  "status": 400,
  "detail": "An error occurred while saving the tenant settings."
}
```

A request to a non-Default tenant has this response:

```json
{
  "title": "Forbidden",
  "status": 403,
  "detail": "Only the Default tenant can manage tenants."
}
```

A handler-level permission failure has this response:

```json
{
  "title": "Forbidden",
  "status": 403,
  "detail": "You do not have sufficient permissions to complete this request"
}
```

Authentication-policy failures occur before the handler. The bearer
authentication handler supplies the `401` or policy-level `403` response, so
those failures do not have an endpoint-defined JSON body.

## Feature-profile definitions

Enable `OrchardCore.Tenants.FeatureProfiles` on the Default tenant to manage profile
definitions. These operations advertise the separate `tenant-feature-profiles`
capability and require API bearer authentication, `AccessRemoteManagement` and
`ManageTenantFeatureProfiles`. They reject child-tenant calls, including in-process
MCP invocation. Tenant lifecycle permissions do not substitute for profile-management
permission.

| Method | Route | Pomi command |
| --- | --- | --- |
| GET | `api/tenants/feature-profiles` | `tenants feature-profiles list` |
| GET | `api/tenants/feature-profiles/by-id?id=...` | `tenants feature-profiles show <id>` |
| GET | `api/tenants/feature-profiles/schema` | `tenants feature-profiles schema` |
| POST | `api/tenants/feature-profiles` | `tenants feature-profiles create` |
| PUT | `api/tenants/feature-profiles/by-id?id=...` | `tenants feature-profiles update <id>` |
| DELETE | `api/tenants/feature-profiles/by-id?id=...` | `tenants feature-profiles delete <id>` |

Create and update accept a complete JSON definition through `--body-file` or `--stdin`:

```json
{
  "id": "standard",
  "name": "Standard tenants",
  "featureRules": [
    { "rule": "Exclude", "expression": "Custom.*" },
    { "rule": "Include", "expression": "Custom.Allowed" }
  ]
}
```

`id` is a case-insensitive, immutable assignment identifier; it cannot contain commas
or surrounding whitespace. `name` is an editable display name and must not duplicate
another profile's display name. Both are required by the API. Legacy recipe definitions
may omit ID/name and continue using the dictionary key as their fallback.

Rules retain their order: later matching rules determine the result. Use `schema` to
find registered rule names and the rule-array schema. Expressions belong to their
rule provider; the built-in Include/Exclude rules support wildcard matching. Updating
replaces the display name and rule array; omission or `[]` clears all rules, while
explicit null, unknown rule names and empty expressions are rejected. Unknown JSON
properties are also rejected. Shared validation applies to admin and recipe updates.

List supports `search` across ID/display name, `skip` (default 0), and `take` (default
50, maximum 200). It returns `items`, `totalCount`, `skip` and `take`. Show and update
return `404` when the identifier is absent. Create returns `201`, an equivalent retry
returns `200`, and a conflicting definition under the same identifier returns `409`.
Equivalent updates avoid another save. Delete returns `204`, including on repetition;
use `--force` to suppress Pomi's deletion confirmation during automation.

Assign the identifier through the existing tenant `featureProfiles` field. Definition
edits and deletion do not rewrite assignments or automatically disable already enabled
features. A missing assigned profile does not restrict feature selection in the existing
validator. Deleting a profile can therefore remove its restriction on future feature
selection; it is not a way to disable a tenant or its installed features.

With MCP enabled, these operations project to `tenants_feature-profiles_list`,
`tenants_feature-profiles_show`, `tenants_feature-profiles_schema`,
`tenants_feature-profiles_create`, `tenants_feature-profiles_update` and
`tenants_feature-profiles_delete`, using the same handlers and permissions.

## Endpoint coverage and sources

The tenant lifecycle reference covers all **8** routes mapped by
`TenantManagementEndpoints.AddTenantManagementEndpoints`. It was derived from
`TenantManagementEndpoints.cs`, `TenantInstallEndpoints.cs`, `TenantModelBase.cs`, `TenantValidator.cs`,
`TenantDatabasePatternResolver.cs`, `TenantConnectionStringRedactor.cs`,
`TenantRemoteManagementCapabilityProvider.cs`, tenant and shell settings/state
extensions, `TenantRemoteManagementConfigurationService.cs`, endpoint metadata
tests, and tenant validator/response/path tests.

Feature-profile definitions are mapped separately by `FeatureProfileManagementEndpoints`
and reuse `FeatureProfilesManager`, the admin editor, recipe step and registered rule schema.
