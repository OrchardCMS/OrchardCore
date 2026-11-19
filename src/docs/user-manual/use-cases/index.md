---
title: Use cases
description: End-to-end walkthroughs for common goals, such as running a blog, a contact form, a multilingual site or site search, with who each one is for and the features it needs.
technical_manual:
  - guides/create-cms-application/README.md
  - reference/glossary/README.md
---

# Use cases

Each use case takes one goal from start to finish: a blog, a contact form, a translated site, and so on. It says who it is for, what you need before you start, and then walks you through the screens **in order**, naming the exact menus, buttons and fields. Each step links to the page that explains that screen in full, so you can read more when you need to.

Use them to plan a new site, or as a checklist the first time you set something up.

| Use case | What you get | Mostly for | Features it needs |
| --- | --- | --- | --- |
| [Build a blog](build-a-blog.md) | A blog with posts that have a banner image, tags and a category, scheduled posts, a menu link and an RSS feed. | Site builders, editors | **Lists**, **Taxonomies**, **Menu**, **Feeds**, **Publish Later** (all but the last come with the Blog recipe) |
| [Add a contact form](contact-form.md) | A contact page whose form emails you each message, then thanks the visitor. | Site builders, administrators | **Forms**, **Flows**, **Workflows**, **HTTP Workflows Activities**, **Email** and an email provider |
| [Build a landing page with widgets](landing-page-with-widgets.md) | A home page built from blocks (text, images, quotes, columns), and a banner shown on every page. | Site builders, editors | **Flows**, **Widgets**, **Layers**, **Home Route**, **Autoroute** |
| [Run a multilingual site](multilingual-site.md) | Pages in several languages, linked to each other, with a language picker for visitors. | Site builders, translators | **Localization**, **Content Localization**, **Content Culture Picker** |
| [Add site search](site-search.md) | A search page and search box that find your pages and posts, and optionally the text inside attached PDF files. | Site builders, administrators | **Indexing**, **Lucene**, **Search**, **PDF Media Indexing** |
| [Give an editor limited access](editor-with-limited-access.md) | A role that can edit only some content types, a user with that role, and a check of what they see. | Administrators | **Roles**, **Users**, **Content Types** |
| [Set up content approval](content-approval.md) | Contributors write drafts, editors review and publish them, and reviewers get notified when a draft is saved. | Administrators, editors | **Contents**, **Roles**; optionally **Workflows** and **Notifications** |
| [Copy content between sites](copy-content-between-sites.md) | Content (and media, types or settings) moved from one site to another, by file or directly over the internet. | Administrators | **Deployment**; optionally **Remote Deployment** |

!!! tip "Not an administrator?"
    Each use case lists the features and permissions it needs. Turning features on and changing roles needs an administrator. If a menu named in a step is missing, see [Roles and permissions](../getting-started/roles-and-permissions.md) for what to ask for.

## How to read a use case

Every use case has the same parts:

- **Who it's for and what you get**: the goal, and what the site does once you're done.
- **Before you start**: the features to turn on, the permissions you need, and anything your IT team must set up first. Features are turned on in **Tools > Features**; see [Features](../getting-started/features.md).
- **The steps**: one section per stage, each with numbered steps.
- **Check that it works**: how to test the result as a visitor would see it.
- **Tips**: common mistakes and good habits.

Steps use a site set up with the **Blog** recipe as the example, because its content types (such as **Page**, **Article**, **Blog** and **Blog Post**) are ready to use. On a site set up with another recipe, the content types and menus may differ; see [Setting up a site](../getting-started/setting-up-a-site.md).

## Words you'll meet

The use cases use the admin's own words: **content item**, **content type**, **part**, **field**, **widget**, **zone**, **layer**, **role**, **permission**, **draft**, **publish**. If one is new to you, the [Glossary](../glossary.md) explains it in plain language.
