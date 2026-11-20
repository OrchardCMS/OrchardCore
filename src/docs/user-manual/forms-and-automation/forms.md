---
title: Forms
description: Build forms such as a contact form from form widgets, show or hide fields with rules, protect them from robots, and handle what visitors send with a workflow.
technical_manual:
  - reference/modules/Forms/README.md
  - reference/modules/ReCaptcha/README.md
  - reference/modules/Workflows/README.md
---

# Forms

The Forms feature lets you build forms, such as a contact form, a quote request or a sign-up form, without code. You add a **Form** widget to a page and put form widgets inside it: text boxes, lists, labels, a button. When a visitor sends the form, a workflow receives what they typed and does something with it: emails it to you, saves it, shows a thank-you page.

This page is the full reference for forms. For a step-by-step walkthrough, see [Add a contact form](../use-cases/contact-form.md).

| | |
| --- | --- |
| **Menu** | None of its own: you add the form widgets to any content item that has a **Flow** part (such as a **Page**), or in a zone through **Design > Widgets** |
| **Permission** | The permissions to edit the content item or the widgets the form lives in |
| **Feature** | **Forms** (`OrchardCore.Forms`), which also turns on **Widgets** and **Flows** |

!!! tip "Don't see the form widgets?"
    They appear in the **Add Widget** list only when the **Forms** feature is on. See [Features](../getting-started/features.md).

## How a form works

A form is made of three things:

1. **A Form widget.** It holds the other widgets and says where the visitor's answers go when they click the button (its **Action**).
2. **Form element widgets** inside it: **Label**, **Input**, **Text Area**, **Select**, **Button**, and, to show errors, **Validation** and **Validation Summary**. With the **ReCaptcha** feature on, there is also a **Re Captcha** widget.
3. **Something that receives the answers.** Usually a workflow that starts with an **Http Request Event**: the form sends the answers to the workflow's address, and the workflow checks them, sends an email, and sends the visitor to another page. See [Workflows](workflows.md).

Each field has an **HTML Name**. It's the name the workflow uses to read what the visitor typed in that field, for example `Email`.

## The form widgets

These are all the widgets the Forms feature adds. Each one is a widget, so you add it with **Add Widget** inside the Form widget.

| Widget | What it is |
| --- | --- |
| **Form** | The container. Holds the other form widgets and sends their values to its **Action** URL. |
| **Label** | A piece of text tied to a field, for when you don't use the field's own label. |
| **Input** | A one-line field: text, email, phone, number, date, checkbox, file, hidden value, and more. |
| **Text Area** | A field of several lines, for messages. |
| **Select** | A choice among options, shown as a drop-down list, checkboxes or radio buttons. |
| **Button** | The button that sends (or resets) the form. |
| **Validation** | Shows the error message of one field, where you place it. |
| **Validation Summary** | Shows all the errors of the form in one list, usually at the top. |
| **Re Captcha** | A "I'm not a robot" check. Needs the **ReCaptcha** feature (`OrchardCore.ReCaptcha`). |

### Form

| Field | What it does |
| --- | --- |
| **Title** | A name for the form, to find it in the editor. It isn't shown on the site. |
| **ID** | Optional. The HTML id of the form. |
| **Action** | The URL the form is sent to. For a workflow, paste the URL of its **Http Request Event**, without the site address (it starts with `/workflows/Invoke?token=`). |
| **Method** | **POST** (the default, for forms that send data) or **GET**. |
| **EncType** | How the answers are packed when the method is POST. Keep `application/x-www-form-urlencoded`; choose `multipart/form-data` if the form has a file upload field. |
| **Anti Forgery Token** | Checked by default. Adds a hidden security token, which the workflow checks to make sure the form came from your site. Keep it checked when a workflow receives the form. |
| **Save Form Location** | Checked by default. Remembers the page the form was on, so a workflow can send the visitor back to it (with the **Http Redirect To Form Location Task**). |

Below the fields, the Form's own **Flow** holds the widgets of the form.

### Input, Text Area and Select

These three share a **Content** tab and a **Visibility Settings** tab.

