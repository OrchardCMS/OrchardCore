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

### `AppendVersion`

Enabling `AppendVersion` or Resources cache busting will automatically append a version hash to all local scripts and style sheets.
This is turned on by default.

### `UseCdn`

Enabling UseCdn will configure the `IResourceManager` to provide any scripts or styles, such as `jQuery`, from the configured CDN.

### `ResourceDebugMode`

When enabled, this will serve scripts or styles that have a CDN configured, or a debug-src, from the local server in non-minified format.
This will also disable the `CdnBaseUrl` prepending.

### `CdnBaseUrl`

When supplied, this will prepend local resources served via the `IResourceManager` or Tag Helpers with the absolute URL provided. This will be disabled in `ResourceDebugMode`.

## Named Resources

Named resources are well-known scripts and stylesheets that are described in a module or theme.  
They have a name, a type (script, stylesheet) and optionally a version.  
The `OrchardCore.Resources` module provides some commonly used ones:

| Name                  | Type   | Versions     | Dependencies  |
|-----------------------|--------|--------------|---------------|
| jQuery                | Script | 3.7.1        | -             |
| jQuery.slim           | Script | 3.7.1        | -             |
| jQuery-ui             | Script | 1.14.2       | jQuery        |
| jQuery-ui-i18n        | Script | 1.14.2       | jQuery-ui     |
| jquery.easing         | Script | 1.4.1        | -             |
| jquery-resizable-dom  | Script | 0.35.0       | -             |
| js-cookie             | Script | 3.0.5        | -             |
| popper                | Script | 1.16.1       | -             |
| popperjs              | Script | 2.11.8       | -             |
| bootstrap             | Script | 4.6.1        | popper        |
| bootstrap             | Script | 5.3.8        | popperjs      |
| bootstrap             | Style  | 4.6.1, 5.3.8 | -             |
| bootstrap-select      | Script | 1.1.2        | bootstrap      |
| bootstrap-select      | Style  | 1.1.2        | -             |
| codemirror            | Script | 5.65.7       | -             |
| codemirror            | Style  | 5.65.7       | -             |
| font-awesome          | Style  | 6.7.2, 7.3.0 | -             |
| font-awesome          | Script | 6.7.2, 7.3.0 | -             |
| font-awesome-v4-shims | Script | 6.7.2, 7.3.0 | -             |
| Sortable              | Script | 1.10.2       | -             |
| list-management      | Script | 1.0.0        | -             |
| trumbowyg             | Style  | 2.28.0       | -             |
| trumbowyg             | Script | 2.28.0       | -             |
| vue-multiselect       | Script | 2.1.6        | -             |
| vuedraggable          | Script | 2.24.3       | Sortable      |
| monaco-loader         | Script | 0.52.2       | -             |
| monaco                | Script | 0.52.2       | monaco-loader |
| nouislider            | Script | 15.6.1       | -             |
| nouislider            | Style  | 15.6.1       | -             |

### Registering a Resource Manifest

Named resources are registered by configuring the `ResourceManagementOptions` options.

This example is provided from `TheBlogTheme` to demonstrate.

```csharp
public class ResourceManagementOptionsConfiguration : IConfigureOptions<ResourceManagementOptions>
{
    private static ResourceManifest _manifest;

    static ResourceManagementOptionsConfiguration()
    {
        _manifest = new ResourceManifest();

        _manifest
            .DefineScript("TheBlogTheme-vendor-jQuery")
            .SetUrl("~/TheBlogTheme/vendor/jquery/jquery.min.js", "~/TheBlogTheme/vendor/jquery/jquery.js")
            .SetCdn("https://code.jquery.com/jquery-3.4.1.min.js", "https://code.jquery.com/jquery-3.4.1.js")
            .SetCdnIntegrity("sha384-vk5WoKIaW/vJyUAd9n/wmopsmNhiy+L2Z+SBxGYnUkunIxVxAv/UtMOhba/xskxh", "sha384-mlceH9HlqLp7GMKHrj5Ara1+LvdTZVMx4S1U43/NxCvAkzIo8WJ0FE7duLel3wVo")
            .SetVersion("3.4.1");
    }

    public void Configure(ResourceManagementOptions options)
    {
        options.ResourceManifests.Add(_manifest);
    }
}

```

In this example we define a script with the unique name `TheBlogTheme-vendor-jQuery`.
We use a name that is unique to `TheBlogTheme` to prevent collisions when multiple themes are active.

