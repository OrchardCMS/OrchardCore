# Liquid (`OrchardCore.Liquid`)

This module provides a way to create templates securely from the admin site.  
For more information about the Liquid syntax, please refer to this site: <https://shopify.github.io/liquid/>.
Liquid syntax is powered by Fluid. Check <https://github.com/sebastienros/fluid> for extra examples and custom filters.

## General concepts

### HTML escaping

All outputs are encoded into HTML by default.  
This means that any HTML reserved chars will be converted to the corresponding HTML entities.  
If you need to render some raw HTML chars you can use the `raw` filter.

## Content Item Filters

All the default filters that are available in the standard Liquid syntax are available in OrchardCore.  
On top of that each Orchard module can provide custom filters for their own purpose. 
Here is a list of common filters that apply to content items.

### `display_url`

Returns the URL of the content item.

Input

```liquid
{{ Model.ContentItem | display_url }}
```

Output

```text
/blog/my-blog-post
```

### `display_text`

Returns the title of the content item.

Input

```liquid
{{ Model.ContentItem | display_text }}
```

Output

```text
My Blog Post
```

### `container`

Returns the container content item of another content item.

Input

```liquid
{{ Model.ContentItem | container | display_text }}
```

In this example we assume `ContentItem` represents a blog post.

Output

```text
Blog
```
## String Filters

### `slugify`

Convert a text into a string that can be used in a URL.

Input

```liquid
{{ "This is some text" | slugify }}
```

Output

```text
this-is-some-text
```

### `local`

Converts a UTC date and time to the local date and time based on the site settings.

Input

```liquid
{{ "now" | local | date: "%c" }}
```

or

```liquid
{{ Model.ContentItem.CreatedUtc | local | date: "%c" }}
```

Output

```text
Wednesday, 02 August 2017 11:54:48
```

### `utc`

Converts a local date and time to the UTC date and time based on the site settings.

Input

```liquid
{{ "now" | utc | date: "%c" }}
```

Output

```text
Wednesday, 02 August 2017 11:54:48
```

### `t`

Localizes a string using the current culture.

Input

```liquid
{{ "Hello!" | t }}
```

Output

```text
Bonjour!
```

## Html Filters

### `absolute_url`

Creates the full absolute URL for the given relative virtual path.

Input

```liquid
{{ '~/some-page' | absolute_url }}
```

Output if there is no URL prefix for the current tenant:

```text
https://example.com/some-page
```

Output if there is a URL prefix for the current tenant:

```text
https://example.com/url-prefix/some-page
```

If the input URL starts with a tilde then the output URL's base will always be the current tenant's root URL, including the tenant prefix if one is configured. E.g. `~/` will always point to the homepage of the tenant, regardless of configuration.

The HTTP request scheme (e.g. "https") and port number are added too.

### `href`

Creates a content URL for a relative virtual path. Recommended for generating URLs in every case you want to refer to a relative path.

Input

```liquid
{{ '~/some-page' | href }}
```

Output if there is no URL prefix for the current tenant:

```text
/some-page
```

Output if there is an URL prefix for the current tenant:

```text
/url-prefix/some-page
```

If the input URL starts with a tilde then the output URL will always be relative to the current tenant's root URL, including the tenant prefix if one is configured. E.g. `~/` will always point to the homepage of the tenant, regardless of configuration.

### `html_class`

Converts a string into a friendly HTML class.

Input

```liquid
{{ "LandingPage" | html_class }}
```

Output

```text
landing-page
```

### `liquid`

Renders a liquid string template.

Input

```liquid
{{ Model.ContentItem.Content.Paragraph.Content.Html | liquid }}
```

In this example we assume that `Model.ContentItem.Content.Paragraph.Content` represents an `HtmlField`, and `Html` is the field value.

Output

```html
<p> <img src="/blog/media/kitten.jpg" /> </p>
```

Optionally you can pass a class for model binding.

### `markdownify`

Converts a Markdown string to HTML.

Input

```liquid
{{ "### Services" | markdownify }}
```

Output

```html
<h3>Services</h3>
```

