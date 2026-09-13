#!/usr/bin/env python3
"""Verify tracked index lifecycle operations through HTTP, Pomi and MCP against real query results."""
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


def operation_tool(command, arguments, status=200):
    result = request('mcp', 'POST', {'jsonrpc': '2.0', 'id': 1, 'method': 'tools/call',
        'params': {'name': 'indexes_' + command, 'arguments': {'query': arguments}}}, client='cli-indexes')['result']
    content = json.loads(result['content'][0]['text'])
    assert content['statusCode'] == status, (command, content['statusCode'], status)
    return json.loads(content['body']) if content['body'] else None


def completed(operation, transport='http'):
    assert operation['state'] in ('Pending', 'Running'), operation['state']
    deadline = time.monotonic() + 150
    next_progress = time.monotonic() + 30
    while True:
        if transport == 'pomi':
            current = pomi('indexes', 'operations', 'show', operation['id'])
        elif transport == 'mcp':
            current = operation_tool('operations_show', {'id': operation['id']})
        else:
            current = request('api/indexes/operations/by-id?id=' + operation['id'], client='cli-indexes')
        assert current['indexId'] == operation['indexId']
        if current['state'] == 'Completed':
            assert current['outcome'] == 'Completed'
            return current
        assert current['state'] in ('Pending', 'Running'), (current['state'], current.get('outcome'))
        assert time.monotonic() < deadline, 'operation completion deadline exceeded'
        if time.monotonic() >= next_progress:
            print('Waiting for operation:', operation['action'], flush=True)
            next_progress += 30
        time.sleep(1)


with tempfile.TemporaryDirectory(prefix='lucene-lifecycle-cli-', dir=state_path.parent) as config_home:
    pomi('context', 'add', name, base, '--current')
    request('api/features/OrchardCore.Lucene:enable?force=true', 'POST')
    request('api/features/OrchardCore.RemoteManagement.Mcp:enable?force=true', 'POST')
    pomi('api', 'refresh', '--force')
    provider = next(value for value in pomi('indexes', 'providers', 'list') if value['name'] == 'Lucene')
    content_source = next(value for value in provider['sources'] if value['type'] == 'Content')
    assert set(content_source['lifecycleActions']) == {'synchronize', 'reset', 'rebuild'}
    item_id = index_id = None
    query_created = False
    types = []
    query_name = name + 'Query'
    for type_name in [name, name + 'Other']:
        pomi('content', 'types', 'create', body={'name': type_name, 'displayName': type_name,
            'settings': {'ContentTypeSettings': {'creatable': True, 'draftable': True, 'versionable': True}},
            'parts': [{'name': 'TitlePart', 'partName': 'TitlePart', 'settings': {
                'LuceneContentIndexSettings': {'Included': True, 'Stored': True, 'Keyword': True}}}]}, client='cli-fixture')
        types.append(type_name)
    try:
        title = 'lifecycle' + secrets.token_hex(8)
        item = pomi('content', 'items', 'create-draft', body={'ContentType': name, 'TitlePart': {'Title': title}}, client='cli-fixture')
        item_id = item['ContentItemId']
        pomi('content', 'items', 'publish', item_id, client='cli-fixture')
        definition = {'name': name, 'indexName': name.lower(), 'indexedContentTypes': [name], 'storeSourceData': True}
        created = pomi('indexes', 'lucene', 'create', body=definition)
        index_id = created['id']
        request('api/queries', 'POST', {'name': query_name, 'source': 'Lucene', 'returnContentItems': True,
            'properties': {'LuceneQueryMetadata': {'Index': name, 'Template': json.dumps({
                'query': {'term': {'TitlePart': title}}, 'size': 10})}}}, status=201)
        query_created = True
        def count():
            return request('api/queries/named/' + query_name + '/execute', 'POST', {})['count']
        operation = request('api/indexes/by-id:synchronize?id=' + index_id, 'POST', client='cli-indexes', status=202)
        completed(operation)
        assert count() == 1
        completed(pomi('indexes', 'reset', index_id, '--force'), 'pomi')
        assert count() == 1
        print('PASS: HTTP synchronization and Pomi reset complete and preserve indexed content', flush=True)
        definition['indexedContentTypes'] = [name + 'Other']
        pomi('indexes', 'lucene', 'update', index_id, body=definition)
        completed(operation_tool('rebuild', {'id': index_id}, status=202), 'mcp')
        assert count() == 0, 'rebuild must remove documents excluded by the new definition'
        definition['indexedContentTypes'] = [name]
        pomi('indexes', 'lucene', 'update', index_id, body=definition)
        last = completed(pomi('indexes', 'rebuild', index_id, '--force'))
        assert count() == 1, 'rebuild must restore pre-existing content from indexing tasks'
        print('PASS: MCP/Pomi rebuilds change real query results and restore existing content', flush=True)
        request('api/indexes/by-id:reset?id=' + index_id, 'POST', client='cli-discovery', status=403)
        request('api/indexes/operations/by-id?id=' + last['id'], client='cli-discovery', status=403)
        request('api/indexes/operations/by-id?id=' + last['id'], client=None, status=401)
        request('api/features/OrchardCore.Lucene:disable', 'POST')
        assert request('api/indexes/operations/by-id?id=' + last['id'], client='cli-indexes')['state'] == 'Completed'
        request('api/indexes/by-id:rebuild?id=' + index_id, 'POST', client='cli-indexes', status=501)
        request('api/features/OrchardCore.Lucene:enable?force=true', 'POST')
        print('PASS: permission denials, persisted status and provider feature gate', flush=True)
    finally:
        request('api/features/OrchardCore.Lucene:enable?force=true', 'POST')
        if query_created:
            request('api/queries/named/' + query_name, 'DELETE', status=(200, 204))
        if index_id:
            pomi('indexes', 'lucene', 'delete', index_id, '--force')
        if item_id:
            pomi('content', 'items', 'delete', item_id, '--force', client='cli-fixture')
        for type_name in types:
            pomi('content', 'types', 'delete', type_name, '--force', client='cli-fixture')
