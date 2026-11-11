---
title: Media library
description: Upload, organize, rename, move, copy, download and delete images and documents, pick them in content items, and control who can see each folder.
technical_manual:
  - reference/modules/Media/README.md
  - reference/modules/Media/MediaGallery.md
  - topics/media/README.md
  - topics/media/media-picker.md
  - reference/modules/SignalR/README.md
---

# Media library

The media library holds the files your site uses: pictures, PDFs, office documents, audio and video. You upload a file once, keep it in a folder, and then pick it from any content item that has a media field, or insert it in a text editor. Use this page to manage the files themselves and to pick them in content.

| | |
| --- | --- |
| **Menu** | **Media > Library** |
| **Permission** | *Manage Media* (see [Who can see and change which folders](#who-can-see-and-change-which-folders)) |
| **Feature** | **Media** (`OrchardCore.Media`) |


## Find your way around the library

Open **Media > Library**. The screen has three areas:

- **The folder tree**, on the left. The top folder is the root of the library. Click a folder to open it. Each folder has a **⋮** button (**Folder Action**) for creating a subfolder or deleting the folder.
- **The top bar**, above the files. It shows the path of the open folder (click a part of the path to go back up), a drive icon (**Storage Info**) that shows the **Available Storage** when the storage can report it, and a gear icon (**Settings**).
- **The file list**, in the middle, with the action bar above it and the pager below it.

The action bar has, from left to right:

| Button | What it does |
| --- | --- |
| **Upload files** | Opens your computer's file chooser to upload files to the open folder. |
| Trash icon (**Delete**) | Deletes the selected files. A badge shows how many files are selected. |
| Download icon (**Download**) | Downloads each selected file to your computer. |
| Grid or list icon (**Grid view** / **Table view**) | Switches between the list of rows and a grid of thumbnails. |
| **Search and filter...** | Shows only the files of the open folder whose name contains what you type. The **×** button clears it. |

The library shows the files of one folder at a time. Subfolders appear in the tree, not in the file list.

### Switch between the list and the grid

The library opens in the list view: one row per file with a file-type icon, the **Name**, **Last modified** date and **Size**. Click a column heading to sort by it; click it again to reverse the order.

Click the grid icon to see thumbnails instead. Images show a small preview; other files show an icon with their extension. Your choice, and the thumbnail size, are remembered in your browser.

Click the gear icon to change:

| Setting | What it does |
| --- | --- |
| **Items per page** | How many files each page shows: 10, 20, 50 or 100. Use **First**, **Previous**, **Next** and **Last** under the list to move between pages. |
| **Thumbnail size** | **Normal** or **Large** thumbnails in the grid view. |

!!! note
    If your IT team has turned thumbnails off, the library always uses the list view, and the grid icon and **Thumbnail size** are hidden. **Settings > Media > Options** shows **Thumbnails disabled** when this is the case.

### Select files

- In the list view, tick the box at the start of a row. The box in the heading row selects or clears every file on the page (**Select All** / **Select None**).
- In the grid view, click a thumbnail. Click it again to clear it. Selected thumbnails are highlighted.

The **Delete** and **Download** buttons work on the selection.

## Create a folder

1. In the folder tree, click the **⋮** button next to the folder that should hold the new one. Use the top folder to create a folder at the root.
2. Click **Create a subfolder**.
3. In **Create folder**, type the name in **Enter a folder name**.
4. Click **Ok**, or press Enter.

The new folder appears in the tree, inside its parent.

!!! note
    Folders can't be renamed. To rename one, create the new folder, move the files into it, and delete the old one.

## Delete a folder

1. In the folder tree, click the **⋮** button next to the folder.
2. Click **Delete**.
3. Confirm with **Ok** when the library asks *Are you sure you want to delete this folder?*

!!! warning
    Deleting a folder also deletes every file and subfolder inside it, and can't be undone. Files that content items still use then show **Not Found** in those items.

You can't delete the root folder, the `_Users` folder that holds personal folders, or the `mediafields` folder used by attached uploads.

## Upload files

1. Open the folder that should receive the files.
2. Click **Upload files** and pick one or more files, or drag files from your computer anywhere onto the library. While you drag, the library shows *Drop your file here*.
3. Watch the **Uploads** box in the corner. It lists each file with its progress.

The files appear in the open folder as soon as each upload finishes. The library uploads up to five files at a time and queues the rest. When some files fail, the **Uploads** box shows **Errors** with a count; point at a file to read why, and click **Clear Errors** to empty the list.

Uploads are checked against these rules:

| Rule | What happens |
| --- | --- |
| **File type** | Only allowed extensions can be uploaded. By default these are common images (`.jpg`, `.jpeg`, `.png`, `.gif`, `.ico`, `.webp`), documents (`.pdf`, `.doc`, `.docx`, `.ppt`, `.pptx`, `.pps`, `.ppsx`, `.odt`, `.xls`, `.xlsx`, `.csv`, `.psd`), audio (`.mp3`, `.m4a`, `.ogg`, `.wav`) and video (`.mp4`, `.m4v`, `.mov`, `.wmv`, `.avi`, `.mpg`, `.ogv`, `.3gp`, `.webm`). |
| **Restricted types** | Some extensions (by default `.css`, `.js` and `.svg`) also need the *Upload media file extensions requiring additional permission* permission, which Editors and Administrators have by default. |
| **File size** | Files larger than the maximum (30,000,000 bytes, about 30 MB, by default) are refused with *This file exceeds the maximum upload size*. |
| **Same name** | If any of the files has the same name as a file already in the folder, nothing is uploaded and the library says *A file with this name already exists in the current folder.* Rename the file on your computer, or delete the old one first. |

Your site may use other values. **Settings > Media > Options** shows the ones in use; see [Media settings](media-settings.md#see-the-media-options).

!!! note
    If the site has **Media Slugify** turned on, new file and folder names are changed to web-friendly names, for example `The team (2020).jpg` becomes `the-team-2020.jpg`. See [Media settings](media-settings.md#make-file-names-web-friendly).

### Upload large files

With the **Media TUS Uploads** (`OrchardCore.Media.Tus`) feature on, uploads are sent in pieces and can survive a broken connection. Each file in the **Uploads** box gets a pause button (**Pause upload** / **Resume upload**), and a file whose upload was interrupted continues from where it stopped when you upload it again from the same browser; it is then marked **Resumed**. The size limit still applies. Nothing else changes in how you upload.

## Rename a file

1. Click the **⋮** button at the end of the file's row (or under its thumbnail).
2. Click **Rename**.
3. In **Rename file**, change the **File name**. It is required and must keep an extension, such as `.jpg`.
4. Click **Ok**.

!!! warning
    Content items keep the path of the file they use. After a rename, a move or a delete, items that used the old path show **Not Found** until you pick the file again.

## Move files

To move one file:

1. Click the file's **⋮** button and click **Move**.
2. In **Move file**, choose the target in **Select a folder**.
3. Click **Ok**.

To move several files, select them and drag them onto a folder in the folder tree. Confirm with **Ok** when the library asks *Are you sure you want to move the selected file(s) to this folder?* The library says *File(s) moved successfully.* Dropping files on the folder they are already in shows *The file is already in this folder*.

## Copy a file

1. Click the file's **⋮** button and click **Copy**.
2. In **Copy file**, choose the target in **Select a folder**.
3. Click **Ok**.

The library says *File copied successfully.* The original stays where it was.

## View, download and share a file

- **View**, in the file's **⋮** menu, opens the file in a new browser tab. The address of that tab is the file's public address; copy it from the address bar to share the file or link to it.
- **Download**, in the same menu, saves the file to your computer. To download several files, select them and click the **Download** button in the action bar; each file is saved separately.

## Delete files

1. Select the files, or click the **⋮** button of a single file.
2. Click the **Delete** button in the action bar, or **Delete** in the file's menu.
3. Confirm with **Ok**.

Deleted files are removed from storage for good. There is no recycle bin.

## Pick media in a content item

Content types can have a **Media** field, such as the image of a blog post. How the field looks depends on the editor the content type uses (see [Fields](../content/fields.md) for the field settings **Required**, **Multiple**, **Allow media text**, **Allow anchors** and the allowed media types).

### Standard editor

1. Click **Add media**. The **Select Media** window opens with the media library inside it.
2. Browse to the file. You can upload files here too, with **Upload files** or by dragging them in.
3. Select one file, or several if the field allows multiple files. The footer shows how many are selected.
4. Click **OK**.

The files appear as thumbnails in the field. Click a thumbnail to select it, then use the buttons above:

| Button | What it does |
| --- | --- |
| **Remove** | Takes the file out of the field. The file stays in the media library. |
| **Media text** | Opens **Enter media text**, a short text kept with this file in this field. Themes usually use it as the image's alternative text for screen readers. Shown when the field allows media text. |
| **Anchor** | Opens **Select an anchor** for an image. Click the part of the image that must stay visible when the site crops or pads it, then click **OK**. **Reset Anchor** puts the point back in the center. Shown when the field allows anchors. |
| Resize icon (**Small Thumbs** / **Large Thumbs**) | Changes the size of the thumbnails in the field. |

When the field allows several files, drag the thumbnails to change their order. When it allows only one, **Add media** is unavailable until you remove the current file. Save or publish the content item to keep your changes.

A thumbnail marked **Not Found** points to a file that was renamed, moved or deleted in the library. Remove it and pick the file again.

### Gallery editor

The gallery editor shows the picked files as cards or as a list, which is easier for many files.

- Click **Add media** to open the same **Select Media** window.
- Use the icons at the top to switch between **List view** and **Grid view**, and between **Small thumbnails** and **Large thumbnails**.
- Each card has buttons to **Edit media text**, **Set anchor** (images only, when allowed), **Download** and **Remove**.
- Drag cards to change their order.

### Attached editor

The attached editor uploads files from your computer directly into the content item, instead of picking them from the library.

1. Click **Add media** and choose files, or drop files on the field where it says *Drop files here*.
2. Wait for the uploads to finish.
3. Save or publish the content item.

When you save, the files are stored in a folder of their own for this content item, under the `mediafields` folder of the library. You need the *Manage Attached Media Fields Folder* permission to use this editor.

## Insert media in text

The HTML and Markdown editors have buttons to insert images and links from the media library, and the `[image]` shortcode shows a picture from the library inside text. See [Editing text](../content/editing-text.md) and [Shortcodes](../design/shortcodes.md).

## See changes made by others live

With the **Media SignalR** (`OrchardCore.Media.SignalR`) feature on, the library updates by itself: when someone else uploads, renames, moves or deletes a file, or you do so in another browser tab, the change appears in your open library without reloading the page. It needs the **SignalR** feature, which turns on with it. Sites that run on several servers need extra setup from your IT team for this to reach everyone.

## Who can see and change which folders

These permissions control the media library. Each one includes the ones below it:

| Permission | What it allows |
| --- | --- |
| *Manage All Media Folders* | Everything in every folder, including other people's personal folders and attached uploads. |
| *Manage Media For Others* | Work in the shared folders and in every user's personal folder. |
| *Manage Own Media* | Work in the shared folders and in your own personal folder. |
| *Manage Media* | Open **Media > Library** and work in the shared folders. |
| *Manage Attached Media Fields Folder* | Upload with the attached editor of media fields (the `mediafields` folder). Includes *Manage Media*. |
| *Upload media file extensions requiring additional permission* | Upload restricted file types such as `.svg`, `.js` or `.css`. Marked security critical. |

By default, Administrators have all of them, Editors have *Manage Media*, *Manage Own Media* and the restricted upload permission, and Authors and Contributors have *Manage Own Media*. Change this in [Roles](../users/roles.md).

**Personal folders.** Everyone with *Manage Own Media* gets a personal folder inside `_Users`, created the first time they open the library. It is named after their user ID, not their user name.

### Restrict who can see files (Secure Media)

Normally every file in the library can be opened by anyone who knows its address. The **Secure Media** (`OrchardCore.Media.Security`) feature adds view permissions, so you can decide who may open the files of each folder, both in the library and on the site:

| Permission | What it allows |
| --- | --- |
| *View media content in all folders* | Open files in every folder. |
| *View media content in the root folder* | Open files that sit directly in the root folder. |
| *View media content in folder '...'* | One permission for each folder at the first level of the library, such as *View media content in folder 'images'*. |
| *View others media content* | Open files in other users' personal folders. |
| *View own media content* | Open files in your own personal folder. |

These come on top of the *Manage* permissions above; they don't replace them. A folder you create at the root gets its own permission, which appears in the role editor right away. Files uploaded with the attached editor follow the content item instead: whoever can view the content item can open its files.

When someone without permission asks for a protected file, the site answers as if the file didn't exist (*Not Found*), so the file's name isn't revealed either.

!!! warning "Turning on Secure Media changes access right away"
    By default only Administrators get *View media content in all folders* and *View others media content*, and the **Authenticated** role gets *View own media content*. Everyone else, including visitors of the public site, can no longer open media files until you give their roles view permissions. Plan the permissions before you turn the feature on, then review your roles in [Roles](../users/roles.md).