### `sanitize_html`

Sanitizes some HTML content.

```liquid
{% capture output %}
  <span class="text-primary">{{ Content }}</span>
{% endcapture %}
{{ output | sanitize_html | raw }}
```

### `shortcode`

Renders Shortcodes. Should be combined with the `raw` filter.

```liquid
{{ Model.ContentItem.Content.RawHtml.Content.Html | shortcode | raw }}
```

## Json Filters

### `json`

Serializes the input value to a json string. To format the json indented, pass the `true` argument to the liquid filter.

Example:

```liquid

{{ Model.ContentItem.Content | json }}

{{ Model.ContentItem.Content | json: true }}
```

### `jsonparse`

Converts a json string to a JObject. 
This can be useful to build collections and iterate over the values in liquid.

Example:

```liquid
{% capture someCollection %}
[
  {"key":"key1", "value":"value1"},
  {"key":"key2", "value":"value2"},
]
{% endcapture %}

{% assign jsonObject = someCollection | jsonparse %}

{% for k in jsonObject %}
  {{k["key"]}} {{k["value"]}}
{% endfor %}
```

## Properties

By default the liquid templates have access to a common set of objects.

### `Model.Content`

When available, a zone shape that contains all the shapes generated by the content's parts and fields.

### `Model.ContentItem`

When available, represents the current content item being rendered.

The following properties are available on the `ContentItem` object.

| Property | Example | Description |
| --------- | ---- |------------ |
| `Id` | `12` | The id of the document in the database. |
| `ContentItemId` | `4qs7mv9xc4ttg5ktm61qj9dy5d` | The common identifier of all versions of the content item. |
| `ContentItemVersionId` | `4jp895achc3hj1qy7xq8f10nmv` | The unique identifier of the content item version. |
| `DisplayText` | `Blog` | The title of a content item. Can be edited manually by using the TitlePart. |
| `Number` | `6` | The version number. |
| `Owner` | `admin` | The username of the creator of this content item. |
| `Author` | `admin` | The username of the editor of this version. |
| `Published` | `true` | Whether this content item version is published or not. |
| `Latest` | `true` | Whether this content item version is the latest of the content item. |
| `ContentType` | `BlogPost` | The content type. |
| `CreatedUtc` | `2017-05-25 00:27:22.647` | When the content item was first created or first published. |
| `ModifiedUtc` | `2017-05-25 00:27:22.647` | When the content item version was created. |
| `PublishedUtc` | `2017-05-25 00:27:22.647` | When the content item was last published. |
| `Content` | `{ ... }` | A document containing all the content properties. See below for usage. |

#### Content property

The `Content` property of a content item exposes all of its parts and fields. It is possible to
inspect all the available properties by evaluating `Content` directly. It will then render the full document.

```liquid
<pre>{{ Model.ContentItem.Content }}</pre>
```

The convention is that each Part is exposed by its name as the first level.  
If the content item has custom fields, they will be available under a part whose name will match the content type.

For example, assuming the type `Product` has a Text field named `Size`, access the value of this field for a 
content item as follows:

```liquid
{{ Model.ContentItem.Content.Product.Size.Text }}
```

Similarly, if the content item has a `Title` part, we can access it like this:

```liquid
{{ Model.ContentItem.Content.TitlePart.Title }}
```
**Note**: This is no longer the recommended way to display the title of a content item.
Use the `Model.ContentItem.DisplayText` property instead.

### User

Represents the authenticated user for the current request.

The following properties are available on the `User` object.

| Property | Example | Description |
| --------- | ---- |------------ |
| `Identity.Name` | `admin` | The name of the authenticated user. |
| `Identity.Claims` |  | The claims of the authenticated user. |

##### user_email filter

Returns the user's email.

```liquid
{{ User | user_email }}
```

##### user_id filter

Returns the user's unique identifier.

```liquid
{{ User | user_id }}
```

##### users_by_id filter

Loads a single or multiple user objects from the database by id(s).

The resulting object has access to the following properties:

