---
title: Site search and indexes
description: Create search indexes with Lucene, Elasticsearch or Azure AI Search, keep them up to date, and give visitors a search page and a search box.
technical_manual:
  - reference/modules/Indexing/README.md
  - reference/modules/Search/README.md
  - reference/modules/Lucene/README.md
  - reference/modules/Elasticsearch/README.md
  - reference/modules/AzureAISearch/README.md
  - topics/search/README.md
  - guides/implement-fulltext-search/README.md
---

# Site search and indexes

Site search lets visitors type words and find matching pages, blog posts and other content items. It works in two parts. An **index** is a copy of your content that a search engine can search fast. The **search page** and the **Search Form** widget let visitors search that index. You create indexes in **Search > Indexes**, pick the one the search page uses in **Settings > Search > Site Search**, and give visitors permission to search it.

This page is the full reference for site search and indexes. For a step-by-step walkthrough, see [Add site search](../use-cases/site-search.md).

| | |
| --- | --- |
| **Menu** | **Search > Indexes**; **Settings > Search > Site Search**; **Settings > Search > Azure AI Search** |
| **Permission** | *Manage Indexes* for the indexes and the **Azure AI Search** menu item. *Manage Search Settings* for the site search settings. The Azure AI Search connection form itself is shown only to administrators. Administrators have these permissions by default. |
| **Feature** | **Indexing** (`OrchardCore.Indexing`), **Search** (`OrchardCore.Search`), and at least one search engine: **Lucene** (`OrchardCore.Lucene`), **Elasticsearch** (`OrchardCore.Elasticsearch`) or **Azure AI Search** (`OrchardCore.AzureAI`) |

## Choose a search engine

Each search engine is a feature. Turn on the ones you need in **Tools > Features**, see [Features](../getting-started/features.md).

| Search engine | Where the index lives | What IT must do first |
| --- | --- | --- |
| **Lucene** | In files on the web server. | Nothing for a single server. When the site runs on several servers, each server keeps its own copy of the index, which IT needs to plan for. |
| **Elasticsearch** | On an Elasticsearch server. | Install or rent an Elasticsearch server and add its address and credentials to the site's configuration. Until then, the Elasticsearch screens show *Elasticsearch is not configured*. |
| **Azure AI Search** | In an Azure AI Search service in Microsoft Azure. | Create the Azure AI Search service. IT either puts the connection in the site's configuration, or gives you the endpoint and key to type in **Settings > Search > Azure AI Search** (see below). |

!!! note "SQL"
    The site's own database can't be used as a search index. To list content with database queries, see [Queries](queries.md).

## Create an index

1. Open **Search > Indexes** and click **Add index**.
2. In **Available Sources**, find the search engine and click **Add** under **Content in Lucene**, **Content in Elasticsearch** or **Content in Azure AI Search**. If the box says *You need to enable a feature which provides an index source.*, turn on a search engine feature first.
3. Type an **Index Name**. The **Name** is filled in for you; change it if you want.
4. Pick the **Culture** and tick the **Content types** to search.
5. Choose the other options described below.
6. Click **Save**.

The index is created in the search engine and filled in the background, so it can take a little while before every content item can be found. You see *An index has been created successfully. The synchronizing process was triggered in the background.*

| Field | What it does |
| --- | --- |
| **Index Name** | Required. The technical name of the index in the search engine. It must be unique and shorter than 255 characters, and you can't change it after the index is created. |
| **Name** | Required. A unique name for the index, shorter than 255 characters. You pick the index by this name in the search settings, in the **Search Form** widget, in queries and in the index permission. |
| **Culture** | **Any culture** indexes content in every language. Pick a culture to index only content in that language, useful for a multilingual site with one index per language. |
| **Content types** | Required, at least one. The content types whose items are added to the index. **Select all** ticks them all. |
| **Index Latest version** | When ticked, the latest draft of an item is indexed if it has one. Otherwise only the published version is indexed, which is what visitors should find. |
| **Analyzer** | How the text is split into words for searching. Lucene and Elasticsearch offer the analyzers your IT team set up (by default `standardanalyzer` for Lucene and `standard` for Elasticsearch). Azure AI Search offers a language analyzer for each language. |
| **Store Source Data** | Lucene and Elasticsearch only. Keeps a copy of the indexed values in the index, so they can be shown in query results. |

### Default Query Settings

These settings decide how the search page reads what a visitor types.

