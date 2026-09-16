# Feature management API (`OrchardCore.Features`)

The Feature management API lists the non-theme features available to a tenant and enables or disables them through Orchard Core's shell feature manager.

The owning feature is `OrchardCore.Features` (**Features**). It is always enabled. Enable and configure `OrchardCore.RemoteManagement` before calling these endpoints; see [Remote Management authentication](../authentication/README.md).

## Authentication and authorization

Every operation requires:

- an access token accepted by the `Api` authentication scheme, sent as `Authorization: Bearer <token>`;
- an authenticated principal with **Access remote management API** (`AccessRemoteManagement`);
- **Manage Features** (`ManageFeatures`).

Antiforgery validation is disabled for these bearer-token endpoints. An absent or invalid token produces `401 Unauthorized`. An authenticated principal missing either permission produces `403 Forbidden`.

## Base route

All paths are relative to the tenant URL:

```text
/api/features
```

For a tenant at `https://cms.example.com/acme`, the collection URL is `https://cms.example.com/acme/api/features`.

## Endpoints

| Method | Path | OpenAPI operation ID | Description | Permission |
| --- | --- | --- | --- | --- |
| `GET` | `/api/features` | `ApiListFeatures` | List and filter non-theme features. | `ManageFeatures` |
| `GET` | `/api/features/{featureId}` | `ApiGetFeature` | Get one feature. | `ManageFeatures` |
| `POST` | `/api/features/{featureId}:enable` | `ApiEnableFeature` | Enable a feature and its dependencies. | `ManageFeatures` |
| `POST` | `/api/features/{featureId}:disable` | `ApiDisableFeature` | Disable a feature after dependency checks. | `ManageFeatures` |

## Feature representation

Feature responses use this shape:

| Property | Type | Description |
| --- | --- | --- |
| `id` | string | Feature ID from the feature manifest. |
| `name` | string or `null` | Display name from the feature manifest. |
| `category` | string or `null` | Manifest category. |
| `description` | string or `null` | Manifest description. |
| `isEnabled` | boolean | Whether the feature is enabled for the tenant. |
| `isAlwaysEnabled` | boolean | Whether the feature cannot be disabled. |
| `enabledByDependencyOnly` | boolean | Whether the feature is intended to be enabled only as a dependency. |
| `defaultTenantOnly` | boolean | Whether the feature is available only on the default tenant. |
| `extensionId` | string or `null` | ID of the module containing the feature. |
| `extensionName` | string or `null` | Display name of that module. |
| `dependencies` | array of strings | Transitive feature dependency IDs, sorted case-insensitively. |
| `enabledDependents` | array of strings | IDs of currently enabled features that depend on this feature, sorted case-insensitively. |

Manifest-contributed strings and relationship arrays are dynamic and depend on the extensions installed for the tenant.

## Paging envelope

The list operation returns:

| Property | Type | Description |
| --- | --- | --- |
| `skip` | integer | Applied zero-based offset. |
| `take` | integer | Requested page size. |
| `totalCount` | integer | Number of matches before paging. |
| `items` | array of feature objects | Current page, containing at most `take` entries. |

## List features

```http
GET /api/features
```

Returns available non-theme module features, ordered by category and then feature ID, both case-insensitively.

### Query parameters

| Name | Type | Required | Default | Constraints and behavior |
| --- | --- | --- | --- | --- |
| `skip` | integer | No | `0` | Zero or greater. |
| `take` | integer | No | `50` | From `1` through `200`. |
| `search` | string | No | — | Case-insensitive substring match against ID, name, or description. |
| `category` | string | No | — | Case-insensitive exact category match. |
| `enabled` | boolean | No | — | `true` returns enabled features; `false` returns disabled features. |

### Request

```http
GET /api/features?category=Content%20Management&enabled=true&skip=0&take=50
Authorization: Bearer eyJ...
Accept: application/json
```

### `200 OK`

