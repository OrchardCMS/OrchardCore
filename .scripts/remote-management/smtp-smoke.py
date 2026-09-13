#!/usr/bin/env python3
"""Verify tenant SMTP configuration, safe readback and local pickup delivery through Pomi/MCP."""
import base64
import socketserver
import threading
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
name = 'SmtpSmoke' + secrets.token_hex(4)
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
    assert content['statusCode'] in (200, 204), (name, content['statusCode'])
    return json.loads(content['body']) if content['body'] else None


class LocalSmtp(socketserver.StreamRequestHandler):
    def handle(self):
        self.connection.settimeout(30)
        self.wfile.write(b'220 localhost test SMTP\r\n')
        while line := self.rfile.readline():
            command = line.decode().strip()
            verb = command.split(' ', 1)[0].upper()
            if verb in ('EHLO', 'HELO'):
                self.wfile.write(b'250-localhost\r\n250 AUTH PLAIN\r\n')
            elif verb == 'AUTH':
                fields = command.split(' ')
                encoded = fields[2] if len(fields) > 2 else None
                if encoded is None:
                    self.wfile.write(b'334 \r\n')
                    encoded = self.rfile.readline().strip()
                self.server.credentials = base64.b64decode(encoded).split(b'\0')[-2:]
                self.wfile.write(b'235 authenticated\r\n')
            elif verb in ('MAIL', 'RCPT', 'RSET', 'NOOP'):
                self.wfile.write(b'250 OK\r\n')
            elif verb == 'DATA':
                self.wfile.write(b'354 message\r\n')
                body = []
                while (line := self.rfile.readline()) != b'.\r\n':
                    if not line:
                        return
                    body.append(line)
                self.server.messages.append(b''.join(body))
                self.wfile.write(b'250 accepted\r\n')
            elif verb == 'QUIT':
                self.wfile.write(b'221 goodbye\r\n')
                return
            else:
                self.wfile.write(b'500 unsupported\r\n')


with tempfile.TemporaryDirectory(prefix='smtp-cli-', dir=state_path.parent) as config_home:
    pomi('context', 'add', name, base, '--current')
    for feature in ['OrchardCore.Email.Smtp', 'OrchardCore.RemoteManagement.Mcp']:
        request('api/features/' + feature + ':enable?force=true', 'POST')
    pomi('api', 'refresh', '--force')
    for path in ['api/settings/sections/smtp', 'api/settings/sections/email']:
        request(path, client=None, status=401)
        request(path, client='cli-discovery', status=403)
        request(path, client='cli-email')
    request('api/email/test', 'POST', {}, client='cli-discovery', status=403)
    schema = pomi('settings', 'sections', 'schema', 'smtp')
    assert schema['properties']['password']['writeOnly']
    config = {'isEnabled': True, 'defaultSender': 'admin@example.test',
        'deliveryMethod': 'SpecifiedPickupDirectory', 'pickupDirectoryLocation': name,
        'password': secrets.token_urlsafe(24)}
    updated = pomi('settings', 'sections', 'update', 'smtp', body=config, client='cli-email')
    assert updated['changed']
    assert 'password' not in updated['section']['values']
    assert updated['section']['values']['hasPassword']
    assert not pomi('settings', 'sections', 'update', 'smtp', body=config)['changed']
    before = pomi('settings', 'sections', 'show', 'smtp')
    pomi('settings', 'sections', 'update', 'smtp', body={'host': 'bad-change', 'port': 70000}, failure=True)
    assert before == pomi('settings', 'sections', 'show', 'smtp')
    assert tool('settings_sections_show', {'path': {'name': 'smtp'}})['values'] == before['values']
    email = pomi('settings', 'sections', 'show', 'email')
    assert 'SMTP' in email['values']['availableProviders']
    pomi('settings', 'sections', 'update', 'email', body={'defaultProvider': 'SMTP'})
    pomi('email', 'test', body={'to': 'recipient@example.test', 'subject': name, 'body': 'Local pickup proof'}, client='cli-email')
    matches = []
    for candidate in (Path(state['root']) / 'App_Data').rglob('*.eml'):
        if name in candidate.read_text():
            matches.append(candidate)
    assert len(matches) == 1, 'Expected one local pickup message'
    assert 'Local pickup proof' in matches[0].read_text()
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
        secondary('api/settings/sections/smtp', status=404)
        secondary('api/features/OrchardCore.Email.Smtp:enable?force=true', 'POST')
        independent = secondary('api/settings/sections/smtp')['values']
        assert not independent['hasPassword']
        assert not independent['isEnabled']
        secondary('api/settings/sections/smtp', 'PUT', {'isEnabled': True, 'defaultSender': 'secondary@example.test',
            'deliveryMethod': 'SpecifiedPickupDirectory', 'pickupDirectoryLocation': name})
        secondary('api/email/test', 'POST', {'provider': 'SMTP', 'to': 'recipient@example.test', 'subject': name + ' child', 'body': 'Child pickup proof'}, status=204)
        child_messages = list((Path(state['root']) / 'App_Data' / 'Sites' / secondary_name).rglob('*.eml'))
        assert len(child_messages) == 1
        assert 'Child pickup proof' in child_messages[0].read_text()
        assert pomi('settings', 'sections', 'show', 'smtp')['values']['defaultSender'] == 'admin@example.test'
    finally:
        request('api/tenants/' + secondary_name + ':stop', 'POST', status=(200, 204))
    with socketserver.TCPServer(('127.0.0.1', 0), LocalSmtp) as server:
        server.messages = []
        server.credentials = None
        thread = threading.Thread(target=server.serve_forever, daemon=True)
        thread.start()
        try:
            secret = secrets.token_urlsafe(24)
            pomi('settings', 'sections', 'update', 'smtp', body={'isEnabled': True,
                'deliveryMethod': 'Network', 'host': '127.0.0.1', 'port': server.server_address[1],
                'autoSelectEncryption': False, 'encryptionMethod': 'None',
                'requireCredentials': True, 'useDefaultCredentials': False, 'userName': 'loopback-test', 'password': secret})
            pomi('email', 'test', body={'provider': 'SMTP', 'to': 'recipient@example.test',
                'subject': name, 'body': 'Loopback SMTP proof'})
            tool('email_test', {'body': {'provider': 'SMTP', 'to': 'recipient@example.test',
                'subject': name, 'body': 'MCP loopback SMTP proof'}})
            assert server.credentials == [b'loopback-test', secret.encode()]
            assert len(server.messages) == 2 and b'Loopback SMTP proof' in server.messages[0]
            assert b'MCP loopback SMTP proof' in server.messages[1]
        finally:
            server.shutdown()
            thread.join(timeout=10)
    pomi('settings', 'sections', 'update', 'smtp', body={'clearPassword': True})
    assert not pomi('settings', 'sections', 'show', 'smtp')['values']['hasPassword']
    pomi('settings', 'sections', 'update', 'smtp', body={'isEnabled': False})
    pomi('email', 'test', body={'provider': 'SMTP', 'to': 'recipient@example.test', 'subject': name, 'body': 'Disabled'}, failure=True)
    assert len(matches) == 1
    print('PASS: SMTP Pomi/MCP settings, write-only credentials, permission gates, retry preservation tenant-isolated pickup delivery and authenticated loopback SMTP', flush=True)
