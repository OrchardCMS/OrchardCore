#!/usr/bin/env python3
"""Verify typed user policies and actual password-reset availability through Pomi/MCP."""
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
name = 'UserPolicySmoke' + secrets.token_hex(4)
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


with tempfile.TemporaryDirectory(prefix='user-policy-cli-', dir=state_path.parent) as config_home:
    pomi('context', 'add', name, base, '--current')
    request('api/settings/sections/user-two-factor', status=(200, 404))
    for feature in ['OrchardCore.Users.Registration', 'OrchardCore.Users.ResetPassword',
            'OrchardCore.Users.ChangeEmail', 'OrchardCore.GitHub.Authentication', 'OrchardCore.Roles',
            'OrchardCore.Users.2FA.AuthenticatorApp', 'OrchardCore.Users.2FA.Email',
            'OrchardCore.Users.2FA.Sms', 'OrchardCore.RemoteManagement.Mcp']:
        request('api/features/' + feature + ':enable?force=true', 'POST')
    pomi('api', 'refresh', '--force')
    desired = {
        'user-login': {'allowRememberMe': False, 'usePersistentAuthenticationCookie': True},
        'user-registration': {'usersMustValidateEmail': True, 'usersAreModerated': True},
        'user-password-reset': {'allowResetPassword': True},
        'user-change-email': {'allowChangeEmail': True},
        'user-two-factor': {'allowRememberClientTwoFactorAuthentication': True, 'numberOfRecoveryCodesToGenerate': 7},
        'user-authenticator-app': {'useEmailAsAuthenticatorDisplayName': True, 'tokenLength': 6},
        'user-external-login': {'useExternalProviderIfOnlyOneDefined': False, 'useScriptToSyncProperties': False},
        'user-external-registration': {'disableNewRegistrations': True},
        'user-email-authenticator': {'subject': 'Verification', 'body': 'Your code is {{ Code }}'},
        'user-sms-authenticator': {'body': '{{ Code }}'},
        'user-role-two-factor': {'requireTwoFactorAuthenticationForSpecificRoles': True, 'roles': ['Administrator']},
    }
    before = {}
    try:
        for section, values in desired.items():
            path = 'api/settings/sections/' + section
            request(path, client=None, status=401)
            request(path, client='cli-discovery', status=403)
            before[section] = pomi('settings', 'sections', 'show', section)['values']
            schema = pomi('settings', 'sections', 'schema', section)
            assert not schema['additionalProperties']
            pomi('settings', 'sections', 'update', section, body=values, client='cli-user-policy')
            readback = pomi('settings', 'sections', 'show', section)['values']
            assert all(readback[key] == value for key, value in values.items()), section
            assert not pomi('settings', 'sections', 'update', section, body=values)['changed'], section
            assert tool('settings_sections_show', {'path': {'name': section}})['values'] == readback
            pomi('settings', 'sections', 'update', section, body={'password': 'not-a-policy-field'}, failure=True)
            assert pomi('settings', 'sections', 'show', section)['values'] == readback
        request('ForgotPassword', client=None, raw=True)
        pomi('settings', 'sections', 'update', 'user-password-reset', body={'allowResetPassword': False})
        request('ForgotPassword', client=None, status=404)
        pomi('settings', 'sections', 'update', 'user-password-reset', body={'allowResetPassword': True})
        request('ForgotPassword', client=None, raw=True)
        pomi('settings', 'sections', 'update', 'user-two-factor', body={'numberOfRecoveryCodesToGenerate': 0}, failure=True)
        assert pomi('settings', 'sections', 'show', 'user-two-factor')['values']['numberOfRecoveryCodesToGenerate'] == 7
        pomi('settings', 'sections', 'update', 'user-role-two-factor', body={'roles': ['MissingRole']}, failure=True)
        pomi('settings', 'sections', 'update', 'user-email-authenticator', body={'body': '{% if %}'}, failure=True)
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
                return json.loads(text) if text else None
        try:
            secondary('api/settings/sections/user-two-factor', status=404)
            secondary('api/features/OrchardCore.Users.2FA.AuthenticatorApp:enable?force=true', 'POST')
            secondary('api/settings/sections/user-two-factor', 'PUT', {'numberOfRecoveryCodesToGenerate': 3})
            assert secondary('api/settings/sections/user-two-factor')['values']['numberOfRecoveryCodesToGenerate'] == 3
            assert pomi('settings', 'sections', 'show', 'user-two-factor')['values']['numberOfRecoveryCodesToGenerate'] == 7
            secondary('api/settings/sections/user-login', 'PUT', {'allowRememberMe': True})
            assert not pomi('settings', 'sections', 'show', 'user-login')['values']['allowRememberMe']
        finally:
            request('api/tenants/' + secondary_name + ':stop', 'POST', status=(200, 204))
        print('PASS: eleven user policy contracts, HTTP/Pomi/MCP, permissions, invalid-update preservation tenant isolation and real password-reset route changes', flush=True)
    finally:
        for section, values in before.items():
            pomi('settings', 'sections', 'update', section, body=values)