```json
{
  "skip": 0,
  "take": 50,
  "totalCount": 1,
  "items": [
    {
      "id": "OrchardCore.Contents",
      "name": "Contents",
      "category": "Content Management",
      "description": "The contents module enables the edition and rendering of content items.",
      "isEnabled": true,
      "isAlwaysEnabled": false,
      "enabledByDependencyOnly": false,
      "defaultTenantOnly": false,
      "extensionId": "OrchardCore.Contents",
      "extensionName": "Contents",
      "dependencies": [
        "OrchardCore.Liquid",
        "OrchardCore.Settings"
      ],
      "enabledDependents": []
    }
  ]
}
```

`totalCount` is the number of matching features before paging. `items` contains at most `take` entries.

### Responses

| Status | Meaning |
| --- | --- |
| `200 OK` | The paged feature list. |
| `400 Bad Request` | Invalid `skip` or `take`. |
| `401 Unauthorized` | The bearer token is absent or rejected. |
| `403 Forbidden` | A required permission is missing. |

## Get a feature

```http
GET /api/features/{featureId}
```

### Path parameters

| Name | Type | Required | Constraints |
| --- | --- | --- | --- |
| `featureId` | string | Yes | A feature manifest ID. Lookup is case-insensitive. URL-encode the value as a path segment. |

### Request

```http
GET /api/features/OrchardCore.Contents
Authorization: Bearer eyJ...
Accept: application/json
```

### `200 OK`

```json
{
  "id": "OrchardCore.Contents",
  "name": "Contents",
  "category": "Content Management",
  "description": "The contents module enables the edition and rendering of content items.",
  "isEnabled": true,
  "isAlwaysEnabled": false,
  "enabledByDependencyOnly": false,
  "defaultTenantOnly": false,
  "extensionId": "OrchardCore.Contents",
  "extensionName": "Contents",
  "dependencies": [
    "OrchardCore.Liquid",
    "OrchardCore.Settings"
  ],
  "enabledDependents": []
}
```

### Responses

| Status | Meaning |
| --- | --- |
| `200 OK` | The feature. |
| `401 Unauthorized` | The bearer token is absent or rejected. |
| `403 Forbidden` | A required permission is missing. |
| `404 Not Found` | No non-theme feature has the supplied ID. |

## Enable a feature

```http
POST /api/features/{featureId}:enable
```

The shell feature manager resolves and enables dependencies. The operation is idempotent when the requested feature is already enabled.

### Parameters

| Location | Name | Type | Required | Default | Constraints and behavior |
| --- | --- | --- | --- | --- | --- |
| Path | `featureId` | string | Yes | — | Available feature ID, using the exact casing returned by list or get. |
| Query | `force` | boolean | No | `false` | When `true`, also enables missing transitive dependencies. Without it, enabling fails if additional features must be enabled. It does not override the active feature profile. |

There is no request body.

### Request

```http
POST /api/features/OrchardCore.Media:enable?force=false
Authorization: Bearer eyJ...
Accept: application/json
```

### `200 OK`

