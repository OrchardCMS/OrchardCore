---
title: Secrets
description: Keep passwords, API keys, connection strings, keys and certificates in one safe place, pick them from settings, move settings credentials into secrets, and move secrets between stores.
technical_manual:
  - reference/modules/Secrets/README.md
  - reference/modules/KeyVault.Azure/README.md
---

# Secrets

A **secret** is a piece of sensitive information the site needs to talk to other services: a password, an API key, a connection string, a signing key or a certificate. Instead of typing the same password into several settings screens, you store it once as a secret, give it a name such as `Smtp.Password`, and pick that name in the settings that need it. To change the password later, you change the secret in one place.

Secret values are encrypted, and once saved they are never shown again, not even to administrators.

| | |
| --- | --- |
| **Menu** | **Tools > Security > Secrets**, **Tools > Security > Migrate Secrets**, **Tools > Security > Secret Stores** |
| **Permission** | *View secrets* (to see the list) and *Manage secrets* (to add, change, move and delete). The Administrator role has both by default. **Migrate Secrets** and **Secret Stores** need *Manage secrets*. |
| **Feature** | **Secrets** (`OrchardCore.Secrets`); optionally **Secrets - Azure Key Vault** (`OrchardCore.Secrets.AzureKeyVault`) |


## Where secrets are kept

Secrets live in a **store**:

- **Database**: the default store, in the site's own database, encrypted.
- **AzureKeyVault**: an Azure Key Vault, a secure vault service from Microsoft, when the **Secrets - Azure Key Vault** feature is on. Your developer or IT team connects the vault to the site in its configuration; there is no screen for that. Once connected, you can pick it as the store of a secret.

## Add a secret

1. Open **Tools > Security > Secrets** and click **Add Secret**.
2. In **Select Secret Type**, click **Create** next to the type you need (see the table below).
3. Type a **Name**. Pick a clear name, such as `Smtp.Password` or `Payment.ApiKey`. It can't be changed later.
4. If more than one store is available, pick the **Store**.
5. Fill in the fields of the type.
6. Optionally, type a **Description** and an **Expiration date**.
7. Click **Save**.

You see *Secret created successfully.* If the name is taken, you see *A secret with this name already exists.*

| Type | Use it for | Fields |
| --- | --- | --- |
| **Text Secret** | A password, an API key, a token or a connection string. | **Value** (required). |
| **RSA Key** | A key pair to sign or encrypt data, for example for the OpenID Connect server. | **Key size**: **2048 bits**, **3072 bits** or **4096 bits**. The site generates a new key pair for you. |
| **X.509 Certificate** | A certificate, for example to sign tokens. | **Source**: **Certificate store** (a certificate installed on the server: pick the **Store location**, **Current user** or **Local machine**, the **Store name**, and type its **Thumbprint**), or **PEM-encoded certificate** (paste the **Certificate**, and optionally its **Private key**). |

| Common field | What it does |
| --- | --- |
| **Name** | Required. The unique name the settings use to find the secret. |
| **Store** | Where the secret is saved. Shown as a list only when there is more than one store. |
| **Description** | Optional. A note about what the secret is for. |
| **Expiration date** | Optional. A reminder only: the secret keeps working after this date. The list and the dashboard warn you before and after it passes. |

## Use a secret in settings

Settings screens that need a credential let you choose where the value is kept:

1. Pick **Use a secret** in the list above the box.
2. Pick the secret in the list.
3. If the secret doesn't exist yet, click **New secret**: a new browser tab opens on the secret editor. Create the secret, come back, and click the reload button next to the list to see it.
4. Click **Save** on the settings screen.

Once you save with a secret selected, the value that was stored with the settings is removed. To go back, pick **User-specified value** and type the value.

These settings can use secrets:

| Settings | Credential |
| --- | --- |
| [Email](email.md): SMTP and Azure Communication Services | **Password**, **Connection String** |
| [SMS](sms.md): Twilio and Azure Communication Services | **Auth Token**, **Connection String** |
| [External sign-in](../users/external-sign-in.md): GitHub, Google, Microsoft, OpenID Connect client | The app or client secret |
| [Analytics and social](analytics-and-social.md): Meta App, X (Twitter) | **Secret Key**, **API secret key**, **Access Token Secret** |
| [Site search](../search/site-search.md): Azure AI Search | **Api Key** |

With the **OpenID Connect Secrets** feature on, the OpenID Connect server can also take its signing and encryption keys from secrets. See [OpenID Connect server](../users/openid-server.md).

If a setting points to a secret while the **Secrets** feature is off, the settings screen warns you and asks you to turn the feature on or type a value.

## Move credentials from settings into secrets

If you set up email, SMS or sign-in providers before using secrets, their credentials are stored with their settings. **Migrate Secrets** moves them into secrets in one step.

1. Open **Tools > Security > Migrate Secrets**. The screen lists every credential kept in settings, with the feature it belongs to.
2. Check the credentials to move.
3. Next to each one, check or change the **Secret name**.
4. Pick the **Destination store**.
5. Click **Move to Secrets**.

The settings then point to the new secrets and no longer keep the values. If a secret with the same name already exists, or a value can't be read, that credential is left unchanged and the screen tells you why. When nothing is left to move, the screen says *Nothing to migrate!*

## Find, edit and delete secrets

**Tools > Security > Secrets** lists the secrets with their type, store and expiration. Use **Search**, the expiration filter (**All**, **Expired**, **Expires within 30 days**, **Never expires**) and the type filter to narrow the list.

- **Edit**: change the value, description or expiration date. Leave **Value** empty to keep the current value; stored values are never displayed. The **Name** can't be changed.
- **Move**: move the secret to another store (see below).
- **Delete**: removes the secret after you confirm. To delete several, check them and pick **Delete** in the **Actions** menu.

!!! warning
    Before you delete a secret, make sure no setting uses it. Settings that point to a deleted secret stop working, for example the site can no longer send email.

## Get warned before a secret expires

When a secret has an **Expiration date**, the admin dashboard shows a warning to people who can view secrets, such as *One secret expires within 30 days.* or *One secret has expired.*, with a **Review secrets** link. The list marks these secrets too. Renew the credential with the service that issued it, then edit the secret with the new value and date.

## Move secrets to another store

Use **Tools > Security > Secret Stores** to move secrets between stores, for example from the database to Azure Key Vault before you stop using the database store.

1. Open **Tools > Security > Secret Stores**.
2. Pick the **Source store**. The screen shows how many active secrets it holds.
3. Under **Move all active secrets from this store to**, pick the **Destination store**.
4. Check *I confirm this move, and the deletion from the source store once the destination is verified.*
5. Click **Move**.

The **Results** list shows **Moved** or **Failed** for each secret. A move checks the destination before deleting the source, keeps each secret's name, and never overwrites a secret that already exists in the destination. To move a single secret, use **Move** in its row on the **Secrets** list.

If only one store is available, the screen says *There is no other writable store to move secrets to. Enable another secret store, such as Azure Key Vault, first.*

!!! note
    Moving deletes the secret from the source with that store's normal delete. Copies in database backups or in the vault's deleted-items retention are not erased. If old copies must stop working, change the credential with the service that issued it.
