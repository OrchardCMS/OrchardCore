#!/usr/bin/env python3
"""Verify security header settings, emitted headers, permissions and feature lifecycle."""
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
name = 'SecurityHeadersSmoke' + secrets.token_hex(4)
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


def pomi(*args, body=None, client='cli-security-headers', status=None, failure=False):
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


def tool(verb, arguments=None, client='cli-security-headers', status=200):
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


with tempfile.TemporaryDirectory(prefix='security-headers-cli-', dir=state_path.parent) as config_home:
    pomi('context', 'add', name, base, '--current')
    feature('OrchardCore.Security', True)
    feature('OrchardCore.Https', True)
    feature('OrchardCore.RemoteManagement.Mcp', True)
    pomi('api', 'refresh', '--force')
    assert [item['name'] for item in pomi('settings', 'sections', 'list')] == ['security-headers']
    assert {'security-headers', 'https'} <= {item['name'] for item in request('api/settings/sections')}
    assert pomi('settings', 'sections', 'schema', 'security-headers')['properties']['contentSecurityPolicy']['type'] == 'object'
    request('api/settings/sections/security-headers', 'PUT', {'contentSecurityPolicy': {}, 'permissionsPolicy': {}, 'referrerPolicy': 'no-referrer'})
    initial = pomi('settings', 'sections', 'show', 'security-headers')
    assert initial['source'] == 'tenant' and not initial['isReadOnly']
    assert not pomi('settings', 'sections', 'update', 'security-headers', body={})['changed']
    policy = {'contentSecurityPolicy': {'default-src': "'self'", 'img-src': "'self' data:"},
        'permissionsPolicy': {'camera': 'self', 'microphone': '*'}, 'referrerPolicy': 'strict-origin'}
    result = pomi('settings', 'sections', 'update', 'security-headers', body=policy)
    assert result['changed'] and result['reloadRequested']
    assert result['section']['values'] == policy
    retry = pomi('settings', 'sections', 'update', 'security-headers', body=policy)
    assert not retry['changed'] and not retry['reloadRequested']
    _, headers = request('api/settings/sections', client=None, status=401, raw=True)
    assert headers['Content-Security-Policy'] == "default-src 'self';img-src 'self' data:", headers
    assert headers['Permissions-Policy'] == 'camera=self,microphone=*', headers
    assert headers['Referrer-Policy'] == 'strict-origin', headers
    assert headers['X-Content-Type-Options'] == 'nosniff', headers
    for invalid in [{'contentSecurityPolicy': None}, {'permissionsPolicy': []}, {'referrerPolicy': None},
            {'referrerPolicy': 'invalid'}, {'contentTypeOptions': 'off'},
            {'contentSecurityPolicy': {'default-src': "'self';script-src *"}},
            {'contentSecurityPolicy': {'default-src': "self\r\nInjected: yes"}},
            {'permissionsPolicy': {'camera': None}}, {'permissionsPolicy': {'camera': 'self,microphone=*'}}]:
        pomi('settings', 'sections', 'update', 'security-headers', body=invalid, status=400)
    assert pomi('settings', 'sections', 'show', 'security-headers')['values'] == policy
    for client in ['cli-denied', 'cli-discovery', 'cli-https']:
        for method in ['GET', 'PUT']:
            request('api/settings/sections/security-headers', method, {} if method == 'PUT' else None,
                client=client, status=403)
        if client == 'cli-denied':
            request('mcp', 'POST', {'jsonrpc': '2.0', 'id': 1, 'method': 'tools/list', 'params': {}}, client=client, status=403)
        else:
            tool('show', {'path': {'name': 'security-headers'}}, client=client, status=403)
    assert tool('show', {'path': {'name': 'security-headers'}})['values'] == policy
    flags = {'contentSecurityPolicy': {'sandbox': None, 'upgrade-insecure-requests': None}}
    result = tool('update', {'path': {'name': 'security-headers'}, 'body': flags})
    assert result['changed']
    _, headers = request('api/settings/sections', client=None, status=401, raw=True)
    assert headers['Content-Security-Policy'] == 'sandbox;upgrade-insecure-requests', headers
    assert headers['Permissions-Policy'] == 'camera=self,microphone=*', headers
    assert not tool('update', {'path': {'name': 'security-headers'}, 'body': flags})['changed']
    pomi('settings', 'sections', 'update', 'security-headers', body={'contentSecurityPolicy': {}, 'permissionsPolicy': {}})
    _, headers = request('api/settings/sections', client=None, status=401, raw=True)
    assert 'Content-Security-Policy' not in headers and 'Permissions-Policy' not in headers
    ids, commands, names = catalog()
    assert len([command for command in commands if command.startswith('settings sections ')]) == 4
    feature('OrchardCore.Security', False)
    request('api/settings/sections/security-headers', status=404)
    assert 'https' in [item['name'] for item in request('api/settings/sections')]
    feature('OrchardCore.Security', True)
    assert request('api/settings/sections/security-headers')['values']['referrerPolicy'] == 'strict-origin'
    feature('OrchardCore.RemoteManagement.Cli', False)
    assert tool('show', {'path': {'name': 'security-headers'}})['name'] == 'security-headers'
    feature('OrchardCore.RemoteManagement.Cli', True)
    request('api/settings/sections/security-headers', 'PUT', initial['values'])
    print('PASS: security headers shared settings validation, replacement, retries and emitted response headers')
    print('PASS: security headers HTTP/CLI/MCP authorization and feature lifecycle')
