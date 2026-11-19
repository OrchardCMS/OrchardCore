---
title: Data pipeline steps
description: Every step you can add to a data pipeline, from sources and transforms to files and destinations, with its settings.
technical_manual:
  - reference/modules/DataPipelines/README.md
  - reference/modules/DataSources/README.md
---

# Data pipeline steps

This page lists every step of the data pipeline designer, by category, with its settings. To build a pipeline, connect its steps and run it, see [Data pipelines](data-pipelines.md).

| | |
| --- | --- |
| **Menu** | **Tools > Data Pipelines > Pipelines**, then the **Steps** toolbox of the designer |
| **Permission** | *Manage data pipelines* |
| **Feature** | **Data Pipelines** (`OrchardCore.DataPipelines`), **Data Pipelines - FTP** (`OrchardCore.DataPipelines.Ftp`), **Data Pipelines - SFTP** (`OrchardCore.DataPipelines.Sftp`) |

Fields marked required must be filled in before the pipeline can be published. In file names, folders and email texts, you can use placeholders: `{PipelineName}`, `{PipelineId}`, `{RunId}`, `{Date:format}` (when the run started, in the site time zone, such as `{Date:yyyy-MM-dd}`) and `{Parameter:name}` (a value a workflow passes to the run). **Send by email** also has `{FileNames}`.

## Sources

| Step | What it does and its key settings |
| --- | --- |
| **Read a data source** | Reads a data set, such as the content items of a content type, the users or a saved query (see [Data sources](data-sources.md)). **Data set** (required, **Change** picks another one and clears the fields and filters), **Fields to read** (none reads every field), **Filters** (**Add a filter**: a **Field**, a **Comparison** and a value; rows must match every filter), **Maximum number of rows** (0 reads every row), **Read only new rows**, **Parameters** (for a saved query, one `name=value` per line). |
| **Read a media file** | Reads the rows of a CSV, Excel, JSON or JSON Lines file of the media library. **File** (required, its path), **Format** (by default **From the extension of the file**), **The first row holds the names of the columns (CSV, Excel)**, **CSV delimiter**, **Worksheet** (empty for the first). The publisher must be allowed to manage the media of the file's folder. |

## Transforms

| Step | What it does and its key settings |
| --- | --- |
| **Filter rows** | Splits rows by a **Condition** (required), a formula such as `[Amount] >= 100`. True rows go to the **Matched** output, the others to **Unmatched**, which you can leave unconnected. |
| **Add calculated fields** | Adds fields computed by formulas. **Add a field**, then give each a **Name** and a **Formula**, such as `[Price] * [Quantity]`. A field with the name of an existing field replaces its value. |
| **Convert time zone** | Converts dates and times. **Convert**: **From UTC to the time zone** or **From the time zone to UTC**. **Time zone**: **The time zone of the site**, **The time zone of the user the pipeline runs for** or **A time zone** you choose. **Fields** (none converts every date and time field). |
| **Select fields** | Keeps, orders, renames and converts fields. For each field: **Field**, **New name** and **Type** (**Same type**, **Text**, **Integer**, **Decimal**, **Boolean**, **Date**, **Date and time**). Without any field, rows pass unchanged. |
| **Sort rows** | **Sort by** (required) one or more fields, each **Ascending** or **Descending**, the most significant first. |
| **Group and summarize** | **Group by** fields (none summarizes every row into one) and **Values** (required): each with a **Name**, a **Function** (**Count**, **Count distinct**, **Sum**, **Average**, **Minimum**, **Maximum**, **Median**) and a **Field**. |
| **Join** | Combines the rows of a **Left** and a **Right** input. **Rows to keep** (inner, left, right or full join), **Match on** (required, pairs of left and right fields that must be equal), **Ignore the case of text**, **Prefix of right fields** (default `Right.`). Connect the smaller data set to the right. |
| **Combine rows** | Appends the rows of every step connected to its input, one after the other. Fields are matched by name. No settings. |
| **Remove duplicates** | Keeps the first row of each set of equal values. **Compare** (none compares every field). |
| **Keep first rows** | **Rows to keep** (required, 100 by default) and **Rows to skip** first. |

The **Filter rows** and **Add calculated fields** editors list the input fields (click one to insert it) and **Functions and operators**, a searchable list of the functions you can use. See [Data sources](data-sources.md) for the formula rules.

## Files