| Field | What it does |
| --- | --- |
| **HTML Name** | Required. The name of the field, used by the workflow to read its value. Use letters, digits, `-`, `_`, `.`, `[` and `]` only, for example `Email`. |
| **ID** | Optional. The HTML id of the field. If you leave it empty, one is made from the HTML Name. |
| **Label option** | **None** (no label), **Standard** (a label shown above the field) or **Screen Readers** (a label only screen readers announce, for fields whose meaning is clear on screen). |
| **Label Text** | The text of the label. Shown when the label option isn't **None**. |
| **Validation option** | **None**, or **Standard** to show this field's error message under it when the workflow finds the value wrong. |

The **Input** widget adds:

| Field | What it does |
| --- | --- |
| **Type** | The kind of field: **Text**, **Number**, **Email**, **Phone**, **Date**, **Time**, **DateTime**, **Local DateTime**, **Month**, **Week**, **Hidden**, **Password**, **Checkbox**, **Color**, **Range**, **File**, **Url**, **Image**, **Reset**, **Search** or **Submit**. The browser shows the matching control, such as a date picker. For **Hidden**, **Reset** and **Submit**, the placeholder, label and validation options are hidden, since they don't apply. |
| **Placeholder Text** | Grey text shown in the empty field, such as *you@example.com*. |
| **Default Value** | The value already filled in. For a hidden field, the value it sends. |

The **Text Area** widget adds **Placeholder Text**, **Default Value** and **Rows** (the height of the box, in lines).

The **Select** widget adds:

| Field | What it does |
| --- | --- |
| **Editor** | **Dropdown** (a drop-down list), **Checkbox** (several choices can be ticked) or **Radio** (one choice among buttons). |
| **Options** | The choices. Click **Add an option**, then type the **Option Text** (what visitors see) and the **Value** (what is sent). Drag options to reorder them, mark one with **Set as default** to preselect it, or remove one. **Edit Data** shows the options as text, to paste a long list at once. |

### Label, Button, Validation and Validation Summary

| Widget | Fields |
| --- | --- |
| **Label** | **Title** (the text of the label), **ID**, and **For**: the ID of the field it describes, so clicking the label puts the cursor in that field. |
| **Button** | **HTML Name** (required), **ID**, **Text** (what the button says, such as *Send*) and **Type**: **Submit** (sends the form), **Button** (does nothing by itself) or **Reset** (clears the form). |
| **Validation** | **For**: the **HTML Name** of the field whose error it shows. Use it when a field's own **Validation option** doesn't put the error where you want. |
| **Validation Summary** | **Show Model Only Error**: checked, it lists only the errors that aren't tied to a field; unchecked, it lists every error. |
| **Re Captcha** | No fields. It tells you whether the [reCAPTCHA settings](../settings/security.md) are configured; if they aren't, the captcha isn't shown on the site. |

## Build a contact form

