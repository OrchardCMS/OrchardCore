#!/usr/bin/env python3
"""Verify per-resource recipe exports: one step, permissions, 404 and a real export, delete, import round trip."""
import json
import os
from pathlib import Path
import secrets
import ssl
import subprocess
import sys
import tempfile
import time
import urllib.error
import urllib.parse
import urllib.request

state_path = Path(sys.argv[1])
state = json.loads(state_path.read_text())
base = state['url']
assert urllib.parse.urlparse(base).hostname in ('127.0.0.1', 'localhost', '::1')
context = ssl.create_default_context(cafile=state.get('certificatePath'))
urllib.request.install_opener(urllib.request.build_opener(urllib.request.HTTPSHandler(context=context)))
wrapper = Path(__file__).with_name('pomi-fixture.py')
marker = secrets.token_hex(4)
tokens = {}
secrets_by_client = {}


def token(client):
    if client not in tokens:
        body = urllib.parse.urlencode(dict(grant_type='client_credentials', client_id=client,
            client_secret=secrets_by_client.get(client, state['OC_CLIENT_SECRET']), scope='orchardcore.management')).encode()
        with urllib.request.urlopen(urllib.request.Request(base + 'connect/token', data=body,
                headers={'Content-Type': 'application/x-www-form-urlencoded'}), timeout=30) as response:
            tokens[client] = json.load(response)['access_token']
    return tokens[client]


def request(path, method='GET', body=None, client='cli-fixture', status=200):
    url = urllib.parse.urljoin(base, path)
    assert urllib.parse.urlparse(url).netloc == urllib.parse.urlparse(base).netloc
    data = body if isinstance(body, bytes) else json.dumps(body).encode() if body is not None else None
    headers = {'Content-Type': 'application/octet-stream' if isinstance(body, bytes) else 'application/json',
        'Accept': 'application/json, text/event-stream'}
    if client:
        headers['Authorization'] = 'Bearer ' + token(client)
    try:
        response = urllib.request.urlopen(urllib.request.Request(url, method=method, headers=headers, data=data), timeout=60)
    except urllib.error.HTTPError as error:
        response = error
    with response:
        text = response.read().decode()
        allowed = status if isinstance(status, tuple) else (status,)
        assert response.status in allowed, (method, path, response.status, text[:600])
        if response.headers.get('Content-Type', '').startswith('text/event-stream'):
            return next(json.loads(line[6:]) for line in text.splitlines()
                if line.startswith('data: ') and json.loads(line[6:]).get('id') == 1)
        return json.loads(text) if text else None


def pomi(*args, body=None):
    env = os.environ.copy()
    env.update(OC_FIXTURE_CONFIG_HOME=config_home, OC_FIXTURE_CLIENT_ID='cli-fixture')
    command = [sys.executable, str(wrapper), str(state_path), *args, '--output', 'json']
    if body is not None:
        command.append('--stdin')
    result = subprocess.run(command, input=json.dumps(body) if body is not None else None,
        capture_output=True, text=True, env=env, timeout=120)
    assert result.returncode == 0, (args[:3], (result.stderr or result.stdout)[:800])
    return json.loads(result.stdout) if result.stdout.strip() else None


def completed(operation):
    deadline = time.monotonic() + 300
    while time.monotonic() < deadline:
        current = request('api/deployment/operations/' + operation['id'])
        if current['state'] in ('succeeded', 'failed', 'uncertain'):
            assert current['state'] == 'succeeded', (current['state'], current.get('errorCode'))
            return current
        time.sleep(3)
    raise AssertionError('The deployment worker did not finish before the deadline')


def comparable(definition):
    # A role lists the whole permission catalog, which grows with the resources created since.
    if 'permissions' in definition:
        return {**definition, 'permissions': [item for item in definition['permissions'] if item['isAssigned'] or item['isEffective']]}
    return definition


def named(value, name):
    if isinstance(value, dict):
        return next((entry for key, entry in value.items() if key.lower() == name.lower()), None)
    return next((entry for entry in value if entry.get('Name', '').lower() == name.lower()), None)


