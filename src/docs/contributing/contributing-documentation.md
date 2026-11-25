# Contributing to the Orchard Core documentation

First of all, thank you for thinking about contributing to the docs! This is especially valuable while you're still new to Orchard because your experiences and revelations can help other newcomers a lot. Be sure to check out [the general contribution docs](README.md) first.

The [Orchard Core documentation site](https://docs.orchardcore.net/) is built with [MkDocs](https://www.mkdocs.org/) and served from [Read the docs](https://readthedocs.org/projects/orchardcore/).

!!! info
    Are you looking for information on contributing code? Head over to [here](contributing-code.md) instead.

!!! tip
    If you're looking for ideas on what to improve in the Orchard Core documentation, check out the [documentation issues](https://github.com/OrchardCMS/OrchardCore/labels/documentation).

## The User Manual and the Technical Manual

The documentation has two manuals, for two kinds of readers. Before you write, decide which one your change belongs to; a change to Orchard Core often needs both.

| | User Manual | Technical Manual |
| --- | --- | --- |
| **Reader** | Editors, content managers, site builders and administrators who work in the admin screens in the browser. | Developers and IT teams who install, configure, host and extend Orchard Core. |
| **Folder** | `src/docs/user-manual/` | Everything else: `getting-started/`, `guides/`, `topics/`, `reference/`, `releases/`, `contributing/`, `community/` |
| **Content** | One task at a time: where the screen is in the admin menu, numbered steps with the exact labels shown on screen, what each field does, the permission and the feature it needs. | Modules and features, configuration (`appsettings.json`, environment variables), recipes and recipe steps, APIs, extension points, code samples, deployment and hosting. |
| **Never** | Code, JSON, `appsettings.json`, recipes or command lines. When a task needs a developer or IT, say so in one sentence and link the Technical Manual. | Step-by-step admin instructions that duplicate the User Manual: link to it instead. |

### Keep the docs honest

Both manuals describe the current code, not memory or older docs. Before you write or change a page:

- Take menu paths from the module's `AdminMenu.cs` and button and field labels from its Razor views (the text in `T["..."]`), and copy them exactly, case included.
- Write a menu path as one bold run with an arrow between the labels, like **Settings → Security → User Login**: never `>`, `->` or italics, and never one bold run per label.
- Take permission names from the module's permission provider: the role editor shows each permission's description, so write the description.
- Take feature names and ids from the module's `Manifest.cs`, as shown in **Tools → Features**.
- Check defaults, required fields and behavior in the drivers, controllers and settings classes, and on a locally running site when in doubt.
- Describe the current behavior, without "new", "now" or version wording. What changed between versions belongs in the [release notes](../releases/3.0.1.md).

### When you change code

Update the docs in the same pull request as the code:

1. **Admin screens, menus, labels, settings, permissions or features changed?** Update the User Manual page that covers them. `user-manual/getting-started/features.md` lists every feature with its User Manual page; a new feature needs a row there and a section on the right page.
2. **Configuration, APIs, recipes, extension points or behavior for developers changed?** Update the module's page under `reference/modules/` or the matching guide or topic.
3. **A new module or feature?** Document it in both manuals: the Technical Manual page (see [Adding docs for a new extension or topic](#adding-docs-for-a-new-extension-or-topic)) and the User Manual task pages for its admin screens.
4. **Something removed or renamed?** Search both manuals for the old name, menu path and label, and fix every page.

### Writing a User Manual page

Each page starts with front matter, then a title equal to the `title`, a short introduction, and a **Menu / Permission / Feature** table:

```markdown
---
title: Users
description: Add people to the site, reset their passwords, and give them roles.
technical_manual:
  - reference/modules/Users/README.md
---

# Users

Every person who signs in has a user account. This page explains how to add and manage them.

| | |
| --- | --- |
| **Menu** | **Access Control > Users** |
| **Permission** | *Manage security settings and all users* |
| **Feature** | **Users** (`OrchardCore.Users`) |

## Add a user

1. Open **Access Control > Users** and click **Add User**.
2. Type the **User name** and the **Email**.
3. Click **Save**.
```

- Use task headings that start with a verb, such as "Add a user", in sentence case.
- Write one action per step, and bold the exact labels shown on screen. Use **Click** for buttons, **Choose** for lists and menus, **Tick** and **Clear** for checkboxes, and **Type** for text.
- List the related Technical Manual pages in `technical_manual`, as paths relative to `src/docs/`. A build hook (`.scripts/docs/manuals.py`) adds a "For developers and IT" box to the page, a box linking back to it at the top of each of those Technical Manual pages, and a short line about missing menus under the Menu / Permission / Feature table. Don't write these boxes yourself.
- Show the main screens, following the [Screenshots](#screenshots) specification.
- Add the page to the `User Manual` section of the `nav` in `mkdocs.yml`, in the section that matches the job, and use the page title as its nav title.

## Editing documentation pages

First, clone the repository using the command `git clone https://github.com/OrchardCMS/OrchardCore.git` and checkout the `main` branch. You can find the documentation files under the `src/docs` folder.

If you open the Orchard Core solution (`OrchardCore.sln` in the root) in Visual Studio or another IDE then you'll be able to browse the files in the `OrchardCore.Docs` project under the `docs` solution folder. If you use a Markdown editor (IDEs usually have built-in support for it, including Visual Studio) then you'll see all the Markdown formatting and embedded images in a WYSIWYG manner, and links will work too.

- To embed YouTube videos, be sure to tick "Enable privacy-enhanced mode." when generating the embed code (this will create a code that references youtube-nocookie.com).
- The videos of the [Orchard Core YouTube channel](https://www.youtube.com/@OrchardCore) are made in [OrchardCore.Videos](https://github.com/OrchardCMS/OrchardCore.Videos). A page shows one with a `<!-- video:<slug> -->` comment followed by its embed code; keep the comment, because it is how the embed code is updated when the video gets a new version.
- If you rename or move a documentation page, external links to it will break. Please add a redirect from the old URL in the `mkdocs.yml` file, under the `redirect_maps` section.

## Adding docs for a new extension or topic

Do the following if you're adding docs for a newly developed extension, or if you add some other completely new topic (in which case adapt it to the section you add the topic to):

1. Add a folder under `reference/modules` with the same name as the module's project without `OrchardCore.`. E.g. for `OrchardCore.AuditTrail` use `AuditTrail`.
2. Add a `README.md` file to it, following the format of existing such files. There, add an overview of the extension's features, configuration, embed demo videos.
3. Link to the `README.md` file from `reference/modules/README.md`.
4. Link to the `README.md` file from the `mkdocs.yml` file in the repository root.
5. If the module contains content parts, also link the `README.md` file from `reference/modules/ContentParts/README.md`.

## Screenshots

User Manual pages show the screens they describe. Every screenshot follows the same specification, so they look the same on every page, and `.scripts/docs/screenshots.py` takes them all:

| | |
| --- | --- |
| **Window** | 1280x800 at a device scale of 1: the whole browser window, with the admin menu. A long editor may capture the whole page, cut at 1600 pixels. |
| **Look** | Light mode with the Gray admin scheme, `en-US`, the admin menu as on a fresh visit, nothing focused or hovered. |
| **Site** | Set up with the Blog recipe, signed in as `admin`, with the features of the page turned on and sample data so lists aren't empty. No personal data or secrets. |
| **File** | WebP at quality 82, under 150 KB, named after the screen in lowercase kebab case, in the `images` folder next to the page. |
| **Page** | Right after the paragraph that introduces the screen, with alt text that says which screen it is. No arrows, highlights or crops. |

To add or retake screenshots:

1. Run a local site set up with the Blog recipe (see [Running the documentation site locally](#running-the-documentation-site-locally) for the docs themselves).
2. Add the screen to `SHOTS` in `.scripts/docs/screenshots.py`: the image file and the address of the screen.
3. Run `pip install playwright pillow`, then `python .scripts/docs/screenshots.py --url <site address> --password <admin password>`. Add `--only <name>` to take only the shots whose file contains that name.
4. Add the image to the page with `![Alt text](images/<name>.webp)`, and commit it.

When a change alters a screen that has a screenshot, retake it in the same pull request.

## Running the documentation site locally

With MkDocs you can get the full docs.orchardcore.net experience locally too. If you are looking to contribute substantial amount of docs then please do run the site locally to make sure what you write will actually look like it should.

1. Follow the [MkDocs installation guide](https://www.mkdocs.org/user-guide/installation/) to install Python, PIP and MkDocs. If you already have a fresh enough Python installation, you can simply run `pip install mkdocs`.
2. Open a command line in the root of your clone of the repository.
3. Run `python -m pip install -r src/docs/requirements.txt` to install dependencies.
4. Run `python -m mkdocs serve` to start the site. You'll then be able to browse it under <http://127.0.0.1:8000>.

## Submitting a pull request and gathering feedback

This happens in the same way as for code contributions, [see there](contributing-code.md).