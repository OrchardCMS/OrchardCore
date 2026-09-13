# Role management API (`OrchardCore.Roles`)

The Role management API lists roles and their assigned/effective permissions, creates and updates roles, and deletes non-system roles.

Enable the owning feature, `OrchardCore.Roles` (**Roles**), and configure `OrchardCore.RemoteManagement`. See [Remote Management authentication](../authentication/README.md).

## Authentication and authorization

Every operation first requires:

- an access token accepted by the `Api` authentication scheme, sent as `Authorization: Bearer <token>`;
- an authenticated principal with **Access remote management API** (`AccessRemoteManagement`).

Operations then enforce:

| Operation | Orchard Core permission |
| --- | --- |
| List, get | **View Roles** (`ViewRoles`) |
| Create, update, delete | **Manage Roles** (`ManageRoles`), a security-critical permission |

`ManageRoles` implies `ViewRoles`. Antiforgery validation is disabled. An absent or invalid token produces `401 Unauthorized`; an authenticated principal missing a required permission produces `403 Forbidden`.

## Base route

All paths are relative to the tenant URL:

```text
/api/roles
```

## Endpoints

| Method | Path | OpenAPI operation ID | Description | Permission |
| --- | --- | --- | --- | --- |
| `GET` | `/api/roles` | `ApiListRoles` | List roles and effective permissions. | `ViewRoles` |
| `GET` | `/api/roles/{roleId}` | `ApiGetRole` | Get one role and its permissions. | `ViewRoles` |
| `POST` | `/api/roles` | `ApiCreateRole` | Create a role and assign permissions. | `ManageRoles` |
| `PUT` | `/api/roles/{roleId}` | `ApiUpdateRole` | Update description and permissions. | `ManageRoles` |
| `DELETE` | `/api/roles/{roleId}` | `ApiDeleteRole` | Delete a non-system role. | `ManageRoles` |

## Role representation

| Property | Type | Description |
| --- | --- | --- |
| `roleId` | string | Stable role identifier. In the built-in role store, this is the role name. |
| `roleName` | string | Role name. |
| `roleDescription` | string or `null` | Role description. |
| `isSystemRole` | boolean | Whether the role is protected as a system role. |
| `isAdminRole` | boolean | Whether the role is both a system role and the configured administrator role. |
| `permissionCount` | integer | Count of entries whose `isEffective` value is `true`. |
| `permissions` | array of permission objects | Every permission contributed by enabled features, ordered by permission name case-insensitively. |

Each permission object contains:

| Property | Type | Description |
| --- | --- | --- |
| `name` | string | Exact permission name. |
| `description` | string or `null` | Permission description. |
| `category` | string or `null` | Permission category. |
| `featureId` | string or `null` | ID of the enabled feature that contributed the permission when it can be resolved. |
| `featureName` | string or `null` | Display name of the contributing feature when it can be resolved. |
| `isSecurityCritical` | boolean | Whether the permission is marked security-critical. |
| `isAssigned` | boolean | Whether the role directly contains this permission claim. |
| `isEffective` | boolean | Whether authorization succeeds for this role, including implied permissions. Every installed permission is effective for the administrator role. |

The permission catalog is dynamic: enabled features can add/remove entries and change the feature metadata returned here.

## Paging envelope

The list operation returns:

| Property | Type | Description |
| --- | --- | --- |
| `skip` | integer | Applied zero-based offset. |
| `take` | integer | Requested page size. |
| `totalCount` | integer | Number of matches before paging. |
| `items` | array of role objects | Current page, containing at most `take` entries. |

## List roles

```http
GET /api/roles
```

Roles are ordered by role name case-insensitively.

### Query parameters

| Name | Type | Required | Default | Constraints and behavior |
| --- | --- | --- | --- | --- |
| `skip` | integer | No | `0` | Zero or greater. |
| `take` | integer | No | `50` | From `1` through `200`. |
| `search` | string | No | — | Case-insensitive substring match against role name or description. |

### Request

```http
GET /api/roles?search=editor&skip=0&take=50
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
      "roleId": "Editor",
      "roleName": "Editor",
      "roleDescription": "Edits site content.",
      "isSystemRole": false,
      "isAdminRole": false,
      "permissionCount": 1,
      "permissions": [
        {
          "name": "ManageUsers",
          "description": "Manage users",
          "category": "Users",
          "featureId": "OrchardCore.Users",
          "featureName": "Users",
          "isSecurityCritical": false,
          "isAssigned": false,
          "isEffective": false
        },
        {
          "name": "PublishContent",
          "description": "Publish or unpublish content for others",
          "category": null,
          "featureId": "OrchardCore.Contents",
          "featureName": "Contents",
          "isSecurityCritical": false,
          "isAssigned": true,
          "isEffective": true
        }
      ]
    }
  ]
}
```

Role descriptions, permissions, and feature associations shown are representative and tenant-dependent. `totalCount` is the number of matching roles before paging.

### Responses

