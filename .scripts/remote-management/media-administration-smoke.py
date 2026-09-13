#!/usr/bin/env python3
"""Verify media administration through Pomi/MCP and tenant isolation."""
import json
import zlib
import html
import re
import struct
import os
from pathlib import Path
import secrets
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
assert urllib.parse.urlparse(base).hostname in ('127.0.0.1', 'localhost', '::1')
wrapper = Path(__file__).with_name('pomi-fixture.py')
name = 'MediaSmoke' + secrets.token_hex(4)
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


with tempfile.TemporaryDirectory(prefix='media-admin-cli-', dir=state_path.parent) as config_home:
    pomi('context', 'add', name, base, '--current')
    for feature in ['OrchardCore.Media', 'OrchardCore.Media.Cache', 'OrchardCore.RemoteManagement.Mcp', 'OrchardCore.Liquid', 'OrchardCore.Autoroute']:
        request('api/features/' + feature + ':enable?force=true', 'POST')
    pomi('api', 'refresh', '--force')
    for path, client in [('api/media/profiles', 'cli-media-profiles'), ('api/media/cache', 'cli-media-cache')]:
        request(path, client=None, status=401)
        request(path, client='cli-discovery', status=403)
        request(path, client=client)
    request('api/media/profiles', client='cli-media-cache', status=403)
    request('api/media/cache', client='cli-media-profiles', status=403)
    profile = {'name': name.lower(), 'width': 100, 'height': 50, 'mode': 'Crop', 'format': 'Png',
        'quality': 90, 'backgroundColor': '#fff', 'autoOrient': True, 'hint': 'live smoke'}
    upload_before = pomi('settings', 'sections', 'show', 'media-upload-policy')['values']
    api_before = pomi('settings', 'sections', 'show', 'media-api')['values']
    secondary_name = None
    try:
        created = pomi('media', 'profiles', 'create', body=profile)
        assert created == profile
        assert pomi('media', 'profiles', 'create', body=profile) == created
        assert pomi('media', 'profiles', 'show', profile['name']) == created
        for change in [{'width': -1}, {'quality': 101}, {'backgroundColor': '#zzzzzz'}]:
            pomi('media', 'profiles', 'update', profile['name'], body={**profile, **change}, status=400)
            assert pomi('media', 'profiles', 'show', profile['name']) == created
        changed = {**profile, 'width': 80}
        assert tool('media_profiles_update', {'query': {'name': profile['name']}, 'body': changed}) == changed
        assert pomi('media', 'profiles', 'show', profile['name']) == changed
        # Render an actual profile through the existing Liquid/media URL pipeline.
        pomi('themes', 'set-current', 'TheTheme')
        png = Path(config_home) / 'input.png'
        def chunk(kind, data):
            return struct.pack('>I', len(data)) + kind + data + struct.pack('>I', zlib.crc32(kind + data))
        png.write_bytes(b'\x89PNG\r\n\x1a\n' + chunk(b'IHDR', struct.pack('>IIBBBBB', 200, 100, 8, 2, 0, 0, 0))
            + chunk(b'IDAT', zlib.compress((b'\0' + b'\xff\0\0' * 200) * 100)) + chunk(b'IEND', b''))
        filename = profile['name'] + '.png'
        pomi('media', 'files', 'upload', filename, '--file', str(png))
        pomi('content', 'types', 'create', body={'name': name + 'Page', 'displayName': name + 'Page',
            'settings': {'ContentTypeSettings': {'draftable': True, 'versionable': True}},
            'parts': [{'name': part, 'partName': part} for part in ['TitlePart', 'LiquidPart', 'AutoroutePart']]})
        liquid = '<img id="media-smoke" src="{{ \'%s\' | asset_url | resize_url: profile: \'%s\' }}">' % (filename, profile['name'])
        page = pomi('content', 'items', 'save', body={'ContentType': name + 'Page', 'TitlePart': {'Title': name},
            'LiquidPart': {'Liquid': liquid}, 'AutoroutePart': {'Path': profile['name']}})
        pomi('content', 'items', 'publish', page['ContentItemId'])
        def rendered_image():
            with urllib.request.urlopen(base + profile['name'], timeout=30) as response:
                page_html = response.read().decode()
            match = re.search(r'<img id="media-smoke" src="([^"]+)"', page_html)
            assert match, 'Rendered media profile image missing'
            url = urllib.parse.urljoin(base, html.unescape(match.group(1)))
            with urllib.request.urlopen(url, timeout=30) as response:
                data = response.read()
            assert data[:8] == b'\x89PNG\r\n\x1a\n'
            assert struct.unpack('>II', data[16:24]) == (80, 50)
            return data
        image_before = rendered_image()
        cache = pomi('media', 'cache', 'show')
        assert cache['resizedConfigured'] and not cache['remoteConfigured']
        pomi('media', 'cache', 'purge', 'remote', '--force', status=503)
        pomi('media', 'cache', 'purge', 'resized', '--force')
        assert rendered_image() == image_before
        patch = {'maxFileSize': 1000, 'allowedFileExtensions': ['.png']}
        update = pomi('settings', 'sections', 'update', 'media-upload-policy', body=patch)
        assert update['changed'] and update['reloadRequested']
        assert not pomi('settings', 'sections', 'update', 'media-upload-policy', body=patch)['changed']
        readback = pomi('settings', 'sections', 'show', 'media-upload-policy')['values']
        assert readback['effectiveMaxFileSize'] == 1000 and readback['effectiveAllowedFileExtensions'] == ['.png']
        pomi('settings', 'sections', 'update', 'media-upload-policy', body={'maxFileSize': readback['hostMaxFileSize'] + 1}, status=400)
        # The same options consumed by existing upload endpoints enforce tenant restrictions.
        too_large = Path(config_home) / 'large.png'
        too_large.write_bytes(png.read_bytes() * 20)
        pomi('media', 'files', 'upload', 'blocked.png', '--file', str(too_large), failure=True)
        pomi('media', 'files', 'upload', 'blocked.jpg', '--file', str(png), failure=True)
        pomi('media', 'files', 'upload', profile['name'] + '-small.png', '--file', str(png))
        pomi('settings', 'sections', 'update', 'media-api', body={'authenticationScheme': 'Bearer'})
        assert pomi('settings', 'sections', 'show', 'media-api')['values']['authenticationScheme'] == 'Bearer'
        pomi('settings', 'sections', 'update', 'media-api', body={'authenticationScheme': 'invalid'}, status=400)
        # A second tenant on this same host must have independent audit and task state.
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
        for feature in ['OrchardCore.Media', 'OrchardCore.Media.Cache']:
            secondary('api/features/' + feature + ':enable?force=true', 'POST')
        secondary('api/media/profiles/by-name?name=' + profile['name'], status=404)
        independent = secondary('api/settings/sections/media-upload-policy')['values']
        assert independent['maxFileSize'] is None and independent['allowedFileExtensions'] is None
        secondary('api/media/profiles', 'POST', {**profile, 'width': 20})
        assert pomi('media', 'profiles', 'show', profile['name'])['width'] == 80
        secondary('api/media/cache/purge?cache=resized', 'POST', status=204)
        assert rendered_image() == image_before
        print('PASS: rendered images/cache regeneration, real upload policy enforcement, two-tenant isolation; media profile CRUD/retries/validation, MCP update, cache availability/purge, permissions, settings limits/retries/authentication.', flush=True)
    finally:
        pomi('settings', 'sections', 'update', 'media-upload-policy', body={key: upload_before[key] for key in ['maxFileSize', 'allowedFileExtensions']})
        pomi('settings', 'sections', 'update', 'media-api', body=api_before)
        pomi('media', 'profiles', 'delete', profile['name'], '--force')
        if secondary_name:
            request('api/tenants/' + secondary_name + ':stop', 'POST', status=(200, 204))
