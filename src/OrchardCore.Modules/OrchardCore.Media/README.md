# Media (`OrchardCore.Media`)

The Media modules provides a UI to upload and organize binary files that can be used while creating content. 

The media-processing liquid filters can also create custom sized images.

## HTML filters

The following filters allow for media manipulation:

### `asset_url`

Returns the URL of a media file, based on its location in the media library.

#### Input

`{{ 'animals/kittens.jpg' | asset_url }}`

or when using your added content

`{{ Model.ContentItem.Content.YourContentType.YourMediaField.Paths.first | asset_url }}`

#### Output

`/media/animals/kittens.jpg`

### `img_tag`

Renders an `<img src />` HTML tag.

#### Input

`{{ 'animals/kittens.jpg' | asset_url | img_tag }}`

#### Output

`<img src="~/media/animals/kittens.jpg" />`

#### Options

You can add as many html attributes as you want with the img_tag.
`{{ 'animals/kittens.jpg' | asset_url | img_tag: alt: 'kittens', class: 'kittens black', data_order: some_var }}`

## Image resizing filters

### `resize_url`

Convert the input URL to create a resized image with the specified size arguments. 

#### Input

`{{ 'animals/kittens.jpg' | asset_url | resize_url: width:100, height:240 | img_tag }}`

#### Output

`<img src="~/media/animals/kittens.jpg?width=100&height=240" />`

#### Arguments

The `width` and `height` arguments are limited to a specific list of values to prevent 
malicious clients from creating too many variations of the same image. The values can be
`16`, `32`, `50`, `100`, `160`, `240`, `480`, `600`, `1024`, `2048`.

#### `width` (or first argument)

The width of the new image. One of the allowed values.

#### `height` (or second argument)

The height of the new image. One of the allowed values.

#### `mode` (or third argument)

The resize mode.

##### `pad`

Pads the resized image to fit the bounds of its container.
If only one dimension is passed, the original aspect ratio will be maintained.

##### `boxpad`

Pads the image to fit the bounds of the container without resizing the original source.
When downscaling, performs the same functionality as `pad`.

##### `max` (Default)

Constrains the resized image to fit the bounds of its container maintaining the original aspect ratio.

##### `min`

Resizes the image until the shortest side reaches the given dimension. Upscaling is disabled in this mode and the original image will be returned if attempted.

##### `stretch`

Stretches the resized image to fit the bounds of its container.

##### `crop`

Resizes the image using the same functionality as `max` then removes any image area falling outside the bounds of its container.

### Input

`{{ 'animals/kittens.jpg' | asset_url | resize_url: width:100, height:240, mode:'crop' }}`

### Output

`<img src="~/media/animals/kittens.jpg?width=100&height=240&rmode=crop" />`

### `append_version`

Appends a version hash for an asset. Can be piped together with the other media filters.

#### Input

`{{ 'animals/kittens.jpg' | asset_url | append_version | img_tag }}`

#### Output

`<img src="~/media/animals/kittens.jpg?v=Ailxbj_jQtYc9LRXKa21DygRzmQqc3OfN1XxSaQ3UWE" />`

## Razor Helpers

To obtain the correct URL for an asset, use the `AssetUrl` helper extension method on the view's base `Orchard` property, e.g.:

`@Orchard.AssetUrl(Model.Paths[0])`

To obtain the correct URL for a resized asset use `AssetUrl` with the optional width, height and resizeMode parameters, e.g.:

`@Orchard.AssetUrl(Model.Paths[0], width: 100 , height: 240, resizeMode: ResizeMode.Crop)`

To append a version hash for an asset use `AssetUrl` with the append version parameter, e.g.:

`@Orchard.AssetUrl(Model.Paths[0], appendVersion: true)`

or with resizing options as well, noting that the version hash is based on the source image

`@Orchard.AssetUrl(Model.Paths[0], width: 100 , height: 240, resizeMode: ResizeMode.Crop, appendVersion: true)`

