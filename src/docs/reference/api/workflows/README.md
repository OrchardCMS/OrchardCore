# Workflow management API (`OrchardCore.Workflows`)

The workflow management API manages workflow type graphs, discovers registered activity
types, executes workflows, and inspects or cancels persisted workflow instances. Enable the
**Workflows** feature (`OrchardCore.Workflows`). The feature advertises the Remote Management
capability `workflows`.

## Authentication and authorization

Every endpoint uses Orchard Core's `Api` authentication scheme, requires
`AccessRemoteManagement`, and disables antiforgery validation. See
[Remote Management authentication](../../modules/RemoteManagement/README.md) for API
credential configuration.

All operations except execution require the security-critical `ManageWorkflows` permission.
`POST /api/workflows/types/{workflowTypeId}/execute` instead requires the security-critical
`ExecuteWorkflows` permission.

Unauthenticated requests return `401 Unauthorized`. Failed permission checks return
`403 Forbidden` Problem Details:

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.4",
  "title": "Forbidden",
  "status": 403,
  "detail": "You do not have sufficient permissions to complete this request"
}
```

## Base route

`/api/workflows`

All request and response bodies use `application/json`. Outer API model property names are
camel-cased. Activity property names inside `properties`, `schema`, `propertiesSchema`,
`activityRecordSchema`, and `defaultProperties` come from CLR activity members and retain
their declared casing.

### Common headers

| Header | Required | Value |
| --- | --- | --- |
| `Authorization` | Yes | Credentials for the configured `Api` scheme, for example `Bearer <token>`. |
| `Content-Type` | For requests with a body | `application/json` |
| `Accept` | No | `application/json` |

## Endpoints

| Method | Path | Permission | Purpose |
| --- | --- | --- | --- |
| `GET` | `/types` | `ManageWorkflows` | List workflow types |
| `GET` | `/types/{workflowTypeId}` | `ManageWorkflows` | Get a workflow type |
| `POST` | `/types` | `ManageWorkflows` | Create a workflow type |
| `PUT` | `/types/{workflowTypeId}` | `ManageWorkflows` | Replace a workflow type |
| `DELETE` | `/types/{workflowTypeId}` | `ManageWorkflows` | Delete a workflow type and its instances |
| `POST` | `/types/validate` | `ManageWorkflows` | Validate a graph without saving |
| `GET` | `/activity-types` | `ManageWorkflows` | List registered activity types |
| `GET` | `/activity-types/{name}` | `ManageWorkflows` | Get an activity type descriptor |
| `POST` | `/types/{workflowTypeId}/enable` | `ManageWorkflows` | Enable a workflow type |
| `POST` | `/types/{workflowTypeId}/disable` | `ManageWorkflows` | Disable a workflow type |
| `POST` | `/types/{workflowTypeId}/execute` | `ExecuteWorkflows` | Start a workflow |
| `GET` | `/instances` | `ManageWorkflows` | List persisted instances |
| `GET` | `/instances/{workflowId}` | `ManageWorkflows` | Get an instance |
| `DELETE` | `/instances/{workflowId}` | `ManageWorkflows` | Cancel an instance |

## Shared contracts

### Paging

Workflow type and activity type lists accept:

| Query parameter | Type | Default | Constraints |
| --- | --- | --- | --- |
| `skip` | integer | `0` | At least `0`. |
| `take` | integer | `50` | From `1` through `200`. |

The instance list uses the same constraints but defaults `take` to `20`. Invalid paging returns
`400 Bad Request` with exact detail
`Skip must be zero or greater and take must be greater than zero.` or
`Take cannot exceed 200.`.

All list envelopes contain the count before paging:

```json
{
  "skip": 0,
  "take": 50,
  "totalCount": 1,
  "items": []
}
```

### Workflow type

| Property | Type | Required | Default/constraints |
| --- | --- | --- | --- |
| `workflowTypeId` | string or null | No | Trimmed. Generated on create when blank. A stable value makes create retries comparable. On update, blank becomes the route ID. |
| `name` | string | Yes | Trimmed; must be unique case-insensitively. |
| `isEnabled` | boolean | No | `false`. |
| `isSingleton` | boolean | No | `false`; controls whether multiple instances may be spawned. |
| `lockTimeout` | integer | No | `0`; lock acquisition timeout in milliseconds. No API range validation. |
| `lockExpiration` | integer | No | `0`; acquired-lock expiration in milliseconds. No API range validation. |
| `deleteFinishedWorkflows` | boolean | No | `false`; removes instances when they finish. |
| `activities` | array | Yes | Must contain at least one item and at least one item with `isStart=true`. |
| `transitions` | array | No | Defaults to `[]`. |

Activity record:

| Property | Type | Required | Default/constraints |
| --- | --- | --- | --- |
| `activityId` | string or null | No | Trimmed and unique within the graph. On create with a stable `workflowTypeId`, an omitted or null ID becomes `{workflowTypeId}-activity-{one-based-index}`. A blank ID uses the normal server generator. |
| `name` | string | Yes | Registered activity type name. |
| `x` | integer | No | `0`; designer coordinate. |
| `y` | integer | No | `0`; designer coordinate. |
| `isStart` | boolean | No | `false`. |
| `properties` | object | No | Defaults to `{}`. Members, types, defaults, and constraints depend on `name`. |

Transition:

| Property | Type | Required | Constraints |
| --- | --- | --- | --- |
| `sourceActivityId` | string | Yes | Must identify an activity in this graph. |
| `sourceOutcomeName` | string | Yes | Must be nonblank. The management API does not validate it against the descriptor's outcomes. |
| `destinationActivityId` | string | Yes | Must identify an activity in this graph. |

The complete graph is replaced on update. An update may supply a nonblank
`workflowTypeId`; it must not conflict with another workflow type.

### Workflow instance

Persisted instances have:

| Property | Type | Notes |
| --- | --- | --- |
| `id` | integer | Persistence identifier. |
| `workflowId` | string | Unique instance identifier. |
| `workflowTypeId` | string | Owning type identifier. |
| `correlationId` | string or null | Caller/object correlation value. |
| `state` | object | Dynamic serialized workflow state. |
| `status` | integer | `0` Idle, `1` Starting, `2` Resuming, `3` Executing, `4` Halted, `5` Finished, `6` Faulted, `7` Aborted. |
| `faultMessage` | string or null | Fault details, when any. |
| `lockTimeout` | integer | Milliseconds. |
| `lockExpiration` | integer | Milliseconds. |
| `blockingActivities` | array | Waiting activities with `activityId`, `isStart`, and `name`. |
| `createdUtc` | string(date-time) | Creation time. |
| `isAtomic` | boolean | `true` when both lock values are greater than zero. |

The `state` object is runtime and activity dependent.

### Errors and validation

Missing workflow types, activity types, or instances return `404 Not Found`:

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.5",
  "title": "Not Found",
  "status": 404
}
```