This example puts a contact form with a name, an email address and a message on a page. To receive the answers, you also need the workflow described in [Handle what visitors send](#handle-what-visitors-send).

1. Open **Content > Content Items**, and create or edit a **Page**.
2. In its **Flow**, click the **+** button (**Add Widget**) and choose **Form**.
3. In the Form widget, give it a **Title** such as *Contact form*. Leave **Method** on **POST** and **Anti Forgery Token** checked. Leave **Action** empty for now.
4. Inside the Form widget's own Flow, click **Add Widget** and choose **Validation Summary**, so visitors see what's wrong if they forget a field.
5. Add an **Input** widget. Set **HTML Name** to `Name`, **Label option** to **Standard**, **Label Text** to *Your name*, **Type** to **Text** and **Validation option** to **Standard**.
6. Add another **Input** widget: **HTML Name** `Email`, **Label Text** *Your email*, **Type** **Email**, **Validation option** **Standard**.
7. Add a **Text Area** widget: **HTML Name** `Message`, **Label Text** *Your message*, **Rows** `5`, **Validation option** **Standard**.
8. Optionally, add a **Re Captcha** widget to stop robots.
9. Add a **Button** widget: **HTML Name** `submit`, **Text** *Send*, **Type** **Submit**.
10. Click **Publish**.

The page now shows the form. It doesn't send anything yet: create the workflow, then paste its URL in the Form's **Action** and publish the page again.

!!! note
    The form widgets don't check required fields by themselves. A field is required because the workflow checks it (with the **Validate Form Field Task**) and sends the visitor back with an error when it's empty.

## Show or hide a field depending on another

On the **Visibility Settings** tab of an **Input**, **Text Area** or **Select**, you can show a field only when it matters, for example a *Company name* field only when *I am writing for* is set to *A company*. The rules run in the visitor's browser, as they fill in the form.

1. Edit the field to show or hide and open its **Visibility Settings** tab.
2. Set **Action** to **Conditionally show** or **Conditionally hide**. (**Always visible** turns the rules off.)
3. Click **New Group**, then **New Rule**.
4. In the rule, pick the other field in **Select Field**, a comparison in **Select Operator**, and type the **Value** to compare with. Check **Case sensitive** if upper and lower case matter.
5. Click **Publish** on the page.

| Operator | Matches when the other field... |
| --- | --- |
| **Is** / **Is not** | equals / doesn't equal the value. |
| **Empty** / **Not empty** | has no value / has a value (no value box). |
| **Contains** / **Does not contain** | includes / doesn't include the value. |
| **Starts with** / **Ends with** | begins / ends with the value. |
| **Greater than** / **Less than** | is above / below the value, for numbers and dates. |

The list offers only the operators that make sense for the other field's type. All the rules of a group must match. If you add several groups, the field is shown (or hidden) as soon as one group matches. Each rule and group has a delete button.

## Handle what visitors send

A workflow receives the form. The usual design is:

1. **Http Request Event**: starts the workflow when the form is sent. Set its **HTTP Method** to **POST**, keep **Validate Antiforgery Token** checked, and click **Regenerate** to get its **URL**. Copy that URL into the Form widget's **Action**.
2. **Bind Model State Task**: keeps what the visitor typed, so the form isn't emptied if it has to be shown again with errors.
3. **Validate Form Field Task**, one per required field: checks that the field isn't empty, with the **Field Name** (the HTML Name) and the **Error Message** to show.
4. **Validate ReCaptcha Task**, if the form has a Re Captcha widget.
5. **Validate Form Task**: goes on with **Valid** if no check failed, or **Invalid** otherwise.
6. On **Invalid**: **Http Redirect To Form Location Task** (or **Http Redirect Task** with the page's URL) sends the visitor back to the form, which shows the errors and the values they typed.
7. On **Valid**: **Email Task** (or **Create Content Task**, **Notify Specific Users Task**...), then **Http Redirect Task** to a thank-you page.

In the workflow, read what the visitor typed with Liquid: `{{ Request.Form.Email }}` is the value of the field whose HTML Name is `Email`.

The [Workflows](workflows.md#example-email-the-site-owner-when-a-form-is-sent) page builds this workflow step by step, and the [Contact form](../use-cases/contact-form.md) use case puts it all together.

| Form task | What it does | Outcomes |
| --- | --- | --- |
| **Validate Antiforgery Token Task** | Checks the form's security token. Not needed when the Http Request Event already validates it. | **Done**, **Valid**, **Invalid** |
| **Bind Model State Task** | Keeps the submitted values so the form can show them again. | **Done** |
| **Validate Form Field Task** | Adds the **Error Message** when the field named in **Field Name** is empty. | **Done**, **Valid**, **Invalid** |
| **Add Model Validation Error Task** | Adds an error of your own to a field (**Key**, its HTML Name) with an **Error Message**, for checks you make with other tasks. | **Done** |
| **Validate Form Task** | Tells whether any check added an error. | **Valid**, **Invalid** |
| **Validate ReCaptcha Task** | Checks the captcha answer; adds *Captcha validation failed. Try again.* when it's wrong. | **Done**, **Valid**, **Invalid** |
| **Http Redirect To Form Location Task** | Sends the visitor back to the page the form was on (needs **Save Form Location** on the Form). Its **Form Location Key** matches the one of the Http Request Event when one workflow handles several forms; leave both empty otherwise. | **Done**, **Failed** |

## Send a form somewhere else

The **Action** of a Form widget can be any URL, not only a workflow's: a page of another service that accepts form posts, for example. In that case, the site doesn't see the answers, and the validation widgets have nothing to show.

## Protect a form from robots

1. Ask your administrator to turn on the **ReCaptcha** feature and to fill in **Settings > Security > reCaptcha** (see [Security settings](../settings/security.md)).
2. Add a **Re Captcha** widget to the form, before the button.
3. In the workflow, add a **Validate ReCaptcha Task** before the **Validate Form Task**.

Anti-forgery tokens protect against forms sent from other sites; the captcha protects against robots filling in your own form. Use both on public forms.

!!! example "See it in practice"
    [Add a contact form](../use-cases/contact-form.md) walks through this step by step.
