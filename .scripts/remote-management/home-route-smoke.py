#!/usr/bin/env python3
"""Verify the existing homepage command, published content, permissions and MCP equivalence."""
import json
import os
from pathlib import Path
import secrets
import subprocess
import sys
import tempfile
import urllib.error
import urllib.parse
import urllib.request

state_path = Path(sys.argv[1])
state = json.loads(state_path.read_text())
base = state['url']
assert urllib.parse.urlparse(base).hostname in ('127.0.0.1', 'localhost', '::1')
wrapper = Path(__file__).with_name('pomi-fixture.py')
name = 'HomeRouteSmoke' + secrets.token_hex(4)
tokens = {}
items, types = [], []


def token(client):
    if client not in tokens:
        body = urllib.parse.urlencode(dict(grant_type='client_credentials', client_id=client,
            client_secret=state['OC_CLIENT_SECRET'], scope='orchardcore.management')).encode()
        with urllib.request.urlopen(urllib.request.Request(base + 'connect/token', data=body,
                headers={'Content-Type': 'application/x-www-form-urlencoded'}), timeout=30) as response:
            tokens[client] = json.load(response)['access_token']
    return tokens[client]


def request(path, method='GET', body=None, client='cli-fixture', status=200, raw=False):
    url = urllib.parse.urljoin(base, path)
    assert urllib.parse.urlparse(url).netloc == urllib.parse.urlparse(base).netloc
    headers = {'Content-Type': 'application/json', 'Accept': 'application/json, text/event-stream'}
    if client:
        headers['Authorization'] = 'Bearer ' + token(client)
    req = urllib.request.Request(url, method=method, headers=headers,
        data=json.dumps(body).encode() if body is not None else None)
    try:
        response = urllib.request.urlopen(req, timeout=60)
    except urllib.error.HTTPError as error:
        response = error
    with response:
        text = response.read().decode()
        assert response.status == status, (method, path, response.status, text[:600])
        if raw:
            return text, response.headers
        if response.headers.get('Content-Type', '').startswith('text/event-stream'):
            return next(json.loads(line[6:]) for line in text.splitlines()
                if line.startswith('data: ') and json.loads(line[6:]).get('id') == 1)
        return json.loads(text) if text else None


def pomi(*args, body=None, client='cli-home-route', status=None, failure=False):
    env = os.environ.copy()
    env.update(OC_FIXTURE_CONFIG_HOME=config_home, OC_FIXTURE_CLIENT_ID=client)
    command = [sys.executable, str(wrapper), str(state_path), *args, '--output', 'json']
    if body is not None:
        command.append('--stdin')
    result = subprocess.run(command, input=json.dumps(body) if body is not None else None,
        capture_output=True, text=True, env=env, timeout=90)
    if failure:
        assert result.returncode != 0, args
        return
    if status:
        assert result.returncode != 0, args
        assert json.loads(result.stderr or result.stdout)['error']['status'] == status, result.stderr[:600]
        return
    assert result.returncode == 0, (args[:3], result.stderr[:800])
    return json.loads(result.stdout) if result.stdout.strip() else None


def tool(content_id, client='cli-home-route', status=200):
    result = request('mcp', 'POST', {'jsonrpc': '2.0', 'id': 1, 'method': 'tools/call',
        'params': {'name': 'settings_set-home-content', 'arguments': {'path': {'contentItemId': content_id}}}},
        client=client)['result']
    content = json.loads(result['content'][0]['text'])
    assert content['statusCode'] == status, content
    assert result.get('isError', False) == (status >= 400)
    return json.loads(content['body']) if content['body'] else None


def catalog():
    manifest = request('api/management/manifest')
    document = request(manifest['openApiUrl'])
    operations = [operation for item in document['paths'].values() for operation in item.values()
        if isinstance(operation, dict) and operation.get('operationId')]
    ids = [operation['operationId'] for operation in operations]
    commands = [' '.join([*metadata['commandGroup'], metadata['verb']])
        for operation in operations if (metadata := operation.get('x-oc-cli'))]
    tools = request('mcp', 'POST', {'jsonrpc': '2.0', 'id': 1, 'method': 'tools/list', 'params': {}})['result']['tools']
    names = [entry['name'] for entry in tools]
    assert len(ids) == len(set(ids)) and len(commands) == len(set(commands)) and len(names) == len(set(names))
    return ids, commands, names


def feature(name, enabled):
    request('api/features/' + name + (':enable?force=true' if enabled else ':disable'), 'POST')


with tempfile.TemporaryDirectory(prefix='home-route-cli-', dir=state_path.parent) as config_home:
    pomi('context', 'add', name, base, '--current')
    feature('OrchardCore.RemoteManagement.Mcp', True)
    feature('OrchardCore.Html', True)
    pomi('themes', 'set-current', 'TheTheme', client='cli-fixture')
    pomi('api', 'refresh', '--force')
    ids, commands, names = catalog()
    assert 'ApiSetHomeContent' in ids
    assert 'settings set-home-content' in commands
    assert 'settings_set-home-content' in names
    definition = {'name': name, 'displayName': name,
        'settings': {'ContentTypeSettings': {'draftable': True, 'versionable': True, 'creatable': True}},
        'parts': [{'name': part, 'partName': part} for part in ['TitlePart', 'HtmlBodyPart', 'AutoroutePart']]}
    pomi('content', 'types', 'create', client='cli-fixture', body=definition)
    marker = name + '-published'
    item = pomi('content', 'items', 'save', client='cli-fixture', body={'ContentType': name,
        'TitlePart': {'Title': name}, 'HtmlBodyPart': {'Html': '<p>' + marker + '</p>'},
        'AutoroutePart': {'Path': name.lower()}})
    content_id = item['ContentItemId']
    for client in ['cli-denied', 'cli-discovery', 'cli-content-reader']:
        request('api/home-route/content/' + content_id, 'PUT', client=client, status=403)
        if client != 'cli-denied':
            tool(content_id, client=client, status=403)
    request('api/home-route/content/' + content_id, 'PUT', client=None, status=401)
    pomi('settings', 'set-home-content', 'missing-' + name, status=404)
    result = pomi('settings', 'set-home-content', content_id)
    assert result['contentItemId'] == content_id and result['contentType'] == name
    assert pomi('settings', 'set-home-content', content_id) == result
    assert tool(content_id) == result
    text, _ = request('', client=None, raw=True)
    assert marker in text
    draft_marker = name + '-unpublished'
    pomi('content', 'items', 'update-draft', content_id, client='cli-fixture',
        body={'HtmlBodyPart': {'Html': '<p>' + draft_marker + '</p>'}})
    assert pomi('settings', 'set-home-content', content_id) == result
    text, _ = request('', client=None, raw=True)
    assert marker in text and draft_marker not in text
    latest = request('api/content/' + content_id + '?version=latest')
    assert draft_marker in latest['HtmlBodyPart']['Html'] and not latest['Published']
    feature('OrchardCore.RemoteManagement.Cli', False)
    assert tool(content_id) == result
    feature('OrchardCore.RemoteManagement.Cli', True)
    print('PASS: existing homepage command, restricted permissions, retries, rendered published content, preserved draft and MCP without CLI.')
    print('Synthetic homepage and its draft remain in the disposable tenant.')
