#!/usr/bin/env python3
"""Verify the Elasticsearch index definitions and lifecycle against a real server, through HTTP, Pomi and MCP.

Every claim about the index is checked by asking Elasticsearch directly. Without a URL the script skips, as there is
nothing to verify against. Azure AI Search has no local emulator, so nothing here applies to it.
"""
import argparse
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

parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument('fixture', help='The fixture.json of a fixture started with the OrchardCore__Search__Elasticsearch__* variables.')
parser.add_argument('url', nargs='?', default=os.environ.get('OC_FIXTURE_ELASTICSEARCH_URL'), help='The URL of the Elasticsearch server of that fixture.')
parser.add_argument('--container', help='The Docker container of that server, which only this option allows to stop and restart for the outage checks.')
options = parser.parse_args()
if not options.url:
    print('SKIP: no Elasticsearch URL was given, so nothing was verified. Pass it as the second argument, see the README.')
    raise SystemExit(0)

state_path = Path(options.fixture)
state = json.loads(state_path.read_text())
base = state['url']
server = options.url.rstrip('/')
# The script creates and removes indexes, so it only runs against disposable local servers.
assert urllib.parse.urlparse(base).hostname in ('127.0.0.1', 'localhost', '::1')
assert urllib.parse.urlparse(server).hostname in ('127.0.0.1', 'localhost', '::1')
wrapper = Path(__file__).with_name('pomi-fixture.py')
name = 'EsSmoke' + secrets.token_hex(4)
index_name = name.lower()
tokens = {}
# Internals that must never reach a client while the server is down.
INTERNALS = ('Connection refused', 'HttpRequestException', 'SocketException', 'Elastic.', 'TransportException', '   at ', server)


def token(client):
    if client not in tokens:
        body = urllib.parse.urlencode(dict(grant_type='client_credentials', client_id=client,
            client_secret=state['OC_CLIENT_SECRET'], scope='orchardcore.management')).encode()
        with urllib.request.urlopen(urllib.request.Request(base + 'connect/token', data=body,
                headers={'Content-Type': 'application/x-www-form-urlencoded'}), timeout=30) as response:
            tokens[client] = json.load(response)['access_token']
    return tokens[client]


def send(path, method='GET', body=None, client='cli-indexes', if_match=None):
    url = urllib.parse.urljoin(base, path)
    assert urllib.parse.urlparse(url).netloc == urllib.parse.urlparse(base).netloc
    headers = {'Content-Type': 'application/json', 'Accept': 'application/json, text/event-stream'}
    if client:
        headers['Authorization'] = 'Bearer ' + token(client)
    if if_match:
        headers['If-Match'] = if_match
    request = urllib.request.Request(url, method=method, headers=headers,
        data=json.dumps(body).encode() if body is not None else None)
    try:
        response = urllib.request.urlopen(request, timeout=60)
    except urllib.error.HTTPError as error:
        response = error
    with response:
        return response.status, response.read().decode(), response.headers


def call(path, method='GET', body=None, client='cli-indexes', status=200, if_match=None):
    code, text, headers = send(path, method, body, client, if_match)
    assert code == status, (method, path, code, status, text[:800])
    if headers.get('Content-Type', '').startswith('text/event-stream'):
        return next(json.loads(line[6:]) for line in text.splitlines()
            if line.startswith('data: ') and json.loads(line[6:]).get('id') == 1)
    return json.loads(text) if text and 'json' in headers.get('Content-Type', '') else None


def call_etag(path, method='GET', body=None, status=200, if_match=None):
    code, text, headers = send(path, method, body, if_match=if_match)
    assert code == status, (method, path, code, status, text[:800])
    assert headers.get('ETag'), (method, path, 'no ETag header')
    return (json.loads(text) if text else None), headers['ETag'], headers


