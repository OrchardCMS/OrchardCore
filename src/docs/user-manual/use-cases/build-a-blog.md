---
title: Build a blog
description: Set up a blog with the Blog recipe or add one to an existing site, write and publish posts with a banner image, tags and a category, schedule posts, link the blog from the menu and share its RSS feed.
technical_manual:
  - guides/create-cms-application/README.md
  - reference/modules/Lists/README.md
  - reference/modules/PublishLater/README.md
  - reference/modules/Taxonomies/README.md
  - reference/modules/Feeds/README.md
  - reference/modules/Menu/README.md
---

# Build a blog

This walkthrough takes you from an empty site to a working blog: posts with a banner image, tags and a category, a post scheduled to go live later, a link in the site menu, and an RSS feed readers can subscribe to.

## Who it's for and what you get

For **site builders** who set the blog up, and **editors** who write the posts.

When it's done:

- a **Blog** page that lists your posts, newest first, a page at a time;
- posts with a title, a subtitle, a banner image, a body, tags and a category;
- posts you can schedule to publish at a date and time you choose;
- a link to the blog in the site's main menu;
- an RSS feed of the blog that news readers can follow.

## Before you start

| You need | Where or who |
| --- | --- |
| A site set up with the **Blog** recipe, or an existing site you add a blog to | Whoever sets up the site; see [Setting up a site](../getting-started/setting-up-a-site.md) |
| The features **Lists**, **Taxonomies**, **Menu**, **Feeds** and **Media** (the Blog recipe turns them on) | An administrator, in **Tools > Features** |
| The feature **Publish Later** (`OrchardCore.PublishLater`) to schedule posts. The Blog recipe doesn't turn it on. | An administrator, in **Tools > Features** |
| Permission to create and publish content, to edit content types (to add Publish Later), and to manage menus | Your administrator; see [Roles and permissions](../getting-started/roles-and-permissions.md) |

## Set up the blog

### With the Blog recipe

The quickest way. On the setup screen of a new site (or a new tenant), choose the **Blog** recipe. It creates:

- a **Blog** content item, set as the home page, that lists **Blog Post** items;
- the **Blog Post** content type with **Title**, **Permalink**, **MarkdownBody**, **Subtitle**, **Banner Image**, **Tags** and **Category**;
- two taxonomies, **Tags** and **Categories**;
- a **Main Menu** with **Home** and **About** links;
- an RSS feed for the blog;
- a **Blog** entry at the top of the admin menu that opens the blog's list of posts.

See [Setting up a site](../getting-started/setting-up-a-site.md).

### On an existing site

Recipes that set up a whole site run only at setup, so on an existing site you build the two content types yourself.

1. Open **Design > Content Definition > Content Types**.
2. Create a **Blog Post** content type with the **Title**, **Autoroute** and **MarkdownBody** (or **HtmlBody**) parts.
3. Add the fields you want to **Blog Post**, such as a **Media Field** for a banner image and **Taxonomy Field**s for tags and a category.
4. Create a **Blog** content type with the **Title**, **Autoroute** and **List** parts.
5. In the **List** part settings of **Blog**, tick **Blog Post** in **Contained Content Types**.
6. Open **Content > Content Items > New > Blog** to create a **Blog** content item.
7. Click **Publish**.

See [Content types](../content/content-types.md) for creating types and adding fields, and [Lists and blogs](../content/lists-and-blogs.md) for the **List** part. The rest of this page uses the names from the Blog recipe.

## Set up tags and categories

The **Tags** field lets editors type new tags as they write, so there is nothing to prepare. Categories are a fixed list you keep in the **Categories** taxonomy.

1. Open **Content > Taxonomy**.
2. Click **Edit** on the **Categories** row.
3. Click **Add Category**.
4. Type the **Title**, for example *Travel*.
5. Choose an **Icon** (required).
6. Click **Publish**.
7. Back on the taxonomy, add more categories the same way.
8. Drag the categories to reorder or nest them.
9. Click **Publish** at the bottom of the taxonomy.

The changes to the taxonomy go live only when you publish it. In the Blog recipe, a post can have **one** category, and only a category with no sub-categories under it. See [Taxonomies](../content/taxonomies.md).

## Write and publish a post

