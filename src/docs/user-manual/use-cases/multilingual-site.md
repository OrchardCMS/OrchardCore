---
title: Run a multilingual site
description: Add languages to the site, make content types translatable, translate pages, let visitors switch language, and handle menus and widgets per language.
technical_manual:
  - reference/modules/ContentLocalization/README.md
  - reference/modules/Localize/README.md
  - reference/modules/DataLocalization/README.md
---

# Run a multilingual site

A multilingual site has each page in several languages, with the translations linked so visitors can switch between them. In Orchard Core a language is a **culture**, such as *en-US* (English, United States) or *fr-FR* (French, France). This walkthrough adds French to an English site, translates its pages, and lets visitors choose their language.

## Who it's for and what you get

For **site builders** who set the languages up, and **translators** who write the translations.

When it's done:

- the site supports two or more cultures, one of them the default;
- pages (or any content type you choose) can be translated, each translation with its own address;
- the admin shows which translations exist and lets you create the missing ones in one click;
- visitors switch language with a culture picker, landing on the translation of the page they were on;
- widgets that show in one language only.

## Before you start

| You need | Where or who |
| --- | --- |
| The features **Localization** (`OrchardCore.Localization`), **Content Localization** (`OrchardCore.ContentLocalization`) and **Content Culture Picker** (`OrchardCore.ContentLocalization.ContentCulturePicker`) | An administrator, in **Tools > Features** |
| The permissions *Manage supported culture*, *Edit content types.*, *Manage ContentCulturePicker settings*, and *Localize content for others* (or *Localize own content*) | Your administrator; see [Roles and permissions](../getting-started/roles-and-permissions.md) |

## Add the languages

1. Open **Settings > Localization > Cultures**.
2. Choose a culture in the list, such as **fr-FR (French (France))**.
3. Click **Add culture**.
4. Repeat steps 2 and 3 for each language.
5. In the **Default culture** column, click the button of the culture most of your content is written in.
6. Leave **Fall back to parent culture(s)** ticked.
7. Click **Save**.

The **Default culture** button's tooltip says *Set as default*. With **Fall back to parent culture(s)** ticked, a *fr-CA* visitor gets *fr* texts when no *fr-CA* text exists. At least one culture is required. The site may restart when you save, so it can be slow to respond for a moment.

## Make a content type translatable

Only content types with the **Localization** part can be translated. Add it to each type you translate, such as **Page**, **Article** and **Blog Post**.

1. Open **Design > Content Definition > Content Types**.
2. Click **Edit** on **Page**.
3. Click **Add Parts**.
4. Tick **Localization**.
5. Click **Add Selected Parts**.

The part is added right away; there is nothing else to save. See [Content types](../content/content-types.md).

## Translate a page

1. Open **Content > Content Items**.
2. On the page's row, click **Localizations**.
3. Click **+** next to **French (France) | fr-FR**.
4. Translate the **Title** and the content.
5. Leave **Permalink** empty, or type an address, such as `/fr/a-propos`.
6. Click **Publish**.

Rows of translatable items show a **Localizations** button. In its list, cultures that already have a translation show an edit icon; the others show a **+** icon. When you click **+**, the site makes a copy of the page in French and opens it, with the message *Localized version of the content created successfully.* The copy keeps the original's content and has an empty **Permalink**; left empty, the address is made from the French title.

You can also translate from the editor: the box at the top, **Current Culture: ...**, opens a list of all the cultures, with a link to edit each translation or create the missing ones. The original must be saved once before you can translate it.

New items are marked as written in the default culture. Items that existed before you added the **Localization** part show the **Localizations** button only once you open them and save or publish them again; they then get the default culture too. To find content in one language, choose it in the culture filter of the content items list (it shows **All cultures** by default). See [Localization](../content/localization.md).

## Let visitors switch language

### The culture picker

With **Content Culture Picker** on and more than one culture, **The Default Theme** shows a picker in its top bar, named after the current language. It lists the other cultures; choosing one takes the visitor to:

1. the translation of the page they're on, if there is one;
2. otherwise, if **Homepage redirect** is ticked, the translation of the home page in that culture;
3. otherwise, the same page.

**The Blog Theme** and **The Agency Theme** don't show the picker. To add it to one of them, or to your own theme, ask your developer; it's a small change to the theme's layout. See the Technical Manual.

### The picker's settings

| Screen | Setting | What it does |
| --- | --- | --- |
| **Settings > Localization > Content Culture Picker** | **Set Cookie** | Remembers the visitor's choice in a cookie. |
| | **Homepage redirect** | When the page has no translation in the chosen culture, goes to the home page's translation instead of staying on the page. |
| **Settings > Localization > Content Culture** | **Set Cookie** | Remembers the culture of the last page the visitor opened, so the rest of the site follows it. |

Whatever the cookie says, a translated page always shows in its own culture: the site takes the culture from the page being viewed.

## Translate the menu

Orchard Core has no screen that translates the items of a menu. The built-in themes show one menu, the one with the alias **main-menu**, whatever the visitor's language. You have these options:

- **Link to translated pages.** A **Content Menu Item** links to the page you choose. When a visitor on the French site uses the culture picker on that page, they're taken to the French translation, but the menu label itself stays the same in every language.
- **One menu per language.** Create a second menu (for example *Menu principal*) under **Content > Menus**, and ask your developer to change the theme so it shows the menu of the visitor's culture.

See [Menus](../design/menus.md).

## Show widgets in one language

To show a widget, such as a banner, only to visitors of one language:

1. In **Design > Widgets**, under **Layers**, click **Add**.
2. Type the layer's **Name**, such as *French*.
3. Click **Create**.
4. Click **Add** next to **Rules**.
5. Click **Add** next to **Culture condition**.
6. Choose **Equals** in **Operation**.
7. Type `fr-FR` in **Value**.
8. Click **Create**.
9. Click **Save**.
10. Add the French widget to a zone.
11. In **The layer**, choose **French**.

Make an *English* layer the same way for the English version. See [Widgets and layers](../design/widgets-and-layers.md).

## Translate the admin's own words (optional)

**Settings > Localization > Dynamic Translations** (feature **Data Localization**, `OrchardCore.DataLocalization`) translates texts that people typed in the admin rather than texts built into Orchard Core, such as the names of content types and fields and of custom admin menus.

1. Open **Settings > Localization > Dynamic Translations**.
2. Choose a **Culture**.
3. Type each **Translation**.
4. Click **Save**.

See [Localization](../content/localization.md).

The screens of the admin itself come in a language once IT installs its translation files. Each user can then choose their admin language if **Admin Culture Picker** or **User Localization** is on.

## Check that it works

1. Open a translated page in a private browser window. Its text is in the page's language.
2. With The Default Theme, use the culture picker: you land on the other translation.
3. Open a page that has no translation and switch language: you go to the home page's translation (with **Homepage redirect**) or stay on the page.
4. Check that the French-only widget shows on French pages only.

## Tips

- Add the **Localization** part before you write a lot of content, so every item starts with the right culture.
- Translate the home page first: the culture picker and **Homepage redirect** depend on it.
- Choose addresses that show the language, such as `/fr/...`, so visitors and search engines can tell the versions apart.
- Translations are separate content items: publishing, unpublishing or deleting one doesn't change the others.
