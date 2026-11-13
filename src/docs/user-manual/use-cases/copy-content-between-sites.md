---
title: Copy content between sites
description: Move content, media, content types and settings from one Orchard Core site to another with a deployment plan, by downloading and importing a file or by sending it straight to a remote site.
technical_manual:
  - reference/modules/Deployment/README.md
  - reference/modules/Deployment.Remote/README.md
  - reference/modules/Recipes/README.md
---

# Copy content between sites

You've prepared pages on a test site and want them on the live site, or you're starting a second site and want to reuse the first one's content. Orchard Core copies content with a **deployment plan**: a list of what to export (the **steps**), which you run to produce a file, or to send the content directly to another site.

## Who it's for and what you get

For **administrators**.

When it's done:

- a deployment plan you can run again whenever you need to copy the same things;
- the content (and, if you choose, its media files, content types and settings) on the other site;
- optionally, a direct link between the two sites, so one click sends the content over.

## Before you start

| You need | Where or who |
| --- | --- |
| The feature **Deployment** (`OrchardCore.Deployment`) on both sites. The Blog recipe turns it on. | An administrator, in **Tools > Features** |
| For a direct link: **Remote Deployment** (`OrchardCore.Deployment.Remote`) on both sites | An administrator, in **Tools > Features** |
| On the source site: *Manage deployment plans* and *Export Data*; for a direct link, *Manage remote instances* and *Export to remote instances* | The **Administrator** role has them |
| On the destination site: *Import Data*; for a direct link, *Manage remote clients* | The **Administrator** role has them |
| Both sites on compatible versions of Orchard Core, with the same features on | Your IT team |

!!! warning "Import overwrites"
    Importing content updates the items that already exist on the destination with the same identity, and creates the others. Try it on a test site first, and make sure the destination has a recent backup (ask IT).

## Create the deployment plan

1. On the **source** site, open **Tools > Deployments > Plans**.
2. Click **Add Deployment Plan**.
3. Type a **Name**, such as *Pages to live*.
4. Click **Create**.
5. On the plans list, click **Manage Steps** on your plan.
6. Click **Add Step**.
7. Click **Add** next to the step you want.
8. Fill in the step's settings.
9. Click **Create**.
10. Repeat steps 6 to 9 for each step.

To find a step, use the categories or the search box. The steps you'll use most for content:

| Step | What it exports |
| --- | --- |
| **Content** | All the content items of the content types you tick. |
| **Content Item** | One content item you choose. |
| **All Content** | Every content item of the site. |
| **Media** | The media files you choose, or all of them with **Include all media.** Content refers to its images by path; without this step the destination shows broken images unless it already has the files. |
| **Update Content Definitions** | The content types and parts you tick, so the destination has the same fields. Include it when the destination doesn't have your content types yet, or they changed. |
| **All Layers**, **Site Settings**, **All Templates**, **All Workflows**, **All Roles**, ... | Other parts of the site setup. |

Leave **Export As Setup recipe** cleared on the content steps; it's for developers who build a recipe that sets up new sites.

To add single items without editing the plan, use the content items list: in an item's **Actions** menu, or after ticking several items, choose **Add to deployment plan**, then choose the plan. See [Import and export](../administration/import-and-export.md).

## Copy with a file

### Export

1. On the plan's **Manage Steps** page, click **Execute**.
2. In **Available Targets**, click **Select** next to **File Download**.

Your browser downloads a `.zip` file with what the steps export, including the media files if the plan has a **Media** step.

### Import

1. On the **destination** site, open **Tools > Deployments > Package Import**.
2. Under **Select a file**, choose the `.zip` file (a `.json` recipe file works too).
3. Click **Import**.

*Deployment package imported.* means it worked. If something's wrong, the message lists the errors, such as a content type that doesn't exist on the destination. Fix the plan (for example add **Update Content Definitions**) and try again.

**Tools > Deployments > JSON Import** does the same with JSON you paste in the **JSON** box, which is handy for a small recipe someone sent you.

## Copy directly to another site

Set up the link once; then each run sends the content in one click. The destination must be reachable from the source over the internet or your network.

### On the destination site: allow a client

1. Open **Tools > Deployments > Remote Clients**.
2. Click **Add Remote Client**.
3. Type a **Client Name**, such as *staging*.
4. Type an **Api Key**: a long, random password.
5. Click **Create**.

Both fields are required. Keep the name and key for the next step, and share them safely.

### On the source site: add the destination

1. Open **Tools > Deployments > Remote Instances**.
2. Click **Add Remote Instance**.
3. Fill in:

    | Field | What to enter |
    | --- | --- |
    | **Name** | Required. A name for the destination, such as *Live site*. It shows as a target when you run a plan. |
    | **Url** | Required. The destination's address followed by `/OrchardCore.Deployment.Remote/ImportRemoteInstance/Import`, such as `https://www.example.com/OrchardCore.Deployment.Remote/ImportRemoteInstance/Import`. For a tenant with a URL prefix, put the prefix before it. |
    | **Client Name** | Required. The client name you created on the destination. |
    | **Api Key** | Required. The same key. |

4. Click **Create**.

### Send the content

1. On the plan's **Manage Steps** page, click **Execute**.
2. In **Available Targets**, click **Select** next to your remote instance (*Sends the deployment plan to a remote instance.*).

*Deployment executed successfully.* means the destination imported it. Otherwise, the message shows the error the destination returned.

See [Remote deployment](../administration/remote-deployment.md).

## Send single items from the content list

With the **Export Content To Deployment Target** feature on, the content items list has an **Export to deployment target** action, for an item or for the items you tick. It sends them straight to a target you choose, such as **File Download** or a remote instance, through the plan chosen in **Settings > Deployment Targets**. See [Import and export](../administration/import-and-export.md).

## Check that it works

1. On the destination, open **Content > Content Items** and find the copied items.
2. Open one on the site, and check its images and links.
3. Check the publication state: items arrive published or as drafts, as they were on the source.

## Tips

- Copy in small, named plans (*Pages*, *Blog posts*, *Site settings*) rather than one big plan, so you know what each run changes.
- Users are exported only by the **All Users** step, which includes their passwords in scrambled (hashed) form, so they can sign in on the other site; think twice before copying accounts between sites.
- The **Secrets** step exports only the names and details of the secrets, unless you choose an **Encryption key** in it: then the values are exported encrypted, and the destination needs the same key to read them. Without a key, set the values again on the destination. See [Secrets](../settings/secrets.md).
- To copy a whole site to a new one, an IT team can also use the export as the site's setup recipe; see the Technical Manual.
