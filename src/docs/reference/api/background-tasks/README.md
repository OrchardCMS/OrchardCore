# Background tasks management API

The **Background Tasks** feature (`OrchardCore.BackgroundTasks`) lists the tasks registered in a tenant and changes their settings. It advertises the `background-tasks` remote-management capability and contributes the `pomi background-tasks` commands and MCP tools.

These operations don't offer immediate execution, cancellation or an execution history. For the module itself, see [Background Tasks](../../modules/BackgroundTasks/README.md).

## Authentication and permissions

Every operation requires an `Api` scheme bearer token, **Access remote management API** (`AccessRemoteManagement`) and **Manage background tasks** (`ManageBackgroundTasks`). An absent or invalid token returns `401`, a missing permission returns `403`.

## Base route

`/api/background-tasks`

## Endpoints

| Method | Endpoint | Operation ID | Pomi command |
| --- | --- | --- | --- |
| `GET` | `/api/background-tasks` | `ApiListBackgroundTasks` | `background-tasks list` |
| `GET` | `/api/background-tasks/by-name?name={name}` | `ApiGetBackgroundTask` | `background-tasks show <name>` |
| `POST` | `/api/background-tasks:validate` | `ApiValidateBackgroundTask` | `background-tasks validate` |
| `PUT` | `/api/background-tasks/by-name?name={name}` | `ApiUpdateBackgroundTask` | `background-tasks update <name>` |
| `POST` | `/api/background-tasks/by-name:enable?name={name}` | `ApiEnableBackgroundTask` | `background-tasks enable <name>` |
| `POST` | `/api/background-tasks/by-name:disable?name={name}` | `ApiDisableBackgroundTask` | `background-tasks disable <name>` |

All these operations are MCP tools. See [MCP server](../../modules/RemoteManagement/README.md#mcp-server).

## Task representation

```json
{
  "name": "OrchardCore.Example.ExampleBackgroundTask",
  "title": "Example task",
  "enabled": true,
  "configuration": {
    "schedule": "*/15 * * * *",
    "description": "Run every fifteen minutes",
    "lockTimeout": 3000,
    "lockExpiration": 30000,
    "usePipeline": false
  }
}
```

`name` comes from the registered implementation: copy it rather than inferring it from the title. The configuration combines the task's defaults with the tenant's overrides. Stale settings of a task that isn't registered are not listed.

## List tasks

`GET /api/background-tasks`

| Location | Name | Type | Required | Description |
| --- | --- | --- | --- | --- |
| Query | `search` | string | No | Case-insensitive search in the name, title and description, at most 1024 characters. |
| Query | `enabled` | boolean | No | Filters on the status. |
| Query | `skip` | integer | No | Zero-based offset. Default `0`, minimum `0`. |
| Query | `take` | integer | No | Page size. Default `50`, range `1`–`200`. |

`200` returns an array ordered by name, without a count. Other values return `400`.

## Show a task

`GET /api/background-tasks/by-name?name={name}`

`200` returns the task, and an unknown name `404`.

## Validate settings

`POST /api/background-tasks:validate`

Checks a configuration without saving it or running a task. The body is the configuration below, and repeating the request has no effect.

```json
{ "isValid": false, "errors": { "schedule": ["Provide a valid five-field cron expression."] } }
```

Invalid settings also return `200`, with `isValid: false`.

## Update a task

`PUT /api/background-tasks/by-name?name={name}`

```json
{
  "schedule": "*/15 * * * *",
  "description": "Run every fifteen minutes",
  "lockTimeout": 3000,
  "lockExpiration": 30000,
  "usePipeline": false
}
```

| Property | Type | Required | Description |
| --- | --- | --- | --- |
| `schedule` | string | Yes | A five-field cron expression of at most 256 characters, with the scheduler's time-zone behavior. |
| `description` | string | No | An administrative description. |
| `lockTimeout` | integer | No | The lock acquisition timeout in milliseconds, nonnegative. |
| `lockExpiration` | integer | No | The lock expiration in milliseconds, nonnegative. |
| `usePipeline` | boolean | No | Whether the execution initializes and runs the tenant's request pipeline. |

The update replaces the complete configuration, and keeps the status and the identity of the task. Read `configuration` first: an omitted number or boolean resets to zero or false, and an omitted description to null. Locking is atomic only when both lock values are positive and a distributed lock is registered.

`200` returns the task. Invalid settings return a `400` validation problem keyed by property, and an unknown name `404`. Identical settings don't write anything; a change makes the scheduler reload its configuration.

## Enable or disable a task

`POST /api/background-tasks/by-name:enable?name={name}` and `POST /api/background-tasks/by-name:disable?name={name}`

Return `204`, also when the task is already in that state, without writing anything. Disabling only affects future scheduling and doesn't cancel running work. Enabling a task with invalid stored settings returns a `400` validation problem: repair them with an update. An unknown name returns `404`.

## Errors

Failures use Problem Details, except invalid settings, which use a validation problem keyed by property.

## Sources

- [Endpoint mappings and handlers](https://github.com/OrchardCMS/OrchardCore/blob/main/src/OrchardCore.Modules/OrchardCore.BackgroundTasks/Endpoints/BackgroundTaskEndpoints.cs)
- [Management service](https://github.com/OrchardCMS/OrchardCore/blob/main/src/OrchardCore.Modules/OrchardCore.BackgroundTasks/Services/BackgroundTaskManagementService.cs)
- [Background Tasks module](../../modules/BackgroundTasks/README.md)