Create, update, and execute validation failures return `400 Bad Request` validation Problem
Details:

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "detail": "The destination activity must exist.",
  "errors": {
    "transitions[0].destinationActivityId": [
      "The destination activity must exist."
    ]
  }
}
```

Validation detects blank/duplicate names or IDs, empty graphs, missing start activities,
unknown activity types, duplicate activity IDs, and invalid transition references. Execute
validation rejects disabled types and unknown requested start activities. The API does not
return `409 Conflict`.

## Workflow type operations

### List workflow types

`GET /api/workflows/types`

Accepts shared `skip` and `take`; no body. Results are ordered by `name`.

```bash
curl -H "Authorization: Bearer $TOKEN" \
  "https://localhost:5001/api/workflows/types?skip=0&take=50"
```

`200 OK` returns:

```json
{
  "skip": 0,
  "take": 50,
  "totalCount": 1,
  "items": [
    {
      "workflowTypeId": "4workflowtype",
      "name": "Remote Workflow",
      "isEnabled": true,
      "isSingleton": false,
      "lockTimeout": 0,
      "lockExpiration": 0,
      "deleteFinishedWorkflows": false,
      "activities": [
        {
          "activityId": "set-output",
          "name": "SetOutputTask",
          "x": 0,
          "y": 0,
          "isStart": true,
          "properties": {
            "OutputName": "result",
            "Syntax": "JavaScript",
            "Value": {
              "Expression": "42"
            }
          }
        }
      ],
      "transitions": []
    }
  ]
}
```

Also returns `400` for invalid paging, `401`, or `403`.

### Get a workflow type

`GET /api/workflows/types/{workflowTypeId}`

`workflowTypeId` is a required string path parameter. No body or query parameters.

```bash
curl -H "Authorization: Bearer $TOKEN" \
  https://localhost:5001/api/workflows/types/4workflowtype
