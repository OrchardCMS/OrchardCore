# Recipe management API (`OrchardCore.Recipes`)

The Recipe management API discovers, validates and executes non-setup Orchard Core recipes. It reports recipe metadata, declared variables, and values supplied by registered recipe environment providers.

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
| `POST` | `/api/recipes/{recipeId}:validate` | `ApiValidateRecipe` | Check a recipe without applying anything. | `ManageRecipes` |
| `POST` | `/api/recipes/{recipeId}:execute` | `ApiExecuteRecipe` | Execute a recipe with environment overrides. | `ManageRecipes` |
| `GET` | `/api/recipes/executions/{executionId}` | `ApiGetRecipeExecution` | Get the status of a recipe execution. | `ManageRecipes` |

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

The recipes of a tenant are harvested once, then kept until its shell is released, for instance when a feature is enabled or the tenant is reloaded. A recipe file added or changed on disk is reported after that. Executing a recipe releases the shell of the tenant when it succeeds.

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

## Validate a recipe

```http
POST /api/recipes/{recipeId}:validate
```

The operation checks what can be checked without applying anything, and changes nothing. It has no request body. Validation needs the same permissions as execution and finds the same recipes, so an identity that can't execute a recipe can't learn anything from validating it. With Pomi, run `pomi recipes validate <recipe-id>`. It's also an MCP tool annotated as read-only, `recipes_validate`.

!!! warning
    A valid result doesn't guarantee that execution succeeds. Nothing is applied and scripts aren't evaluated, so a step can still fail on its data, and steps that ran before it stay applied.

`200 OK` returns the result, including for an invalid recipe:

```json
{
  "valid": false,
  "notice": "A valid result doesn't guarantee that execution succeeds: nothing was applied, scripts weren't evaluated, and a step can still fail on its data.",
  "stepCount": 2,
  "errorCount": 1,
  "warningCount": 0,
  "findings": [
    {
      "severity": "error",
      "code": "unknown-step",
      "message": "No enabled feature handles the step 'Contnet'. The execution would skip this step and still succeed.",
      "stepIndex": 1,
      "stepName": "Contnet"
    }
  ]
}
```

| Property | Type | Description |
| --- | --- | --- |
| `valid` | boolean | Whether there is no finding of the `error` severity. |
| `notice` | string | The statement that a valid result is not a guarantee. |
| `stepCount` | integer | The number of steps of the recipe. |
| `errorCount` | integer | The number of `error` findings. |
| `warningCount` | integer | The number of `warning` findings. |
| `findings` | array of objects | The findings, in the order of the steps. |

Each finding has a `severity`, a stable `code`, a `message`, the zero-based `stepIndex` and the `stepName`, which are `null` when they don't apply. The severity is `error` when the recipe can't be applied as written, `warning` for something likely wrong or that can't be verified, and `info` for a limit of the validation on this step.

