---
title: Content items
description: Find, create, edit, save as draft, publish, unpublish, clone and delete content items, use filters and bulk actions, and clean up old versions.
technical_manual:
  - reference/modules/Contents/README.md
  - topics/content-management/README.md
  - reference/modules/Deployment/README.md
---

# Content items

Everything you write on the site, such as pages, articles, blog posts or menus, is a content item. Each content item has a content type (for example **Article**) that decides which boxes you fill in. This page shows how to find content items, create and change them, and decide when visitors see them.

| | |
| --- | --- |
| **Menu** | **Content > Content Items**, or **Content >** a content type, such as **Content > Article** |
| **Permission** | *List content items* to open the list. Then, for each action: *Edit own content* / *Edit content for others*, *Publish or unpublish own content* / *Publish or unpublish content for others*, *Delete own content* / *Delete content for others*, *Clone own content* / *Clone content*, *Preview own content* / *Preview content*. |
| **Feature** | **Contents** (`OrchardCore.Contents`) |

A content type can also have its own permissions, such as *Edit Article*, so you may see some types and not others.

By default, **Editor** can list, edit, publish, delete, preview and clone everyone's content; **Author** can edit, publish, delete, preview and clone their own content; **Contributor** can edit, preview and clone their own content but not publish it.

## Drafts, published items and versions

A content item can be in these states:

| State | Badge in the list | What visitors see |
| --- | --- | --- |
| Draft only | **Draft** | Nothing. The item has never been published, or was unpublished. |
| Published | **Published** | The item. |
| Published, with a draft | **Published** and **Draft** | The published version. Your changes in the draft stay hidden until you publish them. |

When you edit a published item and click **Save Draft**, the published version stays on the site and your changes are kept in a new draft. Click **Publish** to replace the published version with your changes.

Content types can be set up differently (see [Content types](content-types.md)):

- When a type is not **Draftable**, there is no **Save Draft** button: you can only publish.
- When a type is **Versionable** (the usual case), each publish keeps the previous version. You don't see old versions in the list, but the **Audit Trail** feature lets you compare and restore them, see [Audit trail](audit-trail.md).
- When a type is not **Versionable**, the item has only one version. Saving a draft of a published item then takes it off the site until you publish it again.

## Find content

Open **Content > Content Items**. The page is called **Manage Content** and lists the content items of every type you may see, most recently changed first. To see one content type only, click it under **Content** in the menu, for example **Content > Article**, or choose it in the content type menu above the list.

Each row shows the title, the content type, the status badges, when it was last changed (hover for the exact date and time) and who changed it last. Click the title or **Edit** to open the item. Click the content type badge to list only items of that type.

To narrow the list:

- Type words of the title in **Search** and press **Enter**.
- Use the menus above the rows:

| Menu | Options |
| --- | --- |
| Content type | **All content types**, or one type. Shown on **Content Items**, not on a single type's list. |
| **Show** | **Latest** (the default: the newest version of each item), **Owned by me** (items you own), **Published**, **Unpublished** (items whose newest version is a draft), **All versions**. |
| **Sort** | **Recently created**, **Recently modified** (the default), **Recently published**, **Title**. |

- Click the funnel button before the search box for **Only draft items**, **Only published items** and **Owned by me**.
- With the **Content Localization** feature, a culture menu (**All cultures** or a culture) shows items of one language, see [Localization](localization.md). With the **Taxonomies Contents List Filters** feature, menus filter by taxonomy terms, see [Taxonomies](taxonomies.md).

### Type filters in the search box

The search box also accepts filter words. Click the funnel button, then **Filter syntax**, to see the **Available Filters** box with every filter and the filter you are using now.

