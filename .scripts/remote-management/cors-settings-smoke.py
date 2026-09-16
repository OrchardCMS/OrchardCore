#!/usr/bin/env python3
"""Verify CORS policy sections, live preflight behavior, permissions and feature lifecycle."""
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
name = 'CorsSmoke' + secrets.token_hex(4)
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


def pomi(*args, body=None, client='cli-cors', status=None, failure=False):
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


def tool(verb, arguments=None, client='cli-cors', status=200):
    result = request('mcp', 'POST', {'jsonrpc': '2.0', 'id': 1, 'method': 'tools/call',
        'params': {'name': 'settings_sections_' + verb, 'arguments': arguments or {}}}, client=client)['result']
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


def probe(origin, method='OPTIONS', requested_method='PUT', requested_headers='X-Pomi-Probe', status=204):
    headers = {'Origin': origin}
    if method == 'OPTIONS':
        headers.update({'Access-Control-Request-Method': requested_method, 'Access-Control-Request-Headers': requested_headers})
    req = urllib.request.Request(base + 'api/settings/sections', method=method, headers=headers)
    try:
        response = urllib.request.urlopen(req, timeout=30)
    except urllib.error.HTTPError as error:
        response = error
    with response:
        response.read()
        assert response.status == status, (method, origin, response.status, status)
        return response.headers


with tempfile.TemporaryDirectory(prefix='cors-cli-', dir=state_path.parent) as config_home:
    pomi('context', 'add', name, base, '--current')
    feature('OrchardCore.Cors', True)
    feature('OrchardCore.Https', True)  # Two independent providers share the four section operations.
    feature('OrchardCore.RemoteManagement.Mcp', True)
    pomi('api', 'refresh', '--force')
    assert [item['name'] for item in pomi('settings', 'sections', 'list')] == ['cors']
    assert {'cors', 'https'} <= {item['name'] for item in request('api/settings/sections')}
    schema = pomi('settings', 'sections', 'schema', 'cors')
    assert schema['properties']['policies']['type'] == 'array'
    before = pomi('settings', 'sections', 'show', 'cors')
    assert before['source'] == 'tenant' and not before['isReadOnly']
    assert not pomi('settings', 'sections', 'update', 'cors', body={})['changed']
    origin, partner = 'https://frontend.example.test', 'https://partner.example.test'
    policy = {'name': name, 'allowedOrigins': [origin], 'allowedMethods': ['PUT'], 'allowedHeaders': ['X-Pomi-Probe'],
        'allowCredentials': True, 'exposedHeaders': ['X-Pomi-Result']}
    patch = {'policies': [policy]}
    saved = pomi('settings', 'sections', 'update', 'cors', body=patch)
    assert saved['changed'] and saved['reloadRequested']
    assert not pomi('settings', 'sections', 'update', 'cors', body=patch)['changed']
    assert pomi('settings', 'sections', 'show', 'cors') == saved['section']
    # No explicit default flag: the first configured policy must apply.
    headers = probe(origin)
    assert headers.get('Access-Control-Allow-Origin') == origin
    assert headers.get('Access-Control-Allow-Credentials') == 'true'
    assert 'put' in headers.get('Access-Control-Allow-Methods', '').lower()
    assert 'x-pomi-probe' in headers.get('Access-Control-Allow-Headers', '').lower()
    assert not probe(partner).get('Access-Control-Allow-Origin')
    assert 'delete' not in probe(origin, requested_method='DELETE').get('Access-Control-Allow-Methods', '').lower()
    assert 'x-unlisted' not in probe(origin, requested_headers='X-Unlisted').get('Access-Control-Allow-Headers', '').lower()
    simple = probe(origin, method='GET', status=401)
    assert simple.get('Access-Control-Allow-Origin') == origin
    assert 'x-pomi-result' in simple.get('Access-Control-Expose-Headers', '').lower()
    route = 'api/settings/sections/cors'
    for client, expected in [(None, 401), ('cli-denied', 403), ('cli-discovery', 403), ('cli-https', 403)]:
        request(route, 'PUT', patch, client=client, status=expected)
    for invalid in [{'policies': None}, {'policies': [dict(policy, allowAnyOrigin=True)]},
            {'policies': [dict(policy, allowedOrigins=['*'])]}, {'policies': [dict(policy, allowedOrigins=[origin + '/'])]},
            {'policies': [dict(policy, allowedOrigins=[origin + '/a/..'])]},
            {'policies': [dict(policy, allowedOrigins=['https://*.example.test'])]},
            {'policies': [dict(policy, allowedHeaders=['X-Bad:Value'])]}, {'policies': [policy, policy]},
            {'policies': [dict(policy, isDefaultPolicy=True), dict(policy, name='Other', isDefaultPolicy=True)]},
            {'policies': [dict(policy, secretValue='not-a-property')]}]:
        request(route, 'PUT', invalid, status=400)
    assert pomi('settings', 'sections', 'show', 'cors') == saved['section']
    assert probe(origin).get('Access-Control-Allow-Origin') == origin
    second = dict(policy, name='Partner', allowedOrigins=[partner], isDefaultPolicy=True)
    changed = tool('update', {'path': {'name': 'cors'}, 'body': {'policies': [policy, second]}})
    assert changed['changed']
    assert probe(partner).get('Access-Control-Allow-Origin') == partner
    assert not probe(origin).get('Access-Control-Allow-Origin')
    tool('update', {'path': {'name': 'cors'}, 'body': {}}, client='cli-discovery', status=403)
    wildcard = dict(policy, allowedOrigins=['*'], allowCredentials=False, allowAnyMethod=True, allowAnyHeader=True)
    pomi('settings', 'sections', 'update', 'cors', body={'policies': [wildcard]})
    headers = probe(partner)
    assert headers.get('Access-Control-Allow-Origin') == '*'
    assert not headers.get('Access-Control-Allow-Credentials')
    cleared = pomi('settings', 'sections', 'update', 'cors', body={'policies': []})
    assert cleared['changed'] and cleared['section']['values']['policies'] == []
    assert not pomi('settings', 'sections', 'update', 'cors', body={'policies': []})['changed']
    assert not probe(origin, status=404).get('Access-Control-Allow-Origin')
    assert not probe(origin, method='GET', status=401).get('Access-Control-Allow-Origin')
    ids, commands, names = catalog()
    assert len([command for command in commands if command.startswith('settings sections ')]) == 4
    assert len([tool_name for tool_name in names if tool_name.startswith('settings_sections_')]) == 4
    feature('OrchardCore.Cors', False)
    assert not any(item['name'] == 'cors' for item in request('api/settings/sections'))
    request(route, status=404)
    assert any(item['name'] == 'https' for item in request('api/settings/sections'))
    feature('OrchardCore.Cors', True)
    feature('OrchardCore.RemoteManagement.Cli', False)
    assert tool('show', {'path': {'name': 'cors'}})['values']['policies'] == []
    feature('OrchardCore.RemoteManagement.Cli', True)
    print('CORS: shared sections, replacement/omission/empty-array semantics, validation and no-op retries passed.', flush=True)
    print('Real default/selected policy preflights, origin/method/header/credential behavior and reloads passed.', flush=True)
    print('HTTP/MCP permission denials, two providers without duplicate operations, feature lifecycle and MCP without CLI passed.', flush=True)
