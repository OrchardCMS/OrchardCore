#!/usr/bin/env python3
"""Verify the provider settings sections, the feature preview and the provider index definitions on an HTTPS fixture.

Elasticsearch and Azure AI Search have no server here: this proves the routes, their validation and the 503 of an
unavailable provider, not that an index is created on a real server, which elasticsearch-live-smoke.py does for the first.
"""
import json
import os
from pathlib import Path
import secrets
import ssl
import subprocess
import sys
import tempfile
import time
import urllib.error
import urllib.parse
import urllib.request

state = json.loads(Path(sys.argv[1]).read_text())
base = state['url']
assert urllib.parse.urlparse(base).scheme == 'https', 'Start the fixture with OC_FIXTURE_HTTPS=1.'
assert urllib.parse.urlparse(base).hostname in ('127.0.0.1', 'localhost', '::1')
opener = urllib.request.build_opener(urllib.request.HTTPSHandler(
    context=ssl.create_default_context(cafile=state['certificatePath'])))
tokens = {}
secret = 'test-secret-' + secrets.token_hex(8)

SECTIONS = [
    # name, feature, valid values, secret properties, invalid values
    ('recaptcha', 'OrchardCore.ReCaptcha', {'siteKey': 'site'}, ['secretKey'], {'siteKey': 1}),
    ('reverse-proxy', 'OrchardCore.ReverseProxy', {'enableXForwardedHost': True}, [], {'enableXForwardedHost': 'yes'}),
    ('google-analytics', 'OrchardCore.Google.Analytics', {'trackingId': 'G-TEST'}, [], {'trackingId': ' '}),
    ('google-tag-manager', 'OrchardCore.Google.TagManager', {'containerId': 'GTM-TEST'}, [], {'containerId': ' '}),
    ('google-authentication', 'OrchardCore.Google.GoogleAuthentication', {'clientId': 'id', 'callbackPath': '/signin-smoke-google'}, ['clientSecret'], {'clientId': 'id', 'callbackPath': 'invalid'}),
    ('github-authentication', 'OrchardCore.GitHub.Authentication', {'clientId': 'id', 'saveTokens': True}, ['clientSecret'], {'clientId': ' '}),
    ('microsoft-account', 'OrchardCore.Microsoft.Authentication.MicrosoftAccount', {'appId': 'id'}, ['appSecret'], {'appId': ' '}),
    ('microsoft-entra-id', 'OrchardCore.Microsoft.Authentication.AzureAD', {'displayName': 'Entra', 'appId': 'id', 'tenantId': 'tenant'}, [], {'tenantId': ' '}),
    ('twitter', 'OrchardCore.Twitter', {'consumerKey': 'key'}, ['consumerSecret', 'accessToken', 'accessTokenSecret'], {'consumerKey': ' '}),
    ('twitter-signin', 'OrchardCore.Twitter.Signin', {'saveTokens': True}, [], {'callbackPath': 'invalid'}),
    ('facebook', 'OrchardCore.Facebook', {'appId': 'id', 'version': 'v9.0'}, ['appSecret'], {'appId': 'id', 'version': '9'}),
    ('facebook-login', 'OrchardCore.Facebook.Login', {'saveTokens': True}, [], {'callbackPath': 'invalid'}),
    ('facebook-pixel', 'OrchardCore.Facebook.Pixel', {'pixelId': 'pixel'}, ['conversionsApiAccessToken'], {'pixelId': 1}),
    ('sms', 'OrchardCore.Sms', {'defaultProvider': 'Log'}, [], {'defaultProvider': 'NotAProvider'}),
    ('sms-twilio', 'OrchardCore.Sms', {'isEnabled': True, 'phoneNumber': '+14155550100', 'accountSid': 'sid'}, ['authToken'], {'isEnabled': True, 'phoneNumber': 'invalid', 'accountSid': 'sid'}),
    ('admin', 'OrchardCore.Admin', {'showBreadcrumb': False, 'listLayout': 'Grid'}, [], {'menuBehavior': '1'}),
]


def token(client):
    if client not in tokens:
        body = urllib.parse.urlencode(dict(grant_type='client_credentials', client_id=client,
            client_secret=state['OC_CLIENT_SECRET'], scope='orchardcore.management')).encode()
        with opener.open(urllib.request.Request(base + 'connect/token', data=body), timeout=60) as response:
            tokens[client] = json.load(response)['access_token']
    return tokens[client]