| Property | Example | Description |
| --------- | ---- |------------ |
| `UserId` | `42z3ps88pm8d40zn9cfwbee45c ` | The id of the authenticated user. |
| `UserName` | `admin` | The name of the authenticated user. |
| `NormalizedUserName` | `ADMIN` | The normailzed name of the authenticated user. |
| `Email` | `admin@gmail.com` | The email of the authenticated user. |
| `NormailizedEmail` | `ADMIN@GMAIL>COM` | The normalized email of the authenticated user. |
| `EmailConfirmed` | `true` | True if the user has confirmed his email or if the email confirmation is not required |
| `IsEnabled` | `true` | True if the user is enabled |
| `RoleNames` | `[Editor,Contributor]`  | An array of role names assigned to the user |
| `Properties` | `UserProfile.FirstName.Text` | Holds the Custom Users Settings of the user. |

You can use this filter to load the user information of the current authenticated user like this.
```liquid
{% assign user = User | user_id | users_by_id %}

{{ user.UserName }} - {{ user.Email }}

```

You can use this filter with the UserPicker field to load the picked user's information.

```liquid
{% assign users = Model.ContentItem.Content.SomeType.UserPicker.UserIds | users_by_id %}

{% for user in users %}
  {{ user.UserName }} - {{ user.Email }}
{% endfor %}

```


#### User has_permission filter

Checks if the User has permission clearance, optionally on a resource

```liquid
{{ User | has_permission:"EditContent",Model.ContentItem }}
```

#### User is_in_role filter

Checks if the user is in role

```liquid
{{ User | is_in_role:"Administrator" }}
```

#### User has_claim filter

Checks if the user has a claim of the specified type

```liquid
{{ User | has_claim:"email_verified","true" }}
{{ User | has_claim:"Permission","ManageSettings" }}
```

### Site

Gives access to the current site settings, e.g `Site.SiteName`.

| Property | Example | Description |
| -------- | ------- |------------ |
| `BaseUrl` |  | The base URL of the site. | 
| `Calendar` |  | The site's calendar. | 
| `MaxPagedCount` | `0` | The maximum number of pages that can be paged. | 
| `MaxPageSize` | `100` | The maximum page size that can be set by a user. | 
| `PageSize` | `10` | The default page size of lists. | 
| `SiteName` | `My Site` | The friendly name of the site. | 
| `SuperUser` | `4kxfgfrxqmdpnt5n508cqvpvca` | The user id of the site's super user. | 
| `TimeZoneId` | `America/Los_Angeles` | The site's time zone id as per the tz database, c.f., https://en.wikipedia.org/wiki/List_of_tz_database_time_zones | 
| `UseCdn` | `false` | Enable/disable the use of a CDN. | 
| `ResourceDebugMode` | `Disabled` | Provides options for whether src or debug-src is used for loading scripts and stylesheets | 
| `CdnBaseUrl` | `https://localhost:44300` | If provided a CDN Base url is prepended to local scripts and stylesheets  | 
| `Meta` |  | The meta to render in the head section of the current theme.| 

### Request

Represents the current request.

The following properties are available on the `Request` object.

| Property | Example | Description |
| --------- | ---- |------------ |
| `QueryString` | `?sort=name&page=1` | The escaped query string with the leading '?' character. |
| `UriQueryString` | `?sort=name&page=1` | The query string escaped in a way which is correct for combining into the URI representation. |
| `ContentType` | `application/x-www-form-urlencoded; charset=UTF-8` | The `Content-Type` header. |
| `ContentLength` | `600` | The `Content-Length` header. |
| `Cookies` | Usage: `Request.Cookies.orchauth_Default` | The collection of cookies for this request. |
| `Headers` | Usage: `Request.Headers.accept` | The request headers. Each property value is an array of values.|
| `Query` | Usage: `Request.Query.sort` | The query value collection parsed from `QueryString`. Each property value is an array of values. |
| `Form` | Usage: `Request.Form.value` | The collection of form values. |
| `Protocol` | `https` | The protocol of this request. |
| `Path` | `/OrchardCore.ContentPreview/Preview/Render` | The unescaped path of the request. |
| `UriPath` | `/OrchardCore.ContentPreview/Preview/Render` | The path escaped in a way which is correct for combining into the URI representation. |
| `PathBase` | `/mytenant` | The unescaped base path of the request. |
| `UriPathBase` | `/mytenant` | The base path escaped in a way which is correct for combining into the URI representation. |
| `Host` | `localhost:44300` | The unescaped `Host` header. May contain the port. |
| `UriHost` | `localhost:44300` | The `Host` header properly formatted and encoded for use in a URI in a HTTP header. |
| `IsHttps` | `true` | True if the scheme of the request is `https`. |
| `Scheme` | `https` | The scheme of the request. |
| `Method` | `GET` | The HTTP method. |
| `Route` | Usage: `Request.Route.controller` | The route values for this request. |

