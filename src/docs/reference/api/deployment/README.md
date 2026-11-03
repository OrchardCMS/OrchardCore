# Deployment management API

The **Deployment** feature (`OrchardCore.Deployment`) manages deployment plans and their steps, stores private deployment packages, and queues exports and imports. It advertises the `deployment-plans` remote-management capability and contributes the `pomi deployment` commands and MCP tools. Disabling the feature removes these routes.

To configure remote sites and send a plan to them, see the [Remote Deployment API](../deployment-remote/README.md). For concepts, custom steps and configuration, see the [Deployment module](../../modules/Deployment/README.md).

## Authentication and permissions

Every operation requires an `Api` scheme bearer token and **Access remote management API** (`AccessRemoteManagement`). An absent or invalid token returns `401`, a missing permission returns `403`.

| Operations | Additional permission |
| --- | --- |
| Plans, steps and step types | `ManageDeploymentPlan` |
| Uploading an artifact | `Import` |
| Showing, downloading or deleting an artifact | `Export` for an export artifact, `Import` for an import artifact |
| Queuing or showing an export | `Export` |
| Queuing or showing an import | `Import` |

`ManageDeploymentPlan` doesn't allow executing a plan: managing plans and steps never runs an export.

Artifacts and operations belong to the identity that created them: the tenant, issuer, entity kind and subject of the token. A caller holding the right permissions gets `404` for those of another identity.

## Base route

`/api/deployment`

## Endpoints

| Method | Endpoint | Operation ID | Pomi command | Permission |
| --- | --- | --- | --- | --- |
| `GET` | `/api/deployment/plans` | `ApiListDeploymentPlans` | `deployment plans list` | `ManageDeploymentPlan` |
| `GET` | `/api/deployment/plans/by-id?id={id}` | `ApiGetDeploymentPlan` | `deployment plans show <id>` | `ManageDeploymentPlan` |
| `POST` | `/api/deployment/plans` | `ApiCreateDeploymentPlan` | `deployment plans create` | `ManageDeploymentPlan` |
| `PUT` | `/api/deployment/plans/by-id?id={id}` | `ApiUpdateDeploymentPlan` | `deployment plans update <id>` | `ManageDeploymentPlan` |
| `DELETE` | `/api/deployment/plans/by-id?id={id}` | `ApiDeleteDeploymentPlan` | `deployment plans delete <id> --force` | `ManageDeploymentPlan` |
| `GET` | `/api/deployment/step-types` | `ApiListDeploymentStepTypes` | `deployment step-types list` | `ManageDeploymentPlan` |
| `GET` | `/api/deployment/step-types/schema?type={type}` | `ApiGetDeploymentStepSchema` | `deployment step-types schema <type>` | `ManageDeploymentPlan` |
| `GET` | `/api/deployment/plans/steps?planId={planId}` | `ApiListDeploymentSteps` | `deployment plans steps list <planId>` | `ManageDeploymentPlan` |
| `GET` | `/api/deployment/plans/steps/by-id?planId={planId}&stepId={stepId}` | `ApiGetDeploymentStep` | `deployment plans steps show <planId> <stepId>` | `ManageDeploymentPlan` |
| `POST` | `/api/deployment/plans/steps?planId={planId}` | `ApiAddDeploymentStep` | `deployment plans steps add <planId>` | `ManageDeploymentPlan` |
| `PUT` | `/api/deployment/plans/steps/by-id?planId={planId}&stepId={stepId}` | `ApiUpdateDeploymentStep` | `deployment plans steps update <planId> <stepId>` | `ManageDeploymentPlan` |
| `DELETE` | `/api/deployment/plans/steps/by-id?planId={planId}&stepId={stepId}` | `ApiDeleteDeploymentStep` | `deployment plans steps delete <planId> <stepId> --force` | `ManageDeploymentPlan` |
| `PUT` | `/api/deployment/plans/steps/order?planId={planId}` | `ApiOrderDeploymentSteps` | `deployment plans steps order <planId>` | `ManageDeploymentPlan` |
| `POST` | `/api/deployment/artifacts?fileName={fileName}` | `ApiUploadDeploymentArtifact` | `deployment artifacts upload <fileName> --file <path>` | `Import` |
| `GET` | `/api/deployment/artifacts/{id}` | `ApiGetDeploymentArtifact` | `deployment artifacts show <id>` | `Export` or `Import` |
| `DELETE` | `/api/deployment/artifacts/{id}` | `ApiDeleteDeploymentArtifact` | `deployment artifacts delete <id> --force` | `Export` or `Import` |
| `GET` | `/api/deployment/artifacts/{id}/content` | `ApiDownloadDeploymentArtifact` | `deployment artifacts download <id> --output-file <path>` | `Export` or `Import` |
| `POST` | `/api/deployment/operations:export` | `ApiQueueDeploymentExport` | `deployment operations export` | `Export` |
| `POST` | `/api/deployment/operations:import` | `ApiQueueDeploymentImport` | `deployment operations import --force` | `Import` |
| `GET` | `/api/deployment/operations/{id}` | `ApiGetDeploymentOperation` | `deployment operations show <id>` | `Export` or `Import` |

