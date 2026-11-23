---
title: Add a contact form
description: Build a contact page with form widgets, and a workflow that emails you each message, checks required fields and thanks the visitor.
technical_manual:
  - reference/modules/Forms/README.md
  - reference/modules/Workflows/README.md
  - reference/modules/Email/README.md
---

# Add a contact form

Orchard Core builds forms from **widgets** (a form, text boxes, a button) placed on a page, and handles what happens after a visitor submits the form with a **workflow**. This walkthrough builds a *Contact us* page whose form emails each message to you, then shows the visitor a thank-you message. This page is the step-by-step walkthrough; [Forms](../forms-and-automation/forms.md) is the full reference for every form widget and option.

## Who it's for and what you get

For **site builders** and **administrators**. You need no code: the form, the email and the checks are all set up in the admin.

When it's done:

- a **Contact** page with **Name**, **Email** and **Message** boxes and a **Send** button;
- each submission emailed to the address you choose, with the visitor's details;
- a thank-you message after sending;
- optionally, a check that every box was filled in, with an error message when one is empty.

## Before you start

| You need | Where or who |
| --- | --- |
| The features **Forms** (`OrchardCore.Forms`), **Flows** (`OrchardCore.Flows`), **Workflows** (`OrchardCore.Workflows`) and **HTTP Workflows Activities** (`OrchardCore.Workflows.Http`) | An administrator, in **Tools > Features** |
| The feature **Email** (`OrchardCore.Email`) and an email provider feature: **SMTP Email Provider** (`OrchardCore.Email.Smtp`) or **Azure Communication Services Email** (`OrchardCore.Email.Azure`) | An administrator, in **Tools > Features** |
| The details of your mail server (host, port, user name, password) or Azure Communication Services account | Your IT team or email provider |
| A content type with a **Flow** part, such as **Page** in the Blog recipe | Already there with the Blog recipe; otherwise see [Content types](../content/content-types.md) |
| The permissions *Manage Email Settings*, *Manage workflows*, and permission to create and publish pages | Your administrator; see [Roles and permissions](../getting-started/roles-and-permissions.md) |

## Set up email

The site can't send anything until an email provider is set up.

1. Open **Settings > Communication > Email**.
2. Open the tab of your provider: **SMTP** or **Azure Communication Services**.
3. Tick **Enable SMTP Provider** (or **Enable Azure Provider**).
4. Type the **Sender email address** and the connection details your IT team gave you.
5. Click **Save**.

If you enabled more than one provider, choose the one the site uses:

1. Open the **Providers** tab.
2. Choose the provider in **Default provider**.
3. Click **Save** again.

The **Providers** tab lists a provider only once it's enabled and saved. Then send a test email:

1. Open **Tools > Testing > Email Test**.
2. Choose the **Provider**.
3. Type your address in **To**.
4. Type a **Subject** and a **Body**.
5. Click **Send**.

*Message sent successfully.* means the provider accepted the email; check that it arrives. See [Email](../settings/email.md) for every setting.

## Build the contact page

1. Open **Content > Content Items**.
2. Click **New**.
3. Choose **Page**.
4. Type **Contact** in **Title**.
5. Leave **Permalink** empty.
6. In **Flow**, click the **+** (**Add Widget**) button.
7. Choose **Form**.
8. In the form widget, set these fields:
    - **Title**: type *Contact us*.
    - **Method**: leave **POST**.
    - **Action**: leave it empty.
    - **Anti Forgery Token** and **Save Form Location**: keep them ticked.
9. Inside the form widget, click the **+** (**Add Widget**) button below its fields, and add these widgets, in this order:

    | Widget | Fill in |
    | --- | --- |
    | **Validation Summary** | Nothing. It lists the errors when a check fails. |
    | **Input** | **HTML Name**: `Name`. **Type**: **Text**. **Label option**: **Standard**, **Label Text**: *Your name*. **Validation option**: **Standard**. |
    | **Input** | **HTML Name**: `Email`. **Type**: **Email**. **Label option**: **Standard**, **Label Text**: *Your email*. **Validation option**: **Standard**. |
    | **Text Area** | **HTML Name**: `Message`. **Rows**: 6. **Label option**: **Standard**, **Label Text**: *Your message*. **Validation option**: **Standard**. |
    | **Button** | **HTML Name**: `submit`. **Text**: *Send*. **Type**: **Submit**. |

10. Click **Save Draft**.

With **Permalink** empty, the address becomes `/contact`. The form's **Title** names the form in the admin; the site doesn't show it. You paste the workflow's address into **Action** later, and you publish the page once the workflow is ready.

**HTML Name** is required on every input and button, and may contain only letters, digits, `-`, `_`, `.`, `[` and `]`. The workflow reads the visitor's answers by these names. See [Forms](../forms-and-automation/forms.md) for every form widget and option, such as **Select** lists, placeholders and conditional visibility.

## Create the workflow

### Start the workflow when the form is sent