1. Click **Blog** at the top of the admin menu (or open the **Blog** item from **Content > Content Items**).
2. Click **Create Blog Post**.
3. Type the **Title**.
4. Leave **Permalink** empty.
5. Write the post in **MarkdownBody**.
6. Type a **Subtitle** if you want one.
7. Under **Banner Image**, click **Add media**.
8. Choose an image in the media library, or upload one there.
9. Click **OK**.
10. In **Tags**, type a tag, or choose an existing tag from the suggestions, and press Enter. Repeat for each tag.
11. Under **Category**, choose one category.
12. Click **Preview** to see the post as visitors will, in a new tab.
13. Click **Publish**. To keep working on it first, click **Save Draft** instead.

With **Permalink** left empty, the address is made from the blog's address and the title, such as `/blog/my-first-trip`. Creating a new tag needs the *Manage taxonomies* permission; you can add as many tags as you like. See [Media library](../media/media-library.md) for choosing images and [Content preview](../content/preview.md) for the preview.

The post shows on the blog page, newest first. See [Content items](../content/content-items.md) for editing, unpublishing and deleting posts.

## Schedule a post

Scheduling needs the **Publish Later** part on the **Blog Post** content type. Add it once:

1. Open **Design > Content Definition > Content Types**.
2. Click **Edit** on **Blog Post**.
3. Click **Add Parts**.
4. Tick **PublishLater**.
5. Click **Add Selected Parts**.

The part is added right away; there is nothing else to save.

Then, to schedule a post:

1. Create or edit the post as above.
2. Next to the action buttons at the bottom, choose the date and time in the box beside **Publish Later**.
3. Click **Publish Later**.

The post stays a draft until that time. The site checks for scheduled items every minute and publishes the post when its time comes. In the blog's list of posts, a scheduled post shows a clock badge with the time left; hover over it to see the exact date. To call it off, open the post and click **Cancel Publish Later**. See [Scheduling](../content/scheduling.md).

!!! note
    The date and time are in your own time zone if your profile has one, otherwise in the site's time zone. Scheduled publishing relies on the site's background tasks; if they are turned off in **Tools > Background Tasks**, nothing gets published on time. See [Background tasks](../settings/background-tasks.md).

## Add the blog to the menu

With the Blog recipe the blog is the home page, so the **Home** link already opens it. If you made another page the home page, or added a blog to an existing site, add a link:

1. Open **Content > Menus**. (The admin menu also has a **Main Menu** entry.)
2. Click **Edit** on **Main Menu**.
3. Click **Add Menu Item**.
4. Click **Add** next to **Content Menu Item**.
5. Type the link text in **Name**, for example *Blog*.
6. In **Selected Content Item**, choose the **Blog** item.
7. Click **Publish**.
8. Back on the menu, drag the item to where it should appear.
9. Click **Publish**.

Adding an item saves the menu as a draft, so the link appears on the site only once you publish the menu. A **Link Menu Item** works too: type the blog's address in **Url**, such as `~/blog`. See [Menus](../design/menus.md).

## Share the RSS feed

The blog has an RSS feed as soon as the **Feeds** feature is on. The blog's page tells browsers and news readers about it, so most readers find it from the blog's address.

1. Open the **Blog** item and click **Edit** (from **Blog** in the admin menu, click **Edit Blog**).
2. Below the body, copy the feed's address from the hint under **Feed proxy Url**, such as `https://example.com/Contents/Lists/<id>/rss`, to share or link it.
3. Type in **Feed items count** how many posts the feed holds (20 by default).
4. To use a feed service, paste its public address in **Feed proxy Url**.
5. To stop advertising the feed, tick **Disable RSS feed link generation.**
6. Click **Publish**.

With a **Feed proxy Url**, the blog points readers to the feed service. **Disable RSS feed link generation.** also turns the feed address off. See [RSS feeds](../search/feeds.md).

## Check that it works

1. Open the site in a private browser window, so you see it as a visitor.
2. Open the blog: your published post is listed, with its banner image, and the scheduled post isn't.
3. Open the post, and check its address, tags and category.
4. Click the **Blog** link in the menu.
5. Open the feed address: the newest posts are listed.
6. After the scheduled time, reload the blog: the scheduled post is there.

## Tips

- Write the **Title** before you leave **Permalink** empty: the address comes from the title the first time you save.
- Use a few broad categories and many specific tags.
- Large images slow pages down. Resize photos before you upload them. The Blog recipe also adds a **banner** media profile (2048 x 600 pixels, cropped) that templates can use; see [Media settings](../media/media-settings.md).
- To let someone write posts without publishing them, see [Set up content approval](content-approval.md).
