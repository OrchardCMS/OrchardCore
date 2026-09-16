#!/usr/bin/env python3
"""Verify indexed content changes through existing named Lucene queries."""
import json
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


def wait_for_query(query_name, expected_count, phase):
    deadline = time.monotonic() + 150
    next_progress = time.monotonic() + 30
    while True:
        result = request('api/queries/named/' + query_name + '/execute', 'POST', {})
        if result['count'] == expected_count:
            return result
        assert time.monotonic() < deadline, (phase, 'indexing did not reach expected query result', result['count'], expected_count)
        if time.monotonic() >= next_progress:
            print('Waiting for indexing:', phase, flush=True)
            next_progress += 30
        time.sleep(2)


with tempfile.TemporaryDirectory(prefix='lucene-runtime-cli-', dir=state_path.parent) as config_home:
    pomi('context', 'add', name, base, '--current')
    request('api/features/OrchardCore.Lucene:enable?force=true', 'POST')
    request('api/features/OrchardCore.Indexing.Worker:enable?force=true', 'POST')
    pomi('api', 'refresh', '--force')
    query_names = []
    item_id = None
    index_id = None
    pomi('content', 'types', 'create', body={'name': name, 'displayName': name,
        'settings': {'ContentTypeSettings': {'creatable': True, 'draftable': True, 'versionable': True}},
        'parts': [{'name': 'TitlePart', 'partName': 'TitlePart', 'settings': {
            'LuceneContentIndexSettings': {'Included': True, 'Stored': True, 'Keyword': True}}}]}, client='cli-fixture')
    try:
        first = 'first' + secrets.token_hex(8)
        second = 'second' + secrets.token_hex(8)
        item = pomi('content', 'items', 'create-draft', body={'ContentType': name, 'TitlePart': {'Title': first}}, client='cli-fixture')
        item_id = item['ContentItemId']
        pomi('content', 'items', 'publish', item_id, client='cli-fixture')
        created = pomi('indexes', 'lucene', 'create', body={'name': name, 'indexName': name.lower(),
            'indexedContentTypes': [name], 'storeSourceData': True})
        index_id = created['id']
        for suffix, title in [('First', first), ('Second', second)]:
            query_name = name + suffix
            request('api/queries', 'POST', {'name': query_name, 'source': 'Lucene', 'returnContentItems': True,
                'properties': {'LuceneQueryMetadata': {'Index': name, 'Template': json.dumps({
                    'query': {'term': {'TitlePart': title}}, 'size': 10})}}}, status=201)
            query_names.append(query_name)
        result = wait_for_query(query_names[0], 1, 'initial content')
        assert result['items'][0]['ContentItemId'] == item_id
        assert result['items'][0]['TitlePart']['Title'] == first
        assert request('api/queries/named/' + query_names[1] + '/execute', 'POST', {})['count'] == 0
        print('PASS: newly created index serves pre-existing published content through a named query', flush=True)
        draft = pomi('content', 'items', 'draft', item_id, client='cli-fixture')
        draft['TitlePart']['Title'] = second
        pomi('content', 'items', 'update', item_id, body=draft, client='cli-fixture')
        pomi('content', 'items', 'publish', item_id, client='cli-fixture')
        result = wait_for_query(query_names[1], 1, 'updated content')
        assert result['items'][0]['ContentItemId'] == item_id
        assert result['items'][0]['TitlePart']['Title'] == second
        wait_for_query(query_names[0], 0, 'old indexed title removed')
        assert pomi('indexes', 'lucene', 'show', index_id)['definition'] == created['definition']
        print('PASS: background indexing replaces old indexed terms and existing named queries see updated content', flush=True)
    finally:
        for query_name in query_names:
            request('api/queries/named/' + query_name, 'DELETE', status=(200, 204))
        if index_id:
            pomi('indexes', 'lucene', 'delete', index_id, '--force')
        if item_id:
            pomi('content', 'items', 'delete', item_id, '--force', client='cli-fixture')
        pomi('content', 'types', 'delete', name, '--force', client='cli-fixture')
