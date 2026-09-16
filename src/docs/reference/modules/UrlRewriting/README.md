# URL Rewriting (`OrchardCore.UrlRewriting`)

The URL Rewriting feature allows you to configure URL rewrites and redirects for incoming HTTP requests, significantly improving your site's SEO and user experience. This feature enables you to control how URLs are presented to both users and search engines.

Once enabled, you can manage your rewrite rules by navigating to **Configuration** >> **URL Rewriting**. The order of these rules is crucial, as they are processed sequentially based on their position. The first listed rule is evaluated first for matches. To facilitate this, the UI provides a drag-and-drop feature for easy sorting of the rules.

## Available Rule Sources

| Rule Type         | Description                                                                                                                                                         | Example                                                    |
|-------------------|---------------------------------------------------------------------------------------------------------------------------------------------------------------------|------------------------------------------------------------|
| **Redirect Rule** | The **Redirect Rule** is utilized to send users from one URL to another, which is particularly beneficial for maintaining SEO integrity when URLs change.           | Permanently redirect users from `/about-us` to `/about`.   |
| **Rewrite Rule**  | The **Rewrite Rule** allows you to modify the incoming request URL without changing the URL displayed in the browser's address bar, aiding in content organization. | Change requests for media files from `/img/` to `/media/`. |

## Creating Additional Rule Sources

To add a new rule source, implement the `IUrlRewriteRuleSource` interface. This implementation will allow you to register a new rule source and provide a mechanism to configure these rules.

After implementing your rule source, register it in the service collection using the following method:

```csharp
services.AddRewriteRuleSource<CustomRuleSource>("SourceKey");
```

If your custom rule requires additional properties or user configuration options, create a display driver by implementing `IDisplayDriver<RewriteRule>`. This driver manages the UI for editing custom rule properties. You can register the driver along with your rule source as shown below:

```csharp
services.AddRewriteRuleSource<CustomRuleSource>(CustomRuleSource.SourceName)
    .AddScoped<IDisplayDriver<RewriteRule>, CustomRuleDisplayDriver>();
```

In this example, `CustomRuleSource` represents your implementation of `IUrlRewriteRuleSource`, and `CustomRuleDisplayDriver` provides the user interface for configuring the rule within the admin interface.

## Recipes

The recipe will be accessible only if the OrchardCore.Recipes.Core feature is enabled.

### Recipe for Creating and Updating Rules

The `UrlRewriting` step allows you to create or update URL rewrite rules easily. The example below illustrates how to create a rule that permanently redirects users from `/about-us` to `/about`, along with another rule that serves all media files using the `/img/` prefix instead of `/media/`.

```json
{
  "steps": [
    {
      "name": "UrlRewriting",
      "Rules": [
        {
          "Source": "Redirect",
          "Name": "Redirect about-us to about",
          "Pattern": "^/about-us$",
          "SubstitutionPattern": "/about",
          "IsCaseInsensitive": true,
          "QueryStringPolicy": "Append",
          "RedirectType": "MovedPermanently"
        },
        {
          "Source": "Rewrite",
          "Name": "Serve media URLs from img",
          "Pattern": "^/img/(.*)$",
          "SubstitutionPattern": "/media/$1",
          "IsCaseInsensitive": true,
          "QueryStringPolicy": "Drop",
          "SkipFurtherRules": true
        }
      ]
    }
  ]
}
```

## Explanation of the Rule Properties

### Redirect Rule Properties

- **Id**: A unique identifier for the redirect rule. If the specified ID matches an existing rule, that rule will be updated with the provided properties. To create a new rule, leave the ID property empty or specify a unique value that does not match any existing rule.
- **Name**: A descriptive name for the rule (e.g., "Redirect about-us to about").
- **Pattern**: The URL pattern to match (e.g., `^/about-us$` for an exact match).
- **SubstitutionPattern**: The target URL to which the redirect will occur (e.g., `/about`).
- **IsCaseInsensitive**: When set to `true`, the pattern matching will be case-insensitive.
- **QueryStringPolicy**: Determines how query strings are handled during the redirect:
  - **Append**: Appends the original query string to the new URL.
  - **Drop**: Ignores the query string during the redirect.
- **RedirectType**: Specifies the HTTP status code for the redirect. The following values are supported:
  - **Found**: (HTTP 302) Indicates a temporary redirect.
  - **MovedPermanently**: (HTTP 301) Indicates a permanent redirect, instructing clients to update their bookmarks or links to the new URL.
  - **TemporaryRedirect**: (HTTP 307) Similar to 302 but ensures that the request method remains unchanged (e.g., a POST request remains a POST).
  - **PermanentRedirect**: (HTTP 308) Indicates that the resource has been permanently moved to a new URL.

### Rewrite Rule Properties

