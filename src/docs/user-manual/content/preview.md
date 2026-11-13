---
title: Preview
description: See how a content item will look on the site while you edit it, before you publish, and preview saved drafts from the content list.
technical_manual:
  - reference/modules/ContentPreview/README.md
  - reference/modules/Contents/README.md
---

# Preview

Preview shows you a content item the way visitors will see it, before you publish it. You can watch a live preview that changes as you type, or open the last saved draft of an item from the content list.

| | |
| --- | --- |
| **Menu** | The **Preview** button at the bottom of the content editor, and **Preview Draft** in the content list |
| **Permission** | Live preview: *Preview content* (roles with *Edit content for others* or *Publish or unpublish content for others* have it too). **Preview Draft**: *Preview own content* or *Preview content* (also per content type, such as *Preview Article by others*) |
| **Feature** | **Content Preview** (`OrchardCore.ContentPreview`) for the live preview. **Preview Draft** in the list comes with **Contents** (`OrchardCore.Contents`). |

By default, **Editor** and **Administrator** may preview everyone's content. **Author** and **Contributor** can use **Preview Draft** on their own items, but not the live preview, which needs *Preview content*.

## Preview while you edit

1. Open a content item in the editor, or start a new one.
2. Click **Preview** at the bottom of the page. A new browser tab opens with the page as it would look on the site.
3. Go back to the editor tab and keep writing. The preview tab updates by itself as you change the title, text boxes and choices.
4. When you're happy, click **Publish** or **Save Draft** in the editor.

The preview shows what you typed, even before you save. Nothing you see in the preview is visible to visitors, and closing the preview tab doesn't save or lose anything: only the buttons of the editor save.

Tips:

- Place the editor and the preview tab side by side, in two windows, to see each change as you make it.
- When the preview says **Preview Disconnected**, it no longer receives your changes, for example because you closed the editor or left the page. Reopen the editor; the preview reconnects when the editor page loads again.
- When the preview says **Preview Error**, the page couldn't be built from what you typed, for example while a required box is empty. The message lists the problems. Keep editing: the preview updates once they are fixed.
- When the **Actions layout** is **Menu**, or on a small screen, **Preview** is in the **More actions** menu at the bottom of the editor.

## Preview a saved draft from the list

When an item has unpublished changes, its row in **Content > Content Items** shows a **Preview Draft** button.

1. Find the item in the list. Its row shows the **Draft** badge.
2. Click **Preview Draft**.

The latest saved draft opens in a new tab, as visitors would see it once published. Unlike the live preview, it shows only what was saved. To see the published version instead, click **View**.

## Preview pages of a separate front end (site builders)

Some sites show their pages with a separate application (a "headless" or decoupled site) instead of the Orchard Core theme. The live preview then needs to know the address where that application shows an item. Add the **Preview** part to the content type and set its **Pattern**.

| | |
| --- | --- |
| **Menu** | **Design > Content Definition > Content Types**, **Edit** on the type, then **Edit** next to the **Preview** part |
| **Permission** | *Edit content types.* |

| Setting | What it does |
| --- | --- |
| **Pattern** | The address of the preview page, as a Liquid template, for example built from the item's permalink. Your developer gives you the value. |

Without the **Preview** part, the preview uses the site's own theme, which is what most sites need.
