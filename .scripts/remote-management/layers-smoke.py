#!/usr/bin/env python3
"""Verify layer CRUD, permissions, projection and rendered rules on a disposable tenant."""
import copy
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
name = 'LayerSmoke' + secrets.token_hex(4)
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


def pomi(*args, body=None, client='cli-layers', status=None, failure=False):
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


def tool(verb, arguments=None, client='cli-layers', status=200):
    result = request('mcp', 'POST', {'jsonrpc': '2.0', 'id': 1, 'method': 'tools/call',
        'params': {'name': 'layers_' + verb, 'arguments': arguments or {}}}, client=client)['result']
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


with tempfile.TemporaryDirectory(prefix='layers-cli-', dir=state_path.parent) as config_home:
    pomi('context', 'add', name, base, '--current')
    feature('OrchardCore.Layers', True)
    feature('OrchardCore.RemoteManagement.Mcp', True)
    pomi('api', 'refresh', '--force')
    definition = {'name': name, 'description': 'Layer verification', 'conditions': [
        {'name': 'AllConditionGroup', 'conditions': [
            {'name': 'BooleanCondition', 'properties': {'value': True}},
            {'name': 'IsAnonymousCondition'}]}]}
    try:
        descriptors = pomi('layers', 'conditions')
        assert pomi('layers', 'schema', '--operation', 'create')
        assert any(entry['name'] == 'AllConditionGroup' and entry['canWrite'] for entry in descriptors)
        assert pomi('layers', 'validate', body=definition)['isValid']
        special = copy.deepcopy(definition)
        special['name'] = name + '/space % name'
        assert pomi('layers', 'create', body=special)['name'] == special['name']
        assert pomi('layers', 'show', special['name'])['name'] == special['name']
        assert tool('show', {'query': {'name': special['name']}})['name'] == special['name']
        pomi('layers', 'delete', special['name'], '--force')
        pomi('layers', 'show', special['name'], status=404)
        saved = pomi('layers', 'create', body=definition)
        assert saved == pomi('layers', 'create', body=definition)
        assert pomi('layers', 'list', '--search', name)['totalCount'] == 1
        invalid = copy.deepcopy(definition)
        invalid['conditions'][0]['conditions'][0]['properties']['value'] = 'invalid'
        assert not pomi('layers', 'validate', body=invalid)['isValid']
        pomi('layers', 'update', name, body=invalid, status=400)
        assert saved == pomi('layers', 'show', name)
        assert saved == tool('show', {'query': {'name': name}})
        saved['description'] = 'Updated through MCP'
        updated = tool('update', {'query': {'name': name}, 'body': saved})
        assert updated == pomi('layers', 'show', name)
        pomi('layers', 'delete', name, failure=True)
        assert updated == pomi('layers', 'show', name)

        for path, method, body in [('api/layers', 'GET', None), ('api/layers/by-name?name=' + urllib.parse.quote(name, safe=''), 'GET', None),
                ('api/layer-conditions', 'GET', None), ('api/layers/validate', 'POST', definition),
                ('api/layers', 'POST', definition), ('api/layers/by-name?name=' + urllib.parse.quote(name, safe=''), 'PUT', definition),
                ('api/layers/by-name?name=' + urllib.parse.quote(name, safe=''), 'DELETE', None)]:
            request(path, method, body, client=None, status=401)
            request(path, method, body, client='cli-discovery', status=403)
        tool('show', {'query': {'name': name}}, client='cli-discovery', status=403)
        print('Layer CRUD, retries, validation, HTTP and MCP permissions: passed', flush=True)

        # Compose a public page and widget using the existing content APIs, then prove rule evaluation.
        for module in ['OrchardCore.Html', 'OrchardCore.Autoroute']:
            feature(module, True)
        pomi('themes', 'set-current', 'TheTheme', client='cli-fixture')
        for suffix, parts, stereotype in [('Page', ['TitlePart', 'HtmlBodyPart', 'AutoroutePart'], None),
                ('Widget', ['TitlePart', 'HtmlBodyPart'], 'Widget')]:
            type_name = name + suffix
            settings = {'draftable': True, 'versionable': True, 'creatable': True}
            if stereotype:
                settings['stereotype'] = stereotype
            pomi('content', 'types', 'create', client='cli-fixture', body={'name': type_name, 'displayName': type_name,
                'settings': {'ContentTypeSettings': settings}, 'parts': [{'name': part, 'partName': part} for part in parts]})
            types.append(type_name)
        page = pomi('content', 'items', 'save', client='cli-fixture', body={'ContentType': name + 'Page',
            'TitlePart': {'Title': name}, 'HtmlBodyPart': {'Html': '<p>Layer test page</p>'}, 'AutoroutePart': {'Path': name.lower()}})
        items.append(page['ContentItemId'])
        marker = 'layer-marker-' + secrets.token_hex(8)
        widget = pomi('content', 'items', 'save', client='cli-fixture', body={'ContentType': name + 'Widget',
            'TitlePart': {'Title': 'Layer test widget'}, 'HtmlBodyPart': {'Html': '<p>' + marker + '</p>'},
            'LayerMetadata': {'Layer': name, 'Zone': 'Content', 'Position': 0, 'RenderTitle': False}})
        items.append(widget['ContentItemId'])
        assert marker in request(name.lower(), client=None, raw=True)
        pomi('layers', 'delete', name, '--force', status=409)
        updated['conditions'][0]['conditions'][0]['properties']['value'] = False
        pomi('layers', 'update', name, body=updated)
        assert marker not in request(name.lower(), client=None, raw=True)
        updated['conditions'][0]['conditions'][0]['properties']['value'] = True
        pomi('layers', 'update', name, body=updated)
        assert marker in request(name.lower(), client=None, raw=True)
        print('Layer condition changes affect rendered widgets; referenced deletion refused: passed', flush=True)

        ids, commands, names = catalog()
        assert len([command for command in commands if command.startswith('layers ') and not command.startswith('layers widgets ')]) == 7
        assert len([tool_name for tool_name in names if tool_name.startswith('layers_') and not tool_name.startswith('layers_widgets_')]) == 7
        feature('OrchardCore.RemoteManagement.Cli', False)
        assert not catalog()[1]
        assert tool('show', {'query': {'name': name}})['name'] == name
        feature('OrchardCore.RemoteManagement.Cli', True)
        feature('OrchardCore.Layers', False)
        assert not any(tool_name.startswith('layers_') for tool_name in catalog()[2])
        request('api/layers', status=404, raw=True)
        feature('OrchardCore.Layers', True)
        assert set(catalog()[1]) == set(commands)
        print('Unique catalogs, feature removal/restoration and MCP with CLI disabled: passed', flush=True)
    finally:
        feature('OrchardCore.RemoteManagement.Cli', True)
        feature('OrchardCore.Layers', True)
        for item_id in reversed(items):
            pomi('content', 'items', 'delete', item_id, '--force', client='cli-fixture')
        for type_name in reversed(types):
            pomi('content', 'types', 'delete', type_name, '--force', client='cli-fixture')
        pomi('layers', 'delete', name + '/space % name', '--force')
        pomi('layers', 'delete', name, '--force')
        pomi('layers', 'delete', name, '--force')

print('Layers smoke passed; feature and theme changes remain confined to the disposable fixture.')
