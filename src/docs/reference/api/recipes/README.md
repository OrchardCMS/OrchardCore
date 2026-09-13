# Recipe management API (`OrchardCore.Recipes`)

The Recipe management API discovers and executes non-setup Orchard Core recipes. It reports recipe metadata, declared variables, and values supplied by registered recipe environment providers.

The owning feature is `OrchardCore.Recipes` (**Recipes**), which is always enabled. Enable and configure `OrchardCore.RemoteManagement` before calling these endpoints; see [Remote Management authentication](../authentication/README.md).

## Authentication and authorization

Every operation requires:

- an access token accepted by the `Api` authentication scheme, sent as `Authorization: Bearer <token>`;
- an authenticated principal with **Access remote management API** (`AccessRemoteManagement`);
- **Manage Recipes** (`ManageRecipes`), a security-critical permission.

Antiforgery validation is disabled for these bearer-token endpoints. An absent or invalid token produces `401 Unauthorized`. An authenticated principal missing either permission produces `403 Forbidden`.

## Base route

All paths are relative to the tenant URL:

```text
/api/recipes
```

## Endpoints

| Method | Path | OpenAPI operation ID | Description | Permission |
| --- | --- | --- | --- | --- |
| `GET` | `/api/recipes` | `ApiListRecipes` | List and filter executable recipes. | `ManageRecipes` |
| `GET` | `/api/recipes/{recipeId}` | `ApiGetRecipe` | Get one executable recipe. | `ManageRecipes` |
| `POST` | `/api/recipes/{recipeId}:execute` | `ApiExecuteRecipe` | Execute a recipe with environment overrides. | `ManageRecipes` |

## Recipe representation

| Property | Type | Description |
| --- | --- | --- |
| `id` | string | Opaque, case-sensitive Base64url ID derived from the recipe base path and file name. Use this value unchanged in subsequent calls. |
| `name` | string or `null` | Recipe name. |
| `displayName` | string or `null` | Recipe display name. |
| `description` | string or `null` | Recipe description. |
| `author` | string or `null` | Recipe author. |
| `website` | string or `null` | Recipe website. |
| `version` | string or `null` | Recipe version. |
| `feature` | string | Owning feature display name, or `Application` when no feature is resolved. |
| `featureId` | string or `null` | Owning feature ID when resolved. |
| `fileName` | string or `null` | Recipe file name. |
| `basePath` | string or `null` | Harvested recipe base path. |
| `categories` | array of strings | Recipe-defined categories. |
| `tags` | array of strings | Recipe-defined tags. |
| `isSetupRecipe` | boolean | Always `false` for recipes returned by this API. |
| `parameters` | array of objects | Declared recipe variables merged with current environment values, ordered by parameter name case-insensitively. |

Each `parameters` entry has:

| Property | Type | Accepted values or meaning |
| --- | --- | --- |
| `name` | string | Variable/environment key. |
| `source` | string | `recipe`, `environment`, or `recipe+environment`. |
| `defaultValue` | string or `null` | Recipe variable value converted to text. |
| `currentValue` | string or `null` | Environment value converted to text. |

Recipe files, their metadata, and registered `IRecipeEnvironmentProvider` implementations are extensible. Therefore the recipes, parameter names, and environment-derived values are dynamic. A malformed recipe file can still be listed, but recipe variables that cannot be parsed are omitted.

Setup recipes, recipes tagged `hidden` (case-insensitively), and recipes that cannot be associated with an available extension feature are excluded.

## Paging envelope

The list operation returns:

| Property | Type | Description |
| --- | --- | --- |
| `skip` | integer | Applied zero-based offset. |
| `take` | integer | Requested page size. |
| `totalCount` | integer | Number of matches before paging. |
| `items` | array of recipe objects | Current page, containing at most `take` entries. |

## List recipes

```http
GET /api/recipes
```

Recipes are ordered by `displayName`, falling back to `name`, case-insensitively.

### Query parameters

| Name | Type | Required | Default | Constraints and behavior |
| --- | --- | --- | --- | --- |
| `skip` | integer | No | `0` | Zero or greater. |
| `take` | integer | No | `50` | From `1` through `200`. |
| `search` | string | No | — | Case-insensitive substring match against name, display name, or description. |
| `tag` | string | No | — | Case-insensitive exact match against one recipe tag. |
| `featureId` | string | No | — | Case-insensitive exact owning feature ID match. |

