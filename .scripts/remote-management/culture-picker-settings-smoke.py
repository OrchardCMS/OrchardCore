#!/usr/bin/env python3
"""Verify culture-picker settings, cookies, redirects, permissions and feature lifecycle."""
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
name = 'CulturePickerSmoke' + secrets.token_hex(4)
tokens = {}
items, types = [], []


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


def pomi(*args, body=None, client='cli-culture-picker', status=None, failure=False):
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


def tool(verb, arguments=None, client='cli-culture-picker', status=200):
    result = request('mcp', 'POST', {'jsonrpc': '2.0', 'id': 1, 'method': 'tools/call',
        'params': {'name': 'settings_sections_' + verb, 'arguments': arguments or {}}}, client=client)['result']
    content = json.loads(result['content'][0]['text'])
    assert content['statusCode'] == status, content
    assert result.get('isError', False) == (status >= 400)
    return json.loads(content['body']) if content['body'] else None


def catalog():
    manifest = request('api/management/manifest')
    document = request(manifest['openApiUrl'])
    operations = [operation for item in document['paths'].values() for operation in item.values()
        if isinstance(operation, dict) and operation.get('operationId')]
    ids = [operation['operationId'] for operation in operations]
    commands = [' '.join([*metadata['commandGroup'], metadata['verb']])
        for operation in operations if (metadata := operation.get('x-oc-cli'))]
    tools = request('mcp', 'POST', {'jsonrpc': '2.0', 'id': 1, 'method': 'tools/list', 'params': {}})['result']['tools']
    names = [entry['name'] for entry in tools]
    assert len(ids) == len(set(ids)) and len(commands) == len(set(commands)) and len(names) == len(set(names))
    return ids, commands, names


def feature(name, enabled):
    request('api/features/' + name + (':enable?force=true' if enabled else ':disable'), 'POST')


class NoRedirect(urllib.request.HTTPRedirectHandler):
    def redirect_request(self, request, fp, code, message, headers, new_url):
        return None


def public(path, status=200):
    opener = urllib.request.build_opener(NoRedirect)
    try:
        response = opener.open(urllib.parse.urljoin(base, path), timeout=60)
    except urllib.error.HTTPError as error:
        response = error
    with response:
        text = response.read().decode()
        assert response.status == status, (path, response.status, text[:400])
        cookies = '\n'.join(response.headers.get_all('Set-Cookie') or [])
        return text, response.headers, urllib.parse.unquote(cookies)


