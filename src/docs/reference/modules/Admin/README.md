# Admin (`OrchardCore.Admin`)

The Admin module provides an admin dashboard for your site.

## Custom Admin prefix

If you want to specify another prefix in the urls to access the admin section, you can change it by using this option in the appsettings.json:

``` json
  "OrchardCore": {
    "OrchardCore_Admin": {
      "AdminUrlPrefix": "YourCustomAdminUrl"
      }
    }
```
## Customize Admin branding

By default, OrchardCore logo and site name are displayed in the top navbar.

You can change it by overriding 'AdminBranding' shape, either from a [custom admin theme](../../../guides/create-admin-theme/README.md) or using Admin Templates feature.
You can also use this shape to define admin favicon.

Here are samples using logo and favicon from media module.

=== "Liquid"

    ``` liquid
    {% assign favicon_url = 'favicon.ico' | asset_url %}
    {% link rel:'shortcut icon', type:'image/x-icon', src:favicon_url %}
    {% a area: 'OrchardCore.Admin', controller: 'Admin' , action: 'Index', class: 'ta-navbar-brand' %}
        <img src="{{ 'logo.png' | asset_url }}" alt="{{ Site.SiteName }}" />
        <span>{{ Site.SiteName }}</span>
    {% enda %}
    ```

=== "Razor"

    ``` html
    <link asp-src="~/media/favicon.ico" type="image/x-icon" rel="shortcut icon" />

    <a class="ta-navbar-brand" asp-route-area="OrchardCore.Admin" asp-controller="Admin" asp-action="Index">
        <img src=@Url.Content("~/media/logo.png") alt="@Site.SiteName" />
        <span>@Site.SiteName</span>
    </a>
    ```