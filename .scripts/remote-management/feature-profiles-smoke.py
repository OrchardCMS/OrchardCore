#!/usr/bin/env python3
"""Verify tenant feature-profile discovery, shared mutations and permission checks through HTTP/Pomi/MCP."""
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
name = 'FeatureProfileSmoke' + secrets.token_hex(4)
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
        assert response.status == status, (method, path, response.status, status)
        if raw:
            return text, response.headers
        if response.headers.get('Content-Type', '').startswith('text/event-stream'):
            return next(json.loads(line[6:]) for line in text.splitlines()
                if line.startswith('data: ') and json.loads(line[6:]).get('id') == 1)
        return json.loads(text) if text and 'json' in response.headers.get('Content-Type', '') else None


def pomi(*args, body=None, client='cli-feature-profiles', status=None, failure=False):
    env = os.environ.copy()
    env.update(OC_FIXTURE_CONFIG_HOME=config_home, OC_FIXTURE_CLIENT_ID=client)
    selected_state = state_path
    command = [sys.executable, str(wrapper), str(selected_state), *args, '--output', 'json']
    if body is not None:
        command.append('--stdin')
    result = subprocess.run(command, input=json.dumps(body) if body is not None else None,
        capture_output=True, text=True, env=env, timeout=90)
    if failure:
        assert result.returncode != 0, args
        return
    if status:
        assert result.returncode != 0, args
        assert json.loads(result.stderr or result.stdout)['error']['status'] == status, 'unexpected CLI error status'
        return
    assert result.returncode == 0, (args[:3], 'CLI request failed')
    return json.loads(result.stdout) if result.stdout.strip() else None


def tool(verb, arguments=None, client='cli-feature-profiles', status=200):
    result = request('mcp', 'POST', {'jsonrpc': '2.0', 'id': 1, 'method': 'tools/call',
        'params': {'name': 'tenants_feature-profiles_' + verb, 'arguments': arguments or {}}}, client=client)['result']
    content = json.loads(result['content'][0]['text'])
    assert content['statusCode'] == status, (verb, content['statusCode'], status)
    assert result.get('isError', False) == (status >= 400)
    return json.loads(content['body']) if content['body'] else None


def catalog():
    result = request('mcp', 'POST', {'jsonrpc': '2.0', 'id': 1, 'method': 'tools/list', 'params': {}})['result']
    names = [entry['name'] for entry in result['tools']]
    assert len(names) == len(set(names)), 'duplicate MCP tool names'
    return [name for name in names if name.startswith('tenants_feature-profiles_')]


with tempfile.TemporaryDirectory(prefix='feature-profiles-cli-', dir=state_path.parent) as config_home:
    pomi('context', 'add', name, base, '--current')
    request('api/features/OrchardCore.RemoteManagement.Mcp:enable?force=true', 'POST')
    request('api/features/OrchardCore.Tenants.FeatureProfiles:enable?force=true', 'POST')
    pomi('api', 'refresh', '--force')
    assert len(catalog()) == 6
    schema = pomi('tenants', 'feature-profiles', 'schema')
    assert 'Include' in json.dumps(schema) and 'Exclude' in json.dumps(schema)
    path = 'api/tenants/feature-profiles/by-id?' + urllib.parse.urlencode({'id': name})
    body = {'id': name, 'name': name, 'featureRules': [
        {'rule': 'Exclude', 'expression': 'Custom.*'},
        {'rule': 'Include', 'expression': 'Custom.Allowed'}]}
    created = pomi('tenants', 'feature-profiles', 'create', body=body)
    try:
        assert created == body
        assert pomi('tenants', 'feature-profiles', 'create', body=body) == created
        assert tool('create', {'body': body}) == created
        page = pomi('tenants', 'feature-profiles', 'list', '--search', name, '--take', '1')
        assert page['totalCount'] == 1 and page['items'] == [body]
        assert request(path, client='cli-feature-profiles') == body
        assert tool('show', {'query': {'id': name}}) == body
        request(path, client=None, status=401)
        for denied in ['cli-denied', 'cli-discovery', 'cli-tenants']:
            request(path, client=denied, status=403)
            request(path, 'PUT', body, client=denied, status=403)
            if denied == 'cli-denied':
                request('mcp', 'POST', {'jsonrpc': '2.0', 'id': 1, 'method': 'tools/list', 'params': {}}, client=denied, status=403)
            else:
                tool('update', {'query': {'id': name}, 'body': body}, client=denied, status=403)
        for invalid in [{**body, 'featureRules': None}, {**body, 'featureRules': [None]},
                        {**body, 'featureRules': [{'rule': 'Unknown', 'expression': '*'}]},
                        {**body, 'extra': True}]:
            request(path, 'PUT', invalid, client='cli-feature-profiles', status=400)
            assert request(path, client='cli-feature-profiles') == body
        request('api/tenants/feature-profiles', 'POST', {**body, 'name': 'Conflicting'}, client='cli-feature-profiles', status=409)
        replacement = {**body, 'name': name + ' renamed', 'featureRules': list(reversed(body['featureRules']))}
        assert pomi('tenants', 'feature-profiles', 'update', name, body=replacement) == replacement
        assert tool('update', {'query': {'id': name}, 'body': replacement}) == replacement
        cleared = {**replacement, 'featureRules': []}
        assert request(path, 'PUT', {'id': name, 'name': cleared['name']}, client='cli-feature-profiles') == cleared
        request('api/features/OrchardCore.RemoteManagement.Cli:disable', 'POST')
        try:
            assert tool('show', {'query': {'id': name}}) == cleared
            assert len(catalog()) == 6
        finally:
            request('api/features/OrchardCore.RemoteManagement.Cli:enable?force=true', 'POST')
        request('api/features/OrchardCore.Tenants.FeatureProfiles:disable', 'POST')
        try:
            assert catalog() == []
            request(path, client='cli-feature-profiles', status=404)
        finally:
            request('api/features/OrchardCore.Tenants.FeatureProfiles:enable?force=true', 'POST')
        assert len(catalog()) == 6
        assert request(path, client='cli-feature-profiles') == cleared
        pomi('api', 'refresh', '--force')
        pomi('tenants', 'feature-profiles', 'delete', name, '--force')
        tool('delete', {'query': {'id': name}}, status=204)
        request(path, client='cli-feature-profiles', status=404)
    finally:
        request(path, 'DELETE', client='cli-feature-profiles', status=204)
print('PASS: profile CRUD, retries, schema, permissions, feature lifecycle and HTTP/Pomi/MCP parity')
