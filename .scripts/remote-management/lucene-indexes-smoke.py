#!/usr/bin/env python3
"""Verify Lucene index definitions, shared mutations and permission checks through HTTP/Pomi/MCP."""
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
        assert response.status == status, (method, path, response.status, status)
        if raw:
            return text, response.headers
        if response.headers.get('Content-Type', '').startswith('text/event-stream'):
            return next(json.loads(line[6:]) for line in text.splitlines()
                if line.startswith('data: ') and json.loads(line[6:]).get('id') == 1)
        return json.loads(text) if text and 'json' in response.headers.get('Content-Type', '') else None


def pomi(*args, body=None, client='cli-indexes', status=None, failure=False):
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
    assert result.returncode == 0, (args[:3], 'CLI request failed')
    return json.loads(result.stdout) if result.stdout.strip() else None


def tool(verb, arguments=None, client='cli-indexes', status=200):
    result = request('mcp', 'POST', {'jsonrpc': '2.0', 'id': 1, 'method': 'tools/call',
        'params': {'name': 'indexes_lucene_' + verb, 'arguments': arguments or {}}}, client=client)['result']
    content = json.loads(result['content'][0]['text'])
    assert content['statusCode'] == status, (verb, content['statusCode'], status)
    assert result.get('isError', False) == (status >= 400)
    return json.loads(content['body']) if content['body'] else None


def catalog():
    result = request('mcp', 'POST', {'jsonrpc': '2.0', 'id': 1, 'method': 'tools/list', 'params': {}})['result']
    names = [entry['name'] for entry in result['tools']]
    assert len(names) == len(set(names)), 'duplicate MCP tool names'
    return [name for name in names if name.startswith('indexes_lucene_')]


with tempfile.TemporaryDirectory(prefix='lucene-indexes-cli-', dir=state_path.parent) as config_home:
    pomi('context', 'add', name, base, '--current')
    request('api/features/OrchardCore.RemoteManagement.Mcp:enable?force=true', 'POST')
    request('api/features/OrchardCore.Lucene:enable?force=true', 'POST')
    pomi('api', 'refresh', '--force')
    assert len(catalog()) == 5
    assert 'standardanalyzer' in pomi('indexes', 'lucene', 'analyzers')
    pomi('content', 'types', 'create', body={'name': name, 'displayName': name,
        'settings': {'ContentTypeSettings': {'creatable': True, 'draftable': True, 'versionable': True}},
        'parts': [{'name': 'TitlePart', 'partName': 'TitlePart', 'settings': {}}]}, client='cli-fixture')
    body = {'name': name, 'indexName': name.lower(), 'indexedContentTypes': [name],
        'indexLatest': False, 'culture': 'any', 'analyzerName': 'standardanalyzer',
        'storeSourceData': False, 'queryAnalyzerName': 'standardanalyzer',
        'allowLuceneQueries': False, 'defaultVersion': 'LUCENE_48', 'defaultSearchFields': ['Content.ContentItem.FullText']}
    created = pomi('indexes', 'lucene', 'create', body=body)
    index_id = created['id']
    path = 'api/indexes/lucene/by-id?' + urllib.parse.urlencode({'id': index_id})
    try:
        assert created['definition'] == body
        assert pomi('indexes', 'lucene', 'create', body=body) == created
        assert tool('show', {'query': {'id': index_id}}) == created
        assert request(path, client='cli-indexes') == created
        request(path, client=None, status=401)
        for denied in ['cli-denied', 'cli-discovery']:
            request(path, client=denied, status=403)
            request(path, 'PUT', body, client=denied, status=403)
        for invalid in [{**body, 'indexedContentTypes': []}, {**body, 'analyzerName': 'missing'},
                        {**body, 'indexName': '../outside'}, {**body, 'properties': {'Secret': 'not-allowed'}}]:
            request(path, 'PUT', invalid, client='cli-indexes', status=400)
            assert request(path, client='cli-indexes') == created
        request('api/indexes/lucene', 'POST', {**body, 'storeSourceData': True}, client='cli-indexes', status=409)
        replacement = {**body, 'name': name + ' updated', 'storeSourceData': True, 'indexLatest': True}
        changed = pomi('indexes', 'lucene', 'update', index_id, body=replacement)
        assert changed['definition'] == replacement, {key: (replacement[key], changed['definition'].get(key)) for key in replacement if replacement[key] != changed['definition'].get(key)}
        assert tool('update', {'query': {'id': index_id}, 'body': replacement}) == changed
        request('api/features/OrchardCore.RemoteManagement.Cli:disable', 'POST')
        try:
            assert tool('show', {'query': {'id': index_id}}) == changed
        finally:
            request('api/features/OrchardCore.RemoteManagement.Cli:enable?force=true', 'POST')
        request('api/features/OrchardCore.Lucene:disable', 'POST')
        try:
            assert catalog() == []
            request(path, client='cli-indexes', status=404)
        finally:
            request('api/features/OrchardCore.Lucene:enable?force=true', 'POST')
        assert len(catalog()) == 5
        assert request(path, client='cli-indexes') == changed
        pomi('api', 'refresh', '--force')
        pomi('indexes', 'lucene', 'delete', index_id, '--force')
        tool('delete', {'query': {'id': index_id}}, status=204)
    finally:
        request(path, 'DELETE', client='cli-indexes', status=204)
        pomi('content', 'types', 'delete', name, '--force', client='cli-fixture')
print('PASS: typed Lucene definitions through HTTP/Pomi/MCP, retries, permissions, Lucene feature lifecycle and CLI feature independence')
