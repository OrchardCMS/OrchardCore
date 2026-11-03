# Rate Limits management API

The **Rate Limits** feature (`OrchardCore.RateLimits`) manages the tenant's rate-limit policies and their limiters. It advertises the `rate-limits` remote-management capability and contributes the `pomi rate-limits` commands and MCP tools. Enabling Rate Limits doesn't configure OpenID or remote management.

For the behavior of the policies at runtime, see the [Rate Limits module](../../modules/RateLimits/README.md).

## Authentication and permissions

Every operation requires an `Api` scheme bearer token, **Access remote management API** (`AccessRemoteManagement`) and **Manage Rate Limits** (`ManageRateLimits`). An absent or invalid token returns `401`, a missing permission returns `403`.

The seeded Default Global Policy also limits discovery, management and token requests. Budget automation traffic accordingly, or configure the policy for the intended workload.

## Base route

`/api/rate-limits`

## Endpoints

| Method | Endpoint | Operation ID | Pomi command |
| --- | --- | --- | --- |
| `GET` | `/api/rate-limits/limiter-types` | `ApiListRateLimitSources` | `rate-limits limiter-types list` |
| `GET` | `/api/rate-limits/policies` | `ApiListRateLimitPolicies` | `rate-limits policies list` |
| `GET` | `/api/rate-limits/policies/by-id?policyId={id}` | `ApiGetRateLimitPolicy` | `rate-limits policies show <policyId>` |
| `POST` | `/api/rate-limits/policies` | `ApiCreateRateLimitPolicy` | `rate-limits policies create` |
| `PUT` | `/api/rate-limits/policies/by-id?policyId={id}` | `ApiUpdateRateLimitPolicy` | `rate-limits policies update <policyId>` |
| `DELETE` | `/api/rate-limits/policies/by-id?policyId={id}` | `ApiDeleteRateLimitPolicy` | `rate-limits policies delete <policyId> --force` |
| `POST` | `/api/rate-limits/policies/by-id:enable?policyId={id}` | `ApiEnableRateLimitPolicy` | `rate-limits policies enable <policyId>` |
| `POST` | `/api/rate-limits/policies/by-id:disable?policyId={id}` | `ApiDisableRateLimitPolicy` | `rate-limits policies disable <policyId>` |
| `GET` | `/api/rate-limits/policies/limiters?policyId={id}` | `ApiListRateLimitLimiters` | `rate-limits policies limiters list <policyId>` |
| `GET` | `/api/rate-limits/policies/limiters/by-id?policyId={id}&limiterId={id}` | `ApiGetRateLimitLimiter` | `rate-limits policies limiters show <policyId> <limiterId>` |
| `POST` | `/api/rate-limits/policies/limiters?policyId={id}` | `ApiAddRateLimitLimiter` | `rate-limits policies limiters add <policyId>` |
| `PUT` | `/api/rate-limits/policies/limiters/by-id?policyId={id}&limiterId={id}` | `ApiUpdateRateLimitLimiter` | `rate-limits policies limiters update <policyId> <limiterId>` |
| `DELETE` | `/api/rate-limits/policies/limiters/by-id?policyId={id}&limiterId={id}` | `ApiDeleteRateLimitLimiter` | `rate-limits policies limiters delete <policyId> <limiterId> --force` |

