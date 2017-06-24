# Content Types (Orchard.ContentTypes)

## View Components

### SelectContentTypes

Renders an editor to select a list of content types. 
It can optionally filter content types of a specific stereotype.
The editor returns the selection as a `string[]` on the model.

#### Parameters

| Parameter | Type | Description |
| --------- | ---- | ----------- |
| `selectedContentTypes` | `string[]` | The list of content types that should be marked as selected when rendering the editor. |
| `htmlName` | `string` | The name of the model property to bind the result to.
| `stereotype` (optional) | `string` | A stereotype name to filter the list of content types to be able to select. |

#### Sample

```csharp
@await Component.InvokeAsync("SelectContentTypes", new { selectedContentTypes = Model.ContainedContentTypes, htmlName = Html.NameFor(m => m.ContainedContentTypes) })

```