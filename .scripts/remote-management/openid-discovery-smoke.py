#!/usr/bin/env python3
"""Verify redacted OpenID application/scope discovery through Pomi and MCP."""
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
name = 'OpenIdDiscoverySmoke' + secrets.token_hex(4)
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


def pomi(*args, body=None, client='cli-openid-applications', status=None, failure=False):
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


def tool(resource, verb, arguments=None, client='cli-openid-applications', status=200):
    result = request('mcp', 'POST', {'jsonrpc': '2.0', 'id': 1, 'method': 'tools/call',
        'params': {'name': 'openid_' + resource + '_' + verb, 'arguments': arguments or {}}}, client=client)['result']
    text = result['content'][0]['text']
    assert text.startswith('{'), text[:600]
    content = json.loads(text)
    assert content['statusCode'] == status, content
    assert result.get('isError', False) == (status >= 400)
    return json.loads(content['body']) if content['body'] else None


with tempfile.TemporaryDirectory(prefix='openid-discovery-cli-', dir=state_path.parent) as config_home:
    pomi('context', 'add', name, base, '--current')
    request('api/features/OrchardCore.RemoteManagement.Mcp:enable?force=true', 'POST')
    pomi('api', 'refresh', '--force')
    for resource, client, selector, key in [
            ('applications', 'cli-openid-applications', 'cli-fixture', 'clientId'),
            ('scopes', 'cli-openid-scopes', 'orchardcore.management', 'name')]:
        path = 'api/openid/' + resource
        show_path = path + ('/by-client-id?' if resource == 'applications' else '/by-name?') + urllib.parse.urlencode({key: selector})
        page = pomi('openid', resource, 'list', '--take', '1', client=client)
        assert page['take'] == 1 and page['skip'] == 0 and page['totalCount'] > 0
        assert len(page['items']) == 1
        assert pomi('openid', resource, 'list', '--skip', str(page['totalCount']), client=client)['items'] == []
        shown = pomi('openid', resource, 'show', selector, client=client)
        assert shown[key] == selector
        expected_fields = ({'id', 'clientId', 'displayName', 'clientType', 'applicationType', 'consentType',
            'roles', 'permissions', 'requirements', 'redirectUris', 'postLogoutRedirectUris'} if resource == 'applications'
            else {'id', 'name', 'displayName', 'description', 'resources'})
        assert set(shown) == expected_fields
        assert state['OC_CLIENT_SECRET'] not in json.dumps(shown)
        assert request(show_path, client=client) == shown
        assert tool(resource, 'show', {'query': {key: selector}}, client=client) == shown
        assert tool(resource, 'list', {'query': {'take': 1}}, client=client) == page
        pomi('openid', resource, 'show', 'missing-' + name, client=client, status=404)
        for query in ['skip=-1', 'take=0', 'take=201']:
            request(path + '?' + query, client=client, status=400)
        request(path, client=None, status=401)
        other = 'cli-openid-scopes' if resource == 'applications' else 'cli-openid-applications'
        for denied in ['cli-denied', 'cli-discovery', other]:
            request(path, client=denied, status=403)
            request(show_path, client=denied, status=403)
            if denied != 'cli-denied':
                tool(resource, 'show', {'query': {key: selector}}, client=denied, status=403)
    manifest = request('api/management/manifest')
    document = request(manifest['openApiUrl'])
    operations = [operation for path in document['paths'].values() for operation in path.values()
        if isinstance(operation, dict) and operation.get('operationId')]
    ids = [operation['operationId'] for operation in operations]
    assert len(ids) == len(set(ids))
    tools = request('mcp', 'POST', {'jsonrpc': '2.0', 'id': 1, 'method': 'tools/list', 'params': {}})['result']['tools']
    names = [item['name'] for item in tools]
    assert len(names) == len(set(names))
    assert {'openid_applications_list', 'openid_applications_show', 'openid_scopes_list', 'openid_scopes_show'} <= set(names)
    request('api/features/OrchardCore.RemoteManagement.Cli:disable', 'POST')
    try:
        assert tool('applications', 'show', {'query': {'clientId': 'cli-fixture'}})['clientId'] == 'cli-fixture'
        assert tool('scopes', 'show', {'query': {'name': 'orchardcore.management'}}, client='cli-openid-scopes')['name'] == 'orchardcore.management'
    finally:
        request('api/features/OrchardCore.RemoteManagement.Cli:enable?force=true', 'POST')
    print('PASS: bounded application/scope discovery, explicit redacted fields, separate permissions, HTTP/CLI/MCP equivalence and MCP without CLI.')
