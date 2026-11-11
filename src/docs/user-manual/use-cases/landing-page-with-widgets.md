---
title: Build a landing page
description: Build a page from widgets in a Flow, lay them out in columns, make the page the site's home page, and show a banner on every page with a widget on a layer.
technical_manual:
  - reference/modules/Flow/README.md
  - reference/modules/Widgets/README.md
  - reference/modules/Layers/README.md
  - reference/modules/Autoroute/README.md
  - reference/modules/HomeRoute/README.md
---

# Build a landing page

A landing page is the first page visitors see: a headline, some pictures, a few short blocks of text, a call to action. In Orchard Core you build it from **widgets** stacked in a **Flow**, without writing HTML. This walkthrough builds such a page, makes it the home page, and adds an announcement banner that shows on every page of the site.

## Who it's for and what you get

For **site builders** and **editors**.

When it's done:

- a **Welcome** page built from widgets: text, images, a quote and two side-by-side columns;
- that page as the site's home page;
- a banner widget, such as *Free shipping this week*, shown on every page through the **Always** layer.

## Before you start

| You need | Where or who |
| --- | --- |
| The features **Flows** (`OrchardCore.Flows`), **Widgets** (`OrchardCore.Widgets`), **Layers** (`OrchardCore.Layers`), **Autoroute** (`OrchardCore.Autoroute`) and **Home Route** (`OrchardCore.HomeRoute`). The Blog recipe turns them all on. | An administrator, in **Tools > Features** |
| A content type with a **Flow** part, such as **Page** in the Blog recipe, and some widget types (the Blog recipe has **Paragraph**, **Image**, **Blockquote**, **Container** and **Raw Html**) | Already there with the Blog recipe; otherwise see [Content types](../content/content-types.md) |
| Permission to create and publish pages, the permission *Set homepage.* to change the home page, and *Manage layers* to place site-wide widgets | Your administrator; see [Roles and permissions](../getting-started/roles-and-permissions.md) |

## Build the page

1. Open **Content > Content Items**.
2. Click **New**.
3. Choose **Page**.
4. Type **Welcome** in **Title**.
5. Leave **Permalink** empty.
6. In **Flow**, click the **+** (**Add Widget**) button below the flow.
7. Choose **Paragraph**.
8. Type your headline and an opening sentence in the paragraph's **Content** editor.
9. Click **+** again.
10. Choose **Image**.
11. Under **Image**, click **Add media**.
12. Choose or upload a picture.
13. Click **OK**.
14. Type a short **Caption**.
15. Add a **Blockquote**.
16. Type a customer's words in **Quote**.
17. Add a **Raw Html** widget only if someone gave you a ready-made snippet, such as a video embed.
18. Click **Save Draft**.
19. Click **Preview** to see the page in a new tab.

With **Permalink** empty, the address becomes `/welcome`. The image is required, and the hint of **Caption** says it's used as the image's title or alternative text. A **Raw Html** widget shows its HTML as is.

Each widget has a header with buttons:

| Button | What it does |
| --- | --- |
| Alignment (the first drop-down) | **Left**, **Center**, **Right**, **Justify** or **Inherit** (from the flow). |
| Width (shows **100%**) | How much of the row the widget takes: **25%**, **33%**, **50%**, **66%**, **75%** or **100%**. With the built-in themes, widgets narrower than 100% sit side by side on wide screens. |
| Toggle all widgets | Folds or unfolds all the widgets, to make a long flow easier to read. |
| **+** (**Insert Widget**) | Adds a widget right above this one. |
| Trash | Removes the widget, after you confirm. |

Drag a widget by its header to move it up or down. See [Flows and bags](../content/flows-and-bags.md).

## Lay out two columns

1. In the width drop-down of two widgets that follow each other, such as two **Paragraph**s, choose **50%** for each.
2. To group several widgets into one column, add a **Container** widget.
3. In the container's width drop-down, choose **50%**.
4. Add widgets inside the container with its own **+** button.
5. Click **Save Draft**.
6. Click **Preview** to check the result.

Two widgets at **50%** show side by side on wide screens. A **Container** has its own flow, so the widgets inside it form one column.

## Make it the home page

1. On the page's editor, tick **Set as homepage** under **Permalink**.
2. Click **Publish**.

The **Set as homepage** checkbox appears only on content types whose **Autoroute** settings allow it, and only if your role has *Set homepage.* The page becomes the home page when it's published. The site shows this page at the site's root address. Its own address, `/welcome`, keeps working. The editor of the home page says *This content item is the current homepage*. On a Blog recipe site, the blog was the home page; it stays at `/blog`, so add it to the menu if you still want visitors to find it (see [Build a blog](build-a-blog.md)). See [URLs and the home page](../content/urls-and-homepage.md).

## Add a banner on every page

A widget placed through **Design > Widgets** isn't part of one page: it appears in a **zone** of the site's layout on every page where its **layer** applies. The Blog recipe has two layers: **Always** (every page) and **Homepage** (only the home page).

1. Open **Design > Widgets**.
2. Under **Zones**, find the zone where the banner should go, such as **Content**.
3. Click the zone's **Add Widget** button.
4. Choose **Paragraph**.
5. Type a **Title** (required), for example *Shipping banner*.
6. Leave **Render title** cleared, unless the title should show above the widget.
7. In **The layer** (required), choose **Always**.
8. Type the banner text in **Content**.
9. Click **Publish**.

The banner shows in the **Content** zone of every page. Drag widgets within a zone to order them. To show the banner only on the home page, choose **Homepage** instead. See [Widgets and layers](../design/widgets-and-layers.md).

!!! note "Which zones can I use?"
    **Design > Widgets** lists the zones named in **Settings > Zones** (**Content** and **Footer** with the Blog recipe). A zone shows on the site only if the theme displays it. The Blog theme also displays **Header** and **Messages**; to use them, add their names in **Settings > Zones**. Ask your developer which zones your theme has.

## Show a widget on some pages only

A layer decides where its widgets appear through **rules**. To make one, for example for a summer sale section:

1. In **Design > Widgets**, under **Layers**, click **Add**.
2. Type a **Name**, such as *Summer sale*.
3. Click **Create**.
4. On the layer's page, click **Add** next to **Rules**.
5. Click **Add** next to a condition.
6. Fill in the condition's settings, such as **Operation** and **Value** for a URL.
7. Click **Create**.
8. Back on the layer, click **Save**.
9. Place widgets on the layer as in the previous section, choosing it in **The layer**.

The conditions include **Url condition** (pages whose address matches), **Content type condition**, **Culture condition**, **Role condition**, **Is anonymous condition**, **Is authenticated condition**, **Homepage condition**, and more. Several conditions on one layer must all be true; use an **Any condition** group when one of them is enough. A layer with no rule at all shows its widgets nowhere. See [Widgets and layers](../design/widgets-and-layers.md).

## Check that it works

1. Open the site's root address in a private browser window: the **Welcome** page shows, with the banner.
2. Open another page, such as a blog post: the banner shows there too.
3. Make the window narrow: with the built-in themes, side-by-side widgets stack on small screens.

## Tips

- Keep text in **Paragraph** widgets rather than **Raw Html**, so editors can change it safely.
- Write a real **Caption** for each **Image**: screen readers and search engines read it.
- A flow belongs to its page; a layer widget belongs to the whole site. Use a layer for anything that repeats across pages, such as banners and footers, so you change it in one place.
- To remove a layer, first delete or move its widgets: a layer with widgets can't be deleted.