1. Open **Design > Workflows**.
2. Click **Create Workflow**.
3. Type a **Name**, such as *Contact form*.
4. Click **Create and open the designer**.
5. In the activities list on the left, search for **Http Request Event**.
6. Click **Http Request Event** (or drag it to the canvas).
7. In the activity's settings, choose **POST** in **HTTP Method**.
8. Keep **Validate Antiforgery Token** ticked.
9. Click **Copy** next to the **URL** box, and keep the address.

As the first event, **Http Request Event** becomes the **Start** activity. The form sends an antiforgery token, which is why **Validate Antiforgery Token** stays ticked. The **URL** box already holds an address, and the form will post to it. Anyone with this URL can start the workflow, so don't publish it anywhere but in the form.

### Email the message to you

1. Add an **Email Task**.
2. Drag from the **Done** outcome of **Http Request Event** to **Email Task**, to connect them.
3. Fill in the task's fields:
    - **Recipients**: type your address, such as `contact@example.com`;
    - **Subject**: type `Contact form: {{ Request.Form.Name }}`;
    - **Format**: choose **Html**;
    - **HTML Body**: type `<p>From: {{ Request.Form.Name }} ({{ Request.Form.Email }})</p><p>{{ Request.Form.Message }}</p>`
    - **ReplyTo**: type `{{ Request.Form.Email }}`.

Instead of steps 1 and 2, you can click the **Done** outcome first and choose **Email Task**: that adds the activity and connects it in one go. `{{ Request.Form.Name }}` is replaced with what the visitor typed in the box whose **HTML Name** is `Name`. **ReplyTo** lets you reply straight to the visitor. Leave **From** empty to use the provider's sender address.

### Thank the visitor

1. Add a **Notify Task**.
2. Choose **Success** in **Notification Type**.
3. Type *Thank you, we'll get back to you soon.* in **Message**.
4. Connect both the **Done** and the **Failed** outcomes of **Email Task** to **Notify Task**.
5. Add an **Http Redirect Task**.
6. Type `/contact` in **Location**.
7. Connect the **Done** outcome of **Notify Task** to **Http Redirect Task**.

The visitor goes back to the contact page and sees the message.

### Publish the workflow

1. Open the **Issues** tab, and check that it says *No issues found.*
2. Click **Publish**.

A workflow runs only once it's published. See [Workflows](../forms-and-automation/workflows.md) for the designer, outcomes, and the instances list.

## Connect the form to the workflow

1. Open the **Contact** page again (**Content > Content Items**, **Edit**).
2. In the form widget, paste the workflow URL you copied into **Action**.
3. Click **Publish**.

The workflow URL starts with `/workflows/` and ends with a long token.

## Check that the boxes are filled in (optional)

Without checks, an empty form still sends you an email. To reject it and show errors instead:

1. In the designer, add three **Validate Form Field Task** activities, one per box.
2. In each, type the **Field Name**: `Name`, `Email` or `Message`.
3. In each, type an **Error Message**, such as *Please enter your name.*
4. Remove the connection from **Http Request Event** to **Email Task**.
5. Connect the **Done** outcome of **Http Request Event** to the first check.
6. Connect the **Done** outcome of each check to the next one.
7. Add a **Validate Form Task**.
8. Connect the **Done** outcome of the last check to **Validate Form Task**.
9. Connect the **Valid** outcome of **Validate Form Task** to **Email Task**.
10. Add an **Http Redirect To Form Location Task**.
11. Connect the **Invalid** outcome of **Validate Form Task** to **Http Redirect To Form Location Task**.
12. Click **Publish**.

The checks now run one after the other before the email is sent. **Http Redirect To Form Location Task** sends the visitor back to the page the form was on (that's what **Save Form Location** is for), where the **Validation Summary** widget lists the errors.

To stop spam robots, the **ReCaptcha** features add a **Re Captcha** form widget and a **Validate ReCaptcha Task**; they need keys from Google set in **Settings > Security > reCaptcha**. See [Security](../settings/security.md).

## Check that it works

1. Open `/contact` in a private browser window, so you're a visitor.
2. Fill in the form.
3. Click **Send**. You land back on the page with the thank-you message.
4. Check your inbox for the email.
5. If you added the checks, send the form empty: you see the errors, and no email is sent.
6. In the admin, open **Design > Workflows**.
7. Click **Instances**.

The instances list shows each run of the workflow, and where a failed one stopped.

## Tips

- **No email?** First send a test from **Tools > Testing > Email Test**. If that works, open the workflow's instances to see whether **Email Task** took the **Failed** outcome. See [Troubleshooting](../troubleshooting.md).
- **The form does nothing, or shows an error page?** Check that the form's **Action** is the workflow's current URL (it changes if someone clicks **Regenerate**), that **HTTP Method** is **POST**, and that the workflow is published and enabled.
- Keep the **HTML Name** of each box and the `Request.Form...` names in the workflow identical, including case.
- To keep submissions, not only email them, a workflow can also create a content item with **Create Content Task**.
