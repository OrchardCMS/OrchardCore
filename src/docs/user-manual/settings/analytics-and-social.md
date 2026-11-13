---
title: Analytics and social
description: Connect Google Analytics, Google Tag Manager, a Meta app and the Meta Pixel, X (Twitter), and add Meta social plugin widgets to your pages.
technical_manual:
  - reference/modules/Google/README.md
  - reference/modules/Facebook/README.md
  - reference/modules/X/README.md
---

# Analytics and social

These screens connect your site to outside services: Google Analytics and Google Tag Manager to measure visits, the Meta Pixel to measure ads, a Meta app for Meta's social plugins, and X (Twitter) to post from a workflow. Each service gives you an identifier or keys from its own website; you paste them here.

| Screen | Menu | Permission | Feature |
| --- | --- | --- | --- |
| Google Analytics | **Settings > Integrations > Google Analytics** | *Manage Google Analytics settings* | **Google Analytics** (`OrchardCore.Google.Analytics`) |
| Google Tag Manager | **Settings > Integrations > Google Tag Manager** | *Manage Google Tag Manager settings* | **Google Tag Manager** (`OrchardCore.Google.TagManager`) |
| Meta app | **Settings > Integrations > Meta App** | *View and edit the Facebook app.* | **Meta Core Components** (`OrchardCore.Facebook`), turned on with the other Meta features |
| Meta Pixel | **Settings > Integrations > Meta Pixel** | *Manage Facebook Pixel settings.* and *View and edit the Facebook app.* (see the note below) | **Meta Pixel** (`OrchardCore.Facebook.Pixel`) |
| Meta widgets | **Design > Widgets** | The permissions of [widgets](../design/widgets-and-layers.md) | **Meta Social Plugins Widgets** (`OrchardCore.Facebook.Widgets`) |
| X (Twitter) | **Settings > Integrations > X (Twitter)** | *Manage X (Twitter) settings* | **X (Twitter) Integration** (`OrchardCore.Twitter`) |

The Administrator role has all these permissions by default. Signing in with a Google, Meta or X account is a different feature; see [External sign-in](../users/external-sign-in.md).


## Track visits with Google Analytics

1. In your Google Analytics account, copy the ID of your property's web data stream (it looks like `G-XXXXXXXXXX`).
2. Open **Settings > Integrations > Google Analytics**.
3. Paste it in **Tracking ID**.
4. Click **Save**.

The Google Analytics script is added to every page of the site (not to the admin). **Tracking ID** is required, so it can't be saved empty: to stop tracking, turn off the **Google Analytics** feature in **Tools > Features**.

## Add Google Tag Manager

Google Tag Manager loads your marketing and analytics tags from one place, managed in Google's own interface.

1. In Google Tag Manager, copy the ID of your container (it looks like `GTM-XXXXXXX`).
2. Open **Settings > Integrations > Google Tag Manager**.
3. Paste it in **Container ID**.
4. Click **Save**.

The Tag Manager script is added to every page of the site (not to the admin). Use either Google Analytics or a Google Analytics tag in Tag Manager, not both, or visits are counted twice.

!!! note "Visitor consent"
    If your site asks visitors to accept tracking cookies, the Google Analytics, Google Tag Manager and Meta Pixel scripts are added only for visitors who accepted.

## Connect a Meta app

A Meta app connects your site to Meta's developer platform. The [Meta widgets](#add-meta-social-plugin-widgets) and Meta sign-in need it.

1. On the Meta for Developers website, create an app and copy its App ID and App Secret.
2. Open **Settings > Integrations > Meta App**.
3. Fill in the fields (see the table below).
4. Click **Save**. The site reloads.

