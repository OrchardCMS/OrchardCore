---
title: Translate content
description: Make a multilingual site - choose its cultures, translate content items, add a language picker, and translate the names shown in the admin.
technical_manual:
  - reference/modules/Localize/README.md
  - reference/modules/ContentLocalization/README.md
  - reference/modules/DataLocalization/README.md
  - topics/localize/README.md
---

# Translate content

Orchard Core can run a site in several languages. A **culture** is a language, optionally with a region: `en-US` (English, United States), `fr-FR` (French, France), `fr` (French). You choose the cultures the site supports, then translate each content item into them. Visitors switch language with a culture picker, and editors can work in the admin in their own language.

This page covers every localization screen. For a step-by-step walkthrough, see [Build a multilingual site](../use-cases/multilingual-site.md).

| | |
| --- | --- |
| **Menu** | **Settings > Localization > Cultures**, **Settings > Localization > Content Culture**, **Settings > Localization > Content Culture Picker**, **Settings > Localization > Dynamic Translations** |
| **Permission** | *Manage supported culture*; *Localize content for others*, *Localize own content*; *Manage ContentCulturePicker settings*; *View dynamic translations and statistics*, *Manage all dynamic translations*, *Manage ... translations* (one per culture) |
| **Feature** | **Localization** (`OrchardCore.Localization`), **Content Localization** (`OrchardCore.ContentLocalization`), **Content Culture Picker** (`OrchardCore.ContentLocalization.ContentCulturePicker`), **Admin Culture Picker** (`OrchardCore.Localization.AdminCulturePicker`), **Content Language Header** (`OrchardCore.Localization.ContentLanguageHeader`), **Data Localization** (`OrchardCore.DataLocalization`), **Localized Content Item Sitemaps** (`OrchardCore.ContentLocalization.Sitemaps`) |


## Choose the site's cultures

1. Open **Settings > Localization > Cultures**.
2. Pick a culture in the drop-down list under the table, for example `fr-FR (French (France))`, and click **Add culture**. Repeat for each language.
3. Choose the **Default culture** with the radio button in its row.
4. To remove a culture, click the trash button (**Remove culture**) in its row.
5. Click **Save**.

| Field | What it does |
| --- | --- |
| **Define which cultures are supported by the site.** | Required. The table of supported cultures. At least one is needed. |
| **Default culture** | The culture used when no other one applies, and the culture given to new content items. If you remove the default culture, the first culture in the table becomes the default. |
| **Fall back to parent culture(s)** | When a text has no translation for `fr-CA`, use the `fr` one. |

!!! warning
    Saving restarts the site so the change takes effect. It may be slow to respond for a moment, and a message says *The site has been restarted for the settings to take effect.*

The **Localization** feature also translates the admin screens and the theme's built-in texts into the visitor's culture, using translation files your developer installs. Permission: *Manage supported culture* (**Administrator** by default).

## Make a content type translatable

1. Open **Design > Content Definition > Content Types** and click **Edit** on the type.
2. Click **Add Parts**, tick **Localization** and click **Add Selected Parts**. The part is added right away; there is nothing else to save.

Every content item of that type now has a culture, the default culture for new items. The content items list shows the culture of each item as a badge with a globe icon.

## Translate a content item

1. Open the content item's editor. It must have been saved at least once.
2. Click **Current Culture: ...** at the top of the editor to open the localization panel.
3. Click a culture with a **+** icon to create that translation (cultures with a pencil icon already have one).
4. The site copies the item into a new content item in that culture and opens its editor, with the message *Localized version of the content created successfully.*
5. Translate the texts and click **Publish** or **Save Draft**.

You can also translate from **Content > Content Items**: open the **Localizations** menu on the item's row and click the culture to create (**+**) or edit (pencil).

Each translation is a separate content item, with its own permalink, status and versions, linked to the others of the same *localization set*. If a translation already exists, the site shows *A localization already exists for '...'.* and stays on the item.