### Culture

Represents the current culture.

The following properties are available on the `Culture` object.

| Property | Example | Description |
| --------- | ---- |------------ |
| `Name` | `en-US` | The request's culture as an ISO language code. |
| `Dir` | `rtl` | The text writing direction. |


### HttpContext

Represents the HttpContext of the current request.

The following properties are available on the `HttpContext` object.

| Property | Example | Description |
| --------- | ---- |------------ |
| `Items` | `HttpContext.Items["Item1"]` | Returns an item with key Item1 |


#### httpcontext_add_items
Adds key/value to HttpContext.Items collection

`{% httpcontext_add_items Item1:"value 1", Item2:"Value2"  %}`

#### httpcontext_remove_items
Removes key from HttpContext.Items collection

`{% httpcontext_remove_items "Item1" %}`


## Shape Filters

These filters let you create and filter shapes.

### `shape_new`

Returns a shape with the specified name as input.

Input

```liquid
{% assign date_time = "DateTime" | shape_new %}
```

You can also pass properties when creating the shape.  
Property names get converted to PascalCase. Ex: `prop_name1` can be accessed via `Model.PropName1` in the shape template.

```liquid
{{ Model.Content | shape_new: prop_value1: "some value", prop_value2: 5 }}
```

### `shape_render`

Renders a shape.

```liquid
{{ Model.Content | shape_render }}
```

### `shape_stringify`

Converts a shape to its string representation. Unlike `shape_render`, the result of this filter will
be encoded if rendered in the output.

Input

```liquid
{{ "DateTime" | shape_new | shape_stringify }}

```

Output

```text
Monday, September 11, 2017 3:29:26 PM
```

### `shape_properties`

Returns a shape with the added properties.
Property names get converted to PascalCase. Ex: `prop_name1` can be accessed via `Model.PropName1` in the shape template.

Input

```liquid
{% assign my_shape = "MyCustomShape" | shape_new | shape_properties: prop_value1: "some value", prop_value2: 5 %}
```

## Layout Tags

### `layout`

Sets the layout of a view.

Input

```liquid
{% layout "CustomLayout" %}
```

Internally an alternate is added to the current theme `Layout` shape.

### `render_body`

In a layout, renders the body of the current view.

Input

```liquid
{% render_body %}
```

### `render_section`

In a layout, renders the section with the specified name.

Input

```liquid
{% render_section "Header", required: false %}
```

### `page_title`

Alters and renders the title of the current page.

Input

```liquid
{% page_title Site.SiteName, position: "before", separator: " - " %}
```

The default parameter is a text that is appended to the current value of the title.  
`position` is where the value is appended, in this example at the beginning.  
`separator` is a string that is used to separate all the fragments of the title.

## Shape Tags

### `shape_clear_alternates`

Removes any alternates from a shape.

Input

```liquid
{% shape_clear_alternates my_shape %}
```

### `shape_add_alternates`

Adds alternates to a shape.

Input

```liquid
{% shape_add_alternates my_shape "alternate1 alternate2" %}

{% assign my_alternates = "alternate1,alternate2" | split: "," %}
{% shape_add_alternates my_shape my_alternates %}
```

### `shape_clear_wrappers`

Removes any wrappers from a shape.

Input