### MCP

The JSON operations are exposed as tools of the [tenant MCP server](../../modules/RemoteManagement/README.md#mcp-server). Uploads and downloads aren't, as they transfer binary content. The operations for which Pomi asks a confirmation, that is deleting and queuing an import, are only tools when `AllowConfirmationRequiredTools` is set.

## Plans

### List plans

`GET /api/deployment/plans`

| Location | Name | Type | Required | Description |
| --- | --- | --- | --- | --- |
| Query | `search` | string | No | Name substring filter. |
| Query | `skip` | integer | No | Zero-based offset. Default `0`, minimum `0`. |
| Query | `take` | integer | No | Page size. Default `50`, range `1`–`200`. |

Plans are ordered by name, then identifier. Plan identifiers are local to a tenant.

```json
{
  "skip": 0,
  "take": 50,
  "totalCount": 1,
  "items": [{ "id": 123, "name": "Website export", "stepCount": 2 }]
}
```

Other paging values return `400`.

### Show a plan

`GET /api/deployment/plans/by-id?id=123`

`200` returns one plan as shown above, without its step configuration. A nonpositive `id` returns `400`, and an absent plan `404`.

### Create a plan

`POST /api/deployment/plans`

```json
{ "name": "Website export" }
```

The required `name` is the only accepted property; another one returns `400`. Creation makes an empty plan and returns `200`:

```json
{
  "changed": true,
  "plan": { "id": 123, "name": "Website export", "stepCount": 0 }
}
```

Retrying with an existing name returns that plan with `changed: false`, and leaves its steps unchanged. A blank name returns `400`.

### Rename a plan

`PUT /api/deployment/plans/by-id?id=123`

The body is the same as for creating a plan. Renaming preserves the steps, their order and their configuration. The same name again returns `changed: false`. A blank name or a nonpositive `id` returns `400`, an absent plan `404`, and a name used by another plan `409`.

### Delete a plan

`DELETE /api/deployment/plans/by-id?id=123`

Returns `200` with `changed: true` and a null `plan`. An absent plan returns `404`. A nonpositive `id` returns `400`.

## Step types

### List step types

`GET /api/deployment/step-types`

Returns the registered step factories, which depend on the enabled features, ordered by type. `canConfigure` tells whether the type has an explicit configuration contract.

```json
[
  { "type": "CustomFileDeploymentStep", "canConfigure": true },
  { "type": "ExampleDeploymentStep", "canConfigure": false }
]
```

### Get a step schema

`GET /api/deployment/step-types/schema?type=CustomFileDeploymentStep`

`200` returns the JSON Schema of the type's `values`. This schema is dynamic: it comes from the feature owning the step. A missing `type` returns `400`, an unknown type `404`, and a type without a configuration contract `501`.

## Steps

### Step representation

```json
{
  "id": "site-css",
  "type": "CustomFileDeploymentStep",
  "position": 0,
  "canConfigure": true,
  "values": { "fileName": "assets/site.css" }
}
```

`position` is zero-based. `values` only contains allowlisted configuration: write-only values, such as custom file content and embedded recipe JSON, are never returned, and it is null for a step whose type has no enabled contract.

Every step operation returns `400` for a nonpositive `planId` or an invalid identifier, and `404` for an absent plan.

### List steps

`GET /api/deployment/plans/steps?planId=123`

`200` returns an array of steps in their execution order.

### Show a step

`GET /api/deployment/plans/steps/by-id?planId=123&stepId=site-css`

`200` returns one step. The identifier is compared case-insensitively. An absent step returns `404`.

### Add a step

`POST /api/deployment/plans/steps?planId=123`

```json
{
  "id": "site-css",
  "type": "CustomFileDeploymentStep",
  "values": {
    "fileName": "assets/site.css",
    "fileContent": "body { color: #222; }"
  }
}
```

| Property | Type | Required | Description |
| --- | --- | --- | --- |
| `id` | string | Yes | Identifier chosen by the caller, at most 128 characters. It makes the operation retry-safe. |
| `type` | string | Yes | A registered factory type. |
| `values` | object | Yes | Configuration matching the type's schema. |

Unknown properties return `400`. `200` returns `changed` and `step`:

```json
{
  "changed": true,
  "step": {
    "id": "site-css",
    "type": "CustomFileDeploymentStep",
    "position": 0,
    "canConfigure": true,
    "values": { "fileName": "assets/site.css" }
  }
}
```

Retrying with the same identifier and configuration returns `200` with `changed: false`. A different type or different configuration under that identifier returns `409`. An unknown type returns `404`, a type without a contract `501`, and invalid values a `400` validation problem keyed by property.

### Update a step

`PUT /api/deployment/plans/steps/by-id?planId=123&stepId=site-css`

```json
{ "values": { "fileName": "assets/new.css" } }
```

The values are a patch: omitted properties, including write-only ones, keep their value. The identity, the type and the position don't change. An invalid patch returns `400` and leaves the stored step unchanged, an absent step `404`, and a step without an enabled contract `501`. An equivalent patch returns `changed: false`.

### Delete a step

`DELETE /api/deployment/plans/steps/by-id?planId=123&stepId=site-css`

Returns `200` with `changed: true` and a null `step`. An absent step returns `404`.

### Order steps

`PUT /api/deployment/plans/steps/order?planId=123`

```json
{ "stepIds": ["site-css", "all-content"] }
```

`stepIds` must list every current step exactly once. Otherwise the request returns `400` and the plan is unchanged. `200` returns `changed` with a null `step`, and the current order again returns `changed: false`.

## Artifacts

An artifact is a private deployment package stored under the tenant's `App_Data` folder. It expires after 24 hours by default, see [Private artifact storage](../../modules/Deployment/README.md#private-artifact-storage).

### Artifact representation

```json
{
  "id": "2f1c0a9e5b7d4e8c",
  "kind": "import",
  "fileName": "package.zip",
  "contentType": "application/zip",
  "length": 20480,
  "sha256": "9f86d081884c7d659a2feaa0c55ad015a3bf4f1b2b0b822cd15d6c15b0f00a08",
  "createdUtc": "2026-01-15T10:00:00Z",
  "expiresUtc": "2026-01-16T10:00:00Z"
}
```

`kind` is `import` or `export`. The metadata leaves out the owner and the server paths.

### Upload an artifact

`POST /api/deployment/artifacts?fileName=package.zip`

Send the package as the `application/octet-stream` request body, or use Pomi:

```bash
pomi deployment artifacts upload package.zip --file ./package.zip
```

`fileName` is required and must end with `.zip` or `.json`, without a directory or control characters. The upload goes through the file-creation pipeline, such as an antivirus scan, and the same package validation as an admin import. It doesn't execute the recipe.

| Status | Meaning |
| --- | --- |
| `200` | The artifact representation. |
| `400` | An invalid file name, a package rejected by the file-creation pipeline, or an invalid package. Nothing is stored. |
| `413` | The package exceeds `MaxUploadBytes`. |

The limits are `MaxUploadBytes` (100 MiB), `MaxExpandedBytes` (500 MiB) and `MaxEntries` (10,000), set in the `OrchardCore:Deployment:Package` [configuration section](../../modules/Deployment/README.md).

Every successful upload creates a new artifact with a new identifier, so a retry doesn't converge: delete the artifact you don't use, or let it expire.

### Show an artifact

`GET /api/deployment/artifacts/{id}`

`200` returns the artifact representation. An absent or expired artifact, or one of another identity, returns `404`.

### Download an artifact

`GET /api/deployment/artifacts/{id}/content`

`200` streams the original bytes as `application/zip` or `application/json`, with the artifact's file name. Pomi writes them to a new private file, and removes an incomplete download:

```bash
pomi deployment artifacts download <id> --output-file ./package.zip
```

### Delete an artifact

`DELETE /api/deployment/artifacts/{id}`

`200` returns `{ "changed": true }`. An absent artifact, or one of another identity, returns `404`. An artifact being read returns `409`: retry later.

## Operations

Exports and imports run in a tenant background worker, which polls every minute.

### Operation representation

```json
{
  "id": "6b1f8d2c3a4e5f60",
  "kind": "export",
  "state": "succeeded",
  "artifactId": "2f1c0a9e5b7d4e8c",
  "errorCode": null,
  "createdUtc": "2026-01-15T10:00:00Z",
  "updatedUtc": "2026-01-15T10:01:05Z"
}
```

| Property | Description |
| --- | --- |
| `kind` | `export` or `import`. |
| `state` | `pending`, `running`, `succeeded`, `failed` or `uncertain`. |
| `artifactId` | The artifact holding the package of a completed export. |
| `errorCode` | `execution_failed`, or `execution_interrupted` for an uncertain operation. |

An interrupted execution becomes `uncertain` and is never replayed. An import can commit partially before a failure or an interruption, so review the tenant before submitting a new request.

### Queue an export

`POST /api/deployment/operations:export`

```json
{ "requestId": "nightly-2026-01-15", "planId": 123 }
```

The operation captures a snapshot of the plan's configuration when it is accepted. Download the package from the returned `artifactId` once the state is `succeeded`.

### Queue an import

`POST /api/deployment/operations:import`

```json
{ "requestId": "import-2026-01-15", "artifactId": "2f1c0a9e5b7d4e8c" }
```

`artifactId` must be an import artifact uploaded by the same identity.

### Responses of queuing an operation

| Status | Meaning |
| --- | --- |
| `202` | The operation representation, with a `Location` header for its status. |
| `400` | A `requestId` that is blank or longer than 128 characters, or an invalid target. |
| `404` | The plan or the import artifact doesn't exist. |
| `409` | The plan has steps whose feature is unavailable, the same request is being accepted, or the request identifier belongs to a different request. |

`requestId` makes both operations retry-safe. Repeating a request with the same identifier, by the same identity, returns `202` with the existing operation and doesn't execute it again. The comparison covers the kind and the payload, which for an export is the plan's configuration at that moment: a plan changed in the meantime is a different request, and returns `409`. A retry of an export also returns `404` once the plan is deleted, while an accepted import keeps answering when its artifact has expired.

An operation runs as the caller who submitted it. See [queued deployment operations](../../modules/Deployment/README.md#queued-deployment-operations) for the captured identity and the retention of ended operations, after which a request identifier can be used again.

### Show an operation

`GET /api/deployment/operations/{id}`

`200` returns the operation representation, without the captured configuration. An absent operation, or one of another identity, returns `404`. Reading it requires the current `Export` or `Import` permission, according to its kind.

On a web farm, operations and artifacts stay on the node that accepted them. See [Operations on a web farm](../../modules/Deployment/README.md#operations-on-a-web-farm).

## Errors

Failures use Problem Details, except invalid step values, which use a validation problem keyed by property. Plan and step changes are not protected by an ETag: serialize competing writes and read the plan again after a conflict.

## Sources

- [Plan endpoints](https://github.com/OrchardCMS/OrchardCore/blob/main/src/OrchardCore.Modules/OrchardCore.Deployment/Endpoints/Management/DeploymentPlanEndpoints.cs)
- [Step endpoints](https://github.com/OrchardCMS/OrchardCore/blob/main/src/OrchardCore.Modules/OrchardCore.Deployment/Endpoints/Management/DeploymentStepEndpoints.cs)
- [Artifact endpoints](https://github.com/OrchardCMS/OrchardCore/blob/main/src/OrchardCore.Modules/OrchardCore.Deployment/Endpoints/Management/DeploymentArtifactEndpoints.cs)
- [Operation endpoints](https://github.com/OrchardCMS/OrchardCore/blob/main/src/OrchardCore.Modules/OrchardCore.Deployment/Endpoints/Management/DeploymentOperationEndpoints.cs)
- [Deployment module](../../modules/Deployment/README.md)
