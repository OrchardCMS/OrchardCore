# Theme management API

The theme management API lists site and admin themes available under the
tenant's active feature profile, enables a
theme and its base themes, and selects the current site or admin theme. Enable
the **Themes** feature (`OrchardCore.Themes`) to expose the `themes` capability.

## Authentication and authorization

Every operation requires:

- the Orchard Core `Api` bearer authentication scheme;
- `AccessRemoteManagement`; and
- `ApplyTheme`.

All operations are tenant-scoped. The selected tenant's settings and enabled
features are changed directly.

## Base route

```text
/api/themes
```

## Endpoints

| Method | Path | CLI | Purpose |
| --- | --- | --- | --- |
| `GET` | `/api/themes` | `pomi themes list` | List manageable themes |
| `POST` | `/api/themes/{themeId}:enable` | `pomi themes enable <themeId>` | Enable a theme and its base themes |
| `POST` | `/api/themes/{themeId}:set-current` | `pomi themes set-current <themeId>` | Select and enable a site or admin theme |

## Theme representation

```json
{
  "id": "TheAgencyTheme",
  "name": "Agency Theme",
  "description": "A theme for agency sites.",
  "isAdmin": false,
  "isEnabled": true,
  "isCurrent": true
}
```

`isCurrent` compares the feature ID with the current site theme for site themes
and the current admin theme for admin themes.

## List themes

```http
GET /api/themes
```

| Query | Type | Default | Behavior |
| --- | --- | --- | --- |
| `search` | string | null | Case-insensitive match against ID, name, or description |
| `admin` | boolean | null | Filter site (`false`) or admin (`true`) themes |
| `enabled` | boolean | null | Filter by enablement |
| `current` | boolean | null | Filter by current selection |
| `skip` | integer | `0` | Must be zero or greater |
| `take` | integer | `50` | Must be from 1 through 200 |

The list omits themes blocked by the active feature profile as well as hidden,
always-enabled, dependency-only, and non-theme features.
Current themes sort first, followed by site before admin themes, then name and
ID case-insensitively. Paging is applied last.

```bash
pomi themes list --admin false --enabled true --skip 0 --take 50
```

```json
{
  "skip": 0,
  "take": 50,
  "totalCount": 1,
  "items": [
    {
      "id": "TheAgencyTheme",
      "name": "Agency Theme",
      "description": "A theme for agency sites.",
      "isAdmin": false,
      "isEnabled": true,
      "isCurrent": true
    }
  ]
}
```

Invalid paging returns `400`. Authentication failures return `401`; missing
permissions return `403`.

## Enable a theme

```http
POST /api/themes/{themeId}:enable
```

```bash
pomi themes enable TheAgencyTheme
```

The route accepts a case-insensitive theme ID and returns the canonical theme
representation. It enables the selected feature and each declared base theme.
An already-enabled theme returns `200 OK` without another mutation, so an
identical retry converges. Unknown, hidden, always-enabled, dependency-only, and
non-theme IDs return `404`. If the active feature profile or base-theme chain
prevents enablement, the operation returns `400`.

## Set the current theme

```http
POST /api/themes/{themeId}:set-current
```

```bash
pomi themes set-current TheAgencyTheme
```

The theme manifest determines the target:

- a theme tagged `admin` becomes the current admin theme;
- every other manageable theme becomes the current site theme.

The operation enables the theme and its base themes first, then saves the
selection in a fresh tenant scope when activation reloads the shell. Repeating the
operation when the same theme is current performs no settings mutation and
returns `200 OK`. Unknown or non-manageable themes return `404`; authentication
and authorization failures return `401` or `403`. Failed enablement returns
`400` without changing the selected theme.

## Source coverage

- `src/OrchardCore.Modules/OrchardCore.Themes/Endpoints/Management/ThemeManagementEndpoints.cs`
- `src/OrchardCore.Modules/OrchardCore.Themes/Services/SiteThemeService.cs`
- `src/OrchardCore/OrchardCore.DisplayManagement/Events/ThemeFeatureBuilderEvents.cs`
- `src/OrchardCore/OrchardCore.Admin.Abstractions/IAdminThemeService.cs`
