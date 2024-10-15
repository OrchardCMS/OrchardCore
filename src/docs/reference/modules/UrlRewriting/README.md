# URL Rewriting (`OrchardCore.UrlRewriting`)

The URL Rewriting module allows you to configure URL rewrites and redirects for incoming HTTP requests, enhancing your site's SEO and user experience by controlling how URLs are handled.

## Adding Custom Rule Sources

To create custom rule sources, you can implement the `IUrlRewriteRuleSource` interface. This lets you define custom logic for generating rewrite or redirect rules. After implementing your custom rule source, register it in the service collection using the `AddRewriteRuleSource<CustomRuleSource>("SourceKey")` method.

If your custom rule requires additional properties or user configuration, you can create a display driver by implementing `IDisplayDriver<RewriteRule>`. This driver can be used to manage the UI for editing custom rule properties. Register the driver along with your rule source like so:

```csharp
services.AddRewriteRuleSource<CustomRuleSource>(CustomRuleSource.SourceName)
    .AddScoped<IDisplayDriver<RewriteRule>, CustomRuleDisplayDriver>();
```

In this example, `CustomRuleSource` is your implementation of `IUrlRewriteRuleSource`, and `CustomRuleDisplayDriver` provides a UI to configure the rule in the admin interface.

## Recipes

### Recipe for Creating a Rewrite Rule

The `UrlRewriting` step allows you to create or update URL rewrite rules. The example below demonstrates how to create a new rule that permanently redirects users from `/about-us` to `/about`.

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
          "IgnoreCase": true,
          "AppendQueryString": false,
          "RedirectType": "MovedPermanently"
        },
        {
          "Source": "Rewrite",
          "Name": "Serve media URLs to from img",
          "Pattern": "^/img/(.*)$",
          "SubstitutionPattern": "/media/$1",
          "IgnoreCase": true,
          "IgnoreQueryString": false,
          "SkipFurtherRules": true
        }
      ]
    }
  ]
}
```

#### Explanation of the Rule Properties

- **Id**: The unique identifier for the rewrite rule. If the specified Id matches an existing rule, that rule will be updated with the provided properties. To create a new rule, either leave the Id property empty or specify a new unique value that does not match any existing rule.
- **Name**: A descriptive name for the rule, e.g., `"Redirect about-us to about"`.
- **Pattern**: The URL pattern to match. Here, `^/about-us$` ensures an exact match.
- **SubstitutionPattern**: The target Pattern to redirect to, `/about`.
- **IgnoreCase**: If set to `true`, the matching is case-insensitive.
- **AppendQueryString**: If `false`, the original query string won't be included in the redirect.
- **RedirectType**: Specifies the HTTP status code for the redirect. `MovedPermanently` (HTTP 301) indicates a permanent redirect.
