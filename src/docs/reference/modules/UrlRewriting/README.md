# URL Rewriting (`OrchardCore.UrlRewriting`)

The URL Rewriting feature allows you to configure URL rewrites and redirects for incoming HTTP requests, significantly improving your site's SEO and user experience. This feature enables you to control how URLs are presented to both users and search engines.

Once enabled, you can manage your rewrite rules by navigating to **Configuration** >> **URL Rewriting**. The order of these rules is crucial, as they are processed sequentially based on their position. The first listed rule is evaluated first for matches. To facilitate this, the UI provides a drag-and-drop feature for easy sorting of the rules.

## Available Rule Sources

| Rule Type     | Description     | Example     |
|---------------|---------------|---------------|
| **Redirect Rule** | The **Redirect Rule** is utilized to send users from one URL to another, which is particularly beneficial for maintaining SEO integrity when URLs change. | Permanently redirect users from `/about-us` to `/about`. |
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
