"""Generate version-local Pomi downloads and skill pages for every MkDocs build."""
import importlib.util
import json
import os
import posixpath
from pathlib import Path
import tempfile

from mkdocs.structure.files import File, InclusionLevel

HERE = Path(__file__).resolve().parent
spec = importlib.util.spec_from_file_location('pomi_plugin_builder', HERE / 'build-plugin.py')
builder = importlib.util.module_from_spec(spec)
spec.loader.exec_module(builder)
metadata = {}


class RawMarkdownFile(File):
    """Publish exact Markdown bytes as an asset rather than rendering HTML."""

    def is_documentation_page(self):
        return False


def on_files(files, config):
    global metadata
    with tempfile.TemporaryDirectory(prefix='pomi-docs-') as temporary:
        output = Path(temporary) / 'packages'
        result = builder.build(output)
        metadata = json.loads((output / 'package-metadata.json').read_text())
        if metadata['sourceDirty'] and (os.environ.get('CI') == 'true' or os.environ.get('READTHEDOCS') == 'True'):
            raise ValueError('Published Pomi packages require committed skill, packaging, and branding inputs')
        for path in sorted(output.iterdir()):
            if path.is_file():
                files.append(File.generated(config, 'downloads/' + path.name, content=path.read_bytes()))
        plugin = Path(result['plugin'])
        for path in sorted((plugin / 'assets').iterdir()):
            files.append(File.generated(config, 'agents/assets/' + path.name, content=path.read_bytes()))
        markdown_files = [path for directory in ('skills', 'agents')
                          for path in (plugin / directory).rglob('*.md')]
        for path in sorted(markdown_files):
            relative = path.relative_to(plugin).as_posix()
            raw_uri = 'agents/raw/' + relative
            page_uri = 'agents/' + relative
            files.append(RawMarkdownFile.generated(config, raw_uri, content=path.read_bytes()))
            text = path.read_text()
            if text.startswith('---\n'):
                text = text.split('---\n', 2)[2]
            raw_link = posixpath.relpath(raw_uri, posixpath.dirname(page_uri))
            index_link = posixpath.relpath('agents/index.md', posixpath.dirname(page_uri))
            text = f'[All skills and installation]({index_link}) · [Raw Markdown]({raw_link})\n\n' + text
            files.append(File.generated(config, page_uri, content=text, inclusion=InclusionLevel.NOT_IN_NAV))
    return files


def on_page_markdown(markdown, page, config, files):
    if page.file.src_uri != 'agents/index.md':
        return markdown
    rows = []
    for file in sorted(files, key=lambda item: item.src_uri):
        if file.src_uri.startswith('agents/skills/') and file.src_uri.endswith('/SKILL.md'):
            name = file.src_uri.split('/')[2]
            title = name.removeprefix('orchardcore-cli-').replace('-', ' ').title() if name != 'orchardcore-cli' else 'Start here'
            rows.append(f'| {title} | [Read]({file.src_uri.removeprefix("agents/")}) | [SKILL.md](raw/{file.src_uri.removeprefix("agents/")}) |')
    artifacts = '\n'.join(f'- [{info["file"]}](../downloads/{info["file"]}) — SHA-256 `{info["sha256"]}`'
                          for info in metadata['artifacts'].values())
    version = f'''Package **{metadata['packageVersion']}**, generated from commit
[`{metadata['sourceCommit'][:12]}`](https://github.com/OrchardCMS/OrchardCore/commit/{metadata['sourceCommit']}).
{'**Local working-tree build: inputs include uncommitted changes.**' if metadata['sourceDirty'] else 'Built from committed package inputs.'}

- Pomi: {metadata['compatibility']['pomi']}.
- Server: {metadata['compatibility']['server']}.
- Management protocol major: {metadata['compatibility']['managementProtocolMajor']}.

{metadata['compatibility']['notes']}

Install the same Git revision in Codex:

```bash
codex plugin marketplace add OrchardCMS/OrchardCore --ref {metadata['sourceCommit']}
codex plugin add pomi@orchardcore
```

If `orchardcore` is already registered with a different ref, remove that
marketplace with `codex plugin marketplace remove orchardcore` before adding
the chosen ref and reinstalling. Uncommitted local changes are not included
in a Git installation.

Exact artifacts for this build:

{artifacts}
'''
    return markdown.replace('<!-- pomi-skill-list -->', '\n'.join(rows)).replace('<!-- pomi-package-metadata -->', version)