We set a url for the minified version, and the unminified version, which will be used in `ResourceDebugMode`.
For the same reason we define two CDN Url's, which will be preferred over the local urls if the `UseCdn` setting in the site admin is set.
We set the Cdn Integrity Hashes and the version to `3.4.1`

This script will then be available for the tag helper or API to register by name.

Additionally, we can use the `SetDependencies` method to ensure the script or style is loaded after their dependency.

```csharp
public class ResourceManagementOptionsConfiguration : IConfigureOptions<ResourceManagementOptions>
{
    private static ResourceManifest _manifest;

    static ResourceManagementOptionsConfiguration()
    {
        _manifest = new ResourceManifest();

        _manifest
            .DefineStyle("ModuleName-Bootstrap-Select")
            .SetUrl("~/ModuleName/bootstrap-select.min.css", "~/ModuleName/bootstrap-select.css")
            .SetDependencies("bootstrap:4");
    }

    public void Configure(ResourceManagementOptions options)
    {
        options.ResourceManifests.Add(_manifest);
    }
}

```

In this example, we define a style that depends on Bootstrap version 4. In this case, the latest available minor version of bootstrap version 4 will be added. Alternatively, you can set a specific version of your choice or the latest version available. [See Inline definition](#inline-definition) for more details about versioning usage.

!!! note "Registration"
    Make sure to register this `IConfigureOptions<ResourceManagementOptions>` in the `Startup` or your theme or module.
    `services.AddResourceConfiguration<ResourceManagementOptionsConfiguration>();`

## List management helper

The `list-management` script is a reusable admin helper for searchable lists, bulk selection, filter auto-submit, grouped results, and grouped selection.

Register it in a Razor view with:

```html
<script asp-name="list-management" at="Foot"></script>
```

Then opt a container into automatic initialization:

```html
<form data-list-management>
    ...
</form>
```

You can also initialize it manually:

```javascript
window.listManagement.initialize(element, {
    clientSideSearch: true
});
```

### Default markup conventions

The script now follows a shared set of HTML conventions so most admin pages can use fewer custom `data-*` attributes:

| Element | Default selector |
|---|---|
| Bulk action container | `#actions` |
| Search box | `#search-box` |
| Select-all checkbox | `#select-all` |
| Unselected summary label | `#items` |
| Selected summary label | `#selected-items` |
| Search empty-state alert | `#list-empty` |
| Search no-results alert | `#list-alert` |
| Search summary text | `#list-summary` |
| Searchable item text | `.list-item-search-text` |
| Action to open when Enter is pressed | `.list-item-action` |
| Search groups | `.list-management-group` |
| Group select-all checkbox | `.list-group-select-all` |
| Group select-all row | `.list-group-select-all-container` |
| Group select-all label | `.list-group-select-all-label` |
| Group select-all text | `.list-group-select-all-text` |
| Create action used when no result matches | `#btnCreate` |

When these elements are present, the script also infers common settings such as `submit.Filter`, `submit.BulkAction`, the bulk-action hidden input name, and the selected item checkbox name.

### Supported settings

All options can be provided either as `data-*` attributes on the `data-list-management` root element or as properties passed to `window.listManagement.initialize()`.

| Attribute / option | Default | Purpose |
|---|---|---|
| `data-actions-selector` / `actionsSelector` | `#actions` | Container shown while a bulk selection is active. |
| `data-bulk-action-input-name` / `bulkActionInputName` | `Options.BulkAction` | Hidden input name that receives the selected bulk action. |
| `data-client-side-search` / `clientSideSearch` | `false` | Enables in-browser filtering instead of relying on form submit. |
| `data-client-filters-selector` / `clientFiltersSelector` | `[data-list-filter]` | Inputs and selects that participate in client-side filtering. |
| `data-empty-alert-selector` / `emptyAlertSelector` | `#list-empty` | Empty-state element shown when the original list has no items. |
| `data-filter-change-selector` / `filterChangeSelector` | `.filter-options select, .filter-options input` | Elements that trigger `submit.Filter` when `filterSubmit` is enabled. |
| `data-filter-links-selector` / `filterLinksSelector` | `[data-list-filter-link]` | Clickable links used as client-side filters. |
| `data-filter-selector` / `filterSelector` | `.filter` | Elements hidden while a bulk selection is active. |
| `data-filter-submit` / `filterSubmit` | `false` | Automatically submits the filter form when matching filter inputs change. |
| `data-group-checkbox-selector` / `groupCheckboxSelector` | Inferred from the selected item checkbox name | Checkboxes affected by each grouped select-all toggle. |
| `data-group-label-selector` / `groupLabelSelector` | `.list-group-select-all-label` | Label element that stores the group summary template. |
| `data-group-select-all-selector` / `groupSelectAllSelector` | `.list-group-select-all` | Group-level select-all checkbox selector. |
| `data-group-text-attribute` / `groupTextAttribute` | `data-select-all-text` | Attribute containing the group summary template text. |
| `data-group-text-count-token` / `groupTextCountToken` | `__COUNT__` | Placeholder replaced with the visible item count in group text. |
| `data-group-text-selector` / `groupTextSelector` | `.list-group-select-all-text` | Element whose text is updated with the visible item count. |
| `data-group-toggle-container-selector` / `groupToggleContainerSelector` | `.list-group-select-all-container` | Container hidden when a group has no visible items. |
| `data-item-input-name` / `itemInputName` | `itemIds` | Checkbox name used for bulk selection. |
| `data-items-selector` / `itemsSelector` | `#items` | Summary element shown when no bulk selection is active. |
| `data-min-selected-items` / `minSelectedItems` | `2` | Minimum selected item count required before bulk actions become active. |
| `data-no-results-action-query-name` / `noResultsActionQueryName` | `suggestion` | Query-string name appended when navigating to the no-results action. |
| `data-no-results-action-selector` / `noResultsActionSelector` | `#btnCreate` | Action invoked when Enter is pressed and no visible result matches. |
| `data-normalize-search` / `normalizeSearch` | `false` | Removes diacritics before comparing search text. |
| `data-search-alert-selector` / `searchAlertSelector` | `#list-alert` | Alert shown when search or filters produce no visible matches. |
| `data-search-box-selector` / `searchBoxSelector` | `#search-box` | Search input selector. |
| `data-search-dom-selector` / `searchDomSelector` | `.list-item-search-text` | Descendant elements whose text is added to the search index. |
| `data-search-first-element-classes` / `searchFirstElementClasses` | empty | Extra classes applied to the first visible result. |
| `data-search-group-selector` / `searchGroupSelector` | `.list-management-group` | Group containers hidden when none of their child results remain visible. |
| `data-search-group-visible-selector` / `searchGroupVisibleSelector` | `searchResultSelector` | Visible child selector used to determine whether a group should remain visible. |
| `data-search-result-selector` / `searchResultSelector` | `[data-filter-value]` | Searchable result elements inside the root container. |
| `data-search-summary-selector` / `searchSummarySelector` | `#list-summary` | Summary element updated with visible and total result counts. |
| `data-search-summary-text-attribute` / `searchSummaryTextAttribute` | `data-summary-text` | Attribute containing the summary template. |
| `data-search-summary-total-attribute` / `searchSummaryTotalAttribute` | `data-total-count` | Attribute containing the total result count. |
| `data-search-text-attribute` / `searchTextAttribute` | `data-filter-value` | Attribute read from each result to build searchable text. Use `textContent` to search rendered text directly. |
| `data-select-all-selector` / `selectAllSelector` | `#select-all` | Main select-all checkbox selector. |
| `data-selected-items-selector` / `selectedItemsSelector` | `#selected-items` | Summary element shown while a bulk selection is active. |
| `data-selected-label` / `selectedLabel` | `selected` | Text appended to the selected item count. |
| `data-selection-enabled` / `selectionEnabled` | `true` when selection controls exist, otherwise `false` | Enables bulk-selection behavior. |
| `data-single-result-action-selector` / `singleResultActionSelector` | `.list-item-action` | Action opened when Enter is pressed on a matching result. |
| `data-single-result-action-mode` / `singleResultActionMode` | `single` | Uses the action only when exactly one result matches; set to `first` to always use the first visible result. |
| `data-submit-bulk-action-name` / `submitBulkActionName` | `submit.BulkAction` | Hidden submit button name used to post the current bulk action. |
| `data-submit-filter-name` / `submitFilterName` | `submit.Filter` | Hidden submit button name used for filter submits. |

### Client-side filters

Client-side filters are opt-in and work together with `clientSideSearch`.

```html
<select data-list-filter data-list-filter-attribute="data-status">
    <option value="" selected>Any status</option>
    <option value="enabled">Enabled only</option>
</select>
```

```html
<a data-list-filter-link
   data-list-filter-attribute="data-category"
   data-list-filter-value="content"
   href="#content">
    Content
</a>
```

Use `data-list-filter-mode="exclude"` to hide matching items instead of keeping them.

### Events

- Dispatch `listmanagement:update` on the root element to re-run the current search and filter state.
- Listen for `listmanagement:updated` to react after the visible result set changes.
  
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
settings.UseVersion("3.4");
```

This will use the latest available version between `3.4` and `3.5`. If the version is not available an exception is thrown.

##### Append a version

```csharp
settings.UseAppendVersion(true);
```

This will append a version string that is calculated at runtime as an SHA256 hash of the file, with the calculation cached, and appended to the URL as part of the query string, e.g. `my-script.js?v=eER9OO6zWGKaIq1RlNjImsrWN9y2oTgQKg2TrJnDUWk`

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

From your module, in the `_ViewImports.cshtml` or your view, add `@addTagHelper *, OrchardCore.ResourceManagement`, and take a direct reference to the `OrchardCore.ResourceManagement` nuget package.

#### Register a named script or stylesheet

This example registers the script named `bootstrap` and all its dependencies (jquery).

=== "Liquid"

    ``` liquid
    {% script name:"bootstrap" %}
    ```

=== "Razor"

    ``` html
    <script asp-name="bootstrap"></script>
    ```

And for a stylesheet:

=== "Liquid"

    ``` liquid
    {% style name:"bootstrap" %}
    ```

=== "Razor"

    ``` html
    <style asp-name="bootstrap"></style>
    ```

##### Force the CDN

You can force a resource to be used from its CDN. By default the behavior is defined by configuration.

=== "Liquid"

    ``` liquid
    {% script name:"bootstrap", use_cdn:"true" %}
    ```

=== "Razor"

    ``` html
    <script asp-name="bootstrap" use-cdn="true"></script>
    ```

##### Use specific version

This example will use the latest available version with a Major version of `3`, like `3.4.0`. If the version is not specified
the latest one is always used.

=== "Liquid"

    ``` liquid
    {% script name:"bootstrap", version:"4" %}
    ```

=== "Razor"

    ``` html
    <script asp-name="bootstrap" version="3"></script>
    ```

##### Append a Version Hash

You can append a version hash that will be calculated, and calculation cached, and appended in the format ?v=eER9OO6zWGKaIq1RlNjImsrWN9y2oTgQKg2TrJnDUWk

=== "Liquid"

    ``` liquid
    {% script name:"bootstrap", append_version:"true" %}
    ```

=== "Razor"

    ``` html
    <script asp-name="bootstrap" asp-append-version="true"></script>
    ```

##### Specify location

Specify a location the script should load using `at`, for example `Foot` to rendered wherever the `FootScript` helper is located or `Head` to render with the `HeadScript` [See Foot Resources](#foot-resources). If the location is not specified, or specified as `Inline`, the script will be inserted wherever it is placed (inline).

=== "Liquid"

    ``` liquid
    {% script name:"bootstrap", at:"Foot" %}
    ```

=== "Razor"

    ``` html
    <script asp-name="bootstrap" at="Foot"></script>
    ```

Link and styles tag helpers always inject into the header section of the HTML document, unless the `at` location is set to `Inline`.

#### Inline definition

You can declare a new resource directly from a view, and it will be injected only once even if the view is called multiple time.

=== "Liquid"

    ``` liquid
    {% script name:"foo", src:"~/TheTheme/js/foo.min.js", debug_src:"~/TheTheme/js/foo.js", depends_on:"jQuery", version:"1.0" %}
    {% script name:"bar", src:"~/TheTheme/js/bar.min.js", debug_src:"~/TheTheme/js/bar.js", depends_on:"foo:1.0", version:"1.0" %}
    ```

=== "Razor"

    ``` html
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

=== "Liquid"

    ``` liquid
    {% style name:"bar", src:"~/TheTheme/css/bar.min.css", debug_src:"~/TheTheme/css/bar.css", depends_on:"foo" %}
    {% style name:"foo", src:"~/TheTheme/css/foo.min.css", debug_src:"~/TheTheme/css/foo.css", depends_on:"bootstrap" %}
    ```

=== "Razor"

    ``` html
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
    You do not have to define a name for your script or style unless you want to reference it as a dependency, or declare it as `Inline`. Hence why the above inline examples all include a name.

#### Custom scripts

The following example demonstrates how to inject a custom script in the footer section.

=== "Liquid"

    ``` liquid
    {% scriptblock at: "Foot" %}
        document.write('<!-- some script -->');
    {% endscriptblock %}
    ```

=== "Razor"

    ``` html
    <script at="Foot">
        document.write('<!-- some script -->');
    </script>
    ```

You can also inject a named custom script.

=== "Liquid"

    ``` liquid
    {% scriptblock name: "Carousel", at: "Foot", depends_on:"jQuery" %}
        document.write('<!-- some script -->');
    {% endscriptblock %}
    ```

=== "Razor"

    ``` html
    <script asp-name="Carousel" at="Foot" depends-on="jQuery">
        document.write('<!-- some script -->');
    </script>
    ```

Named script will only be injected once and can optionally specify dependencies.

#### Custom style

The following example demonstrates how to inject a custom style in the head section.
The style block will be injected after all stylesheet resources.

=== "Liquid"

    ``` liquid
    {% styleblock at: "Head" %}
        .my-class {
            /* some style */
        }
    {% endstyleblock %}
    ```

=== "Razor"

    ``` html
    <style at="Head">
        .my-class {
            /* some style */
        }
    </style>
    ```

You can also inject a named style block.
The style block will only be injected once based on its name and can optionally specify dependencies.

=== "Liquid"

    ``` liquid
    {% styleblock name: "my-style", depends_on:"the-theme" %}
        .my-class {
            /* some style */
        }
    {% endstyleblock %}
    ```

=== "Razor"

    ``` html
    <style asp-name="my-style" depends-on="the-theme">
        .my-class {
            /* some style */
        }
    </style>
    ```

#### Link tag

A link tag is used to define the relationship between the current document and an external resource such as a favicon or stylesheet. For a stylesheet, however, use the [style helper](#register-a-named-script-or-stylesheet).

=== "Liquid"

    ``` liquid
    {% link rel:"icon", type:"image/png", sizes:"16x16", src:"~/MyTheme/favicon/favicon-16x16.png" %}
    ```

=== "Razor"

    ``` html
    <link asp-src="~/MyTheme/favicon/favicon-16x16.png" rel="icon" type="image/png" sizes="16x16" />
    ```

Output

```text
<link href="/MyTheme/favicon/favicon-16x16.png" rel="icon" sizes="16x16" type="image/png" />
```

##### Using a file in the media library

If you wish to use files contained in the media library when using the link tag helper, you can use the `AssetUrl` helper directly in razor but in liquid you will need to first assign the filter result to a variable like so to generate the correct URL:

=== "Liquid"

    ``` liquid
    {% assign image_url = 'favicon/favicon-16x16.png' | asset_url %}
    {% link rel:"icon", type:"image/png", sizes:"16x16", src:image_url %}
    ```

=== "Razor"

    ``` html
    <link asp-src=@Orchard.AssetUrl("favicon/favicon-16x16.png") rel="icon" type="image/png" sizes="16x16" />
    ```

#### Meta tags

=== "Liquid"

    ``` liquid
    {% meta name:"description", content:"This is a website" %}
    ```

=== "Razor"

    ``` html
    <meta asp-name="description" content="This is a website" />
    ```

These properties are available:

| Name                         | Description                                                           |
|------------------------------|-----------------------------------------------------------------------|
| `name` (`asp-name` in Razor) | The `name` attribute of the tag                                       |
| `content`                    | The `content` attribute of the tag                                    |
| `httpequiv`                  | The `http-equiv` attribute of the tag                                 |
| `charset`                    | The `charset` attribute of the tag                                    |
| `separator`                  | The separator to use when multiple tags are defined for the same name |

### Rendering

Your `Layout.cshtml` or `Layout.liquid` must make a call to the resource manager to render resources that have been registered.

#### Head Resources

These are generally rendered at the lower portion of the `<head>` section.

=== "Liquid"

    ``` liquid
    <head>
        ...
        {% resources type: "Meta" %}
        {% resources type: "HeadLink" %}
        {% resources type: "HeadScript" %}
        {% resources type: "Stylesheet" %}
    </head>
    ```

=== "Razor"

    ``` html
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

=== "Liquid"

    ``` liquid
    <body>
        ...
        {% resources type: "FootScript" %}
    </body>    
    ```

=== "Razor"

    ``` html
    <body>
        ...
        <resources type="FootScript" />
    </body>
    ```

!!! note
    When using tag helpers in Razor, you must take a direct reference to the `OrchardCore.ResourceManagement` nuget package in each theme or module that uses the tag helpers. This is not required when using Liquid.

### Logging

If you register a resource by name and it is not found this will be logged as an error in your `App_Data/Logs` folder.

## CDN disabled by default

The `UseCdn` option, configured in the _Settings -> General_ section, is disabled by default.
This is to allow access to resources when an internet connection is not available or in countries like China, where CDNs are not always accessible.  

!!! note
    It is recommended to enable the CDN setting after setup.

## Video

<iframe width="560" height="315" src="https://www.youtube-nocookie.com/embed/jlv60tte8UE" title="YouTube video player" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture" allowfullscreen></iframe>
