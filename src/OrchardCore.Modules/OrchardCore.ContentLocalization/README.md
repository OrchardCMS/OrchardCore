# ContentLocalization (`OrchardCore.ContentLocalization`)

This module allows you to localize your content items.

## LocalizationPart

Attach this part to a content type to manage multiple localized versions of a content item.

## ContentCulturePicker (`OrchardCore.ContentLocalization.ContentCulturePicker`)

## ContentCulturePicker Feature

The ContentCulturePicker module helps you manage cultures for the frontend.

Enabling this module results in

-   A `ContentRequestCultureProvider` being added as the first method used to determine the current thread culture.
    This Provider will set the thread culture based on the ContentItem that matches the current url.
-   3 shapes (described below) are available to the frontend theme.

The ContentculturePicker selects the url to redirect to like:

-   If the `ContentItem` has a related ContentItem for the selected culture, it redirects to that Item
-   OR If a HomePage is specified, finds the ContentItem that is set as the Homepage and attempt to find a Localization of this `ContentItem` for the current culture.

-   OR a `NotFound()` page

### Shapes

#### `ContentCulturePicker`

The `ContentCulturePicker` shape loads data for the ContentCulturePickerContainer shape.

#### `ContentCulturePickerContainer`

The `ContentCulturePickerContainer` shape is used to render the CulturePicker.

| Property                  | Description                                                 |
| ------------------------- | ----------------------------------------------------------- |
| `Model.CurrentCulture`    | CultureInfo object representing the current thread culture. |
| `Model.SupportedCultures` | A list of CultureInfo objects for all supported cultures.   |

##### ContentCulturePickerContainer Example

```liquid
TODO
```

```razor
@using System.Globalization

<ul>
    <li class="nav-item dropdown">
        <a class="nav-link dropdown-toggle" href="#" id="oc-culture-picker" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false">@Model.CurrentCulture.DisplayName</a>
        <div class="dropdown-menu" aria-labelledby="oc-culture-picker">
            @foreach (var culture in ((IEnumerable<CultureInfo>)Model.SupportedCultures).Where(entry => !string.Equals(entry.Name, Model.CurrentCulture.Name, StringComparison.OrdinalIgnoreCase)))
            {
            @await DisplayAsync(await New.ContentCulturePickerLink(
               Culture: culture
            ))
            }
        </div>
    </li>
</ul>

```

#### `ContentCulturePickerLink`

The `ContentCulturePickerRender` shape is used to render the CulturePicker

| Property  | Description                                              |
| --------- | -------------------------------------------------------- |
| `Culture` | CultureInfo object representing the link target culture. |

##### ContentCulturePickerLink Example

```liquid
TODO
```

```razor
<a asp-action="RedirectToLocalizedContent"
   asp-controller="ContentculturePicker"
   asp-route-area="OrchardCore.ContentLocalization"
   asp-route-targetculture="@Model.Culture.Name"
   asp-route-contentItemUrl="@Context.Request.Path"
   asp-route-queryString="@Context.Request.QueryString"
   class="dropdown-item">@Model.Culture.DisplayName</a>
```