### Razor image resizing tag helpers

To use the image tag helpers add `@addTagHelper *, OrchardCore.Media` to `_ViewImports.cshtml`. 

`asset-src` is used to obtain the correct URL for the asset and set the `src` attribute. Width, height and resize mode can be set using `img-width`, `img-height` and `img-resize-mode` respectively. e.g.:

`<img asset-src="Model.Paths[0]" alt="..." img-width="100" img-height="240" img-resize-mode="Crop" />`

Alternatively the Asset Url can be resolved independently and the `src` attribute used:

`<img src="@Orchard.AssetUrl(Model.Paths[0])" alt="..." img-width="100" img-height="240" img-resize-mode="Crop" />`

### Razor append version
`asp-append-version` support is available on the OrchardCore tag helpers and MVC tag helpers.

`<img asset-src="Model.Paths[0]" alt="..." asp-append-version="true" />`

Alternatively the Asset Url can be resolved independently and the `src` attribute used:

`<img src="@Orchard.AssetUrl(Model.Paths[0])" alt="..." asp-append-version="true" />`

Or when using the MVC tag helpers and the image is resolved from static assets, i.e. wwwroot

`<img src="/favicon.ico" asp-append-version="true"/>`

> The Razor Helper is accessible on the `Orchard` property if the view is using Orchard Core's Razor base class, or by injecting `OrchardCore.IOrchardHelper` in all other cases.

## Deployment Step Editor

Keep these things in mind when working with the deployment step editor:

- Selecting "Include all media." will ensure that all media is added to the package when this deployment plan executes, regardless of what you see here now.
- Selecting a file will ensure that only that file is added to the package when this deployment plan executes, regardless of what you see here now.
- Selecting a directory will ensure that all the files in that directory at the time this deployment plan executes, are added to the package during execution, regardless of what you see here now.
- Selecting all files in a directory will ensure that only those files are added to the package when this deployment plan executes, even if at that time, that directory has more files than what you see here now.

## Configuration

The following configuration values are used by default and can be customized:

```json
    "OrchardCore.Media": {
      // The accepted sizes for custom width and height
      "SupportedSizes": [ 16, 32, 50, 100, 160, 240, 480, 600, 1024, 2048 ],

      // The number of days to store images in the browser cache
      "MaxBrowserCacheDays": 30,

      // The number of days to store images in the image cache
      "MaxCacheDays": 365,

      // The maximum size of an uploaded file in bytes. 
      // NB: You might still need to configure the limit in IIS (https://docs.microsoft.com/en-us/iis/configuration/system.webserver/security/requestfiltering/requestlimits/)
      "MaxFileSize": 30000000,

      // The list of allowed file extensions
      "AllowedFileExtensions": [

            // Images
            ".jpg",
            ".jpeg",
            ".png",
            ".gif",
            ".ico",
            ".svg",
            
            // Documents
            ".pdf", // Portable Document Format; Adobe Acrobat
            ".doc", // Microsoft Word Document
            ".docx",    
            ".ppt", // Microsoft PowerPoint Presentation
            ".pptx", 
            ".pps", 
            ".ppsx", 
            ".odt", // OpenDocument Text Document
            ".xls", // Microsoft Excel Document
            ".xlsx", 
            ".psd", // Adobe Photoshop Document

            // Audio
            ".mp3",
            ".m4a",
            ".ogg",
            ".wav",

            // Video
            ".mp4", // MPEG-4
            ".m4v", 
            ".mov", // QuickTime
            ".wmv", // Windows Media Video
            ".avi",
            ".mpg",
            ".ogv", // Ogg
            ".3gp", // 3GPP
        ]
    }
```

## CREDITS

### ImageSharp

https://sixlabors.com/projects/imagesharp/

Copyright 2012 James South
Licensed under the Apache License, Version 2.0
