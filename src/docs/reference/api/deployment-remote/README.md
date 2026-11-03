# Remote Deployment management API

The **Remote Deployment** feature (`OrchardCore.Deployment.Remote`) manages the remote clients allowed to send packages to this tenant, the remote instances it can send packages to, and sends a deployment plan to one of them. It advertises the `deployment-remote` remote-management capability and contributes `pomi deployment` commands and MCP tools. It depends on [Deployment](../deployment/README.md).

For the workflow and the site-to-site protocol, see the [Remote Deployment module](../../modules/Deployment.Remote/README.md).

## Authentication and permissions

Every operation requires an `Api` scheme bearer token and **Access remote management API** (`AccessRemoteManagement`), with one of the following:

| Operations | Additional permission |
| --- | --- |
| Remote clients | `ManageRemoteClients` |
| Remote instances | `ManageRemoteInstances` |
| Listing targets | `ExportRemoteInstances` |
| Sending to a target | `ExportRemoteInstances` and `Export`, and the permissions the sources of the plan enforce |

Every mutation, as well as sending, requires HTTPS: a request over HTTP returns `400`.

## Base route

`/api/deployment`

## Endpoints

| Method | Endpoint | Operation ID | Pomi command | Permission |
| --- | --- | --- | --- | --- |
| `GET` | `/api/deployment/remote-clients` | `ApiListRemoteDeploymentClients` | `deployment remote-clients list` | `ManageRemoteClients` |
| `GET` | `/api/deployment/remote-clients/{id}` | `ApiGetRemoteDeploymentClient` | `deployment remote-clients show <id>` | `ManageRemoteClients` |
| `POST` | `/api/deployment/remote-clients` | `ApiCreateRemoteDeploymentClient` | `deployment remote-clients create` | `ManageRemoteClients` |
| `PUT` | `/api/deployment/remote-clients/{id}` | `ApiUpdateRemoteDeploymentClient` | `deployment remote-clients update <id>` | `ManageRemoteClients` |
| `DELETE` | `/api/deployment/remote-clients/{id}` | `ApiDeleteRemoteDeploymentClient` | `deployment remote-clients delete <id> --force` | `ManageRemoteClients` |
| `GET` | `/api/deployment/remote-instances` | `ApiListRemoteDeploymentInstances` | `deployment remote-instances list` | `ManageRemoteInstances` |
| `GET` | `/api/deployment/remote-instances/{id}` | `ApiGetRemoteDeploymentInstance` | `deployment remote-instances show <id>` | `ManageRemoteInstances` |
| `POST` | `/api/deployment/remote-instances` | `ApiCreateRemoteDeploymentInstance` | `deployment remote-instances create` | `ManageRemoteInstances` |
| `PUT` | `/api/deployment/remote-instances/{id}` | `ApiUpdateRemoteDeploymentInstance` | `deployment remote-instances update <id>` | `ManageRemoteInstances` |
| `DELETE` | `/api/deployment/remote-instances/{id}` | `ApiDeleteRemoteDeploymentInstance` | `deployment remote-instances delete <id> --force` | `ManageRemoteInstances` |
| `GET` | `/api/deployment/targets` | `ApiListRemoteDeploymentTargets` | `deployment targets list` | `ExportRemoteInstances` |
| `POST` | `/api/deployment/targets/{id}:send` | `ApiSendToRemoteDeploymentTarget` | `deployment targets send <id> --plan-id <plan> --force` | `ExportRemoteInstances`, `Export` |

### Credentials in Pomi and MCP

