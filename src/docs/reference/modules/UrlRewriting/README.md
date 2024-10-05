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
