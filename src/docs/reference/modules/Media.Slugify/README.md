# Media Slugify (`OrchardCore.Media.Slugify`)

The Media Slugify feature enables _slugifying_ assets in the Media Library.

## Purpose

By default the Media Library does not restrict the naming of folders and files. For example having a file `The team (2020).jpg` in folder `Images & docs` is allowed. The URL of this asset would be `/media/Images%20&%20docs/The%20team%20(2020).jpg`.

This is obviously not a very SEO-friendly URL.

Folders and files can be _slugified_ automatically by enabling the Media Slugify feature. Doing this will rename the example above to `the-team-2020.jpg` in folder `images-docs`. The URL would be `/media/images-docs/the-team-2020.jpg`.

Different files can have the same slug, making it impossible to upload both without renaming one (e.g. `The team (2020).jpg` and `The Team 2020.jpg`).

!!! note
    Enabling the Media Slugify feature will not rename existing folders and files, only new folders and files will be _slugified_.