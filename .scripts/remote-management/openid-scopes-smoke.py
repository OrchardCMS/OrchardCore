#!/usr/bin/env python3
"""Verify OpenID scope mutations, retries, validation and shared readback through Pomi and MCP."""
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
name = 'ScopeManagementSmoke' + secrets.token_hex(4)
tokens = {}


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


def pomi(*args, body=None, client='cli-openid-scopes', status=None, failure=False):
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


def tool(resource, verb, arguments=None, client='cli-openid-scopes', status=200):
    result = request('mcp', 'POST', {'jsonrpc': '2.0', 'id': 1, 'method': 'tools/call',
        'params': {'name': 'openid_' + resource + '_' + verb, 'arguments': arguments or {}}}, client=client)['result']
    text = result['content'][0]['text']
    assert text.startswith('{'), text[:600]
    content = json.loads(text)
    assert content['statusCode'] == status, content
    assert result.get('isError', False) == (status >= 400)
    return json.loads(content['body']) if content['body'] else None


with tempfile.TemporaryDirectory(prefix='openid-scopes-cli-', dir=state_path.parent) as config_home:
    pomi('context', 'add', name, base, '--current')
    request('api/features/OrchardCore.RemoteManagement.Mcp:enable?force=true', 'POST')
    pomi('api', 'refresh', '--force')
    assert pomi('openid', 'scopes', 'schema', '--operation', 'create')
    path = 'api/openid/scopes/by-name?' + urllib.parse.urlencode({'name': name})
    body = {'name': name, 'displayName': 'Scope smoke', 'description': 'Disposable', 'resources': ['resource-b', 'resource-a', 'resource-a']}
    created = pomi('openid', 'scopes', 'create', body=body)
    try:
        assert set(created['resources']) == {'resource-a', 'resource-b'}
        assert pomi('openid', 'scopes', 'create', body=body) == created
        assert tool('scopes', 'create', {'body': body}) == created
        pomi('openid', 'scopes', 'create', body={**body, 'displayName': 'Conflicting'}, status=409)
        assert pomi('openid', 'scopes', 'show', name) == created
        pomi('openid', 'scopes', 'create', body={**body, 'name': name + ' invalid'}, status=400)
        pomi('openid', 'scopes', 'show', name + ' invalid', status=404)
        for invalid in [{'displayName': None}, {'name': 'invalid scope'}, {'resources': None},
                {'resources': [None]}, {'resources': ['']}, {'resources': ['two resources']},
                {'resources': ['oct:Default']}, {'properties': {'unknown': 'cannot-write'}}]:
            pomi('openid', 'scopes', 'update', name, body={**body, **invalid}, status=400)
        assert pomi('openid', 'scopes', 'show', name) == created
        for denied in ['cli-denied', 'cli-discovery', 'cli-openid-applications']:
            request('api/openid/scopes', 'POST', body, client=denied, status=403)
            request(path, 'PUT', body, client=denied, status=403)
            request(path, 'DELETE', client=denied, status=403)
            if denied != 'cli-denied':
                tool('scopes', 'update', {'query': {'name': name}, 'body': body}, client=denied, status=403)
        request('api/openid/scopes', 'POST', body, client=None, status=401)
        replacement = {'name': name, 'displayName': 'Updated'}
        changed = pomi('openid', 'scopes', 'update', name, body=replacement)
        assert changed['id'] == created['id'] and changed['resources'] == [] and changed['description'] is None
        assert pomi('openid', 'scopes', 'update', name, body=replacement) == changed
        assert tool('scopes', 'update', {'query': {'name': name}, 'body': replacement}) == changed
        missing = name + '-missing'
        pomi('openid', 'scopes', 'update', missing, body={'name': missing, 'displayName': 'Missing'}, status=404)
        request('api/features/OrchardCore.RemoteManagement.Cli:disable', 'POST')
        try:
            assert tool('scopes', 'show', {'query': {'name': name}}) == changed
            tool('scopes', 'delete', {'query': {'name': name}}, status=204)
            tool('scopes', 'delete', {'query': {'name': name}}, status=204)
        finally:
            request('api/features/OrchardCore.RemoteManagement.Cli:enable?force=true', 'POST')
        pomi('openid', 'scopes', 'show', name, status=404)
        pomi('openid', 'scopes', 'delete', name, '--force')
        print('PASS: scope create/update/delete, equivalent retries, conflict/invalid/denied writes, HTTP/Pomi/MCP readback and MCP without CLI.')
    finally:
        request(path, 'DELETE', status=204)
