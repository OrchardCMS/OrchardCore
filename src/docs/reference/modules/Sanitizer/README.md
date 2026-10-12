# HTML Sanitizer

An HTML Sanitizer is available as part of the Orchard Core Infrastructure.

The Sanitizer cleans user input that could lead to XSS attacks.

It is used by default for the following parts and fields:

- HTML Body Part
- HTML Field
- HTML Menu Item Part
- Markdown Body Part
- Markdown Field

!!! note
    To disable sanitization on these fields disable the `Sanitize Html` option in the field or part settings.

!!! info
    The content is sanitized each time it is displayed. As the configuration of the sanitizer doesn't change during the lifetime of a tenant, the sanitized result of an HTML fragment up to 64K characters is cached by the tenant, and reused when the same fragment is sanitized again, which saves parsing it again on each request. So the sanitizer event handlers that are configured with `ConfigureHtmlSanitizer()` must return the same result for the same fragment.

## Razor Helper

`@Orchard.SanitizeHtml((string)Model.ContentItem.HtmlBodyPart.Html);`

## Defaults configuration

The elements sanitized by default are listed on this page: <https://github.com/mganss/HtmlSanitizer#tags-allowed-by-default>

Orchard Core changes these defaults by:

- allowing the attribute `class`
- allowing the `mailto` and `tel` schemes
- removing the tag `form`

## Configuring the Sanitizer

The sanitizer is configurable using `IOptions<HtmlSanitizerOptions>` during service registration with a configuration
extension method `ConfigureHtmlSanitizer`.

You may call this extension method multiple times during the startup pipeline to alter configurations.

```csharp
services
    .AddOrchardCms()
    .ConfigureServices(tenantServices =>
        tenantServices.ConfigureHtmlSanitizer((sanitizer) =>
            {
                sanitizer.AllowedSchemes.Add("ftp");
            }));
```

Refer <https://github.com/mganss/HtmlSanitizer> for options.