def elastic(path, method='GET', status=200, body=None):
    request = urllib.request.Request(server + path, method=method, headers={'Content-Type': 'application/json'},
        data=json.dumps(body).encode() if body is not None else None)
    try:
        response = urllib.request.urlopen(request, timeout=30)
    except urllib.error.HTTPError as error:
        response = error
    with response:
        text = response.read().decode()
        assert response.status == status, (method, path, response.status, status, text[:800])
        return json.loads(text) if text else None


def full_name(short_name):
    """Finds the index on the server: its name has the prefix of the tenant, which only the server can confirm."""
    matches = [row['index'] for row in elastic('/_cat/indices?format=json&h=index') if row['index'].endswith('_' + short_name)]
    return matches[0] if len(matches) == 1 else None


def uuid(index):
    return next(iter(elastic('/' + index + '/_settings').values()))['settings']['index']['uuid']


def document_ids(index):
    elastic('/' + index + '/_refresh', 'POST')
    return {hit['_id'] for hit in elastic('/' + index + '/_search?size=100')['hits']['hits']}


def wait_for_documents(index, condition, message):
    deadline = time.monotonic() + 60
    while not condition(document_ids(index)):
        assert time.monotonic() < deadline, message
        time.sleep(1)


def pomi(*args, body=None, client='cli-indexes', status=None, check=True):
    env = os.environ.copy()
    env.update(OC_FIXTURE_CONFIG_HOME=config_home, OC_FIXTURE_CLIENT_ID=client)
    command = [sys.executable, str(wrapper), str(state_path), *args, '--output', 'json']
    if body is not None:
        command.append('--stdin')
    result = subprocess.run(command, input=json.dumps(body) if body is not None else '',
        capture_output=True, text=True, env=env, timeout=120)
    if not check:
        return None
    if status:
        assert result.returncode != 0, args
        error = json.loads(result.stderr or result.stdout)['error']
        assert error['status'] == status, (args[:3], error['status'], status)
        return error
    assert result.returncode == 0, (args[:3], (result.stderr or result.stdout)[:1600])
    return json.loads(result.stdout) if result.stdout.strip() else None


def tool(tool_name, arguments=None, status=200):
    result = call('mcp', 'POST', {'jsonrpc': '2.0', 'id': 1, 'method': 'tools/call',
        'params': {'name': tool_name, 'arguments': arguments or {}}})['result']
    content = json.loads(result['content'][0]['text'])
    assert content['statusCode'] == status, (tool_name, content['statusCode'], status)
    assert result['isError'] == (status >= 400), (tool_name, result['isError'])
    return content.get('body')


def finished(operation, transport='http', state='Completed', outcome='Completed'):
    assert operation['state'] in ('Pending', 'Running'), operation['state']
    deadline = time.monotonic() + 150
    while True:
        if transport == 'pomi':
            current = pomi('indexes', 'operations', 'show', operation['id'])
        elif transport == 'mcp':
            current = tool('indexes_operations_show', {'query': {'id': operation['id']}})
        else:
            current = call('api/indexes/operations/by-id?id=' + operation['id'])
        if current['state'] not in ('Pending', 'Running'):
            assert (current['state'], current['outcome']) == (state, outcome), (operation['action'], current)
            return current
        assert time.monotonic() < deadline, 'operation completion deadline exceeded'
        time.sleep(1)


def lifecycle(index_id, action, **expected):
    return finished(call('api/indexes/by-id:' + action + '?id=' + index_id, 'POST', status=202), **expected)


def profile_names():
    return {item['name'] for item in call('api/indexes?take=200')['items']}


def create_item(type_name, title, publish=True):
    item = pomi('content', 'items', 'create-draft', body={'ContentType': type_name, 'TitlePart': {'Title': title}}, client='cli-fixture')
    created_items.append(item['ContentItemId'])
    if publish:
        pomi('content', 'items', 'publish', item['ContentItemId'], client='cli-fixture')
    return item['ContentItemId']


def definition_route(index_id):
    return 'api/indexes/elasticsearch/by-id?id=' + index_id


