# ContentLocalization (`OrchardCore.ContentLocalization`)

This module allows you to localize your content items.

## LocalizationPart

Attach this part to a content type to manage multiple localized versions of a content item.

## ContentCulturePicker (`OrchardCore.ContentLocalization.ContentCulturePicker`)

## Shapes

### `ContentCulturePicker`

The `ContentCulturePicker` shape loads data for the ContentCulturePickerRender shape.

### `ContentCulturePickerRender`

The `ContentCulturePickerRender` shape is used to render the CulturePicker

| Property | Description |
| --------- | ------------ |
| `Model.CurrentCulture` | CultureInfo object representing the current thread culture. |
| `Model.SupportedCultures` | A list of CultureInfo objects for all supported cultures. |

#### ContentCulturePickerRender Example

```liquid
TODO
```

```razor
@using System.Globalization;
<ul>
    <li class="nav-item dropdown">
        <a class="nav-link dropdown-toggle" href="#" id="oc-culture-picker" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false">@Model.CurrentCulture.DisplayName</a>
        <div class="dropdown-menu" aria-labelledby="oc-culture-picker">
            @foreach (var culture in ((IEnumerable<CultureInfo>)Model.SupportedCultures).Where(c => c != Model.CurrentCulture))
            {
                <a asp-action="RedirectToLocalizedContent" 
                   asp-controller="ContentCulturePicker" 
                   asp-route-area="OrchardCore.ContentLocalization" 
                   asp-route-contentItemUrl="@FullRequestPath" 
                   asp-route-targetCulture="@culture.Name" 
                   class="dropdown-item"
                 >@culture.DisplayName</a>
            }
        </div>
    </li>
</ul>
```