| Filter | What it does | Example |
| --- | --- | --- |
| `text:` | Finds items whose title contains the words. This is the default, so you can leave `text:` out. | `text:summer` or `summer` |
| `type:` | Shows one content type, by its technical name. | `type:Article` |
| `status:` | `Draft`, `Published`, `Owner`, `Latest` or `AllVersions`. | `status:Draft` |
| `sort:` | `Created`, `Modified`, `Published` or `Title`. | `sort:Title` |
| `stereotype:` | Shows the content types of one stereotype, such as widgets. | `stereotype:Widget` |
| `culture:` | With **Content Localization**, shows items of one culture. | `culture:fr-FR` |

You can combine filters with spaces, for example `status:Draft type:Article`. For the words of `text:`, a space means *or*, and you can use `AND`, `NOT`, brackets and quotes: `text:"summer sale"` finds the exact phrase.

The menus and the search box stay in step: choosing **Sort > Title** adds `sort:Title` to the box.

## Create a content item

1. Open **Content > Content Items** and click **New** at the top right, then choose a content type. On a single type's list, the button reads **New Article** (or the name of the type).
2. Fill in the form. Required boxes are marked, and the boxes depend on the content type: see [Editing text](editing-text.md), [Fields](fields.md) and [URLs and the home page](urls-and-homepage.md).
3. Click **Save Draft** to keep it hidden, or **Publish** to put it on the site.

The message *Your Article draft has been saved.* or *Your Article has been published.* appears. Only content types marked **Creatable** are offered under **New**.

When an administrator turns on **Display New menu** in **Settings > Admin**, a **New** section in the admin menu lets you start an item of any type from anywhere.

## Edit a content item

1. In the list, click the item's title or **Edit**.
2. Change what you need.
3. Click one of the buttons at the bottom of the page:

| Button | What it does |
| --- | --- |
| **Publish** | Saves your changes and puts this version on the site. |
| **Save Draft** | Saves your changes without changing what visitors see. |
| **Unpublish** | Takes the item off the site. Shown only when the version you are editing is published. |
| **Delete** | Deletes the item after you confirm. Shown once the item has been saved. |
| **Preview** | Opens a live preview in a new tab, with the **Content Preview** feature. See [Preview](preview.md). |
| **Cancel** | Goes back to the list without saving. Shown when you came from a list. |

After you click **Publish**, **Save Draft** or **Unpublish**, you go back to the list you came from. To stay on the item instead, click the arrow next to the button and choose **and continue** (**Publish and continue**, **Save Draft and continue**, **Unpublish and continue**). The admin remembers this choice for the next time.

Depending on the content type, the buttons area can also hold **Publish Later** and **Archive Later**, see [Scheduling](scheduling.md). When an administrator chooses the **Menu** actions layout, or on a small screen, the less common buttons are grouped in a **More actions** menu.

