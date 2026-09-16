#!/usr/bin/env python3
"""Verify one-time secret delivery, immediate rotation and shared-secret revocation through HTTP/Pomi/MCP."""
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
name = 'CredentialLifecycleSmoke' + secrets.token_hex(4)
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


def set_application_secret(value):
    global application_secret
    application_secret = value
    tokens.pop(name, None)
    path = Path(config_home) / 'application-client.json'
    path.write_text(json.dumps({**state, 'OC_CLIENT_ID': name, 'OC_CLIENT_SECRET': value}))
    path.chmod(0o600)


def authenticate(value, expected_error=None):
    body = urllib.parse.urlencode(dict(grant_type='client_credentials', client_id=name,
        client_secret=value, scope='orchardcore.management')).encode()
    try:
        with urllib.request.urlopen(urllib.request.Request(base + 'connect/token', data=body,
                headers={'Content-Type': 'application/x-www-form-urlencoded'}), timeout=30) as response:
            payload = json.load(response)
        assert expected_error is None, 'Retired credential unexpectedly authenticated'
        return payload['access_token']
    except urllib.error.HTTPError as error:
        with error:
            payload = json.load(error)
        assert expected_error and payload.get('error') == expected_error, 'Unexpected authentication rejection'


with tempfile.TemporaryDirectory(prefix='openid-credentials-cli-', dir=state_path.parent) as config_home:
    set_application_secret(application_secret)
    pomi('context', 'add', name, base, '--current')
    request('api/features/OrchardCore.RemoteManagement.Mcp:enable?force=true', 'POST')
    pomi('api', 'refresh', '--force')
    path = 'api/openid/applications/by-client-id?' + urllib.parse.urlencode({'clientId': name})
    rotate_path = 'api/openid/applications/credentials:rotate?' + urllib.parse.urlencode({'clientId': name})
    revoke_path = rotate_path.replace(':rotate?', ':revoke?')
    definition = {'clientId': name, 'displayName': 'Credential smoke', 'clientType': 'confidential',
        'clientSecret': application_secret, 'consentType': 'implicit', 'allowClientCredentialsFlow': True,
        'roles': ['Cli-openid-scopes'], 'scopes': ['orchardcore.management']}
    original = pomi('openid', 'applications', 'create', body=definition)
    try:
        original_secret = application_secret
        old_token = authenticate(original_secret)
        assert pomi('openid', 'scopes', 'list', client=name)['totalCount'] > 0
        for denied in ['cli-denied', 'cli-discovery', 'cli-openid-scopes', name]:
            request(rotate_path, 'POST', client=denied, status=403)
            request(revoke_path, 'POST', client=denied, status=403)
            if denied != 'cli-denied':
                tool('applications_credentials', 'rotate', {'query': {'clientId': name}}, client=denied, status=403)
        request(rotate_path, 'POST', client=None, status=401)
        authenticate(original_secret)
        # Neither a missing destination nor an occupied destination may rotate the secret.
        pomi('openid', 'applications', 'credentials', 'rotate', name, '--force', failure=True)
        occupied = Path(config_home) / 'occupied.json'
        occupied.write_text('preserve existing output')
        occupied.chmod(0o600)
        pomi('openid', 'applications', 'credentials', 'rotate', name, '--force', '--secret-output-file', str(occupied), failure=True)
        assert occupied.read_text() == 'preserve existing output'
        authenticate(original_secret)
        destination = Path(config_home) / 'replacement.json'
        output = pomi('openid', 'applications', 'credentials', 'rotate', name, '--force', '--secret-output-file', str(destination))
        assert set(output) == {'secretOutputFile'}
        assert Path(output['secretOutputFile']).samefile(destination)
        if os.name != 'nt':
            assert destination.stat().st_mode & 0o777 == 0o600
        credentials = json.loads(destination.read_text())
        assert set(credentials) == {'clientId', 'clientSecret'} and credentials['clientId'] == name
        assert credentials['clientSecret'] != original_secret
        authenticate(original_secret, expected_error='invalid_client')
        set_application_secret(credentials['clientSecret'])
        authenticate(application_secret)
        assert pomi('openid', 'scopes', 'list', client=name)['totalCount'] > 0
        assert pomi('openid', 'applications', 'show', name) == original
        # Raw HTTP also returns the new secret once and explicitly prohibits response caching.
        previous = application_secret
        text, headers = request(rotate_path, 'POST', client='cli-openid-applications', raw=True)
        assert headers.get('Cache-Control') == 'no-store'
        credentials = json.loads(text)
        authenticate(previous, expected_error='invalid_client')
        set_application_secret(credentials['clientSecret'])
        authenticate(application_secret)
        pomi('openid', 'applications', 'credentials', 'revoke', name, '--force')
        authenticate(application_secret, expected_error='invalid_client')
        pomi('openid', 'applications', 'credentials', 'revoke', name, '--force')
        assert pomi('openid', 'applications', 'show', name) == original
        # This fixture retains its configured token lifetime; revoking a secret does not revoke this issued token.
        with urllib.request.urlopen(urllib.request.Request(base + 'api/openid/scopes',
                headers={'Authorization': 'Bearer ' + old_token}), timeout=30) as response:
            assert response.status == 200
        request('api/features/OrchardCore.RemoteManagement.Cli:disable', 'POST')
        try:
            credentials = tool('applications_credentials', 'rotate', {'query': {'clientId': name}})
            set_application_secret(credentials['clientSecret'])
            authenticate(application_secret)
            tool('applications_credentials', 'revoke', {'query': {'clientId': name}}, status=204)
            authenticate(application_secret, expected_error='invalid_client')
        finally:
            request('api/features/OrchardCore.RemoteManagement.Cli:enable?force=true', 'POST')
        missing = name + '-missing'
        unused = Path(config_home) / 'missing.json'
        pomi('openid', 'applications', 'credentials', 'rotate', missing, '--force', '--secret-output-file', str(unused), status=404)
        assert not unused.exists()
        pomi('openid', 'applications', 'credentials', 'revoke', missing, '--force')
        print('PASS: protected one-time output, overwrite/missing-path refusal before rotation, immediate retirement, revocation, preserved grants, permissions and HTTP/Pomi/MCP without CLI.')
    finally:
        request(path, 'DELETE', status=204)