| Permission | Who has it by default |
| --- | --- |
| *Localize content for others* | **Administrator**, **Editor** |
| *Localize own content* | **Administrator**, **Editor**, **Author**, **Contributor** |

You also need the permission to edit content items of the type.

## Find items by culture

In **Content > Content Items**, use the **Culture** drop-down to show the items of one culture, or **All cultures**. You can also type `culture:fr-FR` in the search box.

## Let visitors switch language

Turn on the **Content Culture Picker** feature. When the site has more than one culture, a language menu appears in the top navigation of the site (in themes that show the navigation bar). It lists the other cultures by their native names.

When a visitor picks a culture, the site opens the translation of the page they are on. The picker settings decide what happens when there is no translation.

### Content Culture Picker settings

1. Open **Settings > Localization > Content Culture Picker**.
2. Set the options and click **Save**.

| Setting | What it does |
| --- | --- |
| **Set Cookie** | Remembers the visitor's chosen culture in a cookie. |
| **Homepage redirect** | When the page has no translation in the chosen culture, go to the homepage of that culture instead. |

### Content Culture settings

The **Content Culture Picker** feature also makes the site use the culture of the content item a visitor opens: opening the French translation of a page shows the whole page (menus, theme texts, dates) in French.

1. Open **Settings > Localization > Content Culture**.
2. Tick **Set Cookie** to remember that culture in the visitor's cookie, so the next pages they open stay in it.
3. Click **Save**.

Both settings screens need the *Manage ContentCulturePicker settings* permission (**Administrator** and **Editor** by default).

## Let editors switch the admin language

Turn on the **Admin Culture Picker** feature. When the site has more than one culture, a language menu appears in the top bar of the admin. Each editor picks the language of the admin screens for themselves; the choice is remembered in their browser.

To let each user choose their culture in their profile instead, use the **User Localization** feature (`OrchardCore.Users.Localization`). See [Users](../users/users.md).

## Send the language to browsers and search engines

The **Content Language Header** feature has no screen. When it is on, every page the site sends tells browsers and search engines its language (with the `Content-Language` response header). Turn it on from **Tools > Features**.

## Add translations to the sitemap

With the **Localized Content Item Sitemaps** feature on, content item sitemaps list each page in the default culture and link to its translations in the other cultures, so search engines know they are the same page in other languages. See [Sitemaps](../search/sitemaps.md).

## Translate names shown in the admin

**Data Localization** translates texts that people typed into the site rather than texts in the code, such as the display names of content types, the display names of content fields, and the texts of admin menu items. For example, a French editor can see *Article de blog* instead of *Blog Post*.

1. Open **Settings > Localization > Dynamic Translations**.
2. Pick the **Culture** to translate into.
3. Optionally pick a **Category** (such as the content types or the fields), type in **Search translations**, or tick **Missing only** to show only texts without a translation.
4. Type the translation in the **Translation** column, next to the **Original** text.
5. Click **Save**. **Auto-save** is ticked when the page opens: your changes are then saved two seconds after you stop typing. An **Unsaved changes** badge shows when there is something to save, and switching to another culture asks before discarding it.

Each category shows how many strings it has and how many are translated.

### See translation progress

Click **Statistics** at the bottom of **Dynamic Translations**. The page shows the **Overall Progress**, the **Progress by Culture** (with a button to edit each culture's translations), and the **Progress by Category** for a culture you pick. Click **Edit Translations** to go back.

### Data Localization permissions

| Permission | What it allows |
| --- | --- |
| *View dynamic translations and statistics* | See the translations and statistics without changing them. Given to **Editor** by default. The page then says you have read-only access. |
| *Manage all dynamic translations* | Translate into every culture. Given to **Administrator** by default. |
| *Manage ... translations* | One permission per supported culture, for example *Manage French (France) translations*: translate into that culture only. |
| *Manage dynamic localizations* | An older permission kept for existing roles. |

!!! example "See it in practice"
    [Run a multilingual site](../use-cases/multilingual-site.md) walks through this step by step.
