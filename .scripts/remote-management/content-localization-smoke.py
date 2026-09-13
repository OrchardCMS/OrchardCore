#!/usr/bin/env python3
"""Verify content localization, existing manager behavior, permissions and feature projection."""
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
name = 'LocalizationSmoke' + secrets.token_hex(4)
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


def pomi(*args, body=None, client='cli-content-localizer', status=None, failure=False):
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


def tool(verb, arguments=None, client='cli-content-localizer', status=200):
    result = request('mcp', 'POST', {'jsonrpc': '2.0', 'id': 1, 'method': 'tools/call',
        'params': {'name': 'content_localizations_' + verb, 'arguments': arguments or {}}}, client=client)['result']
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


with tempfile.TemporaryDirectory(prefix='content-localizations-cli-', dir=state_path.parent) as config_home:
    pomi('context', 'add', name, base, '--current')
    feature('OrchardCore.ContentLocalization', True)
    feature('OrchardCore.RemoteManagement.Mcp', True)
    feature('OrchardCore.Html', True)
    feature('OrchardCore.Autoroute', True)
    pomi('themes', 'set-current', 'TheTheme', client='cli-fixture')
    pomi('api', 'refresh', '--force')
    pomi('localization', 'settings', 'update', client='cli-fixture', body={'defaultCulture': 'en', 'supportedCultures': ['en', 'fr', 'de']})
    definition = {'name': name, 'displayName': name,
        'settings': {'ContentTypeSettings': {'draftable': True, 'versionable': True, 'creatable': True}},
        'parts': [{'name': part, 'partName': part} for part in ['TitlePart', 'HtmlBodyPart', 'AutoroutePart', 'LocalizationPart']]}
    pomi('content', 'types', 'create', client='cli-fixture', body=definition)
    marker = name + '-body'
    source = pomi('content', 'items', 'save', client='cli-fixture', body={'ContentType': name,
        'TitlePart': {'Title': name}, 'HtmlBodyPart': {'Html': '<p>' + marker + '</p>'},
        'AutoroutePart': {'Path': name.lower()}})
    source_id = source['ContentItemId']
    route = 'api/content/' + source_id + '/localizations'
    assert len(pomi('content', 'localizations', 'list', source_id)) == 1
    created = pomi('content', 'localizations', 'create', source_id, body={'culture': 'FR'})
    target_id = created['item']['contentItemId']
    assert created['created'] and not created['item']['published'] and created['item']['culture'] == 'fr'
    assert target_id != source_id
    again = pomi('content', 'localizations', 'create', source_id, body={'culture': 'fr'})
    assert not again['created'] and again['item'] == created['item']
    target = request('api/content/' + target_id + '?version=latest')
    assert target['HtmlBodyPart']['Html'] == source['HtmlBodyPart']['Html']
    assert target['AutoroutePart']['Path'] is None  # Existing localization handler clears the route.
    assert target['LocalizationPart']['LocalizationSet'] == source['LocalizationPart']['LocalizationSet']
    assert request('api/content/' + source_id + '?version=latest') == source
    assert len(pomi('content', 'localizations', 'list', source_id)) == 2
    assert len(pomi('content', 'localizations', 'list', source_id, '--version', 'published', client='cli-content-reader')) == 1
    # Reading the latest set does not disclose the unpublished French variant to a published-only reader.
    assert len(pomi('content', 'localizations', 'list', source_id, client='cli-content-reader')) == 1
    for client, code in [(None, 401), ('cli-denied', 403), ('cli-discovery', 403), ('cli-content-localizer-no-edit', 403), ('cli-content-reader', 403)]:
        request(route, 'POST', {'culture': 'de'}, client=client, status=code)
    for body in [{'culture': 'unsupported'}, {'culture': None}, {}, {'culture': 'de', 'published': True}]:
        request(route, 'POST', body, status=400, raw=True)
    pomi('content', 'localizations', 'list', source_id, '--version', 'invalid', status=400)
    localized = tool('create', {'path': {'contentItemId': source_id}, 'body': {'culture': 'de'}})
    assert localized['created'] and not localized['item']['published']
    tool('create', {'path': {'contentItemId': source_id}, 'body': {'culture': 'de'}}, client='cli-content-localizer-no-edit', status=403)
    assert len(tool('list', {'path': {'contentItemId': source_id}})) == 3
    # Editing and publication remain explicit content operations outside localization.
    translated_marker = marker + '-fr'
    translated_path = name.lower() + '-fr'
    pomi('content', 'items', 'update-draft', target_id, body={'AutoroutePart': {'Path': translated_path}}, status=403)  # Existing content API also requires AccessContentApi.
    edited = pomi('content', 'items', 'update-draft', target_id, client='cli-fixture', body={'HtmlBodyPart': {'Html': '<p>' + translated_marker + '</p>'}, 'AutoroutePart': {'Path': translated_path}})
    assert not edited['Published']
    assert not pomi('content', 'localizations', 'create', source_id, body={'culture': 'fr'})['created']
    assert request('api/content/' + target_id + '?version=latest')['HtmlBodyPart'] == edited['HtmlBodyPart']
    request('api/content/' + target_id + '/publish', 'POST', status=200, raw=True)
    variants = pomi('content', 'localizations', 'list', source_id, '--version', 'published', client='cli-content-reader')
    assert len(variants) == 2 and all(item['published'] for item in variants)
    assert translated_marker in request(translated_path, client=None, raw=True)
    ids, commands, names = catalog()
    assert len([value for value in commands if value.startswith('content localizations ')]) == 2
    assert 'content_localizations_create' in names
    feature('OrchardCore.ContentLocalization', False)
    ids, commands, names = catalog()
    assert not any(value.startswith('content localizations ') for value in commands)
    request(route, status=404, raw=True)
    feature('OrchardCore.ContentLocalization', True)
    feature('OrchardCore.RemoteManagement.Cli', False)
    assert len(tool('list', {'path': {'contentItemId': source_id}})) == 3
    feature('OrchardCore.RemoteManagement.Cli', True)
    print('Content localization: draft cloning, sequential retries, source preservation, configured cultures and version-aware variant reads passed.', flush=True)
    print('HTTP/MCP permissions, independent CLI/MCP feature behavior and unique discovery passed.', flush=True)
    print('Content localization smoke passed; synthetic content remains confined to the disposable tenant.', flush=True)
