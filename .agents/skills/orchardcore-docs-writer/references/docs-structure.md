# Docs structure reference

## Toolchain

- **MkDocs** with **Material for MkDocs** (`mkdocs-material`).
- Config: `mkdocs.yml` at repo root (`site` settings, `theme`, `markdown_extensions`, `plugins`, `nav`).
- `docs_dir: src/docs`. Custom theme overrides in `src/docs/theme`.
- Dependencies: `src/docs/requirements.txt`:

```
mkdocs>=1.6.1
mkdocs-material>=9.7.6
mkdocs-git-authors-plugin>=0.10.0
mkdocs-git-revision-date-localized-plugin>=1.5.3
pymdown-extensions>=10.21.3
mkdocs-exclude>=1.0.2
mkdocs-redirects>=1.2.1
```

Commands (Python 3.11+):

```bash
pip install -r src/docs/requirements.txt
python -m mkdocs serve   # local preview at http://127.0.0.1:8000
python -m mkdocs build   # static output
```

## Directory map (`src/docs/`)

| Folder | Contents |
|--------|----------|
| `README.md` | home / "About Orchard Core" |
| `assets/` | images, logos |
| `community/` | contributors, owners |
| `contributing/` | contributing-code.md, contributing-documentation.md, code review, issues |
| `getting-started/` | install, CMS setup, theme, dev tools |
| `guides/` | 20+ tutorials, each `guides/<name>/README.md` |
| `reference/` | API + module reference |
| `reference/modules/<Name>/README.md` | one folder per module (~100), no `OrchardCore.` prefix |
| `reference/glossary/`, `branding/`, `libraries/` | supporting reference |
| `releases/` | per-version release notes |
| `topics/` | deep-dive topics (content management, security, workflows, …) |
| `theme/` | Material theme customizations |

## Nav anatomy

`nav:` in `mkdocs.yml` is an explicit tree; each leaf is `Title: path`. Excerpt:

```yaml
nav:
  - About Orchard Core: README.md
  - Getting started:
      - Development Tools: getting-started/development-tools.md
      - Create a CMS Web application: getting-started/README.md
  - Guides:
      - Follow the Guides: guides/README.md
  - Key Topics:
      - Manage your Content: topics/content-management/README.md
  - Reference:
      - Modules:
          - Overview: reference/README.md
          - CMS Modules:
              - Content Types: reference/modules/ContentTypes/README.md
              - Content Parts:
                  - Title: reference/modules/Title/README.md
          - Core Modules:
              - Display Management: reference/modules/DisplayManagement/README.md
```

Add a page by inserting `- Page Title: relative/path.md` under the correct parent. Pages absent from nav build with a warning and aren't reachable via the menu.

## Markdown extensions in use

| Extension | Syntax |
|-----------|--------|
| Admonition | `!!! note` / `!!! warning` / `!!! tip` + indented body |
| Superfences | language-tagged code fences |
| Tabbed (alternate style) | `=== "Tab title"` then indented block |
| Snippets | embed file fragments |
| Tasklist | `- [ ]` / `- [x]` |
| TOC | auto headings with anchor permalinks |

Admonition example:

```markdown
!!! info
    Looking for code contribution info? See contributing-code.md.
```

Tabbed example (blank line after the marker, 4-space indent):

```markdown
=== "App.razor.css"

    ```css
    html, body { font-family: Helvetica, Arial, sans-serif; }
    ```
```

## Module header convention

```markdown
# <Display Name> (`OrchardCore.<Id>`)
```

Examples: `# Title (`OrchardCore.Title`)`, `# Content Types (`OrchardCore.ContentTypes`)`, `# Audit Trail (`OrchardCore.AuditTrail`)`.

## Adding a new module's docs

1. `src/docs/reference/modules/<Name>/README.md` — header + features, configuration, recipe steps, placement, migrations, demo videos as relevant.
2. Link from `reference/modules/README.md`.
3. `nav:` entry under the appropriate category in `mkdocs.yml`.
4. Content part? also link from `reference/modules/ContentParts/README.md`.

No manifest auto-linking — all references are manual.

## Redirects

When moving/renaming a page, preserve inbound links via the `redirects` plugin in `mkdocs.yml`:

```yaml
plugins:
  - redirects:
      redirect_maps:
        'old/path/README.md': 'new/path/README.md'
```

## Contributing notes

`src/docs/contributing/contributing-documentation.md`:
- Clone `main`, edit `src/docs/`, open the `OrchardCore.Docs` project in `OrchardCore.sln` for IDE support.
- Use `youtube-nocookie.com` for video embeds.
- PR process matches code contributions.