```

`200 OK` returns the complete type:

```json
{
  "workflowTypeId": "4workflowtype",
  "name": "Remote Workflow",
  "isEnabled": true,
  "isSingleton": false,
  "lockTimeout": 0,
  "lockExpiration": 0,
  "deleteFinishedWorkflows": false,
  "activities": [
    {
      "activityId": "set-output",
      "name": "SetOutputTask",
      "x": 0,
      "y": 0,
      "isStart": true,
      "properties": {
        "OutputName": "result",
        "Syntax": "JavaScript",
        "Value": {
          "Expression": "42"
        }
      }
    }
  ],
  "transitions": []
}
```

Also returns `401`, `403`, or `404`.

### Create a workflow type

`POST /api/workflows/types`

The required JSON body uses the workflow type contract. With an explicit stable `workflowTypeId`,
an equivalent retry returns the existing type with `201 Created`. Equivalence is deep equality of
the complete normalized definition, including settings, activity and transition order, IDs,
coordinates, flags, and properties. The same ID with a different definition returns `400`
validation Problem Details under `WorkflowTypeId`.

When a stable workflow ID is supplied, an omitted or null activity ID is generated
deterministically as `{workflowTypeId}-activity-{one-based-index}`. A blank activity ID uses the
normal activity ID generator. Omitting `workflowTypeId` uses a server-generated ID, so repeating
that create can create another workflow type and is not retry-safe.

```bash
curl -X POST -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Remote Workflow",
    "isEnabled": true,
    "isSingleton": false,
    "lockTimeout": 0,
    "lockExpiration": 0,
    "deleteFinishedWorkflows": false,
    "activities": [
      {
        "activityId": "set-output",
        "name": "SetOutputTask",
        "x": 0,
        "y": 0,
        "isStart": true,
        "properties": {
          "OutputName": "result",
          "Syntax": "JavaScript",
          "Value": {
            "Expression": "42"
          }
        }
      }
    ],
    "transitions": []
  }' \
  https://localhost:5001/api/workflows/types
```

`201 Created` returns the saved or equivalent existing type and sets `Location` to
`/api/workflows/types/{workflowTypeId}`:

```json
{
  "workflowTypeId": "4workflowtype",
  "name": "Remote Workflow",
  "isEnabled": true,
  "isSingleton": false,
  "lockTimeout": 0,
  "lockExpiration": 0,
  "deleteFinishedWorkflows": false,
  "activities": [
    {
      "activityId": "set-output",
      "name": "SetOutputTask",
      "x": 0,
      "y": 0,
      "isStart": true,
      "properties": {
        "OutputName": "result",
        "Syntax": "JavaScript",
        "Value": {
          "Expression": "42"
        }
      }
    }
  ],
  "transitions": []
}
```

Also returns `400`, `401`, or `403`.

### Update a workflow type

`PUT /api/workflows/types/{workflowTypeId}`

The path ID and workflow type body are required. The body replaces graph and settings. A blank
body ID uses the route ID. Unlike content definitions and queries, a nonblank body ID may differ
from the route ID when it does not conflict with another workflow type; update does not reject
that identity change solely because the route and body values differ. Repeating a successful
complete replacement converges to the same workflow definition and returns `200`.

```bash
curl -X PUT -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "workflowTypeId": "4workflowtype",
    "name": "Remote Workflow",
    "isEnabled": false,
    "isSingleton": true,
    "lockTimeout": 1000,
    "lockExpiration": 30000,
    "deleteFinishedWorkflows": false,
    "activities": [
      {
        "activityId": "set-output",
        "name": "SetOutputTask",
        "x": 0,
        "y": 0,
        "isStart": true,
        "properties": {
          "OutputName": "result",
          "Syntax": "JavaScript",
          "Value": {
            "Expression": "42"
          }
        }
      }
    ],
    "transitions": []
  }' \
  https://localhost:5001/api/workflows/types/4workflowtype
