#!/usr/bin/env python3
"""Verify shortcode templates, shared rendering, permissions and feature projection on a disposable tenant."""
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
name = 'ShortcodeSmoke' + secrets.token_hex(4)
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
            return text
        if response.headers.get('Content-Type', '').startswith('text/event-stream'):
            return next(json.loads(line[6:]) for line in text.splitlines()
                if line.startswith('data: ') and json.loads(line[6:]).get('id') == 1)
        return json.loads(text) if text else None


def pomi(*args, body=None, client='cli-shortcodes', status=None, failure=False):
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


def tool(verb, arguments=None, client='cli-shortcodes', status=200):
    result = request('mcp', 'POST', {'jsonrpc': '2.0', 'id': 1, 'method': 'tools/call',
        'params': {'name': 'shortcodes_templates_' + verb, 'arguments': arguments or {}}}, client=client)['result']
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


with tempfile.TemporaryDirectory(prefix='shortcodes-cli-', dir=state_path.parent) as config_home:
    pomi('context', 'add', name, base, '--current')
    feature('OrchardCore.Shortcodes.Templates', True)
    feature('OrchardCore.RemoteManagement.Mcp', True)
    pomi('api', 'refresh', '--force')
    saved_name = name.lower()
    definition = {'name': name, 'content': '<b>{{ Content }}</b>', 'hint': 'Smoke hint',
        'usage': '<strong>Safe</strong><script>alert(1)</script>', 'categories': ['Test'],
        'defaultValue': '[' + saved_name + '][/' + saved_name + ']'}
    try:
        assert pomi('shortcodes', 'templates', 'schema', '--operation', 'create')
        assert pomi('shortcodes', 'templates', 'validate', body=definition)['isValid']
        saved = pomi('shortcodes', 'templates', 'create', body=definition)
        assert saved['name'] == saved_name
        assert '<script' not in saved['usage'] and '<strong>Safe</strong>' in saved['usage']
        assert saved == pomi('shortcodes', 'templates', 'create', body=definition)
        assert pomi('shortcodes', 'templates', 'list', '--search', name)['totalCount'] == 1
        assert saved == pomi('shortcodes', 'templates', 'show', name.upper())
        conflict = dict(definition, content='Different')
        pomi('shortcodes', 'templates', 'create', body=conflict, status=409)
        invalid = dict(saved, content='{% if %}')
        assert not pomi('shortcodes', 'templates', 'validate', body=invalid)['isValid']
        pomi('shortcodes', 'templates', 'update', saved_name, body=invalid, status=400)
        assert saved == pomi('shortcodes', 'templates', 'show', saved_name)
        assert saved == tool('show', {'query': {'name': saved_name}})
        saved['hint'] = 'Updated through MCP'
        updated = tool('update', {'query': {'name': saved_name}, 'body': saved})
        assert updated == pomi('shortcodes', 'templates', 'show', saved_name)
        pomi('shortcodes', 'templates', 'delete', saved_name, failure=True)
        assert updated == pomi('shortcodes', 'templates', 'show', saved_name)
        route = 'api/shortcode-templates'
        for path, method, body in [(route, 'GET', None), (route + '/by-name?name=' + saved_name, 'GET', None),
                (route + '/validate', 'POST', definition), (route, 'POST', definition),
                (route + '/by-name?name=' + saved_name, 'PUT', saved), (route + '/by-name?name=' + saved_name, 'DELETE', None)]:
            request(path, method, body, client=None, status=401)
            request(path, method, body, client='cli-discovery', status=403)
        tool('show', {'query': {'name': saved_name}}, client='cli-discovery', status=403)
        print('Shortcode CRUD, normalized retries, schema, confirmation and HTTP/MCP permissions: passed', flush=True)

        feature('OrchardCore.Autoroute', True)
        pomi('themes', 'set-current', 'TheTheme', client='cli-fixture')
        type_name = name + 'Page'
        pomi('content', 'types', 'create', client='cli-fixture', body={'name': type_name, 'displayName': type_name,
            'settings': {'ContentTypeSettings': {'draftable': True, 'versionable': True, 'creatable': True}},
            'parts': [{'name': part, 'partName': part} for part in ['TitlePart', 'LiquidPart', 'AutoroutePart']]})
        types.append(type_name)
        marker = 'shortcode-marker-' + secrets.token_hex(8)
        liquid = '{{ "[' + saved_name + ']'+ marker + '[/' + saved_name + ']" | shortcode | raw }}'
        page = pomi('content', 'items', 'save', client='cli-fixture', body={'ContentType': type_name,
            'TitlePart': {'Title': name}, 'LiquidPart': {'Liquid': liquid}, 'AutoroutePart': {'Path': name.lower()}})
        items.append(page['ContentItemId'])
        assert '<b>' + marker + '</b>' in request(name.lower(), client=None, raw=True)
        updated['content'] = '<em>{{ Content }}</em>'
        pomi('shortcodes', 'templates', 'update', saved_name, body=updated)
        assert '<em>' + marker + '</em>' in request(name.lower(), client=None, raw=True)
        print('Stored shortcode templates render and invalidate cached definitions: passed', flush=True)

        ids, commands, names = catalog()
        assert len([command for command in commands if command.startswith('shortcodes templates ')]) == 6
        assert len([tool_name for tool_name in names if tool_name.startswith('shortcodes_templates_')]) == 6
        feature('OrchardCore.RemoteManagement.Cli', False)
        assert not catalog()[1]
        assert tool('show', {'query': {'name': saved_name}})['name'] == saved_name
        feature('OrchardCore.RemoteManagement.Cli', True)
        feature('OrchardCore.Shortcodes.Templates', False)
        assert not any(tool_name.startswith('shortcodes_templates_') for tool_name in catalog()[2])
        request(route, client='cli-fixture', status=404, raw=True)
        feature('OrchardCore.Shortcodes.Templates', True)
        assert set(catalog()[0]) == set(ids)
        pomi('api', 'refresh', '--force')
        pomi('shortcodes', 'templates', 'delete', saved_name, '--force')
        pomi('shortcodes', 'templates', 'delete', saved_name, '--force')
        pomi('shortcodes', 'templates', 'show', saved_name, status=404)
        print('Unique catalogs, feature removal/restoration and MCP with CLI disabled: passed', flush=True)
    finally:
        for item_id in reversed(items):
            pomi('content', 'items', 'delete', item_id, '--force', client='cli-fixture')
        for type_name in reversed(types):
            pomi('content', 'types', 'delete', type_name, '--force', client='cli-fixture')
        request('api/shortcode-templates/by-name?name=' + saved_name, 'DELETE', status=204)
print('Shortcodes smoke passed; feature and theme changes remain confined to the disposable fixture.')