def docker(*args):
    assert options.container
    subprocess.run(['docker', *args, options.container], check=True, capture_output=True, stdin=subprocess.DEVNULL, timeout=120)


def wait_for_server():
    deadline = time.monotonic() + 180
    while True:
        try:
            if elastic('/_cluster/health?wait_for_status=yellow&timeout=5s')['status'] in ('yellow', 'green'):
                return
        except (OSError, AssertionError):
            pass
        assert time.monotonic() < deadline, 'Elasticsearch did not come back'
        time.sleep(2)


def assert_clean_problem(path, method, body, status=503):
    code, text, _ = send(path, method, body)
    assert code == status, (method, path, code, status, text[:800])
    for internal in INTERNALS:
        assert internal not in text, (method, path, 'leaks', internal)


created_items = []
created_types = []
created_queries = []
created_indexes = []
stopped = False
with tempfile.TemporaryDirectory(prefix='elasticsearch-live-cli-', dir=state_path.parent) as config_home:
    pomi('context', 'add', name, base, '--current')
    call('api/features/OrchardCore.Elasticsearch:enable?force=true', 'POST', client='cli-fixture')
    call('api/features/OrchardCore.RemoteManagement.Mcp:enable?force=true', 'POST', client='cli-fixture')
    pomi('api', 'refresh', '--force')
    provider = next(value for value in pomi('indexes', 'providers', 'list') if value['name'] == 'Elasticsearch')
    assert set(next(value for value in provider['sources'] if value['type'] == 'Content')['lifecycleActions']) == {'synchronize', 'reset', 'rebuild'}
    analyzers = call('api/indexes/elasticsearch/analyzers')
    assert 'standard' in analyzers
    assert pomi('indexes', 'elasticsearch', 'analyzers') == analyzers
    assert tool('indexes_elasticsearch_analyzers') == analyzers
    print('PASS: provider discovery and analyzers through HTTP, Pomi and MCP', flush=True)

    try:
        for type_name in [name, name + 'Other']:
            pomi('content', 'types', 'create', client='cli-fixture', body={'name': type_name, 'displayName': type_name,
                'settings': {'ContentTypeSettings': {'creatable': True, 'draftable': True, 'versionable': True}},
                'parts': [{'name': 'TitlePart', 'partName': 'TitlePart'}]})
            created_types.append(type_name)
        existing_title = 'existing' + secrets.token_hex(6)
        existing_item = create_item(name, existing_title)

        definition = {'name': name, 'indexName': index_name, 'indexedContentTypes': [name]}
        created, etag, headers = call_etag('api/indexes/elasticsearch', 'POST', definition, status=201)
        index_id = created['id']
        created_indexes.append(index_id)
        assert headers['Location'].endswith(index_id), headers['Location']
        assert created['definition']['storeSourceData'] is True
        index = full_name(index_name)
        assert index, 'The fixture did not create the index on this server: set OrchardCore__Search__Elasticsearch__Url and Ports.'
        mappings = elastic('/' + index + '/_mapping')[index]['mappings']
        assert mappings['_source']['enabled'] is True
        assert mappings['properties']['ContentItemId']['type'] == 'keyword'
        wait_for_documents(index, lambda ids: existing_item in ids, 'content published before the index must reach it')
        print('PASS: creating a definition creates a real index with its mappings and synchronizes existing content', flush=True)

        retried, retried_etag, _ = call_etag('api/indexes/elasticsearch', 'POST', definition, status=200)
        assert retried['id'] == index_id and retried_etag == etag
        problem = call('api/indexes/elasticsearch', 'POST', {**definition, 'indexLatest': True}, status=409)
        assert problem['code'] == 'conflict'
        call('api/indexes/elasticsearch', 'POST', {**definition, 'name': name + 'Taken'}, status=409)
        call('api/indexes/elasticsearch', 'POST', {'name': name + 'Upper', 'indexName': 'Upper' + index_name, 'indexedContentTypes': [name]}, status=400)
        call('api/indexes/elasticsearch', 'POST', {**definition, 'name': name + 'Private', 'indexName': index_name + 'p', 'IndexMappings': {}}, status=400)
        call('api/indexes/elasticsearch', 'POST', {**definition, 'name': name + 'Analyzer', 'indexName': index_name + 'a', 'analyzerName': 'unknown'}, status=400)
        assert not {name + suffix for suffix in ('Taken', 'Upper', 'Private', 'Analyzer')} & profile_names()
        assert not full_name('upper' + index_name) and not full_name(index_name + 'p') and not full_name(index_name + 'a')
        assert pomi('indexes', 'elasticsearch', 'create', body=definition)['id'] == index_id
        assert pomi('indexes', 'elasticsearch', 'create', body={**definition, 'indexLatest': True}, status=409)['details']['code'] == 'conflict'
        assert tool('indexes_elasticsearch_create', {'body': definition})['id'] == index_id
        tool('indexes_elasticsearch_create', {'body': {**definition, 'indexLatest': True}}, status=409)
        print('PASS: create retries return 200 with the ETag, conflicts 409 and invalid definitions 400 without touching the server', flush=True)

        shown, shown_etag, _ = call_etag(definition_route(index_id))
        assert shown['id'] == index_id and shown_etag == etag
        assert pomi('indexes', 'elasticsearch', 'show', index_id)['definition']['name'] == name
        changed, changed_etag, _ = call_etag(definition_route(index_id), 'PUT', {**definition, 'culture': 'fr'}, if_match=etag)
        assert changed['definition']['culture'] == 'fr' and changed_etag != etag
        code, _, headers = send(definition_route(index_id), 'PUT', definition, if_match=etag)
        assert code == 412 and headers['ETag'] == changed_etag
        assert call(definition_route(index_id))['definition']['culture'] == 'fr'
        assert pomi('indexes', 'elasticsearch', 'update', index_id, '--if-match', etag, body=definition, status=412)['details']['code'] == 'precondition_failed'
        assert pomi('indexes', 'elasticsearch', 'update', index_id, '--if-match', changed_etag, body=definition)['definition']['culture'] == 'any'
        unchanged, unchanged_etag, _ = call_etag(definition_route(index_id), 'PUT', definition)
        assert unchanged_etag == etag, 'the same state must have the same version'
        call(definition_route(index_id), 'PUT', {**definition, 'indexName': index_name + 'x'}, status=400)
        assert tool('indexes_elasticsearch_update', {'query': {'id': index_id}, 'body': {**definition, 'culture': 'fr'}})['definition']['culture'] == 'fr'
        assert tool('indexes_elasticsearch_update', {'query': {'id': index_id}, 'body': definition})['definition']['culture'] == 'any'
        print('PASS: updates through HTTP, Pomi and MCP, with If-Match honored and the provider index name immutable', flush=True)

        identity = uuid(index)
        assert finished(call('api/indexes/by-id:synchronize?id=' + index_id, 'POST', status=202))['action'] == 'Synchronize'
        assert uuid(index) == identity and existing_item in document_ids(index)
        finished(pomi('indexes', 'reset', index_id, '--force'), 'pomi')
        assert uuid(index) == identity and existing_item in document_ids(index), 'reset must keep the index'
        finished(tool('indexes_synchronize', {'query': {'id': index_id}}, status=202), 'mcp')
        rebuilt = lifecycle(index_id, 'rebuild')
        assert rebuilt['lastTaskId'] > 0
        rebuilt_identity = uuid(index)
        assert rebuilt_identity != identity and existing_item in document_ids(index), 'rebuild must recreate the index and restore its documents'
        call_etag(definition_route(index_id), 'PUT', {**definition, 'indexedContentTypes': [name + 'Other']})
        assert existing_item in document_ids(index), 'updating must not reindex'
        finished(pomi('indexes', 'rebuild', index_id, '--force'), 'pomi')
        assert not document_ids(index), 'rebuild must apply the new definition'
        call_etag(definition_route(index_id), 'PUT', definition)
        lifecycle(index_id, 'rebuild')
        assert document_ids(index) == {existing_item}
        print('PASS: synchronize, reset and rebuild complete, through HTTP status, Pomi and MCP, and do what they say on the server', flush=True)

        search_title = 'search' + secrets.token_hex(6)
        draft = create_item(name, 'draft' + secrets.token_hex(6), publish=False)
        published = create_item(name, search_title)
        wait_for_documents(index, lambda ids: published in ids, 'a published item must reach the index')
        lifecycle(index_id, 'synchronize')
        assert draft not in document_ids(index), 'a draft must stay out of an index of published items'
        document = elastic('/' + index + '/_doc/' + published)['_source']
        assert document['ContentItemId'] == published and search_title in document['Content.ContentItem.FullText']
        query_name = name + 'Query'
        template = json.dumps({'query': {'match': {'Content.ContentItem.FullText': search_title}}, 'size': 10})
        call('api/queries', 'POST', {'name': query_name, 'source': 'Elasticsearch', 'returnContentItems': True,
            'properties': {'ElasticsearchQueryMetadata': {'Index': name, 'Template': template}}}, client='cli-fixture', status=201)
        created_queries.append(query_name)
        results = call('api/queries/named/' + query_name + ':execute', 'POST', {}, client='cli-fixture')
        assert [item['ContentItemId'] for item in results['items']] == [published], results
        assert pomi('queries', 'execute', query_name, body={}, client='cli-fixture')['count'] == 1
        pomi('content', 'items', 'delete', published, '--force', client='cli-fixture')
        wait_for_documents(index, lambda ids: published not in ids, 'a deleted item must leave the index')
        assert call('api/queries/named/' + query_name + ':execute', 'POST', {}, client='cli-fixture')['count'] == 0
        print('PASS: published content reaches the index and can be queried, drafts stay out, and deleted content leaves', flush=True)

        tools = {value['name'] for value in call('mcp', 'POST', {'jsonrpc': '2.0', 'id': 1, 'method': 'tools/list', 'params': {}})['result']['tools']}
        assert {'indexes_elasticsearch_show', 'indexes_elasticsearch_create', 'indexes_elasticsearch_update', 'indexes_synchronize', 'indexes_operations_show'} <= tools
        confirmation_tools = {'indexes_elasticsearch_delete', 'indexes_reset', 'indexes_rebuild'}
        if confirmation_tools & tools:
            assert confirmation_tools <= tools
            finished(tool('indexes_reset', {'query': {'id': index_id}}, status=202), 'mcp')
            finished(tool('indexes_rebuild', {'query': {'id': index_id}}, status=202), 'mcp')
            assert document_ids(index) == {existing_item}
            scratch = tool('indexes_elasticsearch_create', {'body': {'name': name + 'Mcp', 'indexName': index_name + 'mcp', 'indexedContentTypes': [name]}}, status=201)
            created_indexes.append(scratch['id'])
            assert full_name(index_name + 'mcp')
            tool('indexes_elasticsearch_delete', {'query': {'id': scratch['id']}}, status=204)
            created_indexes.remove(scratch['id'])
            assert not full_name(index_name + 'mcp')
            print('PASS: MCP reset, rebuild and delete, enabled by AllowConfirmationRequiredTools', flush=True)
        else:
            result = call('mcp', 'POST', {'jsonrpc': '2.0', 'id': 1, 'method': 'tools/call',
                'params': {'name': 'indexes_elasticsearch_delete', 'arguments': {'query': {'id': index_id}}}})['result']
            assert result['isError'] is True and full_name(index_name), 'a tool that needs a confirmation must not run'
            print('PASS: MCP refuses delete, reset and rebuild without AllowConfirmationRequiredTools', flush=True)

        extra_analyzers = [value for value in analyzers if value != 'standard']
        if extra_analyzers:
            scratch = call('api/indexes/elasticsearch', 'POST', {'name': name + 'Analyzed', 'indexName': index_name + 'analyzed',
                'indexedContentTypes': [name], 'analyzerName': extra_analyzers[0]}, status=201)
            created_indexes.append(scratch['id'])
            scratch_index = full_name(index_name + 'analyzed')
            analysis = elastic('/' + scratch_index + '/_settings')[scratch_index]['settings']['index']['analysis']
            assert extra_analyzers[0] in analysis['analyzer'], analysis
            call(definition_route(scratch['id']), 'DELETE', status=204)
            created_indexes.remove(scratch['id'])
            print('PASS: a configured analyzer reaches the settings of the index', flush=True)
        else:
            print('SKIP: only the standard analyzer is configured on this fixture', flush=True)

        unsourced = {'name': name + 'Unsourced', 'indexName': index_name + 'unsourced', 'indexedContentTypes': [name], 'storeSourceData': False}
        scratch = call('api/indexes/elasticsearch', 'POST', unsourced, status=201)
        created_indexes.append(scratch['id'])
        assert scratch['definition']['storeSourceData'] is False
        scratch_index = full_name(index_name + 'unsourced')
        assert elastic('/' + scratch_index + '/_mapping')[scratch_index]['mappings']['_source']['enabled'] is False
        wait_for_documents(scratch_index, lambda ids: existing_item in ids, 'an index without source still gets its documents')
        assert '_source' not in elastic('/' + scratch_index + '/_search')['hits']['hits'][0]
        sourced = call(definition_route(scratch['id']), 'PUT', {**unsourced, 'storeSourceData': True})
        assert sourced['definition']['storeSourceData'] is True
        lifecycle(scratch['id'], 'rebuild')
        assert elastic('/' + scratch_index + '/_mapping')[scratch_index]['mappings']['_source']['enabled'] is True
        call(definition_route(scratch['id']), 'PUT', unsourced)
        lifecycle(scratch['id'], 'rebuild')
        assert elastic('/' + scratch_index + '/_mapping')[scratch_index]['mappings']['_source']['enabled'] is False
        assert existing_item in document_ids(scratch_index)
        call(definition_route(scratch['id']), 'DELETE', status=204)
        created_indexes.remove(scratch['id'])
        assert not full_name(index_name + 'unsourced')
        print('PASS: storeSourceData reaches the mapping, on creation and on update followed by a rebuild', flush=True)

        orphan = index + 'orphan'
        elastic('/' + orphan, 'PUT', body={})
        assert_clean_problem('api/indexes/elasticsearch', 'POST', {'name': name + 'Orphan', 'indexName': index_name + 'orphan', 'indexedContentTypes': [name]})
        assert name + 'Orphan' not in profile_names()
        elastic('/' + orphan, 'DELETE')
        print('PASS: an index already on the server is rejected with 503, keeping no profile, and is not overwritten', flush=True)

        elastic('/' + index, 'DELETE')
        lifecycle(index_id, 'synchronize', state='Failed', outcome='ProviderMissing')
        lifecycle(index_id, 'reset', state='Failed', outcome='ProviderMissing')
        lifecycle(index_id, 'rebuild')
        assert document_ids(index) == {existing_item}
        print('PASS: an index removed from the server is reported missing, and a rebuild restores it', flush=True)

        if options.container:
            pending = {'name': name + 'Pending', 'indexName': index_name + 'pending', 'indexedContentTypes': [name]}
            docker('stop')
            stopped = True
            names = profile_names()
            assert_clean_problem('api/indexes/elasticsearch', 'POST', pending)
            assert profile_names() == names, 'a failed creation must not keep a profile'
            assert pomi('indexes', 'elasticsearch', 'create', body=pending, status=503)['status'] == 503
            tool('indexes_elasticsearch_create', {'body': pending}, status=503)
            assert_clean_problem(definition_route(index_id), 'DELETE', None)
            assert pomi('indexes', 'elasticsearch', 'delete', index_id, '--force', status=503)['status'] == 503
            assert call(definition_route(index_id))['id'] == index_id, 'a failed removal must keep the profile'
            assert call('api/indexes/elasticsearch/analyzers') == analyzers
            assert call(definition_route(index_id), 'PUT', {**definition, 'culture': 'fr'})['definition']['culture'] == 'fr'
            assert call(definition_route(index_id), 'PUT', definition)['definition']['culture'] == 'any'
            outage_item = create_item(name, 'outage' + secrets.token_hex(6))
            lifecycle(index_id, 'synchronize', state='Failed', outcome='ProviderUnavailable')
            finished(pomi('indexes', 'reset', index_id, '--force'), 'pomi', state='Failed', outcome='ProviderUnavailable')
            lifecycle(index_id, 'rebuild', state='Failed', outcome='ProviderRejected')
            code, text, _ = send('api/queries/named/' + query_name + ':execute', 'POST', {}, client='cli-fixture')
            assert code < 500 and not any(internal in text for internal in INTERNALS), (code, text[:800])
            print('PASS: with the server down, mutations answer 503 or a failed operation, without leaking or losing a profile', flush=True)

            docker('start')
            wait_for_server()
            stopped = False
            assert full_name(index_name) == index and existing_item in document_ids(index)
            lifecycle(index_id, 'synchronize')
            assert document_ids(index) == {existing_item, outage_item}, 'content changed during the outage must be caught up'
            recovered = call('api/indexes/elasticsearch', 'POST', pending, status=201)
            created_indexes.append(recovered['id'])
            assert full_name(pending['indexName'])
            call(definition_route(recovered['id']), 'DELETE', status=204)
            created_indexes.remove(recovered['id'])
            assert not full_name(pending['indexName'])
            print('PASS: after the outage, the index catches up and creations and removals work again', flush=True)
        else:
            print('SKIP: outage checks, as no --container was given', flush=True)

        call(definition_route(index_id), client='cli-discovery', status=403)
        call(definition_route(index_id), client=None, status=401)
        pomi_definition = {'name': name + 'Pomi', 'indexName': index_name + 'pomi', 'indexedContentTypes': [name]}
        created_by_pomi = pomi('indexes', 'elasticsearch', 'create', body=pomi_definition)
        created_indexes.append(created_by_pomi['id'])
        assert full_name(pomi_definition['indexName'])
        pomi('indexes', 'elasticsearch', 'delete', created_by_pomi['id'], '--force')
        created_indexes.remove(created_by_pomi['id'])
        assert not full_name(pomi_definition['indexName'])
        pomi('indexes', 'elasticsearch', 'delete', created_by_pomi['id'], '--force', status=404)
        call(definition_route(index_id), 'DELETE', status=204)
        created_indexes.remove(index_id)
        assert not full_name(index_name)
        elastic('/' + index, 'HEAD', status=404)
        call(definition_route(index_id), 'DELETE', status=404)
        call(definition_route(index_id), status=404)
        print('PASS: deleting a definition removes the index from the server, and deleting again returns 404', flush=True)
    finally:
        if stopped:
            docker('start')
            wait_for_server()
        for query_name in created_queries:
            send('api/queries/named/' + query_name, 'DELETE', client='cli-fixture')
        for index_id in created_indexes:
            send(definition_route(index_id), 'DELETE')
        for item_id in created_items:
            pomi('content', 'items', 'delete', item_id, '--force', client='cli-fixture', check=False)
        for type_name in created_types:
            pomi('content', 'types', 'delete', type_name, '--force', client='cli-fixture', check=False)
print('All Elasticsearch live checks passed. Azure AI Search has no local emulator and was not exercised.')