| Code | Severity | Checked |
| --- | --- | --- |
| `recipe-invalid-json`, `recipe-not-object`, `recipe-unreadable` | `error` | The file is a readable JSON object. A file that isn't valid JSON when the recipes are harvested isn't listed at all, and returns `404 Not Found`. |
| `recipe-no-steps`, `recipe-invalid-variables` | `error` | The recipe has a `steps` array, and `variables` is an object. Without a `steps` array, an execution does nothing and succeeds. |
| `step-not-object`, `step-no-name` | `error` | A step is an object with a `name` string. An execution fails on the first, and skips the second. |
| `unknown-step` | `error` | A handler of an enabled feature processes the step. An execution skips a step without a handler and still succeeds. |
| `step-handler-unverified` | `warning` | A step has no known handler, but an earlier step changes the features or a registered handler doesn't declare its steps, so the step can't be proven wrong. |
| `feature-not-found` | `error` or `warning` | A `Feature` step names existing features and themes. An execution ignores the others. It's an `error` for a feature to enable, and a `warning` for one to disable. |
| `theme-not-found`, `theme-not-enabled` | `error`, `warning` | A `Themes` step selects a theme that exists and is enabled, or that an earlier step enables. An execution selects a missing theme anyway. |
| `recipe-not-found`, `recipes-no-values` | `error` | A `Recipes` step names recipes that exist. |
| `file-not-found`, `file-script-invalid` | `error` | A `[file:text('path')]` or `[file:base64('path')]` script, or the `SourcePath` of a `media` step, resolves to a file of the recipe. A path outside of it isn't found. |
| `media-no-files`, `media-target-invalid`, `media-extension-not-allowed` | `error` | A `media` step has files, whose target paths and extensions are allowed. |
| `media-no-source` | `warning` | A file of a `media` step has no source. An execution skips it. |
| `step-invalid` | `error` | The properties of a step that has a validator have the expected types. |
| `scripts-not-evaluated`, `feature-not-evaluated`, `theme-not-evaluated` | `info`, `warning` | A value that only the execution knows, since it comes from a script. |
| `commands-not-validated`, `inner-recipes-not-validated`, `media-url-not-fetched` | `info` | What a `Command` or `Recipes` step does, and the URL of a `media` file. |

Validation can't tell:

- whether a step succeeds on its data, for instance a content item that a handler refuses, except for the steps above;
- what a script returns, as running it could have effects;
- which steps a feature provides before it's enabled: a step that needs a feature an earlier step enables gets a `warning`, and a step that needs a feature an earlier step disables isn't reported;
- what the steps of an inner recipe do, as only the existence of the recipe is checked.

| Status | Meaning |
| --- | --- |
| `200 OK` | The result of the validation, whether the recipe is valid or not. |
| `401 Unauthorized` | Missing or invalid bearer token. |
| `403 Forbidden` | A required permission is missing. |
| `404 Not Found` | The opaque recipe ID is not present in the current harvested set. |

## Execute a recipe

```http
POST /api/recipes/{recipeId}:execute
```

The operation obtains the current recipe environment, adds the supplied `parameters`, executes the recipe, and releases the tenant shell context after successful completion.