def resources():
    query = lambda name: {'name': name, 'source': 'Sql', 'returnContentItems': False,
        'properties': {'SqlQueryMetadata': {'Template': 'select DocumentId from ContentItemIndex'}}}
    role = lambda name: {'roleName': name, 'roleDescription': 'Exported role', 'permissionNames': ['AccessRemoteManagement']}
    template = lambda name: {'name': name, 'description': 'Exported template', 'content': '<b>{{ Model.Content }}</b>'}
    layer = lambda name: {'name': name, 'description': 'Exported layer', 'conditions': [
        {'name': 'UrlCondition', 'properties': {'value': '/news', 'operation': 'StringStartsWithOperator'}}]}
    placement = lambda name: {'shapeType': name, 'nodes': [{'place': 'Content:1'}]}
    shortcode = lambda name: {'name': name, 'content': '<b>{{ Content }}</b>', 'hint': 'Exported shortcode'}
    return [
        dict(step='Queries', create='api/queries', body=query, read=lambda name: f'api/queries/named/{name}',
            export=lambda name: f'api/queries/named/{name}:export', cli=['queries', 'export'], tool='queries_export', arguments=lambda name: {'path': {'name': name}}, denied=None),
        dict(step='Roles', create='api/roles', body=role, read=lambda name: f'api/roles/{name}',
            export=lambda name: f'api/roles/{name}:export', cli=['roles', 'export'], tool='roles_export', arguments=lambda name: {'path': {'roleId': name}}, denied=None),
        dict(step='Templates', create='api/templates', body=template, read=lambda name: f'api/templates/{name}',
            export=lambda name: f'api/templates/{name}:export', cli=['templates', 'export'], tool='templates_export', arguments=lambda name: {'path': {'name': name}}, denied=None),
        dict(step='Layers', create='api/layers', body=layer, read=lambda name: f'api/layers/by-name?name={name}',
            export=lambda name: f'api/layers/by-name:export?name={name}', cli=['layers', 'export'], tool='layers_export', arguments=lambda name: {'query': {'name': name}}, denied='cli-layers'),
        dict(step='Placements', create='api/placements', body=placement, read=lambda name: f'api/placements/by-shape?shapeType={name}',
            export=lambda name: f'api/placements/by-shape:export?shapeType={name}', cli=['placements', 'export'], tool='placements_export', arguments=lambda name: {'query': {'shapeType': name}}, denied='cli-placements'),
        dict(step='ShortcodeTemplates', create='api/shortcode-templates', body=shortcode, read=lambda name: f'api/shortcode-templates/by-name?name={name}',
            export=lambda name: f'api/shortcode-templates/by-name:export?name={name}', cli=['shortcodes', 'templates', 'export'], tool='shortcodes_templates_export', arguments=lambda name: {'query': {'name': name}}, denied='cli-shortcodes'),
    ]


