#!/usr/bin/env python3
"""Verify embedded custom user settings through HTTP, Pomi and MCP."""
import json
import xml.etree.ElementTree as ET
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
ssl_context = ssl.create_default_context(cafile=state['certificatePath'])
urllib.request.install_opener(urllib.request.build_opener(urllib.request.HTTPSHandler(context=ssl_context)))
assert urllib.parse.urlparse(base).hostname in ('127.0.0.1', 'localhost', '::1')
wrapper = Path(__file__).with_name('pomi-fixture.py')
name = 'CustomUserSmoke' + secrets.token_hex(4)
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
        assert (response.status in status if isinstance(status, tuple) else response.status == status), (method, path, response.status, status)
        if raw:
            return text, response.headers
        if response.headers.get('Content-Type', '').startswith('text/event-stream'):
            return next(json.loads(line[6:]) for line in text.splitlines()
                if line.startswith('data: ') and json.loads(line[6:]).get('id') == 1)
        return json.loads(text) if text and 'json' in response.headers.get('Content-Type', '') else None


def pomi(*args, body=None, client='cli-fixture', status=None, failure=False):
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
    if result.returncode != 0:
        try:
            error = json.loads(result.stderr or result.stdout).get('error', {})
            print('CLI failure:', args[:3], error.get('code'), error.get('status'), error.get('message'), flush=True)
        except (ValueError, AttributeError):
            message = result.stderr or result.stdout
            for value in [state.get('OC_CLIENT_SECRET', ''), *tokens.values()]:
                if value:
                    message = message.replace(value, '[redacted]')
            print('CLI diagnostic:', message[:1600], flush=True)
    assert result.returncode == 0, (args[:3], 'CLI request failed')
    return json.loads(result.stdout) if result.stdout.strip() else None


def tool(name, arguments):
    result = request('mcp', 'POST', {'jsonrpc': '2.0', 'id': 1, 'method': 'tools/call',
        'params': {'name': name, 'arguments': arguments}})['result']
    content = json.loads(result['content'][0]['text'])
    assert content['statusCode'] == 200, (name, content['statusCode'])
    return json.loads(content['body']) if content['body'] else None


with tempfile.TemporaryDirectory(prefix='custom-user-cli-', dir=state_path.parent) as config_home:
    pomi('context', 'add', name, base, '--current')
    request('api/features/OrchardCore.Users.CustomUserSettings:enable?force=true', 'POST')
    request('api/features/OrchardCore.RemoteManagement.Mcp:enable?force=true', 'POST')
    pomi('api', 'refresh', '--force')
    definition = {'name': name, 'displayName': name,
        'settings': {'ContentTypeSettings': {'stereotype': 'CustomUserSettings'}},
        'parts': [{'name': 'TitlePart', 'partName': 'TitlePart'}]}
    pomi('content', 'types', 'create', body=definition)
    user = pomi('users', 'create', body={'userName': name, 'email': name + '@example.test',
        'password': secrets.token_urlsafe(32) + 'aA1!'})
    user_id = user['userId']
    path = 'api/users/' + user_id + '/settings/' + name
    try:
        assert name in [entry['name'] for entry in pomi('users', 'settings', 'types')['items']]
        schema = pomi('users', 'settings', 'schema', name)
        assert 'ContentItemId' not in schema['properties']
        request(path, client=None, status=401)
        request(path, client='cli-discovery', status=404)
        payload = {'TitlePart': {'Title': 'First'}}
        result = pomi('users', 'settings', 'update', user_id, name, body=payload)
        assert result['TitlePart']['Title'] == 'First'
        assert 'ContentItemId' not in result and 'PasswordHash' not in result
        assert pomi('users', 'settings', 'update', user_id, name, body=payload) == result
        assert tool('users_settings_show', {'path': {'userId': user_id, 'name': name}}) == result
        for invalid in [{'PasswordHash': 'forbidden'}, {'ContentType': 'User'}, {'TitlePart': []}]:
            request(path, 'PUT', invalid, status=400)
        assert pomi('users', 'settings', 'show', user_id, name) == result
        secondary_name = name + 'Tenant'
        installed = request('api/tenants/' + secondary_name + ':install', 'POST', {
            'siteName': secondary_name, 'userName': 'admin', 'email': 'admin@example.test',
            'password': secrets.token_urlsafe(32) + 'aA1!', 'recipeName': 'Blank',
            'requestUrlPrefix': secondary_name.lower(), 'enableRemoteManagement': True}, status=201)
        secondary_base = installed['primaryUrl'].rstrip('/') + '/'
        credentials = installed['clientCredentials']
        data = urllib.parse.urlencode({'grant_type': 'client_credentials', 'client_id': credentials['clientId'],
            'client_secret': credentials['clientSecret'], 'scope': 'orchardcore.management'}).encode()
        with urllib.request.urlopen(urllib.request.Request(secondary_base + 'connect/token', data=data,
                headers={'Content-Type': 'application/x-www-form-urlencoded'}), timeout=60) as response:
            secondary_token = json.load(response)['access_token']
        def secondary(path, method='GET', body=None, status=200):
            url = urllib.parse.urljoin(secondary_base, path)
            assert urllib.parse.urlparse(url).netloc == urllib.parse.urlparse(base).netloc
            req = urllib.request.Request(url, method=method,
                headers={'Authorization': 'Bearer ' + secondary_token, 'Content-Type': 'application/json'},
                data=json.dumps(body).encode() if body is not None else None)
            try:
                response = urllib.request.urlopen(req, timeout=60)
            except urllib.error.HTTPError as error:
                response = error
            with response:
                assert response.status == status, ('secondary', method, path, response.status)
                text = response.read().decode()
                return json.loads(text) if text and 'json' in response.headers.get('Content-Type', '') else None
        try:
            secondary('api/users/settings/types', status=404)
            secondary('api/features/OrchardCore.Users.CustomUserSettings:enable?force=true', 'POST')
            assert secondary('api/users/settings/types')['items'] == []
            secondary(path, status=404)
            assert pomi('users', 'settings', 'show', user_id, name) == result
        finally:
            request('api/tenants/' + secondary_name + ':stop', 'POST', status=(200, 204))
        print('PASS: custom user settings HTTP/Pomi/MCP, permissions, safe envelopes, invalid updates, retries and tenant isolation', flush=True)
    finally:
        pomi('users', 'delete', user_id, '--force')
        pomi('content', 'types', 'delete', name, '--force')
