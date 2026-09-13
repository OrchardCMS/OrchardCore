#!/usr/bin/env python3
"""Verify placement CRUD, filters, storage ownership and rendered output on a disposable tenant."""
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
name = 'PlacementSmoke' + secrets.token_hex(4)
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


def pomi(*args, body=None, client='cli-placements', status=None, failure=False):
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


def tool(verb, arguments=None, client='cli-placements', status=200):
    result = request('mcp', 'POST', {'jsonrpc': '2.0', 'id': 1, 'method': 'tools/call',
        'params': {'name': 'placements_' + verb, 'arguments': arguments or {}}}, client=client)['result']
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


with tempfile.TemporaryDirectory(prefix='placements-cli-', dir=state_path.parent) as config_home:
    pomi('context', 'add', name, base, '--current')
    feature('OrchardCore.Placements', True)
    feature('OrchardCore.RemoteManagement.Mcp', True)
    pomi('api', 'refresh', '--force')
    definition = {'shapeType': name, 'nodes': [{'place': 'Content:1', 'displayType': 'Detail', 'contentType': ['Probe']}]}
    file_storage = False
    rendering_created = False
    try:
        assert pomi('placements', 'schema', '--operation', 'create')
        assert {'path', 'contentType', 'contentPart'}.issubset(pomi('placements', 'filters'))
        assert pomi('placements', 'validate', body=definition)['isValid']
        saved = pomi('placements', 'create', body=definition)
        assert saved == pomi('placements', 'create', body=definition)
        assert saved == pomi('placements', 'show', name.upper())
        assert pomi('placements', 'list', '--search', name)['totalCount'] == 1
        invalid = {'shapeType': name, 'nodes': [{'place': '-', 'contenttype': 'Probe'}]}
        assert not pomi('placements', 'validate', body=invalid)['isValid']
        pomi('placements', 'update', name, body=invalid, status=400)
        pomi('placements', 'create', body={'shapeType': name, 'nodes': [{'place': 'Header:1'}]}, status=409)
        assert saved == pomi('placements', 'show', name)
        assert saved == tool('show', {'query': {'shapeType': name}})
        saved['nodes'][0]['place'] = 'Header:2'
        updated = tool('update', {'query': {'shapeType': name}, 'body': saved})
        assert updated == pomi('placements', 'show', name)
        pomi('placements', 'delete', name, failure=True)
        assert updated == pomi('placements', 'show', name)
        route = 'api/placements'
        for path, method, body in [(route, 'GET', None), (route + '/by-shape?shapeType=' + name, 'GET', None),
                ('api/placement-filters', 'GET', None), (route + '/validate', 'POST', definition), (route, 'POST', definition),
                (route + '/by-shape?shapeType=' + name, 'PUT', definition), (route + '/by-shape?shapeType=' + name, 'DELETE', None)]:
            request(path, method, body, client=None, status=401)
            request(path, method, body, client='cli-discovery', status=403)
        tool('show', {'query': {'shapeType': name}}, client='cli-discovery', status=403)
        print('Placement CRUD, schema, filter validation, retries and HTTP/MCP permissions: passed', flush=True)

        # Both stores own separate tenant documents; switching is not data migration.
        feature('OrchardCore.Placements.FileStorage', True)
        file_storage = True
        pomi('placements', 'show', name, status=404)
        file_definition = {'shapeType': name, 'nodes': [{'place': 'Footer:7'}]}
        pomi('placements', 'create', body=file_definition)
        feature('OrchardCore.Placements.FileStorage', False)
        file_storage = False
        assert pomi('placements', 'show', name)['nodes'][0]['place'] == 'Header:2'
        feature('OrchardCore.Placements.FileStorage', True)
        file_storage = True
        assert pomi('placements', 'show', name)['nodes'][0]['place'] == 'Footer:7'
        pomi('placements', 'delete', name, '--force')
        feature('OrchardCore.Placements.FileStorage', False)
        file_storage = False
        print('Database and file storage retain independent tenant definitions: passed', flush=True)

        for module in ['OrchardCore.Html', 'OrchardCore.Autoroute']:
            feature(module, True)
        pomi('themes', 'set-current', 'TheTheme', client='cli-fixture')
        type_name = name + 'Page'
        pomi('content', 'types', 'create', client='cli-fixture', body={'name': type_name, 'displayName': type_name,
            'settings': {'ContentTypeSettings': {'draftable': True, 'versionable': True, 'creatable': True}},
            'parts': [{'name': part, 'partName': part} for part in ['TitlePart', 'HtmlBodyPart', 'AutoroutePart']]})
        types.append(type_name)
        marker = 'placement-marker-' + secrets.token_hex(8)
        for suffix in ['match', 'other']:
            page = pomi('content', 'items', 'save', client='cli-fixture', body={'ContentType': type_name,
                'TitlePart': {'Title': name}, 'HtmlBodyPart': {'Html': '<p>' + marker + '</p>'},
                'AutoroutePart': {'Path': name.lower() + '/' + suffix}})
            items.append(page['ContentItemId'])
        match_path, other_path = name.lower() + '/match', name.lower() + '/other'
        assert marker in request(match_path, client=None, raw=True)
        rendering = {'shapeType': 'HtmlBodyPart', 'nodes': [{'place': '-', 'displayType': 'Detail',
            'contentType': type_name, 'path': '~/' + match_path}]}
        pomi('placements', 'create', body=rendering)
        rendering_created = True
        assert marker not in request(match_path, client=None, raw=True)
        assert marker in request(other_path, client=None, raw=True)
        rendering['nodes'].append({'place': 'Content:1', 'displayType': 'Detail', 'contentType': [type_name], 'path': '~/' + match_path})
        pomi('placements', 'update', 'HtmlBodyPart', body=rendering)
        assert marker in request(match_path, client=None, raw=True)
        pomi('placements', 'update', 'HtmlBodyPart', body={'shapeType': 'HtmlBodyPart', 'nodes': []})
        pomi('placements', 'update', 'HtmlBodyPart', body={'shapeType': 'HtmlBodyPart', 'nodes': []})
        assert marker in request(match_path, client=None, raw=True)
        print('Placement filters, last-match ordering, rendering/cache changes and empty-array deletion: passed', flush=True)

        ids, commands, names = catalog()
        assert len([command for command in commands if command.startswith('placements ')]) == 7
        assert len([tool_name for tool_name in names if tool_name.startswith('placements_')]) == 7
        feature('OrchardCore.RemoteManagement.Cli', False)
        assert not catalog()[1]
        assert tool('show', {'query': {'shapeType': name}})['shapeType'] == name
        feature('OrchardCore.RemoteManagement.Cli', True)
        feature('OrchardCore.Placements', False)
        assert not any(tool_name.startswith('placements_') for tool_name in catalog()[2])
        request(route, client='cli-fixture', status=404, raw=True)
        feature('OrchardCore.Placements', True)
        assert set(catalog()[0]) == set(ids)
        pomi('api', 'refresh', '--force')
        pomi('placements', 'delete', name, '--force')
        pomi('placements', 'delete', name, '--force')
        pomi('placements', 'show', name, status=404)
        print('Unique catalogs, feature removal/restoration and MCP with CLI disabled: passed', flush=True)
    finally:
        if file_storage:
            request('api/placements/by-shape?shapeType=' + name, 'DELETE', status=204)
            feature('OrchardCore.Placements.FileStorage', False)
        if rendering_created:
            request('api/placements/by-shape?shapeType=HtmlBodyPart', 'DELETE', status=204)
        request('api/placements/by-shape?shapeType=' + name, 'DELETE', status=204)
        for item_id in reversed(items):
            pomi('content', 'items', 'delete', item_id, '--force', client='cli-fixture')
        for type_name in reversed(types):
            pomi('content', 'types', 'delete', type_name, '--force', client='cli-fixture')
print('Placements smoke passed; feature and theme changes remain confined to the disposable fixture.')
