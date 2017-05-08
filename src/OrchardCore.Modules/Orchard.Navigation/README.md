# Navigation (Orchard.Navigation)

## Purpose

Provides he `Navigation`, `Pager` and `PagerSlim` shapes.

## Theming

### Pager

This is a multi-purpose pagination component that renders links to specific page numbers.
It can optionally render _First_ and _Last_ links.

| Parameter | Type | Description |
| --------- | ---- |------------ |
| `Page` | `int` | Active page number |
| `PageSize` | `int` | Number of items per page |
| `TotalItemCount` | `double` | Total number of items (used to calculate the number of the last page) |
| `Quantity` | `int?` | Number of pages to show, 7 if not specified |
| `FirstText` | `object` | Text of the "First" link , default: `T["<<"]` |
| `PreviousText` | `object` | Text of the "Previous" link , default: `T["<"]`
| `NextText` | `object` | Text of the "Next" link , default: `T[">"]` |
| `LastText` | `object` | Text of the "Last" link , default: `T[">>"]` |
| `GapText` | `object` | Text of the "Gap" element , default: `T["..."]` |
| `PagerId` | `string` | An identifier for the pager. Used to create alternate like `Pager__[PagerId]` |
| `ShowNext` | `bool` | If true, the "Next" link is always displayed |

Properties inherited from the `List` shape

| Parameter | Type | Description |
| --------- | ---- |------------ |
| `Id` | `string` | The HTML id used for the pager. default: _none_ |
| `Tag` | `string` | The HTML tag used for the pager. default: `ul` |
| `ItemTag` | `string` | The HTML tag used for the pages. default: `li` |
| `FirstClass` | `string` | The HTML class used for the first page. default: `first` |
| `LastClass` | `string` | The HTML tag used for last page. default: `last` |
| `ItemClasses` | `List<string>` | Classes that are assigned to the pages. default: _none_ |
| `ItemAttributes` | `Dictionary<string, string>` | Attributes that are assigned to the pages |

The `PagerId` property is used to create templates for specific instances. For instance, assigning
the value `MainBlog` to `PagerId` and then rendering the pager will look for a template named 
`Pager-MainBlog.cshtml`.

A pager can be further customized by defining templates for the following shapes;
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
| `PreviousClass` | `string` | The HTML class used for the _Previous_ link. default: _none_ |
| `NextClass` | `string` | The HTML class used for the _Next_ link. default: _none_ |
| `PreviousText` | `object` | Text of the "Previous" link , default: `T["<"]`
| `NextText` | `object` | Text of the "Next" link , default: `T[">"]` |

Properties inherited from the `List` shape

| Parameter | Type | Description |
| --------- | ---- |------------ |
| `Id` | `string` | The HTML id used for the pager. default: _none_ |
| `Tag` | `string` | The HTML tag used for the pager. default: `ul` |
| `ItemTag` | `string` | The HTML tag used for the pages. default: `li` |
| `FirstClass` | `string` | The HTML class used for the first page. default: `first` |
| `LastClass` | `string` | The HTML tag used for last page. default: `last` |
| `ItemClasses` | `List<string>` | Classes that are assigned to the pages. default: _none_ |
| `ItemAttributes` | `Dictionary<string, string>` | Attributes that are assigned to the pages |

A slim pager can be further customized by defining templates for the following shapes;
- `Pager_Previous`
- `Pager_Next`

Each of these shapes are ultimately morphed into `Pager_Link`
Alternates for each of these shapes are created using the _PagerId_ like `Pager_Previous__[PagerId]` which
would in turn look for the template `Pager-MainBlog.Previous.cshtml`.