| Status | Meaning |
| --- | --- |
| `200 OK` | The paged role list. |
| `400 Bad Request` | Invalid `skip` or `take`. |
| `401 Unauthorized` | The bearer token is absent or rejected. |
| `403 Forbidden` | A required permission is missing. |

## Get a role

```http
GET /api/roles/{roleId}
```

### Path parameters

| Name | Type | Required | Constraints |
| --- | --- | --- | --- |
| `roleId` | string | Yes | Role manager ID; for the built-in store, use the `roleId` returned by list. URL-encode it as a path segment. |

### Request

```http
GET /api/roles/Editor
Authorization: Bearer eyJ...
Accept: application/json
```

### `200 OK`

Returns one complete [role representation](#role-representation).

```json
{
  "roleId": "Editor",
  "roleName": "Editor",
  "roleDescription": "Edits site content.",
  "isSystemRole": false,
  "isAdminRole": false,
  "permissionCount": 1,
  "permissions": [
    {
      "name": "PublishContent",
      "description": "Publish or unpublish content for others",
      "category": null,
      "featureId": "OrchardCore.Contents",
      "featureName": "Contents",
      "isSecurityCritical": false,
      "isAssigned": true,
      "isEffective": true
    }
  ]
}
```

### Responses

| Status | Meaning |
| --- | --- |
| `200 OK` | The role. |
| `401 Unauthorized` | The bearer token is absent or rejected. |
| `403 Forbidden` | A required permission is missing. |
| `404 Not Found` | The role manager cannot resolve `roleId`. |

## Create a role

```http
POST /api/roles
```

### Headers and JSON body

`Content-Type: application/json` and a JSON body are required.

| Property | Type | Required | Default | Constraints |
| --- | --- | --- | --- | --- |
| `roleName` | string | Yes | `""` | Trimmed before use; must not be empty or contain `/`. An existing role must be equivalent. |
| `roleDescription` | string or `null` | No | `null` | Stored as supplied. |
| `permissionNames` | array of strings | No | `[]` | Exact, case-sensitive installed permission names. Blank entries are ignored. Duplicates are removed case-insensitively. Every nonblank entry must exist in the current permission catalog. |

### Request

```http
POST /api/roles
Authorization: Bearer eyJ...
Content-Type: application/json
Accept: application/json

{
  "roleName": "ContentEditor",
  "roleDescription": "Edits and publishes content.",
  "permissionNames": [
    "EditContent",
    "PublishContent"
  ]
}
```

### `201 Created`

The `Location` header is `/api/roles/{escaped-role-name}`. The same status and existing role are
returned for an equivalent retry. Equivalence requires an ordinal-exact description, including
the distinction between null and empty, and set equality of directly assigned permission claims;
permission order does not matter.

```http
HTTP/1.1 201 Created
Location: /api/roles/ContentEditor
Content-Type: application/json
```

The body is the complete [role representation](#role-representation):

```json
{
  "roleId": "ContentEditor",
  "roleName": "ContentEditor",
  "roleDescription": "Edits and publishes content.",
  "isSystemRole": false,
  "isAdminRole": false,
  "permissionCount": 2,
  "permissions": [
    {
      "name": "EditContent",
      "description": "Edit content for others",
      "category": null,
      "featureId": "OrchardCore.Contents",
      "featureName": "Contents",
      "isSecurityCritical": false,
      "isAssigned": true,
      "isEffective": true
    },
    {
      "name": "PublishContent",
      "description": "Publish or unpublish content for others",
      "category": null,
      "featureId": "OrchardCore.Contents",
      "featureName": "Contents",
      "isSecurityCritical": false,
      "isAssigned": true,
      "isEffective": true
    }
  ]
}
```

The actual `permissions` array includes the tenant's entire installed permission catalog, not only assigned entries.

### Responses

| Status | Meaning |
| --- | --- |
| `201 Created` | The role was created, or an existing role was exactly equivalent. |
| `400 Bad Request` | Missing/malformed JSON, request validation failure, or an Identity role-manager error. |
| `401 Unauthorized` | The bearer token is absent or rejected. |
| `403 Forbidden` | A required permission is missing. |
| `409 Conflict` | The resolved role name exists with a different description or directly assigned permission set. |

## Update a role

```http
PUT /api/roles/{roleId}
```

Role names cannot be changed by this endpoint.

### Parameters and JSON body

| Location | Name | Type | Required | Default | Constraints and behavior |
| --- | --- | --- | --- | --- | --- |
| Path | `roleId` | string | Yes | — | Existing role manager ID. |
| Header | `Content-Type` | string | Yes | — | `application/json`. |
| Body | `roleDescription` | string or `null` | No | `null` | A non-null value replaces the description; omission or `null` leaves it unchanged. An empty string clears it. |
| Body | `permissionNames` | array of strings or `null` | No | `null` | A non-null array replaces assigned permission claims after the same validation as create; omission or `null` leaves them unchanged. For the administrator role, this property is ignored and all permissions remain effective. |

The JSON body itself is required.

Repeating a successful update converges the description and directly assigned permission set to
the same values and returns `200`. Omitted/null properties continue to preserve their current
values.

### Request

```http
PUT /api/roles/ContentEditor
Authorization: Bearer eyJ...
Content-Type: application/json
Accept: application/json

{
  "roleDescription": "Publishes approved content.",
  "permissionNames": [
    "PublishContent"
  ]
}
```

### `200 OK`

Returns the updated complete [role representation](#role-representation).

```json
{
  "roleId": "ContentEditor",
  "roleName": "ContentEditor",
  "roleDescription": "Publishes approved content.",
  "isSystemRole": false,
  "isAdminRole": false,
  "permissionCount": 1,
  "permissions": [
    {
      "name": "PublishContent",
      "description": "Publish or unpublish content for others",
      "category": null,
      "featureId": "OrchardCore.Contents",
      "featureName": "Contents",
      "isSecurityCritical": false,
      "isAssigned": true,
      "isEffective": true
    }
  ]
}
```

### Responses

| Status | Meaning |
| --- | --- |
| `200 OK` | The role was updated or already had the requested values. |
| `400 Bad Request` | Missing/malformed JSON, an unknown permission name, or an Identity role-manager error. |
| `401 Unauthorized` | The bearer token is absent or rejected. |
| `403 Forbidden` | A required permission is missing. |
| `404 Not Found` | The role manager cannot resolve `roleId`. |

## Delete a role

```http
DELETE /api/roles/{roleId}
```

### Path parameters

| Name | Type | Required | Constraints |
| --- | --- | --- | --- |
| `roleId` | string | Yes | Existing role manager ID. A system role cannot be deleted. |

There is no request body.

### Request

```http
DELETE /api/roles/ContentEditor
Authorization: Bearer eyJ...
Accept: application/json
```

### `200 OK`

```json
{
  "roleId": "ContentEditor",
  "action": "delete"
}
```

`action` is always `delete`. If the role is already absent, the convergent retry returns the same
`200 OK` body.

### Responses

| Status | Meaning |
| --- | --- |
| `200 OK` | The non-system role was deleted, or the role was already absent. |
| `400 Bad Request` | The role is a system role, or the Identity role manager rejected deletion. |
| `401 Unauthorized` | The bearer token is absent or rejected. |
| `403 Forbidden` | A required permission is missing. |

## Error and validation responses

Application-level errors use `application/problem+json`. English messages shown here are localizable except where the endpoint supplies invariant text.

Validation errors use the exact model property names as keys:

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "A validation error occurred.",
  "status": 400,
  "errors": {
    "RoleName": [
      "The role name is already in use."
    ],
    "PermissionNames": [
      "Permission 'UnknownPermission' is not installed."
    ]
  }
}
```

Identity manager errors are placed under the empty key:

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "A validation error occurred.",
  "status": 400,
  "errors": {
    "": [
      "Identity role manager error."
    ]
  }
}
```

System-role deletion:

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "Bad request",
  "status": 400,
  "detail": "System roles cannot be deleted."
}
```

Unknown role:

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.5",
  "title": "Not Found",
  "status": 404,
  "detail": "Role not found."
}
```

The update endpoint constructs its not-found response directly and uses different title casing:

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.5",
  "title": "Not found",
  "status": 404,
  "detail": "Role not found."
}
```

If the Identity role manager rejects deletion, the response title is `Could not delete this role.` and `detail` is its error descriptions joined with `, `:

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "Could not delete this role.",
  "status": 400,
  "detail": "Identity role manager error."
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

Invalid paging uses `Bad request` with either `Skip must be zero or greater and take must be greater than zero.` or `Take cannot exceed 200.` Authentication-policy responses are produced by the configured `Api` authentication handler; their body is handler-dependent.

Role creation returns `409 Conflict` when the resolved name belongs to a non-equivalent role.
Other role-manager validation and persistence errors return `400 Bad Request`.

## Endpoint coverage and sources

This page covers all 5 routes mapped by:

- `src/OrchardCore.Modules/OrchardCore.Roles/Endpoints/Management/RoleManagementEndpoints.cs`

Registration, permissions, role services, dynamic permission catalog behavior, and metadata were cross-checked against:

- `src/OrchardCore.Modules/OrchardCore.Roles/Startup.cs`
- `src/OrchardCore.Modules/OrchardCore.Roles/Manifest.cs`
- `src/OrchardCore/OrchardCore.Contents.Core/CommonPermissions.cs`
- `src/OrchardCore/OrchardCore.Roles.Core/RolesPermissions.cs`
- `src/OrchardCore/OrchardCore.Roles.Abstractions/IRoleService.cs`
- `test/OrchardCore.Tests/Modules/OrchardCore.Roles/RoleManagementEndpointsTests.cs`
- `test/OrchardCore.Tests/Apis/RemoteManagement/OwnedModuleEndpointMetadataTests.cs`