with tempfile.TemporaryDirectory(prefix='culture-picker-cli-', dir=state_path.parent) as config_home:
    pomi('context', 'add', name, base, '--current')
    feature('OrchardCore.ContentLocalization.ContentCulturePicker', True)
    feature('OrchardCore.RemoteManagement.Mcp', True)
    feature('OrchardCore.Html', True)
    pomi('themes', 'set-current', 'TheTheme', client='cli-fixture')
    pomi('api', 'refresh', '--force')
    section = 'content-culture-picker'
    initial = request('api/settings/sections/' + section)['values']
    try:
        assert [item['name'] for item in pomi('settings', 'sections', 'list')] == [section]
        schema = pomi('settings', 'sections', 'schema', section)
        assert set(schema['properties']) == {'setCookie', 'redirectToHomepage', 'setCookieOnContentRequest'}
        assert not pomi('settings', 'sections', 'update', section, body={})['changed']
        for invalid in [{'setCookie': None}, {'redirectToHomepage': None}, {'setCookieOnContentRequest': None},
                {'setCookie': 1}, {'setCookieOnContentRequest': 'true'}, {'setCookie': False, 'cookieLifeTime': 10}]:
            pomi('settings', 'sections', 'update', section, body=invalid, status=400)
        assert pomi('settings', 'sections', 'show', section)['values'] == initial
        for client in ['cli-denied', 'cli-discovery', 'cli-content-localizer']:
            for method in ['GET', 'PUT']:
                request('api/settings/sections/' + section, method, {} if method == 'PUT' else None, client=client, status=403)
            if client != 'cli-denied':
                tool('show', {'path': {'name': section}}, client=client, status=403)
        pomi('localization', 'settings', 'update', client='cli-fixture', body={'defaultCulture': 'en', 'supportedCultures': ['en', 'fr']})
        definition = {'name': name, 'displayName': name,
            'settings': {'ContentTypeSettings': {'draftable': True, 'versionable': True, 'creatable': True}},
            'parts': [{'name': part, 'partName': part} for part in ['TitlePart', 'HtmlBodyPart', 'AutoroutePart', 'LocalizationPart']]}
        pomi('content', 'types', 'create', client='cli-fixture', body=definition)
        marker = name + '-body'
        source = pomi('content', 'items', 'save', client='cli-fixture', body={'ContentType': name,
            'TitlePart': {'Title': name}, 'HtmlBodyPart': {'Html': '<p>' + marker + '</p>'},
            'AutoroutePart': {'Path': name.lower()}})
        pomi('settings', 'set-home-content', source['ContentItemId'], client='cli-fixture')
        translated = pomi('content', 'localizations', 'create', source['ContentItemId'], client='cli-content-localizer', body={'culture': 'fr'})
        target_id = translated['item']['contentItemId']
        target_path = name.lower() + '-fr'
        pomi('content', 'items', 'update-draft', target_id, client='cli-fixture', body={'AutoroutePart': {'Path': target_path}})
        request('api/content/' + target_id + '/publish', 'POST', status=200, raw=True)
        selection = 'RedirectToLocalizedContent?' + urllib.parse.urlencode({'targetCulture': 'fr', 'contentItemUrl': '/missing-' + name, 'queryStringValue': '?page=2'})
        patch = {'setCookie': False, 'redirectToHomepage': True, 'setCookieOnContentRequest': False}
        first = pomi('settings', 'sections', 'update', section, body=patch)
        assert first['changed'] and not first['reloadRequested']
        assert not pomi('settings', 'sections', 'update', section, body=patch)['changed']
        _, headers, cookies = public(selection, 302)
        assert urllib.parse.urlparse(headers['Location']).path.endswith('/' + target_path), headers
        assert urllib.parse.urlparse(headers['Location']).query == 'page=2'
        assert '.AspNetCore.Culture' not in cookies
        patch = {'setCookie': True, 'redirectToHomepage': False}
        result = tool('update', {'path': {'name': section}, 'body': patch})
        assert result['changed'] and not result['reloadRequested']
        assert not result['section']['values']['setCookieOnContentRequest']
        _, headers, cookies = public(selection, 302)
        assert urllib.parse.urlparse(headers['Location']).path.endswith('/missing-' + name)
        assert 'c=fr|uic=fr' in cookies, cookies
        text, _, cookies = public(target_path)
        assert marker in text and '.AspNetCore.Culture' not in cookies
        pomi('settings', 'sections', 'update', section, body={'setCookieOnContentRequest': True})
        text, _, cookies = public(target_path)
        assert marker in text and 'c=fr|uic=fr' in cookies
        pomi('settings', 'sections', 'update', section, body={'setCookieOnContentRequest': False})
        assert '.AspNetCore.Culture' not in public(target_path)[2]
        ids, commands, names = catalog()
        assert len([command for command in commands if command.startswith('settings sections ')]) == 4
        feature('OrchardCore.ContentLocalization.ContentCulturePicker', False)
        request('api/settings/sections/' + section, status=404)
        assert len(request('api/content/' + source['ContentItemId'] + '/localizations')) == 2
        feature('OrchardCore.ContentLocalization.ContentCulturePicker', True)
        assert request('api/settings/sections/' + section)['values']['setCookie']
        feature('OrchardCore.RemoteManagement.Cli', False)
        assert tool('show', {'path': {'name': section}})['name'] == section
    finally:
        feature('OrchardCore.ContentLocalization.ContentCulturePicker', True)
        feature('OrchardCore.RemoteManagement.Cli', True)
        request('api/settings/sections/' + section, 'PUT', initial)
    print('PASS: culture picker partial updates, validation, retries and live cookie/redirect behavior')
    print('PASS: culture picker HTTP/CLI/MCP permissions and independent feature lifecycle')
    print('Synthetic localized homepage/content remain in the disposable tenant; original picker settings restored.')
