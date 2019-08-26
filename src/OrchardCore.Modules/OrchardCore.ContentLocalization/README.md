# ContentLocalization (`OrchardCore.ContentLocalization`)

This module allows you to localize your content items.

## LocalizationPart

Attach this part to a content type to manage multiple localized versions of a content item.

## ContentCulturePicker (`OrchardCore.ContentLocalization.ContentCulturePicker`)

## ContentCulturePicker Feature

The ContentCulturePicker feature helps you manage cultures for the frontend.

Enabling this module results in

-   A `ContentRequestCultureProvider` being added as the first method used to determine the current thread culture.
    This Provider will set the thread culture based on the ContentItem that matches the current url.
-   2 shapes (described below) are available to the frontend theme.

The ContentculturePicker selects the url to redirect using the following rules

-   If the `ContentItem` has a related ContentItem for the selected culture, it redirects to that Item.
-   OR If a HomePage is specified, attempts to find a Localization of the Homepage `ContentItem` for the current culture.
-   OR redirects to the current page.

### Localization Cookie

By default, the ContentCulturePicker sets a cookie for the `CookieRequestCultureProvider`. This can be disabled in the  `Configuration/Settings/ContentCulturePicker` settings page.

#### Recipe Step
The cookie can be set during recipes using the settings step. Here is a sample step:

```json
{
  "name": "settings",
  "ContentCulturePickerSettings": {
    "SetCookie": true
  },
},
```

### Shapes

#### `ContentCulturePicker`

The `ContentCulturePicker` shape loads data for the ContentCulturePickerContainer shape. 
You should always render this shape. in your theme. `{% shape "ContentCulturePicker" %}`

#### `ContentCulturePickerContainer`

The `ContentCulturePickerContainer` shape is used to render the CulturePicker. 
You should override this shape in your theme.

| Property                  | Description                                                 |
| ------------------------- | ----------------------------------------------------------- |
| `Model.CurrentCulture`    | CultureViewModel {Name, DisplayName} representing the current thread culture. |
| `Model.SupportedCultures` | A list of CultureViewModel objects for all supported cultures.   |

##### ContentCulturePickerContainer Example

```liquid
{% assign otherCultures = Model.SupportedCultures | remove_culture: cultureName: Model.CurrentCulture.Name %}
<ul>
    <li class="nav-item dropdown">
        <a class="nav-link dropdown-toggle" href="#" id="oc-culture-picker" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false">{{Model.CurrentCulture.DisplayName}}</a>
        <div class="dropdown-menu" aria-labelledby="oc-culture-picker">
            {% for culture in otherCultures %}
                <a class="dropdown-item" href="{{culture.Name | content_culture_picker_url}}">{{culture.DisplayName}}</a
            {% endfor %}
        </div>
    </li>
</ul>

```

```razor
@{ 
    var otherCultures = ((IEnumerable<CultureViewModel>)Model.SupportedCultures).Where(entry => !string.Equals(entry.Name, Model.CurrentCulture.Name, StringComparison.OrdinalIgnoreCase));
}
<ul>
    <li class="nav-item dropdown">
        <a class="nav-link dropdown-toggle" href="#" id="oc-culture-picker" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false">@Model.CurrentCulture.DisplayName</a>
        <div class="dropdown-menu" aria-labelledby="oc-culture-picker">
            @foreach (var culture in otherCultures)
            {
                <a asp-action="RedirectToLocalizedContent"
                   asp-controller="ContentculturePicker"
                   asp-route-area="OrchardCore.ContentLocalization"
                   asp-route-targetculture="@culture.Name"
                   asp-route-contentItemUrl="@Context.Request.Path"
                   class="dropdown-item">@culture.DisplayName</a>
            }
        </div>
    </li>
</ul>
```

## Liquid filters

### `remove_culture`

Removes a culture from a list of CultureViewModel. Assuming the following model:

```text
SupportedCultures = [{Name="fr", DisplayName="Français"}, {Name="en", DisplayName="English"}]
Currentculture = {Name="fr", DisplayName="Français"}
```

Input
```liquid
{{ Model.SupportedCultures | remove_culture: cultureName: Model.CurrentCulture.Name }}
```

Output

```text
[ {Name="en", DisplayName="English"}]
```
### `content_culture_picker_url`

Returns the URL of the `ContentCulturePicker` Action method.

Input

```liquid
{{ Model.Culture.Name | content_culture_picker_url }}
```

Output

```text
/Loc1/RedirectToLocalizedContent?targetculture=fr&contentItemUrl=%2Fblog
```
