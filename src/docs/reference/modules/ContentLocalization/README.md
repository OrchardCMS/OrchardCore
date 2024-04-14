# ContentLocalization (`OrchardCore.ContentLocalization`)

This module allows you to localize your content items.

## LocalizationPart

Attach this part to a content type to manage multiple localized versions of a content item.

## ContentCulturePicker (`OrchardCore.ContentLocalization.ContentCulturePicker`)

## ContentCulturePicker Feature

The `ContentCulturePicker` feature helps you manage cultures for the frontend.

Enabling this feature results in

- A `ContentRequestCultureProvider` being added as the first method used to determine the current thread culture.
    This Provider will set the thread culture based on the ContentItem that matches the current url.

The `ContentCulturePicker` selects the url to redirect using the following rules

- If the `ContentItem` has a related ContentItem for the selected culture, it redirects to that Item.
- OR If a HomePage is specified, attempts to find a Localization of the Homepage `ContentItem` for the current culture.
- OR redirects to the current page.

### Localization Cookie

By default, the `ContentCulturePicker` sets a cookie for the `CookieRequestCultureProvider`. This can be disabled in the  `Configuration/Settings/Localization/Content Culture Picker` settings page.

The `ContentRequestCultureProvider` can set the cookie based on the ContentItem that matches the current url. This setting can be edited in the  `Configuration/Settings/Localization/Content Request Culture Provider` settings page.

#### Recipe Step

The cookie can be set during recipes using the settings step. Here is a sample step:

```json
{
  "name": "settings",
  "ContentCulturePickerSettings": {
    "SetCookie": true
  },
  "ContentRequestCultureProvider": {
      "SetCookie": true
  }
},
```

## Liquid filters

### `switch_culture_url`

Returns the URL of the Action that switches cultures.

Input

```liquid
{{ Model.Culture.Name | switch_culture_url }}
```

Output

```text
/Loc1/RedirectToLocalizedContent?targetculture=fr&contentItemUrl=%2Fblog
```

### `localization_set`

Returns the content item in the specified culture (defaults to request culture).

Input

```liquid
{{ Model.ContentItem.Content.LocalizationPart.LocalizationSet | localization_set: "en" }}
```

Output

```text
Title
```

## Configuration

The following configuration is used by default and can be customized:

```json
{
   "OrchardCore": {
    "OrchardCore_ContentLocalization_CulturePickerOptions": {
     "CookieLifeTime": 14 // Set the culture picker cookie life time (in days).
    }
  }
}
```

## Video

<iframe width="560" height="315" src="https://www.youtube-nocookie.com/embed/cwKa1OA48-4" title="YouTube video player" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture" allowfullscreen></iframe>
