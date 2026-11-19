---
title: Add site search
description: Turn on Lucene search, create an index of your content, make it the site's search index, let visitors use it, add a search box, and search the text of attached PDF files.
technical_manual:
  - guides/implement-fulltext-search/README.md
  - reference/modules/Lucene/README.md
  - reference/modules/Search/README.md
  - reference/modules/Indexing/README.md
---

# Add site search

Visitors expect a search box. Orchard Core's site search looks words up in an **index**: a copy of your content organized for fast searching, kept up to date as you publish. This walkthrough uses **Lucene**, which stores the index on the site's own server and needs no outside service, to give your site a search page at `/search`. This page is the step-by-step walkthrough; [Site search and indexes](../search/site-search.md) is the full reference for every search engine, index option and search setting.

## Who it's for and what you get

For **site builders** and **administrators**.

When it's done:

- a Lucene index of your pages and posts, updated each time content is published;
- a search page at `/search` that visitors can use without signing in;
- a search box on your pages;
- optionally, results that include the text inside PDF files attached to your content.

## Before you start

| You need | Where or who |
| --- | --- |
| The features **Indexing** (`OrchardCore.Indexing`), **Lucene** (`OrchardCore.Lucene`) and **Search** (`OrchardCore.Search`) | An administrator, in **Tools > Features** |
| For PDFs: the feature **PDF Media Indexing** (`OrchardCore.Media.Indexing.Pdf`). **OpenXML Media Indexing** does the same for Word and PowerPoint files, and **Text Media Indexing** for `.txt` and `.md` files. | An administrator, in **Tools > Features** |
| The permissions *Manage Indexes*, *Manage Search Settings* and *Manage Roles* | Your administrator; see [Roles and permissions](../getting-started/roles-and-permissions.md) |

!!! note "Elasticsearch or Azure AI Search instead?"
    The same screens create **Elasticsearch** and **Azure AI Search** indexes. They need a server or an Azure account that your IT team connects to the site first. See [Site search](../search/site-search.md).

## Create the index

1. Open **Search > Indexes**.
2. Click **Add index**.
3. Click **Add** next to **Content in Lucene**.
4. Fill in the form:

    | Field | What to enter |
    | --- | --- |
    | **Index Name** | Required. The index's technical name, such as `search`. |
    | **Name** | Required. A unique name for the index, such as *Search*. It's the name shown in lists and in the role editor. |
    | **Culture** | **Any culture**, or one culture on a multilingual site (make one index per language). |
    | **Content types** | Required. Tick what visitors should find, such as **Page**, **Article** and **Blog Post**. Leave out widgets, menus and taxonomies. |
    | **Index Latest version** | Leave it cleared, so drafts are never found. |
    | **Analyzer**, **Store Source Data**, **Query analyzer**, **Version**, **Allow Lucene queries** | Leave the defaults. |
    | **Query field names** | The fields a search looks in. The full text of each item is ticked by default; leave it. |

5. Click **Save**.

*An index has been created successfully. The synchronizing process was triggered in the background.* After a few moments, the index holds all your published items of those types. New and changed items are added each time you publish. See [Site search](../search/site-search.md).

## Make it the site's search index

1. Open **Settings > Search > Site Search**.
2. Choose your index in **Default search index**.
3. Type a **Placeholder** for the search box if you like, such as *Search the site*.
4. Click **Save**.

## Let visitors search

The search page checks the visitor's permissions. Without this step, visitors who aren't signed in are sent to the sign-in page.

1. Open **Access Control > Roles**.
2. Click **Edit** on **Anonymous**.
3. Under **Indexing**, tick **Query 'Search' Index**.
4. Click **Save**.
5. Do the same for **Authenticated**, so signed-in visitors can search too.

The permission is named after your index's **Name**. *Query any index* works too, but opens every index. The *Query Lucene ... Index* permission in the **Lucene** group doesn't open the search page. See [Roles](../users/roles.md).

## Add a search box

Visitors can type `/search` after your site's address, but a search box is friendlier. Add one with the **Search Form** widget:

1. Open **Design > Widgets**.
2. In a zone, such as **Content**, click **Add Widget**.
3. Choose **Search Form**.
4. Type a **Title** (required).
5. In **The layer**, choose **Always** to show the search box on every page.
6. Leave **Index Name** empty to use the default index.
7. Type a **Placeholder** if you want.
8. Click **Publish**.

You can also add the **Search Form** widget to a single page's **Flow**. See [Widgets and layers](../design/widgets-and-layers.md).

## Search inside PDF files

The site reads the text of a PDF only when the PDF is attached to a content item through a **Media Field**, and only when that field is included in the index.

1. Make sure **PDF Media Indexing** is on.
2. Open **Design > Content Definition > Content Types**.
3. Click **Edit** on the type that holds the files, such as **Article**.
4. If the type doesn't have one, add a **Media Field** named, for example, *Attachment*.
5. Click **Edit** on the field.
6. Under **Lucene Index settings**, tick **Include this element in the index**.
7. Click **Save**.
8. Edit your articles and attach PDFs from the media library in **Attachment**.
9. Publish the articles.
10. Back in **Search > Indexes**, open your index's actions menu and click **Rebuild**.
11. Click **Yes** to confirm.
12. Click **Edit** on the index.
13. In **Query field names**, also tick the field that ends in `.FileText`, such as `Article.Attachment.FileText`.
14. Click **Save**.

See [Content types](../content/content-types.md) for adding a field. **Rebuild** reads the items saved before the change again. Without the `.FileText` field in **Query field names**, searches look only in the full text and skip the PDFs' text. A PDF that's only in the media library, attached to nothing, isn't searchable. Scanned PDFs that contain only pictures of text have no text to find.

## Check that it works

1. Open your site in a private browser window, so you're a visitor.
2. Search for a word from one of your pages, in the search box or at `/search`. The page appears in the results.
3. Search for a word that's only in an attached PDF: the item it's attached to appears.
4. Search for a word that's only in a draft: nothing appears.

## Tips

- **Search returns nothing?** Check that the index exists and has the right content types, that the item is published, and that the index is chosen in **Settings > Search > Site Search**. Then click **Rebuild** in the index's actions menu. See [Troubleshooting](../troubleshooting.md).
- **Visitors are sent to the sign-in page?** The **Anonymous** role lacks the **Query '...' Index** permission.
- After you change which content types or fields an index covers, **Rebuild** it.
- A page built only from widgets in a **Flow** is found by its title; its widgets' text isn't part of the full text. See [Site search](../search/site-search.md) for the index settings of parts and fields.
- **Reset** re-reads all content into the index; **Synchronize** catches up with recent changes. Both run in the background.
