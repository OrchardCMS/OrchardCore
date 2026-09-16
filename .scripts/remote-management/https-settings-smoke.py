#!/usr/bin/env python3
"""Verify typed HTTPS settings using a trusted, disposable dual HTTP/HTTPS fixture."""
import json
import os
from pathlib import Path
import secrets
import ssl
import subprocess
import sys
import tempfile
import urllib.error
import urllib.parse
import urllib.request

state_path = Path(sys.argv[1])
state = json.loads(state_path.read_text())
base, http_base = state['url'], state['httpUrl']
assert urllib.parse.urlparse(base).scheme == 'https'
assert all(urllib.parse.urlparse(url).hostname in ('localhost', '127.0.0.1', '::1') for url in (base, http_base))
ssl_context = ssl.create_default_context(cafile=state['certificatePath'])


class NoRedirect(urllib.request.HTTPRedirectHandler):
    def redirect_request(self, req, fp, code, msg, headers, newurl):
        return None


opener = urllib.request.build_opener(urllib.request.HTTPSHandler(context=ssl_context), NoRedirect())
wrapper = Path(__file__).with_name('pomi-fixture.py')
tokens = {}


def token(client, origin):
    if (client, origin) not in tokens:
        body = urllib.parse.urlencode(dict(grant_type='client_credentials', client_id=client,
            client_secret=state['OC_CLIENT_SECRET'], scope='orchardcore.management')).encode()
        with opener.open(urllib.request.Request(origin + 'connect/token', data=body), timeout=30) as response:
            tokens[client, origin] = json.load(response)['access_token']
    return tokens[client, origin]


def request(path, method='GET', body=None, client='cli-fixture', status=200, origin=base, raw=False, headers=None):
    url = urllib.parse.urljoin(origin, path)
    assert urllib.parse.urlparse(url).netloc == urllib.parse.urlparse(origin).netloc
    fields = {'Content-Type': 'application/json', 'Accept': 'application/json, text/event-stream', **(headers or {})}
    if client:
        fields['Authorization'] = 'Bearer ' + token(client, origin)
    req = urllib.request.Request(url, method=method, headers=fields,
        data=json.dumps(body).encode() if body is not None else None)
    try:
        response = opener.open(req, timeout=60)
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


def pomi(*args, body=None, status=None):
    env = {**os.environ, 'OC_FIXTURE_CONFIG_HOME': config_home, 'OC_FIXTURE_CLIENT_ID': 'cli-https'}
    command = [sys.executable, str(wrapper), str(state_path), *args, '--output', 'json']
    if body is not None:
        command.append('--stdin')
    result = subprocess.run(command, input=json.dumps(body) if body is not None else None,
        capture_output=True, text=True, env=env, timeout=90)
    if status:
        assert result.returncode != 0, args
        assert json.loads(result.stderr or result.stdout)['error']['status'] == status, result.stderr[:600]
        return
    assert result.returncode == 0, (args[:4], result.stderr[:800])
    return json.loads(result.stdout) if result.stdout.strip() else None


def feature(name, enabled):
    request('api/features/' + name + (':enable?force=true' if enabled else ':disable'), 'POST')


def tool(verb, arguments=None, client='cli-https', status=200):
    result = request('mcp', 'POST', {'jsonrpc': '2.0', 'id': 1, 'method': 'tools/call',
        'params': {'name': 'settings_sections_' + verb, 'arguments': arguments or {}}}, client=client)['result']
    content = json.loads(result['content'][0]['text'])
    assert content['statusCode'] == status, content
    assert result.get('isError', False) == (status >= 400)
    return json.loads(content['body']) if content['body'] else None


def hsts_header():
    _, headers = request('api/settings/sections', client=None, status=401, raw=True, headers={'Host': 'hsts.example.test'})
    return headers.get('Strict-Transport-Security')


