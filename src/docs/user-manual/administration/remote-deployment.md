---
title: Remote deployment
description: Send deployment plans straight from one Orchard Core site to another, with a remote client on the receiving site and a remote instance on the sending site.
technical_manual:
  - reference/modules/Deployment.Remote/README.md
  - reference/modules/Deployment/README.md
---

# Remote deployment

Remote deployment sends a deployment plan from one site straight to another, without downloading and uploading a file. For example, editors prepare content on a staging site and an administrator sends it to the live site in one click. You set it up once on both sites:

- On the site that **receives** the data, you add a **remote client**: a name and a secret key that are allowed to send packages.
- On the site that **sends** the data, you add a **remote instance**: the address of the receiving site, with the same name and key.

| | |
| --- | --- |
| **Menu** | **Tools > Deployments > Remote Clients** (receiving site); **Tools > Deployments > Remote Instances** (sending site) |
| **Permission** | *Manage remote clients* for remote clients. *Manage remote instances* for remote instances. *Export Data* to send a plan. Administrators have them by default. |
| **Feature** | **Remote Deployment** (`OrchardCore.Deployment.Remote`), on both sites. It turns on **Deployment** (`OrchardCore.Deployment`). |


!!! warning "The receiving site imports at once"
    The receiving site imports every package sent with a valid client name and key, with no confirmation, just like [Package Import](import-and-export.md#import-a-package). Use a long, random key, use a different key for each sending site, use `https://` addresses, and delete clients you no longer use.

## Allow a site to receive deployments

On the receiving site:

1. Open **Tools > Deployments > Remote Clients** and click **Add Remote Client**.
2. Type a **Client Name**, for example `staging`.
3. Type an **Api Key**: a long, random secret. Keep a copy; you need it on the sending site.
4. Click **Create**.

| Field | What it does |
| --- | --- |
| **Client Name** | Required. The name the sending site gives when it sends a package. |
| **Api Key** | Required. The secret the sending site must give with the name. It is stored encrypted. |

The receiving address of this site is its address followed by `/OrchardCore.Deployment.Remote/ImportRemoteInstance/Import`, for example `https://www.example.com/OrchardCore.Deployment.Remote/ImportRemoteInstance/Import`. For a tenant with a URL prefix, put the prefix after the domain, such as `https://www.example.com/brand/OrchardCore.Deployment.Remote/ImportRemoteInstance/Import`.

Also turn on, in the receiving site, every feature the packages need, see [Features](../getting-started/features.md).

## Connect the sending site

On the sending site:

1. Open **Tools > Deployments > Remote Instances** and click **Add Remote Instance**.
2. Type a **Name**, for example `Live site`.
3. Type the receiving address in **Url** (see above).
4. Type the **Client Name** and **Api Key** of the remote client you created on the receiving site.
5. Click **Create**.

| Field | What it does |
| --- | --- |
| **Name** | Required. The name shown in the list of targets when you run a plan. |
| **Url** | Required. The full receiving address of the other site, starting with `https://` or `http://`. |
| **Client Name** | Required. Exactly the client name on the receiving site. |
| **Api Key** | Required. Exactly the key on the receiving site. |

## Send a plan to another site

1. On the sending site, open **Tools > Deployments > Plans**.
2. Click **Manage Steps** in the plan's row.
3. Click **Execute**.
4. In **Available Targets**, click **Select** under the remote instance's name.

You see *Deployment executed successfully.* when the receiving site accepted the package. A failure during the import itself isn't reported back: the sending site still shows this message, so check the result on the receiving site. When the package is refused, you see *An error occurred while sending the deployment to the remote instance:* followed by the reason, for example:

- **Bad Request (400)**: the client name or key doesn't match, or the receiving site rejected the file. Check both sites.
- **Not Found (404)**: the **Url** is wrong, the tenant prefix is missing, or **Remote Deployment** isn't turned on in the receiving site.

The receiving site shows import errors in its logs; ask your IT team to look.

You can also send single content items from the content list with **Export to Deployment Target**, see [Export content items from the content list](import-and-export.md#export-content-items-from-the-content-list).

## Manage remote clients and instances

- In **Remote Clients** and **Remote Instances**, click a name or **Edit** to change it, then click **Save**. The **Api Key** box keeps the stored key, hidden as dots; type over it to change it.
- Click **Delete** in a row to remove it. A deleted client can no longer send packages; a deleted instance disappears from the targets.
- Type in **Search** to filter the list. To delete several, tick them and choose **Delete** in the **Actions** menu above the list.

When you change a key, change it on both sites.

!!! example "See it in practice"
    [Copy content between sites](../use-cases/copy-content-between-sites.md) walks through this step by step.