with tempfile.TemporaryDirectory(prefix='recipe-export-smoke-') as config_home:
    for feature in ['OrchardCore.Deployment', 'OrchardCore.Layers', 'OrchardCore.Placements', 'OrchardCore.Queries.Sql',
            'OrchardCore.Shortcodes.Templates', 'OrchardCore.Templates', 'OrchardCore.RemoteManagement.Mcp']:
        request('api/features/' + feature + ':enable?force=true', 'POST')
    pomi('context', 'add', 'recipe-export', base, '--current')
    pomi('api', 'refresh', '--force')

    document = request('openapi/v1.json')
    tools = {tool['name']: tool for tool in request('mcp', 'POST', {'jsonrpc': '2.0', 'id': 1, 'method': 'tools/list'})['result']['tools']}

    recipes = []
    for resource in resources():
        name = ('Export' + resource['step'] + marker).lower()
        other = ('Other' + resource['step'] + marker).lower()
        for value in (name, other):
            request(resource['create'], 'POST', resource['body'](value), status=(200, 201))
        original = request(resource['read'](name))

        # Pomi and MCP go through the same route, and the OpenAPI document describes them.
        recipe = pomi(*resource['cli'], name)
        assert recipe == request(resource['export'](name)), resource['step']
        step, = recipe['steps']
        assert step['name'].lower() == resource['step'].lower(), step['name']
        assert len(step[resource['step']]) == 1, step
        assert named(step[resource['step']], name) is not None, step
        assert named(step[resource['step']], other) is None, step
        assert recipe['issetuprecipe'] is False
        assert tools[resource['tool']]['annotations']['readOnlyHint'] is True, resource['tool']
        called = request('mcp', 'POST', {'jsonrpc': '2.0', 'id': 1, 'method': 'tools/call',
            'params': {'name': resource['tool'], 'arguments': resource['arguments'](name)}})['result']
        content = json.loads(called['content'][0]['text'])
        assert content['statusCode'] == 200 and content['body'] == recipe, resource['tool']

        # A missing name is a 404, an anonymous call a 401, and Export or the resource permission alone a 403.
        request(resource['export']('missing' + marker), status=404)
        request(resource['export'](name), client=None, status=401)
        request(resource['export'](name), client='cli-denied', status=403)
        request(resource['export'](name), client='cli-content-export-no-edit', status=403)
        if resource['denied']:
            request(resource['export'](name), client=resource['denied'], status=403)

        recipes.append((resource, name, original, recipe))
        print('PASS: exported', resource['step'], flush=True)

    # The two permissions together are enough, without any other one.
    role = 'CliExportLayers' + marker
    client = 'cli-export-layers-' + marker
    secrets_by_client[client] = secrets.token_urlsafe(32)
    request('api/roles', 'POST', {'roleName': role, 'permissionNames': ['AccessRemoteManagement', 'Export', 'ManageLayers']}, status=(200, 201))
    request('api/openid/applications', 'POST', {'clientId': client, 'displayName': client, 'clientType': 'confidential', 'consentType': 'implicit',
        'clientSecret': secrets_by_client[client], 'allowClientCredentialsFlow': True, 'roles': [role], 'scopes': ['orchardcore.management']}, status=(200, 201))
    layers = next(entry for entry in recipes if entry[0]['step'] == 'Layers')
    assert request(layers[0]['export'](layers[1]), client=client)['steps'] == layers[3]['steps']
    print('PASS: Export with the resource permission is enough', flush=True)

    # Delete, then execute each recipe through the real deployment import.
    operations = []
    for resource, name, original, recipe in recipes:
        request(resource['read'](name), 'DELETE', status=(200, 204))
        request(resource['read'](name), status=404)
        artifact = request(f'api/deployment/artifacts?fileName=export-{resource["step"].lower()}.json', 'POST', json.dumps(recipe).encode())
        operation = request('api/deployment/operations:import', 'POST', {'requestId': f'{marker}-{resource["step"]}', 'artifactId': artifact['id']}, status=202)
        operations.append(operation)
    for operation in operations:
        completed(operation)
    for resource, name, original, recipe in recipes:
        restored = request(resource['read'](name))
        assert comparable(restored) == comparable(original), (resource['step'], comparable(original), comparable(restored))
        print('PASS: restored', resource['step'], flush=True)

    for path in ['/api/queries/named/{name}:export', '/api/roles/{roleId}:export', '/api/templates/{name}:export',
            '/api/layers/by-name:export', '/api/placements/by-shape:export', '/api/shortcode-templates/by-name:export']:
        assert 'get' in document['paths'][path], path

    # Without Deployment, the routes don't exist anymore.
    request('api/features/OrchardCore.Deployment:disable?force=true', 'POST')
    deadline = time.monotonic() + 60
    while time.monotonic() < deadline:
        paths = request('openapi/v1.json')['paths']
        if '/api/layers/by-name:export' not in paths:
            break
        time.sleep(2)
    assert '/api/layers/by-name:export' not in paths
    assert '/api/queries/named/{name}:export' not in paths
    request('api/features/OrchardCore.Deployment:enable?force=true', 'POST')

print('All recipe export checks passed', flush=True)
