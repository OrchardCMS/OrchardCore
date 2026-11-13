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
- Take permission names from the module's permission provider: the role editor shows each permission's description, so write the description.
- Take feature names and ids from the module's `Manifest.cs`, as shown in **Tools > Features**.
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
- Add the page to the `User Manual` section of the `nav` in `mkdocs.yml`, in the section that matches the job, and use the page title as its nav title.

## Editing documentation pages

First, clone the repository using the command `git clone https://github.com/OrchardCMS/OrchardCore.git` and checkout the `main` branch. You can find the documentation files under the `src/docs` folder.

If you open the Orchard Core solution (`OrchardCore.sln` in the root) in Visual Studio or another IDE then you'll be able to browse the files in the `OrchardCore.Docs` project under the `docs` solution folder. If you use a Markdown editor (IDEs usually have built-in support for it, including Visual Studio) then you'll see all the Markdown formatting and embedded images in a WYSIWYG manner, and links will work too.

- To embed YouTube videos, be sure to tick "Enable privacy-enhanced mode." when generating the embed code (this will create a code that references youtube-nocookie.com).
- If you rename or move a documentation page, external links to it will break. Please add a redirect from the old URL in the `mkdocs.yml` file, under the `redirect_maps` section.

## Adding docs for a new extension or topic

Do the following if you're adding docs for a newly developed extension, or if you add some other completely new topic (in which case adapt it to the section you add the topic to):

1. Add a folder under `reference/modules` with the same name as the module's project without `OrchardCore.`. E.g. for `OrchardCore.AuditTrail` use `AuditTrail`.
2. Add a `README.md` file to it, following the format of existing such files. There, add an overview of the extension's features, configuration, embed demo videos.
3. Link to the `README.md` file from `reference/modules/README.md`.
4. Link to the `README.md` file from the `mkdocs.yml` file in the repository root.
5. If the module contains content parts, also link the `README.md` file from `reference/modules/ContentParts/README.md`.

## Running the documentation site locally

With MkDocs you can get the full docs.orchardcore.net experience locally too. If you are looking to contribute substantial amount of docs then please do run the site locally to make sure what you write will actually look like it should.

1. Follow the [MkDocs installation guide](https://www.mkdocs.org/user-guide/installation/) to install Python, PIP and MkDocs. If you already have a fresh enough Python installation, you can simply run `pip install mkdocs`.
2. Open a command line in the root of your clone of the repository.
3. Run `python -m pip install -r src/docs/requirements.txt` to install dependencies.
4. Run `python -m mkdocs serve` to start the site. You'll then be able to browse it under <http://127.0.0.1:8000>.

## Submitting a pull request and gathering feedback

This happens in the same way as for code contributions, [see there](contributing-code.md).