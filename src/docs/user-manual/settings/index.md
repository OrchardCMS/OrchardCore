---
title: Site settings
description: Change how the whole site behaves - its name and time zone, email and SMS, media options, your own settings screens, security, analytics,.
---

# Site settings

This section covers the settings that apply to the whole site rather than to one content item: its name and time zone, the services it sends email and text messages through, its security options, and its connections to analytics and social networks. It is mostly for **administrators**; usually only they see the **Settings** menu.

Most of these screens are under **Settings** in the admin menu, for example **Settings > General**, **Settings > Communication**, **Settings > Media**, **Settings > Security** and **Settings > Integrations**. A few live elsewhere: image profiles under **Media > Profiles**, the email and SMS tests under **Tools > Testing**, and rate limits under **Tools**.

## I want to…

| I want to… | Read |
| --- | --- |
| Change the site name, time zone or number of items per page | [General settings](general.md#change-the-site-name-time-zone-and-page-size) |
| Change how the admin looks and behaves | [General settings](general.md#change-how-the-admin-looks-and-behaves) |
| Connect the site to our mail server and send a test email | [Email](email.md#set-up-an-smtp-provider) |
| Send text messages through Twilio | [SMS](sms.md#set-up-twilio) |
| Add an image size, such as a banner, for the theme to use | [Media settings](../media/media-settings.md#add-a-media-profile) |
| Add a settings screen of our own, such as a footer text | [Custom settings](custom-settings.md#create-a-custom-settings-screen) |
| Protect the login and registration forms with reCAPTCHA | [Security](security.md#protect-sign-in-and-forms-with-recaptcha) |
| Make every page open over HTTPS | [Security](security.md#require-https) |
| Track visits with Google Analytics or Google Tag Manager | [Analytics and social](analytics-and-social.md#track-visits-with-google-analytics) |

## Pages in this section

- [General settings](general.md): site name, time zone, page size, base URL, scripts, cache, the admin's look and debugging options.
- [Email](email.md): SMTP and Azure Communication Services, the default provider and a test email.
- [SMS](sms.md): Twilio and Azure Communication Services, the default provider and a test SMS.
- [Media settings](../media/media-settings.md): image profiles, media options, the media cache, and what IT sets up.
- [Custom settings](custom-settings.md): settings screens you build from a content type.
- [Security](security.md): security headers, CORS, reCAPTCHA, rate limits, reverse proxy and HTTPS.
- [Analytics and social](analytics-and-social.md): Google Analytics, Google Tag Manager, Meta and X (Twitter).

**See it in practice:** [Add a contact form](../use-cases/contact-form.md), which needs an email provider.

!!! warning "Some settings reload the site"
    Saving some of these screens reloads the site, which may answer slowly for a few seconds. See [How settings changes apply](general.md#how-settings-changes-apply).
