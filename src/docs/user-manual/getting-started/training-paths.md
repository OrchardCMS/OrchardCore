---
title: Training paths
description: Ordered reading lists, with time estimates, to train editors, content managers, site builders and administrators, and a checklist of what each person should be able to do at the end.
---

# Training paths

Use these paths to train the people who work on your site, or to train yourself. Each path is an ordered checklist of User Manual pages, with what you learn on each one and a rough time. It ends with a list of things the person should be able to do, so a trainer can check they are ready.

## How to use a path

- Read the [Start here](#start-here) pages first, whatever your role. Then follow your path in order: each page builds on the ones before it.
- Repeat each task yourself as you read. A test site is best: set one up with the **Blog** recipe (see [Set up a site](setting-up-a-site.md)), because its content types (**Page**, **Article**, **Blog**, **Blog Post**), menu, taxonomies and sample content are ready to practice on. On a test site you sign in as the super user, so you see every menu.
- Times are rough estimates for reading a page and trying its tasks once.
- Paths build on each other: a content manager usually knows the Editor path, and a site builder or administrator knows the first two.
- On your real site, you see only what your role allows. If a page describes a menu you don't have, see [Roles and permissions](roles-and-permissions.md).

| Path | For | Time |
| --- | --- | --- |
| [Editor](#editor) | People who write and publish pages and posts | About 2½ hours |
| [Content manager](#content-manager) | People who organize, translate and keep content findable | About 3 hours, after the Editor path |
| [Site builder](#site-builder) | People who shape the site from the admin: content types, theme, widgets, forms, search | About 1 day |
| [Administrator](#administrator) | People who run the site: features, users, roles, sign-in and settings | About 1 day |

## Start here

**Time:** about 45 minutes. Everyone does these first.

- [ ] **1.** [Find your way around](finding-your-way.md): the dashboard, the admin menu, the top bar, quick navigation and how admin lists work.
- [ ] **2.** [Sign in and manage your profile](signing-in.md): sign in and out, reset a forgotten password, set up two-factor authentication and edit your profile.
- [ ] **3.** [Roles and permissions](roles-and-permissions.md): why your menu differs from a colleague's, what the built-in roles do, and how to ask for access.

## Editor

**Time:** about 2½ hours.

- [ ] **1.** [Content items](../content/content-items.md): find, create, save as a draft, publish, unpublish, clone and delete content items.
- [ ] **2.** [Edit text](../content/editing-text.md): write the body in the rich text, HTML or Markdown editor, and insert images and shortcodes.
- [ ] **3.** [Fields](../content/fields.md): fill in each kind of box an editor can show, such as dates, links, pictures and pickers.
- [ ] **4.** [Media library](../media/media-library.md): upload pictures and documents, keep them in folders, and pick them in a content item.
- [ ] **5.** [Titles, URLs and the home page](../content/urls-and-homepage.md): give an item a title and a web address, and change the address of a published item.
- [ ] **6.** [Preview](../content/preview.md): see how an item will look on the site before you publish it.
- [ ] **7.** [Schedule publishing](../content/scheduling.md): publish an item at a future date and time, and take it off the site later.

Then walk through the posting steps of the [Build a blog](../use-cases/build-a-blog.md) use case.

**You're ready when you can:**

- create an **Article**, save it as a draft, preview it, and publish it;
- find your item again in the content list with the search box and filters, and unpublish it;
- upload a picture into a folder of the media library and insert it in the body of an item;
- fill in a required field and understand the message you get when you leave it empty;
- change the permalink of a published item;
- schedule an item to go live tomorrow morning, and cancel the schedule (the **Publish Later** feature must be on: the Blog recipe doesn't turn it on).

## Content manager

**Time:** about 3 hours, after the Editor path.

- [ ] **1.** [Lists and blogs](../content/lists-and-blogs.md): manage the items inside a list such as a blog, add and reorder them.
- [ ] **2.** [Taxonomies](../content/taxonomies.md): build categories and tags, organize terms in a tree, and filter the content list by term.
- [ ] **3.** [Flows and bags](../content/flows-and-bags.md): build a page from widgets, and keep a collection of items inside a content item.
- [ ] **4.** [Menus](../design/menus.md): build the site's navigation, nest and reorder links, and show a link only to some people.
- [ ] **5.** [Translate content](../content/localization.md): add the site's languages, translate content items and let visitors switch language.
- [ ] **6.** [SEO](../content/seo.md): set the page title, description and social sharing picture of an item, and the site-wide defaults.
- [ ] **7.** [RSS feeds](../search/feeds.md): offer a feed for a blog or another list, and choose how many items it shows.
- [ ] **8.** [Audit trail](../content/audit-trail.md): see who changed what, compare two versions, and restore an earlier version or a deleted item.

Then read the [Build a blog](../use-cases/build-a-blog.md) and [Run a multilingual site](../use-cases/multilingual-site.md) use cases.

**You're ready when you can:**

- add a blog post through its blog, and change the order of the items of a list;
- add a term to the **Categories** taxonomy, pick it on a post, and filter the content list by it (with the **Taxonomies Contents List Filters** feature on);
- add a link to a page in the **Main Menu**, move it under another link, and check it on the site;
- build a page from a few widgets in its flow, and reorder them;
- translate a page into a second language and find both versions in the list;
- fill in the SEO of a page;
- find the last change to a page in the audit trail and restore the version before it (the **Audit Trail** feature must be on, with the page's content type selected in its settings).

## Site builder

**Time:** about 1 day.

- [ ] **1.** [Content types](../content/content-types.md): create a content type, add parts and fields to it, and set its options.
- [ ] **2.** [Fields](../content/fields.md): choose each field's editor, required rule, hint and other settings.
- [ ] **3.** [Themes](../design/themes.md): choose the theme of the site and of the admin.
- [ ] **4.** [Widgets and layers](../design/widgets-and-layers.md): put widgets in the zones of the theme, and decide with layer rules where and for whom they show.
- [ ] **5.** [Templates and placements](../design/templates-and-placements.md): change how a piece of a page looks with a Liquid template, and move or hide parts of a page.
- [ ] **6.** [Forms](../forms-and-automation/forms.md): build a form such as a contact form from form widgets, and protect it from robots.
- [ ] **7.** [Workflows](../forms-and-automation/workflows.md): automate what happens when a form is sent or content is published, for example sending an email.
- [ ] **8.** [Site search and indexes](../search/site-search.md): create a search index and give visitors a search page and a search box.

Then work through the [Build a landing page](../use-cases/landing-page-with-widgets.md), [Add a contact form](../use-cases/contact-form.md) and [Add site search](../use-cases/site-search.md) use cases.

**You're ready when you can:**

- create a content type with a title, a permalink, a body and two fields of your own, and create an item of it;
- switch the site to another theme and back;
- add a layer with a rule, and a widget that shows only on the pages that rule allows;
- hide or move a part of a page with a placement;
- build a contact form on a page and have a workflow email you each message (an email provider must be set up);
- create an index, make it the **Default search index**, let visitors query it, and find a published page with a **Search Form** widget.

## Administrator

**Time:** about 1 day.

- [ ] **1.** [Features](features.md): turn features on and off, understand dependencies, and find the page that covers each feature.
- [ ] **2.** [General settings](../settings/general.md): set the site name, time zone and page size, and the look of the admin.
- [ ] **3.** [Users](../users/users.md): add accounts, give roles, change passwords, and disable, unlock or delete accounts.
- [ ] **4.** [Manage roles](../users/roles.md): create roles, choose their permissions, and read the effective permissions.
- [ ] **5.** [Login and registration](../users/login-and-registration.md): decide how people sign in, whether visitors can register, and how passwords are reset.
- [ ] **6.** [Email](../settings/email.md): connect the site to an email service and send a test email.
- [ ] **7.** [External sign-in](../users/external-sign-in.md): let people sign in with Microsoft, Google, GitHub or another provider.
- [ ] **8.** [Import and export](../administration/import-and-export.md): export content and settings with a deployment plan, and import them on another site.
- [ ] **9.** [Troubleshooting](../troubleshooting.md): answer the "why can't I see…" and "why doesn't it work…" questions your colleagues will ask.

Then read the [Give an editor limited access](../use-cases/editor-with-limited-access.md), [Set up content approval](../use-cases/content-approval.md) and [Copy content between sites](../use-cases/copy-content-between-sites.md) use cases.

**You're ready when you can:**

- turn a feature on, and say which page of this manual covers it;
- create a user with the **Author** role, sign in as them in a private browser window, and explain what they can and can't see;
- create a role that can edit only one content type;
- change the site's time zone, and turn visitor registration on and off;
- set up an email provider and receive the test email;
- run a deployment plan that exports a few content items, and import the package on a test site;
- use [Troubleshooting](../troubleshooting.md) to find why a menu is missing for someone.