def send(path, method='GET', body=None, client='cli-fixture', origin=None):
    url = urllib.parse.urljoin(origin or base, path)
    headers = {'Content-Type': 'application/json', 'Accept': 'application/json, text/event-stream'}
    if client:
        headers['Authorization'] = 'Bearer ' + token(client)
    request = urllib.request.Request(url, method=method, headers=headers,
        data=json.dumps(body).encode() if body is not None else None)
    try:
        response = opener.open(request, timeout=120)
    except urllib.error.HTTPError as error:
        response = error
    with response:
        return response.status, response.read().decode(), response.headers.get('Content-Type', '')


def call(path, method='GET', body=None, client='cli-fixture', status=200):
    code, text, content_type = send(path, method, body, client)
    assert code == status, (method, path, code, text[:800])
    if content_type.startswith('text/event-stream'):
        return next(json.loads(line[6:]) for line in text.splitlines()
            if line.startswith('data: ') and json.loads(line[6:]).get('id') == 1)
    return json.loads(text) if text else None


def tool(name, arguments=None):
    result = call('mcp', 'POST', {'jsonrpc': '2.0', 'id': 1, 'method': 'tools/call',
        'params': {'name': name, 'arguments': arguments or {}}})['result']
    return json.loads(result['content'][0]['text'])


def wait_until(check, description):
    # A feature change reloads the tenant, which takes a moment.
    last = None
    for _ in range(60):
        try:
            value = check()
            if value:
                return value
        except (AssertionError, OSError) as error:
            last = error
        time.sleep(1)
    raise AssertionError((description, last))


def enabled_features():
    items, skip = [], 0
    while True:
        page = call(f'api/features?skip={skip}&take=200')
        items += page['items']
        skip += 200
        if skip >= page['totalCount']:
            return {item['id'] for item in items if item['isEnabled']}


def enable(feature):
    call(f'api/features/{feature}:enable?force=true', 'POST')
    wait_until(lambda: feature in enabled_features(), f'enable {feature}')


def camel(name):
    return name[0].upper() + name[1:]


def check_section(name, feature, valid, secret_names, invalid):
    section_path = f'api/settings/sections/{name}'
    schema = call(section_path + '/schema')
    assert schema['additionalProperties'] is False, name
    for property_name in secret_names:
        assert schema['properties'][property_name]['writeOnly'] is True, (name, property_name)
    declared = json.dumps(schema)
    assert ('writeOnly' in declared) == bool(secret_names), name

    # Permissions: an identity without any permission gets nothing.
    assert send(section_path, client='cli-denied')[0] == 403, name
    assert send(section_path, 'PUT', valid, client='cli-denied')[0] == 403, name
    assert send(section_path, client=None)[0] == 401, name

    before = call(section_path)
    assert before['name'] == name

    # Validation, and nothing changed.
    call(section_path, 'PUT', {'notManaged': True}, status=400)
    call(section_path, 'PUT', invalid, status=400)
    assert call(section_path)['values'] == before['values'], name

    values = dict(valid)
    for property_name in secret_names:
        values[property_name] = secret
    code, text, _ = send(section_path, 'PUT', values)
    assert code == 200, (name, code, text[:800])
    assert secret not in text, name
    result = json.loads(text)
    # A secret is new every run, and the other values only on the first run of a fixture.
    assert result['changed'] == (bool(secret_names) or any(before['values'].get(key) != value for key, value in valid.items())), (name, result)
    for key, expected in valid.items():
        assert result['section']['values'][key] == expected, (name, key, result)
    for property_name in secret_names:
        assert property_name not in result['section']['values'], name
        assert result['section']['values']['has' + camel(property_name)] is True, name
        assert property_name in result['section']['redactedProperties'], name
    descriptor = next(item for item in call('api/settings/sections') if item['name'] == name)
    assert descriptor['featureId'] == feature and result['reloadRequested'] == (result['changed'] and descriptor['requiresReload']), (name, descriptor)

    if result['reloadRequested']:
        wait_until(lambda: call(section_path)['values'] is not None, f'reload after {name}')

    # The same values again change nothing, the secret included.
    code, retried, _ = send(section_path, 'PUT', values)
    assert code == 200, retried[:400]
    assert json.loads(retried)['changed'] is False, (name, retried[:400])

    read = send(section_path)[1]
    assert secret not in read, name

    # Secret-bearing sections stay refused through MCP, others go through.
    mcp_values = {key: value for key, value in values.items() if key not in secret_names}
    outcome = tool('settings_sections_update', {'path': {'name': name}, 'body': mcp_values})
    if secret_names:
        assert outcome['statusCode'] == 403, (name, outcome)
    else:
        assert outcome['statusCode'] == 200, (name, outcome)
    assert tool('settings_sections_show', {'path': {'name': name}})['statusCode'] == 200, name

    # Clearing removes a secret.
    if secret_names:
        clear = {'clear' + camel(property_name): True for property_name in secret_names if property_name != 'accessToken'}
        if name == 'sms-twilio':
            # An enabled provider requires its token.
            call(section_path, 'PUT', clear, status=400)
            clear['isEnabled'] = False
        call(section_path, 'PUT', clear)
        current = wait_until(lambda: call(section_path), name)
        for property_name in secret_names:
            if property_name != 'accessToken':
                assert current['values']['has' + camel(property_name)] is False, name
    print('ok', name)


