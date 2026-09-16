#!/usr/bin/env python3
"""Verify widget placement, version permissions, shared moves and rendering on a disposable tenant."""
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
name = 'WidgetSmoke' + secrets.token_hex(4)
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


def pomi(*args, body=None, client='cli-widgets', status=None, failure=False):
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


def tool(verb, arguments=None, client='cli-widgets', status=200):
    result = request('mcp', 'POST', {'jsonrpc': '2.0', 'id': 1, 'method': 'tools/call',
        'params': {'name': 'layers_widgets_' + verb, 'arguments': arguments or {}}}, client=client)['result']
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


with tempfile.TemporaryDirectory(prefix='widgets-cli-', dir=state_path.parent) as config_home:
    pomi('context', 'add', name, base, '--current')
    for module in ['OrchardCore.Layers', 'OrchardCore.Html', 'OrchardCore.Autoroute', 'OrchardCore.RemoteManagement.Mcp']:
        feature(module, True)
    pomi('api', 'refresh', '--force')
    try:
        pomi('themes', 'set-current', 'TheTheme', client='cli-fixture')
        assert {'Content', 'Footer'}.issubset(pomi('layers', 'widgets', 'zones'))
        assert pomi('layers', 'widgets', 'schema', '--operation', 'update')
        for suffix, parts, stereotype in [('Page', ['TitlePart', 'HtmlBodyPart', 'AutoroutePart'], None),
                ('Widget', ['TitlePart', 'HtmlBodyPart'], 'Widget')]:
            type_name = name + suffix
            settings = {'draftable': True, 'versionable': True, 'creatable': True}
            if stereotype:
                settings['stereotype'] = stereotype
            pomi('content', 'types', 'create', client='cli-fixture', body={'name': type_name, 'displayName': type_name,
                'settings': {'ContentTypeSettings': settings}, 'parts': [{'name': part, 'partName': part} for part in parts]})
            types.append(type_name)
        layer = {'name': name, 'conditions': [{'name': 'BooleanCondition', 'properties': {'value': True}}]}
        pomi('layers', 'create', body=layer)
        page = pomi('content', 'items', 'save', client='cli-fixture', body={'ContentType': name + 'Page',
            'TitlePart': {'Title': name}, 'HtmlBodyPart': {'Html': '<p>Widget placement page</p>'}, 'AutoroutePart': {'Path': name.lower()}})
        items.append(page['ContentItemId'])
        markers = ['widget-first-' + secrets.token_hex(6), 'widget-second-' + secrets.token_hex(6)]
        widgets = []
        for marker in markers:
            widget = pomi('content', 'items', 'save', client='cli-fixture', body={'ContentType': name + 'Widget',
                'TitlePart': {'Title': marker}, 'HtmlBodyPart': {'Html': '<p>' + marker + '</p>'}})
            items.append(widget['ContentItemId'])
            widgets.append(widget)
        first, second = [widget['ContentItemId'] for widget in widgets]
        pomi('layers', 'widgets', 'show', first, status=404)
        assert markers[0] not in request(name.lower(), client=None, raw=True)
        draft_marker = 'unpublished-' + secrets.token_hex(6)
        request('api/content/' + first + '/draft', 'POST')
        request('api/content/' + first + '/draft', 'PUT', {'HtmlBodyPart': {'Html': '<p>' + draft_marker + '</p>'}})
        placement = {'layer': name, 'zone': 'Content', 'position': 2, 'renderTitle': False}
        pomi('layers', 'widgets', 'update', first, body=placement, client='cli-layers', status=403)
        pomi('layers', 'widgets', 'update', first, body=placement, client='cli-widgets-editor', status=403)
        assert not request('api/content/' + first + '?version=latest').get('LayerMetadata')
        saved = pomi('layers', 'widgets', 'update', first, body=placement)
        assert saved == pomi('layers', 'widgets', 'update', first, body=placement)
        pomi('layers', 'widgets', 'update', second, body={**placement, 'position': 1})
        html = request(name.lower(), client=None, raw=True)
        assert html.index(markers[1]) < html.index(markers[0]) and draft_marker not in html
        draft = request('api/content/' + first + '?version=latest')
        published = request('api/content/' + first + '?version=published')
        assert not draft['Published'] and draft_marker in draft['HtmlBodyPart']['Html']
        assert published['Published'] and markers[0] in published['HtmlBodyPart']['Html']
        assert draft['ContentItemVersionId'] != published['ContentItemVersionId']
        assert draft['LayerMetadata'] == published['LayerMetadata']
        listing = pomi('layers', 'widgets', 'list', '--layer', name, '--take', '1')
        assert listing['totalCount'] == 2 and listing['items'][0]['contentItemId'] == second
        for invalid in [{**placement, 'layer': 'missing-layer'}, {**placement, 'zone': 'content'}]:
            pomi('layers', 'widgets', 'update', first, body=invalid, status=400)
        for invalid in [{'layer': name, 'zone': 'Content'}, {**placement, 'unknown': True}]:
            pomi('layers', 'widgets', 'update', first, body=invalid, failure=True)
            request('api/layer-widgets/' + first, 'PUT', invalid, client='cli-widgets', status=400, raw=True)
        assert pomi('layers', 'widgets', 'show', first)['placement'] == placement
        pomi('layers', 'widgets', 'update', page['ContentItemId'], body=placement, status=400)
        moved = {**placement, 'zone': 'Footer', 'position': -1}
        assert tool('update', {'path': {'contentItemId': first}, 'body': moved}) == moved
        assert tool('show', {'path': {'contentItemId': first}})['placement'] == moved
        html = request(name.lower(), client=None, raw=True)
        assert markers[0] in html and markers[1] in html and draft_marker not in html
        assert request('api/content/' + first + '?version=published')['LayerMetadata']['Zone'] == 'Footer'
        print('Widget attachment, ordering, invalid input, retries, version preservation and MCP moves: passed', flush=True)

        for path, method, body in [('api/layer-widgets', 'GET', None), ('api/layer-widget-zones', 'GET', None),
                ('api/layer-widgets/' + first, 'GET', None), ('api/layer-widgets/' + first, 'PUT', placement)]:
            request(path, method, body, client=None, status=401)
            request(path, method, body, client='cli-discovery', status=403)
        tool('update', {'path': {'contentItemId': first}, 'body': placement}, client='cli-layers', status=403)
        print('HTTP/MCP module and content edit/publish permissions: passed', flush=True)

        ids, commands, names = catalog()
        assert len([command for command in commands if command.startswith('layers widgets ')]) == 4
        assert len([entry for entry in names if entry.startswith('layers_widgets_')]) == 4
        feature('OrchardCore.RemoteManagement.Cli', False)
        assert not catalog()[1]
        assert tool('show', {'path': {'contentItemId': first}})['placement'] == moved
        feature('OrchardCore.RemoteManagement.Cli', True)
        feature('OrchardCore.Layers', False)
        assert not any(entry.startswith('layers_widgets_') for entry in catalog()[2])
        request('api/layer-widgets', status=404, raw=True)
        feature('OrchardCore.Layers', True)
        assert set(catalog()[1]) == set(commands)
        print('Unique discovery, feature removal/restoration and MCP with CLI disabled: passed', flush=True)
    finally:
        feature('OrchardCore.RemoteManagement.Cli', True)
        feature('OrchardCore.Layers', True)
        for item_id in reversed(items):
            pomi('content', 'items', 'delete', item_id, '--force', client='cli-fixture')
        for type_name in reversed(types):
            pomi('content', 'types', 'delete', type_name, '--force', client='cli-fixture')
        pomi('layers', 'delete', name, '--force')

print('Widgets smoke passed; changes remain confined to the disposable fixture.')