| Field | What it does |
| --- | --- |
| **Query analyzer** | Elasticsearch and Azure AI Search: the analyzer that splits the visitor's words. Lucene shows this field too, but its search page reads the words with the index's **Analyzer**. |
| **Version** | Lucene only. The Lucene version used to read search terms (**Lucene 4.8**). |
| **Allow Lucene queries** | Lucene only. The search page reads terms with the Lucene query syntax whether or not this box is ticked. |
| **Search type** | Elasticsearch only. **Multi-Match Query (Default)** looks for the words in the selected fields. **Query String Query** lets visitors use the Elasticsearch query syntax, such as quotes and `AND`. **Custom Query** uses the query you type in **Query**. |
| **Query** | Elasticsearch, with **Custom Query** only. An Elasticsearch query run for each search. Write `{{ term }}` where the visitor's words go. Ask a developer to write it. |
| **Query field names** | The fields of the index to search. Leave them all unticked to search every field. For a new Lucene index, the full-text field is ticked for you. |

### Decide what is indexed in each content type

What goes into the index is set on the content type, in **Design > Content Definition > Content Types**:

- Each part and field of the type has index settings, for example **Index settings** for Lucene with **Include this element in the index**, **Stored** and **Keyword**, **Elasticsearch Index settings** and **Azure AI Search Index Settings** with **Include this element in the index**. A part or field is indexed only when **Include this element in the index** is ticked.
- The **Full-text** settings of the type (**Include display text**, **Include body parts**, **Use custom full-text**) decide the text that free-text searches look in.

See [Content types](../content/content-types.md). After you change these settings, rebuild the index (see below).

## Search inside documents

With media indexing, the search also finds words inside documents attached to content items through a **Media** field. Turn on the format you need:

| Feature | Files it reads |
| --- | --- |
| **Text Media Indexing** (`OrchardCore.Media.Indexing.Text`) | `.txt` and `.md` |
| **PDF Media Indexing** (`OrchardCore.Media.Indexing.Pdf`) | `.pdf` |
| **OpenXML Media Indexing** (`OrchardCore.Media.Indexing.OpenXML`) | Word `.docx` and PowerPoint `.pptx` |

Each turns on **Media Indexing** (`OrchardCore.Media.Indexing`), which you don't turn on by hand. The text of the file is added to the index under the media field, so tick **Include this element in the index** on that field. When the field allows media text, the text you type for each file is indexed too. Files in the media library that aren't attached to a content item are not indexed. Rebuild the index after turning these features on, so existing items are read again.

## Keep an index up to date

When someone creates, saves, publishes, unpublishes or deletes a content item, the indexes are updated at the end of that request.

**Indexing Worker** (`OrchardCore.Indexing.Worker`) adds a background task, **Indexes Content Updater**, that runs every minute and catches up with any change the indexes missed, for example when the site runs on several servers. **Lucene Worker** (`OrchardCore.Search.Lucene.Worker`) and **Elasticsearch Worker** (`OrchardCore.Search.Elasticsearch.Worker`) are kept for older sites; on a site that uses them, **Indexing Worker** is turned on and does this work. See [Background tasks](../settings/background-tasks.md).

To fix an index by hand, open **Search > Indexes** and use the row's **Actions** menu:

| Action | What it does |
| --- | --- |
| **Synchronize** | Adds the changes made since the index was last updated. Use it when a few items are missing. |
| **Reset** | Starts indexing every content item again, without deleting the index first. |
| **Rebuild** | Deletes the index in the search engine, creates it again and indexes every content item. Use it after changing the analyzer, the index settings of a content type, or media indexing. Search results are incomplete until it finishes. |
| **Force Delete** | Deletes the index even when the search engine can't be reached. |
| **Query Index** | Lucene and Elasticsearch: opens **Run Lucene Query** or **Run Elasticsearch Query** for this index, see [Queries](queries.md). |
| **Index Info** | Elasticsearch: shows the technical details of the index, for reference only. |

**Synchronize**, **Reset**, **Rebuild** and **Force Delete** ask you to confirm; synchronizing, resetting and rebuilding run in the background. To act on several indexes, tick them and use the **Actions** button above the list: **Synchronize**, **Reset**, **Rebuild** or **Delete**.

## Edit or delete an index