| Field | What it does |
| --- | --- |
| **Application Identifier** | The App ID of your Meta app. |
| **Secret Key** | The App Secret of your Meta app. It is stored encrypted and never shown again: leave it blank to keep the current one. You can keep it in a [secret](secrets.md) instead. |
| **Javascript SDK Version** | The version of Meta's Graph API and plugins to use. The default is `v3.2`. |
| **Javascript Sdk js** | The Meta script file to load. The default is `sdk.js`. For the chat plugin, use `sdk/xfbml.customerchat.js`. |
| **Init on every page** | Loads and starts Meta's script on every page of the site. When it is cleared, pages that contain a Meta widget load the script themselves. |
| **Parameters for the FB.init() call** | Optional extra options for Meta's script. The App ID and version are added for you. |

## Measure ads with the Meta Pixel

The Meta Pixel tells Meta which visitors came from your ads and what they did on your site.

1. In Meta Events Manager, copy your Pixel ID.
2. Open **Settings > Integrations > Meta Pixel**.
3. Paste it in **Pixel Identifier**.
4. Optionally, fill in the **Conversions API** fields (see below).
5. Click **Save**.

The pixel is added to every page of the site (not to the admin).

| Field | What it does |
| --- | --- |
| **Pixel Identifier** | The Pixel ID from Meta. |
| **Conversions API Access Token** | Optional. Lets [workflows](../forms-and-automation/workflows.md) send events, such as a purchase or a lead, straight from the site to Meta, in addition to the pixel in the browser. Generate the token in the Conversions API settings of Events Manager. |
| **Test Event Code** | Optional. Makes the events appear in the **Test Events** tab of Events Manager instead of counting as real data. Remove it before going live. |

With the Conversions API set up, the **Meta Conversions API Event** workflow task sends an event: pick or type the **Event Name** (such as *Purchase* or *Lead*), and optionally the **Event ID**, the **Event Source URL** and the **Action Source**.

If the screen says *An error occurred while decrypting a setting. Please apply and save.*, type the access token again and click **Save**.

!!! note "Permissions for the Meta Pixel screen"
    The **Meta Pixel** menu item needs *Manage Facebook Pixel settings.*, but the form on that screen is shown only to roles that also have *View and edit the Facebook app.*

## Add Meta social plugin widgets

With the **Meta Social Plugins Widgets** feature on, editors can add Meta's social plugins to pages as [widgets](../design/widgets-and-layers.md). The feature adds these widget types:

| Widget | What visitors see |
| --- | --- |
| **Facebook Like** | A Like button. |
| **Facebook Share** | A Share button. |
| **Facebook Save** | A Save button. |
| **Facebook Quote** | A button to share a quote selected on the page. |
| **Facebook Comments** | A comments box. |
| **Facebook Continue With** | A "Continue with Facebook" button. |
| **Facebook Chat** | Meta's chat plugin. |

1. Connect a [Meta app](#connect-a-meta-app).
2. Add the widget to a zone, like any widget; see [Widgets and layers](../design/widgets-and-layers.md).
3. In the widget's **Body**, adjust the plugin's template if you need to. It is written in Liquid and starts with Meta's default code for that plugin.
4. Click **Publish**.

!!! warning "Check the default code"
    The **Facebook Chat** widget contains `<PAGE_ID>`: replace it with the ID of your Facebook page. The **Facebook Save** widget's `data-uri` starts with another website's address: replace it with your own site's address. A site builder can change the default code of each widget type in **Design > Content Definition > Content Types**, in the settings of the type's Facebook plugin part.

## Post to X (Twitter) from a workflow

The X (Twitter) integration lets a [workflow](../forms-and-automation/workflows.md) post on your X account, for example when an article is published.

1. In the X developer portal, open your app's **Keys and tokens** tab, and copy the API key and secret, and an access token and its secret with write access.
2. Open **Settings > Integrations > X (Twitter)**.
3. Fill in **API key**, **API secret key**, **Access Token** and **Access Token Secret**.
4. Click **Save**. The site reloads.

The two secret fields are stored encrypted and never shown again: leave them blank to keep the current values. You can keep them in [secrets](secrets.md) instead.

In a workflow, add the **Update X (Twitter) Status** task (*Create a post*) and type the text of the post in **Template for updating X (Twitter) status**. It supports Liquid, so the post can include the title or the address of the content item.