The response waits for the end of the execution, which continues if the client disconnects, as stopping it would leave the recipe partially applied. To get the outcome of an execution whose connection was lost, send a `requestId`, then [get the status](#get-the-status-of-a-recipe-execution) of the execution with this identifier.

### Path and header parameters

| Location | Name | Type | Required | Constraints |
| --- | --- | --- | --- | --- |
| Path | `recipeId` | string | Yes | Opaque, case-sensitive ID returned by list or get. |
| Header | `Content-Type` | string | Yes | `application/json`. |

### JSON body

The JSON request body is required. Its defined properties are:

| Property | Type | Required | Default | Constraints |
| --- | --- | --- | --- | --- |
| `requestId` | string | No | A generated identifier | From 1 through 128 letters, digits, hyphens or underscores, generated by the caller. It becomes the execution ID. See [Retries](#retries). |
| `parameters` | object whose values are any JSON value | No | `{}` | Each property adds an environment key of the same name; keys the site provides, such as `AdminUsername`, return `400 Bad Request`. JSON `null` becomes a null environment value; objects, arrays, strings, numbers, and booleans retain their JSON data shape when deserialized. |

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

`executionId` is the `requestId`, or when there is none, 32 generated lowercase hexadecimal characters. Recipe name fields come from the selected recipe.

### Retries

Without a `requestId`, every request executes the recipe again. With one, repeating a request doesn't execute the recipe again: it reports the outcome of the first request, as `200 OK` when it succeeded and `400 Bad Request` when it failed, and `409 Conflict` when it has no outcome yet. The `parameters` of a repeated request are ignored, and using the same `requestId` for another recipe returns `409 Conflict`. A `requestId` can be used again once its execution is no longer kept, see [the status of an execution](#get-the-status-of-a-recipe-execution).

### Responses

| Status | Meaning |
| --- | --- |
| `200 OK` | Recipe execution completed and the tenant shell context was released. |
| `400 Bad Request` | Malformed/missing JSON, a recipe step failure, or another non-fatal execution exception. |
| `401 Unauthorized` | The bearer token is absent or rejected. |
| `403 Forbidden` | A required permission is missing. |
| `404 Not Found` | The opaque recipe ID is not present in the current harvested set. |
| `409 Conflict` | The `requestId` belongs to an execution that has no outcome yet, or to another recipe. |

## Get the status of a recipe execution

```http
GET /api/recipes/executions/{executionId}
```

Use `pomi recipes executions show <executionId>` from the command line.

The most recent 100 executions of the tenant are kept in its database, whether they come from a `requestId` or not. The ones still running are the last to be removed.

### `200 OK`

```json
{
  "executionId": "deploy-2026-11-02",
  "recipeId": "TW9kdWxlcy9PcmNoYXJkQ29yZS5SZW1vdGVNYW5hZ2VtZW50L1JlY2lwZXN8cmVtb3RlLW1hbmFnZW1lbnQucmVjaXBlLmpzb24",
  "recipeName": "RemoteManagement",
  "state": "failed",
  "errors": [
    "The first step error."
  ],
  "startedUtc": "2026-11-02T08:15:00Z",
  "completedUtc": "2026-11-02T08:15:04Z"
}
```

| Property | Type | Description |
| --- | --- | --- |
| `executionId` | string | The execution identifier. |
| `recipeId` | string | The ID of the executed recipe. |
| `recipeName` | string or `null` | The name of the executed recipe. |
| `state` | string | `running`, `succeeded`, `failed`, or `uncertain` when no outcome was reported after an hour. |
| `errors` | array of strings | The errors reported when the execution failed. |
| `startedUtc` | string | When the execution started, in UTC. |
| `completedUtc` | string or `null` | When the execution ended, in UTC. |

An `uncertain` execution was probably interrupted, for instance by a restart of the application, but it may still be running. Its steps may be partially applied: review the site before executing the recipe again with another `requestId`.

### Responses

| Status | Meaning |
| --- | --- |
| `200 OK` | The state of the execution. |
| `401 Unauthorized` | The bearer token is absent or rejected. |
| `403 Forbidden` | A required permission is missing. |
| `404 Not Found` | The execution is unknown, or no longer kept. |

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
  "detail": "The first step error. The second step error.",
  "executionId": "1a2b3c4d5e6f47a8b9c0d1e2f3a4b5c6"
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

This page covers all 4 routes mapped by:

- `src/OrchardCore.Modules/OrchardCore.Recipes/Endpoints/Management/RecipeManagementEndpoints.cs`

Registration, permissions, recipe contracts, behavior, and metadata were cross-checked against:

- `src/OrchardCore.Modules/OrchardCore.Recipes/Startup.cs`
- `src/OrchardCore.Modules/OrchardCore.Recipes/Services/RecipeExecutionStore.cs`
- `src/OrchardCore.Modules/OrchardCore.Recipes/Manifest.cs`
- `src/OrchardCore.Modules/OrchardCore.RemoteManagement/Recipes/remote-management.recipe.json`
- `src/OrchardCore/OrchardCore.Recipes.Core/RecipePermissions.cs`
- `src/OrchardCore/OrchardCore.Recipes.Abstractions/Models/RecipeDescriptor.cs`
- `src/OrchardCore/OrchardCore.Recipes.Abstractions/Services/IRecipeEnvironmentProvider.cs`
- `test/OrchardCore.Tests/Modules/OrchardCore.Recipes/RecipeManagementEndpointsTests.cs`
- `test/OrchardCore.Tests/Apis/RemoteManagement/OwnedModuleEndpointMetadataTests.cs`
