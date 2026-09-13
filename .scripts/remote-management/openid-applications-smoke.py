#!/usr/bin/env python3
"""Verify application CRUD, redaction, credentials and runtime permission changes through HTTP/Pomi/MCP."""
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
name = 'ApplicationManagementSmoke' + secrets.token_hex(4)
tokens = {}
application_secret = secrets.token_urlsafe(32)


def token(client):
    if client not in tokens:
        body = urllib.parse.urlencode(dict(grant_type='client_credentials', client_id=client,
            client_secret=application_secret if client == name else state['OC_CLIENT_SECRET'], scope='orchardcore.management')).encode()
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
        return json.loads(text) if text else None


def pomi(*args, body=None, client='cli-openid-applications', status=None, failure=False):
    env = os.environ.copy()
    env.update(OC_FIXTURE_CONFIG_HOME=config_home, OC_FIXTURE_CLIENT_ID=client)
    selected_state = Path(config_home) / 'application-client.json' if client == name else state_path
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


def tool(resource, verb, arguments=None, client='cli-openid-applications', status=200):
    result = request('mcp', 'POST', {'jsonrpc': '2.0', 'id': 1, 'method': 'tools/call',
        'params': {'name': 'openid_' + resource + '_' + verb, 'arguments': arguments or {}}}, client=client)['result']
    text = result['content'][0]['text']
    assert text.startswith('{'), 'unexpected MCP envelope'
    content = json.loads(text)
    assert content['statusCode'] == status, (content['statusCode'], status)
    assert result.get('isError', False) == (status >= 400)
    return json.loads(content['body']) if content['body'] else None


def authenticate_application(expected_error=None):
    tokens.pop(name, None)
    try:
        value = token(name)
    except urllib.error.HTTPError as error:
        with error:
            payload = json.load(error)
        assert expected_error and payload.get('error') == expected_error, 'Unexpected token rejection'
        return
    assert expected_error is None and value, 'Expected token rejection'


with tempfile.TemporaryDirectory(prefix='openid-applications-cli-', dir=state_path.parent) as config_home:
    child_state = {**state, 'OC_CLIENT_ID': name, 'OC_CLIENT_SECRET': application_secret}
    child_path = Path(config_home) / 'application-client.json'
    child_path.write_text(json.dumps(child_state))
    child_path.chmod(0o600)
    pomi('context', 'add', name, base, '--current')
    request('api/features/OrchardCore.RemoteManagement.Mcp:enable?force=true', 'POST')
    pomi('api', 'refresh', '--force')
    assert pomi('openid', 'applications', 'schema', '--operation', 'create')
    path = 'api/openid/applications/by-client-id?' + urllib.parse.urlencode({'clientId': name})
    body = {'clientId': name, 'displayName': 'Application smoke', 'clientType': 'confidential',
        'consentType': 'implicit', 'clientSecret': application_secret, 'allowClientCredentialsFlow': True,
        'roles': ['Cli-openid-scopes'], 'scopes': ['orchardcore.management']}
    created = pomi('openid', 'applications', 'create', body=body)
    try:
        assert created['clientId'] == name and created['roles'] == ['Cli-openid-scopes']
        assert application_secret not in json.dumps(created)
        assert not {'clientSecret', 'properties', 'jsonWebKeySet', 'settings'} & created.keys()
        assert pomi('openid', 'applications', 'create', body=body) == created
        assert tool('applications', 'create', {'body': body}) == created
        pomi('openid', 'applications', 'create', body={**body, 'displayName': 'Conflicting'}, status=409)
        pomi('openid', 'applications', 'create', body={**body, 'clientSecret': secrets.token_urlsafe(32)}, status=409)
        authenticate_application()
        request('api/openid/scopes', client=name)
        assert pomi('openid', 'scopes', 'list', client=name)['totalCount'] > 0
        pomi('openid', 'applications', 'show', name, client=name, status=403)
        request(path, client=name, status=403)
        request(path, 'DELETE', client=name, status=403)
        for invalid in [{'displayName': None}, {'clientType': 'other'}, {'applicationType': 'native'},
                {'clientType': 'public'}, {'roles': None}, {'roles': ['missing-role']},
                {'scopes': None}, {'scopes': ['missing-scope']}, {'redirectUris': 'not-a-uri'},
                {'redirectUris': 'https://example.com/callback#fragment'},
                {'properties': {'unknown': 'cannot-write'}}]:
            pomi('openid', 'applications', 'update', name, body={**body, **invalid}, status=400)
        assert pomi('openid', 'applications', 'show', name) == created
        for denied in ['cli-denied', 'cli-discovery', 'cli-openid-scopes']:
            request('api/openid/applications', 'POST', body, client=denied, status=403)
            request(path, 'PUT', body, client=denied, status=403)
            request(path, 'DELETE', client=denied, status=403)
            if denied != 'cli-denied':
                tool('applications', 'update', {'query': {'clientId': name}, 'body': body}, client=denied, status=403)
        request('api/openid/applications', 'POST', body, client=None, status=401)
        replacement = {key: value for key, value in body.items() if key != 'clientSecret'}
        replacement['displayName'] = 'Updated'
        changed = pomi('openid', 'applications', 'update', name, body=replacement)
        assert changed['id'] == created['id'] and changed['displayName'] == 'Updated'
        assert pomi('openid', 'applications', 'update', name, body=replacement) == changed
        assert tool('applications', 'update', {'query': {'clientId': name}, 'body': replacement}) == changed
        authenticate_application()
        request('api/openid/scopes', client=name)
        without_roles = {key: value for key, value in replacement.items() if key != 'roles'}
        changed = pomi('openid', 'applications', 'update', name, body=without_roles)
        assert changed['roles'] == []
        authenticate_application()
        request('api/openid/scopes', client=name, status=403)
        no_grant = {key: value for key, value in without_roles.items() if key not in ('scopes', 'allowClientCredentialsFlow')}
        changed = pomi('openid', 'applications', 'update', name, body=no_grant)
        assert 'gt:client_credentials' not in changed['permissions']
        authenticate_application(expected_error='unauthorized_client')
        changed = pomi('openid', 'applications', 'update', name, body=replacement)
        authenticate_application()
        missing = name + '-missing'
        pomi('openid', 'applications', 'update', missing, body={**replacement, 'clientId': missing}, status=404)
        request('api/features/OrchardCore.RemoteManagement.Cli:disable', 'POST')
        try:
            assert tool('applications', 'show', {'query': {'clientId': name}}) == changed
            tool('applications', 'delete', {'query': {'clientId': name}}, status=204)
            tool('applications', 'delete', {'query': {'clientId': name}}, status=204)
        finally:
            request('api/features/OrchardCore.RemoteManagement.Cli:enable?force=true', 'POST')
        authenticate_application(expected_error='invalid_client')
        pomi('openid', 'applications', 'show', name, status=404)
        pomi('openid', 'applications', 'delete', name, '--force')
        print('PASS: application CRUD, redaction/retries/conflicts, shared validation, credential preservation, runtime role/grant replacement, permissions and MCP without CLI.')
    finally:
        request(path, 'DELETE', status=204)
