#!/usr/bin/env python3
"""Verify CLI/MCP discovery across feature combinations on a disposable fixture."""
import json
from pathlib import Path
import sys
import urllib.error
import urllib.parse
import urllib.request

state = json.loads(Path(sys.argv[1]).read_text())
base = state['url']
assert urllib.parse.urlparse(base).hostname in ('127.0.0.1', 'localhost', '::1')
form = urllib.parse.urlencode(dict(grant_type='client_credentials', client_id=state['OC_CLIENT_ID'],
                                 client_secret=state['OC_CLIENT_SECRET'], scope='orchardcore.management')).encode()
with urllib.request.urlopen(urllib.request.Request(base + 'connect/token', data=form,
        headers={'Content-Type': 'application/x-www-form-urlencoded'}), timeout=30) as response:
    token = json.load(response)['access_token']


def request(path, method='GET', payload=None):
    url = urllib.parse.urljoin(base, path)
    assert urllib.parse.urlparse(url).netloc == urllib.parse.urlparse(base).netloc
    headers = {'Authorization': 'Bearer ' + token, 'Content-Type': 'application/json',
               'Accept': 'application/json, text/event-stream'}
    req = urllib.request.Request(url, method=method, headers=headers,
                                 data=json.dumps(payload).encode() if payload is not None else None)
    try:
        response = urllib.request.urlopen(req, timeout=60)
    except urllib.error.HTTPError as error:
        raise RuntimeError(f'{method} {path}: {error.code} {error.read().decode()[:1000]}') from None
    with response:
        body = response.read().decode()
        if response.headers.get('Content-Type', '').startswith('text/event-stream'):
            messages = [json.loads(line[6:]) for line in body.splitlines() if line.startswith('data: ')]
            return next(message for message in messages if message.get('id') == 1)
        return json.loads(body) if body else None


def feature(name, enabled):
    return request('api/features/' + name + (':enable?force=true' if enabled else ':disable'), 'POST')


snapshots = []


def snapshot(label):
    manifest = request('api/management/manifest')
    document = request(manifest['openApiUrl'])
    ids, commands = [], []
    for item in document['paths'].values():
        for operation in item.values():
            if not isinstance(operation, dict):
                continue
            if operation.get('operationId'):
                ids.append(operation['operationId'])
            if metadata := operation.get('x-oc-cli'):
                commands.append(' '.join([*metadata['commandGroup'], metadata['verb']]))
    assert len(ids) == len(set(ids)), 'Duplicate OpenAPI operation IDs'
    assert len(commands) == len(set(commands)), 'Duplicate command paths'
    result = request('mcp', 'POST', {'jsonrpc': '2.0', 'id': 1, 'method': 'tools/list', 'params': {}})
    assert 'result' in result, result
    names = [tool['name'] for tool in result['result']['tools']]
    assert len(names) == len(set(names)), 'Duplicate MCP tool names'
    snapshots.append({'label': label, 'operationIds': sorted(ids), 'commands': sorted(commands), 'tools': sorted(names)})
    print(label, ':', len(ids), 'operations,', len(commands), 'commands,', len(names), 'tools', flush=True)
    return set(commands), set(names)


# This script changes only the disposable tenant and restores its explicit feature states.
features = ['OrchardCore.RemoteManagement.Mcp', 'OrchardCore.Templates', 'OrchardCore.Media.Tus',
            'OrchardCore.RemoteManagement.Cli']
initial = {name: request('api/features/' + name)['isEnabled'] for name in features}
try:
    for name in features:
        feature(name, True)
    commands, tools = snapshot('cli-and-mcp')
    assert 'templates list' in commands and 'templates_list' in tools
    assert 'media uploads show' in commands and 'media_uploads_show' in tools
    feature('OrchardCore.Media.Tus', False)
    commands, tools = snapshot('without-tus')
    assert 'media uploads show' not in commands and 'media_uploads_show' not in tools
    feature('OrchardCore.Media.Tus', True)
    feature('OrchardCore.Templates', False)
    commands, tools = snapshot('without-templates')
    assert 'templates list' not in commands and 'templates_list' not in tools
    feature('OrchardCore.Templates', True)
    restored_commands, restored_tools = snapshot('restored')
    assert restored_commands == set(snapshots[0]['commands'])
    assert restored_tools == set(snapshots[0]['tools'])
    feature('OrchardCore.RemoteManagement.Cli', False)
    commands, tools = snapshot('mcp-only')
    assert not commands, 'CLI metadata must be gated by the CLI feature'
    assert tools == restored_tools, 'MCP tools must not depend on enabling the CLI feature'
finally:
    # Enable the transport dependency before restoring other feature states.
    feature('OrchardCore.RemoteManagement.Cli', True)
    for name in features:
        if not initial[name]:
            feature(name, False)

(Path(state['root']) / 'catalog-smoke.json').write_text(json.dumps(snapshots, indent=2) + '\n')
print('Catalog smoke passed: operation/command/tool uniqueness, feature removal/restoration, and MCP-only discovery.')
