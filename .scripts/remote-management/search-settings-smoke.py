#!/usr/bin/env python3
"""Verify frontend search settings through HTTP, Pomi, MCP and the existing search page."""
import json
import html
import http.cookiejar
from html.parser import HTMLParser
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
name = 'LuceneSmoke' + secrets.token_hex(4)
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


class LoginForms(HTMLParser):
    def __init__(self):
        super().__init__()
        self.forms = []
        self.current = None

    def handle_starttag(self, tag, attrs):
        attrs = dict(attrs)
        if tag == 'form':
            self.current = {'action': attrs.get('action', ''), 'inputs': []}
            self.forms.append(self.current)
        elif tag == 'input' and self.current is not None and attrs.get('name'):
            self.current['inputs'].append(attrs)

    def handle_endtag(self, tag):
        if tag == 'form':
            self.current = None


def admin_browser():
    browser = urllib.request.build_opener(urllib.request.HTTPCookieProcessor(http.cookiejar.CookieJar()))
    login_url = urllib.parse.urljoin(base, 'Login') + '?' + urllib.parse.urlencode({'returnUrl': urllib.parse.urlparse(base).path + 'search'})
    with browser.open(login_url, timeout=30) as response:
        parser = LoginForms()
        parser.feed(response.read().decode())
    form = next(form for form in parser.forms if any(value.get('type') == 'password' for value in form['inputs']))
    data = {value['name']: value.get('value', '') for value in form['inputs'] if value.get('type') == 'hidden'}
    for value in form['inputs']:
        if value['name'].split('.')[-1].lower() == 'username':
            data[value['name']] = 'admin'
        elif value.get('type') == 'password':
            data[value['name']] = state['adminPassword']
    target = urllib.parse.urljoin(login_url, form['action'])
    assert urllib.parse.urlparse(target).netloc == urllib.parse.urlparse(base).netloc
    with browser.open(urllib.request.Request(target, data=urllib.parse.urlencode(data).encode()), timeout=30) as response:
        assert 'login' not in urllib.parse.urlparse(response.geturl()).path.lower(), 'Fixture administrator login failed'
    return browser


def tool(verb, body=None, client='cli-fixture', status=200):
    arguments = {'path': {'name': 'frontend-search'}}
    if body is not None:
        arguments['body'] = body
    result = request('mcp', 'POST', {'jsonrpc': '2.0', 'id': 1, 'method': 'tools/call',
        'params': {'name': 'settings_sections_' + verb, 'arguments': arguments}}, client=client)['result']
    content = json.loads(result['content'][0]['text'])
    assert content['statusCode'] == status, (verb, content['statusCode'], status)
    return json.loads(content['body']) if content['body'] else None


with tempfile.TemporaryDirectory(prefix='search-settings-cli-', dir=state_path.parent) as config_home:
    pomi('context', 'add', name, base, '--current')
    for feature in ['OrchardCore.Search', 'OrchardCore.Lucene', 'OrchardCore.RemoteManagement.Mcp']:
        request('api/features/' + feature + ':enable?force=true', 'POST')
    pomi('api', 'refresh', '--force')
    route = 'api/settings/sections/frontend-search'
    initial = request(route)['values']
    index_id = None
    pomi('content', 'types', 'create', body={'name': name, 'displayName': name,
        'parts': [{'name': 'TitlePart', 'partName': 'TitlePart'}]})
    try:
        index_id = pomi('indexes', 'lucene', 'create', body={
            'name': name, 'indexName': name.lower(), 'indexedContentTypes': [name]})['id']
        values = {'defaultIndexProfileName': name, 'pageTitle': 'Search page ' + name, 'placeholder': 'Find ' + name}
        first = request(route, 'PUT', values)
        assert first['changed'] and not first['reloadRequested']
        assert not pomi('settings', 'sections', 'update', 'frontend-search', body=values)['changed']
        values['pageTitle'] = 'Updated ' + name
        assert pomi('settings', 'sections', 'update', 'frontend-search', body={'pageTitle': values['pageTitle']})['changed']
        values['placeholder'] = 'MCP ' + name
        assert tool('update', {'placeholder': values['placeholder']})['changed']
        assert tool('show')['values'] == values
        for invalid in [{'pageTitle': False}, {'providerName': 'Lucene'},
                {'defaultIndexProfileName': 'missing-' + name, 'pageTitle': 'Must not apply'}]:
            request(route, 'PUT', invalid, status=400)
        assert request(route)['values'] == values
        print('PASS: HTTP/Pomi/MCP settings patches, retries and atomic validation', flush=True)

        browser = admin_browser()
        with browser.open(urllib.parse.urljoin(base, 'search'), timeout=30) as response:
            page = response.read().decode()
            assert '<h1>' + html.escape(values['pageTitle']) + '</h1>' in page, 'Configured search page title was not rendered'
            assert 'placeholder="' + html.escape(values['placeholder'], quote=True) + '"' in page, 'Configured placeholder was not rendered'
            assert name in page, 'Default profile was not selected'
        with urllib.request.urlopen(urllib.parse.urljoin(base, 'search'), timeout=30) as response:
            assert 'login' in urllib.parse.urlparse(response.geturl()).path.lower(), 'Search settings unexpectedly granted anonymous query access'
        print('PASS: Existing search page uses the configured default/title/placeholder and retains query authorization', flush=True)

        request(route, client=None, status=401)
        request(route, client='cli-indexes', status=403)
        pomi('settings', 'sections', 'show', 'frontend-search', client='cli-indexes', status=403)
        tool('show', client='cli-indexes', status=403)
        cleared = request(route, 'PUT', {'defaultIndexProfileName': None})
        assert cleared['section']['values']['defaultIndexProfileName'] is None
        assert cleared['section']['values']['pageTitle'] == values['pageTitle']
        request('api/features/OrchardCore.Search:disable', 'POST')
        request(route, status=404)
        request('api/features/OrchardCore.Search:enable?force=true', 'POST')
        assert request(route)['values'] == cleared['section']['values']
        request('api/features/OrchardCore.RemoteManagement.Cli:disable', 'POST')
        assert tool('show')['values'] == cleared['section']['values']
        print('PASS: Permission denials, null clearing, feature persistence and MCP without CLI', flush=True)
    finally:
        request('api/features/OrchardCore.Search:enable?force=true', 'POST')
        request('api/features/OrchardCore.RemoteManagement.Cli:enable?force=true', 'POST')
        request(route, 'PUT', initial)
        if index_id:
            pomi('indexes', 'lucene', 'delete', index_id, '--force')
        pomi('content', 'types', 'delete', name, '--force')