These operations are MCP tools, except the deletions, which need `AllowConfirmationRequiredTools`. See [MCP server](../../modules/RemoteManagement/README.md#mcp-server).

Every operation with a `policyId` returns `400` when it is missing, and `404` when the policy is absent.

## Limiter types

`GET /api/rate-limits/limiter-types`

Returns the built-in sources, `FixedWindow`, `SlidingWindow`, `Concurrency` and `TokenBucket`, with the JSON Schema of their complete settings:

```json
[
  {
    "source": "FixedWindow",
    "schema": {
      "type": "object",
      "additionalProperties": false,
      "required": ["permitLimit", "windowSeconds"],
      "properties": {
        "permitLimit": { "type": "integer", "minimum": 1, "maximum": 2147483647 },
        "queueLimit": { "type": "integer", "minimum": 0, "maximum": 2147483647 },
        "windowSeconds": { "type": "integer", "minimum": 1, "maximum": 2147483647 }
      }
    }
  }
]
```

Numeric limits and periods must be positive, and `queueLimit` nonnegative. Omitted queue settings mean no queue and oldest-first processing.

## Policies

### Policy representation

```json
{
  "policyId": "5k8d2n1xq",
  "definition": {
    "name": "Public search",
    "description": "Limit requests to the public search path",
    "scope": "Endpoint",
    "path": "/search",
    "groupName": null
  },
  "isEnabled": false,
  "enabledUtc": null,
  "limiters": []
}
```

### List policies

`GET /api/rate-limits/policies`

| Location | Name | Type | Required | Description |
| --- | --- | --- | --- | --- |
| Query | `skip` | integer | No | Zero-based offset. Default `0`, minimum `0`. |
| Query | `take` | integer | No | Page size. Default `50`, range `1`–`200`. |

`200` returns an array of policies ordered by name, without a count. Other paging values return `400`.

### Show a policy

`GET /api/rate-limits/policies/by-id?policyId=5k8d2n1xq`

`200` returns the policy representation.

### Create a policy

`POST /api/rate-limits/policies`

```json
{
  "name": "Public search",
  "description": "Limit requests to the public search path",
  "scope": "Endpoint",
  "path": "/search"
}
```

| Property | Type | Required | Description |
| --- | --- | --- | --- |
| `name` | string | Yes | Unique in the tenant, case-insensitively, at most 256 characters. |
| `description` | string | No | A description. |
| `scope` | string | Yes | `Global`, `Endpoint` or `Group`. |
| `path` | string | For `Endpoint` | An absolute request path prefix. |
| `groupName` | string | For `Group` | The group name. |

A policy is created disabled. `201` returns the policy, with a `Location` header. Repeating the request with the same name and definition also returns `201`, with the existing policy, and doesn't change it. The same name with another definition returns `409`, and an invalid definition `400`.

### Update a policy

`PUT /api/rate-limits/policies/by-id?policyId=5k8d2n1xq`

Replaces the complete metadata and target, with the body of creation. Read `definition` before editing. The limiters and the status don't change.

While the policy is enabled, the name and the description can change, but a different target returns `409`: disable it first. A name used by another policy also returns `409`. An unchanged definition doesn't write anything.

### Delete a policy

`DELETE /api/rate-limits/policies/by-id?policyId=5k8d2n1xq`

Returns `204`. Deleting an active policy reloads the tenant's rate limiting.

### Enable or disable a policy

`POST /api/rate-limits/policies/by-id:enable?policyId=5k8d2n1xq` and `POST /api/rate-limits/policies/by-id:disable?policyId=5k8d2n1xq`

Return `204`, and reload the tenant's rate limiting when the status changes, so repeating a request has no further effect. Check the actual behavior after enabling, including `429` responses, and choose a path that doesn't include the management or token endpoints.

## Limiters

### Limiter representation

```json
{
  "id": "search-window",
  "source": "FixedWindow",
  "canConfigure": true,
  "values": {
    "permitLimit": 60,
    "queueLimit": 0,
    "windowSeconds": 60
  }
}
```

A source registered by another feature appears with `canConfigure: false` and null `values`.

### List and show limiters

`GET /api/rate-limits/policies/limiters?policyId={id}` returns an array in the policy's order. `GET /api/rate-limits/policies/limiters/by-id?policyId={id}&limiterId={id}` returns one limiter, or `404`.

### Add a limiter

`POST /api/rate-limits/policies/limiters?policyId=5k8d2n1xq`

```json
{
  "id": "search-window",
  "source": "FixedWindow",
  "values": {
    "permitLimit": 60,
    "windowSeconds": 60,
    "queueLimit": 0
  }
}
```

| Property | Type | Required | Description |
| --- | --- | --- | --- |
| `id` | string | Yes | Stable identifier chosen by the caller, at most 128 characters. It makes the request retry-safe. |
| `source` | string | Yes | One of the built-in sources. |
| `values` | object | Yes | The complete settings of the source. |

`200` returns the limiter. Repeating the request with the same settings returns the existing limiter, even when the policy is enabled. Other settings under the same `id`, another `source`, or a change to an enabled policy return `409`. Unknown properties and invalid values return a `400` validation problem keyed with `values.`, such as `values.permitLimit`.

### Update a limiter

`PUT /api/rate-limits/policies/limiters/by-id?policyId=5k8d2n1xq&limiterId=search-window`

Replaces the complete settings, with the body of adding a limiter. `id` must match `limiterId`, otherwise the request returns `400`. The position and the source don't change. An absent limiter returns `404`, and a change to an enabled policy `409`. Identical settings don't write anything.

### Delete a limiter

`DELETE /api/rate-limits/policies/limiters/by-id?policyId=5k8d2n1xq&limiterId=search-window`

Returns `204`, and `404` when the limiter is absent. A deletion from an enabled policy returns `409`: disable it first.

## Errors

Failures use Problem Details, except invalid limiter settings, which use a validation problem.

## Sources

- [Endpoint mappings and handlers](https://github.com/OrchardCMS/OrchardCore/blob/main/src/OrchardCore.Modules/OrchardCore.RateLimits/Endpoints/RateLimitEndpoints.cs)
- [Rate Limits module](../../modules/RateLimits/README.md)
