---
title: Media settings
description: Create image resize profiles, read the media options, choose how the media library signs in, purge the media cache, and learn what web-friendly names, virus scanning and cloud storage do.
technical_manual:
  - reference/modules/Media/README.md
  - reference/modules/Media/MediaGallery.md
  - reference/modules/Media.Slugify/README.md
  - reference/modules/Antivirus/README.md
  - reference/core/file-upload-security.md
  - reference/modules/Media.AmazonS3/README.md
  - reference/modules/Media.Azure/README.md
---

# Media settings

Behind the [media library](media-library.md) are a few settings screens and several features that change how files are stored and served. Most of the limits (allowed file types, maximum size, where files are kept) are set by your IT team in the site's configuration, and the admin only shows them. What you can change in the admin are image profiles, the way the library signs in to the server, and the media cache.

| | |
| --- | --- |
| **Menu** | **Media > Profiles**; **Settings > Media > Options**; **Settings > Media > API**; **Settings > Media > Cache** |
| **Permission** | *Manage Media Profiles*; *View Media Options*; *Manage Media API authentication settings*; *Manage Asset Cache Folder* |
| **Feature** | **Media** (`OrchardCore.Media`); **Media Cache** (`OrchardCore.Media.Cache`) for **Cache** |


## Image profiles

A media profile is a named recipe for resizing images, such as *banner: 2048 × 600, cropped*. Themes and templates ask for an image "with the banner profile", and the site produces and caches a resized copy, so editors can upload one large picture and the site serves the right size everywhere. The Blog recipe, for example, creates a `banner` profile for the header images of the Blog theme.

Profiles only take effect where a theme or template uses them by name. Creating a profile doesn't change any image by itself; ask whoever builds your theme which names it uses.

### See the profiles

Open **Media > Profiles**. Each row shows the profile name and its hint, with **Edit** and **Delete** buttons. Type in **Search** and press Enter to filter by name. To delete several profiles, tick them and choose **Delete** from the **Actions** menu.

### Add a media profile

1. Open **Media > Profiles** and click **Add Media Profile**.
2. Fill in the fields below.
3. Click **Save**, or open the arrow next to it and click **and continue** to save and stay on the page.

| Field | What it does |
| --- | --- |
| **Name** | Required. The name themes and templates use, such as `banner` or `thumbnail`. Must be unique. |
| **Hint** | A short description shown in the profile list, such as the size and mode. |
| **Width** | The width of the resized image, picked from the list of supported sizes (16 to 2048 pixels by default). **Default** means the width isn't changed. **Custom Size** shows a box for any number. |
| **Height** | The same choices for the height. |
| **Resize Mode** | How the image fits the width and height. **Default (Max)** and **Max** shrink the image to fit inside the size and keep its proportions. **Crop** fills the size and cuts off what sticks out. **Pad** fits the image inside the size and fills the rest with a background color. **BoxPad** pads to the size without enlarging the image. **Min** resizes to the smallest size that covers the box, keeping proportions. **Stretch** forces the exact size, which can distort the image. |
| **Background Color** | Shown for **Pad** and **BoxPad**. The fill color, such as `white`, `ffff00`, or `128,64,32`. Empty means no color is set. |
| **Format** | Converts the image to **Gif**, **Jpg**, **Png** or **WebP**. **Default** keeps the original format. |
| **Quality** | A percentage from 1 to 100, used only for JPG and WebP. 100 means no quality change. |
| **Auto Orient** | Turns the image the right way up using the orientation the camera recorded. Ticked by default. |

!!! note
    **Custom Size** is offered only when the site signs its image addresses, which is the default. Otherwise the width and height must be one of the supported sizes.

To change a profile, click **Edit** on its row; the form is the same. Changing a profile changes every image that uses it.

## See the media options

Open **Settings > Media > Options** to see how the media library is configured. The page is read-only: *Media is configured with appsettings.json.* Your IT team changes these values; see the Technical Manual.

| Option | What it means |
| --- | --- |
| **Supported sizes** | The widths and heights offered in media profiles and accepted when images are resized. |
| **Restricted file extensions** | File types that also need the *Upload media file extensions requiring additional permission* permission. |
| **Allowed file extensions** | File types anyone with media access can upload. |
| **Max browser cache days** | How long visitors' browsers may keep a media file before asking for it again. |
| **Max secure files browser cache days** | The same for files protected by Secure Media. 0 means browsers don't keep them. |
| **Max cache days** | How long a resized copy of an image is kept before it is made again. |
| **Max file size** | The largest file that can be uploaded, in bytes. |
| **Max upload chunk size** | The size of the pieces large uploads are split into, in bytes. |
| **Temporary file lifetime** | How long unfinished uploads are kept before they are cleaned up. |
| **Cdn base url** | A content delivery network address put in front of media addresses, when your IT team uses one. |
| **Assets request path** | The start of every media address on the site, `/media` by default. |
| **Assets path** | The folder where media files are stored on the server. |
| **Assets users folder** | The folder that holds users' personal folders, `_Users` by default. |
| **Thumbnails disabled** | **Yes** when the grid of thumbnails is turned off in the media library. |
| **Storage provider** | Where the files are stored, such as the server's disk, Azure or Amazon S3. |
| **Hierarchical namespace** | Whether the storage has real folders. |
| **Atomic move** | Whether the storage moves files in one step. |
| **SignalR** | Whether the library updates live (the **Media SignalR** feature). |
| **TUS uploads** | Whether large uploads can be paused and resumed (the **Media TUS Uploads** feature). |

