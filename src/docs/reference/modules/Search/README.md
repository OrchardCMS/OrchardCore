# Search (`OrchardCore.Search`)

The `OrchardCore.Search` module provides a frontend search experience for your site. It exposes a search page and a search form, and queries an index built by one of the search providers (Lucene, Elasticsearch or Azure AI Search).

This module only provides the user-facing search UI. The actual indexes are created and managed by the [`Indexing`](../Indexing/README.md) infrastructure together with a provider:

- [Lucene](../Lucene/README.md)
- [Elasticsearch](../Elasticsearch/README.md)
- [Azure AI Search](../AzureAISearch/README.md)

Enable `OrchardCore.Search` plus at least one provider feature, and create an index profile, before using the search page.

## Search page

The module registers a search endpoint:

```
/search/{index?}
```

- `index` (optional) — the name of the index profile to query. When omitted, the **default index** configured in the search settings is used.
- `terms` — the search query, passed as a query string value (`/search?terms=orchard`).

If no index name is supplied and no default index is configured, a warning is displayed. If the requested index does not exist, the endpoint returns `404`.

Results are paged using the site's configured page size, with "previous"/"next" links generated automatically.

## Settings

Configure the search experience under **Search** > **Settings** > **Site Search** in the admin (requires the `Manage Search Settings` permission).

| Setting               | Description                                                                                  |
|-----------------------|----------------------------------------------------------------------------------------------|
| `DefaultIndexProfileName` | The index profile queried when no index is specified in the URL.                         |
| `PageTitle`           | The title displayed on the search results page.                                              |
| `Placeholder`         | The placeholder text shown in the search input box.                                          |

These settings can be exported and imported through a deployment plan using the **Search Settings** deployment step.

## Permissions

| Permission              | Description                                                            |
|-------------------------|-----------------------------------------------------------------------|
| `Manage Search Settings` | Allows configuring the site search settings.                         |
| `Query Search Index`    | Allows querying a search index. Granted per index profile.            |

Both permissions are granted to the `Administrator` role by default. To let anonymous or other roles use the search page, grant `Query Search Index` for the relevant index profile.

## Search form

The module ships a `SearchFormPart` and a `Search Form` widget so a search box can be placed anywhere, such as in a layer zone or on a page. The form submits a `GET` request to the `/search` endpoint with the entered `Terms`. The placeholder text is data-localizable.

## Theming

The search UI is rendered through shapes that you can override in your theme:

| Shape            | Template            | Purpose                                  |
|------------------|---------------------|------------------------------------------|
| `Search`         | `Search.cshtml`     | The overall search page.                 |
| `Search-Form`    | `Search-Form.cshtml`| The search input form.                   |
| `Search-Results` | `Search-Results.cshtml` | The list of results.                 |
| `Search-List`    | `Search-List.cshtml`| The container of the result items.       |

Copy these templates into your theme to customize the markup. Result highlighting is provided when the underlying search provider returns highlights.