```

`200 OK` returns the complete updated type:

```json
{
  "workflowTypeId": "4workflowtype",
  "name": "Remote Workflow",
  "isEnabled": false,
  "isSingleton": true,
  "lockTimeout": 1000,
  "lockExpiration": 30000,
  "deleteFinishedWorkflows": false,
  "activities": [
    {
      "activityId": "set-output",
      "name": "SetOutputTask",
      "x": 0,
      "y": 0,
      "isStart": true,
      "properties": {
        "OutputName": "result",
        "Syntax": "JavaScript",
        "Value": {
          "Expression": "42"
        }
      }
    }
  ],
  "transitions": []
}
```

Also returns `400`, `401`, `403`, or `404`.

### Delete a workflow type

`DELETE /api/workflows/types/{workflowTypeId}`

`workflowTypeId` is required. No body or query parameters. The workflow type store also deletes
its persisted workflow instances.

```bash
curl -X DELETE -H "Authorization: Bearer $TOKEN" \
  https://localhost:5001/api/workflows/types/4workflowtype
```

`204 No Content` has no body. Deleting an already absent workflow type is a convergent retry and
also returns `204`. Other responses are `401` or `403`.

### Validate a workflow graph

`POST /api/workflows/types/validate`

The required body uses the workflow type contract. Validation normalizes/generates activity
IDs in memory and does not save the type.

```bash
curl -X POST -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Invalid Workflow",
    "activities": [
      {
        "activityId": "set-output",
        "name": "SetOutputTask",
        "isStart": true,
        "properties": {}
      }
    ],
    "transitions": [
      {
        "sourceActivityId": "set-output",
        "sourceOutcomeName": "Done",
        "destinationActivityId": "missing"
      }
    ]
  }' \
  https://localhost:5001/api/workflows/types/validate
```

`200 OK` returns validation results; graph errors do not change the HTTP status:

```json
{
  "isValid": false,
  "errors": [
    "The destination activity must exist."
  ]
}
```

Also returns `401` or `403`.

## Activity type operations

### List activity types

`GET /api/workflows/activity-types`

Accepts shared `skip` and `take`; no body. Results are ordered by activity `name`.

```bash
curl -H "Authorization: Bearer $TOKEN" \
  "https://localhost:5001/api/workflows/activity-types?skip=0&take=50"
```

`200 OK` returns complete descriptors in the paging envelope. For example, a page beyond the
last registered activity contains:

```json
{
  "skip": 200,
  "take": 50,
  "totalCount": 14,
  "items": []
}
```

The count is installation dependent. Every descriptor contains `name`, `displayText`,
`category`, `hasEditor`, `outcomes`, `schema`, `propertiesSchema`, `activityRecordSchema`, and
`defaultProperties`. `schema` is a compatibility alias with the same value as
`propertiesSchema`. Property schemas, default values, localized display
text/category/outcomes, and the set of activities depend on enabled workflow activity
features. Also returns `400`, `401`, or `403`.

### Get an activity type

`GET /api/workflows/activity-types/{name}`

`name` is a required string path parameter. Lookup is case-insensitive. No body or query
parameters.

```bash
curl -H "Authorization: Bearer $TOKEN" \
  https://localhost:5001/api/workflows/activity-types/CommitTransactionTask
