#!/usr/bin/env python3
"""Verify ETag and If-Match on the management APIs replacing a resource, on a disposable tenant."""
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
suffix = secrets.token_hex(4)
checked = []
retried = []


def fetch_token():
    body = urllib.parse.urlencode(dict(grant_type='client_credentials', client_id='cli-fixture',
        client_secret=state['OC_CLIENT_SECRET'], scope='orchardcore.management')).encode()
    with urllib.request.urlopen(urllib.request.Request(base + 'connect/token', data=body,
            headers={'Content-Type': 'application/x-www-form-urlencoded'}), timeout=30) as response:
        return json.load(response)['access_token']


token = fetch_token()


def request(path, method='GET', body=None, if_match=None, status=None):
    headers = {'Accept': 'application/json', 'Authorization': 'Bearer ' + token, 'Content-Type': 'application/json'}
    if if_match is not None:
        headers['If-Match'] = if_match
    req = urllib.request.Request(urllib.parse.urljoin(base, path), method=method, headers=headers,
        data=json.dumps(body).encode() if body is not None else None)
    try:
        response = urllib.request.urlopen(req, timeout=120)
    except urllib.error.HTTPError as error:
        response = error
    with response:
        text = response.read().decode()
        if status is not None:
            assert response.status == status, (method, path, response.status, text[:800])
        try:
            return response.status, response.headers.get('ETag'), json.loads(text) if text else None
        except json.JSONDecodeError:
            raise AssertionError((method, path, response.status, text[:400]))


def verify(label, path, update, *, change=None, create=None):
    """Checks the versions of a resource, where `update` is a full body and `change` alters it."""
    if create:
        created_status, created_etag, created_body = request(*create)
        assert created_status in (200, 201), (label, created_status, created_body)
        assert created_etag, (label, 'no ETag on create')
    _, first, _ = request(path, status=200)
    assert first and first.startswith('"'), (label, first)
    _, again, _ = request(path, status=200)
    assert first == again, (label, 'the version changes between two reads', first, again)
    if create:
        # A retry that returns the stored resource returns its version too.
        retry_status, retry_etag, retry_body = request(*create)
        assert retry_status in (200, 409), (label, 'create retry', retry_status, retry_body)
        if retry_status == 200:
            assert retry_etag == first, (label, 'the retry and the read disagree', retry_etag, first)
            retried.append(label)

    # A write without If-Match keeps working, so existing callers do.
    _, unconditional, _ = request(path, 'PUT', update, status=200)
    _, read, _ = request(path, status=200)
    assert unconditional == read, (label, 'the write and the read disagree', unconditional, read)

    # A stale If-Match is refused without changing anything, and tells the current version.
    _, current, problem = request(path, 'PUT', change(update), '"stale"', status=412)
    assert problem['code'] == 'precondition_failed', (label, problem)
    assert current == read, (label, 'the 412 has another version', current, read)
    _, unchanged, _ = request(path, status=200)
    assert unchanged == read, (label, 'a refused write changed the resource')

    # The current version applies, and yields a new one that the previous one no longer matches.
    _, applied, _ = request(path, 'PUT', change(update), read, status=200)
    assert applied != read, (label, 'the version did not change', applied)
    _, after, _ = request(path, status=200)
    assert after == applied, (label, 'the write and the read disagree after a change', applied, after)
    request(path, 'PUT', update, read, status=412)
    request(path, 'PUT', change(update), '*', status=200)
    checked.append(label)


def with_description(update):
    return {**update, 'description': 'changed'}


for feature in ['OrchardCore.Layers', 'OrchardCore.Placements', 'OrchardCore.Templates', 'OrchardCore.UrlRewriting', 'OrchardCore.Sitemaps',
                'OrchardCore.Roles', 'OrchardCore.Users', 'OrchardCore.Queries', 'OrchardCore.Workflows', 'OrchardCore.ContentTypes']:
    request('api/features/' + feature + ':enable?force=true', 'POST')
token = fetch_token()

# Roles
role = 'Occ' + suffix
verify('roles', 'api/roles/' + role, {'roleDescription': 'first'}, change=lambda update: {'roleDescription': 'second'},
    create=('api/roles', 'POST', {'roleName': role, 'permissionNames': []}))

# Users
user = 'occ' + suffix
_, _, created = request('api/users', 'POST', {'userName': user, 'email': user + '@example.test', 'isEnabled': True,
    'password': secrets.token_urlsafe(24) + 'aA1!', 'roleNames': []}, status=201)
verify('users', 'api/users/' + created['userId'], {'phoneNumber': '+15550100'}, change=lambda update: {'phoneNumber': '+15550101'})

