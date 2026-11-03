# URL Rewriting management API

The **URL Rewriting** feature (`OrchardCore.UrlRewriting`) manages the tenant's rewrite and redirect rules. It advertises the `url-rewriting` remote-management capability and contributes the `pomi url-rewriting` commands and MCP tools. Disabling the feature removes these operations and the rewriting itself.

For the runtime behavior, see the [URL Rewriting module](../../modules/UrlRewriting/README.md).

## Authentication and permissions

Every operation requires an `Api` scheme bearer token, **Access remote management API** (`AccessRemoteManagement`) and `ManageUrlRewritingRules`. An absent or invalid token returns `401`, a missing permission returns `403`.

## Base route

`/api/url-rewriting`

## Endpoints

| Method | Endpoint | Operation ID | Pomi command |
| --- | --- | --- | --- |
| `GET` | `/api/url-rewriting/rules` | `ApiListUrlRewriteRules` | `url-rewriting rules list` |
| `GET` | `/api/url-rewriting/rules/{id}` | `ApiGetUrlRewriteRule` | `url-rewriting rules show <id>` |
| `GET` | `/api/url-rewriting/sources` | `ApiListUrlRewriteSources` | `url-rewriting rules sources` |
| `POST` | `/api/url-rewriting/rules:validate` | `ApiValidateUrlRewriteRule` | `url-rewriting rules validate` |
| `POST` | `/api/url-rewriting/rules` | `ApiCreateUrlRewriteRule` | `url-rewriting rules create` |
| `PUT` | `/api/url-rewriting/rules/{id}` | `ApiUpdateUrlRewriteRule` | `url-rewriting rules update <id>` |
| `DELETE` | `/api/url-rewriting/rules/{id}` | `ApiDeleteUrlRewriteRule` | `url-rewriting rules delete <id> --force` |
| `PUT` | `/api/url-rewriting/rules/{id}/position` | `ApiMoveUrlRewriteRule` | `url-rewriting rules move <id>` |

These operations are MCP tools, except delete, which needs `AllowConfirmationRequiredTools`. See [MCP server](../../modules/RemoteManagement/README.md#mcp-server).

Saving a changed rule, deleting a rule or moving a rule reloads the tenant. An unchanged save and the deletion of a missing rule don't.

## Definition

Unknown properties return `400`.

```json
{
  "id": "redirect-about",
  "name": "Redirect old about URL",
  "source": "Redirect",
  "pattern": "^/about-us$",
  "substitutionPattern": "/about",
  "isCaseInsensitive": false,
  "queryStringPolicy": "Append",
  "redirectType": "MovedPermanently"
}
```

| Property | Type | Required | Description |
| --- | --- | --- | --- |
| `id` | string | No | 1 to 128 ASCII letters, digits, underscores or hyphens. It makes creation retry-safe. On update, it must match the route. |
| `name` | string | Yes | A display label, which doesn't need to be unique. |
| `source` | string | Yes | `Rewrite` or `Redirect`. It can't change on update. |
| `pattern` | string | Yes | The regular expression to match. |
| `substitutionPattern` | string | Yes | The replacement, with optional capture references. |
| `isCaseInsensitive` | boolean | No | Whether matching ignores case. Default `false`. |
| `queryStringPolicy` | string | No | `Append`, the default, or `Drop`. |
| `skipFurtherRules` | boolean | No | `Rewrite` only: whether to stop processing the following rules. Default `false`. |
| `redirectType` | string | No | `Redirect` only: `Found`, the default, `MovedPermanently`, `TemporaryRedirect` or `PermanentRedirect`. |

## Rule representation

```json
{
  "id": "redirect-about",
  "name": "Redirect old about URL",
  "source": "Redirect",
  "order": 0,
  "createdUtc": "2026-01-15T10:00:00Z",
  "isWritable": true,
  "definition": {
    "id": "redirect-about",
    "name": "Redirect old about URL",
    "source": "Redirect",
    "pattern": "^/about-us$",
    "substitutionPattern": "/about",
    "isCaseInsensitive": false,
    "queryStringPolicy": "Append",
    "redirectType": "MovedPermanently"
  }
}
```

`definition` can be sent back for a later update. A rule of a source registered by another feature has `isWritable: false` and a null `definition`, as its metadata isn't exposed. It can still be moved and deleted.

## List rules

`GET /api/url-rewriting/rules`

| Location | Name | Type | Required | Description |
| --- | --- | --- | --- | --- |
| Query | `search` | string | No | Case-insensitive search in the name. |
| Query | `skip` | integer | No | Zero-based offset. Default `0`, minimum `0`. |
| Query | `take` | integer | No | Page size. Default `50`, range `1`–`200`. |

Rules are in the order they run. `200` returns `skip`, `take`, `totalCount` and `items`, and other paging values `400`.

## Show a rule

`GET /api/url-rewriting/rules/{id}`

`200` returns the rule, and an absent one `404`.

## List sources

`GET /api/url-rewriting/sources`

```json
[
  {
    "name": "Redirect",
    "displayName": "Redirect",
    "description": "URL Redirect Rule",
    "isWritable": true
  }
]
```

`isWritable` tells whether this API can edit the rules of the source.

## Validate a definition

`POST /api/url-rewriting/rules:validate`

Checks a complete definition, including that its source can parse it, without saving it or matching a request. Valid and invalid definitions both return `200`, and repeating the request has no effect:

```json
{ "isValid": false, "errors": { "queryStringPolicy": ["Use Append or Drop."] } }
```

## Create a rule

`POST /api/url-rewriting/rules`

| Status | Meaning |
| --- | --- |
| `201` | Created, with a `Location` header. |
| `200` | An equivalent definition already exists under this `id`, and is returned unchanged. |
| `400` | An invalid definition, with a validation problem keyed by property. |
| `409` | The `id` belongs to a different definition. |

Without an `id`, each successful request creates another rule with a generated identifier. Supply one when the request can be retried.

```bash
pomi url-rewriting rules create --stdin <<'JSON'
{
  "id": "redirect-about",
  "name": "Redirect old about URL",
  "source": "Redirect",
  "pattern": "^/about-us$",
  "substitutionPattern": "/about",
  "redirectType": "MovedPermanently"
}
JSON
```

## Update a rule

`PUT /api/url-rewriting/rules/{id}`

Replaces the complete definition, and keeps the source, the identity and the order. An omitted option gets its default. An invalid definition returns a `400` validation problem, and an absent rule `404`. To change the source, create another rule.

## Delete a rule

`DELETE /api/url-rewriting/rules/{id}`

Returns `204`, also when the rule is absent.

## Move a rule

`PUT /api/url-rewriting/rules/{id}/position`

```json
{ "position": 0 }
```

`position` is a zero-based index in the complete list of rules; a value outside of it returns `400`. `200` returns the rule at its position, and an absent rule `404`. Moving a rule to its current position doesn't write anything.

## Errors

Failures use Problem Details, except invalid definitions, which use a validation problem keyed by property. Validation builds the runtime rule without evaluating it: test the resulting URL, status code and order after applying a change.

## Sources

- [Endpoint mappings and handlers](https://github.com/OrchardCMS/OrchardCore/blob/main/src/OrchardCore.Modules/OrchardCore.UrlRewriting/Endpoints/Management/RewriteManagementEndpoints.cs)
- [Definition mapper](https://github.com/OrchardCMS/OrchardCore/blob/main/src/OrchardCore.Modules/OrchardCore.UrlRewriting/Endpoints/Management/RewriteDefinitionMapper.cs)
- [URL Rewriting module](../../modules/UrlRewriting/README.md)
