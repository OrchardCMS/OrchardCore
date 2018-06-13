# Menu (OrchardCore.Menu)

## Shapes 

### `Menu`

The `Menu` shape is used to render a Menu.

| Property | Description |
| --------- | ------------ |
| `Model.ContentItemId` | If defined, contains the content item identifier of the menu to render. |
| `Model.Items` | The list of menu items shapes for the menu. These are shapes of type `MenuItem`. |
| `Model.Differentiator` | If defined, contains the formatted name of the menu. For instance `MainMenu`. |

#### Alternates

| Definition | Template | Filename|
| ---------- | --------- | ------------ |
| `Menu__[Differentiator]` | `Menu__MainMenu` | `Menu-MainMenu.cshtml` |

### `MenuItem`

The `MenuItem` shape is used to render a menu item.

| Property | Description |
| --------- | ------------ |
| `Model.Menu` | The `Menu` shape owning this item. |
| `Model.ContentItem` | The content item representing this menu item. |
| `Model.Level` | The level of the menu item. `0` for top level menu items. |
| `Model.Items` | The list of sub menu items shapes. These are shapes of type `MenuItem`. |
| `Model.Differentiator` | If defined, contains the formatted name of the menu. For instance `MainMenu`. |

#### Alternates

| Definition | Template | Filename|
| ---------- | --------- | ------------ |
| `MenuItem__level__[level]` | `MenuItem__level__2` | `MenuItem-level-2.cshtml` |
| `MenuItem__[ContentType]` | `MenuItem__HtmlMenuItem` | `MenuItem-HtmlMenuItem.cshtml` |
| `MenuItem__[ContentType]__level__[level]` | `MenuItem__HtmlMenuItem__level__2` | `MenuItem-HtmlMenuItem-level-2.cshtml` |
| `MenuItem__[MenuName]` | `MenuItem__MainMenu` | `MenuItem-MainMenu.cshtml` |
| `MenuItem__[MenuName]__level__[level]` | `MenuItem__MainMenu__level__2` | `MenuItem-MainMenu-level-2.cshtml` |
| `MenuItem__[MenuName]__[ContentType]` | `MenuItem__MainMenu__HtmlMenuItem` | `MenuItem-MainMenu-HtmlMenuItem.cshtml` |
| `MenuItem__[MenuName]__[ContentType]__level__[level]` | `MenuItem__MainMenu__HtmlMenuItem__level__2` | `MenuItem-MainMenu-HtmlMenuItem-level-2.cshtml` |

### `MenuItemLink`

The `MenuItemLink` shape is used to render a menu item link.
This shape is created by morphing a `MenuItem` shape into a `MenuItemLink`. Hence all the properties
available on the `MenuItem` shape are still available.

| Property | Description |
| --------- | ------------ |
| `Model.Menu` | The `Menu` shape owning this item. |
| `Model.ContentItem` | The content item representing this menu item. |
| `Model.Level` | The level of the menu item. `0` for top level menu items. |
| `Model.Items` | The list of sub menu items shapes. These are shapes of type `MenuItem`. |
| `Model.Differentiator` | If defined, contains the formatted name of the menu. For instance `MainMenu`. |

#### Alternates

| Definition | Template | Filename|
| ---------- | --------- | ------------ |
| `MenuItemLink__level__[level]` | `MenuItemLink__level__2` | `MenuItemLink-level-2.cshtml` |
| `MenuItemLink__[ContentType]` | `MenuItemLink__HtmlMenuItem` | `MenuItemLink-HtmlMenuItem.cshtml` |
| `MenuItemLink__[ContentType]__level__[level]` | `MenuItemLink__HtmlMenuItem__level__2` | `MenuItemLink-HtmlMenuItem-level-2.cshtml` |
| `MenuItemLink__[MenuName]` | `MenuItemLink__MainMenu` | `MenuItemLink-MainMenu.cshtml` |
| `MenuItemLink__[MenuName]__level__[level]` | `MenuItemLink__MainMenu__level__2` | `MenuItemLink-MainMenu-level-2.cshtml` |
| `MenuItemLink__[MenuName]__[ContentType]` | `MenuItemLink__MainMenu__HtmlMenuItem` | `MenuItemLink-MainMenu-HtmlMenuItem.cshtml` |
| `MenuItemLink__[MenuName]__[ContentType]__level__[level]` | `MenuItemLink__MainMenu__HtmlMenuItem__level__2` | `MenuItemLink-MainMenu-HtmlMenuItem-level-2.cshtml` |

## CREDITS

### nestedSortable jQuery plugin

https://github.com/ilikenwf/nestedSortable  
License: MIT
