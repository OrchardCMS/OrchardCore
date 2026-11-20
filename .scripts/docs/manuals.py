"""
MkDocs hook that links the User Manual and the Technical Manual to each other.

Every User Manual page (under `user-manual/`) lists the Technical Manual pages it relates to in its
front matter:

    ---
    technical_manual:
      - reference/modules/Users/README.md
    ---

From that list, the hook:

- appends a "For developers and IT" box to the User Manual page, linking to those Technical Manual pages;
- adds a "User Manual" box to each of those Technical Manual pages, after its first paragraph, linking back
  to every User Manual page that lists it. A User Manual page can opt out of these back-links with
  `backlink: false` in its front matter; section landing pages and general pages always do.

It also:

- adds a short "Missing a menu or button?" line under the Menu / Permission / Feature table of User Manual pages;
- boosts User Manual pages in search, and keeps the boxes out of the search index;
- marks User Manual pages so the theme can show which manual a page belongs to.

The links are only written once, in the User Manual front matter, so the two manuals never drift apart.
"""

from __future__ import annotations

import logging
import posixpath
import re

import yaml

log = logging.getLogger("mkdocs.hooks.manuals")

USER_MANUAL_PREFIX = "user-manual/"

# General User Manual pages that would only add noise as back-links on technical pages.
NO_BACKLINK = {
    "user-manual/index.md",
    "user-manual/glossary.md",
    "user-manual/troubleshooting.md",
    "user-manual/getting-started/roles-and-permissions.md",
    "user-manual/getting-started/training-paths.md",
    "user-manual/getting-started/features.md",
}

ROLES_PAGE = "user-manual/getting-started/roles-and-permissions.md"

# Technical Manual page kinds, by path prefix, shown next to each link.
KINDS = [
    ("reference/modules/", "module reference"),
    ("reference/core/", "reference"),
    ("reference/", "reference"),
    ("guides/", "guide"),
    ("topics/", "topic"),
    ("getting-started/", "getting started"),
]

_FRONT_MATTER = re.compile(r"^---\s*\n(.*?)\n---\s*\n", re.DOTALL)
_HEADING = re.compile(r"^#\s+(.+?)\s*$", re.MULTILINE)
_INFO_TABLE = re.compile(r"(?m)^\|[^\n]*\*\*Permission\*\*[^\n]*\|[ \t]*$")

# technical page src_uri -> [(user manual src_uri, title)]
_backlinks: dict[str, list[tuple[str, str]]] = {}
# any page src_uri -> title
_titles: dict[str, str] = {}


def _read_front_matter(path: str) -> tuple[dict, str]:
    with open(path, encoding="utf-8-sig") as file:
        text = file.read()

    match = _FRONT_MATTER.match(text)
    if not match:
        return {}, text

    try:
        meta = yaml.safe_load(match.group(1)) or {}
    except yaml.YAMLError:
        meta = {}

    return meta, text[match.end():]


def _title_of(meta: dict, body: str, src_uri: str) -> str:
    if isinstance(meta.get("title"), str):
        return meta["title"]

    heading = _HEADING.search(body)
    if heading:
        # Drop the module id in "Title (`OrchardCore.Title`)" headings.
        return re.sub(r"\s*\(`[^`]+`\)\s*$", "", heading.group(1)).strip()

    return posixpath.splitext(posixpath.basename(src_uri))[0]


def _kind_of(src_uri: str) -> str:
    for prefix, kind in KINDS:
        if src_uri.startswith(prefix):
            return kind
    return ""


def _relative(from_uri: str, to_uri: str) -> str:
    return posixpath.relpath(to_uri, posixpath.dirname(from_uri) or ".")


def _excluded_box(lines: list[str]) -> str:
    # data-search-exclude keeps the box out of the search index, so results show the page's own text.
    return '<div class="oc-crosslink" data-search-exclude markdown>\n\n' + "\n".join(lines) + "\n\n</div>\n"


