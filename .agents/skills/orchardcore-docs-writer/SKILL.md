---
name: orchardcore-docs-writer
description: Authors OrchardCore documentation — User Manual pages for people who use the admin, Technical Manual pages and module README docs for developers, nav entries, admonitions, tabbed content, and redirects. Use when the user needs to add or edit a docs page, document a new module or feature, keep the docs in sync with a code change, build/serve the docs site, or wire a page into the navigation.
---

# OrchardCore Docs Writer

This skill guides you through writing OrchardCore documentation following project conventions.

Docs are **MkDocs** + **Material for MkDocs**. Config: `mkdocs.yml` at repo root; pages live under `src/docs/`. The nav is **explicit** in `mkdocs.yml` — a new `.md` file is invisible until you add it to `nav:`.

## Two manuals

The site has two manuals. A code change often needs both; update them in the same pull request as the code.

| | User Manual | Technical Manual |
|---|---|---|
| Folder | `src/docs/user-manual/` | everything else under `src/docs/` |
| Reader | Editors, site builders, administrators working in the admin | Developers and IT teams |
| Content | Tasks: menu path, numbered steps with the exact on-screen labels, fields, permission, feature | Modules, configuration, recipes, APIs, extension points, code |
| Never | Code, JSON, `appsettings.json`, recipes | Admin click-by-click steps (link the User Manual instead) |

**Be honest**: take menu paths from `AdminMenu.cs`, labels from the Razor views (`T["..."]`, exact case), permission descriptions from the permission provider, feature names and ids from `Manifest.cs`, and check behavior in drivers, controllers and settings, or on a running site. Never write from memory or from older docs.

**User Manual page template** (see `src/docs/contributing/contributing-documentation.md`):

```markdown
---
title: Users
description: Add people to the site, reset their passwords, and give them roles.
technical_manual:
  - reference/modules/Users/README.md
---

# Users

Short introduction.

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

- Sentence-case task headings; one action per step; **Click** buttons, **Choose** from lists, **Tick**/**Clear** checkboxes, **Type** text.
- `technical_manual` paths are relative to `src/docs/`. The hook `.scripts/docs/manuals.py` turns them into the boxes linking the two manuals; never write those boxes by hand.
- Add the page to the `User Manual` section of `nav` in `mkdocs.yml`, under the job it belongs to, with the page title as nav title. A new feature also needs its row in `user-manual/getting-started/features.md`.
- When a label, menu path or feature is renamed or removed, search `src/docs/user-manual/` for the old text and fix every page.

## Where pages live

| Section | Path under `src/docs/` |
|---------|------------------------|
| Home | `README.md` |
| User Manual | `user-manual/<section>/<page>.md` |
| Technical Manual landing | `technical-manual/index.md` |
| Getting started | `getting-started/` |
| Guides (tutorials) | `guides/<name>/README.md` |
| Key topics | `topics/<topic>/README.md` |
| Module reference | `reference/modules/<Name>/README.md` |
| Releases | `releases/` |
| Contributing | `contributing/` |

Module name in the path drops the `OrchardCore.` prefix: `OrchardCore.Title` → `reference/modules/Title/README.md`.

## Workflow A: add a page to an existing area

### Step 1: Write the Markdown

Create the file under the right section. For a module, use the header convention:

```markdown
# Title (`OrchardCore.Title`)

Short description of what the module does.

## Section
...
```

### Step 2: Add it to the nav

Edit `mkdocs.yml` `nav:` under the matching parent. Format `- Page Title: path/to/file.md`:

```yaml
nav:
  - Reference:
      - Modules:
          - Core Modules:
              - Display Management: reference/modules/DisplayManagement/README.md
              - My Module: reference/modules/MyModule/README.md   # added
```

### Step 3: Build / preview

```bash
pip install -r src/docs/requirements.txt   # first time (Python 3.11+)
python -m mkdocs serve                      # http://127.0.0.1:8000
python -m mkdocs build                      # static site
```

## Workflow B: document a new module

1. Create `src/docs/reference/modules/<Name>/README.md` (no `OrchardCore.` prefix) with the `# Name (`OrchardCore.Name`)` header.
2. Link it from `src/docs/reference/modules/README.md` (the module index).
3. Add a `nav:` entry in `mkdocs.yml` under the right category (CMS Modules / Core Modules / Content Parts / …).
4. If it's a content part, also link from `reference/modules/ContentParts/README.md`.