## Choose how the media library signs in

The media library, the media pickers and uploads talk to the server through the Media API. **Settings > Media > API** decides how they prove who you are.

1. Open **Settings > Media > API**.
2. Choose the **Media API authentication**:
    - **Cookie (default)** uses your admin sign-in. It works without any other setup.
    - **Bearer — OAuth2 + PKCE (requires OpenID Connect)** uses access tokens from the site's OpenID Connect server. It needs the **OpenID Connect Token Validation** feature and a registered client, which your IT team prepares, usually with a recipe.
3. Click **Save**.

!!! warning
    Don't switch to **Bearer** unless your IT team has set it up. Without that setup the media library can't sign in and shows *Could not sign in to the Media API*. Switch back to **Cookie (default)** to fix it.

## Purge the media cache

When your files are stored in the cloud (Azure or Amazon S3), the **Media Cache** (`OrchardCore.Media.Cache`) feature keeps local copies of the files the site serves, so it doesn't fetch them from the cloud each time. It turns on with the cloud storage features.

To clear these copies, for example after files were changed directly in the cloud storage:

1. Open **Settings > Media > Cache**. The page is titled **Asset Cache**.
2. Click **Purge All**.
3. Confirm with *Are you sure you want to purge the entire asset cache?*

The site fetches the files again as visitors ask for them. If no cloud storage is configured, the page says *The asset cache feature is enabled, but a remote media store feature is not enabled, or not configured with appsettings.json.* and there is nothing to purge.

## Make file names web-friendly

With the **Media Slugify** (`OrchardCore.Media.Slugify`) feature on, the names of folders and files you create or upload are turned into web-friendly names: lowercase, with dashes instead of spaces and without special characters. Accented letters are replaced by plain ones by default. For example, `The team (2020).jpg` in `Images & docs` becomes `the-team-2020.jpg` in `images-docs`.

There is no settings screen. Existing files keep their names. Two different names can give the same web-friendly name (`The team (2020).jpg` and `The Team 2020.jpg`), so the second upload is then refused as a duplicate; rename one of the files first.

## Scan uploads for viruses

The **ClamAV Antivirus Scanner** (`OrchardCore.Antivirus.ClamAV`) feature checks files with a ClamAV virus scanner before the site stores them. It covers uploads to the media library and the import of deployment packages.

- A file with a virus is refused and never stored.
- If the scanner can't be reached or doesn't answer in time, uploads are refused too, until it works again.

Your IT team runs the ClamAV service and connects the site to it; there is no admin screen. Turn the feature on only after they confirm the scanner is ready, otherwise every upload fails.

## Store media in the cloud

By default the media files live on the web server's disk. These features keep them in a cloud storage service instead, which suits sites that run on several servers or need a lot of space. Your IT team sets up the storage account and the connection; you keep using the media library the same way.

| Feature | What it does |
| --- | --- |
| **Azure Media Storage** (`OrchardCore.Media.Azure.Storage`) | Stores the media files in Microsoft Azure Blob Storage. |
| **Azure Media Image Cache** (`OrchardCore.Media.Azure.ImageCache`) | Stores the resized copies of images in Azure Blob Storage instead of the server's disk. |
| **Amazon Media Storage** (`OrchardCore.Media.AmazonS3`) | Stores the media files in Amazon S3. |
| **Amazon Media Image Cache** (`OrchardCore.Media.AmazonS3.ImageCache`) | Stores the resized copies of images in Amazon S3. |

The features listed as **(Obsolete)** on **Tools > Features** are old names kept for existing sites; they turn on the image cache features above.

When a storage feature is on, a read-only page shows its configuration:

- **Settings > Media > Azure Blob Options** (permission *View Azure Media Options*): **Connection String** (only **Configured** or **Not Configured**), **Container Name**, **Base Path**, **Create Container**, **Remove Container** and **Remove Files From Base Path**.
- **Settings > Media > Amazon S3 Options** (permission *View Amazon S3 Media Options*): **Bucket Name**, **Base Path** and **Create Bucket**.

!!! warning
    Turning on a cloud storage feature doesn't copy the files you already have. Ask your IT team to move them before switching, or the existing files show **Not Found**.
