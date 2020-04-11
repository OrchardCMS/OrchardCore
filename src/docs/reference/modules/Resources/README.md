# Resources (`OrchardCore.Resources`)

## Purpose

The `Resources` module provides commonly used resources like JavaScript libraries and CSS files. It also enables the Resource Manager
so any module can describe what resources are necessary on any page or component. When the full page is rendered all the required
resources are computed and custom `<script>` and `<link>` tags are rendered accordingly. You can also register custom `<meta>` tags.

## Resource Locations

`Resources` will be served via the `StaticFileMiddleware` from either a module or themes `wwwroot` folder.
When defining a resource the naming convention is `~/ThemeName/scripts/path-to-file.js` or `~/Module.Name/styles/path-to-file.css`

The tilde (~) is a convention used to indicate a relative path, for example a tenant base path.
All script or stylesheet resources should be prefixed with the `~` character.

## Resource Settings

Resource Settings are configured through the site admin.

### AppendVersion

Enabling `AppendVersion` or Resources cache busting will automatically append a version hash to all local scripts and style sheets.
This is turned on by default.

### UseCdn

Enabling UseCdn will configure the `IResourceManager` to provide any scripts or styles, such as `jQuery`, from the configured CDN.

### ResourceDebugMode

When enabled will serve scripts or styles, that have a CDN configured, or a debug-src, from the local server in non minified format.  
This will also disabled the CdnBaseUrl prepending.

### CdnBaseUrl

When supplied this will prepend local resources served via the `IResourceManager` or Tag Helpers with the absolute url provided. This will
be disabled in `ResourceDebugMode`

## Named Resources

Named resources are well-known scripts and stylesheets that are described in a module or theme.  
They have a name, a type (script, stylesheet) and optionally a version.  
The `OrchardCore.Resources` module provides some commonly used ones:

| Name                  | Type   | Versions      | Dependencies   |
| --------------------- | ------ | ------------- | -------------- |
| jQuery                | Script | 3.4.1         | -              |
| jQuery.slim           | Script | 3.4.1         | -              |
| jQuery-ui             | Script | 1.12.1        | jQuery         |
| jQuery-ui-i18n        | Script | 1.7.2         | jQuery-ui      |
| popper                | Script | 1.16.0        | -              |
| bootstrap             | Script | 3.4.0, 4.4.1  | jQuery, Popper |
| bootstrap             | Style  | 3.4.0, 4.4.1  | -              |
| codemirror            | Script | 5.52.2        | -              |
| codemirror            | Style  | 5.52.2        | -              |
| font-awesome          | Style  | 4.7.0, 5.13.0 | -              |
| font-awesome          | Script | 5.13.0        | -              |
| font-awesome-v4-shims | Script | 5.13.0        | -              |

### Registering a Resource Manifest

Named resources are registered by implementing the `IResourceManifestProvider` interface.

This example is provided from `TheBlogTheme` to demonstrate.

```csharp
public class ResourceManifest : IResourceManifestProvider
{
    public void BuildManifests(IResourceManifestBuilder builder)
    {
        var manifest = builder.Add();

        manifest
            .DefineScript("TheBlogTheme-vendor-jQuery")
            .SetUrl("~/TheBlogTheme/vendor/jquery/jquery.min.js", "~/TheBlogTheme/vendor/jquery/jquery.js")
            .SetCdn("https://code.jquery.com/jquery-3.4.1.min.js", "https://code.jquery.com/jquery-3.4.1.js")
            .SetCdnIntegrity("sha384-vk5WoKIaW/vJyUAd9n/wmopsmNhiy+L2Z+SBxGYnUkunIxVxAv/UtMOhba/xskxh", "sha384-mlceH9HlqLp7GMKHrj5Ara1+LvdTZVMx4S1U43/NxCvAkzIo8WJ0FE7duLel3wVo")
            .SetVersion("3.4.1");
        }
    }

```

In this example we define a script with the unique name `TheBlogTheme-vendor-jQuery`. 
We use a name that is unique to `TheBlogTheme` to prevent collisions when multiple themes are active. 

We set a url for the minified version, and the unminified version, which will be used in `ResourceDebugMode`.
For the same reason we define two CDN Url's, which will be preferred over the local urls if the `UseCdn` setting in the site admin is set. 
We set the Cdn Integrity Hashes and the version to `3.4.1`

This script will then be available for the tag helper or API to register by name. 

!!! note "Registration"
    Make sure to register this `IResourceManifestProvider` in the `Startup` or your theme or module.
    `serviceCollection.AddScoped<IResourceManifestProvider, ResourceManifest>();`

## Usage

There are two ways to invoke a resource: either by using the `IResourceManager` API or a Tag Helper.  
The API is necessary if you need to inject a resource from code, however it is recommended to use a Tag Helper when inside a view.

### Using the API

From your module, add a reference to the `OrchardCore.Resources.Abstractions` project.  
From the class you want to use the API in, inject the `OrchardCore.ResourceManagement.IResourceManager` interface.

#### Register a named resource