Module docs are **not** auto-discovered from the manifest — every link is manual.

## Markdown conventions

### Admonitions

```markdown
!!! note
    A neutral aside.

!!! warning
    Something that can bite.

!!! tip
    A helpful suggestion.
```

### Tabbed content

```markdown
=== "App.razor"

    ```razor
    <!DOCTYPE html>
    ```

=== "_Imports.razor"

    ```csharp
    @using System.Net.Http
    ```
```

### Code fences

Always tag the language: ` ```csharp `, ` ```json `, ` ```bash `, ` ```liquid `.

### YouTube embeds

Host videos on YouTube, and never commit a video or its assets to the repository. Use privacy mode — `https://www.youtube-nocookie.com/embed/<id>`.

## Docs theme

The docs theme lives in `src/docs/theme/` (template overrides), `src/docs/assets/css/` and `src/docs/assets/js/`. `assets/css/theme.css` holds its tokens: every color and radius is a `--oc-*` custom property defined there for the light and dark (`slate`) schemes, and the other stylesheets use the tokens.

When you change the theme, or the HTML of a page such as the cards on the home page, avoid the habits that make a design look generated:

1. No thick colored left (or top) border on cards, callouts or tables for emphasis: use a plain border, a light tint or a stronger heading.
2. No dotted or dashed dividers or borders added for decoration.
3. No gradient text, and no purple-to-blue gradient.
4. No sparkles as shorthand for AI.
5. No emoji in the theme, headings or buttons.
6. No badges or pills unless they carry a status the reader acts on.
7. No row of stat cards at the top of a page.
8. No cards inside cards, and don't wrap every section in a card.
9. No icon in front of every heading or list item.
10. No glassmorphism, heavy blur, glow or colored shadows.
11. No labels, captions or helper text that restate the obvious.
12. No grid of identical cards for content that isn't a collection.
13. Sentence case headings, and no ALL-CAPS eyebrow or label above a section.
14. No hover effect that moves the layout (scale or shift).
15. Colors, type and radii come from the tokens in `theme.css`, never one-off values or inline styles.
16. No fake live indicators, such as pulsing dots, for things that aren't real-time.

Check the result at phone and desktop widths, in light and dark mode.

## Moving / renaming a page

Add a redirect so old links survive. In `mkdocs.yml` under `plugins: redirects: redirect_maps:`:

```yaml
redirect_maps:
  'old/path/README.md': 'new/path/README.md'
```

## Quick Reference

### Toolchain

| Item | Value |
|------|-------|
| Generator | MkDocs + Material for MkDocs |
| Config | `mkdocs.yml` (root) |
| Docs dir | `src/docs` |
| Deps | `src/docs/requirements.txt` |
| Serve | `python -m mkdocs serve` |
| Build | `python -m mkdocs build` |

### Enabled Markdown extensions

Admonitions (`!!! type`), `pymdownx.superfences` (code), `pymdownx.tabbed` (`=== "Tab"`), `pymdownx.snippets` (file embed), `pymdownx.tasklist`, `toc` with permalinks.

### Module header

```markdown
# <Display Name> (`OrchardCore.<Id>`)
```

## Gotchas

- A new page must be added to `nav:` in `mkdocs.yml` or it won't appear (build warns about pages not in nav).
- Tabbed content needs a blank line after `=== "Label"` and 4-space indentation of the block.
- Use repo-relative Markdown links between docs pages (`../Placement/README.md`), not absolute URLs.
- When moving a page, add a `redirect_maps` entry — broken inbound links otherwise.
- Match the module-header convention exactly; the docs are scanned for consistency.

## References

- `references/docs-structure.md` — full directory map, nav anatomy, extensions, redirects
- `src/docs/contributing/contributing-documentation.md` (repo) — official guide
- `mkdocs.yml` (repo root) — config + nav
- `AGENTS.md` (repo root) — build commands