If a box is wrong or missing, nothing is saved and the form shows what to fix. See [Read the messages after you save](../getting-started/finding-your-way.md#read-the-messages-after-you-save).

## Publish, unpublish and other row actions

You can also act on an item from the list, without opening it. The row buttons are:

| Button | Shown when | What it does |
| --- | --- | --- |
| **Edit** | You may edit the item. | Opens the editor. |
| **View** | The item is published. | Opens the published page on the site, in a new tab. |
| **Preview Draft** | The item has a draft and you may preview it. | Shows the draft as visitors would see it, in a new tab. |

The **Actions** menu at the end of the row holds:

| Action | Shown when | What it does |
| --- | --- | --- |
| **Publish Draft** | The item has a draft and you may publish. | Publishes the draft. |
| **Discard Draft** | The item has a draft. | Throws the draft away after you confirm. A published version stays on the site. If the item was never published, this removes it. |
| **Unpublish** | The item is published and you may publish. | Takes it off the site after you confirm. Your content is kept as a draft. |
| **Delete** | You may delete the item. | Deletes it after you confirm. |
| **Clone** | You may clone the item. | Makes a copy, saved as a draft. |
| **Audit Trail** | The **Audit Trail** feature is on. | Shows the item's history, see [Audit trail](audit-trail.md). |
| **View as JSON**, **Download as JSON** | The **View Or Download Content As JSON** feature is on and you have *Export Data*. | Shows or downloads the item in a technical format. With a draft, the menu offers the draft and the published version separately. |
| **Add to Deployment Plan** | The **Add Content To Deployment Plan** feature is on. | Adds the item to a deployment plan, to copy it to another site. |
| **Export to Deployment Target** | The **Export Content To Deployment Target** feature is on. | Sends the item straight to the deployment target set in **Settings > Deployment Targets**. |

The last three are for moving content between sites, see [Import and export](../administration/import-and-export.md).

!!! note
    **Discard Draft** also needs the right to delete the item. If you can edit but not delete, the action is listed but the admin refuses it.

## Clone a content item

1. In the list, open the row's **Actions** menu and click **Clone**.

The message *Successfully cloned. The clone was saved as a draft.* appears, and the copy is in the list with the same title. If the content type has a permalink, the copy gets its own, with a number added to make it unique. The copy is never set as the home page. Edit the copy to change its title and content, then publish it.

## Delete content

1. In the list, open the row's **Actions** menu and click **Delete**, or click **Delete** at the bottom of the editor.
2. Click **Ok** to confirm.

The item disappears from the site and from the admin lists, with all its versions. With the **Audit Trail** feature, an administrator can find and restore deleted items, but only for the content types the audit trail is set to record (none by default), see [Audit trail](audit-trail.md).

## Act on several items at once

1. Tick the boxes of the items, or the box above the rows to select every item on the page.
2. Click **Actions** above the rows and choose **Publish Now**, **Unpublish** or **Delete**. With the deployment features, **Add to deployment plan** and **Export to deployment target** are also listed.
3. Click **Ok** to confirm.

The **Actions** menu appears once two or more items are selected. If one item can't be changed, nothing is changed and a message such as *Couldn't publish selected content.* appears. If you aren't allowed to change one of the items, the admin refuses the whole action. See [Work with admin lists](../getting-started/finding-your-way.md#work-with-admin-lists).

## Change the owner or the creation date

Every content item records who created it (its owner), who changed it last, and when it was created, changed and published. A site builder can let editors change the owner and the creation date, in the settings of the **Common** part of the content type:

| Setting | What it does |
| --- | --- |
| **Display date editor** | Adds a **Created** box to the editor, to change the date the item was created or first published. |
| **Display owner editor** | Adds a required **Owner** box, where you type the user name of the new owner. The box is shown only to people with *Edit the owner of a content item*. |

If you type a user name that doesn't exist, the form says *Invalid username provided for Owner.* The owner matters for the "own content" permissions: an **Author** can edit only items they own.

## Clean up old versions

Each publish of a versionable content type keeps the previous version. Over time a site can hold many old versions. **Content Version Pruning** deletes old versions once a day.

| | |
| --- | --- |
| **Menu** | **Settings > Content Version Pruning** |
| **Permission** | *Manage Content Version Pruning settings* (given to the **Administrator** role by default) |
| **Feature** | **Content Version Pruning** (`OrchardCore.Contents.VersionPruning`) |

1. Open **Settings > Content Version Pruning**.
2. Fill in the form (see the table).
3. Click **Save**.

| Field | What it does |
| --- | --- |
| **Disable** | Stops the cleanup without losing your settings. |
| **Retention period (days)** | Old versions changed longer ago than this are deleted. The default is 30 days. |
| **Archived versions to keep per item** | How many of the most recent old versions to keep for each item, whatever their age. The default is 1. Set 0 to keep none. |
| **Content types to prune** | The content types to clean up. When none is ticked, nothing is deleted. |

The latest version and the published version of an item are never deleted. The cleanup runs as the **Content Version Pruning Background Task**, which you can see in **Tools > Background Tasks**, see [Background tasks](../settings/background-tasks.md).

!!! warning
    Pruned versions are deleted for good. Keep enough days and versions to be able to go back to an older text when you need to.