```liquid
{% shape_clear_wrappers my_shape %}
```

### `shape_add_wrappers`

Adds wrappers to a shape.

Input

```liquid
{% shape_add_wrappers my_shape "wrapper1 wrapper2" %}

{% assign my_wrappers = "wrapper1,wrapper2" | split: "," %}
{% shape_add_wrappers my_shape my_wrappers %}
```

### `shape_clear_classes`

Removes any classes from a shape.

Input

```liquid
{% shape_clear_classes my_shape %}
```

### `shape_add_classes`

Adds classes to a shape.

Input

```liquid
{% shape_add_classes my_shape "class1 class2" %}

{% assign my_classes "class1,class2" | split: "," %}
{% shape_add_classes my_shape my_classes %}
```

### `shape_clear_attributes`

Removes any attributes from a shape.

Input

```liquid
{% shape_clear_attributes my_shape %}
```

### `shape_add_attributes`

Adds attributes to a shape.

Input

```liquid
{% shape_add_attributes my_shape attr_name1: "value1", attr_name2: "value2" ... %}
```

### `shape_add_properties`

Adds properties to a shape. This can be useful to pass values from a parent shape.  
Property names get converted to PascalCase.  
Ex: `prop_name1` can be accessed via `Model.Properties["PropName1"]` or `Model.Properties.PropName1` in the shape template.

Input

```liquid
{% shape_add_properties my_shape prop_name1: "value1", prop_name2: 2  %}
```

### `shape_remove_property`

Removes a property from a shape by name.

Input

```liquid
{% shape_remove_property my_shape "prop_name1" %}
```

### `shape_type`

Sets the type of a shape.

Input

```liquid
{% shape_type my_shape "MyType" %}
```

Whenever the type is changed, it is recommended to clear the shape alternates before using the `shape_clear_alternates` tag.

### `shape_display_type`

Sets the display type of a shape.

Input

```liquid
{% shape_display_type my_shape "Summary" %}
```

Whenever the display type is changed, it is recommended to clear the shape alternates first.

### `shape_position`

Sets the position of a shape.

Input

```liquid
{% shape_position my_shape "Content:before" %}
```

### `shape_tab`

Sets the tab of a shape.

Input

```liquid
{% shape_tab my_shape "properties" %}
```

### `shape_remove_item`

Removes a shape by its name in a Zone.

Input

```liquid
{% shape_remove_item Model.Content "HtmlBodyPart" %}
{{ Model.Content | shape_render }}
```

In this example, the `Model.Content` property evaluates to a zone shape, typically from a Content Item shape template, which contains the `HtmlBodyPart` shape
rendered for the Body Part element. This call will remove the specific shape named `HtmlBodyPart`.

### `shape_pager`

Replaces the properties of a Pager shape.

Input

```liquid
{% shape_pager Model.Pager next_class: 'next', next_text: '>>' %}
```

### `shape_build_display`

Creates the display shape for a content item. It can be used in conjunction with `shape_render`
to render a content item.

Input

```liquid
{{ mycontentitem | shape_build_display: "Detail" | shape_render }}
```

### `shape`

Creates and renders a new shape, with optional caching arguments.

Input

```liquid
{% shape "menu", alias: "alias:main-menu", cache_id: "main-menu", cache_expires_after: "00:05:00", cache_tag: "alias:main-menu" %}
```

When using the shape tag a specific wrapper and / or alternate can be specified.

```liquid
{% shape "menu", alias: "alias:main-menu", alternate: "Menu_Footer" %}
```

### `shape_cache`

Sets the caching parameters of a shape.

Input

```liquid
{% shape_cache my_shape cache_id: "my-shape", cache_expires_after: "00:05:00" %}
```

