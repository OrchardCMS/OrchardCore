#!/usr/bin/env python3
"""Verify queued custom-settings exports preserve the initiating user's access."""
import io
import json
from pathlib import Path
import secrets
import ssl
import sys
import time
import urllib.error
import urllib.parse
import urllib.request
import zipfile

state = json.loads(Path(sys.argv[1]).read_text())
base = state['url']
assert urllib.parse.urlparse(base).hostname in ('localhost', '127.0.0.1', '::1')
context = ssl.create_default_context(cafile=state['certificatePath'])
urllib.request.install_opener(urllib.request.build_opener(urllib.request.HTTPSHandler(context=context)))
data = urllib.parse.urlencode({'grant_type': 'client_credentials', 'client_id': 'cli-fixture',
    'client_secret': state['OC_CLIENT_SECRET'], 'scope': 'orchardcore.management'}).encode()
with urllib.request.urlopen(urllib.request.Request(base + 'connect/token', data=data), timeout=30) as response:
    token = json.load(response)['access_token']


def request(path, method='GET', body=None, status=(200,), raw=False):
    payload = body if isinstance(body, bytes) else json.dumps(body).encode() if body is not None else None
    req = urllib.request.Request(base + path, method=method, data=payload,
        headers={'Authorization': 'Bearer ' + token, 'Content-Type': 'application/octet-stream' if isinstance(body, bytes) else 'application/json'})
    try:
        response = urllib.request.urlopen(req, timeout=90)
    except urllib.error.HTTPError as error:
        response = error
    with response:
        assert response.status in status, (path, response.status)
        data = response.read()
        return data if raw else json.loads(data) if data else None


def completed(operation):
    for _ in range(75):
        result = request('api/deployment/operations/' + operation['id'])
        if result['state'] in ('succeeded', 'failed', 'uncertain'):
            assert result['state'] == 'succeeded', (result['state'], result.get('errorCode'))
            return result
        time.sleep(2)
    raise AssertionError('Queued deployment timed out')


name = 'SettingsExport' + secrets.token_hex(5)
for feature in ['OrchardCore.Deployment', 'OrchardCore.CustomSettings']:
    request('api/features/' + feature + ':enable?force=true', 'POST')
request('api/content-definition/types', 'POST', {'name': name, 'displayName': name,
    'settings': {'ContentTypeSettings': {'stereotype': 'CustomSettings'}},
    'parts': [{'name': 'TitlePart', 'partName': 'TitlePart'}]}, status=(200, 201))
request('api/custom-settings/' + name, 'PUT', {'TitlePart': {'Title': name}})
recipe = {'name': name, 'steps': [{'name': 'deployment', 'Plans': [{'Name': name,
    'Steps': [{'Type': 'CustomSettingsDeploymentStep', 'Step': {'Id': 'settings', 'IncludeAll': False, 'SettingsTypeNames': [name]}}]}]}]}
if '--include-users' in sys.argv:
    recipe['steps'][0]['Plans'][0]['Steps'].append({'Type': 'AllUsersDeploymentStep', 'Step': {'Id': 'users'}})
artifact = request('api/deployment/artifacts?fileName=plan.json', 'POST', json.dumps(recipe).encode(), status=(200, 201))
completed(request('api/deployment/operations/import', 'POST', {'requestId': name + 'import', 'artifactId': artifact['id']}, status=(202,)))
plans = request('api/deployment/plans?search=' + name)
plan = next(plan for plan in plans['items'] if plan['name'] == name)
result = completed(request('api/deployment/operations/export', 'POST', {'requestId': name + 'export', 'planId': plan['id']}, status=(202,)))
archive = request('api/deployment/artifacts/' + result['artifactId'] + '/content', raw=True)
with zipfile.ZipFile(io.BytesIO(archive)) as package:
    exported = json.loads(package.read('Recipe.json'))
assert any(step.get('name') == 'custom-settings' and step.get(name, {}).get('TitlePart', {}).get('Title') == name
    for step in exported['steps']), 'Queued export omitted the authorized custom settings'
if '--include-users' in sys.argv:
    assert any(step.get('name') == 'Users' and step.get('Users') for step in exported['steps'])
print('PASS: queued export retained authorized custom settings without an HTTP execution context', flush=True)