```csharp
var settings = _resourceManager.RegisterResource("script", "bootstrap")
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

This will append a version string that is calculated at runtime as an SHA256 hash of the file, the calculation cached, and appended to the url as part of the query string, e.g. `my-script.js?v=eER9OO6zWGKaIq1RlNjImsrWN9y2oTgQKg2TrJnDUWk`

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

``` liquid tab="Liquid"
{% script name:"bootstrap" %}
```

``` html tab="Razor"
<script asp-name="bootstrap"></script>
```

And for a stylesheet:

``` html tab="Razor"
<style asp-name="bootstrap"></style>
```

##### Force the CDN

You can force a resource to be used from its CDN. By default the behavior is defined by configuration.

``` liquid tab="Liquid"
{% script name:"bootstrap", use_cdn:"true" %}
```

``` html tab="Razor"
<script asp-name="bootstrap" use-cdn="true"></script>
```

##### Use specific version

This example will use the latest available version with a Major version of `3`, like `3.4.0`. If the version is not specified
the latest one is always used.

``` liquid tab="Liquid"
{% script name:"bootstrap", version:"4" %}
```

``` html tab="Razor"
<script asp-name="bootstrap" version="3"></script>
```

##### Append a Version Hash

You can append a version hash that will be calculated, and calculation cached, and appended in the format ?v=eER9OO6zWGKaIq1RlNjImsrWN9y2oTgQKg2TrJnDUWk

``` liquid tab="Liquid"
{% script name:"bootstrap", append_version:"true" %}
```

``` html tab="Razor"
<script asp-name="bootstrap" asp-append-version="true"></script>
```

##### Specify location

By default all scripts are rendered in the footer. You can override it like this:

``` liquid tab="Liquid"
{% script name:"bootstrap", at:"Foot" %}
```

``` html tab="Razor"
<script asp-name="bootstrap" at="Foot"></script>
```

Styles, however, are always injected in the header section of the HTML document.

#### Inline definition

You can declare a new resource directly from a view, and it will be injected only once even if the view is called multiple time.

``` liquid tab="Liquid"
{% script name:"foo", src:"~/TheTheme/js/foo.min.js", debug_src:"~/TheTheme/js/foo.js", depends_on:"jQuery", version:"1.0" %}
{% script name:"bar", src:"~/TheTheme/js/bar.min.js", debug_src:"~/TheTheme/js/bar.js", depends_on:"foo:1.0", version:"1.0" %}
```

``` html tab="Razor"
<script asp-name="foo" asp-src="~/TheTheme/js/foo.min.js?v=1.0" debug-src="~/TheTheme/js/foo.js?v=1.0" depends-on="jQuery" version="1.0"></script>
<script asp-name="bar" asp-src="~/TheTheme/js/bar.min.js?v=1.0" debug-src="~/TheTheme/js/bar.js?v=1.0" depends-on="foo:1.0" version="1.0"></script>
```

We define a script named `foo` with a dependency on `jQuery` with the version `1.0`. 

We then define a script named `bar` which also takes a dependency on version `1.0` of the `foo` script.

If the version was not set the one with the highest number would be used.

When rendering the scripts the resource manager will order the output based on the dependencies, regardless of the order they are written to:

1. `jQuery`
2. `foo`
3. `bar`

You can also do the same for a stylesheet:

``` liquid tab="Liquid"
{% style name:"bar", src:"~/TheTheme/css/bar.min.css", debug_src:"~/TheTheme/css/bar.css", depends_on:"foo" %}
{% style name:"foo", src:"~/TheTheme/css/foo.min.css", debug_src:"~/TheTheme/css/foo.css", depends_on:"bootstrap" %}
```

``` html tab="Razor"
<style asp-name="bar" asp-src="~/TheTheme/css/bar.min.css" debug-src="~/TheTheme/css/bar.css" depends-on="foo"></style>
<style asp-name="foo" asp-src="~/TheTheme/css/foo.min.css" debug-src="~/TheTheme/css/foo.css" depends-on="bootstrap"></style>
```

In this example define a style named `bar` with a dependency on the style named `foo`

We then define the style named `foo`

When rendering the scripts the resource manager will order the output based on the dependencies, regardless of the order they are written to:

1. `bootstrap`
2. `foo`
3. `bar`

!!! note
    You do not have to define a name for your script or style unless you want to reference it as a dependency.

#### Custom scripts

The following example demonstrates how to inject a custom script in the footer section.

``` liquid tab="Liquid"
{% scriptblock at: "Foot" %}
    document.write('<!-- some script -->');
{% endscriptblock %}
```

``` html tab="Razor"
<script at="Foot">
    document.write('<!-- some script -->');
</script>
```

#### Meta tags

``` liquid tab="Liquid"
{% meta name:"description", content:"This is a website" %}
```

``` html tab="Razor"
<meta asp-name="description" content="This is a website" />
```

These properties are available:

| Name                         | Description                                                           |
| ---------------------------- | --------------------------------------------------------------------- |
| `name` (`asp-name` in Razor) | The `name` attribute of the tag                                       |
| `content`                    | The `content` attribute of the tag                                    |
| `httpequiv`                  | The `http-equiv` attribute of the tag                                 |
| `charset`                    | The `charset` attribute of the tag                                    |
| `separator`                  | The separator to use when multiple tags are defined for the same name |

### Rendering

Your `Layout.cshtml` or `Layout.liquid` must make a call to the resource manager to render resources that have been registered.

#### Head Resources

These are generally rendered at the lower portion of the `<head>` section.

``` liquid tab="Liquid"
<head>
    ...
    {% resources type: "Meta" %}
    {% resources type: "HeadLink" %}
    {% resources type: "HeadScript" %}
    {% resources type: "Stylesheet" %}
</head>    
```

``` html tab="Razor"
<head>
    ...
    <resources type="Meta" />
    <resources type="HeadLink" />
    <resources type="HeadScript" />
    <resources type="Stylesheet" />
</head>
```

#### Foot Resources

These should be rendered at the bottom of the `<body>` section.

``` liquid tab="Liquid"
<body>
    ...
    {% resources type: "FootScript" %}
</body>    
```

``` html tab="Razor"
<body>
    ...
    <resources type="FootScript" />
</body>
```

### Logging

If you register a resource by name and it is not found this will be logged as an error in your `App_Data/Logs` folder.