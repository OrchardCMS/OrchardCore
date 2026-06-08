---
name: orchardcore-docs-writer
description: Authors OrchardCore documentation — MkDocs Material pages, module README docs, nav entries, admonitions, tabbed content, and redirects. Use when the user needs to add or edit a docs page, document a new module, build/serve the docs site, or wire a page into the navigation.
---

# OrchardCore Docs Writer

This skill guides you through writing OrchardCore documentation following project conventions.

Docs are **MkDocs** + **Material for MkDocs**. Config: `mkdocs.yml` at repo root; pages live under `src/docs/`. The nav is **explicit** in `mkdocs.yml` — a new `.md` file is invisible until you add it to `nav:`.

## Where pages live

| Section | Path under `src/docs/` |
|---------|------------------------|
| Home | `README.md` |
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

Use privacy mode — `https://www.youtube-nocookie.com/embed/<id>`.

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
