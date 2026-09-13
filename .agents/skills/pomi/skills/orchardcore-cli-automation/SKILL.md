---
name: orchardcore-cli-automation
description: Manages Orchard Core features, tenant feature profiles, deployment plans, package exports/imports, recipes, queries, Lucene index definitions, workflows, users, roles, and OpenID applications, scopes and shared-secret credentials through `pomi`. Use for enabling module capabilities, executing recipes, defining/executing SQL or other queries, managing workflow types and instances, and provisioning tenant-local security principals and permissions.
---

# Pomi CLI Automation

Before issuing commands, read the [shared context, authentication, and output rules](../orchardcore-cli/references/shared-rules.md).
For first-time access or login problems, follow [authentication and contexts](../orchardcore-cli/references/authentication.md).
These rules apply even when this specialist is selected directly.

Use this skill after selecting the exact tenant context. For automated creation,
follow the main CLI skill's installation flow with `--enable-remote-management`
and use its returned context without `pomi login`. Retain the separate private
administrator credential file for the user. Refresh OpenAPI after feature changes
because commands and schemas are dynamic.

## Features

```bash
pomi features list --search Media --skip 0 --take 200
pomi features show OrchardCore.Media
pomi features enable OrchardCore.Media
pomi features disable OrchardCore.Media --force
pomi api refresh --force
```

`--force` skips the CLI confirmation only. The feature API's separate
`--api-force true` option includes missing dependencies (enable) or enabled
dependents (disable); use it only when those additional changes are authorized.
For example, `pomi features disable OrchardCore.Media --force` keeps dependency
checks enabled. Never add `--api-force true` merely to avoid a prompt.
Re-read the feature state after mutation.

## Tenant feature profiles

Manage definitions from the Default tenant with `OrchardCore.Tenants.FeatureProfiles`
enabled and both `AccessRemoteManagement` and `ManageTenantFeatureProfiles` permissions.
Tenant lifecycle permission alone is insufficient. Refresh the host OpenAPI first.

```bash
pomi tenants feature-profiles list --take 200
pomi tenants feature-profiles schema
pomi tenants feature-profiles show standard
pomi tenants feature-profiles create --body-file profile.json
pomi tenants feature-profiles update standard --body-file profile.json
pomi tenants feature-profiles delete standard --force
```

Bodies contain `id`, `name` and ordered `featureRules` objects with `rule` and
`expression`. IDs are case-insensitive assignment keys and cannot change or contain
commas/surrounding whitespace. Names are editable and must be unique. Discover rule
names through `schema`; preserve order because later matching rules take precedence.
Updates replace all editable values; omitted rules or `[]` clears them, while null
and unknown rule names are rejected. Equivalent create retries succeed; a different
definition under the same ID conflicts. Repeated deletion succeeds.

Assign existing IDs through the tenant API's `featureProfiles` field, using the
[tenant update workflow](../orchardcore-cli/references/tenants.md) and preserving the
other tenant fields. Do not assume `tenants install` has a profile flag: inspect live
help. Profile edits/deletion do not rewrite assignments or disable installed features.
A missing assigned profile removes its restriction on future feature selection;
deleting a profile is not tenant shutdown. Read back both the host definition and
the child's feature eligibility after a policy change.

