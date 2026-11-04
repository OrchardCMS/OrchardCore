#!/usr/bin/env python3
"""Validate the checked-in marketplace without running the package builder.

Pass --codex /path/to/codex to additionally install into an isolated profile.
"""
import argparse
import json
import os
from pathlib import Path
import re
import shutil
import subprocess
import sys
import tempfile

REPO = Path(__file__).resolve().parents[2]
parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument('--codex', type=Path)
args = parser.parse_args()
source = REPO / '.agents/skills/pomi'
expected = {'orchardcore-cli' + suffix for suffix in (
    '', '-automation', '-content-definitions', '-content-items', '-graphql',
    '-media', '-menus', '-settings', '-templates', '-themes')}
assert {p.parent.name for p in source.glob('skills/*/SKILL.md')} == expected
assert not list((REPO / '.agents/skills').glob('orchardcore-cli*')), 'Duplicate canonical skills'
assert not any(p.is_symlink() for p in source.rglob('*')), 'Plugin must not depend on external symlinks'
manifest = json.loads((source / '.codex-plugin/plugin.json').read_text())
claude = json.loads((source / '.claude-plugin/plugin.json').read_text())
assert manifest['name'] == claude['name'] == source.name == 'pomi'
assert manifest['version'] == claude['version']
assert manifest['skills'] == './skills/'
agents = list(source.glob('agents/*.agent.md'))
assert [path.name for path in agents] == ['pomi.agent.md'], 'Ship one canonical designer agent'
profile = agents[0].read_text()
frontmatter = profile.split('---\n', 2)
assert len(frontmatter) == 3 and frontmatter[0] == '', 'Agent needs YAML frontmatter'
assert re.search(r'^name: pomi$', frontmatter[1], re.M)
assert re.search(r'^description: .+', frontmatter[1], re.M)
assert len(frontmatter[2]) <= 30000, 'Copilot agent prompt exceeds the supported limit'
assert json.loads((source / 'compatibility.json').read_text())['managementProtocolMajor'] == 1
for icon in ('composerIcon', 'logo', 'logoDark'):
    assert (source / manifest['interface'][icon]).read_bytes().startswith(b'\x89PNG')

# A fetched revision must contain everything, including dot-directories, without
# depending on an installer executing our build script or checking out src/docs.
with tempfile.TemporaryDirectory(prefix='pomi-marketplace-') as temporary:
    root = Path(temporary) / 'marketplace'
    for catalog in ('.agents/plugins/marketplace.json', '.claude-plugin/marketplace.json'):
        data = json.loads((REPO / catalog).read_text())
        assert data['name'] == 'orchardcore'
        assert len(data['plugins']) == 1
        entry = data['plugins'][0]
        assert entry['name'] == 'pomi'
        path = entry['source']['path'] if isinstance(entry['source'], dict) else entry['source']
        assert path == './.agents/skills/pomi'
        assert (REPO / path).resolve() == source
        target = root / catalog
        target.parent.mkdir(parents=True, exist_ok=True)
        shutil.copyfile(REPO / catalog, target)
    relocated = root / '.agents/skills/pomi'
    shutil.copytree(source, relocated)
    subprocess.run([sys.executable, str(REPO / '.scripts/remote-management/verify-plugin-links.py'), str(relocated)], check=True)
    if args.codex:
        env = os.environ.copy()
        env['CODEX_HOME'] = str(Path(temporary) / 'profile')
        Path(env['CODEX_HOME']).mkdir()
        def codex(*arguments):
            result = subprocess.run([str(args.codex.resolve()), 'plugin', *arguments, '--json'],
                                    env=env, text=True, capture_output=True, check=True)
            return json.loads(result.stdout)
        codex('marketplace', 'add', str(root))
        result = codex('add', 'pomi@orchardcore')
        print(json.dumps(result))
        # Locate the installed copy without depending on the response schema.
        installed = list(Path(env['CODEX_HOME']).glob('plugins/cache/**/.codex-plugin/plugin.json'))
        assert len(installed) == 1, installed
        destination = installed[0].parent.parent
        assert {p.parent.name for p in destination.glob('skills/*/SKILL.md')} == expected
        for file in source.rglob('*'):
            if file.is_file() and '__pycache__' not in file.parts:
                assert (destination / file.relative_to(source)).read_bytes() == file.read_bytes(), file
print('Marketplace passed: two catalogs, one designer agent, ten canonical skills, manifests, branding and relocated references.')