### Request

```http
GET /api/recipes?featureId=OrchardCore.Users&skip=0&take=50
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
      "id": "TW9kdWxlcy9PcmNoYXJkQ29yZS5SZW1vdGVNYW5hZ2VtZW50L1JlY2lwZXN8cmVtb3RlLW1hbmFnZW1lbnQucmVjaXBlLmpzb24",
      "name": "RemoteManagement",
      "displayName": "Orchard Core Remote Management",
      "description": "Configures shared OpenID Connect authentication, token validation, and the remote management scope without registering client applications.",
      "author": "The Orchard Core Team",
      "website": "https://orchardcore.net",
      "version": "1.0.0",
      "feature": "Remote Management",
      "featureId": "OrchardCore.RemoteManagement",
      "fileName": "remote-management.recipe.json",
      "basePath": "Modules/OrchardCore.RemoteManagement/Recipes",
      "categories": [
        "api",
        "security"
      ],
      "tags": [
        "cli",
        "openid",
        "openapi",
        "remote-management"
      ],
      "isSetupRecipe": false,
      "parameters": [
        {
          "name": "Tenant",
          "source": "environment",
          "defaultValue": null,
          "currentValue": "Default"
        }
      ]
    }
  ]
}
```

The metadata shown is representative; values come from installed recipe files and environment providers. `totalCount` is the number of matches before paging.

### Responses

| Status | Meaning |
| --- | --- |
| `200 OK` | The paged recipe list. |
| `400 Bad Request` | Invalid `skip` or `take`. |
| `401 Unauthorized` | The bearer token is absent or rejected. |
| `403 Forbidden` | A required permission is missing. |

## Get a recipe

```http
GET /api/recipes/{recipeId}
```

### Path parameters

| Name | Type | Required | Constraints |
| --- | --- | --- | --- |
| `recipeId` | string | Yes | Opaque ID returned by the list endpoint. Matching is ordinal and case-sensitive. |

### Request

```http
GET /api/recipes/TW9kdWxlcy9PcmNoYXJkQ29yZS5SZW1vdGVNYW5hZ2VtZW50L1JlY2lwZXN8cmVtb3RlLW1hbmFnZW1lbnQucmVjaXBlLmpzb24
Authorization: Bearer eyJ...
Accept: application/json
```

### `200 OK`

```json
{
  "id": "TW9kdWxlcy9PcmNoYXJkQ29yZS5SZW1vdGVNYW5hZ2VtZW50L1JlY2lwZXN8cmVtb3RlLW1hbmFnZW1lbnQucmVjaXBlLmpzb24",
  "name": "RemoteManagement",
  "displayName": "Orchard Core Remote Management",
  "description": "Configures shared OpenID Connect authentication, token validation, and the remote management scope without registering client applications.",
  "author": "The Orchard Core Team",
  "website": "https://orchardcore.net",
  "version": "1.0.0",
  "feature": "Remote Management",
  "featureId": "OrchardCore.RemoteManagement",
  "fileName": "remote-management.recipe.json",
  "basePath": "Modules/OrchardCore.RemoteManagement/Recipes",
  "categories": [
    "api",
    "security"
  ],
  "tags": [
    "cli",
    "openid",
    "openapi",
    "remote-management"
  ],
  "isSetupRecipe": false,
  "parameters": [
    {
      "name": "Tenant",
      "source": "environment",
      "defaultValue": null,
      "currentValue": "Default"
    }
  ]
}
```

### Responses

| Status | Meaning |
| --- | --- |
| `200 OK` | The recipe. |
| `401 Unauthorized` | The bearer token is absent or rejected. |
| `403 Forbidden` | A required permission is missing. |
| `404 Not Found` | The opaque recipe ID is not present in the current harvested set. |

## Execute a recipe

```http
POST /api/recipes/{recipeId}:execute
```

The operation obtains the current recipe environment, overlays the supplied `parameters`, executes the recipe, and releases the tenant shell context after successful completion.

### Path and header parameters

| Location | Name | Type | Required | Constraints |
| --- | --- | --- | --- | --- |
| Path | `recipeId` | string | Yes | Opaque, case-sensitive ID returned by list or get. |
| Header | `Content-Type` | string | Yes | `application/json`. |