`apiKey` is a secret. Pomi doesn't accept it on the command line: supply it with `--api-key-env`, `--api-key-file` or `--api-key-stdin`, or in a private `--body-file`. The create and update operations are not MCP tools, as their arguments would reach the model's context. The other operations are tools, except delete and send, which need `AllowConfirmationRequiredTools` as Pomi asks a confirmation for them. See [MCP server](../../modules/RemoteManagement/README.md#mcp-server).

## Remote clients

A remote client is a name and an API key that another site uses to send packages to this tenant. The key is protected with ASP.NET Core Data Protection, and is never returned.

```json
{
  "id": "4h9c2m7qw1",
  "clientName": "staging",
  "hasApiKey": true
}
```

### List and show clients

`GET /api/deployment/remote-clients` returns an array ordered by `clientName`, without paging. `GET /api/deployment/remote-clients/{id}` returns one client, or `404`.

### Create a client

`POST /api/deployment/remote-clients`

```json
{
  "clientName": "staging",
  "apiKey": "<generated-key>"
}
```

| Property | Type | Required | Constraints |
| --- | --- | --- | --- |
| `clientName` | string | Yes | At most 256 characters. |
| `apiKey` | string | Yes | At most 4096 characters. |

`200` returns `changed` and `resource`:

```json
{
  "changed": true,
  "resource": { "id": "4h9c2m7qw1", "clientName": "staging", "hasApiKey": true }
}
```

Repeating the request with the same name and key returns `changed: false`. The same name with another key returns `409`. An invalid body returns a `400` validation problem.

### Update a client

`PUT /api/deployment/remote-clients/{id}`

`clientName` is required. An omitted or null `apiKey` keeps the stored one, and an empty one returns `400`: delete the client to revoke it. An absent client returns `404`, and a name used by another client `409`. An equivalent update returns `changed: false`.

### Delete a client

`DELETE /api/deployment/remote-clients/{id}`

Returns `200` with `changed: true`, or `changed: false` when the client is already absent. `resource` is null.

## Remote instances

A remote instance is a destination this tenant can send packages to. The source site stores its API key as it was supplied, so restrict access to the tenant database, its backups and `ManageRemoteInstances`. The key is never returned.

```json
{
  "id": "7x2p8k3vn5",
  "name": "Production",
  "url": "https://example.com/OrchardCore.Deployment.Remote/ImportRemoteInstance/Import",
  "clientName": "staging",
  "hasApiKey": true
}
```

### List and show instances

`GET /api/deployment/remote-instances` returns an array ordered by `name`, without paging. `GET /api/deployment/remote-instances/{id}` returns one instance, or `404`.

### Create an instance

`POST /api/deployment/remote-instances`

| Property | Type | Required | Constraints |
| --- | --- | --- | --- |
| `name` | string | Yes | At most 256 characters. |
| `url` | string | Yes | An absolute HTTPS URL of at most 2048 characters, without user information or a fragment. HTTP is only accepted for a loopback address. |
| `clientName` | string | Yes | At most 256 characters. |
| `apiKey` | string | Yes | At most 4096 characters. |

`200` returns `changed` and `resource`. Repeating the request with the same name and values returns `changed: false`, and the same name with other values returns `409`. An invalid body returns a `400` validation problem.

### Update an instance

`PUT /api/deployment/remote-instances/{id}`

All the properties except `apiKey` are required. An omitted or null `apiKey` keeps the stored key, unless `url` changes: a stored key doesn't follow a new destination, so the request must then supply the key, or it returns `400`. An absent instance returns `404`, and a name used by another instance `409`. An equivalent update returns `changed: false`.

### Delete an instance

`DELETE /api/deployment/remote-instances/{id}`

Returns `200` with `changed: true`, or `changed: false` when the instance is already absent. `resource` is null.

## Targets

### List targets

`GET /api/deployment/targets`

Returns the remote instances ordered by name, with only what sending needs. This lets an operator send without being able to read or change the destinations.

```json
[{ "id": "7x2p8k3vn5", "name": "Production" }]
```

### Send a plan

`POST /api/deployment/targets/{id}:send`

```json
{ "planId": 123 }
```

Builds the plan's package and sends it once to the remote instance, which imports it. Redirects aren't followed, so that the API key can't reach another location, and the request times out after two minutes.

| Status | Meaning |
| --- | --- |
| `200` | `{ "succeeded": true }`: the destination confirmed the import. |
| `400` | A nonpositive `planId`, a request over HTTP, or a stored URL that doesn't meet the rules above. |
| `403` | The caller lacks `Export`, or a source of the plan denied the export. |
| `404` | The instance or the plan doesn't exist. |
| `502` | The deployment wasn't confirmed. |

`502` always has the same message, so that it doesn't reveal which hosts the server can reach. It can happen after the import has started, and an import can commit partially: inspect the destination before sending again.

Sending isn't idempotent. Each successful request exports and imports again, and nothing retries automatically.

## Errors

A missing resource on read and update returns an empty `404`. Other failures use Problem Details, except invalid bodies, which use a validation problem keyed by property.

## Sources

- [Client and instance endpoints](https://github.com/OrchardCMS/OrchardCore/blob/main/src/OrchardCore.Modules/OrchardCore.Deployment.Remote/Endpoints/RemoteDeploymentEndpoints.cs)
- [Target endpoints](https://github.com/OrchardCMS/OrchardCore/blob/main/src/OrchardCore.Modules/OrchardCore.Deployment.Remote/Endpoints/RemoteDeploymentTargetEndpoints.cs)
- [Remote Deployment module](../../modules/Deployment.Remote/README.md)