# Tenants
tenant = 'Occ' + suffix
tenant_update = {'requestUrlPrefix': tenant.lower(), 'description': 'first', 'databaseProvider': 'Sqlite', 'tablePrefix': tenant}
verify('tenants', 'api/tenants/' + tenant, tenant_update, change=with_description,
    create=('api/tenants', 'POST', {'name': tenant, **tenant_update}))

# OpenID
scope = 'occ' + suffix
scope_body = {'name': scope, 'displayName': scope, 'description': 'first', 'resources': []}
verify('openid scopes', 'api/openid/scopes/by-name?name=' + scope, scope_body, change=with_description,
    create=('api/openid/scopes', 'POST', scope_body))
application = 'occ-' + suffix
application_body = {'clientId': application, 'displayName': 'first', 'clientType': 'public', 'applicationType': 'native', 'consentType': 'explicit',
    'roles': [], 'scopes': [], 'allowAuthorizationCodeFlow': True, 'redirectUris': 'http://127.0.0.1:33418/callback'}
verify('openid applications', 'api/openid/applications/by-client-id?clientId=' + application, application_body,
    change=lambda update: {**update, 'displayName': 'second'}, create=('api/openid/applications', 'POST', application_body))

# Content definitions
part = 'Occ' + suffix + 'Part'
part_body = {'name': part, 'settings': {'ContentPartSettings': {'attachable': True, 'description': 'first'}}, 'fields': []}
verify('content parts', 'api/content-definition/parts/' + part, part_body,
    change=lambda update: {**update, 'settings': {'ContentPartSettings': {'attachable': True, 'description': 'second'}}},
    create=('api/content-definition/parts', 'POST', part_body))
content_type = 'Occ' + suffix + 'Type'
type_body = {'name': content_type, 'displayName': 'First ' + suffix, 'settings': {'ContentTypeSettings': {'creatable': True}}, 'parts': []}
verify('content types', 'api/content-definition/types/' + content_type, type_body, change=lambda update: {**update, 'displayName': 'Second ' + suffix},
    create=('api/content-definition/types', 'POST', type_body))

# Queries
query = 'Occ' + suffix
query_body = {'name': query, 'source': 'Sql', 'properties': {'SqlQueryMetadata': {'Template': "SELECT DocumentId FROM ContentItemIndex WHERE ContentType='First'"}}}
verify('queries', 'api/queries/named/' + query, query_body,
    change=lambda update: {**update, 'properties': {'SqlQueryMetadata': {'Template': "SELECT DocumentId FROM ContentItemIndex WHERE ContentType='Second'"}}},
    create=('api/queries', 'POST', query_body))

# Layers
layer = 'Occ' + suffix
layer_body = {'name': layer, 'description': 'first', 'conditions': []}
verify('layers', 'api/layers/by-name?name=' + layer, layer_body, change=with_description, create=('api/layers', 'POST', layer_body))

# Placements
shape = 'Occ' + suffix
placement_body = {'shapeType': shape, 'nodes': [{'place': 'Content:1'}]}
verify('placements', 'api/placements/by-shape?shapeType=' + shape, placement_body,
    change=lambda update: {**update, 'nodes': [{'place': 'Content:2'}]}, create=('api/placements', 'POST', placement_body))

# Templates
template = 'Occ__' + suffix
template_body = {'name': template, 'content': 'first', 'description': 'first'}
verify('templates', 'api/templates/' + template, template_body, change=with_description, create=('api/templates', 'POST', template_body))

# URL rewriting
rule = {'id': 'occ' + suffix, 'name': 'occ' + suffix, 'source': 'Redirect', 'pattern': '^/occ-' + suffix + '$',
    'substitutionPattern': '/first', 'isCaseInsensitive': True, 'queryStringPolicy': 'Append', 'redirectType': 'Found'}
rule_path = 'api/url-rewriting/rules/' + rule['id']
verify('url rewriting rules', rule_path, rule, change=lambda update: {**update, 'substitutionPattern': '/second'},
    create=('api/url-rewriting/rules', 'POST', rule))

# Moving a rule isn't a change that an update could lose.
other = {**rule, 'id': 'other' + suffix, 'name': 'other' + suffix}
request('api/url-rewriting/rules', 'POST', other, status=201)
before_move = request(rule_path, status=200)[1]
request('api/url-rewriting/rules/' + other['id'] + '/position', 'PUT', {'position': 0}, status=200)
assert request(rule_path, status=200)[1] == before_move

# Workflow types
workflow = {'name': 'Occ' + suffix, 'isEnabled': False, 'transitions': [], 'activities': [{'activityId': 'set-output', 'name': 'SetOutputTask',
    'x': 0, 'y': 0, 'isStart': True, 'properties': {'OutputName': 'result', 'Syntax': 'JavaScript', 'Value': {'Expression': '42'}}}]}
