#!/usr/bin/env python3
"""Verify Markdown file links and heading anchors in an extracted CLI plugin.

Runs without the repository or network. Relative links must stay inside the
plugin and resolve to packaged files. External manuals must use an official
GitHub URL. This checks their URL shape, not remote HTTP availability.
"""
import argparse
import json
from pathlib import Path
import re
import urllib.parse


def markdown_links(text):
    # Our skill links use inline Markdown syntax. Ignore command/code examples.
    prose = re.sub(r'^```[^\n]*\n.*?^```\s*$', '', text, flags=re.M | re.S)
    return re.findall(r'\[[^\]\n]+\]\(([^)\s]+)\)', prose)


def heading_ids(text):
    seen = {}
    anchors = set()
    for heading in re.findall(r'^#{1,6}\s+(.+?)\s*#*$', text, flags=re.M):
        slug = re.sub(r'[^\w\- ]', '', heading.lower()).replace(' ', '-')
        count = seen.get(slug, 0)
        seen[slug] = count + 1
        anchors.add(slug + (f'-{count}' if count else ''))
    return anchors


def verify(root):
    root = root.resolve()
    failures = []
    local_count = 0
    manual_count = 0
    files = sorted(root.rglob('*.md'))
    if not (root / '.codex-plugin/plugin.json').is_file():
        failures.append('Missing plugin manifest')
    for file in files:
        for link in markdown_links(file.read_text()):
            url = urllib.parse.urlsplit(link)
            if url.scheme or url.netloc:
                if 'src/docs/' in url.path:
                    if (url.scheme != 'https' or url.netloc != 'github.com'
                            or not re.fullmatch(r'/OrchardCMS/OrchardCore/blob/(?:main|v[^/]+|[0-9a-f]{40})/src/docs/.+', url.path)):
                        failures.append(f'{file.relative_to(root)}: manual is not official: {link}')
                    manual_count += 1
                continue
            target = (file.parent / urllib.parse.unquote(url.path)).resolve() if url.path else file
            if not target.is_relative_to(root):
                failures.append(f'{file.relative_to(root)}: link escapes plugin: {link}')
            elif not target.is_file():
                failures.append(f'{file.relative_to(root)}: missing linked file: {link}')
            elif url.fragment and target.suffix == '.md' and urllib.parse.unquote(url.fragment) not in heading_ids(target.read_text()):
                failures.append(f'{file.relative_to(root)}: missing heading: {link}')
            local_count += 1
    if not local_count:
        failures.append('No local Markdown links found')
    return {'markdownFiles': len(files), 'localLinks': local_count,
            'versionedManualLinks': manual_count, 'failures': failures}


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('plugin', type=Path, help='Extracted plugin root')
    args = parser.parse_args()
    report = verify(args.plugin)
    print(json.dumps(report, indent=2))
    raise SystemExit(bool(report['failures']))