- To change an index, click **Edit** in its row, change the settings and click **Save**. The **Index Name** can't be changed.
- To remove an index, click **Delete** in its row and confirm. The index is deleted in the search engine and in the site. If the search engine can't delete it, you see *Unable to delete the index for the provider ...*; use **Force Delete** in the **Actions** menu instead.

The list shows the **Name**, the **Source** (search engine and type) and when each index was **Last modified**. Type in **Search** to filter it.

## Set up the search page

1. Open **Settings > Search > Site Search**.
2. Pick the **Default search index**.
3. Type a **Placeholder** if you want, the hint shown in the empty search box.
4. Click **Save**.

| Field | What it does |
| --- | --- |
| **Default search index** | The index the search page uses when its address doesn't name one. |
| **Placeholder** | The text shown in the empty search box. When it is empty, the box shows *Enter your search term(s)*. With Dynamic Translations, you can translate it, see [Localization](../content/localization.md). |

If the page shows *No available indexes. Add at least one index to search.*, create an index first.

### Let visitors search

The search page checks the visitor's permission. In **Access Control > Roles**, give the **Anonymous** role (and **Authenticated**, for signed-in people) one of these permissions, see [Roles](../users/roles.md):

- *Query 'Name' Index*, where *Name* is the name of the index. There is one for each index.
- *Query any index*, to allow every index.

Without one of them, visitors can't see search results.

## Use the search page

Visitors search at `/search` on your site, for example `https://www.example.com/search`. The page has a search box and a **Search** button. Results show the matching content items, a page at a time, with links to the next and previous pages; the number per page is the page size in **Settings > General**. When nothing matches, the page says *There are no such results.*

To search an index other than the default one, add its name to the address: `/search/Name`, where *Name* is the index's **Name**. When no default index is set and the address names none, the page warns *No default search index has been configured.*

## Add a search box to your pages

The **Search Form** widget puts a search box in any zone, for example in the header.

1. Add a **Search Form** widget to a zone, see [Widgets and layers](../design/widgets-and-layers.md).
2. Fill in **Placeholder** (required), the hint shown in the box.
3. Leave **Index Name** empty to search the default index, or type the **Name** of another index.
4. Publish the widget.

## Connect to Azure AI Search

When your IT team gives you the connection details instead of putting them in the configuration:

1. Open **Settings > Search > Azure AI Search**.
2. Type the **Endpoint**, such as `https://my-service.search.windows.net`.
3. Pick the **Authentication Type** and fill in the field it shows.
4. Click **Save**.

| Field | What it does |
| --- | --- |
| **Endpoint** | Required. The address of your Azure AI Search service. It must be a valid URL. |
| **Authentication Type** | **Default** uses the sign-in set up on the server. **Managed Identity** uses the identity Azure gives the site. **API Key** uses a key of the search service. |
| **Api Key** | Required with **API Key**. Choose **User-specified value** and type the key (it is stored encrypted), or **Use a secret** and pick a stored secret, see [Secrets](../settings/secrets.md). |
| **Identity Client Id** | With **Managed Identity**. Leave it blank to use the system identity. |

When IT has already configured Azure AI Search, the page says so and offers **Use custom Azure Search AI settings** to use different settings for this site. When IT has turned this screen off, the menu item doesn't appear.

## Pick content with search in a Content Picker field

**Lucene Content Picker** (`OrchardCore.Search.Lucene.ContentPicker`) and **Elasticsearch Content Picker** (`OrchardCore.Search.Elasticsearch.ContentPicker`) add an editor to the **Content Picker** field that finds items through a search index, handy when there are many items to choose from. In the field's settings on the content type, choose **Lucene** or **Elasticsearch** as the editor, then pick the **Index** to search. See [Fields](../content/fields.md).

## Copy indexes to another site

Deployment plans have steps that export index definitions and rebuild or reset indexes on the target site: **Index Profiles**, **Rebuild Indexes**, **Reset Indexes**, the Lucene, Elasticsearch and Azure AI Search index steps, and **Search Settings**. See [Import and export](../administration/import-and-export.md).

## Other features

- **Lucene (Obsolete)** (`OrchardCore.Search.Lucene`), **Elasticsearch (Obsolete)** (`OrchardCore.Search.Elasticsearch`) and **Azure AI Search (Obsolete)** (`OrchardCore.Search.AzureAI`) are older names kept so existing sites keep working. Each turns on the current feature; you don't need to turn them on.

!!! example "See it in practice"
    [Add site search](../use-cases/site-search.md) walks through this step by step.