```

`200 OK` returns one complete descriptor. This exact zero-property example is available when
the `OrchardCore.Workflows.Session` feature is enabled:

```json
{
  "name": "CommitTransactionTask",
  "displayText": "Commit Transaction Task",
  "category": "Session",
  "hasEditor": true,
  "outcomes": ["Done", "Valid", "Invalid"],
  "schema": {
    "type": "object",
    "properties": {}
  },
  "propertiesSchema": {
    "type": "object",
    "properties": {}
  },
  "activityRecordSchema": {
    "type": "object",
    "properties": {
      "ActivityId": {
        "type": ["string", "null"]
      },
      "Name": {
        "type": ["string", "null"],
        "const": "CommitTransactionTask"
      },
      "X": {
        "type": "integer",
        "default": 0
      },
      "Y": {
        "type": "integer",
        "default": 0
      },
      "IsStart": {
        "type": "boolean",
        "default": false
      },
      "Properties": {
        "type": "object",
        "properties": {}
      }
    },
    "required": ["Name", "ActivityId"]
  },
  "defaultProperties": {}
}
```

Schemas are dynamic as described above, and the response includes every persisted property of
the selected activity. Also returns `401`, `403`, or `404`.

## Workflow state operations

### Enable a workflow type

`POST /api/workflows/types/{workflowTypeId}/enable`

`workflowTypeId` is required. No body or query parameters.

```bash
curl -X POST -H "Authorization: Bearer $TOKEN" \
  https://localhost:5001/api/workflows/types/4workflowtype/enable
```

`200 OK` returns the complete workflow type with `"isEnabled": true`:

```json
{
  "workflowTypeId": "4workflowtype",
  "name": "Remote Workflow",
  "isEnabled": true,
  "isSingleton": false,
  "lockTimeout": 0,
  "lockExpiration": 0,
  "deleteFinishedWorkflows": false,
  "activities": [],
  "transitions": []
}
```

The stored graph is returned; the empty arrays above represent a minimal response shape, not a
valid graph for creation. Enabling an already enabled type is a convergent retry and returns the
same `200` representation. Also returns `401`, `403`, or `404`.

### Disable a workflow type

`POST /api/workflows/types/{workflowTypeId}/disable`

`workflowTypeId` is required. No body or query parameters.

```bash
curl -X POST -H "Authorization: Bearer $TOKEN" \
  https://localhost:5001/api/workflows/types/4workflowtype/disable
```

`200 OK` returns the complete workflow type with `"isEnabled": false`:

```json
{
  "workflowTypeId": "4workflowtype",
  "name": "Remote Workflow",
  "isEnabled": false,
  "isSingleton": false,
  "lockTimeout": 0,
  "lockExpiration": 0,
  "deleteFinishedWorkflows": false,
  "activities": [],
  "transitions": []
}
```

Disabling an already disabled type is a convergent retry and returns the same `200`
representation. Also returns `401`, `403`, or `404`.

### Execute a workflow type

`POST /api/workflows/types/{workflowTypeId}/execute`

`workflowTypeId` is a required string path parameter. The JSON body has:

| Property | Type | Required | Default/behavior |
| --- | --- | --- | --- |
| `startActivityId` | string or null | No | If supplied, must identify an activity in the type. Otherwise normal start activities are used. |
| `correlationId` | string or null | No | Passed to the new instance. |
| `input` | object | No | Defaults to `{}`; arbitrary workflow input values. |

```bash
curl -X POST -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "startActivityId": "set-output",
    "correlationId": "content:4xyz",
    "input": {
      "requestedBy": "api"
    }
  }' \
  https://localhost:5001/api/workflows/types/4workflowtype/execute
