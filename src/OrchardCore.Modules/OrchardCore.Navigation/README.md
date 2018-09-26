# Navigation (OrchardCore.Navigation)

## Purpose

Provides the `Navigation`, `Pager` and `PagerSlim` shapes.

## Theming

Navigation can be themed by adding the appropriate partial view files to your theme's views folder.
A good example can be found in the [TheAdmin theme project](https://github.com/OrchardCMS/OrchardCore/tree/dev/src/OrchardCore.Themes/TheAdmin).

That theme creates the standard, vertical navigation menu that is found on the admin dashboard of any OrchardCore application.
The TheAdmin theme provides the following alternates to the default ones provided in the Navigation module:  
`Navigation-admin.cshtml`  
`NavigationItem-admin.cshtml`  
`NavigationItemLink-admin.cshtml`  

The theme developer has full control over how and where navigation is displayed on their OrchardCore application.

### Pager

This is a multi-purpose pagination component that renders links to specific page numbers.
It can optionally render _First_ and _Last_ links.

| Parameter | Type | Description |
| --------- | ---- |------------ |
| `Page` | `int` | Active page number |
| `PageSize` | `int` | Number of items per page |
| `TotalItemCount` | `double` | Total number of items (used to calculate the number of the last page) |
| `Quantity` | `int?` | Number of pages to show, 7 if not specified |
| `FirstText` | `object` | Text of the "First" link, default: `T["<<"]` |
| `PreviousText` | `object` | Text of the "Previous" link, default: `T["<"]`
| `NextText` | `object` | Text of the "Next" link, default: `T[">"]` |
| `LastText` | `object` | Text of the "Last" link, default: `T[">>"]` |
| `GapText` | `object` | Text of the "Gap" element, default: `T["..."]` |
| `PagerId` | `string` | An identifier for the pager. Used to create alternate like `Pager__[PagerId]` |
| `ShowNext` | `bool` | If true, the "Next" link is always displayed |

Properties inherited from the `List` shape

| Parameter | Type | Description |
| --------- | ---- |------------ |
| `Id` | `string` | The HTML id used for the pager, default: _none_ |
| `Tag` | `string` | The HTML tag used for the pager, default: `ul` |
| `ItemTag` | `string` | The HTML tag used for the pages, default: `li` |
| `FirstClass` | `string` | The HTML class used for the first page, default: `first` |
| `LastClass` | `string` | The HTML tag used for last page, default: `last` |
| `ItemClasses` | `List<string>` | Classes that are assigned to the pages, default: _none_ |
| `ItemAttributes` | `Dictionary<string, string>` | Attributes that are assigned to the pages |

The `PagerId` property is used to create templates for specific instances. For instance, assigning
the value `MainBlog` to `PagerId` and then rendering the pager will look for a template named 
`Pager-MainBlog.cshtml`.

A pager can be further customized by defining templates for the following shapes:

- `Pager_Gap`
- `Pager_First`
- `Pager_Previous`
- `Pager_Next`
- `Pager_Last`
- `Pager_CurrentPage`

Each of these shapes are ultimately morphed into `Pager_Link`
Alternates for each of these shapes are created using the _PagerId_ like `Pager_Previous__[PagerId]` which
would in turn look for the template `Pager-MainBlog.Previous.cshtml`.

### PagerSlim

This shape renders a pager that is comprised of two links: _Previous_ and _Next_.

| Parameter | Type | Description |
| --------- | ---- |------------ |
| `PreviousClass` | `string` | The HTML class used for the _Previous_ link, default: _none_ |
| `NextClass` | `string` | The HTML class used for the _Next_ link, default: _none_ |
| `PreviousText` | `object` | Text of the "Previous" link, default: `T["<"]`
| `NextText` | `object` | Text of the "Next" link, default: `T[">"]` |

Properties inherited from the `List` shape

| Parameter | Type | Description |
| --------- | ---- |------------ |
| `Id` | `string` | The HTML id used for the pager, default: _none_ |
| `Tag` | `string` | The HTML tag used for the pager, default: `ul` |
| `ItemTag` | `string` | The HTML tag used for the pages, default: `li` |
| `FirstClass` | `string` | The HTML class used for the first page, default: `first` |
| `LastClass` | `string` | The HTML tag used for last page, default: `last` |
| `ItemClasses` | `List<string>` | Classes that are assigned to the pages, default: _none_ |
| `ItemAttributes` | `Dictionary<string, string>` | Attributes that are assigned to the pages |

A slim pager can be further customized by defining templates for the following shapes:

- `Pager_Previous`
- `Pager_Next`

Examples of Liquid alternates or templates for `Pager_Next` and `Pager_Previous`:

```liquid
{% shape_clear_alternates Model %}
{% shape_type Model "Pager_Link" %}
{% shape_add_classes Model "page-link" %}
{{ Model | shape_render }}
```

Each of these shapes are ultimately morphed into `Pager_Link`
Alternates for each of these shapes are created using the _PagerId_ like `Pager_Previous__[PagerId]` which
would in turn look for the template `Pager-MainBlog.Previous.cshtml`.

## Extending Navigation

Navigation can be extended, through code, by implementing `INavigationProvider` and registering it in the extending module (or theme) `Startup.cs` file.

Below is a sample implementation of an `INavigationProvider` used to extend the "main" navigation section of the site.

```csharp
public class MainMenu : INavigationProvider
    {
        public MainMenu(IStringLocalizer<MainMenu> localizer)
        {
            T = localizer;
        }

        public IStringLocalizer T { get; set; }

        public async Task BuildNavigation(string name, NavigationBuilder builder)
        {
            //Only interact with the "main" navigation menu here.
            if (!String.Equals(name, "main", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            builder
                .Add(T["Notifications"], T["Notifications"], layers => layers
                    .Action("Index", "Template", new { area = "CRT.Client.OrchardModules.CommunicationTemplates", groupId = 1 })
                    .LocalNav()
                );
        }
    }
```  

This provider will be called as long as the site is using a theme that includes a line similar to the following, which causes the navigation menu to be rendered by your theme at the location specified:
`@await DisplayAsync(await New.Navigation(MenuName: "main", RouteData: @ViewContext.RouteData))`

Examples of extending the admin navigation can be found in various OrchardCore modules. Searching the repository for "AdminMenu" will locate various settings. Below is a partial list:
`OrchardCore.Modules/OrchardCore.Admin/AdminFilter.cs`
`OrchardCore.Modules/OrchardCore.Media/AdminMenu.cs`

At this time, the Admin Menu is the only navigation with code dynamically adding items in the OrchardCore git repository. However, as the example above shows, the pattern can be used to control any named navigation.
