#!/usr/bin/env python3
"""Verify URL rewrite CRUD, source validation, ordering, redirects and authorization."""
import copy
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
name = 'RewriteSmoke' + secrets.token_hex(4)
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
            return text
        if response.headers.get('Content-Type', '').startswith('text/event-stream'):
            return next(json.loads(line[6:]) for line in text.splitlines()
                if line.startswith('data: ') and json.loads(line[6:]).get('id') == 1)
        return json.loads(text) if text else None


def pomi(*args, body=None, client='cli-url-rewriting', status=None, failure=False):
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


def tool(verb, arguments=None, client='cli-url-rewriting', status=200):
    result = request('mcp', 'POST', {'jsonrpc': '2.0', 'id': 1, 'method': 'tools/call',
        'params': {'name': 'url-rewriting_rules_' + verb, 'arguments': arguments or {}}}, client=client)['result']
    content = json.loads(result['content'][0]['text'])
    assert content['statusCode'] == status, content
    assert result.get('isError', False) == (status >= 400)
    return json.loads(content['body']) if content['body'] else None


class NoRedirect(urllib.request.HTTPRedirectHandler):
    def redirect_request(self, req, fp, code, msg, headers, newurl):
        return None


opener = urllib.request.build_opener(NoRedirect())


def probe(path, expected):
    try:
        response = opener.open(base.rstrip('/') + path, timeout=30)
    except urllib.error.HTTPError as error:
        response = error
    with response:
        response.read()
        assert response.status == expected, (path, response.status, expected)
        return response.headers


def feature(enabled):
    request('api/features/OrchardCore.UrlRewriting' + (':enable?force=true' if enabled else ':disable'), 'POST')


with tempfile.TemporaryDirectory(prefix='rewrite-cli-', dir=state_path.parent) as config_home:
    feature(True)
    request('api/features/OrchardCore.RemoteManagement.Mcp:enable?force=true', 'POST')
    pomi('context', 'add', name, base, '--current')
    sources = pomi('url-rewriting', 'rules', 'sources')
    assert {'Rewrite', 'Redirect'} <= {source['name'] for source in sources if source['isWritable']}
    initial = pomi('url-rewriting', 'rules', 'list', '--take', '200')['items']
    offset = len(initial)
    path = '/pomi-rewrite-' + secrets.token_hex(4)
    first_id, second_id, rewrite_id = [name + suffix for suffix in ['first', 'second', 'rewrite']]
    first = {'id': first_id, 'name': first_id, 'source': 'Redirect', 'pattern': '^' + path + '/(.*)$',
        'substitutionPattern': '/target/$1?fixed=1', 'redirectType': 'Found', 'queryStringPolicy': 'Append'}
    assert pomi('url-rewriting', 'rules', 'validate', body=first)['isValid']
    created = pomi('url-rewriting', 'rules', 'create', body=first)
    assert created['id'] == first_id and created['definition']['substitutionPattern'] == first['substitutionPattern']
    retry = pomi('url-rewriting', 'rules', 'create', body=first)
    assert retry == created, {key: (created.get(key), retry.get(key)) for key in created if created.get(key) != retry.get(key)}
    pomi('url-rewriting', 'rules', 'create', body={**first, 'name': 'different'}, status=409)
    for invalid in [{**first, 'queryStringPolicy': 'invalid'}, {**first, 'redirectType': '999'},
            {**first, 'pattern': '('}, {**first, 'substitutionPattern': '/target\nRewriteRule .* /other [R=302]'},
            {**first, 'skipFurtherRules': True}]:
        assert not pomi('url-rewriting', 'rules', 'validate', body=invalid)['isValid']
        pomi('url-rewriting', 'rules', 'update', first_id, body=invalid, status=400)
    assert pomi('url-rewriting', 'rules', 'show', first_id) == created
    headers = probe(path + '/one?original=2', 302)
    location = urllib.parse.urlparse(headers['Location'])
    assert location.path == '/target/one', location
    assert urllib.parse.parse_qs(location.query) == {'fixed': ['1'], 'original': ['2']}, location
    drop = {**first, 'queryStringPolicy': 'Drop', 'redirectType': 'TemporaryRedirect'}
    pomi('url-rewriting', 'rules', 'update', first_id, body=drop)
    location = urllib.parse.urlparse(probe(path + '/two?original=2', 307)['Location'])
    assert location.path == '/target/two' and urllib.parse.parse_qs(location.query) == {'fixed': ['1']}, location
    second = {**first, 'id': second_id, 'name': second_id, 'substitutionPattern': '/second/$1', 'redirectType': 'PermanentRedirect'}
    tool('create', {'body': second}, status=201)
    assert probe(path + '/two', 307)
    moved = pomi('url-rewriting', 'rules', 'move', second_id, body={'position': offset})
    assert moved['order'] == offset
    assert urllib.parse.urlparse(probe(path + '/two', 308)['Location']).path == '/second/two'
    pomi('url-rewriting', 'rules', 'move', second_id, body={'position': offset})
    rewrite = {'id': rewrite_id, 'name': rewrite_id, 'source': 'Rewrite', 'pattern': '^' + path + '$',
        'substitutionPattern': '/api/management/manifest', 'skipFurtherRules': True}
    probe(path, 404)
    pomi('url-rewriting', 'rules', 'create', body=rewrite)
    probe(path, 401)  # Reaches the protected API after an in-process rewrite.
    assert request(path.lstrip('/'))['openApiUrl']
    request(path.lstrip('/'), client='cli-denied', status=403)
    for client in ['cli-discovery', 'cli-denied']:
        for method, route, body in [('GET', '', None), ('GET', '/' + first_id, None),
                ('POST', '/validate', first), ('POST', '', first), ('PUT', '/' + first_id, first),
                ('PUT', '/' + first_id + '/position', {'position': 0}), ('DELETE', '/' + first_id, None)]:
            request('api/url-rewriting/rules' + route, method, body, client=client, status=403)
        request('api/url-rewriting/sources', client=client, status=403)
    tool('show', {'path': {'id': first_id}}, client='cli-discovery', status=403)
    request('api/url-rewriting/rules', 'POST', {**first, 'unexpected': True}, status=400, raw=True)
    feature(False)
    request('api/url-rewriting/rules', status=404, raw=True)
    probe(path + '/two', 404)
    feature(True)
    assert tool('show', {'path': {'id': first_id}})['id'] == first_id
    request('api/features/OrchardCore.RemoteManagement.Cli:disable', 'POST')
    assert tool('show', {'path': {'id': first_id}})['id'] == first_id
    request('api/features/OrchardCore.RemoteManagement.Cli:enable?force=true', 'POST')
    for rule_id in [first_id, second_id, rewrite_id]:
        pomi('url-rewriting', 'rules', 'delete', rule_id, '--force')
        pomi('url-rewriting', 'rules', 'delete', rule_id, '--force')
    probe(path, 404)
    assert pomi('url-rewriting', 'rules', 'list', '--take', '200')['items'] == initial
    print('PASS: rewrite rules CLI/MCP CRUD, stable retries, source validation and persisted ordering')
    print('PASS: real redirects/captures/status/query policies and in-process rewriting')
    print('PASS: resource permissions, feature lifecycle and MCP without CLI')
