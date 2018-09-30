# Media (OrchardCore.Media)

The Media modules provides a UI to upload and organize binary files that can be used while creating content. 

The media processing liquid filters can also create custom sized images.

## HTML filters

The following filters allow for media manipulation:

### asset_url

Returns the URL of a media based on its location in the media library.

#### Input

`{{ 'animals/kittens.jpg' | asset_url }}`

or when using your added content

`{{ Model.ContentItem.Content.YourContentType.YourMediaField.Paths.first | asset_url }}`

#### Output

`/media/animals/kittens.jpg`

### img_tag

Renders an `<img src />` HTML tag.

#### Input

`{{ 'animals/kittens.jpg' | asset_url | img_tag }}`

#### Output

`<img src="/media/animals/kittens.jpg" />`

#### Options

##### alt (Default)

The alternate text attribute value

## Image resizing filters

### resize_url

Convert the input URL to create a dynamic image with the specified size arguments. 

#### Input

`{{ 'animals/kittens.jpg' | asset_url | resize_url: width:100, height:240 | img_tag }}`

#### Output

`<img src="/media/animals/kittens.jpg?width=100&height=240" />`

#### Arguments

The `width` and `height` arguments are limited to a specific list of values to prevent 
malicious clients from creating too many variations of the same image. The values can be
`16`, `32`, `50`, `100`, `160`, `240`, `480`, `600`, `1024`, `2048`.

#### width (or first argument)

The width of the new image. One of the allowed values.

#### height (or second argument)

The height of the new image. One of the allowed values.

#### mode (or third argument)

The resize mode.

##### pad

Pads the resized image to fit the bounds of its container.
If only one dimension is passed, the original aspect ratio will be maintained.

##### boxpad

Pads the image to fit the bounds of the container without resizing the original source.
When downscaling, performs the same functionality as `pad`.

##### max (Default)

Constrains the resized image to fit the bounds of its container maintaining the original aspect ratio.

##### min
Resizes the image until the shortest side reaches the given dimension. Upscaling is disabled in this mode and the original image will be returned if attempted.

##### stretch

Stretches the resized image to fit the bounds of its container.

### Input

`{{ 'animals/kittens.jpg' | asset_url | resize_url: width:100, height:240, mode:'crop' }}`

### Output

`<img src="/media/animals/kittens.jpg?width=100&height=240&rmode=crop" />`

## Razor Helpers

To obtain the correct URL for an asset, use the `AssetUrl` helper extension method on the view's base `Orchard` property, e.g.:

`@Orchard.AssetUrl(Model.Field.Paths[0])`

To obtain the correct URL for a resized asset use `AssetUrl` with the optional width, height and resizeMode parameters, e.g.:

`@Orchard.AssetUrl(Model.Field.Paths[0], width: 100 , height: 240, resizeMode: ResizeMode.Crop)`

### Razor image resizing tag helpers

To use the image tag helpers add `@addTagHelper *, OrchardCore.Media` to _ViewImports. `asset-src` is used to obtain the correct URL for the asset and set the `src` attribute. Width, height and resize mode can be set using `img-width`, `img-height` and `img-resize-mode` respectively. e.g.:

`<img asset-src="Model.Field.Paths[0]" alt="..." img-width="100" img-height="240" img-resize-mode="Crop" />`

Alternatively the Asset Url can be resolved independently and the `src` attribute used:

`<img src="@Orchard.AssetUrl(Model.Field.Paths[0])" alt="..." img-width="100" img-height="240" img-resize-mode="Crop" />`

> The Razor Helper is accessible on the `Orchard` property if the view is using Orchard Core's Razor base class, or by injecting `OrchardCore.IOrchardHelper` in all other cases.

## CREDITS

### ImageSharp

https://sixlabors.com/projects/imagesharp/

Copyright 2012 James South
Licensed under the Apache License, Version 2.0
