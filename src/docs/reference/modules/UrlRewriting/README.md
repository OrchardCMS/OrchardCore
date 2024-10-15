# URL Rewriting (`OrchardCore.UrlRewriting`)

The URL Rewriting module enables you to configure URL rewrites and redirects for incoming HTTP requests, significantly enhancing your site's SEO and user experience. This allows you to control how URLs are handled and presented to users and search engines.

After enabling this feature, you can manage your rewrite rules by navigating to **Configuration** >> **URL Rewriting**.

## Available Rule Sources

### Redirect Rule

The **Redirect Rule** is utilized to send users from one URL to another. This is particularly beneficial for maintaining SEO integrity when URLs change. For example, a redirect rule can permanently redirect users from `/about-us` to `/about`, ensuring both users and search engines are directed to the correct page.

### Rewrite Rule

The **Rewrite Rule** allows you to modify the incoming request URL without changing the URL displayed in the browser's address bar. This can be advantageous for organizing content and serving files more effectively. For instance, a rewrite rule can change requests for media files from the `/img/` prefix to `/media/`, thus creating a consistent structure for accessing media files.

## Adding Rule Sources

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
          "Order": 1,
          "Pattern": "^/about-us$",
          "SubstitutionPattern": "/about",
          "IsCaseInsensitive": true,
          "AppendQueryString": false,
          "RedirectType": "MovedPermanently"
        },
        {
          "Source": "Rewrite",
          "Name": "Serve media URLs from img",
          "Order": 2,
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

- **Id**: A unique identifier for the redirect rule. If the specified Id matches an existing rule, that rule will be updated with the provided properties. To create a new rule, leave the Id property empty or specify a unique value that does not match any existing rule.
- **Name**: A descriptive name for the rule (e.g., `"Redirect about-us to about"`).
- **Order**: A execution sequence number for this rule.
- **Pattern**: The URL pattern to match (e.g., `^/about-us$` for an exact match).
- **SubstitutionPattern**: The target URL to redirect to (e.g., `/about`).
- **IsCaseInsensitive**: When set to `true`, matching is case-insensitive.
- **AppendQueryString**: When `false`, the original query string will not be included in the redirect.
- **RedirectType**: Specifies the HTTP status code for the redirect. `MovedPermanently` (HTTP 301) indicates a permanent redirect.

### Rewrite Rule Properties

- **Id**: A unique identifier for the rewrite rule. Similar to redirect rules, if the specified Id matches an existing rule, that rule will be updated. Leave the Id empty to create a new rule.
- **Name**: A descriptive name for the rule (e.g., `"Serve media URLs from img"`).
- **Order**: A execution sequence number for this rule.
- **Pattern**: The URL pattern to match (e.g., `^/img/(.*)$` to match any URL starting with `/img/`).
- **SubstitutionPattern**: The target URL for the rewrite (e.g., `/media/$1`, where `$1` captures the matched portion of the original URL).
- **IsCaseInsensitive**: When set to `true`, matching is case-insensitive.
- **IgnoreQueryString**: When set to `false`, the query string from the original request will be considered during the rewrite.
- **SkipFurtherRules**: When set to `true`, subsequent rules will not be processed if this rule matches.