_, _, created = request('api/workflows/types', 'POST', workflow, status=201)
verify('workflow types', 'api/workflows/types/' + created['workflowTypeId'], {**created}, change=lambda update: {**update, 'name': 'OccChanged' + suffix})

# Sitemaps
sitemap = {'name': 'Occ' + suffix, 'path': 'occ-' + suffix + '.xml', 'enabled': True, 'kind': 'Sitemap', 'containedSitemapIds': []}
_, _, created = request('api/sitemaps', 'POST', sitemap, status=201)
verify('sitemaps', 'api/sitemaps/' + created['id'], sitemap, change=lambda update: {**update, 'name': 'OccChanged' + suffix})

# Site settings, whose updates are partial
_, _, settings = request('api/settings', status=200)
site_name = settings['siteName']
verify('site settings', 'api/settings', {'siteName': site_name}, change=lambda update: {'siteName': site_name + ' changed'})
request('api/settings', 'PUT', {'siteName': site_name}, status=200)

# Every readable section has a stable version, and the update of one returns its next version.
_, _, sections = request('api/settings/sections', status=200)
assert sections
for section in sections:
    path = 'api/settings/sections/' + section['name']
    assert request(path, status=200)[1] == request(path, status=200)[1], section['name']
request('api/settings/sections/' + sections[0]['name'], 'PUT', {}, '"stale"', status=412)
verify('settings sections', 'api/settings/sections/layer-zones', {'zones': ['Content', 'Footer']}, change=lambda update: {'zones': ['Content']})
request('api/settings/sections/layer-zones', 'PUT', {'zones': ['Content', 'Footer']}, status=200)

# OpenAPI declares the headers, and Pomi projects the optional header into an option.
_, _, document = request('openapi/v1.json', status=200)
operation = document['paths']['/api/roles/{roleId}']['put']
if_match = next(parameter for parameter in operation['parameters'] if parameter['name'] == 'If-Match')
assert if_match['in'] == 'header' and not if_match.get('required')
assert 'ETag' in operation['responses']['200']['headers'] and '412' in operation['responses']
assert 'ETag' in document['paths']['/api/roles/{roleId}']['get']['responses']['200']['headers']

with tempfile.TemporaryDirectory(prefix='occ-smoke-') as config_home:
    def pomi(*args, body=None, failure=False):
        env = os.environ.copy()
        env.update(OC_FIXTURE_CONFIG_HOME=config_home)
        command = [sys.executable, str(wrapper), str(state_path), *args, '--output', 'json']
        if body is not None:
            command.append('--stdin')
        result = subprocess.run(command, input=json.dumps(body) if body is not None else None, text=True, capture_output=True, env=env, timeout=180)
        assert (result.returncode != 0) == failure, (args[:3], result.returncode, result.stdout[:400], result.stderr[:400])
        return result

    pomi('context', 'add', 'occ', base, '--current')
    pomi('api', 'refresh', '--force')
    assert '--if-match' in pomi('roles', 'update', '--help').stdout
    # Without the option, an update keeps working.
    pomi('roles', 'update', role, body={'roleDescription': 'from pomi'})
    shown = pomi('roles', 'show', role, '--show-etag')
    etag = next(line[len('ETag: '):] for line in shown.stderr.splitlines() if line.startswith('ETag: '))
    assert json.loads(shown.stdout)['roleName'] == role
    refused = pomi('roles', 'update', role, '--if-match', '"stale"', body={'roleDescription': 'refused'}, failure=True)
    assert json.loads(refused.stderr or refused.stdout)['error']['details']['code'] == 'precondition_failed'
    pomi('roles', 'update', role, '--if-match', etag, body={'roleDescription': 'accepted'})

# MCP tools can't send the header, and stay available for the same operations.
request('api/features/OrchardCore.RemoteManagement.Mcp:enable?force=true', 'POST')
token = fetch_token()
req = urllib.request.Request(base + 'mcp', method='POST', data=json.dumps({'jsonrpc': '2.0', 'id': 1, 'method': 'tools/list', 'params': {}}).encode(),
    headers={'Authorization': 'Bearer ' + token, 'Content-Type': 'application/json', 'Accept': 'application/json, text/event-stream'})
with urllib.request.urlopen(req, timeout=120) as response:
    text = response.read().decode()
payload = next((line[5:] for line in text.splitlines() if line.startswith('data:')), text)
tools = {tool['name']: tool for tool in json.loads(payload)['result']['tools']}
assert 'roles_update' in tools and 'If-Match' not in json.dumps(tools['roles_update']['inputSchema'])

# Clean up what doesn't go away with the fixture and could disturb another smoke script.
request('api/layers/by-name?name=' + layer, 'DELETE')
request('api/placements/by-shape?shapeType=' + shape, 'DELETE')
request('api/templates/' + template, 'DELETE')
print(json.dumps({'status': 'passed', 'resources': checked, 'retried': retried}))