- **Id**: A unique identifier for the rewrite rule. If the specified ID matches an existing rule, that rule will be updated. To create a new rule, leave the ID empty.
- **Name**: A descriptive name for the rule (e.g., "Serve media URLs from img").
- **Pattern**: The URL pattern to match (e.g., `^/img/(.*)$` matches any URL starting with `/img/`).
- **SubstitutionPattern**: The target URL for the rewrite (e.g., `/media/$1`, where `$1` captures the matched portion of the original URL).
- **IsCaseInsensitive**: When set to `true`, the pattern matching will be case-insensitive.
- **QueryStringPolicy**: Determines how query strings are handled during the rewrite:
  - **Append**: Appends the original query string to the new URL.
  - **Drop**: Ignores the query string during the rewrite.
- **SkipFurtherRules**: When set to `true`, any subsequent rules will not be processed if this rule matches.

## Validation and updates

The admin editor and recipe importer use the rewrite manager's validation handlers.
Validation checks required names, match patterns, query policies and redirect statuses,
then asks the registered source to construct its runtime rule before saving. Rewrite
and redirect arguments cannot contain literal whitespace or control characters because
they are emitted as individual Apache rewrite arguments. Escape match characters or
URL-encode substitution characters as appropriate. A substitution is a replacement
expression, not a regular expression.

Edits use independent metadata copies so invalid changes do not mutate stored rules.
Saving an unchanged rule or deleting a missing rule does not reload the tenant.
Changed saves, deletes and ordering changes request a reload. The manager uses one-based ordering positions, matching the admin sortable list
which includes a header row. Stored rule order values are zero-based.

## Remote management

When this feature is enabled, its management endpoints require both
`AccessRemoteManagement` and `ManageUrlRewritingRules` through the API authentication
scheme. Pomi and MCP reuse these endpoints and the existing rewrite manager.
The module advertises the `url-rewriting` capability.

| Method and route | Pomi command | Behavior |
| --- | --- | --- |
| `GET /api/url-rewriting/rules` | `url-rewriting rules list` | Runtime-ordered page; `skip`, `take` (1–200) and optional name `search`. |
| `GET /api/url-rewriting/rules/{id}` | `url-rewriting rules show <id>` | Stored identity, order and allowlisted definition. |
| `GET /api/url-rewriting/sources` | `url-rewriting rules sources` | Registered sources and editing support. |
| `POST /api/url-rewriting/rules/validate` | `url-rewriting rules validate` | Validation and source parsing without saving or matching a request. |
| `POST /api/url-rewriting/rules` | `url-rewriting rules create` | Create a complete definition. |
| `PUT /api/url-rewriting/rules/{id}` | `url-rewriting rules update <id>` | Replace a complete definition, preserving source, identity and order. |
| `DELETE /api/url-rewriting/rules/{id}` | `url-rewriting rules delete <id> --force` | Delete; a missing rule succeeds without mutation. |
| `PUT /api/url-rewriting/rules/{id}/position` | `url-rewriting rules move <id>` | Move using `{"position": 0}` for the first rule. |

```bash
pomi url-rewriting rules create --stdin <<'JSON'
{
  "id": "redirect-about",
  "name": "Redirect old about URL",
  "source": "Redirect",
  "pattern": "^/about-us$",
  "substitutionPattern": "/about",
  "redirectType": "MovedPermanently",
  "queryStringPolicy": "Append"
}
JSON
pomi url-rewriting rules show redirect-about
```

The optional `id` accepts 1–128 ASCII letters, digits, underscores or hyphens.
Without it, creation generates a new identifier each time. Supplying the same ID
and normalized definition again returns the stored rule (HTTP 200); a different
definition with that ID returns HTTP 409. Names are display labels and need not be
unique. A newly created rule returns HTTP 201 with its location.

Updates require the complete definition. Omitted options reset to their defaults:
case-sensitive matching, `Append`, `Found` for redirects and `skipFurtherRules: false`
for rewrites. `redirectType` is only valid for `Redirect`; `skipFurtherRules` is only
valid for `Rewrite`. Enum values use names, not numbers. Unknown properties are
rejected. A supplied body ID must match the route ID. Rule source changes are not
supported; create another rule when changing source.

Readback includes a `definition` object suitable for a subsequent update. The API
supports the built-in `Rewrite` and `Redirect` definitions. Custom sources appear
in discovery and stored-rule listings with `isWritable: false` and an opaque/null
definition. Their arbitrary metadata is not exposed or edited; authorized callers
can still reorder or delete these stored rules.

This module uses its source's regex and rewrite options; it does not store common
Rules-module condition trees. Validation constructs the registered runtime rule
but does not evaluate it against a request. Test the resulting URL, status code,
query policy and ordering after applying a change. Matching excludes the configured
admin URL prefix; management API URLs otherwise participate in normal rewriting.
Changed paths are rerouted through the tenant endpoints before the remaining middleware,
so the target endpoint's authorization still applies. Disabling the feature removes its
management operations and runtime rewriting.
