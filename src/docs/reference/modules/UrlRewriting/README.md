# URL Rewriting (`OrchardCore.UrlRewriting`)

The URL Rewriting module allows you to configure URL rewrites and redirects for incoming HTTP requests, enhancing your site's SEO and user experience by controlling how URLs are handled.

## Available Rules

### Redirect Rule

The Redirect rule is used to send users from one URL to another. This is particularly useful for maintaining SEO when URLs change. In this example, the rule permanently redirects users from `/about-us` to `/about`, ensuring that users and search engines are directed to the correct page.

### Rewrite Rule

The Rewrite rule allows you to modify the incoming request URL without changing the URL displayed in the browser. This can be beneficial for organizing and serving content more effectively. In this example, the rule rewrites requests for media files from the `/img/` prefix to `/media/`, enabling a consistent structure for media file access.

## Adding Custom Rule Sources

To create custom rule sources, implement the `IUrlRewriteRuleSource` interface. This allows you to define custom logic for generating rewrite or redirect rules. After implementing your custom rule source, register it in the service collection using the `AddRewriteRuleSource<CustomRuleSource>("SourceKey")` method.

If your custom rule requires additional properties or user configuration, create a display driver by implementing `IDisplayDriver<RewriteRule>`. This driver manages the UI for editing custom rule properties. Register the driver along with your rule source as shown below:

```csharp
services.AddRewriteRuleSource<CustomRuleSource>(CustomRuleSource.SourceName)
    .AddScoped<IDisplayDriver<RewriteRule>, CustomRuleDisplayDriver>();
```

In this example, `CustomRuleSource` is your implementation of `IUrlRewriteRuleSource`, and `CustomRuleDisplayDriver` provides a UI for configuring the rule in the admin interface.

## Recipes

### Recipe for Creating a Rewrite Rule

The `UrlRewriting` step enables you to create or update URL rewrite rules. The example below illustrates how to create a rule that permanently redirects users from `/about-us` to `/about` and another rule that serves all media files using the `/img/` prefix instead of `/media/`.

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
          "AppendQueryString": false,
          "RedirectType": "MovedPermanently"
        },
        {
          "Source": "Rewrite",
          "Name": "Serve media URLs from img",
          "Pattern": "^/img/(.*)$",
          "SubstitutionPattern": "/media/$1",
          "IsCaseInsensitive": true,
          "IgnoreQueryString": false,
          "SkipFurtherRules": true
        }
      ]
    }
  ]
}
```

## Explanation of the Rule Properties

### Redirect Rule Properties

- **Id**: The unique identifier for the redirect rule. If the specified Id matches an existing rule, that rule will be updated with the provided properties. To create a new rule, either leave the Id property empty or specify a unique value that does not match any existing rule.
- **Name**: A descriptive name for the rule (e.g., `"Redirect about-us to about"`).
- **Pattern**: The URL pattern to match (e.g., `^/about-us$` for an exact match).
- **SubstitutionPattern**: The target URL to redirect to (e.g., `/about`).
- **IsCaseInsensitive**: If set to `true`, matching is case-insensitive.
- **AppendQueryString**: If `false`, the original query string will not be included in the redirect.
- **RedirectType**: Specifies the HTTP status code for the redirect. `MovedPermanently` (HTTP 301) indicates a permanent redirect.

### Rewrite Rule Properties

- **Id**: The unique identifier for the rewrite rule. Similar to redirect rules, if the specified Id matches an existing rule, that rule will be updated. Leave the Id empty to create a new rule.
- **Name**: A descriptive name for the rule (e.g., `"Serve media URLs from img"`).
- **Pattern**: The URL pattern to match (e.g., `^/img/(.*)$` to match any URL starting with `/img/`).
- **SubstitutionPattern**: The target URL for the rewrite (e.g., `/media/$1`, where `$1` captures the matched portion of the original URL).
- **IsCaseInsensitive**: If set to `true`, the matching is case-insensitive.
- **IgnoreQueryString**: If set to `false`, the query string from the original request will be considered during the rewrite.
- **SkipFurtherRules**: If set to `true`, subsequent rules will not be processed if this rule matches.
