# Media Slugify (`OrchardCore.Media.Slugify`)

The Media Slugify feature enables _slugifying_ assets in the Media Library.

## Purpose

By default the Media Library does not restrict the naming of folders and files. For example having a file `The team (2020).jpg` in folder `Images & docs` is allowed. The URL of this asset would be `/media/Images%20&%20docs/The%20team%20(2020).jpg`.

This is obviously not a very SEO-friendly URL.

Folders and files can be _slugified_ automatically by enabling the Media Slugify feature. Doing this will rename the example above to `the-team-2020.jpg` in folder `images-docs`. The URL would be `/media/images-docs/the-team-2020.jpg`.

Different files can have the same slug, making it impossible to upload both without renaming one (e.g. `The team (2020).jpg` and `The Team 2020.jpg`).

By default, transliteration happens when the feature is enabled. The following configuration values are used by default and can be customized:

```json
{
  "OrchardCore": {
    "Media": {
      "Slugify": {
        // Enable/Disable Transliteration.
        "Transliterate": true
      }
    }
  }
}
```

!!! note
    Enabling the Media Slugify feature will not rename existing folders and files, only new folders and files will be _slugified_.

## Videos

This video gives an overview of how Orchard Core manages the files of your site.

<!-- video:media-overview -->
<iframe width="560" height="315" src="https://www.youtube-nocookie.com/embed/rZ8-gT1Rcxg" title="Media: Manage the Files of Your Site" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture" allowfullscreen></iframe>

This video uploads, organizes and reuses files in the media library.

<!-- video:media-library-basics -->
<iframe width="560" height="315" src="https://www.youtube-nocookie.com/embed/96HKBh1upho" title="Media Library: Upload, Organize and Reuse Files" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture" allowfullscreen></iframe>

This video attaches images and files to content items with the Media field.

<!-- video:media-field -->
<iframe width="560" height="315" src="https://www.youtube-nocookie.com/embed/5wbolSzAOpg" title="Media Field: Attach Images and Files to Content" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture" allowfullscreen></iframe>

This video resizes and crops images the same way everywhere with media profiles.

<!-- video:media-profiles -->
<iframe width="560" height="315" src="https://www.youtube-nocookie.com/embed/kobmZSw_z_w" title="Media Profiles: Resize and Crop Images Consistently" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture" allowfullscreen></iframe>

This video goes through the media options, the allowed file types and the media API.

<!-- video:media-settings -->
<iframe width="560" height="315" src="https://www.youtube-nocookie.com/embed/8UT6FKmFhSo" title="Media Settings: Options, Allowed Files and the Media API" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture" allowfullscreen></iframe>

This video covers web-friendly file names, resumable uploads and live updates of the media library.

<!-- video:media-uploads -->
<iframe width="560" height="315" src="https://www.youtube-nocookie.com/embed/GPew_nIP47o" title="Media Uploads: Slugified Names, Resumable Uploads and Live Updates" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture" allowfullscreen></iframe>

This video resizes images, builds asset URLs and uses the media tag helpers in templates.

<!-- video:media-in-templates -->
<iframe width="560" height="315" src="https://www.youtube-nocookie.com/embed/aGWXBEQEa-0" title="Media in Templates: Image Resizing, Asset URLs and Tag Helpers" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture" allowfullscreen></iframe>