def on_files(files, config):
    _backlinks.clear()
    _titles.clear()

    for file in files:
        if not file.src_uri.endswith(".md"):
            continue

        meta, body = _read_front_matter(file.abs_src_path)
        _titles[file.src_uri] = _title_of(meta, body, file.src_uri)

        if not file.src_uri.startswith(USER_MANUAL_PREFIX):
            continue

        if (file.src_uri in NO_BACKLINK or file.src_uri.endswith("/index.md") or meta.get("backlink") is False):
            continue

        for target in meta.get("technical_manual") or []:
            _backlinks.setdefault(target, []).append((file.src_uri, None))

    for target, sources in _backlinks.items():
        if target not in _titles:
            log.warning("User Manual pages %s link to the missing Technical Manual page '%s'.", [s for s, _ in sources], target)
        _backlinks[target] = sorted({(source, _titles.get(source, source)) for source, _ in sources}, key=lambda item: item[1])

    return files


def _add_access_note(markdown: str, src_uri: str) -> str:
    """Adds one line under the page's Menu / Permission / Feature table."""
    if src_uri == ROLES_PAGE:
        return markdown

    match = _INFO_TABLE.search(markdown)
    if not match:
        return markdown

    # Find the end of the table the row belongs to.
    end = match.end()
    while True:
        next_line_end = markdown.find("\n", end + 1)
        next_line = markdown[end + 1: next_line_end if next_line_end != -1 else None]
        if not next_line.startswith("|"):
            break
        end = next_line_end if next_line_end != -1 else len(markdown)

    link = _relative(src_uri, ROLES_PAGE)
    note = (
        "\n\nMissing a menu or a button? It depends on the features turned on for your site and on your role's "
        f"permissions: see [Roles and permissions]({link}).\n{{ .oc-access-note }}"
    )
    return markdown[:end] + note + markdown[end:]


def _technical_links(src_uri: str, targets: list[str]) -> list[str]:
    seen = set()
    links = []
    for target in targets:
        if target not in _titles or target in seen:
            continue
        seen.add(target)
        kind = _kind_of(target)
        suffix = f" ({kind})" if kind else ""
        links.append(f"    - [{_titles[target]}]({_relative(src_uri, target)}){suffix}")
    return links


def on_page_markdown(markdown, page, config, files):
    src_uri = page.file.src_uri

    if src_uri.startswith(USER_MANUAL_PREFIX):
        page.meta.setdefault("search", {}).setdefault("boost", 2)
        page.meta["manual"] = "user"

        markdown = _add_access_note(markdown, src_uri)

        links = _technical_links(src_uri, page.meta.get("technical_manual") or [])
        if not links:
            return markdown

        box = _excluded_box([
            '!!! abstract "For developers and IT"',
            "    The Technical Manual explains how to install, configure and extend what this page describes:",
            "",
            *links,
        ])
        return markdown.rstrip() + "\n\n" + box

    sources = _backlinks.get(src_uri)
    if not sources:
        return markdown

    page.meta["manual"] = "technical"

    links = ", ".join(f"[{title}]({_relative(src_uri, source)})" for source, title in sources)
    box = _excluded_box([
        '!!! tip "Using this from the admin screens?"',
        f"    The User Manual explains it step by step, without code: {links}.",
    ])

    # Keep the page heading and its first paragraph first.
    heading = _HEADING.search(markdown)
    if not heading or markdown[: heading.start()].strip():
        return box + "\n" + markdown

    rest = markdown[heading.end():]
    paragraph = re.match(r"\s*\n((?:(?![#|!<`>\-*]|\d+\.)[^\n]+\n)+)", rest)
    if paragraph:
        split = heading.end() + paragraph.end()
    else:
        split = heading.end()

    return markdown[:split].rstrip() + "\n\n" + box + "\n" + markdown[split:].lstrip("\n")
