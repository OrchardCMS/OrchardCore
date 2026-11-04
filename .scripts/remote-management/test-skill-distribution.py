#!/usr/bin/env python3
"""Verify built website downloads, raw Markdown, metadata and reproducibility."""
import hashlib
import importlib.util
import json
from pathlib import Path
import re
import subprocess
import sys
import tempfile
import zipfile

REPO = Path(__file__).resolve().parents[2]
site = Path(sys.argv[1]).resolve()
metadata = json.loads((site / 'downloads/package-metadata.json').read_text())
assert metadata['sourceCommit'] == subprocess.check_output(['git', 'rev-parse', 'HEAD'], cwd=REPO, text=True).strip()
assert metadata['skillCount'] == 10
assert metadata['compatibility']['managementProtocolMajor'] == 1

with zipfile.ZipFile(site / 'downloads/pomi-plugin.zip') as plugin, zipfile.ZipFile(site / 'downloads/pomi-skills.zip') as skills:
    root = 'pomi-plugin/plugins/pomi/'
    manifest = json.loads(plugin.read(root + '.codex-plugin/plugin.json'))
    claude = json.loads(plugin.read(root + '.claude-plugin/plugin.json'))
    assert manifest['version'] == claude['version'] == metadata['packageVersion']
    assert manifest['name'] == claude['name'] == 'pomi'
    assert root + 'agents/pomi.agent.md' in plugin.namelist()
    assert not any('/agents/' in name for name in skills.namelist()), 'Skills-only ZIP must not ship agent profiles'
    source = REPO / '.agents/skills/pomi'
    for file in source.rglob('*'):
        if not file.is_file() or '__pycache__' in file.parts:
            continue
        relative = file.relative_to(source).as_posix()
        if relative in ('.codex-plugin/plugin.json', '.claude-plugin/plugin.json'):
            original = json.loads(file.read_text())
            packaged = json.loads(plugin.read(root + relative))
            assert packaged.pop('version').startswith(original.pop('version') + '+git.')
            assert packaged == original, relative
        else:
            assert plugin.read(root + relative) == file.read_bytes(), relative
    for icon in ('composerIcon', 'logo', 'logoDark'):
        assert plugin.read(root + manifest['interface'][icon].removeprefix('./')).startswith(b'\x89PNG')
    for package, location in [(plugin, root), (skills, 'pomi-skills/')]:
        stored = json.loads(package.read(location + 'package-metadata.json'))
        assert all(stored[key] == value for key, value in metadata.items() if key != 'artifacts')
    for market in ('.agents/plugins/marketplace.json', '.claude-plugin/marketplace.json'):
        entry = json.loads(plugin.read('pomi-plugin/' + market))['plugins'][0]
        source = entry['source']['path'] if isinstance(entry['source'], dict) else entry['source']
        assert source == './plugins/pomi'
    count = 0
    canonical = REPO / '.agents/skills/pomi'
    markdown_files = [file for directory in ('skills', 'agents')
                      for file in (canonical / directory).rglob('*.md')]
    for file in sorted(markdown_files):
        rel = file.relative_to(REPO / '.agents/skills/pomi').as_posix()
        expected = file.read_bytes()
        assert plugin.read(root + rel) == expected, rel
        if rel.startswith('skills/'):
            assert skills.read('pomi-skills/' + rel) == expected, rel
        assert (site / 'agents/raw' / rel).read_bytes() == expected, rel
        assert not (site / 'agents/raw' / rel.removesuffix('.md') / 'index.html').exists(), rel
        rendered = site / 'agents' / rel.removesuffix('.md') / 'index.html'
        # Generated references and SKILL pages retain directory URLs.
        if not rendered.is_file():
            rendered = site / 'agents' / rel.replace('.md', '.html')
        assert rendered.is_file(), rendered
        html = rendered.read_text()
        assert 'Raw Markdown' in html and 'All skills and installation' in html
        count += 1
    with tempfile.TemporaryDirectory(prefix='pomi-distribution-') as temporary:
        plugin.extractall(temporary)
        extracted = Path(temporary) / root
        subprocess.run([sys.executable, str(REPO / '.scripts/remote-management/verify-plugin-links.py'), str(extracted)], check=True)
        # Compare installed directory layouts for Copilot and standalone Claude.
        for agent_dir in ('.github/skills', '.claude/skills'):
            for member in skills.namelist():
                prefix = 'pomi-skills/skills/'
                if member.startswith(prefix):
                    target = Path(temporary) / agent_dir / member.removeprefix(prefix)
                    target.parent.mkdir(parents=True, exist_ok=True)
                    target.write_bytes(skills.read(member))
            target = Path(temporary) / agent_dir / 'orchardcore-cli-content-items/SKILL.md'
            assert (target.parent / '../orchardcore-cli/references/shared-rules.md').resolve().is_file()

checksums = (site / 'downloads/SHA256SUMS').read_text()
for alias, artifact in metadata['artifacts'].items():
    content = (site / 'downloads' / alias).read_bytes()
    assert content == (site / 'downloads' / artifact['file']).read_bytes()
    assert hashlib.sha256(content).hexdigest() == artifact['sha256']
    assert f'{artifact["sha256"]}  {artifact["file"]}\n' in checksums
page = (site / 'agents/index.html').read_text()
assert '<!-- pomi-' not in page
assert metadata['sourceCommit'] in page
assert '../downloads/pomi-plugin.zip' in page and '../downloads/pomi-skills.zip' in page
assert 'raw/skills/orchardcore-cli/SKILL.md' in page
download_links = re.findall(r'href="([^"]*downloads/[^"]*)"', page)
assert download_links and all(link.startswith('../downloads/') for link in download_links)

spec = importlib.util.spec_from_file_location('builder', REPO / '.scripts/remote-management/build-plugin.py')
builder = importlib.util.module_from_spec(spec)
spec.loader.exec_module(builder)
with tempfile.TemporaryDirectory(prefix='pomi-reproducibility-') as temporary:
    first = Path(temporary) / 'first'
    second = Path(temporary) / 'second'
    builder.build(first)
    builder.build(second)
    for name in ('pomi-plugin.zip', 'pomi-skills.zip', 'package-metadata.json', 'SHA256SUMS'):
        assert (first / name).read_bytes() == (second / name).read_bytes(), name
        assert (first / name).read_bytes() == (site / 'downloads' / name).read_bytes(), f'Site differs from builder: {name}'
print(f'Distribution passed: {count} raw/rendered Markdown files, both ZIPs, branding, manifests, version metadata, checksums, relative URLs and reproducibility.')