```

`200 OK` returns the resulting workflow state, dynamic output, and the persisted
`executedActivityIds` collection. A newly executed workflow currently returns an empty collection:

```json
{
  "workflow": {
    "id": 0,
    "workflowId": "4workflowinstance",
    "workflowTypeId": "4workflowtype",
    "correlationId": "content:4xyz",
    "state": {},
    "status": 5,
    "faultMessage": null,
    "lockTimeout": 0,
    "lockExpiration": 0,
    "blockingActivities": [],
    "createdUtc": "2026-08-29T01:25:00Z",
    "isAtomic": false
  },
  "output": {
    "result": 42
  },
  "executedActivityIds": []
}
```

If the type is disabled, `400` has error key `IsEnabled` and message
`The workflow type must be enabled before it can be executed.`. An unknown start activity uses
key `StartActivityId` and message `The specified start activity does not exist.`. Also returns
`401`, `403`, or `404`.

Each successful request starts a workflow execution. Retrying this command is intentionally not
idempotent, even when the body is unchanged.

## Workflow instance operations

### List workflow instances

`GET /api/workflows/instances`

| Query parameter | Type | Required | Default/constraints |
| --- | --- | --- | --- |
| `workflowTypeId` | string | No | No filter; when supplied, limits instances to this type. |
| `skip` | integer | No | `0`, minimum `0`. |
| `take` | integer | No | `20`, from `1` through `200`. |

```bash
curl -H "Authorization: Bearer $TOKEN" \
  "https://localhost:5001/api/workflows/instances?workflowTypeId=4workflowtype&skip=0&take=20"
```

`200 OK` returns:

```json
{
  "skip": 0,
  "take": 20,
  "totalCount": 1,
  "items": [
    {
      "id": 1,
      "workflowId": "4workflowinstance",
      "workflowTypeId": "4workflowtype",
      "correlationId": "content:4xyz",
      "state": {},
      "status": 5,
      "faultMessage": null,
      "lockTimeout": 0,
      "lockExpiration": 0,
      "blockingActivities": [],
      "createdUtc": "2026-08-29T01:25:00Z",
      "isAtomic": false
    }
  ]
}
```

Also returns `400`, `401`, or `403`.

### Get a workflow instance

`GET /api/workflows/instances/{workflowId}`

`workflowId` is a required string path parameter. No body or query parameters.

```bash
curl -H "Authorization: Bearer $TOKEN" \
  https://localhost:5001/api/workflows/instances/4workflowinstance
```

`200 OK` returns:

```json
{
  "id": 1,
  "workflowId": "4workflowinstance",
  "workflowTypeId": "4workflowtype",
  "correlationId": "content:4xyz",
  "state": {},
  "status": 5,
  "faultMessage": null,
  "lockTimeout": 0,
  "lockExpiration": 0,
  "blockingActivities": [],
  "createdUtc": "2026-08-29T01:25:00Z",
  "isAtomic": false
}
```

Also returns `401`, `403`, or `404`.

### Cancel a workflow instance

`DELETE /api/workflows/instances/{workflowId}`

`workflowId` is a required string path parameter. No body or query parameters. Cancellation
deletes the persisted workflow state; it does not return the deleted instance.

```bash
curl -X DELETE -H "Authorization: Bearer $TOKEN" \
  https://localhost:5001/api/workflows/instances/4workflowinstance
```

`204 No Content` has no body. Canceling an already absent workflow instance is a convergent retry
and also returns `204`. Other responses are `401` or `403`.

## Endpoint coverage and sources

This page covers all 14 routes mapped by:

* `src/OrchardCore.Modules/OrchardCore.Workflows/Endpoints/Api/WorkflowManagementApiEndpoints.cs`

Contracts and behavior were also derived from:

* `src/OrchardCore.Modules/OrchardCore.Workflows/Models/Api/WorkflowApiModels.cs`
* `src/OrchardCore.Modules/OrchardCore.Workflows/Services/WorkflowApiService.cs`
* `src/OrchardCore/OrchardCore.Workflows.Abstractions/Models/`
* `test/OrchardCore.Tests/Apis/WorkflowManagement/WorkflowApiTests.cs`
* `test/OrchardCore.Tests/Apis/RemoteManagement/ContentAndWorkflowEndpointMetadataTests.cs`