| Step | What it does and its key settings |
| --- | --- |
| **Create a file** | Writes the rows to a file. **Format** (required): **CSV (comma separated values) (.csv)**, **Excel workbook (.xlsx)**, **JSON (.json)**, **JSON Lines (.jsonl)**. **File name** without the extension (default `{PipelineName}-{Date:yyyyMMdd-HHmmss}`). **Start with a row of column names**, **Name the columns with the labels of the fields**, **CSV delimiter** (type `tab` for tabs), **Indent the JSON**. For a workbook, connect several steps to the input to write each one to its own worksheet, and name them in **Worksheet names**, one per line. |
| **Zip files** | Compresses every file it receives into one archive. **Archive name** without `.zip`. |

## Destinations

| Step | What it does and its key settings |
| --- | --- |
| **Save to the media library** | Saves the files to a **Folder** of the media library (empty for the root). **Replace a file with the same name** (otherwise a number is added to the name). The publisher must be allowed to manage the media of the folder. The media library accepts Excel and CSV files by default but not JSON or zip files; the editor warns you, and accepting more extensions is a job for IT. See [Media library](../media/media-library.md). |
| **Share a download link** | Emails users of the site a secure link to download each file. **Recipients** (required, user names or emails of site users, separated by commas), **Days the link works** (required, 1 to 365, default 7), **Maximum downloads** (0 for no limit), **Email subject**, **Message**. The link works only for its recipients, once signed in. See [Shared links](shared-links.md). |
| **Send by email** | Sends one email with every file attached, through the site's email settings (see [Email](../settings/email.md)). **To** (required), **Cc**, **Bcc** (separated by commas or semicolons), **Subject**, **Body**, **The body is HTML**, **Send the email when there are no files to attach**, **Maximum size (MB)** (default 10; larger attachments fail the step). |
| **Send to a web API** | Sends each file in its own request. **URL** (required), **Method** (POST or PUT), **Body** (**A form with the file (multipart/form-data)** with its **Form field**, or **The content of the file**), **Headers**, **Authentication** (**None**, **Basic (user name and password)**, **Bearer token**, **API key in a header**), **Timeout (seconds)** (default 100). A response other than 2xx fails the step. |
| **Upload to an FTP server** | Needs **Data Pipelines - FTP**. **Host** (required), **Port** (21, or 990 for implicit TLS), **Encryption** (**Explicit TLS (FTPES)**, **Implicit TLS (FTPS)**, **None (plain FTP)**), **Validate the certificate of the server**, **User name** (required), **Password**, **Remote folder** (created if missing), **Replace existing files**, **Timeout (seconds)** (default 30). |
| **Upload to an SFTP server** | Needs **Data Pipelines - SFTP**. **Host** (required), **Port** (default 22), **User name** (required), **Password**, **Private key** and its **Passphrase**, **Host key fingerprint**, **Remote folder** (created if missing), **Replace existing files**, **Timeout (seconds)** (default 30). |
| **Save content items** | Creates or updates content items of a **Content type** (required) from rows. **Rows**: **Update the item a row matches, or create one**, **Create items; skip the rows that match one**, or **Update the items rows match; reject the others**. **Match on**: **Content item id** or **Display text**, and the field of the rows to compare. **Values** (required): each **Part or field** of the items and the **Field of the rows** that sets it. **Publish the items** (otherwise drafts), **Items per save** (default 100). Rows that can't be saved go to the **Rejected** output with the reason in an **Error** field. The publisher must be allowed to edit, and publish, items of this type. |

## Passwords, keys and tokens

The password, private key, passphrase, token and API key fields of steps are stored encrypted and are never shown again. The field stays empty: leave it empty to keep the current value, type a new one to replace it, or check **Remove the password** (or token, key...) to clear it. These values are not exported with the pipeline, so enter them again after importing a pipeline on another site. They are kept by the pipeline itself, not in the site's [Secrets](../settings/secrets.md).

## FTP and SFTP security

- **None (plain FTP)** isn't encrypted: the user name, the password and the files travel in clear text. Prefer an encrypted option.
- Clear **Validate the certificate of the server** only for a server with a self-signed certificate on a trusted network.
- For SFTP, fill in the **Host key fingerprint** (the SHA-256 fingerprint of the server key) so the step refuses to connect to a server that presents another key. When it is empty, the step warns that the server isn't verified, and the run's **Log** shows the fingerprint the server presented, which you can copy into the field after checking it with the server's owner.
