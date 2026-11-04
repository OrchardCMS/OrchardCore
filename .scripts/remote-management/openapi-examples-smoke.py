#!/usr/bin/env python3
"""Post every OpenAPI request example to its endpoint, and verify Pomi and MCP ignore the examples."""
import json
import os
from pathlib import Path
import secrets
import ssl
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
context = ssl.create_default_context(cafile=state.get('certificatePath'))
urllib.request.install_opener(urllib.request.build_opener(urllib.request.HTTPSHandler(context=context)))
wrapper = Path(__file__).with_name('pomi-fixture.py')
expected = {'ApiCreateTenantManagement', 'ApiCreateRole', 'ApiCreateUser', 'ApiCreateQuery', 'ApiCreateOpenIdApplication',
    'ApiCreateDraftContentItem', 'ApiCreateContentItem'}
placeholder_credentials = ('password', 'clientSecret')
tokens = {}


def token():
    if 'value' not in tokens:
        body = urllib.parse.urlencode(dict(grant_type='client_credentials', client_id='cli-fixture',
            client_secret=state['OC_CLIENT_SECRET'], scope='orchardcore.management')).encode()
        with urllib.request.urlopen(urllib.request.Request(base + 'connect/token', data=body,
                headers={'Content-Type': 'application/x-www-form-urlencoded'}), timeout=30) as response:
            tokens['value'] = json.load(response)['access_token']
    return tokens['value']


def request(path, method='GET', body=None, status=200):
    url = urllib.parse.urljoin(base, path.lstrip('/'))
    assert urllib.parse.urlparse(url).netloc == urllib.parse.urlparse(base).netloc
    req = urllib.request.Request(url, method=method, data=json.dumps(body).encode() if body is not None else None,
        headers={'Content-Type': 'application/json', 'Accept': 'application/json, text/event-stream', 'Authorization': 'Bearer ' + token()})
    try:
        response = urllib.request.urlopen(req, timeout=60)
    except urllib.error.HTTPError as error:
        response = error
    with response:
        text = response.read().decode()
        allowed = status if isinstance(status, tuple) else (status,)
        assert response.status in allowed, (method, path, response.status, text[:800])
        if response.headers.get('Content-Type', '').startswith('text/event-stream'):
            return response.status, next(json.loads(line[6:]) for line in text.splitlines()
                if line.startswith('data: ') and json.loads(line[6:]).get('id') == 1)
        return response.status, json.loads(text) if text else None


def pomi(*args):
    env = os.environ.copy()
    env.update(OC_FIXTURE_CONFIG_HOME=config_home, OC_FIXTURE_CLIENT_ID='cli-fixture')
    result = subprocess.run([sys.executable, str(wrapper), str(state_path), *args], capture_output=True, text=True, env=env, timeout=120)
    assert result.returncode == 0, (args[:3], (result.stderr or result.stdout)[:800])
    return result.stdout


def describes(where, example, actual):
    """Every property of the example exists in the actual response, with the same kind unless one of them is null."""
    if example is None or actual is None:
        return
    assert isinstance(example, (dict)) == isinstance(actual, dict) and isinstance(example, list) == isinstance(actual, list), (where, example, actual)
    if isinstance(example, dict):
        for name, value in example.items():
            assert name in actual, (where, name, sorted(actual))
            describes(where + '.' + name, value, actual[name])
    elif isinstance(example, list) and example and actual:
        describes(where + '[]', example[0], actual[0])


with tempfile.TemporaryDirectory(prefix='openapi-examples-smoke-') as config_home:
    # The content examples use the Article type of the Blog recipe, which this fixture doesn't have.
    for feature in ['OrchardCore.Queries.Sql', 'OrchardCore.Title', 'OrchardCore.RemoteManagement.Mcp']:
        request('api/features/' + feature + ':enable?force=true', 'POST')
    request('api/content-definition/types', 'POST', {'name': 'Article', 'displayName': 'Article',
        'settings': {'ContentTypeSettings': {'draftable': True, 'versionable': True, 'creatable': True, 'listable': True}},
        'parts': [{'name': 'TitlePart', 'partName': 'TitlePart', 'settings': {}}]}, status=(200, 201, 409))

    _, document = request('openapi/v1.json')
    found = set()
    for path, item in document['paths'].items():
        for method, operation in item.items():
            if not isinstance(operation, dict):
                continue
            body = operation.get('requestBody', {}).get('content', {}).get('application/json', {})
            if 'example' not in body:
                continue
            found.add(operation['operationId'])
            for credential in placeholder_credentials:
                if credential in body['example']:
                    assert 'replace' in body['example'][credential].lower(), (operation['operationId'], 'a credential example is not a placeholder')
            status, response = request(path, method.upper(), body['example'], status=(200, 201))
            example = operation['responses'][str(status)]['content']['application/json']['example']
            describes(operation['operationId'], example, response)
            print('PASS: accepted', operation['operationId'], status, flush=True)
    assert expected <= found, expected - found

    # The examples stay out of the projections.
    pomi('context', 'add', 'examples', base, '--current')
    pomi('api', 'refresh', '--force')
    help_text = pomi('users', 'create', '--help')
    assert 'Replace-Me' not in help_text and 'replace-with' not in pomi('openid', 'applications', 'create', '--help')
    assert 'ExampleTenant' not in pomi('tenants', 'create', '--help')
    _, tools = request('mcp', 'POST', {'jsonrpc': '2.0', 'id': 1, 'method': 'tools/list'})
    serialized = json.dumps(tools['result']['tools'])
    assert '"example"' not in serialized
    assert 'Replace-Me' not in serialized and 'replace-with-a-generated-secret' not in serialized and 'ExampleTenant' not in serialized
    print('PASS: Pomi and MCP do not surface the examples', flush=True)

print('All OpenAPI example checks passed', flush=True)