def check_feature_preview():
    # A disabled feature with an enabled dependency tree, then a real change compared with what the preview said.
    target = 'OrchardCore.Facebook.Widgets'
    before = enabled_features()
    assert target not in before
    assert send(f'api/features/{target}/preview?action=enable', client='cli-denied')[0] == 403
    assert send(f'api/features/{target}/preview?action=enable', client=None)[0] == 401
    call(f'api/features/{target}/preview?action=remove', status=400)
    call(f'api/features/NotAFeature/preview?action=enable', status=404)
    preview = call(f'api/features/{target}/preview?action=enable&force=true')
    assert preview['isAllowed'] is True and target in preview['featuresToEnable'], preview
    assert enabled_features() == before, 'A preview changed the features.'
    enable(target)
    after = wait_until(enabled_features, 'features')
    missing = set(preview['featuresToEnable']) - after
    assert not missing, (missing, preview)
    extra = after - before - set(preview['featuresToEnable'])
    assert not extra, (extra, preview)

    preview = call(f'api/features/{target}/preview?action=disable')
    assert preview['featuresToDisable'][-1] == target, preview
    expected = set(preview['featuresToDisable']) | set(preview['dependencyOnlyFeaturesToDisable'])
    code, text, _ = send(f'api/features/{target}:disable', 'POST')
    assert code == 200, text[:600]
    gone = wait_until(lambda: (lambda current: current if target not in current else None)(enabled_features()), 'disable')
    assert after - gone == expected, (after - gone, expected)

    # Disabling a feature others need requires force, and nothing changes.
    blocked = call('api/features/OrchardCore.Contents/preview?action=disable')
    assert blocked['requiresForce'] is True and blocked['isAllowed'] is False, blocked
    assert 'OrchardCore.Contents' in blocked['featuresToDisable']
    assert enabled_features() == gone

    # Read-only, so that it is usable through MCP.
    tools = call('mcp', 'POST', {'jsonrpc': '2.0', 'id': 1, 'method': 'tools/list', 'params': {}})['result']['tools']
    described = next(item for item in tools if item['name'] == 'features_preview')
    assert described['annotations']['readOnlyHint'] is True, described
    outcome = tool('features_preview', {'path': {'featureId': target}, 'query': {'action': 'enable'}})
    assert outcome['statusCode'] == 200, outcome

    # The command documented for Pomi, which refreshes its catalog from the server.
    wrapper = Path(__file__).with_name('pomi-fixture.py')
    state_path = Path(sys.argv[1])
    with tempfile.TemporaryDirectory(prefix='provider-cli-', dir=state_path.parent) as config_home:
        env = {**os.environ, 'OC_FIXTURE_CONFIG_HOME': config_home, 'OC_FIXTURE_CLIENT_ID': 'cli-fixture'}
        pomi = lambda *args: subprocess.run([sys.executable, str(wrapper), str(state_path), *args, '--output', 'json'],
            capture_output=True, text=True, env=env, timeout=120)
        for args in (('context', 'add', 'provider-coverage', base, '--current'), ('api', 'refresh', '--force')):
            result = pomi(*args)
            assert result.returncode == 0, (args, result.stderr[:800])
        result = pomi('features', 'preview', target, '--action', 'enable', '--api-force', 'true')
        assert result.returncode == 0, result.stderr[:800]
        assert target in json.loads(result.stdout)['featuresToEnable'], result.stdout[:800]
    print('ok feature preview')