See the [profile API contract](https://github.com/sebastienros/OrchardCore/blob/d36ce61633a7ed819773f07744b9314066bb599f/src/docs/reference/api/tenants/README.md#feature-profile-definitions).

## Recipes

```bash
pomi recipes list --search setup --take 200
pomi recipes show <recipe-id>
pomi recipes schema --operation execute
pomi recipes execute <recipe-id> --body-file parameters.json --force
```

Use the opaque case-sensitive ID returned by list/show. Recipe execution starts
a new execution and is not generally retry-idempotent; do not automatically
repeat it after an ambiguous failure.

## Queries

```bash
pomi queries sources list --take 200
pomi queries schema --operation create
pomi queries validate --body-file query.json
pomi queries create --body-file query.json
pomi queries show RecentNews
pomi queries update RecentNews --body-file query.json
pomi queries execute RecentNews --body-file parameters.json
pomi queries delete RecentNews --force
```

Read the selected source's schema before creating a query. Keep paging,
publication state, taxonomy filters, ordering, and projection in the query
rather than duplicating them in Liquid.

Query updates are full semantic replacements. Preserve `returnContentItems`
explicitly: omitting this non-nullable boolean changes it to `false`.

## Index discovery and Lucene definitions

Select the exact tenant context. Common discovery requires `OrchardCore.Indexing`;
typed content definitions require `OrchardCore.Lucene` and content support. Both
require `AccessRemoteManagement` and `ManageIndexes`. Enable the necessary features
and authorized dependencies, then refresh discovery. Do not infer provider write
support merely from its appearance in the provider list.

```bash
pomi indexes providers list
pomi indexes list --page 1 --page-size 50
pomi indexes show INDEX_ID
pomi indexes lucene analyzers
pomi indexes lucene create --body-file index.json
pomi indexes lucene show INDEX_ID
pomi indexes lucene update INDEX_ID --body-file index.json
pomi indexes lucene delete INDEX_ID --force
```

Minimal `index.json`, after defining the `Article` content type:

```json
{"name":"Articles","indexName":"articles","indexedContentTypes":["Article"]}
```

Responses contain `id` and `definition`. For updates, preserve the returned
`definition` and change only intended fields: omitted optional values reset to
their defaults, rather than preserving the old values. The administrative `name`
is also referenced by named queries and search settings; renaming does not rewrite
those references. The provider `indexName` is immutable and must be a single valid
filename. Arbitrary private profile properties are not part of this contract.

Discover analyzer names instead of guessing. Defaults are all cultures (`any`),
published content, source storage off, `standardanalyzer` for indexing/querying,
Lucene query syntax off, compatibility version `LUCENE_48`, and the full-text field.
Defined legacy compatibility versions remain accepted. Explicit null/blank settings
are rejected; an empty selected query-field list is allowed, but content types must
be nonempty and unique.

Equivalent create/update retries are no-ops; conflicting creates require inspection.
Deletion of an already missing profile succeeds. `--force` confirms deletion locally;
it does not force server-side removal after provider failure. Do not automatically
retry uncertain provider or post-persistence handler failures.

Creation schedules synchronization. It does not prove indexing has completed, and
updating a definition does not automatically rebuild existing documents. Enable
`OrchardCore.Indexing.Worker` for ongoing content updates. Verify results through an
existing named Lucene query before claiming the index is ready; when testing content
updates, verify changed indexed terms, not merely freshly loaded database content.
Discover `lifecycleActions` on each provider/source before requesting lifecycle
work. Lucene content indexes support:

```bash
pomi indexes synchronize <index-id>
pomi indexes reset <index-id> --force
pomi indexes rebuild <index-id> --force
pomi indexes operations show <operation-id>
```

Capture the returned operation ID and poll `operations show` with a bounded wait.
`Pending` and `Running` are not completion. `Completed` confirms that no further
queued tasks were observed after successful processing; subsequent content changes
can require another synchronization. Inspect `outcome` on `Failed` before deciding
whether to retry. `Uncertain` means the server cannot confirm progress, not that
work was cancelled: never automatically repeat it. `LockExpired` means a provider
call may have outlived the lock lease; inspect the index before requesting more work. Repeated requests create new
operations. No token refresh or successful HTTP 202 proves the index is ready.

Reset reprocesses the queue without recreating the index. Rebuild recreates it;
use rebuild when changing selected content types must remove old indexed documents.
These operations use the same coordinator as admin and recipe actions. Ongoing
scheduled updates still require the worker feature. Other providers' typed
definitions and lifecycle adapters are not covered by the Lucene contract.

See the [Lucene definition contract](https://github.com/sebastienros/OrchardCore/blob/738a7c14b4e1ac8a0418fc11967c681b6ac444cd/src/docs/reference/modules/Lucene/README.md#remote-content-index-definitions)
and [shared index coordination](https://github.com/sebastienros/OrchardCore/blob/738a7c14b4e1ac8a0418fc11967c681b6ac444cd/src/docs/reference/modules/Indexing/README.md#coordinating-profiles-and-provider-resources).
See also the [operation status contract](https://github.com/sebastienros/OrchardCore/blob/738a7c14b4e1ac8a0418fc11967c681b6ac444cd/src/docs/reference/modules/Indexing/README.md#remote-lifecycle-requests).

## Workflows

```bash
pomi workflow activity-types list --take 200
pomi workflow activity-types show <activity-name>
pomi workflow types schema --operation create
pomi workflow types validate --body-file workflow.json
pomi workflow types create --body-file workflow.json
pomi workflow types show <workflow-type-id>
pomi workflow types enable <workflow-type-id>
pomi workflow types execute <workflow-type-id> --body-file input.json
pomi workflow instances list --workflow-type-id <workflow-type-id>
pomi workflow instances show <workflow-id>
pomi workflow instances cancel <workflow-id> --force
```

Use singular `workflow` in command groups. Discover activity schemas from the
target tenant; enabled modules contribute activity types and properties.
Workflow execution starts a new instance and may not be safe to retry.
Workflow DTOs generally use camelCase, unlike PascalCase content-item
properties. Preserve the casing in the live operation/activity schemas; do
not copy content-item casing into workflow payloads.

## Users and roles

```bash
pomi users schema --operation create
pomi users create --body-file user.json
pomi users list --search editor --role Editor --take 200
pomi users show <user-id>
pomi users update <user-id> --body-file user.json
pomi users disable <user-id> --force
pomi users delete <user-id> --force

pomi roles schema --operation create
pomi roles create --body-file role.json
pomi roles list --search Content --take 200
pomi roles show <role-id>
pomi roles update <role-id> --body-file role.json
pomi roles delete <role-id> --force
```

Apply least privilege. Every API still requires `AccessRemoteManagement`; grant
only resource permissions needed by the automation identity. Use stable IDs
and full replacement bodies where the live schema requires them. Never embed
passwords or client secrets in checked-in JSON or command arguments.
User create/update exposes `--password-env`, `--password-file`, and
`--password-stdin`. Supply exactly one source, or put the complete payload
in a protected `--body-file`/`--stdin`. Inline `--body` and `--password` are
not exposed for secret-bearing operations. Inspect live help on older servers.

## OpenID applications and scope management

```bash
pomi openid applications list --skip 0 --take 50
pomi openid applications show <client-id>
pomi openid applications schema --operation create
pomi openid applications create --body-file application.json
pomi openid applications update <client-id> --body-file application.json
pomi openid applications delete <client-id> --force
pomi openid scopes list --skip 0 --take 50
pomi openid scopes show orchardcore.management
pomi openid scopes schema --operation create
pomi openid scopes create --body-file scope.json
pomi openid scopes update reporting --body-file scope.json
pomi openid scopes delete reporting --force
```

Requires `OrchardCore.OpenId.Management` and `AccessRemoteManagement`, plus
`ManageApplications` for applications or `ManageScopes` for scopes. Use the client
ID for application lookup and the scope name for scope lookup. The returned `id`
is an administrative storage identifier. Page through results using `totalCount`,
`skip` and `take`; the maximum page size is 200.

Application create/update requires `clientId`, `displayName` and `clientType`
(`public` or `confidential`). Confidential creation needs `clientSecret`. Supply
the complete request through a protected `--body-file` or `--stdin`; inline body
and secret arguments are not exposed. An omitted secret preserves an existing
confidential credential. Other settings are replacements: omitted roles, scopes
and redirect URIs clear them, and omitted grant/endpoint flags are false. Use
registered role/scope names and inspect the live schema before replacing settings.
The body client ID must match the update target. Equivalent creation retries
succeed; differing definitions or credentials conflict. Responses omit credentials,
keys, custom properties and private settings. Registered grants do not establish which flows the
server currently permits. Deletion prevents new client authentication; it does not
promise immediate invalidation of every previously issued token.

For confidential clients, use `pomi openid applications credentials rotate <client-id>
--secret-output-file <new-private-path> --force` or `credentials revoke <client-id>
--force`. Rotation immediately retires the old secret, with no overlap. The destination
is required even with `--output none`; existing files are refused before mutation.
Pomi writes the one-time JSON response privately and prints only its path. Never
print the file or put its contents in logs, prompts or source control. Update the
application's credential source privately; other saved contexts are not changed.
Do not automatically retry rotation after an uncertain response. Recover with an
explicit new rotation using another authorized identity or a still-valid token.
Revocation retires the shared secret while retaining the confidential client and
its settings; it does not disable other authentication methods or necessarily
invalidate issued tokens. MCP callers receive the secret directly and must keep it
out of transcripts and store it securely. Public clients reject both operations.

Application provisioning does not alter the independent administrator account.
Unattended installation still saves its application context automatically. For an
additional application, select its tenant context and use the existing
`OC_CLIENT_ID`/`OC_CLIENT_SECRET` credential sources for noninteractive requests;
keep the administrator handoff file separate from application credentials.

Scope create/update bodies require `name` and `displayName`. Updates replace the
editable fields: omitted `description` is cleared, and omitted `resources` or
`[]` clears resources. Explicit null resources are invalid. The body name must
match the update target; scope renaming is not supported through this API.
Equivalent create retries return the existing scope; a different definition under
that name conflicts. Unedited properties are preserved. Deleting a scope does not
revoke existing tokens or remove application permission strings.

## Automation sequence

1. Select an explicit context.
2. Check compatibility and refresh discovery.
3. Enable required features.
4. Refresh discovery again.
5. Inspect the operation schema.
6. Validate definitions where supported.
7. Execute mutations in dependency order.
8. Read back resources and capture stable IDs.
9. Treat destructive actions and execution-style commands as non-retryable
   unless their API reference explicitly says otherwise.

Versioned API references (live tenant schemas take precedence):

- [Features](https://github.com/sebastienros/OrchardCore/blob/4d4fc0fb66a5d789dff6d8057bbc074107918533/src/docs/reference/api/features/README.md)
- [Recipes](https://github.com/sebastienros/OrchardCore/blob/4d4fc0fb66a5d789dff6d8057bbc074107918533/src/docs/reference/api/recipes/README.md)
- [Queries](https://github.com/sebastienros/OrchardCore/blob/4d4fc0fb66a5d789dff6d8057bbc074107918533/src/docs/reference/api/queries/README.md)
- [Workflows](https://github.com/sebastienros/OrchardCore/blob/4d4fc0fb66a5d789dff6d8057bbc074107918533/src/docs/reference/api/workflows/README.md)
- [Users](https://github.com/sebastienros/OrchardCore/blob/4d4fc0fb66a5d789dff6d8057bbc074107918533/src/docs/reference/api/users/README.md)
- [Roles](https://github.com/sebastienros/OrchardCore/blob/4d4fc0fb66a5d789dff6d8057bbc074107918533/src/docs/reference/api/roles/README.md)

- [OpenID management](https://github.com/sebastienros/OrchardCore/blob/80579b862fb369b8366fe220ab2b6f4df80300fa/src/docs/reference/api/openid/README.md)

## Deployment plans and steps

Requires `OrchardCore.Deployment`, `AccessRemoteManagement` and
`ManageDeploymentPlan` in the selected tenant. Refresh OpenAPI after enabling
features. Plan configuration and package execution have separate permissions;
`ManageDeploymentPlan` alone does not grant Export or Import.

```bash
pomi deployment plans list --search Release --take 50
pomi deployment plans create --body-file plan.json
pomi deployment plans show 42
pomi deployment plans update 42 --body-file plan.json
pomi deployment step-types list
pomi deployment step-types schema CustomFileDeploymentStep
pomi deployment plans steps list 42
pomi deployment plans steps add 42 --body-file step.json
pomi deployment plans steps show 42 readme
pomi deployment plans steps update 42 readme --body-file patch.json
pomi deployment plans steps order 42 --body-file order.json
pomi deployment plans steps delete 42 readme --force
pomi deployment plans delete 42 --force
```

Plan create/update bodies contain `name`. Use the returned numeric `plan.id` in
subsequent commands; equivalent creates retain existing steps. Step creation takes
`id`, `type` and `values`, for example:

```json
{"id":"readme","type":"CustomFileDeploymentStep","values":{"fileName":"readme.txt","fileContent":"Release notes"}}
```

Reuse the same step ID and values for a retry; conflicting values under an existing
ID return conflict. Updates contain only `values`; omitted properties are retained.
Read the live type schema before choosing fields or clearing values. Custom file
contents and embedded recipe JSON are write-only and absent from readback: omission
is not evidence they are empty. Paths must be relative package paths without
traversal; `Recipe.json` is reserved.

Feature-owned schemas include the AllFeatures `ignoreDisabledFeatures` switch,
Templates/AdminTemplates `exportAsFiles`, content-definition export/replacement/
deletion selectors, and Media `includeAll`/`filePaths`/`directoryPaths`. Enable the
owning feature and refresh discovery before using its type. For definition exports
and replacement, select existing source type/part names; deletion names may exist
only on the destination. For Media, select existing relative media paths with `/`
separators. Enabling `includeAll` clears individual selections for definition and
media exports. Use the live schema for exact property names and supported types.

For query-based content export, use `QueryBasedContentDeploymentStep` with an
existing content-returning `queryName`. `queryParameters` is an optional string
containing a JSON object; JSON null clears it, while the string `"null"` is invalid
configuration. `exportAsSetupRecipe` enables the existing source's setup identity
rewriting. Create the query first, configure the step, then export the plan through
the artifact/operation workflow below. Invalid patches leave the step unchanged.

Ordering takes `{"stepIds":["readme","metadata"]}` with every existing ID exactly
once. Unsupported factories are discoverable with `canConfigure:false`; do not
invent schemas or serialize arbitrary CLR properties. Disabled-feature steps retain
their stored configuration and identity. Plan/step deletion is repeatable.

## Deployment packages and queued operations

Use the selected source tenant to export, and select the intended destination
context before uploading/importing. Operations and artifacts are private to their
tenant and initiating OAuth user/application. Export requires Export permission;
upload/import requires Import permission, plus AccessRemoteManagement.

```bash
pomi deployment operations export --request-id release-export-001 --plan-id 42
pomi deployment operations show OPERATION_ID
pomi deployment artifacts show ARTIFACT_ID
pomi deployment artifacts download ARTIFACT_ID --output-file ./release.zip
pomi deployment artifacts upload release.zip --file ./release.zip
pomi deployment operations import --request-id release-import-001 --artifact-id ARTIFACT_ID --force
pomi deployment artifacts delete ARTIFACT_ID --force
```

Submission returns an operation ID, not completion. Poll `operations show` with
a bounded wait until succeeded, failed or uncertain. The background worker polls
every minute; pending/running means work is still outstanding. On export success,
use its returned artifact ID for download. Downloads require a new local file and
never overwrite an existing file. Upload validates ZIP/JSON without executing it.
Use the upload response's artifact ID for import and verify destination effects
after success. Artifacts expire after 24 hours by default.

Keep the request ID for retries of the same submission. Changed export snapshots
or different import artifacts under the same ID conflict. Do not generate a new
ID merely because polling timed out or an import failed: recipes can partially
commit. Inspect the recorded state and target effects before deciding to submit
new work. Uncertain operations are never automatically replayed. Keep exported
packages private; they may contain application configuration or content.

MCP exposes JSON operation and artifact metadata plus execution commands. Use Pomi
for file upload/download; binary transfers are excluded from MCP tools.


## Audit and scheduled task administration

Use `pomi audit-trail events list --q 'category:User event:Created' --page-size 50`
and `audit-trail events show <event-id>` with `ViewAuditTrail`. These operations
return metadata rather than snapshots/payloads. Paginate with `page` and `pageSize`
(maximum 200); `id:` filters by correlation ID. The existing admin filter syntax
and enabled event producers still apply.

Use `pomi background-tasks list` and `show <registered-name>` with
`ManageBackgroundTasks`. Read `configuration`, validate the complete JSON through
`background-tasks validate`, then `update <name>`. Configuration updates preserve
enabled status; omitted numeric/Boolean values reset to zero/false. Cron expressions
have five fields, and lock timeout/expiration are nonnegative milliseconds.
Use separate `enable <name>` / `disable <name>` commands, then read back. These
commands affect future scheduling; do not claim they immediately run or cancel a
task. Existing settings signals notify the scheduler. Enabling invalid persisted
settings is rejected; disabling remains available so they can be repaired.



## Feature-owned deployment selectors and background permissions

Discover `deployment step-types list` and the exact schema before adding steps.
Settings selectors use `includeAll` and `settingsTypeNames`. Site settings use an
explicit `settings` list. Core index selectors use profile names and require a
nonempty selection unless `includeAll` is true; legacy Lucene selectors use index
names. Configuration-free factories take `{}` and reject arbitrary model fields.
Refresh discovery after enabling the contributing feature.

Queued exports retain the first submitting identity without retaining its tokens.
Grant the existing settings-type and user-resource permissions to that context;
`Export` alone does not authorize private settings or credential-bearing user
records (`ManageUsers`). Poll the operation and require `succeeded` before using
its artifact. A denied source fails the job. Do not retry a failed job blindly or
publish downloaded packages, which may contain site secrets or user credentials.

## Remote deployment destinations

Enable `OrchardCore.Deployment.Remote` and refresh discovery. Configure a target's
`deployment remote-clients` with `clientName` and a strong `apiKey`, and the source's
`deployment remote-instances` with `name`, the target import `url`, `clientName` and
the same key. Use stdin/private body files; readback returns only `hasApiKey`.
Updates require non-key fields and preserve an omitted/null key. Delete a client to
revoke it. Keep source and target CLI contexts distinct.

Use `deployment targets list`, then `deployment targets send <id> --plan-id <plan>`
with approval appropriate to the intended import. This requires both
`ExportRemoteInstances` and `Export`; management permissions are separate. Sending
can change the target immediately and is not idempotent. On timeout/error, inspect
the target before any retry. Use HTTPS destinations, and preserve private credential
handoff files when provisioning a persistent site. Existing setup contexts need no
interactive `pomi login`.

## Explicit content exports

`content export <id>` (Download feature) returns the published version in the admin
JSON format; `--latest true` selects the current draft/latest version. It requires
Export and per-item EditContent permissions. Keep private content exports in private
files.

For packages, enable `OrchardCore.Contents.Deployment.ExportContentToDeploymentTarget`
and add `ExportContentToDeploymentTargetDeploymentStep` with `contentItemIds` (1–200)
and `latest` to a plan. Then queue `deployment operations export` or send the plan to
a configured remote target. Include content definitions in the plan when the target
needs them. Do not automate a legacy form-based selector without explicit IDs.
Queued exports fail on denied or missing selected content; inspect failures before
retrying. Existing setup contexts already support client credentials without login.
