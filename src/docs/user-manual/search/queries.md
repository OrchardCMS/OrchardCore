---
title: Queries
description: Save SQL, Lucene and Elasticsearch queries by name, try them on the Run Query screens, and reuse them in templates, the API and deployment plans.
technical_manual:
  - reference/modules/Queries/README.md
  - reference/modules/SQLIndexing/README.md
  - reference/modules/Lucene/README.md
  - reference/modules/Elasticsearch/README.md
---

# Queries

A query is a saved question to the site's data, such as "the three latest blog posts". You give it a name, and templates, the API, GraphQL and deployment plans run it by that name. A query can read the site's database with SQL, or a Lucene or Elasticsearch search index. Writing the query itself needs some technical knowledge; this page shows the screens where queries are created, tried and managed.

| | |
| --- | --- |
| **Menu** | **Search > Queries > All Queries**; **Search > Queries > Run SQL Query**; **Search > Queries > Run Lucene Query**; **Search > Queries > Run Elasticsearch Query** |
| **Permission** | *Manage queries* for **All Queries**. *Manage SQL Queries* for SQL queries and **Run SQL Query**. *Manage Lucene Indexes* for **Run Lucene Query**. *Manage Elasticsearch Indexes* for **Run Elasticsearch Query**. Administrators and Editors have *Manage queries* and *Manage SQL Queries* by default. |
| **Feature** | **Queries** (`OrchardCore.Queries`), plus **SQL Queries** (`OrchardCore.Queries.Sql`), **Lucene** (`OrchardCore.Lucene`) or **Elasticsearch** (`OrchardCore.Elasticsearch`) for each kind of query |


!!! warning "SQL queries can read everything"
    A SQL query can read any table of the site's database. *Manage SQL Queries* is marked as a security-critical permission: give it only to people you trust with all the site's data.

**Queries Core Services** (`OrchardCore.Queries.Core`) is turned on for you when a feature needs it.

## Find a query

Open **Search > Queries > All Queries**. Each row shows the query's name and its **Source** (**SQL query**, **Lucene query** or **Elasticsearch query**), with these buttons:

- **Run** opens the matching Run Query screen with the query filled in, so you can try it.
- **Edit** opens the query.
- **Delete** removes it after you confirm.

Type in **Search** to filter the list by name. To delete several queries, tick them and choose **Delete** in the **Actions** menu above the list.

## Add a query

1. Open **Search > Queries > All Queries** and click **Add Query**.
2. In **Available Query Types**, click **Add** under **SQL**, **Lucene** or **Elasticsearch**. If the box says *You need to enable a feature which provides a query source.*, click **Go to Features** and turn on **SQL Queries**, **Lucene** or **Elasticsearch**.
3. Type the **Name**.
4. Fill in the fields of the query type, described below.
5. Click **Save**.

You see *Query created successfully.*

Every query has these fields:

| Field | What it does |
| --- | --- |
| **Name** | Required. The technical name templates and the API use to run the query, for example `RecentBlogPosts`. Use letters, digits and no spaces: a name with other characters shows *Name contains illegal characters*. It must be unique. |
| **Schema** | Optional. Describes the results for the GraphQL API, as JSON. Leave it empty unless a developer gives you one. **See documentation** opens the Technical Manual. |

### SQL query

| Field | What it does |
| --- | --- |
| **Return Documents** | When ticked, the query returns the matching content items (or other documents) instead of raw rows. The query must then return a list of document ids. |
| **Query** | Required. The SQL query, written in standard SQL and converted for the site's database. Table names get the site's table prefix automatically. The query is checked when you save, and mistakes are shown under the box. |

For example, the **Blog** recipe adds a **RecentBlogPosts** query with **Return Documents** ticked:

```sql
SELECT DocumentId FROM ContentItemIndex WHERE ContentType='BlogPost' ORDER BY CreatedUtc DESC LIMIT @limit:3
```

`@limit:3` is a parameter: whoever runs the query can pass a `limit` value, and `3` is used when they don't. The tables you can query are listed in the Technical Manual.

!!! warning "Never put visitor input in the query text"
    SQL queries are Liquid templates. The editor reminds you not to use Liquid output such as `{{ input }}` for values that come from users, because that can let someone run their own SQL. Use parameters like `@name` instead. When it finds Liquid output in the query, a warning appears after you save; the query is still saved.

### Lucene or Elasticsearch query

| Field | What it does |
| --- | --- |
| **Index** | Required. The Lucene or Elasticsearch index to search, see [Site search](site-search.md). |
| **Return Content Items** | When ticked, the query returns the matching content items. Otherwise it returns the values stored in the index. |
| **Query** | Required. The search, written as a JSON query in the Elasticsearch query format, for both Lucene and Elasticsearch. The hint links to the Elasticsearch documentation. |

## Edit or delete a query

- To change a query, click **Edit** in its row, change it and click **Save**. You see *Query updated successfully.*
- To delete it, click **Delete** in its row and confirm. Templates or pages that use the query stop getting results, so check with the site's developer first.

## Try a SQL query

**Run SQL Query** lets you try a SQL query on the live database before you save it.

1. Open **Search > Queries > Run SQL Query**.
2. Type the query in **Template**. To start from an example, click the **click here** link in the hint, which fills in a query for all content items.
3. If the query has parameters, type their values in **Parameters** as JSON, for example `{"limit": 5}`.
4. Click **Query**.

The screen shows the **Final SQL Query**, the query as the database runs it with the table prefix and the database's own syntax, the time it took (*Query executed in ... ms*) and the rows it returned. Click **Create** to save the query as a new SQL query. If the query has a mistake, the error is shown on the screen instead of results.

## Try a Lucene or Elasticsearch query

1. Open **Search > Queries > Run Lucene Query** or **Run Elasticsearch Query**. You can also choose **Query Index** in an index's **Actions** menu in **Search > Indexes**.
2. Pick the **Index**.
3. Type the **Query**. The **Match All query** link in the hint fills in a query that returns every document of the index.
4. Type values for any parameters in **Parameters**, as JSON.
5. Click **Run Query**.

The screen shows *Found ... result(s) in ... ms* and a table of the results. For Lucene, the table shows the values stored in the index; for Elasticsearch, the stored source data or field values. Click **Save Query** to save it as a new query.

## Use a query

Developers and template designers run saved queries by name from Liquid and Razor templates (see [Templates and placements](../design/templates-and-placements.md)), from GraphQL (with a **Schema**) and from the queries API. You can also export the content items a query returns with the **Queried Content Items** deployment step, see [Import and export](../administration/import-and-export.md).

To let an application or a role run queries through the API, give the role one of these permissions in **Access Control > Roles**, see [Roles](../users/roles.md):

- *Execute Api - All queries* for every query.
- *Execute Api - Name*, where *Name* is the name of the query. There is one for each query.

Roles with *Query Lucene Api* or *Query Elasticsearch Api* can also send searches to the Lucene or Elasticsearch API. Editors have these two by default.

## Copy queries to another site

Add the **All Queries** step to a deployment plan to export every query, see [Import and export](../administration/import-and-export.md).