with tempfile.TemporaryDirectory(prefix='https-cli-', dir=state_path.parent) as config_home:
    pomi('context', 'add', 'https-settings-' + secrets.token_hex(4), base, '--current')
    feature('OrchardCore.Https', True)
    feature('OrchardCore.RemoteManagement.Mcp', True)
    pomi('api', 'refresh', '--force')
    try:
        descriptor = next(item for item in pomi('settings', 'sections', 'list') if item['name'] == 'https')
        assert descriptor['requiresHttps'] and descriptor['requiresReload']
        schema = pomi('settings', 'sections', 'schema', 'https')
        assert not schema['additionalProperties'] and schema['properties']['sslPort']['maximum'] == 65535
        saved = pomi('settings', 'sections', 'show', 'https')
        assert saved['source'] == 'tenant' and not saved['isReadOnly']
        assert not saved['redactedProperties'] and not saved['readOnlyProperties']
        assert set(saved['values']) == {'strictTransportSecurityMode', 'requireHttps', 'requireHttpsPermanent', 'sslPort'}
        assert not pomi('settings', 'sections', 'update', 'https', body={})['changed']
        for path, method, body in [('api/settings/sections', 'GET', None), ('api/settings/sections/https', 'GET', None),
                ('api/settings/sections/https/schema', 'GET', None), ('api/settings/sections/https', 'PUT', {})]:
            request(path, method, body, client=None, status=401)
            if path != 'api/settings/sections':
                request(path, method, body, client='cli-discovery', status=403)
        assert request('api/settings/sections', client='cli-discovery') == []
        request('api/settings/sections/https', 'PUT', {}, client='cli-https', origin=http_base, status=403)
        tool('update', {'path': {'name': 'https'}, 'body': {}}, client='cli-discovery', status=403)
        for invalid in [{'sslPort': 0}, {'sslPort': 65536}, {'sslPort': 1.5}, {'requireHttps': None},
                {'strictTransportSecurityMode': 1}, {'strictTransportSecurityMode': 'invalid'}, {'siteSalt': 'rejected'}]:
            pomi('settings', 'sections', 'update', 'https', body=invalid, status=400)
        assert saved == pomi('settings', 'sections', 'show', 'https')
        print('Section discovery/schema, safe readback, no-op retries, invalid fields and HTTP/OAuth permissions: passed', flush=True)

        https_port = urllib.parse.urlparse(base).port
        changed = pomi('settings', 'sections', 'update', 'https', body={'sslPort': https_port})
        assert changed['changed'] and changed['reloadRequested']
        assert not pomi('settings', 'sections', 'update', 'https', body={'sslPort': https_port})['changed']
        changed = tool('update', {'path': {'name': 'https'}, 'body': {'requireHttps': True, 'strictTransportSecurityMode': 'Enabled'}})
        assert changed['section']['values']['sslPort'] == https_port
        _, headers = request('api/settings/sections', origin=http_base, client=None, status=307, raw=True)
        assert headers['Location'] == base + 'api/settings/sections'
        assert 'max-age=31536000' in hsts_header()
        tool('update', {'path': {'name': 'https'}, 'body': {'requireHttpsPermanent': True}})
        request('api/settings/sections', origin=http_base, client=None, status=308, raw=True)
        pomi('settings', 'sections', 'update', 'https', body={'requireHttps': False, 'strictTransportSecurityMode': 'FromConfiguration'})
        assert not hsts_header()  # Development environment.
        reset = pomi('settings', 'sections', 'update', 'https', body={'sslPort': None, 'requireHttpsPermanent': False, 'strictTransportSecurityMode': 'Disabled'})
        assert reset['section']['values']['sslPort'] is None
        assert not reset['section']['values']['requireHttps']
        print('HTTPS redirect 307/308, real HSTS headers, environment mode, omitted fields, null reset and reloads: passed', flush=True)

        feature('OrchardCore.RemoteManagement.Cli', False)
        assert tool('show', {'path': {'name': 'https'}})['values'] == reset['section']['values']
        feature('OrchardCore.RemoteManagement.Cli', True)
        feature('OrchardCore.Https', False)
        assert not any(item['name'] == 'https' for item in request('api/settings/sections'))
        request('api/settings/sections/https', status=404)
        feature('OrchardCore.Https', True)
        assert tool('show', {'path': {'name': 'https'}})['values'] == reset['section']['values']
        manifest = request('api/management/manifest')
        document = request(manifest['openApiUrl'])
        operations = [operation for item in document['paths'].values() for operation in item.values()
            if isinstance(operation, dict) and operation.get('operationId')]
        ids = [item['operationId'] for item in operations]
        assert len(ids) == len(set(ids))
        assert len([item for item in operations if item.get('x-oc-cli', {}).get('commandGroup') == ['settings', 'sections']]) == 4
        print('Unique catalog, feature removal/restoration, persisted settings and MCP with CLI disabled: passed', flush=True)
    finally:
        feature('OrchardCore.RemoteManagement.Cli', True)
        feature('OrchardCore.Https', True)
        pomi('settings', 'sections', 'update', 'https', body={'requireHttps': False, 'requireHttpsPermanent': False,
            'strictTransportSecurityMode': 'Disabled', 'sslPort': None})
print('HTTPS settings smoke passed on a disposable tenant with certificate verification enabled.')
