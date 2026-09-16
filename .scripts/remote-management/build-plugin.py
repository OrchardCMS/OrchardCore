#!/usr/bin/env python3
"""Build reproducible Pomi plugin and skills archives from the current checkout."""
import argparse
import hashlib
import importlib.util
import json
from pathlib import Path
import shutil
import subprocess
import zipfile

REPO = Path(__file__).resolve().parents[2]


def write_json(path, value):
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(value, indent=2) + '\n', encoding='utf-8', newline='\n')


def archive_tree(root, destination):
    # Stored entries avoid compressor-version differences. Fixed timestamps,
    # permissions and ordering make identical inputs produce identical bytes.
    with zipfile.ZipFile(destination, 'w', compression=zipfile.ZIP_STORED) as archive:
        for path in sorted(root.rglob('*')):
            if path.is_file():
                entry = zipfile.ZipInfo(root.name + '/' + path.relative_to(root).as_posix(), (1980, 1, 1, 0, 0, 0))
                entry.create_system = 3
                entry.external_attr = 0o100644 << 16
                archive.writestr(entry, path.read_bytes())


def build(output, repo=REPO):
    output = Path(output).resolve()
    if output.exists():
        raise ValueError(f'{output} already exists; choose a new output directory')
    output.mkdir(parents=True)
    revision = subprocess.check_output(['git', 'rev-parse', 'HEAD'], cwd=repo, text=True).strip()
    inputs = ['.agents/skills/pomi', '.agents/plugins', '.claude-plugin', '.scripts/remote-management', 'design/artifacts/logos/pomi', 'LICENSE', '.gitattributes']
    dirty = bool(subprocess.check_output(['git', 'status', '--porcelain', '--', *inputs], cwd=repo, text=True).strip())
    marketplace = output / 'pomi-plugin'
    source = repo / '.agents/skills/pomi'
    plugin = marketplace / 'plugins/pomi'
    shutil.copytree(source, plugin, ignore=shutil.ignore_patterns('__pycache__', '*.pyc'))
    manifest = json.loads((plugin / '.codex-plugin/plugin.json').read_text())
    manifest['version'] += '+git.' + revision[:12] + ('.working' if dirty else '')
    write_json(plugin / '.codex-plugin/plugin.json', manifest)
    claude = json.loads((plugin / '.claude-plugin/plugin.json').read_text())
    claude['version'] = manifest['version']
    write_json(plugin / '.claude-plugin/plugin.json', claude)
    skills = sorted((plugin / 'skills').glob('orchardcore-cli*'))
    compatibility = json.loads((plugin / 'compatibility.json').read_text())
    digest = hashlib.sha256(json.dumps(compatibility, sort_keys=True).encode())
    digest.update(Path(__file__).read_bytes())
    for path in sorted(plugin.rglob('*')):
        if path.is_file():
            digest.update(path.relative_to(plugin).as_posix().encode() + b'\0' + path.read_bytes() + b'\0')
    metadata = {'packageVersion': manifest['version'], 'sourceCommit': revision,
                'sourceDirty': dirty, 'contentSha256': digest.hexdigest(), 'skillCount': len(skills),
                'compatibility': compatibility}
    write_json(plugin / 'package-metadata.json', metadata)
    # Reuse the checked-in catalogs, adjusting only the extracted location and
    # name so a downloaded snapshot can coexist with the Git marketplace.
    for path in ('.agents/plugins/marketplace.json', '.claude-plugin/marketplace.json'):
        catalog = json.loads((repo / path).read_text())
        catalog['name'] = 'pomi-download'
        if 'interface' in catalog:
            catalog['interface']['displayName'] = 'Pomi download'
        entry = catalog['plugins'][0]
        entry['source'] = ({'source': 'local', 'path': './plugins/pomi'}
                           if isinstance(entry['source'], dict) else './plugins/pomi')
        write_json(marketplace / path, catalog)
    (marketplace / 'README.md').write_text('''# Install the Pomi plugin

Keep this extracted directory in a stable location.

Codex CLI:

```bash
codex plugin marketplace add /absolute/path/to/pomi-plugin
codex plugin add pomi@pomi-download
```

Start a new Codex task after installation. For Claude Code, load the plugin
for a session with `claude --plugin-dir /absolute/path/to/pomi-plugin/plugins/pomi`.
For persistent Claude Code installation, run `/plugin marketplace add /absolute/path/to/pomi-plugin`
and `/plugin install pomi@pomi-download` inside Claude Code.

The [plugin instructions](plugins/pomi/README.md) link to all skills.
For Git installation, use the Orchard Core repository marketplace instead.
Downloading alone does not register skills.
''', encoding='utf-8', newline='\n')
    spec = importlib.util.spec_from_file_location('plugin_links', Path(__file__).with_name('verify-plugin-links.py'))
    links = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(links)
    validation = links.verify(plugin)
    if validation['failures']:
        raise ValueError(validation['failures'])
    skills_only = output / 'pomi-skills'
    shutil.copytree(plugin / 'skills', skills_only / 'skills')
    shutil.copyfile(plugin / 'LICENSE', skills_only / 'LICENSE')
    write_json(skills_only / 'package-metadata.json', metadata)
    plugin_zip = output / 'pomi-plugin.zip'
    skills_zip = output / 'pomi-skills.zip'
    archive_tree(marketplace, plugin_zip)
    archive_tree(skills_only, skills_zip)
    artifacts = {}
    identity = revision if not dirty else revision + '-working-' + metadata['contentSha256'][:12]
    for archive in (plugin_zip, skills_zip):
        specific = archive.with_name(f'{archive.stem}-{identity}.zip')
        shutil.copyfile(archive, specific)
        artifacts[archive.name] = {'sha256': hashlib.sha256(archive.read_bytes()).hexdigest(), 'file': specific.name}
    write_json(output / 'package-metadata.json', {**metadata, 'artifacts': artifacts})
    (output / 'SHA256SUMS').write_text(''.join(f'{info["sha256"]}  {name}\n{info["sha256"]}  {info["file"]}\n' for name, info in artifacts.items()), encoding='utf-8', newline='\n')
    return {'plugin': str(plugin), 'archive': str(plugin_zip), 'skillsArchive': str(skills_zip),
            'skills': len(skills), 'metadata': metadata}


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('output', type=Path, help='New directory for the plugin, skills and ZIP files')
    args = parser.parse_args()
    print(json.dumps(build(args.output)))