For more information about the available caching parameters please refer to [this section](../DynamicCache/README.md#shape-tag-helper-attributes)

### `zone`

Renders some HTML content in the specified zone.

Input

```liquid
{% zone "Header" %}
    <!-- some content goes here -->
{% endzone %}
```

The content of this block can then be reused from the Layout using the `{% render_section "Header" %}` code.

## Tag Helper tags

ASP.NET Core MVC provides a set of tag helpers to render predefined HTML outputs. The Liquid module provides a way to call into these Tag Helpers using custom liquid tags.


### `form`

Invokes the `form` tag helper of ASP.NET Core.

```liquid
{% form action:"Create", controller: "Todo", method: "post" %}
    ... ... ...
{% endform %}
```

###  `input`

Using `helper` invokes the `input` tag helper of ASP.NET Core and binds `Text` of the Model

```liquid
{% helper "input", for: "Text", class: "form-control" %}
```

### `label`

Using `helper` invokes the `label` tag helper of ASP.NET Core and binds `Text` of the Model

```liquid
{% helper "label", for: "Text" %}
```

### `validation_summary`

Using `helper` invokes the `validation_summary` tag helper of ASP.NET Core with `div`  

```liquid
{% helper "div", validation_summary: "All" %}
```

### `validation_for`

Using `helper` invokes the `validation_for` tag helper of ASP.NET Core with `span` and binds `Text` of the Model

```liquid
{% helper "span", validation_for: "Text" %}
```

### `link`

Invokes the `link` tag helper from the `Orchard.ResourceManagement` package. [See this section.](../Resources/README.md#link-tag)

### `meta`

Invokes the `meta` tag helper from the `Orchard.ResourceManagement` package. [See this section.](../Resources/README.md#meta-tags)

### `resources`

Invokes the `resources` tag helper from the `Orchard.ResourceManagement` package. [See this section.](../Resources/README.md#rendering)

### `script`

Invokes the `script` tag helper from the `Orchard.ResourceManagement` package. [See this section.](../Resources/README.md#inline-definition)

### `style`

Invokes the `style` tag helper from the `Orchard.ResourceManagement` package. [See this section.](../Resources/README.md#inline-definition)

### `a`

Invokes the `a` content link tag helper from the `OrchardCore.Contents` package.

### `route_*`
Route data can be added using `route_*` to tag helper of ASP.NET Core that supports route data using `asp-route-*` attribute.

In following example, `route_returnUrl` adds `returnUrl` to form action.

```liquid
{% form action: "Update", method: "post",  route_returnUrl: Request.Query["returnurl"] %}
    ... ... ...
{% endform %}
```

In following example, `route_todoid` adds `Model.TodoId` to hyperlink.

```liquid
{% a action: "Delete" , controller: "Todo", class: "btn btn-danger", route_todoid: Model.TodoId %}
    Delete
{% enda %}
```


### `antiforgerytoken`

Renders a `<hidden>` element (antiforgery token) that will be validated when the containing `<form>` is submitted.

Example

```liquid
{% antiforgerytoken %}
```

### `helper` and `block`

Allows custom Razor [TagHelpers](https://docs.microsoft.com/en-us/aspnet/core/mvc/views/tag-helpers/intro?view=aspnetcore-3.0) to be called from liquid.

For attributes (HtmlAttributeName) use camel-case and replace all `-` with `_`.

Helper Tag example

```liquid
{% helper "mycustomtag", customAttribute: "foo" %}
```

Block Tag example

```liquid
{% block "mycustomtag", customAttribute: "foo" %}
{% endblock %}
```

## Razor Helpers

### `LiquidToHtmlAsync`

To render a liquid string template as `IHtmlContent` within Razor use the `LiquidToHtmlAsync` helper extension method on the view's base `Orchard` property, e.g.:

Input

```csharp
@await Orchard.LiquidToHtmlAsync((string)Model.ContentItem.Content.Paragraph.Content.Html)
```

In this example we assume that `Model.ContentItem.Content.Paragraph.Content` represents an `HtmlField`, and `Html` is the field value, and we cast to a string, as extension methods do not support dynamic dispatching.

Output

```html
<p> <img src="/media/kitten.jpg" /> </p>
```

Optionally you can pass a class for model binding.

## CREDITS

### Fluid

<https://github.com/sebastienros/fluid>  
Copyright (c) 2017 Sebastien Ros  
MIT License