The response is the updated [feature representation](#feature-representation), with `isEnabled` equal to `true`.

```json
{
  "id": "OrchardCore.Media",
  "name": "Media",
  "category": "Content Management",
  "description": "The media module adds media management support.",
  "isEnabled": true,
  "isAlwaysEnabled": false,
  "enabledByDependencyOnly": false,
  "defaultTenantOnly": false,
  "extensionId": "OrchardCore.Media",
  "extensionName": "Media",
  "dependencies": [
    "OrchardCore.ContentTypes"
  ],
  "enabledDependents": []
}
```

### Responses

| Status | Meaning |
| --- | --- |
| `200 OK` | The feature is enabled, or was already enabled. |
| `400 Bad Request` | The feature manager did not enable a previously disabled feature; dependencies or the active feature profile can prevent the operation. |
| `401 Unauthorized` | The bearer token is absent or rejected. |
| `403 Forbidden` | A required permission is missing. |
| `404 Not Found` | The feature ID is unavailable. |

## Disable a feature

```http
POST /api/features/{featureId}:disable
```

The shell feature manager checks enabled dependents before disabling the feature. The operation is idempotent when the feature is already disabled.

### Parameters

| Location | Name | Type | Required | Default | Constraints and behavior |
| --- | --- | --- | --- | --- | --- |
| Path | `featureId` | string | Yes | — | Available feature ID, using the exact casing returned by list or get. |
| Query | `force` | boolean | No | `false` | When `true`, also disables enabled transitive dependents. Without it, disabling fails if additional features must be disabled. It does not override the active feature profile or make an always-enabled feature disableable. |

There is no request body.

### Request

```http
POST /api/features/OrchardCore.Media:disable
Authorization: Bearer eyJ...
Accept: application/json
```

### `200 OK`

The response is the updated [feature representation](#feature-representation), with `isEnabled` equal to `false`.

```json
{
  "id": "OrchardCore.Media",
  "name": "Media",
  "category": "Content Management",
  "description": "The media module adds media management support.",
  "isEnabled": false,
  "isAlwaysEnabled": false,
  "enabledByDependencyOnly": false,
  "defaultTenantOnly": false,
  "extensionId": "OrchardCore.Media",
  "extensionName": "Media",
  "dependencies": [
    "OrchardCore.ContentTypes"
  ],
  "enabledDependents": []
}
```

### Responses

| Status | Meaning |
| --- | --- |
| `200 OK` | The feature is disabled, or was already disabled. |
| `400 Bad Request` | The feature is always enabled, or the feature manager did not disable a previously enabled feature because of dependents or the active feature profile. |
| `401 Unauthorized` | The bearer token is absent or rejected. |
| `403 Forbidden` | A required permission is missing. |
| `404 Not Found` | The feature ID is unavailable. |

## Error responses

Application-level failures use `application/problem+json`. English messages shown here are localizable.

Invalid paging:

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "Bad request",
  "status": 400,
  "detail": "Take cannot exceed 200."
}
```

Failure to enable a previously disabled feature:

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "Bad request",
  "status": 400,
  "detail": "The feature 'Example.Feature' could not be enabled. Check its dependencies and the active feature profile."
}
```

An always-enabled feature cannot be disabled:

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "Bad request",
  "status": 400,
  "detail": "You can only disable features that are not always enabled."
}
```

If dependent/profile checks prevent disabling a previously enabled feature, the detail is `The feature '<featureId>' could not be disabled. Check its dependents and the active feature profile.`

Missing feature:

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.5",
  "title": "Not Found",
  "status": 404,
  "detail": "Feature not found: 'Example.Missing'."
}
```

Operation-specific permission failure:

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.4",
  "title": "Forbidden",
  "status": 403,
  "detail": "You do not have sufficient permissions to complete this request"
}
```

Authentication-policy responses are produced by the configured `Api` authentication handler. A `401` includes its challenge, including `WWW-Authenticate` when supplied by that handler; policy-level `403` response bodies are handler-dependent.

## Endpoint coverage and sources

This page covers all 4 routes mapped by:

- `src/OrchardCore.Modules/OrchardCore.Features/Endpoints/Management/FeatureManagementEndpoints.cs`

Feature registration, permissions, behavior, and metadata were cross-checked against:

- `src/OrchardCore.Modules/OrchardCore.Features/Startup.cs`
- `src/OrchardCore.Modules/OrchardCore.Features/Manifest.cs`
- `src/OrchardCore.Modules/OrchardCore.Contents/Manifest.cs`
- `src/OrchardCore.Modules/OrchardCore.Media/Manifest.cs`
- `src/OrchardCore/OrchardCore.Features.Core/FeaturesPermissions.cs`
- `test/OrchardCore.Tests/Modules/OrchardCore.Features/FeatureManagementEndpointsTests.cs`
- `test/OrchardCore.Tests/Apis/RemoteManagement/OwnedModuleEndpointMetadataTests.cs`


## CLI confirmation and dependency handling

`pomi features disable <feature-id> --force` skips the CLI confirmation prompt.
It does not change the API's dependency behavior. The HTTP `force` query
parameter is exposed as `--api-force true` in the CLI, for both enable and
disable. To deliberately disable dependents without an interactive prompt, use
`pomi features disable <feature-id> --force --api-force true`. Neither option
bypasses authorization or the active feature profile.