### JSON body

The JSON request body is required. Its only defined property is:

| Property | Type | Required | Default | Constraints |
| --- | --- | --- | --- | --- |
| `parameters` | object whose values are any JSON value | No | `{}` | Each property overlays the environment key of the same name. JSON `null` becomes a null environment value; objects, arrays, strings, numbers, and booleans retain their JSON data shape when deserialized. |

Parameter names and accepted values are recipe-specific. Consult the recipe's `parameters` array and the recipe implementation. Values are not validated by this endpoint before execution.

### Request

```http
POST /api/recipes/TW9kdWxlcy9PcmNoYXJkQ29yZS5SZW1vdGVNYW5hZ2VtZW50L1JlY2lwZXN8cmVtb3RlLW1hbmFnZW1lbnQucmVjaXBlLmpzb24:execute
Authorization: Bearer eyJ...
Content-Type: application/json
Accept: application/json

{
  "parameters": {
    "Region": "EU",
    "CreateSampleContent": true,
    "MaximumItems": 10,
    "OptionalValue": null
  }
}
```

To use only the current environment and recipe defaults, send:

```json
{
  "parameters": {}
}
```

### `200 OK`

```json
{
  "executionId": "1a2b3c4d5e6f47a8b9c0d1e2f3a4b5c6",
  "recipeId": "TW9kdWxlcy9PcmNoYXJkQ29yZS5SZW1vdGVNYW5hZ2VtZW50L1JlY2lwZXN8cmVtb3RlLW1hbmFnZW1lbnQucmVjaXBlLmpzb24",
  "recipeName": "RemoteManagement",
  "displayName": "Orchard Core Remote Management"
}
```

`executionId` is dynamically generated as 32 lowercase hexadecimal characters. Recipe name fields
come from the selected recipe. Every successful request executes the recipe again and receives a
new execution ID, so retrying this command is intentionally not idempotent.

### Responses

| Status | Meaning |
| --- | --- |
| `200 OK` | Recipe execution completed and the tenant shell context was released. |
| `400 Bad Request` | Malformed/missing JSON, a recipe step failure, or another non-fatal execution exception. |
| `401 Unauthorized` | The bearer token is absent or rejected. |
| `403 Forbidden` | A required permission is missing. |
| `404 Not Found` | The opaque recipe ID is not present in the current harvested set. |

## Error responses

Application-level failures use `application/problem+json`. English messages shown here are localizable.

Invalid paging:

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "Bad request",
  "status": 400,
  "detail": "Skip must be zero or greater and take must be greater than zero."
}
```

Unknown recipe:

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.5",
  "title": "Not Found",
  "status": 404,
  "detail": "Recipe not found."
}
```

A `RecipeExecutionException` uses the recipe step errors joined with spaces:

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "Recipe execution failed.",
  "status": 400,
  "detail": "The first step error. The second step error."
}
```

For another non-fatal exception, `detail` is `Unexpected error occurred while running the '<display name>' recipe.` Both the detail and display name are dynamic.

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

This page covers all 3 routes mapped by:

- `src/OrchardCore.Modules/OrchardCore.Recipes/Endpoints/Management/RecipeManagementEndpoints.cs`

Registration, permissions, recipe contracts, behavior, and metadata were cross-checked against:

- `src/OrchardCore.Modules/OrchardCore.Recipes/Startup.cs`
- `src/OrchardCore.Modules/OrchardCore.Recipes/Manifest.cs`
- `src/OrchardCore.Modules/OrchardCore.RemoteManagement/Recipes/remote-management.recipe.json`
- `src/OrchardCore/OrchardCore.Recipes.Core/RecipePermissions.cs`
- `src/OrchardCore/OrchardCore.Recipes.Abstractions/Models/RecipeDescriptor.cs`
- `src/OrchardCore/OrchardCore.Recipes.Abstractions/Services/IRecipeEnvironmentProvider.cs`
- `test/OrchardCore.Tests/Modules/OrchardCore.Recipes/RecipeManagementEndpointsTests.cs`
- `test/OrchardCore.Tests/Apis/RemoteManagement/OwnedModuleEndpointMetadataTests.cs`
