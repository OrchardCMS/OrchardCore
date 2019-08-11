# Resources (`OrchardCore.Resources`)

## Purpose

The `Resources` module provides commonly used resources like JavaScript libraries and CSS files. It also enables the Resource Manager
so any module can describe what resources are necessary on any page or component. When the full page is rendered all the required
resources are computed and custom `<script>` and `<link>` tags are rendered accordingly. You can also register custom `<meta>` tags.


## Resource Settings

Resource Settings are configured through the site admin.

#### UseCdn

Enabling UseCdn will configure the `IResourceManager` to provide any scripts or styles, such as jQuery, from the configured CDN.

#### ResourceDebugMode

When enabled will serve scripts or styles, that have a CDN configured, or a debug-src, from the local server in non minified format.
This will also disabled the CdnBaseUrl prepending.

#### CdnBaseUrl

When supplied this will prepend local resources served via the `IResourceManager` or Tag Helpers with the absolute url provided. This will
be disabled in `ResourceDebugMode` 

## Named Resources

Named resources are well-known scripts and stylesheets that are described in a module. They have a name, a type (script, stylesheet) 
and optionally a version. The `OrchardCore.Resources` modules provides some commonly used ones:

| Name | Type | Versions | Dependencies |
| ---- | ---- | -------- | ------------ |
| jQuery | Script | 3.3.1 | - |
| jQuery.slim | Script | 3.3.1 | - |
| jQuery-ui | Script | 1.12.1 | jQuery |
| jQuery-ui-i18n | Script | 1.7.2 | jQuery-ui |
| popper | Script | 1.15.0 | - |
| bootstrap | Script | 3.4.0, 4.3.1 | jQuery, Popper |
| bootstrap | Style | 3.4.0, 4.3.1 | - |
| codemirror | Script | 5.4.2 | - |
| codemirror | Style | 5.4.2 | - |
| font-awesome | Style | 4.7.0, 5.7.2 | - |
| font-awesome | Script | 5.7.2 | - |
| font-awesome-v4-shims | Script | 5.7.2 | - |

## Usage

There are two ways to invoke a resource: either by using the `IResourceManager` API or a Tag Helper.
The API is necessary if you need to inject a resource from code. However it is recommended to use a Tag Helper when inside a view.

### Using the API

From your module, add a reference to the `OrchardCore.Resources.Abstractions` project.
From the class you want to use the API in, inject the `OrchardCore.ResourceManagement.IResourceManager` interface.

#### Register a named resource

```csharp
var settings = resourceManager.RegisterResource("script", "bootstrap")
```

The result of this call is an object of type `RequireSettings` that is used to pass more parameters to the required resource.

##### Place the resource at the beginning of the HTML document

```csharp
settings.AtHead();
```

##### Place the resource at the end of the HTML document

```csharp
settings.AtFoot();
```

##### Set the version to use

```csharp
settings.UseVersion("3.3");
```

This will use the latest available version between `3.3` and `3.4`. If the version is not available an exception is thrown.

##### Append a version

```csharp
settings.UseAppendVersion(true);
```

This will append a version string that is calculated at runtime as an SHA256 hash of the file, the calculation cached, and 
appended to the url as part of the query string, e.g. `my-script.js?v=eER9OO6zWGKaIq1RlNjImsrWN9y2oTgQKg2TrJnDUWk` 

#### Register custom script

At the beginning of the HTML document:

```csharp
resourceManager.RegisterHeadScript(new HtmlString("<script>alert('Hello')</script>"));
```

At the end of the HTML document:

```csharp
resourceManager.RegisterFootScript(new HtmlString("<script>alert('Hello')</script>"));
```

### Add custom meta tag

```csharp
resourceManager.RegisterMeta(new MetaEntry { Content = "Orchard", Name = "generator" });
```

You can also add more content to an existing tag like this:

```csharp
resourceManager.AppendMeta(new MetaEntry { Name = "keywords", Content = "orchard" }, ",");
```

### Using the Tag Helpers

From your module, in the `_ViewImports.cshtml` or your view, add `@addTagHelper *, OrchardCore.ResourceManagement`.

#### Register a named script

This example registers the script named `bootstrap` and all its dependencies (jquery).

```liquid
{% script name:"bootstrap" %}
```

```razor
<script asp-name="bootstrap"></script>
```

And for a stylesheet:

```razor
<style asp-name="bootstrap"></style>
```

##### Force the CDN

You can force a resource to be used from its CDN. By default the behavior is defined by configuration.

```liquid
{% script name:"bootstrap", use_cdn:"true" %}
```

```razor
<script asp-name="bootstrap" use-cdn="true"></script>
```

##### Use specific version

This example will use the latest available version with a Major version of `3`, like `3.4.0`. If the version is not specified
the latest one is always used.

```liquid
{% script name:"bootstrap", version:"4" %}
```

```razor
<script asp-name="bootstrap" version="3"></script>
```

##### Append a Version Hash

You can append a version hash that will be calculated, and calculation cached, and appended in the format ?v=eER9OO6zWGKaIq1RlNjImsrWN9y2oTgQKg2TrJnDUWk

```liquid
{% script name:"bootstrap", append_version:"true" %}
```

```razor
<script asp-name="bootstrap" asp-append-version="true"></script>
```

##### Specify location

By default all scripts are rendered in the footer. You can override it like this:

```liquid
{% script name:"bootstrap", at:"Foot" %}
```

```razor
<script asp-name="bootstrap" at="Foot"></script>
```

Styles, however, are always injected in the header section of the HTML document.

#### Inline definition

You can declare a new resource directly from a view, and it will be injected only once even if the view is called multiple time.

```liquid
{% script src:"/TheTheme/js/foo.min.js", debug_src:"/TheTheme/js/foo.js" %}
```

```razor
<script asp-name="foo" asp-src="~/TheTheme/js/foo.min.js?v=1.0" debug-src="~/TheTheme/js/foo.js?v=1.0" depends-on="baz:1.0" version="1.0"></script>
```

In this example we also define a dependency on the script named `baz` with the version `1.0`. If the version was not set
the one with the highest number will be used.

You can also do the same for a stylesheet:

```liquid
{% style src:"/TheTheme/css/bar.min.css", debug_src:"/TheTheme/css/bar.css" %}
```

```razor
<style asp-src="~/TheTheme/css/bar.min.css" debug-src="~/TheTheme/css/bar.css"></style>
```

#### Custom scripts

The following example demonstrates how to inject a custom script in the footer section.

```liquid
{% scriptblock at: "Foot" %}
    document.write('<!-- some script -->');
{% endscriptblock %}
```

```razor
<script at="Foot">
    document.write('<!-- some script -->');
</script>
```

#### Meta tags

```liquid
{% meta name:"description", content:"This is a website" %}
```

```razor
<meta asp-name="description" content="This is a website" />
```

These properties are available:

| Name | Description |
| ---- | ---- | 
| `name` (`asp-name` in Razor)| The `name` attribute of the tag |
| `content` | The `content` attribute of the tag |
| `httpequiv` | The `http-equiv` attribute of the tag |
| `charset` | The `charset` attribute of the tag |
| `separator` | The separator to use when multiple tags are defined for the same name |