def check_index_definitions(route, features, provider_name):
    for feature in features:
        enable(feature)
    wait_until(lambda: send(f'api/indexes/{route}/analyzers')[0] == 200, f'{route} routes')
    assert send(f'api/indexes/{route}/analyzers', client='cli-denied')[0] == 403
    assert send(f'api/indexes/{route}/analyzers', client=None)[0] == 401
    analyzers = call(f'api/indexes/{route}/analyzers')
    assert analyzers, route

    name = 'smoke' + secrets.token_hex(4)
    valid = {'Name': name, 'IndexName': name, 'IndexedContentTypes': ['Article']}
    call(f'api/indexes/{route}', 'POST', {**valid, 'AnalyzerName': 'not-an-analyzer'}, status=400)
    call(f'api/indexes/{route}', 'POST', {**valid, 'IndexedContentTypes': []}, status=400)
    assert send(f'api/indexes/{route}', 'POST', {**valid, 'IndexMappings': {}})[0] == 400, 'Private mappings are not part of a definition.'
    call(f'api/indexes/{route}/by-id?id=missing', status=404)
    call(f'api/indexes/{route}/by-id?id=missing', 'DELETE', status=404)

    # There is no server: the provider has to report it, without leaving a profile behind.
    code, text, _ = send(f'api/indexes/{route}', 'POST', valid)
    assert code == 503, (route, code, text[:800])
    assert all(item['name'] != name for item in call('api/indexes')['items']), 'A failed creation kept its profile.'

    catalog = call('api/indexes/providers') if send('api/indexes/providers')[0] == 200 else None
    if catalog is not None:
        entries = catalog if isinstance(catalog, list) else catalog.get('items', [])
        assert any(entry.get('name') == provider_name for entry in entries), (provider_name, catalog)
    print('ok index definitions', route)


def check_catalog():
    manifest = call('api/management/manifest')
    document = call(manifest['openApiUrl'])
    commands = {' '.join([*operation['x-oc-cli']['commandGroup'], operation['x-oc-cli']['verb']])
        for item in document['paths'].values() for operation in item.values()
        if isinstance(operation, dict) and operation.get('x-oc-cli')}
    assert 'features preview' in commands
    tools = {item['name'] for item in call('mcp', 'POST', {'jsonrpc': '2.0', 'id': 1, 'method': 'tools/list', 'params': {}})['result']['tools']}
    assert {'indexes_elasticsearch_create', 'indexes_azure-ai-search_create'} <= tools, sorted(name for name in tools if name.startswith('indexes'))
    for route in ('elasticsearch', 'azure-ai-search'):
        for verb in ('show', 'create', 'update', 'delete', 'analyzers'):
            assert f'indexes {route} {verb}' in commands, (route, verb)
    print('ok catalog')


enable('OrchardCore.RemoteManagement.Mcp')
check_feature_preview()
for feature in sorted({entry[1] for entry in SECTIONS}):
    # OrchardCore.Facebook is only enabled as the dependency of its other features.
    if not call(f'api/features/{feature}/preview?action=enable&force=true').get('blockedReason'):
        enable(feature)
listed = {entry['name'] for entry in wait_until(lambda: call('api/settings/sections'), 'sections')}
assert {entry[0] for entry in SECTIONS} <= listed, {entry[0] for entry in SECTIONS} - listed
for entry in SECTIONS:
    check_section(*entry)
check_index_definitions('azure-ai-search', ['OrchardCore.AzureAI'], 'AzureAISearch')
check_index_definitions('elasticsearch', ['OrchardCore.Elasticsearch'], 'Elasticsearch')
check_catalog()
print('All provider coverage checks passed.')
